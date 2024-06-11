namespace apbd_04.repository;

public interface IWarehouseRepository
{
    Task<int> CreateProductWarehouseRecord(ProductWarehouseRequest request);

    Task<bool> IsProductIdExisting(int id);
    
    Task<bool> IsWarehouseIdExisting(int id);

    Task<int> IsOrderExisting(int id, int amount, DateTime createDate);

    Task<bool> IsOrderCompleted(int orderId);
    
    Task<bool> UpdateOrderFullfilledAt(int orderId, DateTime dateTime);

    Task<int> FulfillOrder(ProductWarehouseRequest request, int orderId);

    Task<int> AddProductToWarehouseSP(ProductWarehouseRequest request);
}