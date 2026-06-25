using AppTodoList.Data;
using AppTodoList.LogicaNegocio;
using AppTodoList.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

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

app.Run();
