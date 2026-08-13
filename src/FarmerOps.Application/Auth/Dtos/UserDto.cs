using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;

namespace FarmerOps.Application.Auth.Dtos;

public sealed record UserDto(Guid Id, string Email, UserRole Role, Guid? FieldAgentId)
{
    public static UserDto FromEntity(User user) => new(user.Id, user.Email, user.Role, user.FieldAgentId);
}
