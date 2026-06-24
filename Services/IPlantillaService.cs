using AppTodoList.Models;

namespace AppTodoList.Services;

public interface IPlantillaService
{
    Task<IEnumerable<PlantillaTarea>> ObtenerTodasAsync();
    Task<PlantillaTarea?> ObtenerPorIdAsync(int id);
    Task<PlantillaTarea> CrearAsync(PlantillaTarea plantilla);
    Task<PlantillaTarea?> ActualizarAsync(PlantillaTarea plantilla);
    Task<bool> EliminarAsync(int id);
    Task<TodoItem?> InstanciarAsync(int id);
}
