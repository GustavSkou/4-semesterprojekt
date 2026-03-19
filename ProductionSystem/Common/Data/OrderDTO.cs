namespace Common.Data;

public class OrderDTO
{
    public int Id { get; }
    public Item[] Items { get; }

    public OrderDTO(int id, Item[] items)
    {
        Id = id;
        Items = items;
    }
}