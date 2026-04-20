using Common.Data;

namespace Common.PubSubDataSource;

public interface IPubSubDataSource
{
    void Publish(ProductionEvent e);
}