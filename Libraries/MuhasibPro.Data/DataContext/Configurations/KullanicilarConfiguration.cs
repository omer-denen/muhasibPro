using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Data.DataContext.Configurations;

public class KullanicilarConfiguration : IEntityTypeConfiguration<Kullanici>
{
    public void Configure(EntityTypeBuilder<Kullanici> builder)
    {
        builder.HasMany(k => k.KullaniciFirmaRoller).WithOne(kfr => kfr.Kullanici).HasForeignKey(kfr => kfr.KullaniciId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
