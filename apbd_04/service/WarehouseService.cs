using System.Data.Common;
using System.Data.SqlClient;
using apbd_04.repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace apbd_04.service;

public class WarehouseService(IWarehouseRepository _warehouseRepository) : IWarehouseService
{
    public async Task<int> CreateProductWarehouseRecord(ProductWarehouseRequest request)
    {
        if (!await ValidateRequest(request))
        {
            return -1;
        }
        Task<int> orderId =
            _warehouseRepository.IsOrderExisting(request.IdProduct, request.Amount, request.CreatedAt);
        Task<bool> isOrderCompleted = _warehouseRepository.IsOrderCompleted(await orderId);
        if (await isOrderCompleted)
        {
            return -1;
        }

        return await _warehouseRepository.FulfillOrder(request, await orderId);
    }

    public async Task<int> CreateProductWarehouseRecordWithStoredProcedure(ProductWarehouseRequest request)
    {
        return await _warehouseRepository.AddProductToWarehouseSP(request);
    }

    private async Task<bool> ValidateRequest(ProductWarehouseRequest request)
    {
        Task<bool> isProductExisting = _warehouseRepository.IsProductIdExisting(request.IdProduct);
        Task<bool> isWarehouseExisting = _warehouseRepository.IsWarehouseIdExisting(request.IdWarehouse);

        await Task.WhenAll([isProductExisting, isWarehouseExisting]);

        return isProductExisting.Result && isWarehouseExisting.Result && request.Amount > 0;
    }
}