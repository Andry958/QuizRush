using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace DataAccess.Models
{


    public class RefreshToken : BaseEntity
    {
        public int Id { get; set; }   // 🔥 int, не string

        public string Token { get; set; }

        public DateTime Expires { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        // ВАЖЛИВО
        public string UserId { get; set; }   // 🔥 string, не int

        public User User { get; set; }
    }


}
