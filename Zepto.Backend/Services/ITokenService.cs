using Zepto.Backend.Models;

namespace Zepto.Backend.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
