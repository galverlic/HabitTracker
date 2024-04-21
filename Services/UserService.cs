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

        public async Task<IEnumerable<Models.User>> GetUsers()
        {
            var response = await client.From<Models.User>().Select("*").Get();
            return response.Models;
        }

        public async Task<Models.User> GetUserById(Guid userId)
        {
            var response = await client.From<Models.User>()
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

            var newUser = new Models.User
            {
                UserId = signUpResponse.User.Id,
                Name = name,
                Email = signUpResponse.User.Email
            };

            try
            {
                var insertResponse = await client.From<Models.User>().Insert(newUser);
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

        public async Task<bool> LogInWithGoogle()
        {
            var authUrl = new Uri("https://your-supabase-project.supabase.co/auth/v1/authorize?provider=google");
            var callbackUrl = new Uri("habittrackerdatascheme://callback");

            try
            {
                var authResult = await WebAuthenticator.AuthenticateAsync(
                    new WebAuthenticatorOptions
                    {
                        Url = authUrl,
                        CallbackUrl = callbackUrl
                    });

                // Log all available details from the auth result for debugging
                Console.WriteLine($"Authentication result - Access Token: {authResult.AccessToken}");
                Console.WriteLine($"ID Token: {authResult.IdToken}");
                Console.WriteLine($"Refresh Token: {authResult.RefreshToken}");
                Console.WriteLine($"Expires In: {authResult.ExpiresIn}");
                Console.WriteLine("Properties:");
                foreach (var prop in authResult.Properties)
                {
                    Console.WriteLine($"  {prop.Key}: {prop.Value}");
                }

                if (!string.IsNullOrEmpty(authResult.AccessToken))
                {
                    Console.WriteLine("Authentication successful.");
                    // Optionally, save the access token in secure storage
                    await SecureStorage.SetAsync("oauth_token", authResult.AccessToken);
                    return true;  // Authentication successful
                }
                else
                {
                    Console.WriteLine("Authentication failed: No access token received.");
                    return false;  // Authentication failed, no token received
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication failed with an exception: {ex.Message}");
                return false;  // Authentication failed
            }
        }






        public async Task LogOut()
        {
            await client.Auth.SignOut();  // SignOut() does not return an object with an error property.
        }
    }
}
