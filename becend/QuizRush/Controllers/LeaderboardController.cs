using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QuizRush.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(ILeaderboardService leaderboardService, ILogger<LeaderboardController> logger)
        {
            _leaderboardService = leaderboardService;
            _logger = logger;
        }
        [HttpGet("top10")]
        public async Task<ActionResult<IEnumerable<PlayerLeaderboardDto>>> GetTop10Players()
        {
            try{
                var topPlayers = await _leaderboardService.GetTop10PlayersAsync();
                return Ok(topPlayers);
            }
            catch (Exception ex){
                _logger.LogError($"Error getting top 10 players: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("quiz/{quizId}/top10")]
        public async Task<ActionResult<IEnumerable<PlayerLeaderboardDto>>> GetTop10PlayersByQuiz(int quizId)
        {
            try{
                var topPlayers = await _leaderboardService.GetTop10PlayersByQuizAsync(quizId);
                return Ok(topPlayers);
            }
            catch (Exception ex){
                _logger.LogError($"Error getting top 10 players by quiz {quizId}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics(string userId)
        {
            try{
                var statistics = await _leaderboardService.GetUserStatisticsAsync(userId);
                return Ok(statistics);
            }
            catch (KeyNotFoundException ex){
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex){
                _logger.LogError($"Error getting user statistics for {userId}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}/history")]
        public async Task<ActionResult<IEnumerable<AttemptHistoryDto>>> GetUserAttemptsHistory(
            string userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try{
                if (pageNumber < 1 || pageSize < 1)
                    return BadRequest(new { message = "Page number and page size must be greater than 0" });

                var history = await _leaderboardService.GetUserAttemptsHistoryAsync(userId, pageNumber, pageSize);
                return Ok(history);
            }
            catch (Exception ex) { 
                _logger.LogError($"Error getting user attempts history for {userId}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("popular-quizzes")]
        public async Task<ActionResult<IEnumerable<QuizPopularityDto>>> GetPopularQuizzes([FromQuery] int limit = 10)
        {
            try{
                if (limit < 1)
                    return BadRequest(new { message = "Limit must be greater than 0" });

                var popularQuizzes = await _leaderboardService.GetPopularQuizzesAsync(limit);
                return Ok(popularQuizzes);
            }
            catch (Exception ex){
                _logger.LogError($"Error getting popular quizzes: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("quiz/{quizId}/statistics")]
        public async Task<ActionResult<QuizPopularityDto>> GetQuizStatistics(int quizId)
        {
            try{
                var statistics = await _leaderboardService.GetQuizStatisticsAsync(quizId);
                return Ok(statistics);
            }
            catch (KeyNotFoundException ex){
                _logger.LogWarning($"Quiz not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex){
                _logger.LogError($"Error getting quiz statistics for {quizId}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}/position")]
        public async Task<ActionResult<object>> GetUserLeaderboardPosition(string userId)
        {
            try{
                var position = await _leaderboardService.GetUserLeaderboardPositionAsync(userId);
                if (position == -1)
                    return Ok(new { position = 0, message = "User has no attempts yet" });

                return Ok(new { position });
            }
            catch (Exception ex){
                _logger.LogError($"Error getting user leaderboard position for {userId}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("attempt")]
        [Authorize]
        public async Task<ActionResult<object>> SaveQuizAttempt([FromBody] SaveQuizAttemptRequest request)
        {
            try{
                if (request == null)
                    return BadRequest(new { message = "Request body is required" });

                if (request.QuizId <= 0 || request.TotalQuestions <= 0 || request.CorrectAnswers < 0)
                    return BadRequest(new { message = "Invalid request parameters" });

                if (request.CorrectAnswers > request.TotalQuestions)
                    return BadRequest(new { message = "Correct answers cannot be greater than total questions" });

                if (request.Rating < 0 || request.Rating > 5)
                    return BadRequest(new { message = "Rating must be between 0 and 5" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found" });

                var attemptId = await _leaderboardService.SaveQuizAttemptAsync(
                    request.QuizId,
                    userId,
                    request.Score,
                    request.CorrectAnswers,
                    request.TotalQuestions,
                    request.Duration,
                    request.Rating
                );

                return Ok(new { id = attemptId, message = "Quiz attempt saved successfully" });
            }
            catch (Exception ex){
                _logger.LogError($"Error saving quiz attempt: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/current/statistics")]
        [Authorize]
        public async Task<ActionResult<UserStatisticsDto>> GetCurrentUserStatistics()
        {
            try{
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found" });

                var statistics = await _leaderboardService.GetUserStatisticsAsync(userId);
                return Ok(statistics);
            }
            catch (KeyNotFoundException ex){
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex){
                _logger.LogError($"Error getting current user statistics: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/current/position")]
        [Authorize]
        public async Task<ActionResult<object>> GetCurrentUserLeaderboardPosition()
        {
            try{
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User ID not found" });

                var position = await _leaderboardService.GetUserLeaderboardPositionAsync(userId);
                if (position == -1)
                    return Ok(new { position = 0, message = "User has no attempts yet" });
                return Ok(new { position });
            }
            catch (Exception ex){
                _logger.LogError($"Error getting current user leaderboard position: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    public class SaveQuizAttemptRequest
    {
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int Duration { get; set; } // в секундах
        public int Rating { get; set; } = 0; // 1-5 
    }
}
