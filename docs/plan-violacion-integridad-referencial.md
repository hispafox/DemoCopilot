# Plan: Validación de Integridad Referencial en Eliminación de Usuarios Asignados

> Generado por el agente planificador · 2026-06-26

## 1. Resumen

Añadir validación de integridad referencial en `UsuarioAsignadoLogica.EliminarAsync` para impedir la eliminación de usuarios que tienen tareas asignadas, evitando así dejar FKs huérfanas en la base de datos. El controlador capturará la excepción y devolverá BadRequest con el mensaje de error.

## 2. Requisitos funcionales

1. Al intentar eliminar un usuario asignado que tiene tareas asociadas, el sistema debe lanzar una excepción `InvalidOperationException` con un mensaje descriptivo.
2. El controlador debe capturar la excepción y devolver un código HTTP 400 (BadRequest) con el mensaje de error en el cuerpo de la respuesta.
3. Si el usuario no tiene tareas asociadas, la eliminación debe proceder normalmente.

## 3. Cambios en el modelo de datos

### Entidades nuevas o modificadas

_No aplica. El modelo de datos no cambia._

### Migración necesaria

_No aplica. No se requiere migración._

## 4. DTOs

### DTOs de entrada

_No aplica. No se crean ni modifican DTOs de entrada._

### DTOs de salida

_No aplica. No se crean ni modifican DTOs de salida._

## 5. Endpoints

_No aplica. No se crean ni modifican endpoints._ El endpoint `DELETE /api/UsuariosAsignados/{id}` ya existe, solo cambia su comportamiento interno de validación.

## 6. Lógica de negocio

Reglas de negocio a implementar en `UsuarioAsignadoLogica.EliminarAsync`:

1. **Validación previa a eliminación:**
   - Antes de eliminar el usuario, verificar si existen tareas en `TodoItems` con `UsuarioAsignadoId` igual al id del usuario a eliminar.
   - Usar `await _contexto.TodoItems.AnyAsync(t => t.UsuarioAsignadoId == id)` para realizar la comprobación.

2. **Lanzar excepción si hay tareas asociadas:**
   - Si la comprobación anterior devuelve `true`, lanzar una `InvalidOperationException` con el mensaje:
     ```
     No se puede eliminar el usuario asignado con Id {id} porque tiene tareas asociadas.
     ```

3. **Proceder con eliminación si no hay tareas:**
   - Si no hay tareas asociadas, proceder con la eliminación normal (ya implementada).

**Referencia:** La implementación debe seguir exactamente el mismo patrón que `CategoriaLogica.EliminarAsync` (líneas 40-50 de `LogicaNegocio/CategoriaLogica.cs`).

## 7. Capas afectadas

### Crear:

_No se crean archivos nuevos._

### Modificar:

- `LogicaNegocio/UsuarioAsignadoLogica.cs` — añadir validación de integridad referencial en el método `EliminarAsync` (antes de la línea `_contexto.UsuariosAsignados.Remove(existente);`)
- `Controllers/UsuariosAsignadosController.cs` — añadir bloque `try-catch` en el método `Eliminar` para capturar `InvalidOperationException` y devolver `BadRequest` con el mensaje de error

## 8. Tests unitarios a implementar

_Esta corrección NO incluye tests unitarios_ porque el proyecto no cuenta actualmente con un proyecto de pruebas xUnit. La adición de tests se abordará en una historia separada de configuración de infraestructura de testing.

**Tests recomendados para cuando se implemente el proyecto de tests:**

- `EliminarUsuarioAsignadoConTareasAsociadas_LanzaInvalidOperationException`: verificar que al intentar eliminar un usuario con tareas asignadas se lanza la excepción esperada con el mensaje correcto.
- `EliminarUsuarioAsignadoSinTareasAsociadas_EliminaCorrectamente`: verificar que un usuario sin tareas asociadas se elimina sin problemas.
- `ControladorEliminar_CuandoLanzaInvalidOperationException_DevuelveBadRequest`: verificar que el controlador captura la excepción y devuelve código 400 con el error serializado.

## 9. Criterios de aceptación

- [ ] El método `UsuarioAsignadoLogica.EliminarAsync` consulta si hay tareas asociadas antes de eliminar
- [ ] Si hay tareas asociadas, se lanza `InvalidOperationException` con mensaje descriptivo incluyendo el Id
- [ ] Si no hay tareas asociadas, la eliminación procede normalmente (comportamiento actual)
- [ ] El método `UsuariosAsignadosController.Eliminar` tiene un bloque `try-catch` que captura `InvalidOperationException`
- [ ] El controlador devuelve `BadRequest(new { error = ex.Message })` cuando se captura la excepción
- [ ] El proyecto compila sin errores (`dotnet build`)
- [ ] El patrón de validación es idéntico al usado en `CategoriaLogica.EliminarAsync`

## 10. Skills a invocar

Esta corrección **NO requiere el orquestador `nueva-feature`** porque es una modificación puntual de dos archivos existentes sin afectar el modelo de datos ni otros artefactos.

**Implementación directa:**

El agente desarrollador-democopilot debe modificar directamente los dos archivos afectados sin invocar skills intermedios:

| Orden | Skill | Motivo |
|-------|-------|--------|
| 1 | _Modificación directa_ | Editar `LogicaNegocio/UsuarioAsignadoLogica.cs` y `Controllers/UsuariosAsignadosController.cs` |
| 2 | _Compilación_ | Ejecutar `dotnet build` para verificar que no hay errores |
| 3 | `commit-message` | Generar el mensaje de commit siguiendo las convenciones del proyecto |

**Justificación:**

- **diseño-analisis**: N/A — no hay cambios en el modelo de datos ni endpoints nuevos
- **modelo**: N/A — no se modifican entidades
- **dto**: N/A — no se modifican contratos de entrada/salida
- **base-de-datos**: N/A — no hay cambios en AppDbContext ni migraciones
- **logica-negocio**: N/A (implementación directa) — solo se añade una validación a un método existente
- **validaciones**: N/A — la validación se implementa como parte de la lógica de negocio, no como atributo de DTO
- **servicio**: N/A — el servicio no cambia, solo propaga la excepción
- **controlador**: N/A (implementación directa) — solo se añade manejo de excepción
- **ui-ux-pro-max**: N/A — no hay cambios en el frontend
- **frontend-react**: N/A — no hay cambios en el frontend

---

**Implementación técnica:**

### Cambio 1: `LogicaNegocio/UsuarioAsignadoLogica.cs`

Insertar después de la línea `if (existente is null) return false;` y antes de `_contexto.UsuariosAsignados.Remove(existente);`:

```csharp
// Validación: no se puede eliminar un usuario con tareas asociadas
var tieneTareasAsociadas = await _contexto.TodoItems
    .AnyAsync(t => t.UsuarioAsignadoId == id);

if (tieneTareasAsociadas)
    throw new InvalidOperationException(
        $"No se puede eliminar el usuario asignado con Id {id} porque tiene tareas asociadas.");
```

### Cambio 2: `Controllers/UsuariosAsignadosController.cs`

Reemplazar el método `Eliminar` actual (líneas 51-57) por:

```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> Eliminar(int id)
{
    try
    {
        var eliminado = await _servicio.EliminarAsync(id);
        if (!eliminado)
            return NotFound();
        return NoContent();
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

**Referencia:** Patrón idéntico al usado en `CategoriasController.Eliminar` (líneas 48-61 de `Controllers/CategoriasController.cs`).
