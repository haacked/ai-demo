using Haack.AIDemoWeb.Entities;

namespace AIDemo.Web.Startup;

public class AIDemoDbInitializer(IServiceProvider serviceProvider, ILogger<DbInitializer<AIDemoDbContext>> logger)
    : DbInitializer<AIDemoDbContext>(serviceProvider, logger)
{
    protected override async Task SeedDatabaseAsync(AIDemoDbContext dbContext, CancellationToken cancellationToken)
    {
        static List<User> GetPreconfiguredUsers()
        {
            return [
                new() { NameIdentifier = "113634338398706942182" },
            ];
        }

        if (!dbContext.Users.Any())
        {
            dbContext.Users.AddRange(GetPreconfiguredUsers());
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}