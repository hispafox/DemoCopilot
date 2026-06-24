using AppTodoList.LogicaNegocio;
using AppTodoList.Models;

namespace AppTodoList.Services;

public class PlantillaService : IPlantillaService
{
    private readonly IPlantillaLogica _logica;

    public PlantillaService(IPlantillaLogica logica)
    {
        _logica = logica;
    }

    public async Task<IEnumerable<PlantillaTarea>> ObtenerTodasAsync()
        => await _logica.ObtenerTodasAsync();

    public async Task<PlantillaTarea?> ObtenerPorIdAsync(int id)
        => await _logica.ObtenerPorIdAsync(id);

    public async Task<PlantillaTarea> CrearAsync(PlantillaTarea plantilla)
        => await _logica.CrearAsync(plantilla);

    public async Task<PlantillaTarea?> ActualizarAsync(PlantillaTarea plantilla)
        => await _logica.ActualizarAsync(plantilla.Id, plantilla);

    public async Task<bool> EliminarAsync(int id)
        => await _logica.EliminarAsync(id);

    public async Task<TodoItem?> InstanciarAsync(int id)
        => await _logica.InstanciarAsync(id);
}
