using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.TodoListAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Data.Config;

public class TodoListConfiguration : IEntityTypeConfiguration<TodoList>
{
    public void Configure(EntityTypeBuilder<TodoList> builder)
    {
        builder.ToTable("TodoLists");

        builder.HasKey(x => x.Id);

        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.Title)
            .HasMaxLength(LengthConstants.MediumTitleMaxLength)
            .IsRequired();

        builder.Property(x => x.Colour)
            .HasConversion(
                colour => colour.Code,
                colourString => Colour.From(colourString))
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasMaxLength(LengthConstants.UserIdMaxLength)
            .IsRequired();

        builder.HasMany(x => x.Items)
            .WithOne(x => x.List)
            .HasForeignKey(x => x.ListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
