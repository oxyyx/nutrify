using Nutrify.Contracts.Common;
using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Services;

public interface IFoodItemService
{
    Task<PagedResponse<FoodItemDto>> GetAllAsync(
        string userId,
        PaginationRequest pagination,
        string? search = null,
        int? categoryId = null,
        FoodItemType? type = null);

    Task<FoodItemDto?> GetByIdAsync(int id, string userId);
    Task<FoodItemDto> CreateAsync(string userId, CreateFoodItemRequest request);
    Task<FoodItemDto?> UpdateAsync(int id, string userId, UpdateFoodItemRequest request);
    Task<bool> DeleteAsync(int id, string userId);
}
