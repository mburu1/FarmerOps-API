using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FarmerOps.Infrastructure.Persistence;

/// <summary>Idempotent dev/demo seed: a handful of Kenyan counties and sub-counties so Farmer creation
/// has valid DistrictIds to reference out of the box.</summary>
public static class ApplicationDbContextSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Regions.AnyAsync(cancellationToken))
            return;

        var counties = new (string Name, string Code, string[] SubCounties)[]
        {
            ("Nakuru", "NAK", ["Naivasha", "Molo", "Njoro"]),
            ("Kiambu", "KMB", ["Thika", "Ruiru", "Limuru"]),
            ("Meru", "MRU", ["Imenti North", "Tigania East", "Buuri"]),
            ("Uasin Gishu", "UGS", ["Ainabkoi", "Kesses", "Turbo"]),
            ("Kakamega", "KKM", ["Lurambi", "Mumias East", "Butere"])
        };

        foreach (var (name, code, subCounties) in counties)
        {
            var region = new Region(name, code);
            db.Regions.Add(region);

            foreach (var subCounty in subCounties)
                db.Districts.Add(new District(subCounty, region.Id));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
