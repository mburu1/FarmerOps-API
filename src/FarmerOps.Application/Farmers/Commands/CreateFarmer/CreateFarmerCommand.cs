using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Farmers.Dtos;
using FarmerOps.Domain.Entities;
using FarmerOps.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Farmers.Commands.CreateFarmer;

public sealed record CreateFarmerCommand(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string NationalId,
    Guid DistrictId,
    decimal FarmSizeAcres,
    CropType PrimaryCrop,
    double? GeoLatitude,
    double? GeoLongitude) : IRequest<FarmerDto>;

public sealed class CreateFarmerCommandValidator : AbstractValidator<CreateFarmerCommand>
{
    public CreateFarmerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.NationalId).NotEmpty().MaximumLength(30);
        RuleFor(x => x.DistrictId).NotEmpty();
        RuleFor(x => x.FarmSizeAcres).GreaterThan(0);
        RuleFor(x => x.PrimaryCrop).IsInEnum();
        RuleFor(x => x.GeoLatitude).InclusiveBetween(-90, 90).When(x => x.GeoLatitude.HasValue);
        RuleFor(x => x.GeoLongitude).InclusiveBetween(-180, 180).When(x => x.GeoLongitude.HasValue);
    }
}

public sealed class CreateFarmerCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateFarmerCommand, FarmerDto>
{
    public async Task<FarmerDto> Handle(CreateFarmerCommand request, CancellationToken cancellationToken)
    {
        var districtExists = await db.Districts.AnyAsync(d => d.Id == request.DistrictId, cancellationToken);
        if (!districtExists)
            throw new FluentValidation.ValidationException([
                new FluentValidation.Results.ValidationFailure(nameof(request.DistrictId), "District does not exist.")
            ]);

        var farmer = new Farmer(
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.NationalId,
            request.DistrictId,
            request.FarmSizeAcres,
            request.PrimaryCrop,
            request.GeoLatitude,
            request.GeoLongitude);

        db.Farmers.Add(farmer);
        await db.SaveChangesAsync(cancellationToken);

        return FarmerDto.FromEntity(farmer);
    }
}
