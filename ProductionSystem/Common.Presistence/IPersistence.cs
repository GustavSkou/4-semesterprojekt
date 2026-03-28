using Common.Data;

namespace Common.Presistence;

public interface IPersistence
{
    Item[] GetComponents();

    void SaveProductionEvent(ProductionEvent productionEvent);
}