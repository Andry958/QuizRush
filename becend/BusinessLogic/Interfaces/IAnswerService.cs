using BusinessLogic.DTOs;

namespace BusinessLogic.Interfaces
{
    public interface IAnswerService
    {
        Task<IEnumerable<AnswerDto>> GetAllAnswersAsync();
        Task<AnswerDto?> GetAnswerByIdAsync(int id);
        Task<AnswerDto> CreateAnswerAsync(AnswerDto answerDto);
        Task UpdateAnswerAsync(int id, AnswerDto answerDto);
        Task DeleteAnswerAsync(int id);
        Task<IEnumerable<AnswerDto>> GetAnswersByQuestionIdAsync(int questionId);
    }
}
