using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using to_dogether_api.Services;

namespace to_dogether_api.Middleware;

public class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly JwtService _jwtService;

    public JwtAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        JwtService jwtService)
        : base(options, logger, encoder, clock)
    {
        _jwtService = jwtService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Authorization header'ı kontrol et
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string token;
        
        // Hem "Bearer token" hem de sadece "token" formatını destekle
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authHeader["Bearer ".Length..].Trim();
        }
        else
        {
            token = authHeader.Trim();
        }

        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            // Token'ı validate et ve claims'leri al
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
            }

            var ticket = new AuthenticationTicket(principal, "JWT");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "JWT authentication failed");
            return Task.FromResult(AuthenticateResult.Fail("Token validation failed"));
        }
    }
} 