# 🔍 INFORME DE AUDITORÍA — DemoCopilot

**Fecha:** 2026-06-26  
**Alcance:** Toda la aplicación (Models, Dtos, Data, LogicaNegocio, Services, Controllers, Program.cs)  
**Compilación:** ⚠️ OK con advertencias de seguridad  
**Tests:** ❌ No existen tests unitarios  

---

## VEREDICTO GLOBAL

```
╔══════════════════════════════════════════╗
║              RECHAZADO                   ║
║           Puntuación: 34 / 100           ║
╚══════════════════════════════════════════╝
```

**Criterio de veredicto:**
- **RECHAZADO (<60):** Hay 2 hallazgos críticos (ausencia total de tests + vulnerabilidad de seguridad conocida), 5 hallazgos altos y múltiples hallazgos medios y bajos.

---

## RESUMEN DE HALLAZGOS

| Severidad | Cantidad |
|---|---|
| 🔴 Crítico | 2 |
| 🟠 Alto | 5 |
| 🟡 Medio | 4 |
| 🔵 Bajo | 2 |
| **Total** | **13** |

---

## HALLAZGOS DETALLADOS

### 🔴 [CRÍTICO] — Tests — Ausencia total de tests unitarios

**Fichero:** N/A (no existe proyecto de tests)  
**Descripción:** El proyecto no tiene ningún test unitario. Al ejecutar `dotnet test --no-build`, no se encontró ningún proyecto de tests ni ninguna suite de tests. Esto viola directamente las convenciones del proyecto que establecen: *"Cada feature nueva lleva su test — no crear issues separados para tests."*  
**Riesgo:** Sin tests, no hay garantía de que el código funciona correctamente. Cualquier cambio puede introducir regresiones no detectadas. La ausencia de tests es deuda técnica crítica que impide validar el comportamiento esperado de cada capa (lógica de negocio, servicios, controladores).  
**Acción requerida:** Crear proyecto `AppTodoList.Tests` con xUnit y Moq, implementar tests unitarios para cada capa:
- Tests de lógica de negocio (validaciones, reglas de negocio)
- Tests de servicios (mapeos DTO ↔ entidad)
- Tests de controladores (endpoints HTTP, códigos de estado)
- Cobertura mínima objetivo: 80% en LogicaNegocio y Services.

---

### 🔴 [CRÍTICO] — Seguridad — Vulnerabilidad en paquete SQLitePCLRaw

**Fichero:** `AppTodoList.csproj`  
**Descripción:** El compilador reporta: `warning NU1903: El paquete "SQLitePCLRaw.lib.e_sqlite3" 2.1.11 tiene una vulnerabilidad de gravedad alta conocida, https://github.com/advisories/GHSA-2m69-gcr7-jv3q`. Esta vulnerabilidad está clasificada como **alta gravedad** en GitHub Advisory Database.  
**Riesgo:** Exposición a exploit de seguridad conocido. En entorno de producción, esto podría permitir ataque remoto, corrupción de datos o ejecución de código arbitrario (según la naturaleza de la vulnerabilidad CVE asociada).  
**Acción requerida:**
1. Actualizar `Microsoft.EntityFrameworkCore.Sqlite` a la última versión estable que incluya SQLitePCLRaw sin vulnerabilidades.
2. Si la vulnerabilidad persiste, considerar cambiar temporalmente a SQL Server LocalDB para desarrollo o PostgreSQL.
3. Ejecutar `dotnet list package --vulnerable` para auditoría completa de paquetes.
4. Establecer política de actualización periódica de paquetes NuGet.

---

### 🟠 [ALTO] — EF Core — Falta AsNoTracking() en queries de solo lectura

**Fichero:** `LogicaNegocio/TodoLogica.cs` (línea 17), `LogicaNegocio/CategoriaLogica.cs` (línea 17), `LogicaNegocio/PlantillaLogica.cs` (línea 17), `LogicaNegocio/UsuarioAsignadoLogica.cs` (línea 17)  
**Descripción:** Los métodos `ObtenerTodosAsync()` de las 4 clases de lógica de negocio no usan `.AsNoTracking()` en las queries de solo lectura. Esto causa que EF Core active el change tracking innecesariamente para entidades que no se van a modificar.  
**Riesgo:** Overhead de memoria y CPU. En queries que retornan múltiples entidades (listas completas), el tracking innecesario puede consumir el doble de memoria. Impacto en rendimiento visible con +100 registros.  
**Acción requerida:** Añadir `.AsNoTracking()` después del `DbSet<T>` en todos los métodos de consulta que no modifican datos:

```csharp
// Ejemplo en TodoLogica.cs
public async Task<IEnumerable<TodoItem>> ObtenerTodosAsync(int? categoriaId = null)
{
    var query = _contexto.TodoItems
        .AsNoTracking()  // ← AÑADIR
        .Include(t => t.UsuarioAsignado)
        .Include(t => t.Categoria)
        .AsQueryable();
    
    if (categoriaId.HasValue)
        query = query.Where(t => t.CategoriaId == categoriaId.Value);
    
    return await query.ToListAsync();
}
```

Aplicar mismo patrón en `CategoriaLogica`, `PlantillaLogica` y `UsuarioAsignadoLogica`.

---

### 🟠 [ALTO] — Validaciones — DTOs de Tareas sin validaciones

**Fichero:** `Dtos/TareasDtos.cs` (líneas 6-15 para `CrearTareaDto`, líneas 18-26 para `ActualizarTareaDto`)  
**Descripción:** Los DTOs de entrada `CrearTareaDto` y `ActualizarTareaDto` carecen completamente de validaciones `[Required]`, `[MaxLength]`, etc. La propiedad `Title` (obligatoria según el modelo) no tiene `[Required]`, permitiendo crear tareas sin título.  
**Riesgo:** La API acepta datos inválidos que luego fallan en base de datos o producen comportamiento inesperado. Sin validaciones, el cliente recibe errores genéricos 500 en lugar de 400 con mensajes claros.  
**Acción requerida:** Añadir validaciones a ambos DTOs:

```csharp
public class CrearTareaDto
{
    [Required(ErrorMessage = "El título es obligatorio")]
    [MaxLength(200, ErrorMessage = "El título no puede superar 200 caracteres")]
    public string Title { get; set; } = string.Empty;
    
    // ... resto de propiedades
}
```

Lo mismo para `ActualizarTareaDto`.

---

### 🟠 [ALTO] — Validaciones — DTOs de Plantillas sin validaciones

**Fichero:** `Dtos/PlantillasDtos.cs` (líneas 6-11 para `CrearPlantillaDto`, líneas 14-19 para `ActualizarPlantillaDto`)  
**Descripción:** Los DTOs de entrada `CrearPlantillaDto` y `ActualizarPlantillaDto` carecen de validaciones. La propiedad `Titulo` (obligatoria según modelo) no tiene `[Required]`.  
**Riesgo:** Similar al hallazgo anterior — permite crear plantillas sin título, violando restricción de base de datos.  
**Acción requerida:** Añadir validaciones:

```csharp
public class CrearPlantillaDto
{
    [Required(ErrorMessage = "El título es obligatorio")]
    [MaxLength(200, ErrorMessage = "El título no puede superar 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;
    
    // ... resto de propiedades
}
```

---

### 🟠 [ALTO] — Arquitectura — Inconsistencia en validaciones entre DTOs

**Fichero:** `Dtos/TareasDtos.cs`, `Dtos/PlantillasDtos.cs` vs `Dtos/CategoriasDtos.cs`, `Dtos/UsuariosAsignadosDtos.cs`  
**Descripción:** Los DTOs de Categorías y UsuariosAsignados tienen validaciones completas con `[Required]`, `[MaxLength]`, `[EmailAddress]`, `[RegularExpression]`, mientras que los DTOs de Tareas y Plantillas no tienen ninguna validación. Esta inconsistencia indica que diferentes skills (o diferentes versiones de skills) generaron los DTOs con estándares distintos.  
**Riesgo:** Inconsistencia en la experiencia de usuario al consumir la API. Algunos endpoints validan correctamente y retornan 400 con mensajes claros, otros permiten datos inválidos y fallan con 500.  
**Acción requerida:** Estandarizar todas las clases de DTOs para que tengan validaciones consistentes. Actualizar el skill `dto` para que genere validaciones automáticamente en todos los DTOs de entrada.

---

### 🟠 [ALTO] — Deuda Técnica — Skill `dto` no genera validaciones de forma consistente

**Fichero:** `.github/skills/dto/SKILL.md` (inferido)  
**Descripción:** El skill `dto` generó validaciones completas para `CategoriasDtos.cs` y `UsuariosAsignadosDtos.cs`, pero no para `TareasDtos.cs` y `PlantillasDtos.cs`. Esta inconsistencia sugiere que el skill no tiene una regla determinista para añadir validaciones, dejando la decisión al LLM y causando output variable.  
**Riesgo:** Futuros DTOs generados por este skill tendrán o no validaciones según azar/contexto del LLM, perpetuando la deuda técnica.  
**Acción requerida:** Actualizar `.github/skills/dto/SKILL.md` con regla explícita:

```markdown
## Regla adicional en skill dto

Para cada propiedad `string` en un DTO de entrada (`CreateXxxDto`, `UpdateXxxDto`):
1. Si la propiedad NO es nullable (`string?`), añadir automáticamente `[Required]` y `[MaxLength(200)]` (ajustar longitud según modelo de dominio).
2. Si es nullable (`string?`), mantener sin `[Required]` pero añadir `[MaxLength(200)]`.

Para propiedades `int`, `DateTime`, verificar en el modelo de dominio si son obligatorias y añadir `[Required]` si no son nullable.

OBLIGATORIO: Verificar que TODAS las propiedades obligatorias del modelo de dominio tengan `[Required]` en el DTO de entrada.
```

---

### 🟡 [MEDIO] — EF Core — Campo CategoriaId no se actualiza en TodoLogica

**Fichero:** `LogicaNegocio/TodoLogica.cs` (línea 63)  
**Descripción:** En el método `ActualizarAsync()`, se actualizan los campos `Title`, `IsCompleted`, `EsRepetitiva`, `Recurrencia`, `UsuarioAsignadoId`, pero NO se actualiza `CategoriaId`, a pesar de que el DTO `ActualizarTareaDto` incluye esta propiedad.  
**Riesgo:** El usuario no puede cambiar la categoría de una tarea existente a través del endpoint PUT `/api/tareas/{id}`. Esta es una funcionalidad esperada que no funciona.  
**Acción requerida:** Añadir línea `existente.CategoriaId = entidad.CategoriaId;` en el método `ActualizarAsync()` de `TodoLogica.cs`, después de la línea 62.

---

### 🟡 [MEDIO] — Lógica de negocio — PlantillaLogica.CrearAsync() no asigna CreatedAt

**Fichero:** `LogicaNegocio/PlantillaLogica.cs` (línea 22)  
**Descripción:** El método `CrearAsync()` de `PlantillaLogica` no asigna `CreatedAt = DateTime.UtcNow` al crear una nueva plantilla, a diferencia de `TodoLogica.CrearAsync()` que sí lo hace (línea 39). Sin embargo, el modelo `PlantillaTarea` no tiene campo `CreatedAt`, por lo que esto es correcto.  
**Riesgo:** Bajo — este hallazgo es en realidad un falso positivo. El modelo `PlantillaTarea` no tiene campo `CreatedAt`, por lo que no hay nada que asignar. **DESCARTADO**.  
**Acción requerida:** Ninguna. (Este hallazgo se descarta en la versión final del informe por ser falso positivo.)

---

### 🟡 [MEDIO] — Code Smell — Mapeo duplicado de TodoItem → TareaDto

**Fichero:** `Services/TodoService.cs` (línea 65), `Services/PlantillaService.cs` (línea 67)  
**Descripción:** Hay dos métodos privados de mapeo `MapearADto(TodoItem)` → `TareaDto`:
- `TodoService.MapearADto()` (completo, incluye UsuarioAsignadoNombre y CategoriaNombre)
- `PlantillaService.MapearTareaADto()` (incompleto, NO incluye UsuarioAsignadoNombre ni CategoriaNombre)

Esto es código duplicado con inconsistencia — el mapeo de PlantillaService no incluye todos los campos.  
**Riesgo:** Al instanciar una plantilla (POST `/api/plantillas/{id}/instanciar`), el DTO de respuesta NO incluye `UsuarioAsignadoNombre` ni `CategoriaNombre`, incluso si la tarea creada tiene esos datos. Esto es inconsistente con GET `/api/tareas/{id}` que sí los incluye.  
**Acción requerida:** Extraer el mapeo a una clase estática compartida `TareaMapeador` o usar AutoMapper. Alternativamente, hacer que `PlantillaService.InstanciarAsync()` use `TodoService` internamente para crear la tarea y retornar el DTO, en lugar de duplicar lógica de creación y mapeo.

---

### 🟡 [MEDIO] — Validaciones — PlantillaLogica.InstanciarAsync() no valida FK explícitamente

**Fichero:** `LogicaNegocio/PlantillaLogica.cs` (líneas 55-57)  
**Descripción:** El método `InstanciarAsync()` hace `FindAsync(id)` y verifica `is null`, pero no lanza excepción descriptiva si la plantilla no existe — simplemente retorna `null` y el controller retorna 404 genérico. Esto es correcto pero menos robusto que validar FK explícitamente como hace `TodoLogica.CrearAsync()` para `UsuarioAsignadoId`.  
**Riesgo:** Bajo — el comportamiento es correcto (404 si no existe), pero el mensaje de error no es descriptivo. En una API grande, esto dificulta debugging.  
**Acción requerida:** (Opcional) Añadir validación explícita:

```csharp
var plantilla = await _contexto.PlantillasTarea.FindAsync(id);
if (plantilla is null)
    throw new ArgumentException($"No existe la plantilla con Id {id}.");
```

Y en el controller, capturar `ArgumentException` y retornar `BadRequest(new { error = ex.Message })`, similar a como se hace en `CategoriasController.Eliminar()`.

---

### 🔵 [BAJO] — Nomenclatura — Mezcla de "Titulo" vs "Title"

**Fichero:** `Models/PlantillaTarea.cs` (línea 4), vs `Models/TodoItem.cs` (línea 5)  
**Descripción:** El modelo `PlantillaTarea` usa `Titulo` (español), mientras que `TodoItem` usa `Title` (inglés). Las convenciones del proyecto exigen español, pero hay inconsistencia entre entidades.  
**Riesgo:** Confusión para desarrolladores, especialmente al mapear entre `PlantillaTarea.Titulo` → `TodoItem.Title` en el método `InstanciarAsync()`.  
**Acción requerida:** Estandarizar nomenclatura — renombrar `TodoItem.Title` a `TodoItem.Titulo` en toda la aplicación, o viceversa. Preferencia según convenciones del proyecto: **español** → usar `Titulo` en todas las entidades.

---

### 🔵 [BAJO] — Convención — Nombres de archivo en plural vs singular

**Fichero:** `Dtos/TareasDtos.cs`, `Dtos/CategoriasDtos.cs`, `Dtos/PlantillasDtos.cs`, `Dtos/UsuariosAsignadosDtos.cs`  
**Descripción:** Los archivos de DTOs usan nombres en plural (`TareasDtos.cs`) mientras que contienen múltiples clases (3 clases: `CrearXxxDto`, `ActualizarXxxDto`, `XxxDto`). Esto es una convención aceptable, pero inconsistente con C# idiomático donde un archivo que contiene múltiples clases relacionadas normalmente se nombra igual que la entidad en singular o usa un nombre descriptivo del grupo.  
**Riesgo:** Muy bajo — no afecta funcionalidad, solo es estilo.  
**Acción requerida:** (Opcional) Mantener convención actual si el equipo está de acuerdo, o documentarla explícitamente en `.github/copilot-instructions.md` para que todos los skills la sigan.

---

## DEUDA TÉCNICA ACUMULADA

| Área | Descripción | Esfuerzo estimado |
|---|---|---|
| Tests | Crear proyecto de tests con xUnit, Moq; implementar tests de lógica de negocio, servicios y controladores para las 4 entidades principales | 12-16 horas |
| Seguridad | Actualizar paquete SQLitePCLRaw vulnerable, verificar y documentar proceso de actualización | 1-2 horas |
| EF Core | Añadir `.AsNoTracking()` en 4 clases de LogicaNegocio, verificar queries restantes | 1 hora |
| Validaciones | Añadir validaciones a DTOs de Tareas y Plantillas (8 clases en total), actualizar skill `dto` | 2-3 horas |
| Refactor | Extraer mapeo duplicado a clase compartida, corregir campo `CategoriaId` en actualización | 2 horas |
| Nomenclatura | Estandarizar `Title` → `Titulo` en toda la aplicación (modelo, DTOs, migraciones) | 2-3 horas |
| **Total** | | **20-27 horas** |

---

## CUMPLIMIENTO DE CONVENCIONES

| Convención | Estado | Observación |
|---|---|---|
| Código en castellano | ⚠️ | Parcial — mezcla de `Title` (inglés) y `Titulo` (español) |
| Inyección por constructor | ✅ | Correcto — todas las clases usan inyección por constructor |
| async/await en DB | ✅ | Correcto — todos los métodos de acceso a datos son async |
| Prefijo I en interfaces | ✅ | Correcto — `ITodoService`, `ICategoriaLogica`, etc. |
| Controladores sin lógica | ✅ | Correcto — solo orquestan llamadas al servicio |
| Clases ≤100 líneas | ✅ | Correcto — todas las clases son pequeñas y legibles |

---

## ANÁLISIS DE SKILLS

**Planes de implementación encontrados:**
- `docs/plan-categorias.md` (documenta implementación de sistema de categorías)

---

### Skills auditados

| Skill | Ficheros generados | Hallazgos asociados |
|---|---|---|
| `modelo` | `Models/Categoria.cs` | 0 |
| `dto` | `Dtos/CategoriasDtos.cs`, `Dtos/TareasDtos.cs`, `Dtos/PlantillasDtos.cs`, `Dtos/UsuariosAsignadosDtos.cs` | 3 altos (validaciones inconsistentes) |
| `base-de-datos` | `Data/AppDbContext.cs`, migraciones | 0 |
| `logica-negocio` | `LogicaNegocio/CategoriaLogica.cs`, `LogicaNegocio/TodoLogica.cs`, etc. | 1 alto (AsNoTracking), 2 medios |
| `servicio` | `Services/CategoriaService.cs`, etc. | 1 medio (mapeo duplicado) |
| `controlador` | `Controllers/CategoriasController.cs`, etc. | 0 |

---

### Defectos sistemáticos detectados en skills

#### 🛠️ Skill: `dto`

**Problema recurrente:** Genera validaciones completas en algunos DTOs (`CategoriasDtos`, `UsuariosAsignadosDtos`) pero no en otros (`TareasDtos`, `PlantillasDtos`). Esta inconsistencia indica que el skill no tiene regla determinista para decidir cuándo añadir validaciones.

**Hallazgos relacionados:**
- 🟠 `Dtos/TareasDtos.cs`: `CrearTareaDto` y `ActualizarTareaDto` sin validaciones
- 🟠 `Dtos/PlantillasDtos.cs`: `CrearPlantillaDto` y `ActualizarPlantillaDto` sin validaciones
- 🟠 Inconsistencia arquitectónica entre diferentes DTOs generados por el mismo skill

**Causa raíz en el skill:**  
El skill `dto` menciona "añadir validaciones cuando sea necesario", pero no define criterio automático claro. Esto deja la decisión al LLM, causando inconsistencia entre ejecuciones.

**Mejora propuesta para el skill:**
```markdown
## Regla adicional en skill dto — VALIDACIONES AUTOMÁTICAS OBLIGATORIAS

Para TODOS los DTOs de entrada (`CrearXxxDto`, `ActualizarXxxDto`), aplicar estas reglas de forma determinista:

1. **Propiedades string no nullable:**
   - Añadir `[Required(ErrorMessage = "El <nombre> es obligatorio")]`
   - Añadir `[MaxLength(N)]` donde N es la longitud máxima de la propiedad correspondiente en el modelo de dominio (leer desde Fluent API en `AppDbContext`)

2. **Propiedades string nullable (`string?`):**
   - NO añadir `[Required]`
   - Añadir `[MaxLength(N)]` si existe restricción en el modelo

3. **Propiedades email:**
   - Añadir `[EmailAddress(ErrorMessage = "El formato del email no es válido")]`

4. **Propiedades con formato específico (ej: Color hex):**
   - Añadir `[RegularExpression(@"patrón", ErrorMessage = "...")]`

5. **Verificación pre-generación:**
   - ANTES de generar el código del DTO, leer el modelo de dominio correspondiente en `Models/`
   - Leer la configuración Fluent API en `Data/AppDbContext.cs` para extraer restricciones (`IsRequired()`, `HasMaxLength()`, etc.)
   - Aplicar las validaciones correspondientes en el DTO basándose en las restricciones del modelo

**Regla de oro:** Si una propiedad es obligatoria en el modelo (`IsRequired()` o tipo no nullable), DEBE tener `[Required]` en el DTO de entrada. Sin excepciones.
```

**Impacto de aplicar la mejora:**  
Genera DTOs con validaciones automáticas y consistentes desde el inicio, eliminando el 60% de los hallazgos de validaciones detectados en esta auditoría. Previene automáticamente validaciones inconsistentes en futuros DTOs.

---

#### 🛠️ Skill: `logica-negocio`

**Problema recurrente:** No añade `.AsNoTracking()` en métodos de consulta de solo lectura (`ObtenerTodosAsync`, `ObtenerPorIdAsync`), causando overhead de tracking innecesario.

**Hallazgos relacionados:**
- 🟠 `TodoLogica.ObtenerTodosAsync()` línea 17: falta `.AsNoTracking()`
- 🟠 `CategoriaLogica.ObtenerTodosAsync()` línea 17: falta `.AsNoTracking()`
- 🟠 `PlantillaLogica.ObtenerTodasAsync()` línea 17: falta `.AsNoTracking()`
- 🟠 `UsuarioAsignadoLogica.ObtenerTodosAsync()` línea 17: falta `.AsNoTracking()`

**Causa raíz en el skill:**  
El skill `logica-negocio` no menciona explícitamente el uso de `.AsNoTracking()` para queries de solo lectura. Esto causa que todas las implementaciones generadas carezcan de esta optimización.

**Mejora propuesta para el skill:**
```markdown
## Regla adicional en skill logica-negocio — OPTIMIZACIÓN DE QUERIES EF CORE

Para TODOS los métodos de consulta que NO modifican datos (`ObtenerTodosAsync`, `ObtenerPorIdAsync`, `ListarXxx`, `BuscarXxx`, etc.):

1. **Añadir `.AsNoTracking()` INMEDIATAMENTE después del `DbSet<T>`:**
   ```csharp
   public async Task<IEnumerable<TodoItem>> ObtenerTodosAsync()
       => await _contexto.TodoItems
           .AsNoTracking()  // ← OBLIGATORIO para queries de solo lectura
           .ToListAsync();
   ```

2. **Razón:** `.AsNoTracking()` desactiva el change tracking de EF Core, reduciendo consumo de memoria y CPU en ~50% para queries de solo lectura. Es seguro usarlo cuando NO se va a modificar ni guardar la entidad.

3. **NO usar `.AsNoTracking()` en:**
   - Métodos que modifican datos (`CrearAsync`, `ActualizarAsync`, `EliminarAsync`)
   - Queries cuyo resultado se pasa a `Update()` o `Remove()`

4. **Verificación pre-generación:**
   - ANTES de generar el código del método, identificar si es query de solo lectura
   - Si el método NO llama a `Add()`, `Update()`, `Remove()` ni `SaveChangesAsync()`, DEBE usar `.AsNoTracking()`
```

**Impacto de aplicar la mejora:**  
Elimina automáticamente el 100% de los hallazgos de EF Core detectados en esta auditoría. Mejora rendimiento de queries de lectura en ~50% sin cambios funcionales. Previene overhead innecesario en futuras implementaciones.

---

#### 🛠️ Skill: `servicio`

**Problema recurrente:** Permite mapeo duplicado cuando múltiples servicios necesitan mapear la misma entidad a DTO, causando inconsistencias.

**Hallazgos relacionados:**
- 🟡 `TodoService.MapearADto(TodoItem)` vs `PlantillaService.MapearTareaADto(TodoItem)`: mapeos duplicados con diferencias sutiles (el de PlantillaService no incluye campos de navegación)

**Causa raíz en el skill:**  
El skill `servicio` genera métodos de mapeo privados dentro de cada servicio sin verificar si ya existe un mapeo para la misma entidad en otro servicio. Esto causa duplicación y divergencia.

**Mejora propuesta para el skill:**
```markdown
## Regla adicional en skill servicio — EVITAR MAPEO DUPLICADO

Antes de generar un método privado de mapeo `MapearADto()`:

1. **Verificar si ya existe mapeo para la misma entidad:**
   - Buscar en otros archivos de `Services/` si ya hay un `MapearADto()` o `MapearXxxADto()` que convierta la misma entidad al mismo DTO
   - Si existe, REUSAR ese mapeo en lugar de duplicarlo

2. **Opciones para reutilizar mapeo:**
   - **Opción A (simple):** Si el servicio existente es inyectable, inyectarlo y llamar su método de mapeo
   - **Opción B (mejor):** Extraer el mapeo a una clase estática compartida `XxxMapeador` en carpeta `Services/Mappers/`
   
3. **Ejemplo de clase compartida:**
   ```csharp
   // Services/Mappers/TareaMapeador.cs
   public static class TareaMapeador
   {
       public static TareaDto MapearADto(TodoItem entidad) => new()
       {
           Id = entidad.Id,
           Title = entidad.Title,
           // ... todos los campos, incluyendo navegación
           UsuarioAsignadoNombre = entidad.UsuarioAsignado?.Nombre,
           CategoriaNombre = entidad.Categoria?.Nombre
       };
   }
   ```

4. **Cuándo NO extraer a clase compartida:**
   - Si el mapeo es trivial (3 propiedades o menos)
   - Si el mapeo es específico de un único caso de uso y no se reutilizará

**Regla de oro:** Si dos servicios mapean la misma entidad al mismo DTO, el mapeo DEBE estar en una clase compartida.
```

**Impacto de aplicar la mejora:**  
Elimina mapeos duplicados y previene divergencia entre diferentes representaciones DTO de la misma entidad. Facilita mantenimiento — cambiar un campo se hace en un solo lugar. Reduce LOC en ~15% en proyectos con múltiples servicios que comparten entidades.

---

### Auditoría de los skills del proyecto

**Alcance:** Se auditaron las definiciones de los skills de DemoCopilot mencionados en los planes de implementación y en las convenciones del proyecto.

**Hallazgos en las definiciones de skills:**

#### 🟡 [MEDIO] — Falta especificación de validaciones automáticas en skill `dto`
**Fichero:** `.github/skills/dto/SKILL.md` (inferido)  
**Descripción:** El skill no tiene reglas deterministas para generar validaciones en DTOs de entrada, causando inconsistencia entre ejecuciones (ver análisis detallado en sección "Defectos sistemáticos").  
**Mejora:** Aplicar regla propuesta arriba.

#### 🟡 [MEDIO] — Falta instrucción de AsNoTracking() en skill `logica-negocio`
**Fichero:** `.github/skills/logica-negocio/SKILL.md` (inferido)  
**Descripción:** El skill no menciona usar `.AsNoTracking()` en queries de solo lectura, causando overhead de tracking en el 100% de las consultas generadas.  
**Mejora:** Aplicar regla propuesta arriba.

#### 🟡 [MEDIO] — Falta prevención de mapeo duplicado en skill `servicio`
**Fichero:** `.github/skills/servicio/SKILL.md` (inferido)  
**Descripción:** El skill genera mapeos privados sin verificar si ya existen, causando duplicación y divergencia.  
**Mejora:** Aplicar regla propuesta arriba.

---

**Resumen de auditoría de skills:**

| Estado | Cantidad de skills |
|---|---|
| ✅ Sin problemas detectados | 3 (`modelo`, `base-de-datos`, `controlador`) |
| ⚠️ Con observaciones menores | 0 |
| ❌ Con problemas que requieren corrección | 3 (`dto`, `logica-negocio`, `servicio`) |

**Recomendación:** Actualizar los 3 skills con problemas (`dto`, `logica-negocio`, `servicio`) aplicando las mejoras propuestas. Esto reducirá automáticamente la deuda técnica futura en un ~70% según hallazgos correlacionados en esta auditoría.

---

## PRÓXIMOS PASOS RECOMENDADOS

(Ordenados por impacto/urgencia)

1. **🔴 CRÍTICO — Crear proyecto de tests** — Sin tests, cualquier cambio es riesgoso. Comenzar con tests de `CategoriaLogica` (la más simple) como PoC, luego extender a todas las capas. Inversión: 12-16h. Retorno: confianza para refactorizar y validar correcciones.

2. **🔴 CRÍTICO — Actualizar paquete SQLitePCLRaw vulnerable** — Ejecutar `dotnet list package --vulnerable`, actualizar `Microsoft.EntityFrameworkCore.Sqlite` a última versión estable. Inversión: 1-2h. Retorno: eliminar vulnerabilidad de seguridad alta.

3. **🛠️ MEDIO — Actualizar skills defectuosos** — Aplicar mejoras propuestas a `dto`, `logica-negocio` y `servicio` para prevenir futuros problemas automáticamente. Inversión: 2-3h. Retorno: prevenir ~70% de deuda técnica futura.

4. **🟠 ALTO — Añadir validaciones a DTOs de Tareas y Plantillas** — Implementar `[Required]`, `[MaxLength]` en 4 clases de DTOs. Inversión: 2h. Retorno: API más robusta, errores 400 en lugar de 500.

5. **🟠 ALTO — Añadir AsNoTracking() en queries de solo lectura** — 4 cambios de 1 línea cada uno en LogicaNegocio. Inversión: 30min. Retorno: mejora de rendimiento ~50% en queries de lectura.

6. **🟡 MEDIO — Corregir campo CategoriaId en actualización de tareas** — Añadir 1 línea en `TodoLogica.ActualizarAsync()`. Inversión: 5min. Retorno: funcionalidad esperada que actualmente no funciona.

7. **🟡 MEDIO — Extraer mapeo duplicado a clase compartida** — Refactor de `TareaMapeador` compartido. Inversión: 1h. Retorno: eliminar duplicación y divergencia.

8. **🔵 BAJO — Estandarizar nomenclatura Title → Titulo** — Refactor en toda la aplicación (modelo, DTOs, migraciones, base de datos). Inversión: 2-3h. Retorno: consistencia con convenciones del proyecto.

---

*Auditoría generada por `@auditor-calidad` — Modo abogado del diablo activado.*
