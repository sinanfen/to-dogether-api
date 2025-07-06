namespace to_dogether_api.DTOs;

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password, string? ColorCode = null, string? InviteToken = null);
public record RefreshTokenRequest(string RefreshToken);
public record AuthResponse(string AccessToken, string RefreshToken, string Username, int UserId, string? InviteToken = null);
public record RefreshTokenResponse(string AccessToken, string RefreshToken);
public record UpdateProfileRequest(string Username, string ColorCode); 