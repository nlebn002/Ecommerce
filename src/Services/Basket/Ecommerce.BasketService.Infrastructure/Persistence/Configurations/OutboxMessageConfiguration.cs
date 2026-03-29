using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.BasketService.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.Type)
            .IsRequired();

        builder.Property(message => message.Payload)
            .IsRequired();

        builder.Property(message => message.OccurredOnUtc)
            .IsRequired();

        builder.Property(message => message.LastError);

        builder.Property(message => message.AttemptCount)
            .IsRequired();

        builder.HasIndex(message => new { message.ProcessedOnUtc, message.OccurredOnUtc });
    }
}
