using System.Reflection;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Common.Interfaces;
using CleanArchitecture.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // auto register entities
        var entityMethod = typeof(ModelBuilder).GetMethod("Entity", []);
        var entities = Assembly.GetAssembly(typeof(BaseEntity))?.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(BaseEntity).IsAssignableFrom(t));

        if (entities == null)
        {
            throw new Exception("No entities found");
        }

        foreach (var entity in entities)
        {
            entityMethod?.MakeGenericMethod(entity).Invoke(modelBuilder, []);
        }
    }
}
