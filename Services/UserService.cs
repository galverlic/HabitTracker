using HabitTracker.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Services
{
    public class UserService : IUserService
    {
        private readonly SQLiteAsyncConnection _db;

        public UserService()
        {
            // Use the DatabasePath from the Constants class
            _db = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            _db.CreateTableAsync<User>().Wait();
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _db.Table<User>().ToListAsync();
        }

        public async Task<bool> CreateUser(string email, string password)
        {
            try
            {
                // Check if user already exists
                var existingUserCount = await _db.Table<User>().Where(u => u.Email == email).CountAsync();
                if (existingUserCount > 0)
                {
                    Console.WriteLine("User already exists.");
                    return false;
                }

                // Hash password
                var hashedPassword = HashPassword(password);

                var newUser = new User
                {
                    Email = email,
                    Password = hashedPassword,
                    Name = "" // Consider how you want to handle names
                };

                await _db.InsertAsync(newUser);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during user creation: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LogIn(string email, string password)
        {
            try
            {
                // Query to find the user by email
                // Assuming QueryAsync is the correct method to asynchronously query the database
                var users = await _db.QueryAsync<User>("SELECT * FROM User WHERE Email = ?", new object[] { email });

                var user = users.FirstOrDefault(); // This assumes that email is unique in your User table
                if (user == null)
                {
                    Console.WriteLine("User not found.");
                    return false;
                }

                // Verify password - Ensure this method checks hashed passwords securely
                if (VerifyPassword(password, user.Password))
                {
                    Console.WriteLine("Login successful.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Incorrect password.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during login: {ex.Message}");
                return false;
            }
        }


        // Helper method for hashing passwords
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // Helper method for verifying passwords
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var enteredHash = HashPassword(enteredPassword);
            return enteredHash == storedHash;
        }
    }
}
