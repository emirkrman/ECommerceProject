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
            var webProjectPath = Path.Combine(currentDirectory.FullName, "ECommerceProject.Web");
            var localSettingsPath = Path.Combine(webProjectPath, "appsettings.Local.json");
            var defaultSettingsPath = Path.Combine(webProjectPath, "appsettings.json");

            if (File.Exists(localSettingsPath))
            {
                var localConnectionString = ReadConnectionString(localSettingsPath);
                if (!string.IsNullOrWhiteSpace(localConnectionString))
                    return localConnectionString;
            }

            if (File.Exists(defaultSettingsPath))
            {
                var defaultConnectionString = ReadConnectionString(defaultSettingsPath);
                if (!string.IsNullOrWhiteSpace(defaultConnectionString))
                    return defaultConnectionString;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new FileNotFoundException(
            "DefaultConnection bulunamadi. appsettings.Local.json veya appsettings.json tanimlayin.");
    }

    private static string? ReadConnectionString(string path)
    {
        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream);

        if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
            connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
        {
            return defaultConnection.GetString();
        }

        return null;
    }
}
