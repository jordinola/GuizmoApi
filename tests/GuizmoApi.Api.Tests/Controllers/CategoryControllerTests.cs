using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using GuizmoApi.Api.Controllers;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Interfaces;

namespace GuizmoApi.Api.Tests.Controllers;

public class CategoryControllerTests
{
    private readonly Mock<ICategoryService> _serviceMock = new();
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _controller = new CategoryController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_returns_200_with_list()
    {
        var dtos = new List<CategoryDto> { new(1, "Electronics"), new(2, "Toys") };
        _serviceMock.Setup(s => s.GetAllAsync(default)).ReturnsAsync(dtos);

        var result = await _controller.GetAll(default);

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetAll_returns_200_with_empty_list_when_no_categories()
    {
        _serviceMock.Setup(s => s.GetAllAsync(default)).ReturnsAsync([]);

        var result = await _controller.GetAll(default);

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Create_returns_201_with_created_category()
    {
        var request = new CreateCategoryRequest("Electronics");
        var dto = new CategoryDto(1, "Electronics");
        _serviceMock.Setup(s => s.CreateAsync(request, default)).ReturnsAsync(dto);

        var result = await _controller.Create(request, default);

        var created = result as CreatedResult;
        created.Should().NotBeNull();
        created!.StatusCode.Should().Be(201);
        created.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task Update_returns_404_when_not_found()
    {
        _serviceMock.Setup(s => s.UpdateAsync(99, It.IsAny<UpdateCategoryRequest>(), default))
            .ReturnsAsync((CategoryDto?)null);

        var result = await _controller.Update(99, new UpdateCategoryRequest("X"), default);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_returns_200_with_updated_category_when_found()
    {
        var dto = new CategoryDto(1, "Updated");
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateCategoryRequest>(), default)).ReturnsAsync(dto);

        var result = await _controller.Update(1, new UpdateCategoryRequest("Updated"), default);

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeEquivalentTo(dto);
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
