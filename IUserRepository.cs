namespace SopraOwaspKata
{
    public interface IUserRepository
    {
        User GetUserByUserNameAndPassword(string username, string password);
        User GetUserByUserName(string username);
        User GetUserById(int id);
        bool UpdateUserRole(int id, string newRole);
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
    }
}
