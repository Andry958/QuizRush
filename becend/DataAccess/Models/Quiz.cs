namespace DataAccess.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public User CreatedBy { get; set; }

        public List<Question> Questions { get; set; } = new();
        public List<GameSession> GameSessions { get; set; } = new();
    }
}
