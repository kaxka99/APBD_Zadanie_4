namespace apbd_04.service;

public interface IWarehouseService
{
    Task<int> CreateProductWarehouseRecord(ProductWarehouseRequest request);
    Task<int> CreateProductWarehouseRecordWithStoredProcedure(ProductWarehouseRequest request);
}