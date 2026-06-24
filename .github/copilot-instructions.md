# DemoCopilot — Lista de Tareas ASP.NET Core

Demo didáctica para curso de GitHub Copilot. Aplicación CRUD de lista de tareas construida con ASP.NET Core.

## Stack

- **Backend**: ASP.NET Core 8 Minimal API o Controllers (según decisión del curso)
- **Base de datos**: SQLite con Entity Framework Core
- **Frontend**: Razor Pages o React + Vite (según decisión del curso)
- **Tests**: xUnit + Moq

## Arquitectura

Estructura de capas simple, apropiada para una demo:

```
DemoCopilot/
├── Models/          # Entidades de dominio (TodoItem, etc.)
├── Data/            # DbContext y configuración de EF Core
├── Services/        # Lógica de negocio (ITodoService, TodoService)
├── Controllers/     # Endpoints HTTP
└── Tests/           # Proyecto xUnit separado
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

## Reglas para la demo

- **Cada feature nueva lleva su test** — no crear issues separados para tests.
- Mantener el código simple: si hay una forma más corta de hacer algo, úsala.
- No añadir features no pedidas (sin logging estructurado, sin health checks, sin paginación) a menos que se solicite explícitamente.
- Los snippets de código deben caber en una pantalla de presentación (~30 líneas).
