using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using DataAccess.Data;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services
{
    public class QuizSessionService : IQuizService
    {
        private readonly QuizRushContext _context;

        public QuizSessionService(QuizRushContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QuizDto>> GetAllQuizzesAsync()
        {
            var quizzes = await _context.Quizzes.ToListAsync();
            return quizzes.Select(MapToDto);
        }

        public async Task<QuizDto?> GetQuizByIdAsync(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            return quiz == null ? null : MapToDto(quiz);
        }

        public async Task<QuizDto> CreateQuizAsync(QuizDto quizDto)
        {
            var quiz = new Quiz
            {
                Title = quizDto.Title,
                Description = quizDto.Description,
                CreatedById = quizDto.CreatedById
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return MapToDto(quiz);
        }

        public async Task UpdateQuizAsync(int id, QuizDto quizDto)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                throw new KeyNotFoundException($"Quiz with id {id} not found.");
            }

            quiz.Title = quizDto.Title;
            quiz.Description = quizDto.Description;
            
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuizAsync(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }

        private static QuizDto MapToDto(Quiz quiz)
        {
            return new QuizDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CreatedById = quiz.CreatedById
            };
        }
    }
}
