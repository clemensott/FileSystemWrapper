using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FileSystemWeb.Helpers
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly bool isDev;
        private readonly string password;

        public BasicAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            isDev = configuration.AsEnumerable().Any(IsDev);
            password = configuration["Password"];
        }

        private bool IsDev(KeyValuePair<string, string> pair)
        {
            bool boolValue;
            int intValue;

            return pair.Key == "DEV_ENVIRONMENT" &&
                ((int.TryParse(pair.Value, out intValue) && intValue > 0) ||
                (bool.TryParse(pair.Value, out boolValue) && boolValue));
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return new ValueTask<AuthenticateResult>(HandleAuth()).AsTask();
        }

        private AuthenticateResult HandleAuth()
        {
            System.Diagnostics.Debug.WriteLine("HandleAuth: {0}", Request.Headers.ContainsKey("password"));

            if (!isDev)
            {
                if (Request.Headers.ContainsKey("password"))
                {
                    if (Request.Headers["password"] != password) return AuthenticateResult.Fail("Wrong password in header");
                }
                else if (Request.Query.ContainsKey("password"))
                {
                    if (Request.Query["password"] != password) return AuthenticateResult.Fail("Wrong password in query");
                }
                else return AuthenticateResult.Fail("Password is missing");
            }

            Claim[] claims = new[] { new Claim(ClaimTypes.Name, "Admin") };
            ClaimsIdentity identity = new ClaimsIdentity(claims, Scheme.Name);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
