using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SopraOwaspKata
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
            if (IsAccountLockedOut(userName))
            {
                return new UserLoginReturnDto
                {
                    IsAuthenticated = false,
                };
            }

            var message = "";
            var user = GetUserByUserName(userName);
            if (user == null) {
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
                    Message = "UserName does not match with password"
                };
            }

            if (user.Password == password)
            {
                ResetLockout(userName);
                return new UserLoginReturnDto
                {
                    IsAuthenticated = true,
                    Message = message
                };
            }
            else
            {
                RegisterFailedAttempt(userName);
                return new UserLoginReturnDto
                {
                    IsAuthenticated = false,
                };
            }
        }

        public bool IsAccountLockedOut(string username)
        {
            if (_lockoutInfo.TryGetValue(username, out var lockoutDetails) && lockoutDetails.AttemptCount >= _lockoutThreshold)
            {
                if (DateTime.UtcNow - lockoutDetails.LastAttempt < _lockoutTime)
                {
                    return true; // User is locked out
                }
                else
                {
                    ResetLockout(username); // Reset on timeout
                    return false;
                }
            }
            return false;
        }

        private void RegisterFailedAttempt(string username)
        {
            _lockoutInfo.AddOrUpdate(username,
                (1, DateTime.UtcNow), // if adding for the first time
                (key, oldValue) => (oldValue.AttemptCount + 1, DateTime.UtcNow)); // if updating existing entry
        }

        private void ResetLockout(string username)
        {
            try
            {
                _lockoutInfo.TryUpdate(username, (0, DateTime.UtcNow), _lockoutInfo[username]);
            }
            catch { }
        }
    }
}
