using FarmerOps.Application.Common.Exceptions;
using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Regions.Dtos;
using FarmerOps.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Application.Regions.Commands.CreateDistrict;

public sealed record CreateDistrictCommand(string Name, Guid RegionId) : IRequest<DistrictDto>;

public sealed class CreateDistrictCommandValidator : AbstractValidator<CreateDistrictCommand>
{
    public CreateDistrictCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RegionId).NotEmpty();
    }
}

public sealed class CreateDistrictCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateDistrictCommand, DistrictDto>
{
    public async Task<DistrictDto> Handle(CreateDistrictCommand request, CancellationToken cancellationToken)
    {
        var regionExists = await db.Regions.AnyAsync(r => r.Id == request.RegionId, cancellationToken);
        if (!regionExists)
            throw new NotFoundException(nameof(Region), request.RegionId);

        var district = new District(request.Name, request.RegionId);
        db.Districts.Add(district);
        await db.SaveChangesAsync(cancellationToken);

        return DistrictDto.FromEntity(district);
    }
}
