using Newtonsoft.Json;
using SopraOwaspKata.Dto;
using SopraOwaspKata.Model;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SopraOwaspKata.Repository
{
    public class JsonFileUserRepository : IUserRepository
    {
        private static readonly ConcurrentDictionary<string, (int AttemptCount, DateTime LastAttempt)> _lockoutInfo = new ConcurrentDictionary<string, (int, DateTime)>();
        private readonly int _lockoutThreshold = 5;
        private readonly TimeSpan _lockoutTime = TimeSpan.FromMinutes(15);
        private List<User> _users;
        private readonly string _filePath = "users.json";
        public JsonFileUserRepository()
        {
            var usersJson = File.ReadAllText(_filePath);
            _users = JsonConvert.DeserializeObject<List<User>>(usersJson);
        }

        public User GetUserByUserNameAndPassword(string username, string password)
        {
            return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase) && u.Password == password);
        }

        public User GetUserByUserName(string username)
        {
            return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
        }

        public User GetUserById(int id)
        {
            return _users.Find(u => u.Id == id);
        }

        public bool UpdateUserRole(int id, string newRole)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.Role = newRole;
                SaveUsers();
                return true;
            }
            return false;
        }

        private void SaveUsers()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var usersJson = System.Text.Json.JsonSerializer.Serialize(_users, options);
            File.WriteAllText(_filePath, usersJson);
        }

        public UserLoginReturnDto AuthenticateUser(string userName, string password)
        {
            var message = "";
            var user = GetUserByUserName(userName);
            if (user == null)
            {
                return new UserLoginReturnDto
                {
                    IsAuthenticated = false,
                    Message = "UserName does not exist in database"
                };
            }

            if (user.Password != password)
            {
                return new UserLoginReturnDto
                {
                    IsAuthenticated = false,
                    Message = "UserName exists but does not match with password"
                };
            }

            if (user.Password == password)
            {
                return new UserLoginReturnDto
                {
                    IsAuthenticated = true,
                    Message = message
                };
            }
            else
            {
                return new UserLoginReturnDto
                {
                    IsAuthenticated = false,
                };
            }
        }

        public bool CreateUser(CreateUserDto createUserDto)
        {
            var user = new User
            {
                Id = GetNewId(),
                Password = createUserDto.Password,
                Role = createUserDto.Role,
                Username = createUserDto.UserName
            };

            // Load existing users from the JSON file
            try
            {
                var existingUsersJson = File.ReadAllText(_filePath);
                var existingUsers = System.Text.Json.JsonSerializer.Deserialize<List<User>>(existingUsersJson) ?? new List<User>();
                existingUsers.Add(user);

                // Serialize the updated list of users
                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedUsersJson = System.Text.Json.JsonSerializer.Serialize(existingUsers, options);

                // Write the updated data back to the JSON file
                File.WriteAllText(_filePath, updatedUsersJson);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private int GetNewId()
        {
            var highestId = _users.OrderByDescending(x => x.Id).First();
            return ++highestId.Id;
        }
    }
}
