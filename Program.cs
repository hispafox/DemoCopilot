using AppTodoList.Data;
using AppTodoList.LogicaNegocio;
using AppTodoList.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITodoLogica, TodoLogica>();
builder.Services.AddScoped<IPlantillaLogica, PlantillaLogica>();
builder.Services.AddScoped<IUsuarioAsignadoLogica, UsuarioAsignadoLogica>();
builder.Services.AddScoped<ICategoriaLogica, CategoriaLogica>();

builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<IPlantillaService, PlantillaService>();
builder.Services.AddScoped<IUsuarioAsignadoService, UsuarioAsignadoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "database", tags: new[] { "db", "sql" });

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    contexto.Database.EnsureCreated();
    DatosEjemplo.Inicializar(contexto);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.ToString(),
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            })
        }, new JsonSerializerOptions { WriteIndented = true });
        
        await context.Response.WriteAsync(result);
    }
});

app.Run();

// Hacer Program accesible para tests de integración
public partial class Program { }
