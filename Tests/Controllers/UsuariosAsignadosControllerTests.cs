using AppTodoList.Controllers;
using AppTodoList.Dtos;
using AppTodoList.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AppTodoList.Tests.Controllers;

public class UsuariosAsignadosControllerTests
{
    [Fact]
    public async Task Eliminar_ConUsuarioConTareas_DevuelveBadRequest()
    {
        // Arrange
        var mockServicio = new Mock<IUsuarioAsignadoService>();
        mockServicio.Setup(x => x.EliminarAsync(1)).ThrowsAsync(new InvalidOperationException("No se puede eliminar"));
        var controller = new UsuariosAsignadosController(mockServicio.Object);

        // Act
        var resultado = await controller.Eliminar(1);

        // Assert
        var badRequest = resultado.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(400);
    }
}
