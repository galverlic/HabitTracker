using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HabitTracker.Models;
using Supabase;

namespace HabitTracker.Services
{
    public class UserService : IUserService
    {
        private readonly Client _supabaseClient;

        public UserService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;

        }

        public async Task<IEnumerable<User>> GetUser()
        {
            var response = await _supabaseClient.From<User>().Get();
            return response.Models;
        }

        public async Task CreateUser(User user)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _supabaseClient.From<User>().Insert(user);

        }
        
        // DELETE USER

        // UPDATE USER
    }
}
