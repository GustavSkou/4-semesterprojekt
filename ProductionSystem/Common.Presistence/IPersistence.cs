using Common.Data;

namespace Common.Presistence;

public interface IPersistence
{
    void SaveProductionEvent(ProductionEvent productionEvent);
}