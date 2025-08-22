using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace boat_shareTests.Infrastructure;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestAuth";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Allow configuring user via headers for each request
        var userId = Request.Headers.ContainsKey("X-Test-UserId") ? Request.Headers["X-Test-UserId"].ToString() : "1";
        var role = Request.Headers.ContainsKey("X-Test-Role") ? Request.Headers["X-Test-Role"].ToString() : "Admin";
        var email = Request.Headers.ContainsKey("X-Test-Email") ? Request.Headers["X-Test-Email"].ToString() : "test@test.com";
        var name = Request.Headers.ContainsKey("X-Test-Name") ? Request.Headers["X-Test-Name"].ToString() : "Test User";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
