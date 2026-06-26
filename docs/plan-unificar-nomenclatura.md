# Plan: Unificar Nomenclatura de Métodos (ObtenerTodos vs ObtenerTodas)

> Generado por el agente planificador · 2026-06-26

## 1. Resumen

Unificar la nomenclatura de los métodos que listan entidades en el módulo de Plantillas, cambiando `ObtenerTodas` (femenino) por `ObtenerTodos` (masculino genérico) para mantener consistencia con el resto de módulos de la aplicación (Categorías, Tareas, Usuarios).

## 2. Requisitos funcionales

1. Todos los métodos de listado de entidades deben usar `ObtenerTodos` como nombre base (masculino genérico).
2. La funcionalidad actual debe mantenerse idéntica — solo cambia el nombre del método.
3. El endpoint HTTP `GET /api/plantillas` debe seguir funcionando correctamente tras el cambio.

## 3. Cambios en el modelo de datos

**N/A** — Este cambio no afecta el modelo de datos ni la estructura de la base de datos.

### Entidades nuevas o modificadas

N/A

### Migración necesaria

N/A — No se requiere migración.

## 4. DTOs

**N/A** — Los DTOs existentes no cambian.

### DTOs de entrada

N/A

### DTOs de salida

N/A — `PlantillaDto` permanece igual.

## 5. Endpoints

**N/A** — El endpoint HTTP no cambia su ruta ni su comportamiento. Solo cambia el nombre del método interno del controlador.

| Verbo | Ruta | Cuerpo | Respuesta exitosa | Errores posibles |
|-------|------|--------|-------------------|------------------|
| GET | /api/plantillas | - | 200 OK con `IEnumerable<PlantillaDto>` | - |

> **Nota:** La firma HTTP es idéntica; solo se renombra el método C# del controlador.

## 6. Lógica de negocio

**N/A** — La lógica de negocio no cambia. Solo se renombran los métodos que la implementan.

## 7. Capas afectadas

**Crear:**

Ninguno.

**Modificar:**

- `Controllers/PlantillasController.cs` — renombrar método `ObtenerTodas()` → `ObtenerTodos()`
- `Services/IPlantillaService.cs` — renombrar método `ObtenerTodasAsync()` → `ObtenerTodosAsync()`
- `Services/PlantillaService.cs` — renombrar método `ObtenerTodasAsync()` → `ObtenerTodosAsync()`
- `LogicaNegocio/IPlantillaLogica.cs` — renombrar método `ObtenerTodasAsync()` → `ObtenerTodosAsync()`
- `LogicaNegocio/PlantillaLogica.cs` — renombrar método `ObtenerTodasAsync()` → `ObtenerTodosAsync()`

**Frontend (si existe):**

- `frontend/src/services/plantillaService.ts` (o similar) — verificar si hay llamadas que dependan del nombre del método (normalmente NO, ya que el endpoint HTTP no cambia).

## 8. Tests unitarios a implementar

**N/A** — No se requieren tests nuevos. Los tests existentes de Plantillas (si existen) simplemente necesitarán actualizar las llamadas a los métodos renombrados.

Si existen tests del módulo Plantillas:
- Actualizar llamadas de `servicio.ObtenerTodasAsync()` → `servicio.ObtenerTodosAsync()`
- Actualizar llamadas de `logica.ObtenerTodasAsync()` → `logica.ObtenerTodosAsync()`

## 9. Criterios de aceptación

1. ✅ Todos los métodos de listado del módulo Plantillas se llaman `ObtenerTodos` o `ObtenerTodosAsync`.
2. ✅ No existen referencias a `ObtenerTodas` en ninguna capa (Controller, Service, LogicaNegocio).
3. ✅ El endpoint `GET /api/plantillas` sigue funcionando correctamente y devuelve la lista de plantillas.
4. ✅ La aplicación compila sin errores.
5. ✅ Los tests existentes (si los hay) pasan correctamente tras actualizar las llamadas.
6. ✅ El frontend (si existe) sigue funcionando sin cambios (el endpoint HTTP no cambia).

## 10. Skills a invocar

Este es un cambio puramente de refactoring/nomenclatura que **NO requiere la ejecución de skills** del catálogo de `docs/skills-orquestacion.md`, ya que:

- No hay cambios en el modelo de datos → **N/A** `diseño-analisis`, `modelo`
- No hay cambios en DTOs → **N/A** `dto`
- No hay cambios en base de datos → **N/A** `base-de-datos`
- No hay cambios en lógica de negocio → **N/A** `logica-negocio`
- No hay cambios en validaciones → **N/A** `validaciones`
- No hay cambios en servicios (solo renombrado) → **N/A** `servicio`
- No hay cambios en endpoints (solo renombrado) → **N/A** `controlador`

**Implementación directa:**

Este cambio se implementa mediante operaciones de **renombrado de símbolos** (refactoring) en las 5 clases afectadas:

1. Renombrar `ObtenerTodasAsync` → `ObtenerTodosAsync` en `IPlantillaLogica.cs`
2. Renombrar `ObtenerTodasAsync` → `ObtenerTodosAsync` en `PlantillaLogica.cs`
3. Renombrar `ObtenerTodasAsync` → `ObtenerTodosAsync` en `IPlantillaService.cs`
4. Renombrar `ObtenerTodasAsync` → `ObtenerTodosAsync` en `PlantillaService.cs`
5. Renombrar `ObtenerTodas` → `ObtenerTodos` en `PlantillasController.cs`

**Skill final:**

| Orden | Skill | Motivo |
|-------|-------|--------|
| 1 | `commit-message` | Generar mensaje de commit tras completar el renombrado |

---

## Notas de implementación

### Orden de renombrado recomendado

Para evitar errores de compilación durante el proceso, renombrar en este orden (de dentro hacia fuera):

1. Interfaz `IPlantillaLogica` → renombrar método
2. Implementación `PlantillaLogica` → renombrar método (corrige error de implementación de interfaz)
3. Interfaz `IPlantillaService` → renombrar método
4. Implementación `PlantillaService` → renombrar método (corrige error de implementación de interfaz)
5. Controlador `PlantillasController` → renombrar método (corrige llamada al servicio)

### Verificación post-implementación

Ejecutar:

```bash
dotnet build
```

Debe compilar sin errores ni warnings.

Si existen tests:

```bash
dotnet test
```

Deben pasar todos los tests tras actualizar las llamadas a los métodos renombrados.
