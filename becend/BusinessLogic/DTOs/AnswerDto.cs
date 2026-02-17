using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs
{
    public class AnswerDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        public int QuestionId { get; set; }
    }
}
