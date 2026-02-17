using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Text { get; set; } = string.Empty;

        public int QuizId { get; set; }

        public int TimeLimit { get; set; }
    }
}
