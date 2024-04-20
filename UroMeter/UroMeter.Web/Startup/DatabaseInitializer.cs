using AspNetCore.AsyncInitialization;
using Microsoft.EntityFrameworkCore;
using UroMeter.DataAccess;

namespace UroMeter.Web.Startup;

/// <summary>
/// Contains database migration helper methods.
/// </summary>
internal sealed class DatabaseInitializer : IAsyncInitializer
{
    private readonly AppDbContext appDbContext;

    /// <summary>
    /// Database initializer. Performs migration and data seed.
    /// </summary>
    /// <param name="appDbContext">Data context.</param>
    public DatabaseInitializer(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async Task InitializeAsync()
    {
        await appDbContext.Database.MigrateAsync();
    }
}