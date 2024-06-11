using System.Data.SqlClient;
using apbd_04.service;
using Microsoft.AspNetCore.Mvc;

namespace apbd_04.controller;

[Route("api/warehouse")]
[ApiController]
public class WarehouseController(IWarehouseService _warehouseService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProductWarehouseRecord(ProductWarehouseRequest request)
    {
        int result = await _warehouseService.CreateProductWarehouseRecord(request);
        if (result == -1)
        {
            return BadRequest("Invalid request parameters");
        }

        return Ok("ID of inserted row: " + result);
    }
    
    [HttpPost]
    [Route("sp")]
    public async Task<IActionResult> CreateProductWarehouseRecordWithStoredProcedure(ProductWarehouseRequest request)
    {
        try
        {
            Decimal result = await _warehouseService.CreateProductWarehouseRecordWithStoredProcedure(request);
            return Ok("ID of inserted row: " + result);
        }
        catch (SqlException e)
        {
            return BadRequest("Invalid request parameters");
        }
    }
    
}