using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Web.Orders.Data
{
    internal class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
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
        }
    }
}
