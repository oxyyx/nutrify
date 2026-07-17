using Nutrify.Contracts.Settings;

namespace Nutrify.Api.Services;

public interface IUserSettingsService
{
    Task<UserSettingsDto> GetAsync(string userId);
    Task<UserSettingsDto> UpdateAsync(string userId, UpdateUserSettingsRequest request);
}
