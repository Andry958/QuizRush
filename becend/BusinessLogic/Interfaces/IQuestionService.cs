using BusinessLogic.DTOs;

namespace BusinessLogic.Interfaces
{
    public interface IQuestionService
    {
        Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync();
        Task<QuestionDto?> GetQuestionByIdAsync(int id);
        Task<QuestionDto> CreateQuestionAsync(QuestionDto questionDto);
        Task UpdateQuestionAsync(int id, QuestionDto questionDto);
        Task DeleteQuestionAsync(int id);
        Task<IEnumerable<QuestionDto>> GetQuestionsByQuizIdAsync(int quizId);
    }
}
