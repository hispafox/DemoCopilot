using AppTodoList.Models;

namespace AppTodoList.Services;

public interface ITodoService
{
    Task<IEnumerable<TodoItem>> ObtenerTodosAsync();
    Task<TodoItem?> ObtenerPorIdAsync(int id);
    Task<TodoItem> CrearAsync(TodoItem tarea);
    Task<TodoItem?> ActualizarAsync(TodoItem tarea);
    Task<bool> EliminarAsync(int id);
    Task<TodoItem?> CompletarAsync(int id);
}
