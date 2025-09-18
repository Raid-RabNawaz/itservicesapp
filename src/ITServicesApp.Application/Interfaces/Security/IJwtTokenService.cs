using ITServicesApp.Application.DTOs;

namespace ITServicesApp.Application.Interfaces.Security
{
    public interface IJwtTokenService
    {
        string CreateToken(UserDto user);
    }
}
