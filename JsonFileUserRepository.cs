using Newtonsoft.Json;
using System.Text.Json;

namespace SopraOwaspKata
{
    public class JsonFileUserRepository : IUserRepository
    {
        private List<User> _users;
        private readonly string _filePath = "users.json";
        public JsonFileUserRepository()
        {
            var usersJson = File.ReadAllText(_filePath);
            _users = JsonConvert.DeserializeObject<List<User>>(usersJson);
        }

        public User GetUserByUserNameAndPassword(string username, string password)
        {
            return _users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public User GetUserByUserName(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
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
    }
}
