using TweetAPI.Domain;

namespace TweetAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string email, string password);
    }
}
