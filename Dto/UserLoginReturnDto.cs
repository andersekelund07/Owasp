namespace SopraOwaspKata.Dto
{
    public class UserLoginReturnDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}
