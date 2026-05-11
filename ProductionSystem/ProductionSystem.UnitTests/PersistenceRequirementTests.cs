using Common.Data;
using Microsoft.EntityFrameworkCore;
using PersistencePlugin;
using PersistencePlugin.Models;
using Xunit;

namespace ProductionSystem.UnitTests;

public class PersistenceRequirementTests
{
    [Fact]
    public async Task F09_saves_production_data_to_database()
    {
        var options = CreateOptions();
        var handler = new DataHandler(options);

        handler.SaveProductionEvent(new ProductionEvent
        {
            DateAndTime = new DateTime(2026, 5, 11, 10, 30, 0, DateTimeKind.Utc),
            Description = "AGV delivered components",
            Source = "agv",
            Type = "step-status",
            Level = "low",
        });

        await WaitUntilAsync(async () =>
        {
            await using var db = new ProductionDbContext(options);
            return await db.logs.CountAsync() == 1;
        });

        await using var verifyDb = new ProductionDbContext(options);
        var savedLog = await verifyDb.logs
            .Include(log => log.source)
            .Include(log => log.level)
            .Include(log => log.type)
            .SingleAsync();

        Assert.Equal("AGV delivered components", savedLog.description);
        Assert.Equal("agv", savedLog.source.name);
        Assert.Equal("low", savedLog.level.name);
        Assert.Equal("step-status", savedLog.type.name);
        Assert.Equal(new DateTime(2026, 5, 11, 10, 30, 0, DateTimeKind.Utc), savedLog.timestamp);
    }

    [Fact]
    public void F20_tracks_inventory_items_in_database()
    {
        var options = CreateOptions();

        using (var seedDb = new ProductionDbContext(options))
        {
            seedDb.components.AddRange(
                new component { id = 1, name = "Ryzen 7 7800X3D", tray_id = 1, price = 399 },
                new component { id = 2, name = "GeForce RTX 4070", tray_id = 14, price = 599 });
            seedDb.SaveChanges();
        }

        var handler = new DataHandler(options);
        var items = handler.GetComponents();

        Assert.Equal(2, items.Length);
        Assert.Contains(items, item => item.TrayId == 1 && item.Name == "Ryzen 7 7800X3D");
        Assert.Contains(items, item => item.TrayId == 14 && item.Name == "GeForce RTX 4070");
    }

    private static DbContextOptions<ProductionDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ProductionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> predicate, int timeoutMs = 3000)
    {
        var startedAt = DateTime.UtcNow;
        while (!await predicate())
        {
            if ((DateTime.UtcNow - startedAt).TotalMilliseconds > timeoutMs)
                throw new TimeoutException("Condition was not met before timeout.");

            await Task.Delay(10);
        }
    }
}
