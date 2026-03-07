namespace DataAccess.Models
{
    public class QuizAttempt : BaseEntity
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public DateTime CompletedAt { get; set; }
        public int Duration { get; set; } // in seconds (optional)
        public int Rating { get; set; } = 0; // 1-5 rating (optional)

        // Navigation properties
        public Quiz? Quiz { get; set; }
        public User? User { get; set; }
    }
}
