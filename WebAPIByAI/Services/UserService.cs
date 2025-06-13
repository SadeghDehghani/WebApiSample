using Microsoft.AspNetCore.Identity;
using WebAPIByAI.Interfaces;
using WebAPIByAI.Models;

namespace WebAPIByAI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public UserService(AppDbContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public void Register(User user, string password)
        {
            user.PasswordHash = _hasher.HashPassword(user, password);
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User? Authenticate(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return null;
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }
    }
}
