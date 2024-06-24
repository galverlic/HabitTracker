using HabitTracker.Models;
using Newtonsoft.Json;
using Postgrest.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Postgrest.Constants;

namespace HabitTracker.Services
{
    public class HabitService : IHabitService
    {
        private readonly Supabase.Client client;

        public HabitService(Supabase.Client client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<Habit>> GetHabitsForToday(Guid userId)
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            var allHabits = await GetHabitsByUserId(userId);
            return allHabits.Where(habit => habit.Frequency.HasFlag(Habit.ConvertDayToHabitFrequency(today)));
        }

        public async Task AddHabitToUser(Habit habit, Guid userId)
        {
            habit.UserId = userId;
            await client.From<Habit>().Insert(new List<Habit> { habit });
        }

        public async Task<IEnumerable<Habit>> GetHabitsByUserId(Guid userId)
        {
            var response = await client.From<Habit>()
                                       .Select("*")
                                       .Filter("user_id", Operator.Equals, userId.ToString())
                                       .Get();

            return response.Models;
        }

        public async Task CreateHabit(Habit habit)
        {
            await client.From<Habit>().Insert(habit);
        }

        public async Task DeleteHabit(Guid habitId)
        {
            try
            {
                await client.From<Habit>().Where(h => h.HabitId == habitId).Delete();
                Debug.WriteLine($"Habit with ID {habitId} deleted successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting habit with ID {habitId}: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateHabit(Habit habit)
        {
            var updateQuery = client.From<Habit>().Where(h => h.HabitId == habit.HabitId);

            if (!string.IsNullOrWhiteSpace(habit.Name))
                updateQuery = updateQuery.Set(h => h.Name, habit.Name);

            if (!string.IsNullOrWhiteSpace(habit.Description))
                updateQuery = updateQuery.Set(h => h.Description, habit.Description);

            updateQuery = updateQuery.Set(h => h.FrequencyValue, habit.FrequencyValue);

            if (habit.CurrentRepetition >= 0)
                updateQuery = updateQuery.Set(h => h.CurrentRepetition, habit.CurrentRepetition);

            if (habit.TargetRepetition.HasValue && habit.TargetRepetition.Value > 0)
                updateQuery = updateQuery.Set(h => h.TargetRepetition, habit.TargetRepetition.Value);

            if (habit.StartDate != default(DateTime))
                updateQuery = updateQuery.Set(h => h.StartDate, habit.StartDate);

            updateQuery = updateQuery.Set(h => h.IsCompleted, habit.IsCompleted);

            if (habit.Streak >= 0)
                updateQuery = updateQuery.Set(h => h.Streak, habit.Streak);

            var response = await updateQuery.Update();
        }

        public async Task<List<HabitProgress>> GetProgressForHabitAsync(Guid habitId, DateTime date)
        {
            var response = await client.From<HabitProgress>()
                                       .Select("*")
                                       .Filter("habit_id", Operator.Equals, habitId.ToString())
                                       .Filter("date", Operator.Equals, date.Date.ToString("yyyy-MM-dd"))
                                       .Get();
            return response.Models;
        }

        public async Task AddProgressAsync(HabitProgress progress)
        {
            await client.From<HabitProgress>().Insert(progress);
        }
        public async Task<int> CalculateStreak(Guid habitId)
        {
            var progressRecords = await client.From<HabitProgress>()
                                              .Select("*")
                                              .Filter("habit_id", Postgrest.Constants.Operator.Equals, habitId.ToString())
                                              .Order("date", Postgrest.Constants.Ordering.Descending)
                                              .Get();

            int streak = 0;
            DateTime previousDate = DateTime.Today;

            foreach (var progress in progressRecords.Models)
            {
                if (progress.Date == previousDate || progress.Date == previousDate.AddDays(-1))
                {
                    streak++;
                    previousDate = progress.Date;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        public async Task ResetDailyHabits(Guid userId)
        {
            try
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);
                var habits = await GetHabitsByUserId(userId);

                foreach (var habit in habits)
                {
                    var progressYesterday = await GetHabitProgress(habit.HabitId, yesterday);
                    bool wasCompletedYesterday = progressYesterday?.IsCompleted ?? false;

                    if (habit.Frequency.HasFlag(Habit.ConvertDayToHabitFrequency(today.DayOfWeek)))
                    {
                        if (wasCompletedYesterday)
                        {
                            habit.Streak++;
                        }
                        else if (habit.Frequency.HasFlag(Habit.ConvertDayToHabitFrequency(yesterday.DayOfWeek)))
                        {
                            habit.Streak = 0;
                        }

                        var newProgress = new HabitProgress
                        {
                            HabitId = habit.HabitId,
                            Date = today,
                            CurrentRepetition = 0,
                            IsCompleted = false
                        };
                        await AddOrUpdateHabitProgress(newProgress);
                    }
                    else if (wasCompletedYesterday)
                    {
                        habit.Streak++;
                    }

                    await UpdateHabit(habit);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to reset daily habits: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Habit>> GetHabitsByDateAndUserId(DateTime date, Guid userId)
        {
            var formattedDate = date.Date.ToString("yyyy-MM-dd");

            Debug.WriteLine($"Fetching habits for user {userId} on or before {formattedDate}");

            var habitResponse = await client.From<Habit>()
                                            .Select("*")
                                            .Filter("user_id", Operator.Equals, userId.ToString())
                                            .Filter("start_date", Operator.LessThanOrEqual, formattedDate)
                                            .Get();

            var habits = habitResponse.Models;
            Debug.WriteLine($"Retrieved {habits.Count} habits for user {userId} on or before {formattedDate}");

            var filteredHabits = habits.Where(habit => habit.FrequencyDays.Contains(date.DayOfWeek)).ToList();
            Debug.WriteLine($"Filtered {filteredHabits.Count} habits for the current day of week: {date.DayOfWeek}");

            var progressResponse = await client.From<HabitProgress>()
                                               .Select("*")
                                               .Filter("date", Operator.Equals, formattedDate)
                                               .Get();

            var progressRecords = progressResponse.Models;
            Debug.WriteLine($"Retrieved {progressRecords.Count} progress records for date {formattedDate}");

            var joinedData = filteredHabits
                .GroupJoin(progressRecords,
                           habit => habit.HabitId,
                           progress => progress.HabitId,
                           (habit, progresses) => new { Habit = habit, Progresses = progresses })
                .SelectMany(joined => joined.Progresses.DefaultIfEmpty(),
                            (joined, progress) =>
                            {
                                joined.Habit.CurrentRepetition = progress?.CurrentRepetition ?? 0;
                                joined.Habit.IsCompleted = progress?.IsCompleted ?? false;
                                return joined.Habit;
                            })
                .ToList();

            Debug.WriteLine($"Joined {joinedData.Count} habits with progress records");

            return joinedData;
        }



        public async Task<HabitProgress> GetHabitProgress(Guid habitId, DateTime date)
        {
            var formattedDate = date.Date.ToString("yyyy-MM-dd");
            Debug.WriteLine($"Querying habit progress for habitId: {habitId} on date: {formattedDate}");

            try
            {
                var response = await client.From<HabitProgress>()
                                           .Select("*")
                                           .Filter("habit_id", Operator.Equals, habitId.ToString())
                                           .Filter("date", Operator.Equals, formattedDate)
                                           .Single();

                if (response == null)
                {
                    Debug.WriteLine($"[ERROR] No habit progress found for habitId: {habitId} on date: {formattedDate}");
                }
                else
                {
                    Debug.WriteLine($"[SUCCESS] Retrieved habit progress: {response.ProgressId}");
                }

                return response;
            }
            catch (PostgrestException pgEx)
            {
                Debug.WriteLine($"[POSTGREST EXCEPTION] Error querying habit progress: {pgEx.Message}");
                if (pgEx.Response.Content != null)
                {
                    Debug.WriteLine($"Response Content: {await pgEx.Response.Content.ReadAsStringAsync()}");
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GENERAL EXCEPTION] Error querying habit progress: {ex.Message}");
                return null;
            }
        }





        public async Task AddOrUpdateHabitProgress(HabitProgress progress)
        {
            try
            {
                // Ensure the date is in UTC and set correctly
                if (progress.Date.Kind == DateTimeKind.Unspecified)
                {
                    progress.Date = DateTime.SpecifyKind(progress.Date, DateTimeKind.Utc);
                }
                //else
                //{
                //    progress.Date = progress.Date.ToUniversalTime();
                //}

                var dateFormatted = progress.Date.ToString("yyyy-MM-dd");
                Debug.WriteLine($"[START] AddOrUpdateHabitProgress for date: {dateFormatted} and habitId: {progress.HabitId}");
                Debug.WriteLine($"[DEBUG] Progress Date Kind: {progress.Date.Kind}, Date: {progress.Date}");

                // Attempt to fetch existing progress
                var existingProgressResponse = await client.From<HabitProgress>()
                    .Filter("habit_id", Operator.Equals, progress.HabitId.ToString())
                    .Filter("date", Operator.Equals, dateFormatted)
                    .Get();

                var existingProgress = existingProgressResponse.Models.FirstOrDefault();

                if (existingProgress == null)
                {
                    // No existing progress, insert new record
                    Debug.WriteLine("No existing progress found. Inserting new progress.");
                    progress.Date = DateTime.SpecifyKind(progress.Date, DateTimeKind.Utc); // Ensure it is still UTC
                    var insertResponse = await client.From<HabitProgress>().Insert(progress);
                    if (!insertResponse.Models.Any())
                    {
                        Debug.WriteLine($"[ERROR] Failed to insert new progress for habitId: {progress.HabitId} on date: {dateFormatted}");
                        return;
                    }
                    Debug.WriteLine($"[SUCCESS] Inserted new progress with ID: {insertResponse.Models.First().ProgressId}");
                }
                else
                {
                    // Existing progress found, update record
                    Debug.WriteLine($"Existing progress found: {existingProgress.ProgressId}. Updating progress.");
                    existingProgress.CurrentRepetition = progress.CurrentRepetition;
                    existingProgress.IsCompleted = progress.IsCompleted;

                    // Explicitly set existingProgress date to UTC before update
                    if (existingProgress.Date.Kind == DateTimeKind.Unspecified)
                    {
                        existingProgress.Date = DateTime.SpecifyKind(existingProgress.Date, DateTimeKind.Utc);
                    }
                    else
                    {
                        existingProgress.Date = existingProgress.Date.ToUniversalTime();
                    }

                    var matchConditions = new Dictionary<string, string>
            {
                { "habit_id", progress.HabitId.ToString() },
                { "date", dateFormatted }
            };

                    Debug.WriteLine($"[DEBUG] Before Update - Habit ID: {existingProgress.HabitId}, Date: {existingProgress.Date}, Date Kind: {existingProgress.Date.Kind}, CurrentRepetition: {existingProgress.CurrentRepetition}");
                    Debug.WriteLine($"[DEBUG] existingProgress Date before update: {existingProgress.Date}");

                    // Convert the object to JSON to inspect the data being sent
                    string jsonData = JsonConvert.SerializeObject(existingProgress);
                    Debug.WriteLine($"[DEBUG] JSON data being sent to update: {jsonData}");

                    var updateResponse = await client.From<HabitProgress>()
                                                      //.Match(matchConditions)
                                                      .Update(existingProgress);

                    Debug.WriteLine($"[DEBUG] updateResponse: {updateResponse}");

                    if (!updateResponse.Models.Any())
                    {
                        Debug.WriteLine($"[ERROR] Failed to update progress for habitId: {progress.HabitId} on date: {dateFormatted}");
                        return;
                    }

                    var updatedProgress = updateResponse.Models.First();
                    Debug.WriteLine($"[SUCCESS] Updated progress with ID: {updatedProgress.ProgressId}");
                    Debug.WriteLine($"[DEBUG] Updated progress Date: {updatedProgress.Date}, Date Kind: {updatedProgress.Date.Kind}, CurrentRepetition: {updatedProgress.CurrentRepetition}");
                }
            }
            catch (PostgrestException pgEx)
            {
                Debug.WriteLine($"AddOrUpdateHabitProgress Error: {pgEx.Message}");
                if (pgEx.Response.Content != null)
                {
                    Debug.WriteLine($"Response Content: {await pgEx.Response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddOrUpdateHabitProgress Error: {ex.Message}");
            }
        }




        public async Task DeleteHabitProgress(Guid progressId)
        {
            try
            {
                await client.From<HabitProgress>().Where(p => p.ProgressId == progressId).Delete();
                Debug.WriteLine($"Habit progress with ID {progressId} deleted successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting habit progress with ID {progressId}: {ex.Message}");
                throw;
            }
        }

    }
}
