using Microsoft.EntityFrameworkCore;
using Nutrify.Api.Data;
using Nutrify.Api.Entities;
using Nutrify.Api.Mapping;
using Nutrify.Contracts.Common;
using Nutrify.Contracts.Intake;

namespace Nutrify.Api.Services;

public class IntakeService(NutrifyDbContext db) : IIntakeService
{
    public async Task<PagedResponse<IntakeEntryDto>> GetEntriesAsync(
        string userId,
        PaginationRequest pagination,
        DateOnly? date = null,
        DateOnly? from = null,
        DateOnly? to = null,
        string? search = null)
    {
        var query = db.IntakeEntries
            .Include(e => e.FoodItem)
            .Where(e => e.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = search.ToLower();
            query = query.Where(e => e.FoodItem.Name.ToLower().Contains(pattern));
        }

        if (date.HasValue)
        {
            var start = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end = date.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(e => e.ConsumedAt >= start && e.ConsumedAt < end);
        }
        else
        {
            if (from.HasValue)
            {
                var start = from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                query = query.Where(e => e.ConsumedAt >= start);
            }

            if (to.HasValue)
            {
                var end = to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                query = query.Where(e => e.ConsumedAt < end);
            }
        }

        var totalCount = await query.CountAsync();
        var page = Math.Max(1, pagination.Page);
        var pageSize = Math.Clamp(pagination.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(e => e.ConsumedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<IntakeEntryDto>(
            items.Select(e => e.ToDto()).ToList(),
            page,
            pageSize,
            totalCount,
            totalPages
        );
    }

    public async Task<IntakeEntryDto?> GetByIdAsync(int id, string userId)
    {
        var entry = await db.IntakeEntries
            .Include(e => e.FoodItem)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        return entry?.ToDto();
    }

    public async Task<IntakeEntryDto> CreateAsync(string userId, CreateIntakeEntryRequest request)
    {
        var foodItem = await db.FoodItems.FindAsync(request.FoodItemId)
            ?? throw new KeyNotFoundException($"Food item with id {request.FoodItemId} not found.");

        if (foodItem.UserId != userId)
        {
            throw new KeyNotFoundException($"Food item with id {request.FoodItemId} not found.");
        }

        var entry = new IntakeEntry
        {
            UserId = userId,
            FoodItemId = request.FoodItemId,
            Amount = request.Amount,
            ConsumedAt = request.ConsumedAt ?? DateTime.UtcNow
        };

        db.IntakeEntries.Add(entry);
        await db.SaveChangesAsync();

        await db.Entry(entry).Reference(e => e.FoodItem).LoadAsync();

        return entry.ToDto();
    }

    public async Task<IntakeEntryDto?> UpdateAsync(int id, string userId, UpdateIntakeEntryRequest request)
    {
        var entry = await db.IntakeEntries
            .Include(e => e.FoodItem)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (entry is null)
            return null;

        entry.Amount = request.Amount;
        entry.ConsumedAt = request.ConsumedAt;

        await db.SaveChangesAsync();

        return entry.ToDto();
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var entry = await db.IntakeEntries
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (entry is null)
            return false;

        db.IntakeEntries.Remove(entry);
        await db.SaveChangesAsync();

        return true;
    }
}
