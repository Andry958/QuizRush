using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace QuizRush.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerService _answerService;

        public AnswersController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnswerDto>>> GetAllAnswers()
        {
            var answers = await _answerService.GetAllAnswersAsync();
            return Ok(answers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AnswerDto>> GetAnswer(int id)
        {
            var answer = await _answerService.GetAnswerByIdAsync(id);
            if (answer == null)
            {
                return NotFound();
            }
            return Ok(answer);
        }

        [HttpGet("question/{questionId}")]
        public async Task<ActionResult<IEnumerable<AnswerDto>>> GetAnswersByQuestion(int questionId)
        {
            var answers = await _answerService.GetAnswersByQuestionIdAsync(questionId);
            return Ok(answers);
        }

        [HttpPost]
        public async Task<ActionResult<AnswerDto>> CreateAnswer(AnswerDto answerDto)
        {
            var answer = await _answerService.CreateAnswerAsync(answerDto);
            return CreatedAtAction(nameof(GetAnswer), new { id = answer.Id }, answer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnswer(int id, AnswerDto answerDto)
        {
            if (id != answerDto.Id && answerDto.Id != 0)
            {
                return BadRequest("Id in URL and body must match (or body Id can be 0)");
            }

            try
            {
                await _answerService.UpdateAnswerAsync(id, answerDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            await _answerService.DeleteAnswerAsync(id);
            return NoContent();
        }
    }
}
