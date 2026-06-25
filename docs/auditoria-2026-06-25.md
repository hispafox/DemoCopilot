# 🔍 INFORME DE AUDITORÍA — DemoCopilot

**Fecha:** 2026-06-25  
**Alcance:** Aplicación completa (Controllers, Services, LogicaNegocio, Models, Dtos, Data, Program.cs, Migraciones)  
**Compilación:** ✅ OK (con 2 advertencias de seguridad)  
**Tests:** ❌ **NO HAY PROYECTO DE TESTS**

---

## VEREDICTO GLOBAL

```
╔══════════════════════════════════════════╗
║           RECHAZADO                     ║
║  Puntuación: 35 / 100                   ║
╚══════════════════════════════════════════╝
```

**Criterio:** Hay 3 hallazgos críticos (vulnerabilidad de seguridad, ausencia de tests, violación de integridad de datos) que hacen el código inaceptable para producción.

---

## RESUMEN DE HALLAZGOS

| Severidad | Cantidad |
|---|---|
| 🔴 Crítico | 3 |
| 🟠 Alto | 3 |
| 🟡 Medio | 3 |
| 🔵 Bajo | 2 |
| **Total** | **11** |

---

## HALLAZGOS DETALLADOS

### 🔴 [CRÍTICO] — Seguridad — Vulnerabilidad de dependencia conocida
**Fichero:** `AppTodoList.csproj`  
**Descripción:** El paquete `SQLitePCLRaw.lib.e_sqlite3` versión 2.1.11 tiene una vulnerabilidad de seguridad de gravedad **alta** identificada como GHSA-2m69-gcr7-jv3q.  
**Riesgo:** Explotación de vulnerabilidad conocida que puede comprometer la seguridad de la aplicación.  
**Acción requerida:** Actualizar inmediatamente el paquete `Microsoft.EntityFrameworkCore.Sqlite` a la última versión que incluya una versión parcheada de SQLitePCLRaw.

---

### 🔴 [CRÍTICO] — Arquitectura — Ausencia total de tests
**Fichero:** (proyecto completo)  
**Descripción:** El proyecto no contiene ningún proyecto de tests (xUnit, NUnit, MSTest). La ejecución de `dotnet test` no encuentra tests.  
**Riesgo:** Sin cobertura de tests, no hay garantía de que el código funcione correctamente. Cambios futuros pueden introducir regresiones sin detectar. Esto contradice explícitamente las instrucciones del proyecto: *"Cada feature nueva lleva su test — no crear issues separados para tests"*.  
**Acción requerida:** Crear proyecto `AppTodoList.Tests`, implementar tests unitarios para toda la lógica de negocio y tests de integración para los controladores.

---

### 🔴 [CRÍTICO] — Corrección — Violación de integridad referencial
**Fichero:** `LogicaNegocio/TodoLogica.cs` (líneas 37-47, 59-72)  
**Descripción:** Los métodos `CrearAsync` y `ActualizarAsync` validan la existencia de `UsuarioAsignadoId` pero **NO validan `CategoriaId`**. Un cliente puede enviar un `CategoriaId` inexistente y la tarea se creará/actualizará, pero al intentar cargar la relación con `.Include(t => t.Categoria)` se obtendrá `null`, generando inconsistencias.  
**Riesgo:** Datos huérfanos en la base de datos. Violación de integridad referencial lógica (la FK en DB lo permite porque es nullable con SetNull, pero la lógica de negocio debería validar antes de asignar).  
**Acción requerida:** Añadir validación en `TodoLogica.CrearAsync` y `ActualizarAsync`:
```csharp
if (entidad.CategoriaId.HasValue &&
    !await _contexto.Categorias.AnyAsync(c => c.Id == entidad.CategoriaId.Value))
    throw new ArgumentException($"No existe la categoría con Id {entidad.CategoriaId}.");
```

---

### 🟠 [ALTO] — Performance — Ausencia de AsNoTracking en queries de solo lectura
**Ficheros:** 
- `LogicaNegocio/CategoriaLogica.cs` (línea 17)
- `LogicaNegocio/UsuarioAsignadoLogica.cs` (línea 16)
- `LogicaNegocio/PlantillaLogica.cs` (línea 16)
- `LogicaNegocio/TodoLogica.cs` (líneas 16-26)

**Descripción:** Los métodos `ObtenerTodosAsync()` y queries con `.Include()` no usan `.AsNoTracking()`. EF Core rastrea todas las entidades cargadas por defecto, consumiendo memoria y tiempo de CPU innecesariamente cuando solo se va a leer datos.  
**Riesgo:** Degradación de performance en listas largas. Consumo de memoria innecesario en el change tracker de EF Core.  
**Acción requerida:** Añadir `.AsNoTracking()` después del `DbSet` en todos los métodos de lectura:
```csharp
public async Task<IEnumerable<Categoria>> ObtenerTodosAsync()
    => await _contexto.Categorias.AsNoTracking().ToListAsync();

public async Task<IEnumerable<TodoItem>> ObtenerTodosAsync(int? categoriaId = null)
{
    var query = _contexto.TodoItems
        .AsNoTracking()  // <-- AÑADIR AQUÍ
        .Include(t => t.UsuarioAsignado)
        .Include(t => t.Categoria)
        .AsQueryable();
    // ...
}
```

---

### 🟠 [ALTO] — Validaciones — DTOs sin validaciones de entrada
**Ficheros:** 
- `Dtos/TareasDtos.cs` (CrearTareaDto, ActualizarTareaDto)
- `Dtos/PlantillasDtos.cs` (CrearPlantillaDto, ActualizarPlantillaDto)

**Descripción:** Los DTOs de entrada para tareas y plantillas **no tienen validaciones** (`[Required]`, `[MaxLength]`, etc.). Un cliente puede enviar `Title = null` o strings de longitud ilimitada.  
**Riesgo:** 
- Violación de constraints de base de datos (el campo `Title` es `IsRequired` en Fluent API → crash en `SaveChangesAsync`).
- Strings excesivamente largos en memoria.
- Falta de feedback inmediato al cliente (error 500 en lugar de 400 con validación).

**Acción requerida:** Añadir Data Annotations a los DTOs:
```csharp
public class CrearTareaDto
{
    [Required(ErrorMessage = "El título es obligatorio")]
    [MaxLength(200, ErrorMessage = "El título no puede superar 200 caracteres")]
    public string Title { get; set; } = string.Empty;
    // ...
}
```

---

### 🟠 [ALTO] — Corrección — Actualización incompleta de CategoriaId
**Fichero:** `LogicaNegocio/TodoLogica.cs` (líneas 59-72)  
**Descripción:** El método `ActualizarAsync` actualiza `UsuarioAsignadoId` (línea 70) pero **NO actualiza `CategoriaId`**, a pesar de que el DTO lo incluye y el plan lo especifica.  
**Riesgo:** Imposible cambiar la categoría de una tarea mediante PUT. Inconsistencia entre la API y el contrato del DTO.  
**Acción requerida:** Añadir la línea `existente.CategoriaId = entidad.CategoriaId;` después de la línea 70.

---

### 🟡 [MEDIO] — Complejidad — Clase larga
**Fichero:** `LogicaNegocio/TodoLogica.cs`  
**Descripción:** La clase tiene 119 líneas de código, superando el límite de 100 líneas establecido en las convenciones del proyecto para una demo pedagógica.  
**Riesgo:** Dificulta la legibilidad en presentaciones. Va en contra del objetivo didáctico del proyecto (*"Los snippets de código deben caber en una pantalla de presentación (~30 líneas)"*).  
**Acción requerida:** Extraer el método `CompletarAsync` y su lógica de recurrencia a una clase separada `RecurrenciaLogica` o simplificar la implementación.

---

### 🟡 [MEDIO] — Configuración — Uso de EnsureCreated en lugar de migrations
**Fichero:** `Program.cs` (línea 29)  
**Descripción:** Se usa `contexto.Database.EnsureCreated()` para inicializar la base de datos en lugar de aplicar migraciones con `contexto.Database.Migrate()`.  
**Riesgo:** 
- `EnsureCreated()` **ignora las migraciones** completamente — las crea pero nunca las ejecuta.
- Desincronización entre esquema de desarrollo y producción.
- Imposible aplicar migraciones incrementales en producción.

**Acción requerida:** Reemplazar con:
```csharp
contexto.Database.Migrate();
```

---

### 🟡 [MEDIO] — Deuda técnica — Datos de ejemplo incompletos
**Fichero:** `Data/DatosEjemplo.cs`  
**Descripción:** La clase `DatosEjemplo` crea usuarios, plantillas y tareas de ejemplo, pero **no crea categorías de ejemplo** a pesar de que la feature de categorías ya está implementada.  
**Riesgo:** Al ejecutar la aplicación por primera vez, no hay categorías precargadas para demostrar la funcionalidad. Esto contradice el propósito didáctico del proyecto.  
**Acción requerida:** Añadir método `SeedCategorias()` que cree 2-3 categorías (ej: "Trabajo", "Personal", "Urgente") y asignar algunas tareas a esas categorías en `SeedTareas()`.

---

### 🔵 [BAJO] — Convenciones — Inconsistencia en mensajes de error personalizados
**Ficheros:** `Dtos/CategoriasDtos.cs` vs `Dtos/UsuariosAsignadosDtos.cs`  
**Descripción:** Los DTOs de `Categoria` usan mensajes de error personalizados en español (`ErrorMessage = "El nombre es obligatorio"`), mientras que los DTOs de `UsuarioAsignado` usan solo `[Required]` sin mensaje personalizado (mostrarán mensajes en inglés por defecto).  
**Riesgo:** Inconsistencia de UX. Algunos errores se muestran en español, otros en inglés.  
**Acción requerida:** Estandarizar — añadir `ErrorMessage` en español a todos los DTOs o quitarlo de todos (preferiblemente añadirlo para consistencia con el idioma del proyecto).

---

### 🔵 [BAJO] — Herramientas — EF Tools desactualizadas
**Fichero:** (dotnet-ef global tool)  
**Descripción:** La versión de EF Tools instalada es 10.0.2, mientras que el runtime de EF Core es 10.0.9.  
**Riesgo:** Puede causar incompatibilidades al generar migraciones. No es crítico pero es mala práctica.  
**Acción requerida:** Ejecutar `dotnet tool update --global dotnet-ef` para actualizar a la última versión.

---

## DEUDA TÉCNICA ACUMULADA

| Área | Descripción | Esfuerzo estimado |
|---|---|---|
| Tests | Crear proyecto de tests con cobertura para lógica de negocio (6 clases × 3-5 tests cada una ≈ 25 tests) | 8-12 horas |
| Validaciones | Añadir Data Annotations a DTOs faltantes + validación de CategoriaId en TodoLogica | 2-3 horas |
| Performance | Añadir AsNoTracking a todos los métodos de consulta (4 clases) | 30 minutos |
| Refactor | Extraer lógica de recurrencia de TodoLogica, reducir clase a <100 líneas | 1-2 horas |
| Seguridad | Actualizar dependencias vulnerables | 15 minutos |
| Configuración | Cambiar EnsureCreated por Migrate + verificar | 30 minutos |
| **TOTAL** | | **13-18 horas** |

---

## CUMPLIMIENTO DE CONVENCIONES

| Convención | Estado | Observación |
|---|---|---|
| Código en castellano | ✅ | Todas las clases, métodos y variables están en español |
| Inyección por constructor | ✅ | No se detectó uso de `new Service()` directo |
| async/await en DB | ✅ | Todos los métodos de acceso a datos son async |
| Prefijo I en interfaces | ✅ | `ITodoService`, `ICategoriaLogica`, etc. |
| Controladores sin lógica | ✅ | Los controladores solo orquestan llamadas al servicio (única excepción: try-catch en CategoriasController.Eliminar) |
| Clases ≤100 líneas | ⚠️ | `TodoLogica` tiene 119 líneas (supera el límite) |

---

## PRÓXIMOS PASOS RECOMENDADOS

(Ordenados por impacto/urgencia)

1. **[CRÍTICO]** Actualizar `Microsoft.EntityFrameworkCore.Sqlite` para resolver la vulnerabilidad de seguridad — **15 minutos** — Impacto: elimina riesgo de explotación.

2. **[CRÍTICO]** Añadir validación de `CategoriaId` en `TodoLogica.CrearAsync` y `ActualizarAsync` — **30 minutos** — Impacto: evita violación de integridad de datos.

3. **[CRÍTICO]** Crear proyecto de tests básico con al menos 10 tests críticos (CRUD de categorías + validación de eliminación con tareas asociadas) — **4 horas** — Impacto: asegura corrección del código.

4. **[ALTO]** Añadir validaciones `[Required]` y `[MaxLength]` a DTOs de tareas y plantillas — **1 hora** — Impacto: mejor feedback al cliente, evita crashes.

5. **[ALTO]** Añadir `.AsNoTracking()` a queries de solo lectura — **30 minutos** — Impacto: mejora performance.

6. **[ALTO]** Corregir `TodoLogica.ActualizarAsync` para actualizar `CategoriaId` — **10 minutos** — Impacto: completar funcionalidad de categorías.

7. **[MEDIO]** Cambiar `EnsureCreated()` por `Migrate()` en `Program.cs` — **30 minutos** — Impacto: arquitectura correcta de migraciones.

8. **[MEDIO]** Refactorizar `TodoLogica` para reducir a <100 líneas — **2 horas** — Impacto: mejora legibilidad didáctica.

---

*Auditoría generada por `@auditor-calidad` — Modo abogado del diablo activado.*
