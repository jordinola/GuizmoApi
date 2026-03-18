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
        var dtos = new List<GuizmoDto> { new(1, "Widget", "Acme", null, 9.99m, 1, "Electronics") };
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
        var dto = new GuizmoDto(1, "Widget", "Acme", null, 9.99m, 1, "Electronics");
        _serviceMock.Setup(s => s.GetByIdAsync(1, default)).ReturnsAsync(dto);

        var result = await _controller.GetById(1, default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_returns_201_with_location_header()
    {
        var request = new CreateGuizmoRequest("Widget", "Acme", null, 9.99m, 1);
        var dto = new GuizmoDto(1, "Widget", "Acme", null, 9.99m, 1, "Electronics");
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

        var result = await _controller.Update(99, new UpdateGuizmoRequest("X", "Y", null, 1m, 1), default);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_returns_200_when_found()
    {
        var dto = new GuizmoDto(1, "New", "NewMfg", null, 2m, 1, "Electronics");
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateGuizmoRequest>(), default)).ReturnsAsync(dto);

        var result = await _controller.Update(1, new UpdateGuizmoRequest("New", "NewMfg", null, 2m, 1), default);

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

    [Fact]
    public async Task GetPaged_returns_200_with_paged_result()
    {
        var pagedResult = new PagedResult<GuizmoDto>(
            Items: new List<GuizmoDto> { new(1, "Widget", "Acme", null, 9.99m, 1, "Electronics") },
            TotalCount: 1,
            PageNumber: 1,
            PageSize: 10,
            TotalPages: 1
        );
        _serviceMock.Setup(s => s.GetPagedAsync(It.IsAny<GuizmoPagedQuery>(), default))
            .ReturnsAsync(pagedResult);

        var result = await _controller.GetPaged(new GuizmoPagedQuery(), default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeEquivalentTo(pagedResult);
    }

    [Fact]
    public async Task GetPaged_passes_query_params_to_service()
    {
        var query = new GuizmoPagedQuery(PageNumber: 2, PageSize: 5, SortOrder: "desc");
        var pagedResult = new PagedResult<GuizmoDto>(
            Items: Enumerable.Empty<GuizmoDto>(),
            TotalCount: 0,
            PageNumber: 2,
            PageSize: 5,
            TotalPages: 0
        );
        _serviceMock.Setup(s => s.GetPagedAsync(query, default)).ReturnsAsync(pagedResult);

        var result = await _controller.GetPaged(query, default);

        _serviceMock.Verify(s => s.GetPagedAsync(query, default), Times.Once);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRecommended_returns_200_with_list()
    {
        var dtos = new List<GuizmoDto> { new(1, "Widget", "Acme", null, 9.99m, 1, "Electronics") };
        _serviceMock.Setup(s => s.GetRecommendedAsync(5, null, default)).ReturnsAsync(dtos);

        var result = await _controller.GetRecommended(5, new GuizmoRecommendedQuery(null), default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetRecommended_passes_userId_and_guizmoId_to_service()
    {
        _serviceMock.Setup(s => s.GetRecommendedAsync(7, 42, default))
            .ReturnsAsync(Enumerable.Empty<GuizmoDto>());

        await _controller.GetRecommended(7, new GuizmoRecommendedQuery(42), default);

        _serviceMock.Verify(s => s.GetRecommendedAsync(7, 42, default), Times.Once);
    }
}
