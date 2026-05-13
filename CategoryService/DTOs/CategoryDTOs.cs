using System.ComponentModel.DataAnnotations;

namespace CategoryService.DTOs;

public record CreateCategoryDto(
    [Required, MaxLength(100)] 
    string Name,
    
    string? Icon,
    
    string? Color,
    [Required]
    string Type
);

public record UpdateCategoryDto(
    string? Name,

    string? Icon,

    string? Color,

    string? Type
);

public record CategoryResponseDto(
    int CategoryId, 
    int? UserId, 
    string Name, 
    string? Icon, 
    string? Color,
    string Type, 
    bool IsDefault, 
    bool IsActive, 
    DateTime CreatedAt
);