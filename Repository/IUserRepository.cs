using SopraOwaspKata.Dto;
using SopraOwaspKata.Model;

namespace SopraOwaspKata.Repository
{
    public interface IUserRepository
    {
        User GetUserByUserNameAndPassword(string username, string password);
        User GetUserByUserName(string username);
        User GetUserById(int id);
        bool UpdateUserRole(int id, string newRole);
        UserLoginReturnDto AuthenticateUser(string userName, string password);
        public bool IsAccountLockedOut(string username);
        public bool CreateUser(CreateUserDto createUserDto);
    }
}
