using Common.Data;

namespace Common.Persistence;

public interface IPersistence
{
    Item[] GetComponents();

    void SaveProductionEvent(ProductionEvent productionEvent);
}