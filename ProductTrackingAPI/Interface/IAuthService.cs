using ProductTrackingAPI.DTOs;
using System.Security.Claims;

namespace ProductTrackingAPI.Interface
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

        Task<string> ChangePasswordAsync(ChangePasswordRequesDto request, ClaimsPrincipal userClaims);
    }
}
