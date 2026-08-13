using FarmerOps.Application.Common.Interfaces;
using FarmerOps.Application.Regions.Dtos;
using FarmerOps.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FarmerOps.Application.Regions.Commands.CreateRegion;

public sealed record CreateRegionCommand(string Name, string Code) : IRequest<RegionDto>;

public sealed class CreateRegionCommandValidator : AbstractValidator<CreateRegionCommand>
{
    public CreateRegionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
    }
}

public sealed class CreateRegionCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateRegionCommand, RegionDto>
{
    public async Task<RegionDto> Handle(CreateRegionCommand request, CancellationToken cancellationToken)
    {
        var region = new Region(request.Name, request.Code);
        db.Regions.Add(region);
        await db.SaveChangesAsync(cancellationToken);

        return RegionDto.FromEntity(region);
    }
}
