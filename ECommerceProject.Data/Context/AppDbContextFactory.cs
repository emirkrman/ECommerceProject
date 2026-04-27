using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerceProject.Data.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string GetConnectionString()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory != null)
        {
            var appSettingsPath = Path.Combine(currentDirectory.FullName, "ECommerceProject.Web", "appsettings.json");
            if (File.Exists(appSettingsPath))
            {
                using var stream = File.OpenRead(appSettingsPath);
                using var document = JsonDocument.Parse(stream);

                if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
                    connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
                {
                    return defaultConnection.GetString()
                        ?? throw new InvalidOperationException("DefaultConnection bos olamaz.");
                }
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new FileNotFoundException("ECommerceProject.Web/appsettings.json bulunamadi.");
    }
}
