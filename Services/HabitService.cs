﻿using HabitTracker.Models;
using Newtonsoft.Json;
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
            Debug.WriteLine($"Inserting Habit: {JsonConvert.SerializeObject(habit)}");
            await client.From<Habit>().Insert(habit);
        }

        public async Task DeleteHabit(Guid habitId)
        {
            await client.From<Habit>().Where(h => h.HabitId == habitId).Delete();
        }

        public async Task UpdateHabit(Habit habit)
        {
            var updateQuery = client.From<Habit>().Where(h => h.HabitId == habit.HabitId);

            if (habit.Name != null)
                updateQuery = updateQuery.Set(h => h.Name, habit.Name);

            if (habit.Description != null)
                updateQuery = updateQuery.Set(h => h.Description, habit.Description);

            if (habit.FrequencyValue >= 0)
                updateQuery = updateQuery.Set(h => h.FrequencyValue, habit.FrequencyValue);

            if (habit.CurrentRepetition >= 0)
                updateQuery = updateQuery.Set(h => h.CurrentRepetition, habit.CurrentRepetition);

            if (habit.TargetRepetition != 0)
                updateQuery = updateQuery.Set(h => h.TargetRepetition, habit.TargetRepetition);

            if (habit.StartDate != default(DateTime))
                updateQuery = updateQuery.Set(h => h.StartDate, habit.StartDate);

            updateQuery = updateQuery.Set(h => h.IsCompleted, habit.IsCompleted);

            if (habit.Streak >= 0)
                updateQuery = updateQuery.Set(h => h.Streak, habit.Streak);

            await updateQuery.Update();
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

            // Fetch all habits for the user that start on or before the given date
            var habitResponse = await client.From<Habit>()
                                            .Select("*")
                                            .Filter("user_id", Postgrest.Constants.Operator.Equals, userId.ToString())
                                            .Filter("start_date", Postgrest.Constants.Operator.LessThanOrEqual, formattedDate)
                                            .Get();
            var habits = habitResponse.Models;

            // Filter habits by frequency
            var filteredHabits = habits.Where(habit => habit.FrequencyDays.Contains(date.DayOfWeek)).ToList();

            // Fetch progress records for these habits on the specified date
            var progressResponse = await client.From<HabitProgress>()
                                               .Select("*")
                                               .Filter("date", Postgrest.Constants.Operator.Equals, formattedDate)
                                               .Get();
            var progressRecords = progressResponse.Models;

            // Match habits with their corresponding progress
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

            return joinedData;
        }

        public async Task<HabitProgress> GetHabitProgress(Guid habitId, DateTime date)
        {
            var response = await client.From<HabitProgress>()
                                       .Select("*")
                                       .Filter("habit_id", Operator.Equals, habitId.ToString())
                                       .Filter("date", Operator.Equals, date.Date.ToString("yyyy-MM-dd"))
                                       .Single();

            return response;
        }

        public async Task AddOrUpdateHabitProgress(HabitProgress progress)
        {
            var existingProgressResponse = await client.From<HabitProgress>()
                .Filter("habit_id", Operator.Equals, progress.HabitId.ToString())
                .Filter("date", Operator.Equals, progress.Date.ToString("yyyy-MM-dd"))
                .Get();

            var existingProgress = existingProgressResponse.Models.FirstOrDefault();

            if (existingProgress == null)
            {
                await client.From<HabitProgress>().Insert(progress);
            }
            else
            {
                existingProgress.CurrentRepetition = progress.CurrentRepetition;
                existingProgress.IsCompleted = progress.IsCompleted;
                await client.From<HabitProgress>().Update(existingProgress);
            }
        }


    }
}
