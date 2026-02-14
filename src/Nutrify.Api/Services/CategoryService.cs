using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Entities;
using Nutrify.Api.Mapping;
using Nutrify.Contracts.Categories;

namespace Nutrify.Api.Services;

public class CategoryService(NutrifyDbContext db) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync(string userId, string? search = null)
    {
        var query = db.Categories
            .Include(c => c.FoodItems)
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search));
        }

        var categories = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => c.ToDto()).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, string userId)
    {
        var category = await db.Categories
            .Include(c => c.FoodItems)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        return category?.ToDto();
    }

    public async Task<CategoryDto> CreateAsync(string userId, CreateCategoryRequest request)
    {
        var exists = await db.Categories
            .AnyAsync(c => c.UserId == userId && c.Name == request.Name);

        if (exists)
        {
            throw new InvalidOperationException($"Category '{request.Name}' already exists.");
        }

        var category = new Category
        {
            Name = request.Name,
            UserId = userId
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return category.ToDto();
    }

    public async Task<CategoryDto?> UpdateAsync(int id, string userId, UpdateCategoryRequest request)
    {
        var category = await db.Categories
            .Include(c => c.FoodItems)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category is null)
            return null;

        var exists = await db.Categories
            .AnyAsync(c => c.UserId == userId && c.Name == request.Name && c.Id != id);

        if (exists)
        {
            throw new InvalidOperationException($"Category '{request.Name}' already exists.");
        }

        category.Name = request.Name;
        category.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return category.ToDto();
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var category = await db.Categories
            .Include(c => c.FoodItems)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category is null)
            return false;

        if (category.FoodItems.Count != 0)
        {
            throw new InvalidOperationException(
                "Cannot delete category that has food items assigned to it.");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        return true;
    }
}
