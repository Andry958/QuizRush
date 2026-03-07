namespace BusinessLogic.DTOs
{
    /// <summary>
    /// DTO для отримання статистики користувача
    /// </summary>
    public class UserStatisticsDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // Загальна статистика
        public int TotalAttempts { get; set; }
        public double AverageScore { get; set; }
        public int TotalCorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double AccuracyPercentage { get; set; } // Відсоток правильних відповідей
        public int TotalDuration { get; set; } // Загальний час в секундах
        
        // Рейтинги
        public double AverageRating { get; set; } // Середній рейтинг квізів (1-5)
        
        // Дати
        public DateTime FirstAttemptAt { get; set; }
        public DateTime LastAttemptAt { get; set; }
        
        // Історія спроб
        public List<AttemptHistoryDto> AttemptHistory { get; set; } = new();
    }
}
