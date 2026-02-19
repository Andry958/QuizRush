using Microsoft.AspNetCore.Identity;

namespace DataAccess.Models
{
    public class User : IdentityUser
    {
        public List<Quiz> CreatedQuizzes { get; set; } = new();
        public List<GameSession> GameSessions { get; set; } = new();

        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
