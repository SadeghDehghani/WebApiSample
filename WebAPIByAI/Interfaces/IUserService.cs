using WebAPIByAI.Models;

namespace WebAPIByAI.Interfaces
{
    public interface IUserService
    {
        void Register(User user, string password);
        User? Authenticate(string username, string password);
    }
}
