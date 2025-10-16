using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync().ConfigureAwait(false); // This line should be removed in production
        // await initialiser.MigrateAsync().ConfigureAwait(false); // This line should be used in production
        await initialiser.SeedAsync().ConfigureAwait(false);
    }
}

public class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    AppDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task MigrateAsync()
    {
        try
        {
            await dbContext.Database.MigrateAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while migrating the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await roleManager.CreateAsync(administratorRole).ConfigureAwait(false);
        }

        // Default users
        var administrator = new ApplicationUser
        {
            UserName = "administrator@localhost.dev", Email = "administrator@localhost.dev",
        };

        if (userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await userManager.CreateAsync(administrator, "Administrator1!").ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await userManager.AddToRolesAsync(administrator, [administratorRole.Name]).ConfigureAwait(false);
            }
        }

        // Default data Seed, if necessary
        if (!dbContext.Set<TodoList>().Any())
        {
            var todoList = new TodoList { Title = "Todo List", UserId = administrator.Id, };
            todoList.AddTodoItem(new TodoItem { Title = "Make a todo list 📃", UserId = administrator.Id });
            todoList.AddTodoItem(new TodoItem { Title = "Check off the first item ✅", UserId = administrator.Id });
            todoList.AddTodoItem(new TodoItem
            {
                Title = "Realise you've already done two things on the list! 🤯", UserId = administrator.Id
            });
            todoList.AddTodoItem(new TodoItem
            {
                Title = "Reward yourself with a nice, long nap 🏆", UserId = administrator.Id
            });
            await dbContext.AddAsync(todoList).ConfigureAwait(false);

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
