using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using GuizmoApi.Api.Controllers;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Interfaces;

namespace GuizmoApi.Api.Tests.Controllers;

public class GuizmosControllerTests
{
    private readonly Mock<IGuizmoService> _serviceMock = new();
    private readonly GuizmosController _controller;

    public GuizmosControllerTests()
    {
        _controller = new GuizmosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_returns_200_with_list()
    {
        var dtos = new List<GuizmoDto> { new(1, "Widget", "Acme", null, 9.99m) };
        _serviceMock.Setup(s => s.GetAllAsync(default)).ReturnsAsync(dtos);

        var result = await _controller.GetAll(default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetById_returns_404_when_not_found()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(99, default)).ReturnsAsync((GuizmoDto?)null);

        var result = await _controller.GetById(99, default);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_returns_200_when_found()
    {
        var dto = new GuizmoDto(1, "Widget", "Acme", null, 9.99m);
        _serviceMock.Setup(s => s.GetByIdAsync(1, default)).ReturnsAsync(dto);

        var result = await _controller.GetById(1, default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_returns_201_with_location_header()
    {
        var request = new CreateGuizmoRequest("Widget", "Acme", null, 9.99m);
        var dto = new GuizmoDto(1, "Widget", "Acme", null, 9.99m);
        _serviceMock.Setup(s => s.CreateAsync(request, default)).ReturnsAsync(dto);

        var result = await _controller.Create(request, default);

        var created = result.Result as CreatedAtActionResult;
        created.Should().NotBeNull();
        created!.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Update_returns_404_when_not_found()
    {
        _serviceMock.Setup(s => s.UpdateAsync(99, It.IsAny<UpdateGuizmoRequest>(), default))
            .ReturnsAsync((GuizmoDto?)null);

        var result = await _controller.Update(99, new UpdateGuizmoRequest("X", "Y", null, 1m), default);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_returns_200_when_found()
    {
        var dto = new GuizmoDto(1, "New", "NewMfg", null, 2m);
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateGuizmoRequest>(), default)).ReturnsAsync(dto);

        var result = await _controller.Update(1, new UpdateGuizmoRequest("New", "NewMfg", null, 2m), default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_returns_204_when_deleted()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, default)).ReturnsAsync(true);

        var result = await _controller.Delete(1, default);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_returns_404_when_not_found()
    {
        _serviceMock.Setup(s => s.DeleteAsync(99, default)).ReturnsAsync(false);

        var result = await _controller.Delete(99, default);

        result.Should().BeOfType<NotFoundResult>();
    }
}
