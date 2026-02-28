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

            // validate quiz existence and that it has questions
            var quiz = await _ctx.Quizzes
                                 .Include(q => q.Questions)
                                 .FirstOrDefaultAsync(q => q.Id == quizId)
                         ?? throw new KeyNotFoundException("Quiz not found.");

            if (quiz.Questions == null || !quiz.Questions.Any())
                throw new InvalidOperationException("Quiz must contain at least one question.");

            var session = new GameSession
            {
                QuizId = quizId,
                PinCode = pinCode ?? string.Empty,
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

            // cannot join after game has started
            if (session.IsActive)
                throw new InvalidOperationException("Cannot join once the game has already started.");

            // prevent duplicate nicknames
            if (session.Players.Any(p => p.Nickname == nickname))
                throw new InvalidOperationException("A player with the same nickname has already joined the session.");

            var player = new Player
            {
                Nickname = nickname,
                Score = 0
            };

            session.Players.Add(player);
            await _ctx.SaveChangesAsync();

            return player.Id;
        }

        public async Task StartGameSessionAsync(int gameSessionId, string userId)
        {
            var session = await GetSessionAsync(gameSessionId);

            if (session.CreatedById != userId)
                throw new InvalidOperationException("Only the creator can start the session.");

            if (session.IsActive)
                throw new InvalidOperationException("Session is already started.");

            session.IsActive = true;
            session.CurrentQuestionIndex = 0;

            await _ctx.SaveChangesAsync();
        }

        public async Task<QuizQuestionDto?> GetCurrentQuestionAsync(int gameSessionId)
        {
            var session = await GetSessionWithQuizAsync(gameSessionId);

            if (!session.IsActive)
                throw new InvalidOperationException("Game is not active.");

            var questions = session.Quiz?.Questions?.OrderBy(q => q.Id).ToList() ?? new List<Question>();

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

            // make sure the player actually belongs to this session
            var player = session.Players.FirstOrDefault(p => p.Id == playerId)
                         ?? throw new KeyNotFoundException("Player is not part of this game session.");

            var questions = session.Quiz?.Questions?.OrderBy(q => q.Id).ToList() ?? new List<Question>();

            if (session.CurrentQuestionIndex >= questions.Count)
                throw new InvalidOperationException("No current question available.");

            var question = questions[session.CurrentQuestionIndex];

            // prevent answering the same question twice
            if (player.AnsweredQuestions.Any(aq => aq.QuestionId == question.Id))
                throw new InvalidOperationException("Player has already answered the current question.");

            var answer = question.Answers.FirstOrDefault(a => a.Id == answerId)
                         ?? throw new KeyNotFoundException("Answer not found for current question.");

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

            if (session.Quiz?.Questions == null || session.CurrentQuestionIndex >= session.Quiz.Questions.Count)
                session.IsActive = false;

            await _ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<LeaderboardDto>> EndGameSessionAsync(int gameSessionId, string userId)
        {
            var session = await GetSessionWithPlayersAsync(gameSessionId);

            if (session.CreatedById != userId)
                throw new InvalidOperationException("Only the creator can end the session.");

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
                .Include(gs => gs.Quiz!)
                    .ThenInclude(q => q.Questions!)
                        .ThenInclude(q => q.Answers!)
                .FirstOrDefaultAsync(gs => gs.Id == id)
                ?? throw new KeyNotFoundException("Game session not found.");
        }

        private async Task<GameSession> GetSessionWithQuizAndPlayersAsync(int id)
        {
            return await _ctx.GameSessions
                .Include(gs => gs.Quiz!)
                    .ThenInclude(q => q.Questions!)
                        .ThenInclude(q => q.Answers!)
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