using Nutrify.Contracts.Common;
using Nutrify.Contracts.Intake;

namespace Nutrify.Api.Services;

public interface IIntakeService
{
    Task<PagedResponse<IntakeEntryDto>> GetEntriesAsync(
        string userId,
        PaginationRequest pagination,
        DateOnly? date = null,
        DateOnly? from = null,
        DateOnly? to = null,
        string? search = null);

    Task<IntakeEntryDto?> GetByIdAsync(int id, string userId);
    Task<IntakeEntryDto> CreateAsync(string userId, CreateIntakeEntryRequest request);
    Task<IntakeEntryDto?> UpdateAsync(int id, string userId, UpdateIntakeEntryRequest request);
    Task<bool> DeleteAsync(int id, string userId);
}
