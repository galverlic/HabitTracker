using HabitTracker.Models;
using Postgrest.Exceptions;

namespace HabitTracker.Services
{
    public class UserService : IUserService
    {
        private readonly Supabase.Client client;

        public UserService(Supabase.Client client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var response = await client.From<User>().Select("*").Get();
            return response.Models;
        }

        public async Task<User> GetUserById(Guid userId)
        {
            var response = await client.From<User>()
                                       .Select("*")
                                       .Filter("user_id", Postgrest.Constants.Operator.Equals, userId.ToString())
                                       .Single();
            return response;
        }

        public async Task<Guid> GetCurrentUserId()
        {
            var currentUser = client.Auth.CurrentUser; // Make sure 'client' is your Supabase client instance
            return currentUser != null ? Guid.Parse(currentUser.Id) : Guid.Empty;
        }


        public async Task<bool> CreateUser(string name, string email, string password)
        {
            var signUpResponse = await client.Auth.SignUp(email, password);
            if (signUpResponse.User == null || string.IsNullOrEmpty(signUpResponse.User.Id))
            {
                Console.WriteLine("Failed to sign up user.");
                return false;
            }

            var newUser = new User
            {
                UserId = signUpResponse.User.Id,
                Name = name,
                Email = signUpResponse.User.Email
            };

            try
            {
                var insertResponse = await client.From<User>().Insert(newUser);
                Console.WriteLine("Insert successful.");
                return true;
            }
            catch (PostgrestException ex)
            {
                Console.WriteLine($"PostgrestException during insert: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.ToString()))
                {
                    Console.WriteLine($"Error details: {ex.ToString()}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General exception during insert: {ex.Message}");
                return false;
            }
        }






        public async Task<bool> LogIn(string email, string password)
        {
            var signInResponse = await client.Auth.SignIn(email, password);
            return signInResponse.User != null;  // Check if the User object is not null.
        }

        public async Task LogOut()
        {
            await client.Auth.SignOut();  // SignOut() does not return an object with an error property.
        }
    }
}
