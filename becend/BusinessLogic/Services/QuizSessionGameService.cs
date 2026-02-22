using BusinessLogic.Interfaces;
using DataAccess.Data;
using DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static BusinessLogic.Interfaces.IQuizSessionGameService;

namespace BusinessLogic.Services
{
    public class QuizSessionGameService : IQuizSessionGameService
    {
        private readonly IQuizService _quizService;
        private readonly SignInManager<User> _signInManager;
        private readonly QuizRushContext _ctx;

        public QuizSessionGameService(
            IQuizService quizService,
            SignInManager<User> signInManager,
            QuizRushContext ctx)
        {
            _quizService = quizService;
            _signInManager = signInManager;
            _ctx = ctx;
        }

        public async Task<GameSessionDto> CreateGameSessionAsync(
            string userId,
            int quizId,
            string? pinCode,
            string? title,
            string description)
        {
            var user = await _signInManager.UserManager.FindByIdAsync(userId)
                       ?? throw new KeyNotFoundException("User not found.");

            var session = new GameSession
            {
                QuizId = quizId,
                PinCode = pinCode,
                CreatedById = userId,
                StartedAt = DateTime.UtcNow,
                IsActive = false,
                Players = new List<Player>()
            };

            session.Players.Add(new Player
            {
                Nickname = user.UserName!,
                Score = 0
            });

            _ctx.GameSessions.Add(session);
            await _ctx.SaveChangesAsync();

            return new GameSessionDto
            {
                GameSessionId = session.Id,
                QuizId = quizId,
                PinCode = pinCode,
                CreatedAt = session.StartedAt
            };
        }

        public async Task<int> JoinGameSessionAsync(int gameSessionId, string nickname)
        {
            var session = await GetSessionWithPlayersAsync(gameSessionId);

            if (!session.IsActive)
                throw new InvalidOperationException("Game is not active.");

            var player = new Player
            {
                Nickname = nickname,
                Score = 0
            };

            session.Players.Add(player);
            await _ctx.SaveChangesAsync();

            return player.Id;
        }

        public async Task StartGameSessionAsync(int gameSessionId)
        {
            var session = await GetSessionAsync(gameSessionId);

            session.IsActive = true;
            session.CurrentQuestionIndex = 0;

            await _ctx.SaveChangesAsync();
        }

        public async Task<QuizQuestionDto?> GetCurrentQuestionAsync(int gameSessionId)
        {
            var session = await GetSessionWithQuizAsync(gameSessionId);

            if (!session.IsActive)
                throw new InvalidOperationException("Game is not active.");

            var questions = session.Quiz.Questions.OrderBy(q => q.Id).ToList();

            if (session.CurrentQuestionIndex >= questions.Count)
                return null;

            var question = questions[session.CurrentQuestionIndex];

            return new QuizQuestionDto
            {
                QuestionId = question.Id,
                QuestionText = question.Text,
                AnswerOptions = question.Answers.Select(a => new QuizAnswerOptionDto
                {
                    AnswerId = a.Id,
                    AnswerText = a.Text
                }).ToList()
            };
        }

        public async Task SubmitAnswerAsync(int gameSessionId, int playerId, int answerId)
        {
            var session = await GetSessionWithQuizAndPlayersAsync(gameSessionId);

            if (!session.IsActive)
                throw new InvalidOperationException("Game is not active.");

            var player = session.Players.First(p => p.Id == playerId);

            var question = session.Quiz.Questions
                .OrderBy(q => q.Id)
                .ElementAt(session.CurrentQuestionIndex);

            var answer = question.Answers.First(a => a.Id == answerId);

            bool isCorrect = answer.IsCorrect;

            if (isCorrect)
                player.Score++;

            player.AnsweredQuestions.Add(new AnsweredQuestion
            {
                QuestionId = question.Id,
                SelectedAnswerId = answerId,
                IsCorrect = isCorrect,
                Points = isCorrect ? 1 : 0
            });

            await _ctx.SaveChangesAsync();
        }

        public async Task NextQuestionAsync(int gameSessionId)
        {
            var session = await GetSessionWithQuizAsync(gameSessionId);

            session.CurrentQuestionIndex++;

            if (session.CurrentQuestionIndex >= session.Quiz.Questions.Count)
                session.IsActive = false;

            await _ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<LeaderboardDto>> EndGameSessionAsync(int gameSessionId)
        {
            var session = await GetSessionWithPlayersAsync(gameSessionId);

            session.IsActive = false;
            await _ctx.SaveChangesAsync();

            return MapLeaderboard(session);
        }

        public async Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync(int gameSessionId)
        {
            var session = await GetSessionWithPlayersAsync(gameSessionId);
            return MapLeaderboard(session);
        }

        public async Task<List<GameSessionDto>> GetAllGameSessionsAsync()
        {
            var sessions = await _ctx.GameSessions.ToListAsync();

            return sessions.Select(s => new GameSessionDto
            {
                GameSessionId = s.Id,
                QuizId = s.QuizId,
                PinCode = s.PinCode,
                CreatedAt = s.StartedAt
            }).ToList();
        }

        public async Task DeleteGameSessionAsync(int gameSessionId)
        {
            var session = await _ctx.GameSessions.FindAsync(gameSessionId)
                          ?? throw new KeyNotFoundException("Game session not found.");

            _ctx.GameSessions.Remove(session);
            await _ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<QuizAttemptResultDto>> GetAllPlayersResultsAsync(int gameSessionId)
        {
            var session = await GetSessionWithPlayersAsync(gameSessionId);

            return session.Players
                          .SelectMany(p => p.AnsweredQuestions.Select(aq => new QuizAttemptResultDto
                          {
                              QuestionId = aq.QuestionId,
                              SelectedAnswerId = aq.SelectedAnswerId,
                              IsCorrect = aq.IsCorrect,
                              Points = aq.Points
                          }))
                          .ToList();
        }

        public async Task<IEnumerable<QuizAttemptResultDto>> GetPlayerResultsAsync(int gameSessionId, int playerId)
        {
            var session = await GetSessionWithPlayersAsync(gameSessionId);

            var player = session.Players.FirstOrDefault(p => p.Id == playerId)
                         ?? throw new KeyNotFoundException("Player not found in this session.");

            return player.AnsweredQuestions.Select(aq => new QuizAttemptResultDto
            {
                QuestionId = aq.QuestionId,
                SelectedAnswerId = aq.SelectedAnswerId,
                IsCorrect = aq.IsCorrect,
                Points = aq.Points
            }).ToList();
        }

        // ------------------------ Приватні методи з бд ------------------------

        private async Task<GameSession> GetSessionAsync(int id)
        {
            return await _ctx.GameSessions
                .FirstOrDefaultAsync(gs => gs.Id == id)
                ?? throw new KeyNotFoundException("Game session not found.");
        }

        private async Task<GameSession> GetSessionWithPlayersAsync(int id)
        {
            return await _ctx.GameSessions
                .Include(gs => gs.Players)
                .FirstOrDefaultAsync(gs => gs.Id == id)
                ?? throw new KeyNotFoundException("Game session not found.");
        }

        private async Task<GameSession> GetSessionWithQuizAsync(int id)
        {
            return await _ctx.GameSessions
                .Include(gs => gs.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(gs => gs.Id == id)
                ?? throw new KeyNotFoundException("Game session not found.");
        }

        private async Task<GameSession> GetSessionWithQuizAndPlayersAsync(int id)
        {
            return await _ctx.GameSessions
                .Include(gs => gs.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                .Include(gs => gs.Players)
                    .ThenInclude(p => p.AnsweredQuestions)
                .FirstOrDefaultAsync(gs => gs.Id == id)
                ?? throw new KeyNotFoundException("Game session not found.");
        }

        private IEnumerable<LeaderboardDto> MapLeaderboard(GameSession session)
        {
            return session.Players
                .OrderByDescending(p => p.Score)
                .Select((p, index) => new LeaderboardDto
                {
                    UserId = p.Id.ToString(),
                    UserName = p.Nickname,
                    TotalPoints = p.Score,
                    Rank = index + 1
                });
        }
    }
}