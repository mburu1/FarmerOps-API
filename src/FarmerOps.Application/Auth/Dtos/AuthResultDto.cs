namespace FarmerOps.Application.Auth.Dtos;

public sealed record AuthResultDto(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, UserDto User);
