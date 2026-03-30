using Ecommerce.Common.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.OrderService.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Type).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Payload).IsRequired();
        builder.Property(entity => entity.LastError).HasColumnType("text");
        builder.HasIndex(entity => new { entity.ProcessedOnUtc, entity.OccurredOnUtc });
    }
}
