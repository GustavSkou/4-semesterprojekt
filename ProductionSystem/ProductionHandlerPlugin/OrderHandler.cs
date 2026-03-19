using System.Text.Json;
using Common.Data;
using CommonProductionHandler;

namespace ProductionHandlerPlugin;

public class OrderHandler
{
    private static OrderHandler _instance = new OrderHandler();

    public static OrderHandler Instance { get { return _instance; } }

    private Queue<OrderDTO> _orderQueue;

    public Queue<OrderDTO> OrderQueue { get { return _orderQueue; } }

    public event EventHandler? NewOrder;

    private OrderHandler()
    {
        _orderQueue = new Queue<OrderDTO>();
    }

    public void AddOrderCommandToQueue(ProductionCommand command)
    {
        OrderDTO order = ParseCommandToOrder(command);
        _orderQueue.Append(order);

        NewOrder?.Invoke(this, EventArgs.Empty);
        Console.WriteLine($"Add order: {order.Id} to queue");
    }

    public OrderDTO ParseCommandToOrder(ProductionCommand command)
    {
        if (command.Parameters is null)
            throw new ArgumentException("Command has no parameters");

        var id = command.Parameters["id"].GetInt32();

        var itemIds = command.Parameters["items"].Deserialize<int[]>() ?? Array.Empty<int>();
        var items = itemIds
            .Select(x => new Item { TrayId = x })
            .ToArray();

        return new OrderDTO(id, items);
    }
}