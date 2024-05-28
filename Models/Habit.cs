using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HabitTracker.Models
{
    [Table("habits")]
    public class Habit : BaseModel
    {
        [PrimaryKey("habit_id", shouldInsert: true)]
        public Guid HabitId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("frequency")]
        public int FrequencyValue { get; set; }

        [Column("current_repetition")]
        public int CurrentRepetition { get; set; }

        [Column("target_repetition")]
        public int? TargetRepetition { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("reminder_time")]
        public TimeSpan? ReminderTime { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; }

        [Column("streak")]
        public int Streak { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public HabitFrequency Frequency
        {
            get => (HabitFrequency)FrequencyValue;
            set => FrequencyValue = (int)value;
        }

        [Flags]
        public enum HabitFrequency
        {
            None = 0,
            Monday = 1,
            Tuesday = 2,
            Wednesday = 4,
            Thursday = 8,
            Friday = 16,
            Saturday = 32,
            Sunday = 64
        }

        [JsonIgnore]
        public List<DayOfWeek> FrequencyDays
        {
            get => Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                       .Where(d => Frequency.HasFlag(ConvertDayToHabitFrequency(d))).ToList();
            set
            {
                FrequencyValue = value.Aggregate(0, (acc, day) => acc | (int)ConvertDayToHabitFrequency(day));
            }
        }

        public static HabitFrequency ConvertDayToHabitFrequency(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => HabitFrequency.Monday,
                DayOfWeek.Tuesday => HabitFrequency.Tuesday,
                DayOfWeek.Wednesday => HabitFrequency.Wednesday,
                DayOfWeek.Thursday => HabitFrequency.Thursday,
                DayOfWeek.Friday => HabitFrequency.Friday,
                DayOfWeek.Saturday => HabitFrequency.Saturday,
                DayOfWeek.Sunday => HabitFrequency.Sunday,
                _ => HabitFrequency.None,
            };
        }

        [JsonIgnore]
        public List<HabitProgress> Progress { get; set; } = new List<HabitProgress>();
    }
}
