using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace HabitTracker.Models
{
    [Table("habit_progress")]
    public class HabitProgress : BaseModel
    {
        [PrimaryKey("progress_id", shouldInsert: true)]
        public Guid ProgressId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("current_repetition")]
        public int CurrentRepetition { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; }

        [Column("habit_id")]
        public Guid HabitId { get; set; }
    }
}
