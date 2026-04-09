using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Common.Data;
using Common.Presistence;

namespace PersistencePlugin;

public class DataHandler : IPersistence
{
    public void SaveProductionEvent(ProductionEvent productionEvent)
    {
        throw new NotImplementedException();
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