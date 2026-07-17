using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Entities;
using Nutrify.Api.Mapping;
using Nutrify.Contracts.Common;
using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Services;

public class FoodItemService(NutrifyDbContext db, IOpenFoodFactsClient openFoodFacts) : IFoodItemService
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
            var pattern = search.ToLower();
            query = query.Where(f => f.Name.ToLower().Contains(pattern));
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

    public async Task<BarcodeLookupResponse?> LookupByBarcodeAsync(
        string barcode,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeBarcode(barcode);
        if (normalized is null)
            throw new ArgumentException("Barcode must be 6 to 32 digits.");

        var existing = await db.FoodItems
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.UserId == userId && f.Barcode == normalized, cancellationToken);

        if (existing is not null)
            return new BarcodeLookupResponse(BarcodeLookupSource.Internal, existing.ToDto(), null);

        var product = await openFoodFacts.GetProductAsync(normalized, cancellationToken);
        return product is not null
            ? new BarcodeLookupResponse(BarcodeLookupSource.External, null, product)
            : null;
    }

    public async Task<FoodItemDto> CreateAsync(string userId, CreateFoodItemRequest request)
    {
        await EnsureCategoryOwnedAsync(request.CategoryId, userId);
        var barcode = await ValidateBarcodeAsync(request.Barcode, userId);
        var (servingSize, servingSizeName) = NormalizeServing(request.ServingSize, request.ServingSizeName);

        var foodItem = new FoodItem
        {
            Name = request.Name,
            Type = request.Type,
            Unit = request.Type == FoodItemType.Drink ? "mL" : "g",
            Barcode = barcode,
            ServingSize = servingSize,
            ServingSizeName = servingSizeName,
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

        await EnsureCategoryOwnedAsync(request.CategoryId, userId);
        var barcode = await ValidateBarcodeAsync(request.Barcode, userId, excludeId: id);
        var (servingSize, servingSizeName) = NormalizeServing(request.ServingSize, request.ServingSizeName);

        foodItem.Name = request.Name;
        foodItem.Type = request.Type;
        foodItem.Unit = request.Type == FoodItemType.Drink ? "mL" : "g";
        foodItem.Barcode = barcode;
        foodItem.ServingSize = servingSize;
        foodItem.ServingSizeName = servingSizeName;
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

    // A serving name without a size is meaningless, so it's dropped; the name
    // is optional and defaults to "serving" in the UI.
    private static (decimal? Size, string? Name) NormalizeServing(decimal? size, string? name)
    {
        if (size is null)
            return (null, null);

        if (size <= 0)
            throw new ArgumentException("Serving size must be greater than zero.");

        var trimmedName = name?.Trim();
        return (size, string.IsNullOrEmpty(trimmedName) ? null : trimmedName);
    }

    // Barcodes are digits only (EAN-8 through EAN-14 / UPC); returns null for
    // blank input so an empty form field clears the barcode.
    private static string? NormalizeBarcode(string? barcode)
    {
        var trimmed = barcode?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;

        return trimmed.Length is >= 6 and <= 32 && trimmed.All(char.IsAsciiDigit)
            ? trimmed
            : throw new ArgumentException("Barcode must be 6 to 32 digits.");
    }

    private async Task<string?> ValidateBarcodeAsync(string? barcode, string userId, int? excludeId = null)
    {
        var normalized = NormalizeBarcode(barcode);
        if (normalized is null)
            return null;

        var taken = await db.FoodItems.AnyAsync(f =>
            f.UserId == userId && f.Barcode == normalized && f.Id != excludeId);

        if (taken)
            throw new InvalidOperationException("Another food item already uses this barcode.");

        return normalized;
    }

    // Categories are per-user; reject references to ids owned by other users
    // with the same "not found" used for genuinely missing ids.
    private async Task EnsureCategoryOwnedAsync(int? categoryId, string userId)
    {
        if (categoryId is null)
            return;

        var owned = await db.Categories.AnyAsync(c => c.Id == categoryId && c.UserId == userId);
        if (!owned)
            throw new KeyNotFoundException($"Category with id {categoryId} not found.");
    }
}
