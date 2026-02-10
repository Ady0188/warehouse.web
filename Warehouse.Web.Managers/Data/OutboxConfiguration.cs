using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Web.Managers.Data;

internal class OutboxConfiguration : IEntityTypeConfiguration<Outbox>
{
    public void Configure(EntityTypeBuilder<Outbox> builder)
    {
        builder.Property(p => p.StoreName)
            .IsRequired();
        
        builder.Property(p => p.UserName)
            .IsRequired();
        
        builder.Property(p => p.Method)
            .IsRequired();
        
        builder.Property(p => p.ObjectId)
            .IsRequired();
        
        builder.Property(p => p.ObjectName)
            .IsRequired();
        
        builder.Property(p => p.OldData)
            .HasColumnType("jsonb");

        builder.Property(p => p.NewData)
            .HasColumnType("jsonb")
            .IsRequired();
    }
}
