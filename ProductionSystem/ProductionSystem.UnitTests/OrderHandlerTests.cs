using System.Text.Json;
using CommonProductionHandler;
using ProductionHandlerPlugin;
using Xunit;

namespace ProductionSystem.UnitTests;

public class OrderHandlerTests
{
    // Test 1: "Kan vi lave en ordre rigtigt?"
    // Vi giver id + items og forventer et OrderDTO med de samme data.
    [Fact]
    public void ParseCommandToOrder_MapsIdAndItems()
    {
        ClearQueue();

        var command = CreateOrderCommand(1, new[] { 10, 11 });
        var order = OrderHandler.Instance.ParseCommandToOrder(command);

        Assert.Equal(1, order.Id);
        Assert.Equal(2, order.Items.Length);
        Assert.Equal(10, order.Items[0].TrayId);
        Assert.Equal(11, order.Items[1].TrayId);
    }

    // Test 2: "Hvad hvis der mangler data?"
    // Hvis Parameters er null, skal vi få en fejl.
    [Fact]
    public void ParseCommandToOrder_ThrowsWhenParametersMissing()
    {
        ClearQueue();

        var command = new ProductionCommand { Name = "order", Parameters = null };

        Assert.Throws<ArgumentException>(() => OrderHandler.Instance.ParseCommandToOrder(command));
    }

    // Test 3: "Kommer ordren i koen, og bliver eventet kaldt?"
    [Fact]
    public void AddOrderCommandToQueue_EnqueuesAndRaisesEvent()
    {
        ClearQueue();

        var wasRaised = false;
        EventHandler? handler = (_, __) => wasRaised = true;
        OrderHandler.Instance.NewOrder += handler;

        try
        {
            var command = CreateOrderCommand(2, new[] { 20, 21 });
            OrderHandler.Instance.AddOrderCommandToQueue(command);

            Assert.True(wasRaised);
            Assert.Single(OrderHandler.Instance.OrderQueue);
        }
        finally
        {
            OrderHandler.Instance.NewOrder -= handler;
        }
    }

    private static ProductionCommand CreateOrderCommand(int id, int[] items)
    {
        var parameters = new Dictionary<string, JsonElement>
        {
            ["id"] = JsonSerializer.SerializeToElement(id),
            ["items"] = JsonSerializer.SerializeToElement(items),
        };

        return new ProductionCommand
        {
            Name = "order",
            Parameters = parameters,
        };
    }

    private static void ClearQueue()
    {
        OrderHandler.Instance.OrderQueue.Clear();
    }
}
