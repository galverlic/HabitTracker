using Postgrest.Attributes;
using Postgrest.Models;

namespace HabitTracker.Models
{
    [Table("Users")]
    public class User : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }
    }
}
