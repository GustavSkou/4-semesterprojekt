using Common.Data;
using Common.Persistence;
using Microsoft.EntityFrameworkCore;
using PersistencePlugin.Models;
using System.Collections.Concurrent;
using System.Linq;
using Common.Service;

namespace PersistencePlugin;

public class DataHandler : IPersistence, IPlugin
{
    private readonly DbContextOptions<ProductionDbContext> _dbOptions;
    private ConcurrentQueue<ProductionEvent> productionEvents;
    private bool processingTaskRunning;

    public DataHandler()
    {
        //Console.WriteLine("[DataHandler instance]");
        productionEvents = new ConcurrentQueue<ProductionEvent>();

        // this should probably be move to a config file :)
        var connectionString = "Host=localhost;Port=5433;Database=configurepc;Username=configurepc;Password=configurepc";

        _dbOptions = new DbContextOptionsBuilder<ProductionDbContext>()
            .UseNpgsql(connectionString)
            .Options;
    }

    public void SaveProductionEvent(ProductionEvent productionEvent)
    {
        Console.WriteLine("Saving Production event");
        
        productionEvents.Enqueue(productionEvent);
        
        if (!processingTaskRunning) 
        {            
            _ = ProcessProductionEvents();
        }     
    }

    private async Task ProcessProductionEvents() {
        try {
            processingTaskRunning = true;
            using var dbContext = new ProductionDbContext(_dbOptions);

            while(productionEvents.TryDequeue(out ProductionEvent productionEvent)) 
            {
                if (string.IsNullOrEmpty(productionEvent.Source))
                    continue;

                if (string.IsNullOrEmpty(productionEvent.Level))
                    continue;

                if (string.IsNullOrEmpty(productionEvent.Type))
                    continue;

                string sourceName = productionEvent.Source.Trim().ToLower();
                string levelName = productionEvent.Level.Trim().ToLower();
                string typeName = productionEvent.Type.Trim().ToLower();

                source source = await dbContext.sources.FirstOrDefaultAsync(s => s.name == sourceName) ?? 
                    new source { 
                        name = sourceName, 
                    };

                level level = await dbContext.levels.FirstOrDefaultAsync(l => l.name == levelName) ?? 
                    new level { 
                        name = levelName, 
                    };

                type type = await dbContext.types.FirstOrDefaultAsync(t => t.name == typeName) ?? 
                    new type { 
                        name = typeName
                    };

                if (source.id == 0)
                    dbContext.sources.Add(source);

                if (level.id == 0)
                    dbContext.levels.Add(level);

                if (type.id == 0)
                    dbContext.types.Add(type);

                if (source.id == 0 || level.id == 0 || type.id == 0)
                    await dbContext.SaveChangesAsync();

                DateTime dateTime = productionEvent.DateAndTime != null ? (DateTime)productionEvent.DateAndTime : DateTime.Now;

                log log = new log {
                    timestamp   = dateTime,
                    description = productionEvent.Description,
                    level_id    = level.id,
                    source_id   = source.id,
                    type_id     = type.id,
                };

                dbContext.logs.Add(log);
                await dbContext.SaveChangesAsync();
            }
        } finally {
            processingTaskRunning = false;
        }
    }
    

    public Item[] GetComponents()
    {
        return Array.Empty<Item>();
        /*
        try
        {
            using var db = new ProductionDbContext(_dbOptions);

            return db.components
                .OrderBy(c => c.id)
                .Select((c, i) => new Item
                {
                    TrayId = c.tray_id ?? (i + 1),
                    Name = c.name
                })
                .ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Persistence fallback in GetComponents: {ex.Message}");
            return Array.Empty<Item>();
        }
        */
    }

    public void Start() { }

    public void Stop() { }
}