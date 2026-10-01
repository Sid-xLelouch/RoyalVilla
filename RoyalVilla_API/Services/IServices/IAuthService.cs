using RoyalVilla.DTO;

namespace RoyalVilla_API.Services.IServices
{
    public interface IAuthService
    {
        Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO);

        Task<TokenDTO?> LoginAsync(LoginRequestDTO loginRequestDTO);

        Task<bool> IsEmailExistsAsync(string email);

        Task<TokenDTO?> RefreshAccessTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO);

    }
}
