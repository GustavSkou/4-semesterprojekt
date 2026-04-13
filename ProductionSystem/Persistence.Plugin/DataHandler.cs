using Common.Data;
using Common.Presistence;
using Microsoft.EntityFrameworkCore;
using PersistencePlugin.Models;

namespace PersistencePlugin;

public class DataHandler : IPersistence
{
    private readonly DbContextOptions<ProductionDbContext> _dbOptions;

    public DataHandler()
    {
        var connectionString = "Host=localhost;Port=5433;Database=configurepc;Username=configurepc;Password=configurepc";

        _dbOptions = new DbContextOptionsBuilder<ProductionDbContext>()
            .UseNpgsql(connectionString)
            .Options;
    }

    public void SaveProductionEvent(ProductionEvent productionEvent)
    {
        /*
        using var dbContext = new ProductionDbContext(_dbOptions);
        
        source source = new source();
        level level = new level();
        type type = new type();

        if (!string.IsNullOrEmpty(productionEvent.Source))
        {
            source.
            {
                name = productionEvent.Source
            };
        }

        if (!string.IsNullOrEmpty(productionEvent.Level))
        {
            level 
            {
                name = productionEvent.Level
            };
        }

        if (!string.IsNullOrEmpty(productionEvent.Type))
        {
            type type = new type
            {
                name = productionEvent.Type
            };
        }
        


        log log = new log
        {
            timestamp   = productionEvent.DateAndTime,
            description = productionEvent.Description,
            level_id    = 
            source_id   =
            type_id     =
            level =
            source =

type=;
        };

        dbContext.logs.save
        

        /*productionEvent.Type;
        productionEvent.Level;
        productionEvent.Source;*/



    }

    public Item[] GetComponents()
    {
        throw new NotImplementedException();
        /*
        using var db = new ProductionDbContext();
        return db.Components
            .OrderBy(c => c.Id)
            .Select((c, i) => new Item { TrayId = i + 1, Name = c.Name })
            .ToArray();
    */}
}