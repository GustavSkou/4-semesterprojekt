using System.ComponentModel;
using System.Runtime.CompilerServices;
using Common.Data;
using CommonProductionHandler;

namespace ProductionHandlerPlugin;

public class OrderHandler
{
    private static OrderHandler _instance = new OrderHandler();

    public static OrderHandler Instance { get {return _instance; } }

    private Queue<Order> _orderQueue;

    public Queue<Order> OrderQueue { get {return _orderQueue; } }

    public event EventHandler NewOrder;

    private OrderHandler()
    {
        _orderQueue = new Queue<Order>();
    }

    public void AddOrderCommandToQueue(ProductionCommand command)
    {
        Order order = ParseCommandToOrder(command);
        _orderQueue.Append(order);
        NewOrder.Invoke(this, null);
    }

    public Order ParseCommandToOrder(ProductionCommand command)
    {
        //command.Parameters;
        throw new NotImplementedException();
    } 
}