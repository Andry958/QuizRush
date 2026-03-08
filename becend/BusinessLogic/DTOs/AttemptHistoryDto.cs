namespace BusinessLogic.DTOs
{
    /// <summary>
    /// DTO для зберігання інформації про одну спробу квіза
    /// </summary>
    public class AttemptHistoryDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CompletedAt { get; set; }
        public int Duration { get; set; } // in seconds
        public int Rating { get; set; } // 1-5 rating
        public double ScorePercentage { get; set; }
    }
}
