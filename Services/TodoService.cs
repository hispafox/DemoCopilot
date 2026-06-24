using AppTodoList.LogicaNegocio;
using AppTodoList.Models;

namespace AppTodoList.Services;

public class TodoService : ITodoService
{
    private readonly ITodoLogica _logica;

    public TodoService(ITodoLogica logica)
    {
        _logica = logica;
    }

    public async Task<IEnumerable<TodoItem>> ObtenerTodosAsync()
        => await _logica.ObtenerTodosAsync();

    public async Task<TodoItem?> ObtenerPorIdAsync(int id)
        => await _logica.ObtenerPorIdAsync(id);

    public async Task<TodoItem> CrearAsync(TodoItem tarea)
        => await _logica.CrearAsync(tarea);

    public async Task<TodoItem?> ActualizarAsync(TodoItem tarea)
        => await _logica.ActualizarAsync(tarea.Id, tarea);

    public async Task<bool> EliminarAsync(int id)
        => await _logica.EliminarAsync(id);

    public async Task<TodoItem?> CompletarAsync(int id)
        => await _logica.CompletarAsync(id);
}
