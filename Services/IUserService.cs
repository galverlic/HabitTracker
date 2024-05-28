using HabitTracker.Models;

namespace HabitTracker.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetUsers();
        Task<Guid> GetCurrentUserId();
        Task<User> GetUserById(Guid userId);
        Task<bool> CreateUser(string name, string email, string password);
        Task<bool> LogIn(string email, string password);
        Task<bool> LogInWithGoogle();

        Task LogOut();
        //Task DeleteUser(int userId);
        //Task UpdateUser(User user);
    }
}
