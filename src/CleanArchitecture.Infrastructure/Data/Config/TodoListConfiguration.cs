using CleanArchitecture.Domain.TodoListAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Data.Config;

public class TodoListConfiguration : IEntityTypeConfiguration<TodoList>
{
    public void Configure(EntityTypeBuilder<TodoList> builder)
    {
        builder.Property(x => x.Title)
            .IsRequired();

        builder.Property(x => x.Colour)
            .HasConversion(
                colour => colour.Code,
                colourString => Colour.From(colourString))
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasMany(x => x.Items)
            .WithOne(x => x.List)
            .HasForeignKey(x => x.ListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
