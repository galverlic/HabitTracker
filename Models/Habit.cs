using Postgrest.Attributes;
using Postgrest.Models;

namespace HabitTracker.Models
{
    [Table("Habits")]
    public class Habit : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("frequency")]
        public string Frequency { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("reminder_time")]
        public TimeSpan? ReminderTime { get; set; } // Assuming reminder_time can be null

        //private bool isCompleted;
        [Column("is_completed")]
        public bool IsCompleted { get; set; }


        [Column("streak")]
        public int Streak { get; set; }

        // Implement the PropertyChanged event handler


    }
}
