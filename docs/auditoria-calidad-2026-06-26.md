# 🔍 Auditoría de Calidad de Código - DemoCopilot
**Fecha**: 2026-06-26  
**Auditor**: GitHub Copilot (modo abogado del diablo)  
**Alcance**: Auditoría completa de la aplicación

---

## 📊 VEREDICTO: ⚠️ RECHAZADO (NO APTO PARA PRODUCCIÓN)

**Puntuación de Calidad**: 42/100

### Resumen Ejecutivo
La aplicación presenta una arquitectura de capas bien definida y código limpio en términos de formato, pero tiene **graves fallos de seguridad que impiden su despliegue en producción**. Destacan la ausencia total de autenticación/autorización, validaciones insuficientes, y violaciones de integridad referencial que pueden causar inconsistencia de datos.

---

## 🚨 HALLAZGOS CRÍTICOS (8) - Bloquean producción

### 1. ⛔ **Falta de Autenticación y Autorización** [OWASP A01:2021]
**Severidad**: CRÍTICA  
**Archivo**: `Program.cs`, todos los controladores  
**Descripción**: La API está completamente abierta sin ningún mecanismo de autenticación. Cualquiera puede crear, modificar y eliminar datos.

```csharp
// Program.cs - NO HAY configuración de autenticación
builder.Services.AddControllers();
// ❌ Falta: AddAuthentication(), AddAuthorization()

// Controladores - NO HAY atributos [Authorize]
[ApiController]
[Route("api/[controller]")]
public class TareasController : ControllerBase // ❌ Sin protección
```

**Impacto**: Acceso no autorizado a todos los recursos (OWASP A01:2021)  
**Recomendación**: Implementar JWT o Identity con políticas de autorización

---

### 2. ⛔ **Violación de Integridad Referencial - UsuarioAsignado**
**Severidad**: CRÍTICA  
**Archivo**: `LogicaNegocio/UsuarioAsignadoLogica.cs` línea 43  
**Descripción**: Permite eliminar usuarios que tienen tareas asignadas, dejando FKs huérfanas.

```csharp
public async Task<bool> EliminarAsync(int id)
{
    var existente = await _contexto.UsuariosAsignados.FindAsync(id);
    if (existente is null) return false;
    
    // ❌ NO valida si tiene tareas asociadas
    _contexto.UsuariosAsignados.Remove(existente);
    await _contexto.SaveChangesAsync();
    return true;
}
```

**Impacto**: Inconsistencia de datos, posibles errores en queries con Include  
**Recomendación**: Validar tareas asociadas antes de eliminar (igual que en CategoriaLogica)

---

### 3. ⛔ **Violación de Integridad Referencial - Plantilla**
**Severidad**: CRÍTICA  
**Archivo**: `LogicaNegocio/PlantillaLogica.cs` línea 36  
**Descripción**: Permite eliminar plantillas que tienen tareas instanciadas.

```csharp
public async Task<bool> EliminarAsync(int id)
{
    var existente = await _contexto.PlantillasTarea.FindAsync(id);
    if (existente is null) return false;
    
    // ❌ NO valida si tiene tareas asociadas
    _contexto.PlantillasTarea.Remove(existente);
    await _contexto.SaveChangesAsync();
    return true;
}
```

**Impacto**: Pérdida de trazabilidad de tareas generadas desde plantillas  
**Recomendación**: Validar con `AnyAsync(t => t.PlantillaId == id)` antes de eliminar

---

### 4. ⛔ **Falta de Validación en DTOs de Tareas**
**Severidad**: CRÍTICA  
**Archivo**: `Dtos/TareasDtos.cs`  
**Descripción**: `CrearTareaDto` y `ActualizarTareaDto` no tienen Data Annotations. Se pueden enviar datos vacíos o inválidos.

```csharp
public class CrearTareaDto
{
    public string Title { get; set; } = string.Empty; // ❌ Sin [Required], sin [MaxLength]
    public TipoRecurrencia? Recurrencia { get; set; }  // ❌ Sin validación de EsRepetitiva
}
```

**Impacto**: Se pueden crear tareas con títulos vacíos, longitud excesiva, o recurrencia sin flag  
**Recomendación**: Añadir `[Required]`, `[MaxLength(200)]`, validación condicional para Recurrencia

---

### 5. ⛔ **Falta de Validación en DTOs de Plantillas**
**Severidad**: CRÍTICA  
**Archivo**: `Dtos/PlantillasDtos.cs`  
**Descripción**: Los DTOs de plantillas no tienen ninguna validación.

```csharp
public class CrearPlantillaDto
{
    public string Titulo { get; set; } = string.Empty; // ❌ Sin validaciones
    public TipoRecurrencia? Recurrencia { get; set; }
}
```

**Impacto**: Igual que tareas - datos inválidos en BD  
**Recomendación**: Añadir Data Annotations

---

### 6. ⛔ **Validación Incompleta de FKs en TodoLogica**
**Severidad**: CRÍTICA  
**Archivo**: `LogicaNegocio/TodoLogica.cs` línea 31  
**Descripción**: Solo valida `UsuarioAsignadoId`, pero ignora `CategoriaId` y `PlantillaId`.

```csharp
public async Task<TodoItem> CrearAsync(TodoItem entidad)
{
    // ✅ Valida UsuarioAsignadoId
    if (entidad.UsuarioAsignadoId.HasValue && 
        !await _contexto.UsuariosAsignados.AnyAsync(u => u.Id == entidad.UsuarioAsignadoId.Value))
        throw new ArgumentException(...);
    
    // ❌ NO valida CategoriaId
    // ❌ NO valida PlantillaId
    
    _contexto.TodoItems.Add(entidad);
    await _contexto.SaveChangesAsync();
    return entidad;
}
```

**Impacto**: Permite FKs inválidas, inconsistencia de datos  
**Recomendación**: Validar todas las FKs opcionales antes de insertar

---

### 7. ⛔ **CORS Demasiado Permisivo**
**Severidad**: ALTA (elevada a CRÍTICA por contexto de seguridad)  
**Archivo**: `Program.cs` línea 22  
**Descripción**: CORS permite todos los headers y métodos sin restricción.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()   // ❌ Demasiado amplio
              .AllowAnyMethod();  // ❌ Demasiado amplio
    });
});
```

**Impacto**: Potencial explotación en ataques CSRF si se añade autenticación  
**Recomendación**: Especificar headers y métodos permitidos explícitamente

---

### 8. ⛔ **EnsureCreated en Código de Aplicación**
**Severidad**: ALTA (elevada a CRÍTICA por riesgo en prod)  
**Archivo**: `Program.cs` línea 38  
**Descripción**: Usa `EnsureCreated()` que no es compatible con migraciones y puede destruir datos.

```csharp
using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    contexto.Database.EnsureCreated(); // ❌ NO usar en producción
    DatosEjemplo.Inicializar(contexto);
}
```

**Impacto**: Si se despliega en prod, puede sobrescribir esquema o ignorar migraciones  
**Recomendación**: Usar solo en dev, aplicar migraciones en prod con `dotnet ef database update`

---

## ⚠️ HALLAZGOS ALTOS (11) - Afectan calidad/mantenibilidad

### 9. **Violación SRP - TodoLogica.CompletarAsync**
**Archivo**: `LogicaNegocio/TodoLogica.cs` línea 73  
**Descripción**: Un método hace dos cosas: completar tarea + crear nueva tarea repetitiva

```csharp
public async Task<TodoItem?> CompletarAsync(int id)
{
    var tarea = await _contexto.TodoItems.FindAsync(id);
    tarea.IsCompleted = true;
    
    // ❌ Segunda responsabilidad mezclada aquí
    if (tarea.EsRepetitiva && tarea.Recurrencia.HasValue)
    {
        var nuevaTarea = new TodoItem { /* ... */ };
        _contexto.TodoItems.Add(nuevaTarea);
        resultado = nuevaTarea;
    }
    await _contexto.SaveChangesAsync();
    return resultado;
}
```

**Recomendación**: Separar en `CompletarAsync` y `CrearTareaRepetitivaAsync`

---

### 10. **Duplicación de Código - MapearTareaADto**
**Archivos**: `Services/TodoService.cs` línea 65, `Services/PlantillaService.cs` línea 61  
**Descripción**: Mapeo de `TodoItem → TareaDto` duplicado en dos servicios

**Recomendación**: Crear clase estática `TareaMappings.MapearADto(TodoItem)` compartida

---

### 11. **Problema Potencial N+1**
**Archivo**: `LogicaNegocio/TodoLogica.cs` línea 45  
**Descripción**: `ActualizarAsync` devuelve entidad sin navegación cargada, pero el DTO espera `UsuarioAsignadoNombre`

```csharp
public async Task<TodoItem?> ActualizarAsync(int id, TodoItem entidad)
{
    var existente = await _contexto.TodoItems.FindAsync(id); // ❌ Sin Include
    // ... actualiza campos
    await _contexto.SaveChangesAsync();
    return existente; // ❌ UsuarioAsignado y Categoria son null
}
```

**Impacto**: El DTO devuelto tiene `UsuarioAsignadoNombre` y `CategoriaNombre` null  
**Recomendación**: Recargar con `Include` antes de devolver, o hacer eager loading inicial

---

### 12. **Falta de Rate Limiting**
**Archivo**: `Program.cs`  
**Descripción**: Sin protección contra abuso de endpoints

**Recomendación**: Añadir `AddRateLimiter()` con políticas por IP y por endpoint

---

### 13. **Falta de Logging de Seguridad**
**Archivo**: Todos los controladores y servicios  
**Descripción**: No hay auditoría de quién hace qué (porque ni siquiera hay autenticación)

**Recomendación**: Implementar logging estructurado con Serilog + Application Insights

---

### 14. **Magic Strings**
**Archivo**: `Program.cs` línea 22, 52  
**Descripción**: "AllowFrontend" como string literal puede causar errores de typo

```csharp
options.AddPolicy("AllowFrontend", policy => { /* ... */ });
app.UseCors("AllowFrontend"); // ❌ Si hay typo, falla en runtime
```

**Recomendación**: Constante `private const string CorsPolicyName = "AllowFrontend";`

---

### 15. **Inconsistencia en Mapeo de PlantillaService**
**Archivo**: `Services/PlantillaService.cs` línea 61  
**Descripción**: `MapearTareaADto` no incluye `UsuarioAsignadoNombre` ni `CategoriaNombre`

```csharp
private static TareaDto MapearTareaADto(TodoItem entidad) => new()
{
    // ... campos base
    PlantillaId = entidad.PlantillaId
    // ❌ Falta UsuarioAsignadoNombre
    // ❌ Falta CategoriaNombre
};
```

**Recomendación**: Usar mapeo unificado o añadir campos faltantes

---

### 16. **Falta de Manejo de Concurrencia**
**Archivo**: Todas las clases de lógica  
**Descripción**: Sin tokens de concurrencia, dos usuarios pueden sobrescribirse

**Recomendación**: Añadir `[Timestamp]` o `RowVersion` en entidades críticas

---

### 17. **Falta de Paginación**
**Archivo**: Todos los endpoints GET de listados  
**Descripción**: `/api/tareas` devuelve TODAS las tareas sin límite

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<TareaDto>>> ObtenerTodos([FromQuery] int? categoriaId)
{
    var tareas = await _servicio.ObtenerTodosAsync(categoriaId); // ❌ Sin paginación
    return Ok(tareas);
}
```

**Impacto**: Con 10,000 tareas, el JSON pesa MB y tarda segundos  
**Recomendación**: Añadir parámetros `page` y `pageSize`, devolver `PagedResult<T>`

---

### 18. **Configuración Hardcodeada**
**Archivo**: `Program.cs` línea 24  
**Descripción**: CORS origin hardcodeado `http://localhost:5173`

**Recomendación**: Mover a `appsettings.json` → `CorsSettings:AllowedOrigins[]`

---

### 19. **Falta de Manejo de Excepciones Global**
**Archivo**: `Program.cs`  
**Descripción**: Sin middleware para capturar excepciones no controladas

**Recomendación**: Implementar middleware de excepciones que devuelva Problem Details (RFC 7807)

---

## ℹ️ HALLAZGOS MEDIOS (8) - Mejoras recomendadas

### 20. **Falta de Índices en FKs**
**Archivo**: `Data/AppDbContext.cs`  
**Descripción**: `CategoriaId`, `UsuarioAsignadoId`, `PlantillaId` no tienen índices

```csharp
// ❌ Falta:
entity.HasIndex(e => e.CategoriaId);
entity.HasIndex(e => e.UsuarioAsignadoId);
entity.HasIndex(e => e.PlantillaId);
```

**Impacto**: Queries filtradas por FK son table scans (lentas con muchos datos)

---

### 21. **Falta de Comentarios XML**
**Archivos**: Todos  
**Descripción**: Sin `/// <summary>` para documentación de Swagger

**Recomendación**: Añadir XML comments + `options.IncludeXmlComments()` en Swagger

---

### 22. **DateTime.UtcNow Hardcodeado**
**Archivos**: `TodoLogica.cs` línea 37, 87, 98, `PlantillaLogica.cs` línea 53  
**Descripción**: Dificulta testing de lógica temporal

**Recomendación**: Inyectar `ISystemClock` (o abstracción propia) para poder mockear

---

### 23. **Falta de Health Checks**
**Archivo**: `Program.cs`  
**Descripción**: Sin endpoint `/health` para monitoreo

**Recomendación**: Añadir `builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>()`

---

### 24. **Falta de Validación de Enums**
**Archivo**: DTOs de Tareas y Plantillas  
**Descripción**: `TipoRecurrencia?` puede recibir valores fuera del enum

**Recomendación**: Validar con `[EnumDataType(typeof(TipoRecurrencia))]`

---

### 25. **Servicios Demasiado Simples**
**Archivos**: Toda la carpeta `Services/`  
**Descripción**: La capa de servicios solo mapea DTO ↔ Entidad, no añade lógica

**Recomendación**: Considerar fusionar servicios con lógica si no hay validaciones adicionales

---

### 26. **Falta de Validación Condicional**
**Archivo**: `Dtos/TareasDtos.cs`  
**Descripción**: Si `EsRepetitiva = true`, `Recurrencia` debe ser requerida

**Recomendación**: Implementar `IValidatableObject` o FluentValidation

---

### 27. **Constantes Mágicas en DbContext**
**Archivo**: `Data/AppDbContext.cs`  
**Descripción**: Números 100, 200, 7 sin constantes nombradas

```csharp
entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100); // ❌ Magic number
```

**Recomendación**: `private const int MaxNombreLength = 100;`

---

## 📝 HALLAZGOS BAJOS (3) - Limpieza de código

### 28. **Inconsistencia de Nomenclatura**
**Descripción**: Algunos métodos se llaman `ObtenerTodos`, otros `ObtenerTodas`

**Recomendación**: Unificar a `ObtenerTodos` (masculino genérico) o usar nombres más descriptivos

---

### 29. **DTOs de Entrada vs Actualización Redundantes**
**Archivos**: Todos los `*Dtos.cs`  
**Descripción**: `CrearXxxDto` y `ActualizarXxxDto` tienen los mismos campos (excepto Id)

**Recomendación**: Evaluar si se pueden unificar en un solo DTO con validaciones condicionales

---

### 30. **Falta de .editorconfig**
**Archivo**: Raíz del proyecto  
**Descripción**: Sin configuración de estilo de código compartida

**Recomendación**: Añadir `.editorconfig` con reglas de formato del equipo

---

## 📈 RESUMEN POR CATEGORÍA

| Categoría | Críticos | Altos | Medios | Bajos | **Total** |
|-----------|----------|-------|--------|-------|-----------|
| **Seguridad OWASP** | 3 | 2 | 0 | 0 | **5** |
| **Integridad de Datos** | 4 | 1 | 0 | 0 | **5** |
| **Validaciones** | 1 | 0 | 2 | 0 | **3** |
| **Arquitectura/SOLID** | 0 | 2 | 1 | 1 | **4** |
| **Performance** | 0 | 2 | 1 | 0 | **3** |
| **Mantenibilidad** | 0 | 3 | 4 | 2 | **9** |
| **Testing/Observabilidad** | 0 | 1 | 1 | 0 | **2** |
| **TOTAL** | **8** | **11** | **8** | **3** | **30** |

---

## 🎯 PLAN DE ACCIÓN RECOMENDADO

### Sprint 1 - CRÍTICO (1-2 semanas)
1. ✅ Implementar autenticación JWT + Identity
2. ✅ Añadir Data Annotations a todos los DTOs
3. ✅ Validar integridad referencial en todos los `EliminarAsync`
4. ✅ Validar todas las FKs en `CrearAsync` y `ActualizarAsync`
5. ✅ Quitar `EnsureCreated` y usar solo migraciones
6. ✅ Ajustar política CORS a headers/métodos específicos

### Sprint 2 - ALTO (1 semana)
7. ✅ Implementar rate limiting
8. ✅ Añadir logging estructurado (Serilog)
9. ✅ Refactorizar `CompletarAsync` (separar responsabilidades)
10. ✅ Unificar mapeos duplicados
11. ✅ Corregir N+1 en `ActualizarAsync`
12. ✅ Implementar paginación en listados

### Sprint 3 - MEDIO (3-5 días)
13. ✅ Añadir índices a FKs
14. ✅ Implementar health checks
15. ✅ Abstraer `DateTime.UtcNow` para testing
16. ✅ Añadir middleware de excepciones global
17. ✅ Mover configuración a `appsettings.json`

### Sprint 4 - LIMPIEZA (1-2 días)
18. ✅ Añadir comentarios XML
19. ✅ Crear constantes para valores mágicos
20. ✅ Unificar nomenclatura
21. ✅ Crear `.editorconfig`

---

## 🏁 CONCLUSIÓN

La aplicación **NO ESTÁ LISTA PARA PRODUCCIÓN** debido a fallos críticos de seguridad e integridad de datos. Sin embargo, la arquitectura de capas es sólida y el código es limpio y legible.

**Tiempo estimado para resolver críticos**: 1-2 semanas  
**Tiempo total para alcanzar calidad producción**: 3-4 semanas

Una vez resueltos los hallazgos críticos y altos, la aplicación será apta para producción con calidad empresarial.

---

**Generado por**: Auditoría automatizada de calidad  
**Issues de GitHub**: Se crearán automáticamente para cada hallazgo crítico y alto
