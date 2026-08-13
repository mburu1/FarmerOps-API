using FarmerOps.Domain.Common;

namespace FarmerOps.Domain.Entities;

/// <summary>Mirrors a Kenyan county.</summary>
public class Region : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;

    private readonly List<District> _districts = [];
    public IReadOnlyCollection<District> Districts => _districts.AsReadOnly();

    private Region()
    {
    }

    public Region(string name, string code)
    {
        Name = name;
        Code = code;
    }
}
