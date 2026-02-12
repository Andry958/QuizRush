namespace DataAccess.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        public int TimeLimit { get; set; } // секунди
        public List<Answer> Answers { get; set; } = new();
    }
}
