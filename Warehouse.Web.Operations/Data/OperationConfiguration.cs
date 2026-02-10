using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Web.Operations.Data;

internal class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.Property(p => p.Code)
            .IsRequired()
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();

        builder.Property(p => p.StoreId)
            .IsRequired();

        builder.Property(p => p.AgentId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .IsRequired();

        builder.Property(p => p.Type)
        .IsRequired();


        // 1) Навигация через публичное свойство Products
        builder.HasMany(o => o.Products)
               .WithOne()
               .HasForeignKey("OperationId") // shadow FK column
               .OnDelete(DeleteBehavior.Cascade);

        // 2) Указать EF, что использовать field access для навигации
        builder.Navigation(o => o.Products)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Опционально — явное указание backing field (works in EF Core)
        var navigation = builder.Metadata.FindNavigation(nameof(Operation.Products));
        navigation.SetField("_products");
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        // если вы хотите shadow FK
        builder.Property<long>("OperationId").IsRequired();

        builder.Property(p => p.ProductId).IsRequired();
        builder.Property(p => p.Code).IsRequired();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(250);
        builder.Property(p => p.Unit).IsRequired();
        builder.Property(p => p.Manufacturer).IsRequired();
        builder.Property(p => p.Quantity).IsRequired();
        builder.Property(p => p.BuyPrice).HasColumnType("numeric(18,2)");
        builder.Property(p => p.SellPrice).HasColumnType("numeric(18,2)");
    }
}
