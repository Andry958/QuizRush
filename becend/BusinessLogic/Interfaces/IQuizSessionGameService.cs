using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IQuizSessionGameService
    { 
        Task<GameSessionDto> CreateGameSessionAsync(string userId, int quizId, string? pinCode, string? title, string description);
        Task<int> JoinGameSessionAsync(int gameSessionId, string nickName);

        Task StartGameSessionAsync(int gameSessionId);
        Task SubmitAnswerAsync(int gameSessionId, int playerId, int answerId);
        Task<QuizQuestionDto?> GetCurrentQuestionAsync(int gameSessionId);
        Task NextQuestionAsync(int gameSessionId);
        Task<IEnumerable<LeaderboardDto>> EndGameSessionAsync(int gameSessionId);
        Task<IEnumerable<QuizAttemptResultDto>> GetPlayerResultsAsync(int gameSessionId, int playerId);
        Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync(int gameSessionId);
        Task<IEnumerable<QuizAttemptResultDto>> GetAllPlayersResultsAsync(int gameSessionId);
        Task<List<GameSessionDto>> GetAllGameSessionsAsync();
        Task DeleteGameSessionAsync(int gameSessionId);

        // --- DTO ---
        public class GameSessionDto
        {
            public int GameSessionId { get; set; }
            public int QuizId { get; set; }
            public string? PinCode { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class QuizAttemptResultDto
        {
            public int QuestionId { get; set; }
            public int SelectedAnswerId { get; set; }
            public bool IsCorrect { get; set; }
            public int Points { get; set; }
        }

        public class QuizQuestionDto
        {
            public int QuestionId { get; set; }
            public string QuestionText { get; set; }
            public List<QuizAnswerOptionDto> AnswerOptions { get; set; } = new();
        }

        public class QuizAnswerOptionDto
        {
            public int AnswerId { get; set; }
            public string AnswerText { get; set; }
        }

        public class LeaderboardDto
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public int TotalPoints { get; set; }
            public int Rank { get; set; }
        }
    }
}