﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HabitTracker.Models;

namespace HabitTracker.ViewModels
{
    public partial class AddUserViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private string _userName;
        [ObservableProperty]
        private string _userEmail;
        [ObservableProperty]
        private string _userPassword;
        [ObservableProperty]
        private string confirmPassword;
        [ObservableProperty]
        private string feedbackMessage;


        public AddUserViewModel(IUserService userService)
        {
            _userService = userService;
        }

        [RelayCommand]
        private async Task AddUser()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserEmail) || string.IsNullOrWhiteSpace(UserPassword))
            {
                FeedbackMessage = "Please fill out all fields!";
                return;
            }

            if (UserPassword != ConfirmPassword)
            {
                FeedbackMessage = "Passwords do not match.";
                return;
            }

            try
            {
                User newUser = new User
                {
                    Name = UserName,
                    Email = UserEmail,
                    Password = UserPassword
                };

                await _userService.CreateUser(UserEmail, UserPassword);
                FeedbackMessage = "User created successfully.";
                await Shell.Current.GoToAsync("HabitsListingPage");
            }
            catch (Exception ex)
            {
                FeedbackMessage = $"Error: {ex.Message}";
            }
        }


    }
}