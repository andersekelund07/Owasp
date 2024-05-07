using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SopraOwaspKata
{
    public class LocalJsonFileAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserRepository _userRepository;

        public LocalJsonFileAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserRepository userRepository)
            : base(options, logger, encoder, clock)
        {
            _userRepository = userRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if Username header is present
            if (!Request.Headers.TryGetValue("Username", out var usernameHeader))
            {
                return AuthenticateResult.Fail("Username header is missing in the request.");
            }

            // Get the username value
            var username = usernameHeader.FirstOrDefault();
            if (string.IsNullOrEmpty(username))
            {
                return AuthenticateResult.Fail("Username is not provided or empty.");
            }

            // Use the user repository to validate the username
            var user = _userRepository.GetUserByUserName(username);
            if (user == null)
            {
                return AuthenticateResult.Fail("No user found with the given username.");
            }

            // Create user identity and authentication ticket
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
