using AppTodoList.Controllers;
using AppTodoList.Dtos;
using AppTodoList.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AppTodoList.Tests.Controllers;

public class PlantillasControllerTests
{
    [Fact]
    public async Task Instanciar_ConPlantillaExistente_DevuelveCreatedAtAction()
    {
        // Arrange
        var mockServicio = new Mock<IPlantillaService>();
        mockServicio.Setup(x => x.InstanciarAsync(1)).ReturnsAsync(new TareaDto { Id = 5, Title = "Instanciada" });
        var controller = new PlantillasController(mockServicio.Object);

        // Act
        var resultado = await controller.Instanciar(1);

        // Assert
        var created = resultado.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Eliminar_ConPlantillaConTareas_DevuelveBadRequest()
    {
        // Arrange
        var mockServicio = new Mock<IPlantillaService>();
        mockServicio.Setup(x => x.EliminarAsync(1)).ThrowsAsync(new InvalidOperationException("No se puede eliminar"));
        var controller = new PlantillasController(mockServicio.Object);

        // Act
        var resultado = await controller.Eliminar(1);

        // Assert
        var badRequest = resultado.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(400);
    }
}
