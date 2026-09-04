using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NETFace.Attendance.Api.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public string ApiKey { get; set; } = string.Empty;
}

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    public const string ApiKeyHeaderName = "X-Api-Key";
    public const string DeviceTokenHeaderName = "X-Device-Token";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? providedKey = null;

        if (Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            providedKey = apiKeyHeaderValues.FirstOrDefault();
        }
        else if (Request.Headers.TryGetValue(DeviceTokenHeaderName, out var deviceTokenHeaderValues))
        {
            providedKey = deviceTokenHeaderValues.FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(providedKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var expectedKey = Options.ApiKey;
        if (string.IsNullOrWhiteSpace(expectedKey) || !string.Equals(providedKey, expectedKey, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key or Device Token."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "TerminalDevice"),
            new Claim(ClaimTypes.Name, "TerminalDevice"),
            new Claim(ClaimTypes.Role, "Device")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
