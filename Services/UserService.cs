using HabitTracker.Models;
using Supabase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

            Console.WriteLine($"SignUp response: {signUpResponse.User?.Id}");


            // Check if the user object is not null, which indicates the user was created successfully.
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Name = name,
                Email = signUpResponse.User.Email,
                // Set other fields as needed
            };
            Console.WriteLine($"[Before Insert] UserID: {newUser.UserId}");

            try
            {
                var insertResponse = await client.From<User>().Insert(newUser).ConfigureAwait(false);

                Console.WriteLine($"[After Insert] UserID: {newUser.UserId}");
                Console.WriteLine($"Insert successful: {insertResponse}");

                return insertResponse.ResponseMessage == null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during insert: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
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
