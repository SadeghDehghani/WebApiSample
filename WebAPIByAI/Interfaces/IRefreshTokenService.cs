using WebAPIByAI.Models;

namespace WebAPIByAI.Interfaces
{
    public interface IRefreshTokenService
    {
        RefreshToken GenerateRefreshToken(User user);
        RefreshToken? GetRefreshToken(string token);
        void RemoveOldTokens(User user);




    }
}
