using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using DataAccess.Data;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly QuizRushContext _context;

        public LeaderboardService(QuizRushContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<PlayerLeaderboardDto>> GetTop10PlayersAsync()
        {
            var topPlayers = await _context.QuizAttempts
                .GroupBy(qa => qa.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalAttempts = g.Count(),
                    AverageScore = g.Average(qa => qa.Score),
                    TotalCorrectAnswers = g.Sum(qa => qa.CorrectAnswers),
                    TotalQuestions = g.Sum(qa => qa.TotalQuestions),
                    LastAttemptAt = g.Max(qa => qa.CompletedAt)
                })
                .OrderByDescending(x => x.AverageScore)
                .ThenByDescending(x => x.TotalAttempts)
                .Take(10)
                .ToListAsync();

            var userIds = topPlayers.Select(p => p.UserId).ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            var result = topPlayers.Select((tp, index) => new PlayerLeaderboardDto
            {
                Position = index + 1,
                UserId = tp.UserId,
                UserName = users.ContainsKey(tp.UserId) ? users[tp.UserId].UserName ?? "Unknown" : "Unknown",
                Email = users.ContainsKey(tp.UserId) ? users[tp.UserId].Email ?? "Unknown" : "Unknown",
                TotalAttempts = tp.TotalAttempts,
                AverageScore = Math.Round(tp.AverageScore, 2),
                TotalCorrectAnswers = tp.TotalCorrectAnswers,
                TotalQuestions = tp.TotalQuestions,
                LastAttemptAt = tp.LastAttemptAt
            }).ToList();

            return result;
        }
        public async Task<IEnumerable<PlayerLeaderboardDto>> GetTop10PlayersByQuizAsync(int quizId)
        {
            var topPlayers = await _context.QuizAttempts
                .Where(qa => qa.QuizId == quizId)
                .GroupBy(qa => qa.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalAttempts = g.Count(),
                    AverageScore = g.Average(qa => qa.Score),
                    TotalCorrectAnswers = g.Sum(qa => qa.CorrectAnswers),
                    TotalQuestions = g.Sum(qa => qa.TotalQuestions),
                    LastAttemptAt = g.Max(qa => qa.CompletedAt)
                })
                .OrderByDescending(x => x.AverageScore)
                .ThenByDescending(x => x.TotalAttempts)
                .Take(10)
                .ToListAsync();

            var userIds = topPlayers.Select(p => p.UserId).ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            var result = topPlayers.Select((tp, index) => new PlayerLeaderboardDto
            {
                Position = index + 1,
                UserId = tp.UserId,
                UserName = users.ContainsKey(tp.UserId) ? users[tp.UserId].UserName ?? "Unknown" : "Unknown",
                Email = users.ContainsKey(tp.UserId) ? users[tp.UserId].Email ?? "Unknown" : "Unknown",
                TotalAttempts = tp.TotalAttempts,
                AverageScore = Math.Round(tp.AverageScore, 2),
                TotalCorrectAnswers = tp.TotalCorrectAnswers,
                TotalQuestions = tp.TotalQuestions,
                LastAttemptAt = tp.LastAttemptAt
            }).ToList();

            return result;
        }
        public async Task<UserStatisticsDto> GetUserStatisticsAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {userId} not found");
            }

            var attempts = await _context.QuizAttempts
                .Where(qa => qa.UserId == userId)
                .Include(qa => qa.Quiz)
                .OrderByDescending(qa => qa.CompletedAt)
                .ToListAsync();

            if (attempts.Count == 0)
            {
                return new UserStatisticsDto
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? "Unknown",
                    Email = user.Email ?? "Unknown",
                    TotalAttempts = 0,
                    AverageScore = 0,
                    TotalCorrectAnswers = 0,
                    TotalQuestions = 0,
                    AccuracyPercentage = 0,
                    TotalDuration = 0,
                    AverageRating = 0,
                    FirstAttemptAt = DateTime.MinValue,
                    LastAttemptAt = DateTime.MinValue,
                    AttemptHistory = new()
                };
            }

            var totalCorrectAnswers = attempts.Sum(a => a.CorrectAnswers);
            var totalQuestions = attempts.Sum(a => a.TotalQuestions);
            // точніть в процентах
            var accuracyPercentage = totalQuestions > 0 ? (totalCorrectAnswers / (double)totalQuestions) * 100 : 0;

            var statisticsDto = new UserStatisticsDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? "Unknown",
                Email = user.Email ?? "Unknown",
                TotalAttempts = attempts.Count,
                AverageScore = Math.Round(attempts.Average(a => a.Score), 2),
                TotalCorrectAnswers = totalCorrectAnswers,
                TotalQuestions = totalQuestions,
                AccuracyPercentage = Math.Round(accuracyPercentage, 2),
                TotalDuration = attempts.Sum(a => a.Duration),
                AverageRating = Math.Round(attempts.Average(a => a.Rating), 2),
                FirstAttemptAt = attempts.Min(a => a.CompletedAt),
                LastAttemptAt = attempts.Max(a => a.CompletedAt),
                AttemptHistory = attempts.Select(a => new AttemptHistoryDto
                {
                    Id = a.Id,
                    QuizId = a.QuizId,
                    QuizTitle = a.Quiz?.Title ?? "Unknown",
                    Score = a.Score,
                    CorrectAnswers = a.CorrectAnswers,
                    TotalQuestions = a.TotalQuestions,
                    CompletedAt = a.CompletedAt,
                    Duration = a.Duration,
                    Rating = a.Rating,
                    ScorePercentage = Math.Round((a.CorrectAnswers / (double)a.TotalQuestions) * 100, 2)
                }).ToList()
            };

            return statisticsDto;
        }
        public async Task<IEnumerable<AttemptHistoryDto>> GetUserAttemptsHistoryAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            var attempts = await _context.QuizAttempts
                .Where(qa => qa.UserId == userId)
                .Include(qa => qa.Quiz)
                .OrderByDescending(qa => qa.CompletedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return attempts.Select(a => new AttemptHistoryDto
            {
                Id = a.Id,
                QuizId = a.QuizId,
                QuizTitle = a.Quiz?.Title ?? "Unknown",
                Score = a.Score,
                CorrectAnswers = a.CorrectAnswers,
                TotalQuestions = a.TotalQuestions,
                CompletedAt = a.CompletedAt,
                Duration = a.Duration,
                Rating = a.Rating,
                ScorePercentage = Math.Round((a.CorrectAnswers / (double)a.TotalQuestions) * 100, 2)
            }).ToList();
        }
        public async Task<IEnumerable<QuizPopularityDto>> GetPopularQuizzesAsync(int limit = 10)
        {
            var popularQuizzes = await _context.QuizAttempts
                .GroupBy(qa => qa.QuizId)
                .Select(g => new
                {
                    QuizId = g.Key,
                    TotalAttempts = g.Count(),
                    AverageScore = g.Average(qa => qa.Score),
                    AverageRating = g.Average(qa => qa.Rating),
                    TotalPlayers = g.Select(qa => qa.UserId).Distinct().Count()
                })
                .OrderByDescending(x => x.TotalAttempts)
                .ThenByDescending(x => x.AverageRating)
                .Take(limit)
                .ToListAsync();

            var quizIds = popularQuizzes.Select(p => p.QuizId).ToList();
            var quizzes = await _context.Quizzes
                .Where(q => quizIds.Contains(q.Id))
                .Include(q => q.CreatedBy)
                .ToDictionaryAsync(q => q.Id, q => q);

            var result = popularQuizzes.Select((pq, index) => new QuizPopularityDto
            {
                Id = pq.QuizId,
                Title = quizzes.ContainsKey(pq.QuizId) ? quizzes[pq.QuizId].Title : "Unknown",
                Description = quizzes.ContainsKey(pq.QuizId) ? quizzes[pq.QuizId].Description : "Unknown",
                ImageUrl = quizzes.ContainsKey(pq.QuizId) ? quizzes[pq.QuizId].ImageUrl : "",
                CreatedByUserName = quizzes.ContainsKey(pq.QuizId) ? quizzes[pq.QuizId].CreatedBy?.UserName ?? "Unknown" : "Unknown",
                TotalAttempts = pq.TotalAttempts,
                AverageScore = Math.Round(pq.AverageScore, 2),
                AverageRating = Math.Round(pq.AverageRating, 2),
                TotalPlayers = pq.TotalPlayers,
                PopularityRank = index + 1
            }).ToList();

            return result;
        }
        public async Task<QuizPopularityDto> GetQuizStatisticsAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.CreatedBy)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                throw new KeyNotFoundException($"Quiz with id {quizId} not found");
            }

            var quizStats = await _context.QuizAttempts
                .Where(qa => qa.QuizId == quizId)
                .GroupBy(qa => qa.QuizId)
                .Select(g => new
                {
                    TotalAttempts = g.Count(),
                    AverageScore = g.Average(qa => qa.Score),
                    AverageRating = g.Average(qa => qa.Rating),
                    TotalPlayers = g.Select(qa => qa.UserId).Distinct().Count()
                })
                .FirstOrDefaultAsync();

            if (quizStats == null)
            {
                return new QuizPopularityDto
                {
                    Id = quiz.Id,
                    Title = quiz.Title,
                    Description = quiz.Description,
                    ImageUrl = quiz.ImageUrl,
                    CreatedByUserName = quiz.CreatedBy?.UserName ?? "Unknown",
                    TotalAttempts = 0,
                    AverageScore = 0,
                    AverageRating = 0,
                    TotalPlayers = 0,
                    PopularityRank = 0
                };
            }

            return new QuizPopularityDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                ImageUrl = quiz.ImageUrl,
                CreatedByUserName = quiz.CreatedBy?.UserName ?? "Unknown",
                TotalAttempts = quizStats.TotalAttempts,
                AverageScore = Math.Round(quizStats.AverageScore, 2),
                AverageRating = Math.Round(quizStats.AverageRating, 2),
                TotalPlayers = quizStats.TotalPlayers,
                PopularityRank = 0
            };
        }
        public async Task<int> GetUserLeaderboardPositionAsync(string userId)
        {
            var userStats = await _context.QuizAttempts
                .Where(qa => qa.UserId == userId)
                .GroupBy(qa => qa.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    AverageScore = g.Average(qa => qa.Score)
                })
                .FirstOrDefaultAsync();

            if (userStats == null)
            {
                return -1; // User has no attempts
            }

            var position = await _context.QuizAttempts
                .GroupBy(qa => qa.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    AverageScore = g.Average(qa => qa.Score)
                })
                .Where(x => x.AverageScore > userStats.AverageScore)
                .CountAsync();

            return position + 1;
        }
        public async Task<int> SaveQuizAttemptAsync(int quizId, string userId, int score, int correctAnswers,
            int totalQuestions, int duration, int rating = 0)
        {
            var quizAttempt = new QuizAttempt
            {
                QuizId = quizId,
                UserId = userId,
                Score = score,
                CorrectAnswers = correctAnswers,
                TotalQuestions = totalQuestions,
                CompletedAt = DateTime.UtcNow,
                Duration = duration,
                Rating = rating
            };

            _context.QuizAttempts.Add(quizAttempt);
            await _context.SaveChangesAsync();

            return quizAttempt.Id;
        }
    }
}
