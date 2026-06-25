# DemoCopilot — Lista de Tareas ASP.NET Core

Demo didáctica para curso de GitHub Copilot. Aplicación CRUD de lista de tareas construida con ASP.NET Core.

## Stack

- **Backend**: ASP.NET Core 8 Minimal API o Controllers (según decisión del curso)
- **Base de datos**: SQLite con Entity Framework Core
- **Frontend**: Razor Pages o React + Vite (según decisión del curso)
- **Tests**: xUnit + Moq

## Arquitectura

Estructura de capas del proyecto:

```
DemoCopilot/
├── Models/          # Entidades de dominio (TodoItem, etc.)
├── Dtos/            # Contratos de entrada/salida de la API
├── Data/            # DbContext y configuración de EF Core
├── LogicaNegocio/   # Reglas de negocio y acceso a datos (IXxxLogica, XxxLogica)
├── Services/        # Orquestación y mapeo DTO ↔ entidad (IXxxService, XxxService)
├── Controllers/     # Endpoints HTTP
└── Tests/           # Proyecto xUnit separado
```

Flujo de llamadas:
```
Controller → [DTO] → Service → [Entidad] → LogicaNegocio → DbContext
```

- No usar patrones complejos (CQRS, mediator) — el objetivo es claridad didáctica.
- Mantener las clases pequeñas y fáciles de leer en pantalla.

## Convenciones de código

- Idioma del código: **castellano** (nombres de clases, métodos, variables).
- Idioma de comentarios y mensajes de UI: **español** (es una demo para hispanohablantes).
- Siempre inyectar dependencias por constructor, nunca `new` directo de servicios.
- Usar `async/await` en todos los métodos que accedan a base de datos.
- Prefijo `I` para interfaces: `ITodoService`.
- Los controladores solo orquestan — sin lógica de negocio dentro de ellos.

## Modelo principal

```csharp
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Build y Tests

```bash
dotnet build
dotnet test
dotnet ef migrations add <Nombre>
dotnet ef database update
```

## Skills disponibles

| Skill | Cuándo usarlo |
|---|---|
| `nueva-feature` | **Orquestador principal.** Implementa cualquier feature nueva de principio a fin (entidad nueva, campo nuevo, endpoint nuevo). Ejecuta todos los skills necesarios en orden. |
| `diseño-analisis` | Crear o regenerar `docs/analisis-diseño.md` |
| `modelo` | Crear o actualizar entidades en `Models/` |
| `dto` | Crear o actualizar DTOs en `Dtos/` |
| `base-de-datos` | Configurar `AppDbContext`, Fluent API y migraciones |
| `logica-negocio` | Crear o actualizar `LogicaNegocio/` |
| `validaciones` | Añadir validaciones a DTOs y lógica |
| `servicio` | Crear o actualizar `Services/` |
| `controlador` | Crear o actualizar `Controllers/` |
| `frontend-react` | Crear o actualizar el frontend React + Vite + TypeScript en `frontend/` |
| `commit-message` | Generar el mensaje de commit |
| `estimacion-proyecto` | Generar `docs/insights-proyecto.xlsx` con estimaciones de esfuerzo, comparativa con/sin Copilot, modelo de datos y endpoints |

## Agentes disponibles

| Agente | Cuándo usarlo |
|---|---|
| `@planificador-democopilot` | Planificar una feature nueva, generar `docs/plan-*.md` |
| `@desarrollador-democopilot` | Implementar un plan de `docs/plan-*.md` capa a capa |
| `@verificador-democopilot` | Verificar que la implementación de un plan es correcta |
| `@auditor-calidad` | **Auditoría completa de calidad:** code smells, deuda técnica, SOLID, seguridad, convenciones. Modo abogado del diablo. |

## Reglas para la demo

- **Cada feature nueva lleva su test** — no crear issues separados para tests.
- Mantener el código simple: si hay una forma más corta de hacer algo, úsala.
- No añadir features no pedidas (sin logging estructurado, sin health checks, sin paginación) a menos que se solicite explícitamente.
- Los snippets de código deben caber en una pantalla de presentación (~30 líneas).
