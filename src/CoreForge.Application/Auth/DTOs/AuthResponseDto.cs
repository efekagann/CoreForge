namespace CoreForge.Application.Auth.DTOs;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string Email,
    IList<string> Roles
);
