namespace DataAccess.Models
{
    public class GameSession
    {
        public int Id { get; set; }
        public string PinCode { get; set; } = string.Empty;
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        
        public string CreatedById { get; set; } = string.Empty;
        public User CreatedBy { get; set; }

        public DateTime StartedAt { get; set; }
        public bool IsActive { get; set; }

        public List<Player> Players { get; set; } = new();
    }
}
