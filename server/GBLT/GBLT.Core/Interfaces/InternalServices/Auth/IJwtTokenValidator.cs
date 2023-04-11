using System.Security.Claims;

namespace Core.Service
{
    public interface IJwtTokenValidator
    {
        ClaimsPrincipal GetPrincipalFromToken(string token);
    }
}