using WebAPIByAI.Interfaces;
using WebAPIByAI.Models;

namespace WebAPIByAI.Services
{

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AppDbContext _context;

        public RefreshTokenService(AppDbContext context)
        {
            _context = context;
        }

        public RefreshToken GenerateRefreshToken(User user)
        {
            var token = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                User = user
            };

            _context.RefreshTokens.Add(token);
            _context.SaveChanges();

            return token;
        }

        public RefreshToken? GetRefreshToken(string token)
        {
            return _context.RefreshTokens.FirstOrDefault(t => t.Token == token);
        }

        public void RemoveOldTokens(User user)
        {
            var oldTokens = _context.RefreshTokens.Where(t => t.UserId == user.Id);
            _context.RefreshTokens.RemoveRange(oldTokens);
            _context.SaveChanges();
        }
    }
}
