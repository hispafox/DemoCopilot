using AppTodoList.Models;

namespace AppTodoList.Data;

public static class DatosEjemplo
{
    public static void Inicializar(AppDbContext contexto)
    {
        SeedUsuarios(contexto);
        SeedPlantillas(contexto);
        SeedTareas(contexto);
    }

    private static void SeedUsuarios(AppDbContext contexto)
    {
        if (contexto.UsuariosAsignados.Any())
            return;

        contexto.UsuariosAsignados.AddRange(
            new UsuarioAsignado { Nombre = "Ana García",    Email = "ana@demo.com"    },
            new UsuarioAsignado { Nombre = "Carlos López",  Email = "carlos@demo.com" }
        );
        contexto.SaveChanges();
    }

    private static void SeedPlantillas(AppDbContext contexto)
    {
        if (contexto.PlantillasTarea.Any())
            return;

        contexto.PlantillasTarea.AddRange(
            new PlantillaTarea { Titulo = "Revisión semanal", EsRepetitiva = true,  Recurrencia = TipoRecurrencia.Semanal },
            new PlantillaTarea { Titulo = "Backup diario",    EsRepetitiva = true,  Recurrencia = TipoRecurrencia.Diaria  }
        );
        contexto.SaveChanges();
    }

    private static void SeedTareas(AppDbContext contexto)
    {
        if (contexto.TodoItems.Any())
            return;

        var ana = contexto.UsuariosAsignados.First(u => u.Email == "ana@demo.com");

        contexto.TodoItems.AddRange(
            new TodoItem
            {
                Title       = "Configurar entorno",
                IsCompleted = true,
                CreatedAt   = DateTime.UtcNow.AddDays(-3)
            },
            new TodoItem
            {
                Title              = "Revisar PR pendientes",
                IsCompleted        = false,
                CreatedAt          = DateTime.UtcNow,
                EsRepetitiva       = true,
                Recurrencia        = TipoRecurrencia.Semanal,
                ProximaFecha       = DateTime.UtcNow.AddDays(7),
                UsuarioAsignadoId  = ana.Id
            }
        );
        contexto.SaveChanges();
    }
}
