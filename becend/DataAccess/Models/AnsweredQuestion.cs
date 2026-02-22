using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class AnsweredQuestion : BaseEntity
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }

        public bool IsCorrect { get; set; }
        public int Points { get; set; }
    }
}
