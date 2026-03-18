using System.ComponentModel.DataAnnotations;

namespace GuizmoApi.Application.DTOs;

public record CreateCategoryRequest(
    string Name
);
