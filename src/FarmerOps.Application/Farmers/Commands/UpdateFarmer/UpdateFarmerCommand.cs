using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Farmers.Dtos;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Farmers.Commands.UpdateFarmer;

public sealed record UpdateFarmerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string PhoneNumber,
    decimal FarmSizeAcres,
    CropType PrimaryCrop,
    double? GeoLatitude,
    double? GeoLongitude) : IRequest<FarmerDto>;

public sealed class UpdateFarmerCommandValidator : AbstractValidator<UpdateFarmerCommand>
{
    public UpdateFarmerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.FarmSizeAcres).GreaterThan(0);
        RuleFor(x => x.PrimaryCrop).IsInEnum();
    }
}

public sealed class UpdateFarmerCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateFarmerCommand, FarmerDto>
{
    public async Task<FarmerDto> Handle(UpdateFarmerCommand request, CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.Id);

        farmer.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.FarmSizeAcres,
            request.PrimaryCrop,
            request.GeoLatitude,
            request.GeoLongitude);

        await db.SaveChangesAsync(cancellationToken);

        return FarmerDto.FromEntity(farmer);
    }
}
