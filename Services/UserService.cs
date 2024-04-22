using Postgrest.Exceptions;
using Supabase.Gotrue;
using System.Web;
using static Supabase.Gotrue.Constants;

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
            var authorizationUrl = new Uri("https://efchmeypnivdcbcxxwow.supabase.co/auth/v1/authorize?provider=google");
            var callbackUrl = new Uri("myapp://callback");

            try
            {
                var authResult = await WebAuthenticator.AuthenticateAsync(authorizationUrl, callbackUrl);

                if (string.IsNullOrWhiteSpace(authResult?.AccessToken))
                {
                    Console.WriteLine("Authentication failed: No access token received.");
                    return false;
                }

                if (!authResult.ExpiresIn.HasValue)
                {
                    Console.WriteLine("Authentication failed: Expiration time is missing.");
                    return false;
                }

                // Calculate seconds until the token expires
                int expiresInSeconds = (int)(authResult.ExpiresIn.Value - DateTimeOffset.UtcNow).TotalSeconds;

                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString.Add("access_token", authResult.AccessToken);
                queryString.Add("expires_in", expiresInSeconds.ToString());
                queryString.Add("refresh_token", authResult.RefreshToken);
                queryString.Add("token_type", "bearer");

                string fullUrl = $"https://efchmeypnivdcbcxxwow.supabase.co/auth/v1/token?{queryString}";
                var session = await client.Auth.GetSessionFromUrl(new Uri(fullUrl));

                if (session != null && session.User != null)
                {
                    return await AddOrUpdateUser(session.User);

                    Console.WriteLine("Google authentication successful.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Google authentication failed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google authentication exception: {ex.Message}");
                return false;
            }
        }


        private async Task<bool> AddOrUpdateUser(User user)
        {
            var newUser = new Models.User
            {
                UserId = user.Id,
                Name = user.UserMetadata["full_name"] as string ?? "N/A",  // Assuming 'full_name' is a field in the UserMetadata
                Email = user.Email
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




            public async Task LogOut()
        {
            await client.Auth.SignOut();  // SignOut() does not return an object with an error property.
        }
    }
}
