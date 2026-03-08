namespace BusinessLogic.DTOs
{
    /// <summary>
    /// DTO для рейлінгу квізів за популярністю
    /// </summary>
    public class QuizPopularityDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
        
        // Популярність
        public int TotalAttempts { get; set; }
        public double AverageScore { get; set; }
        public double AverageRating { get; set; } // Середній рейтинг від користувачів (1-5)
        public int TotalPlayers { get; set; } // Кількість унікальних гравців
        
        // Рейтинг популярності (більше спроб = вище рейтинг)
        public int PopularityRank { get; set; } // Позиція у списку популярних квізів
    }
}
