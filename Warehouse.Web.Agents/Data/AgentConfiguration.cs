using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Web.Agents.Data;

internal class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(DataSchemaConstants.DEFAULT_NAME_LENGTH)
            .IsRequired();

        builder.Property(p => p.Phone)
            .HasMaxLength(DataSchemaConstants.DEFAULT_PHONE_LENGTH);

        builder.Property(p => p.ManagerId)
            .IsRequired();
    }
}
