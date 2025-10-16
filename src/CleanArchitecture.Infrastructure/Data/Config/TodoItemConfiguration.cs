using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.TodoListAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Data.Config;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.Property(x => x.Title)
            .HasMaxLength(LengthConstants.MediumTitleMaxLength);

        builder.Property(x => x.Note)
            .HasMaxLength(LengthConstants.MediumContentMaxLength);

        builder.Property(x => x.Priority)
            .HasConversion<int>();

        builder.Property(x => x.ListId)
            .IsRequired();

        builder.Property(x => x.Done)
            .IsRequired();

        builder.HasOne(x => x.List)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
