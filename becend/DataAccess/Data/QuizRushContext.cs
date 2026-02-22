using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;

namespace DataAccess.Data
{
    public class QuizRushContext : IdentityDbContext<User>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AnsweredQuestion> AnsweredQuestions { get; set; }

        public QuizRushContext(DbContextOptions<QuizRushContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User -> Quiz (One to Many)
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.CreatedBy)
                .WithMany(u => u.CreatedQuizzes)
                .HasForeignKey(q => q.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz -> Question (One to Many)
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(quiz => quiz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question -> Answer (One to Many)
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz -> GameSession (One to Many)
            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.Quiz)
                .WithMany(q => q.GameSessions)
                .HasForeignKey(gs => gs.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> GameSession (One to Many)
            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.CreatedBy)
                .WithMany(u => u.GameSessions)
                .HasForeignKey(gs => gs.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);

            // GameSession -> Player (One to Many)
            modelBuilder.Entity<Player>()
                .HasOne(p => p.GameSession)
                .WithMany(gs => gs.Players)
                .HasForeignKey(p => p.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
