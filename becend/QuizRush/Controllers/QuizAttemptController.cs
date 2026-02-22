using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace QuizRushAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameSessionController : ControllerBase
    {
        private readonly IQuizSessionGameService _gameService;

        public GameSessionController(IQuizSessionGameService gameService)
        {
            _gameService = gameService;
        }


        // створення сесії. потрібен зарежстрований юзер для створення.
        [HttpPost("create")] 
        public async Task<IActionResult> CreateSession([FromQuery] string userId, [FromQuery] int quizId, [FromQuery] string? pinCode, [FromQuery] string? title, [FromQuery] string description)
        {
            var session = await _gameService.CreateGameSessionAsync(userId, quizId, pinCode, title, description);
            return Ok(session);
        }

        [HttpPost("{sessionId}/join")] // приєднання до сесії. потрібно просто ввести нік нейм і айді сесії для приєднання
        public async Task<IActionResult> JoinSession(int sessionId, [FromQuery] string nickName)
        {
            var playerId = await _gameService.JoinGameSessionAsync(sessionId, nickName);
            return Ok(new { PlayerId = playerId });
        }

        [HttpPost("{sessionId}/start")] // початок сесії. тільки сервер може почати сесію, після цього всі гравці отримують перше питання
        public async Task<IActionResult> StartSession(int sessionId)
        {
            await _gameService.StartGameSessionAsync(sessionId);
            return Ok();
        }

        [HttpPost("{sessionId}/answer")] // надсилання відповіді на поточне питання.
                                         // гравець надсилає свою відповідь, вказуючи айді сесії, свій айді та айді вибраної відповіді
        public async Task<IActionResult> SubmitAnswer(int sessionId, int playerId, int answerId)
        {
            await _gameService.SubmitAnswerAsync(sessionId, playerId, answerId);
            return Ok();
        }

        // отримати поточне питання
        [HttpGet("{sessionId}/current")]
        public async Task<IActionResult> GetCurrentQuestion(int sessionId)
        {
            var question = await _gameService.GetCurrentQuestionAsync(sessionId);
            return Ok(question);
        }

        // перейти на інше питання. після переходження застосувати GetCurrentQuestion
        [HttpPost("{sessionId}/next")]
        public async Task<IActionResult> NextQuestion(int sessionId)
        {
            await _gameService.NextQuestionAsync(sessionId);
            return Ok();
        }

        // закінчення сесії гри і отримання фінальних результатів
        [HttpPost("{sessionId}/end")]
        public async Task<IActionResult> EndSession(int sessionId)
        {
            var leaderboard = await _gameService.EndGameSessionAsync(sessionId);
            return Ok(leaderboard);
        }
        // отримати результати конкретного гравця по сесії
        [HttpGet("{sessionId}/results/{playerId}")]
        public async Task<IActionResult> GetPlayerResults(int sessionId, int playerId)
        {
            var results = await _gameService.GetPlayerResultsAsync(sessionId, playerId);
            return Ok(results);
        }
        // отримати лідерборд по сесії
        [HttpGet("{sessionId}/leaderboard")]
        public async Task<IActionResult> GetLeaderboard(int sessionId)
        {
            var leaderboard = await _gameService.GetLeaderboardAsync(sessionId);
            return Ok(leaderboard);
        }
        // отримати результати всіх гравців по сесії
        [HttpGet("{sessionId}/results")]
        public async Task<IActionResult> GetAllPlayersResults(int sessionId)
        {
            var results = await _gameService.GetAllPlayersResultsAsync(sessionId);
            return Ok(results);
        }
        // отримати всі сесії гри
        [HttpGet("sessions")]
        public async Task<IActionResult> GetAllGameSessions()
        {
            var sessions = await _gameService.GetAllGameSessionsAsync();
            return Ok(sessions);

        }
        // видалити сесію гри
        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeleteGameSession(int sessionId)
        {
            await _gameService.DeleteGameSessionAsync(sessionId);
            return NoContent();
        }
    }
}