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
        Task<IEnumerable<User>> GetUser();
        Task CreateUser(User user);
        //Task DeleteUser(int userId);
        //Task UpdateUser(User user);
    }
}
