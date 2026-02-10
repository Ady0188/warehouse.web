using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Web.Catalog.Data;

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Code)
            .IsRequired()
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();

        builder.Property(p => p.Name)
            .HasMaxLength(DataSchameConstants.DEFAULT_NAME_LENGTH)
            .IsRequired();

        builder.Property(p => p.Unit)
            .HasMaxLength(DataSchameConstants.DEFAULT_UNIT_LENGTH)
            .IsRequired();

        builder.Property(p => p.BuyPrice)
            .IsRequired();

        builder.Property(p => p.SellPrice)
            .IsRequired();

        builder.Property(p => p.LimitRemain)
            .IsRequired();
    }
}
