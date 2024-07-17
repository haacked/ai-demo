using Haack.AIDemoWeb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pgvector.EntityFrameworkCore;

namespace AIDemoWeb.Entities;

// ReSharper disable once UnusedType.Global
/// <summary>
/// This is used for EF design time tools such as the EF Core CLI used to run migrations, etc.
/// </summary>
public class AIDemoContextFactory : IDesignTimeDbContextFactory<AIDemoContext>
{
    public AIDemoContext CreateDbContext(string[] args)
    {
        // Used only for EF .NET Core CLI tools (update database/migrations etc.)
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile(path: "appsettings.Development.json", optional: true, reloadOnChange: true);
        var configuration = builder.Build();
        var optionsBuilder = new DbContextOptionsBuilder<AIDemoContext>()
            .UseNpgsql(configuration.GetConnectionString(AIDemoContext.ConnectionStringName),
                options =>
                {
                    options.UseVector();
                    options.UseNetTopologySuite();
                    options.MigrationsAssembly("AIDemo.Web");
                });
        return new AIDemoContext(optionsBuilder.Options);
    }
}
