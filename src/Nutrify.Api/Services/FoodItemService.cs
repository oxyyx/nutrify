using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Entities;
using Nutrify.Api.Mapping;
using Nutrify.Contracts.Common;
using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Services;

public class FoodItemService(NutrifyDbContext db) : IFoodItemService
{
    public async Task<PagedResponse<FoodItemDto>> GetAllAsync(
        string userId,
        PaginationRequest pagination,
        string? search = null,
        int? categoryId = null,
        FoodItemType? type = null)
    {
        var query = db.FoodItems
            .Include(f => f.Category)
            .Where(f => f.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(f => f.Name.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(f => f.CategoryId == categoryId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(f => f.Type == type.Value);
        }

        var totalCount = await query.CountAsync();
        var page = Math.Max(1, pagination.Page);
        var pageSize = Math.Clamp(pagination.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<FoodItemDto>(
            items.Select(f => f.ToDto()).ToList(),
            page,
            pageSize,
            totalCount,
            totalPages
        );
    }

    public async Task<FoodItemDto?> GetByIdAsync(int id, string userId)
    {
        var foodItem = await db.FoodItems
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

        return foodItem?.ToDto();
    }

    public async Task<FoodItemDto> CreateAsync(string userId, CreateFoodItemRequest request)
    {
        var foodItem = new FoodItem
        {
            Name = request.Name,
            Type = request.Type,
            Unit = request.Type == FoodItemType.Drink ? "mL" : "g",
            UserId = userId,
            CaloriesKcal = request.CaloriesKcal,
            ProteinG = request.ProteinG,
            CarbohydratesG = request.CarbohydratesG,
            FatG = request.FatG,
            FiberG = request.FiberG,
            CategoryId = request.CategoryId
        };

        db.FoodItems.Add(foodItem);
        await db.SaveChangesAsync();

        // Reload with category for the DTO
        await db.Entry(foodItem).Reference(f => f.Category).LoadAsync();

        return foodItem.ToDto();
    }

    public async Task<FoodItemDto?> UpdateAsync(int id, string userId, UpdateFoodItemRequest request)
    {
        var foodItem = await db.FoodItems
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

        if (foodItem is null)
            return null;

        foodItem.Name = request.Name;
        foodItem.Type = request.Type;
        foodItem.Unit = request.Type == FoodItemType.Drink ? "mL" : "g";
        foodItem.CaloriesKcal = request.CaloriesKcal;
        foodItem.ProteinG = request.ProteinG;
        foodItem.CarbohydratesG = request.CarbohydratesG;
        foodItem.FatG = request.FatG;
        foodItem.FiberG = request.FiberG;
        foodItem.CategoryId = request.CategoryId;
        foodItem.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // Reload category if changed
        await db.Entry(foodItem).Reference(f => f.Category).LoadAsync();

        return foodItem.ToDto();
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var foodItem = await db.FoodItems
            .Include(f => f.IntakeEntries)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

        if (foodItem is null)
            return false;

        if (foodItem.IntakeEntries.Count != 0)
        {
            throw new InvalidOperationException(
                "Cannot delete food item that has intake entries. Remove the intake entries first.");
        }

        db.FoodItems.Remove(foodItem);
        await db.SaveChangesAsync();

        return true;
    }
}
