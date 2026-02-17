using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using DataAccess.Data;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly QuizRushContext _context;

        public QuestionService(QuizRushContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync()
        {
            var questions = await _context.Questions.ToListAsync();
            return questions.Select(MapToDto);
        }

        public async Task<QuestionDto?> GetQuestionByIdAsync(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            return question == null ? null : MapToDto(question);
        }

        public async Task<QuestionDto> CreateQuestionAsync(QuestionDto questionDto)
        {
            var question = new Question
            {
                Text = questionDto.Text,
                QuizId = questionDto.QuizId,
                TimeLimit = questionDto.TimeLimit
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return MapToDto(question);
        }

        public async Task UpdateQuestionAsync(int id, QuestionDto questionDto)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                throw new KeyNotFoundException($"Question with id {id} not found.");
            }

            question.Text = questionDto.Text;
            question.TimeLimit = questionDto.TimeLimit;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuestionAsync(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByQuizIdAsync(int quizId)
        {
            var questions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            return questions.Select(MapToDto);
        }

        private static QuestionDto MapToDto(Question question)
        {
            return new QuestionDto
            {
                Id = question.Id,
                Text = question.Text,
                QuizId = question.QuizId,
                TimeLimit = question.TimeLimit
            };
            
        }
    }
}
