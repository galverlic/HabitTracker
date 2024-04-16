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
        private static readonly string CurrentUserIdKey = "current_user_id";  // Good use of a constant for the key

        public UserService()
        {
            _db = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            _db.CreateTableAsync<User>().Wait();  // Consider handling this asynchronously
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _db.Table<User>().ToListAsync();
        }

        public async Task<User> GetUserById(Guid userId)
        {
            var user = await _db.Table<User>().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");  // Throwing if user not found is good practice
            return user;
        }

        public async Task<Guid> GetCurrentUserId()
        {
            var userIdString = await SecureStorage.GetAsync("current_user_id");
            if (!string.IsNullOrEmpty(userIdString))
            {
                return Guid.Parse(userIdString);
            }
            throw new InvalidOperationException("No user is currently logged in.");
        }

        public async Task SetCurrentUserId(Guid userId)
        {
            await SecureStorage.SetAsync("current_user_id", userId.ToString());
        }

        public async Task<bool> CreateUser(string name, string email, string password)
        {
            var existingUser = await _db.Table<User>().FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                Console.WriteLine("User already exists.");
                return false;  // Early exit if user exists
            }

            var hashedPassword = HashPassword(password);
            var newUser = new User
            {
                Name = name,
                Email = email,
                Password = hashedPassword
                 // You might want to manage names differently
            };

            await _db.InsertAsync(newUser);
            return true;
        }

        public async Task<bool> LogIn(string email, string password)
        {
            var user = await _db.Table<User>().FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && VerifyPassword(password, user.Password))
            {
                await SetCurrentUserId(user.UserId);  // Correctly sets the current user ID upon login
                return true;
            }
            return false;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return HashPassword(enteredPassword) == storedHash;
        }

        public async Task LogOut()
        {
            await SecureStorage.SetAsync(CurrentUserIdKey, string.Empty);  // Correctly clears the stored user ID upon logout
        }
    }
}
