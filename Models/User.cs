using SQLite;


namespace HabitTracker.Models
{
    public class User
    {
        [PrimaryKey]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Password { get; set; }

        // Navigation properties are not automatically managed by SQLite-net
        // You might manage related data loading manually in your service or repository classes
    }
}
