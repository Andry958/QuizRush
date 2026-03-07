using BusinessLogic.DTOs;

namespace BusinessLogic.Interfaces
{
    public interface ILeaderboardService
    {
        Task<IEnumerable<PlayerLeaderboardDto>> GetTop10PlayersAsync();
        Task<IEnumerable<PlayerLeaderboardDto>> GetTop10PlayersByQuizAsync(int quizId);
        Task<UserStatisticsDto> GetUserStatisticsAsync(string userId);
        Task<IEnumerable<AttemptHistoryDto>> GetUserAttemptsHistoryAsync(string userId, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<QuizPopularityDto>> GetPopularQuizzesAsync(int limit = 10);
        Task<QuizPopularityDto> GetQuizStatisticsAsync(int quizId);
        Task<int> GetUserLeaderboardPositionAsync(string userId);
        Task<int> SaveQuizAttemptAsync(int quizId, string userId, int score, int correctAnswers, 
            int totalQuestions, int duration, int rating = 0);
    }
}
