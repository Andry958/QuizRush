using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using DataAccess.Data;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services
{
    public class AnswerService : IAnswerService
    {
        private readonly QuizRushContext _context;

        public AnswerService(QuizRushContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnswerDto>> GetAllAnswersAsync()
        {
            var answers = await _context.Answers.ToListAsync();
            return answers.Select(MapToDto);
        }

        public async Task<AnswerDto?> GetAnswerByIdAsync(int id)
        {
            var answer = await _context.Answers.FindAsync(id);
            return answer == null ? null : MapToDto(answer);
        }

        public async Task<AnswerDto> CreateAnswerAsync(AnswerDto answerDto)
        {
            var answer = new Answer
            {
                Text = answerDto.Text,
                IsCorrect = answerDto.IsCorrect,
                QuestionId = answerDto.QuestionId
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return MapToDto(answer);
        }

        public async Task UpdateAnswerAsync(int id, AnswerDto answerDto)
        {
            var answer = await _context.Answers.FindAsync(id);
            if (answer == null)
            {
                throw new KeyNotFoundException($"Answer with id {id} not found.");
            }

            answer.Text = answerDto.Text;
            answer.IsCorrect = answerDto.IsCorrect;

            await _context.SaveChangesAsync();
        }


        public async Task DeleteAnswerAsync(int id)
        {
            var answer = await _context.Answers.FindAsync(id);
            if (answer != null)
            {
                _context.Answers.Remove(answer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<AnswerDto>> GetAnswersByQuestionIdAsync(int questionId)
        {
            var answers = await _context.Answers
                .Where(a => a.QuestionId == questionId)
                .ToListAsync();

            return answers.Select(MapToDto);
        }


        private static AnswerDto MapToDto(Answer answer)
        {
            return new AnswerDto
            {
                Id = answer.Id,
                Text = answer.Text,
                IsCorrect = answer.IsCorrect,
                QuestionId = answer.QuestionId
            };
        }
        
    }
}
