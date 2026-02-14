using Nutrify.Contracts.Categories;

namespace Nutrify.Api.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(string userId, string? search = null);
    Task<CategoryDto?> GetByIdAsync(int id, string userId);
    Task<CategoryDto> CreateAsync(string userId, CreateCategoryRequest request);
    Task<CategoryDto?> UpdateAsync(int id, string userId, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int id, string userId);
}
