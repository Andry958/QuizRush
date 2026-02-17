using BusinessLogic.DTOs;

namespace BusinessLogic.Interfaces
{
    public interface IQuizService
    {
        Task<IEnumerable<QuizDto>> GetAllQuizzesAsync();
        Task<QuizDto?> GetQuizByIdAsync(int id);
        Task<QuizDto> CreateQuizAsync(QuizDto quizDto);
        Task UpdateQuizAsync(int id, QuizDto quizDto);
        Task DeleteQuizAsync(int id);
    }
}
