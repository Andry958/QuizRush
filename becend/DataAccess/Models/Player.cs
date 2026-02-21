namespace DataAccess.Models
{
    public class Player : BaseEntity
    {
        public int Id { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public int GameSessionId { get; set; }
        public GameSession GameSession { get; set; }

        public int Score { get; set; }
    }
}
