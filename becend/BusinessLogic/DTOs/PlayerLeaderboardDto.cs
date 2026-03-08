namespace BusinessLogic.DTOs
{
    /// <summary>
    /// DTO для відображення гігаторів у лідерборді
    /// </summary>
    public class PlayerLeaderboardDto
    {
        public int Position { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public double AverageScore { get; set; }
        public int TotalCorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime LastAttemptAt { get; set; }
    }
}
