using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUsers();
        Task<bool> CreateUser(string email, string password);
        Task<bool> LogIn(string email, string password);
        //Task DeleteUser(int userId);
        //Task UpdateUser(User user);
    }
}
