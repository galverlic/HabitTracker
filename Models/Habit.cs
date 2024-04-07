using SQLite;

namespace HabitTracker.Models
{
    public class Habit
    {
        [PrimaryKey]
        public Guid HabitId { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public string Frequency { get; set; }

        public int CurrentRepetition { get; set; }

        public int? TargetRepetition { get; set; }

        public DateTime StartDate { get; set; }

        public TimeSpan? ReminderTime { get; set; }

        public bool IsCompleted { get; set; }

        public int Streak { get; set; }

        // For SQLite-net, simply keep the foreign key property without EF Core attributes
        public Guid UserId { get; set; }

        // SQLite-net does not automatically handle navigation properties, so they are typically not included
    }

    
}
