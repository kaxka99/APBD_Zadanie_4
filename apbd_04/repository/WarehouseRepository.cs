using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace apbd_04.repository;

public class WarehouseRepository(IConfiguration _configuration) : IWarehouseRepository
{
    public async Task<int> CreateProductWarehouseRecord(ProductWarehouseRequest request)
    {
        return 0;
    }

    public async Task<bool> IsProductIdExisting(int id)
    {
        await using var con = new SqlConnection(provideConnectionString());
        await con.OpenAsync();
        
        await using var cmd = new SqlCommand();
        cmd.Connection = con;
        cmd.CommandText = "SELECT * FROM Product WHERE IdProduct = @IdProduct";
        cmd.Parameters.AddWithValue("@IdProduct", id);

        var updatedObjects = cmd.ExecuteScalarAsync();
        return updatedObjects != null && (int) await updatedObjects > 0;
    }
    
    public async Task<bool> IsWarehouseIdExisting(int id)
    {
        await using var con = new SqlConnection(provideConnectionString());
        await con.OpenAsync();
        
        await using var cmd = new SqlCommand();
        cmd.Connection = con;
        cmd.CommandText = "SELECT * FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        cmd.Parameters.AddWithValue("@IdWarehouse", id);

        var updatedObjects = cmd.ExecuteScalarAsync();
        var result = await updatedObjects;
        return result != null && (int) result > 0;
    }
    
    public async Task<int> IsOrderExisting(int id, int amount, DateTime createDate)
    {
        await using var con = new SqlConnection(provideConnectionString());
        await con.OpenAsync();
        
        await using var cmd = new SqlCommand();
        cmd.Connection = con;
        cmd.CommandText = "SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreateDate";
        cmd.Parameters.AddWithValue("@IdProduct", id);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Parameters.AddWithValue("@CreateDate", createDate);

        using (var dr = await cmd.ExecuteReaderAsync())
        {
            while (await dr.ReadAsync())
            {
                return (int)dr["IdOrder"];
            }
        }
                
        
        return -1;
    }

    public async Task<bool> IsOrderCompleted(int orderId)
    {
        await using var con = new SqlConnection(provideConnectionString());
        await con.OpenAsync();
        
        await using var cmd = new SqlCommand();
        cmd.Connection = con;
        cmd.CommandText = "SELECT * FROM Product_Warehouse WHERE IdOrder = @IdOrder";
        cmd.Parameters.AddWithValue("@IdOrder", orderId);
        
        var result = await cmd.ExecuteScalarAsync();
        return result != null && (int) result > 0;
    }
    
    public async Task<bool> UpdateOrderFullfilledAt(int orderId, DateTime dateTime)
    {
        await using var con = new SqlConnection(provideConnectionString());
        await con.OpenAsync();
        
        await using var cmd = new SqlCommand();
        cmd.Connection = con;
        cmd.CommandText = "UPDATE [Order] SET FullfilledAt = @FullfilledAt WHERE IdOrder = @IdOrder";
        cmd.Parameters.AddWithValue("@IdOrder", orderId);
        cmd.Parameters.AddWithValue("@FullfilledAt", dateTime);

        var updatedObjects = cmd.ExecuteNonQueryAsync();
        return await updatedObjects > 0;
    }

    public async Task<int> InsertProductWarehouseRow(ProductWarehouseRequest request, int orderId)
    {
        await using var con = new SqlConnection(provideConnectionString());
        await con.OpenAsync();

        var price = GetPriceFromProduct(request.IdProduct);
        
        await using var cmd = new SqlCommand();
        cmd.Connection = con;
        cmd.CommandText = "INSERT INTO Product_Warehouse VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt); SELECT CAST(scope_identity() AS int)";
        cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
        cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        cmd.Parameters.AddWithValue("@IdOrder", orderId);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@Price", request.Amount * price.Result);
        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        var id = cmd.ExecuteScalarAsync();
        return (int) await id;
    }

    public async Task<Decimal> GetPriceFromProduct(int productId)
    {
        await using var con = new SqlConnection(provideConnectionString());
        await con.OpenAsync();
        
        await using var cmd = new SqlCommand();
        cmd.Connection = con;
        cmd.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
        cmd.Parameters.AddWithValue("@IdProduct", productId);

        using (var dr = await cmd.ExecuteReaderAsync())
        {
            while (await dr.ReadAsync())
            {
                return (decimal)dr["Price"];
            }
        }
                
        
        return Decimal.MinusOne;
    }

    public async Task<int> FulfillOrder(ProductWarehouseRequest request, int orderId)
    {
        await using var con = new SqlConnection(provideConnectionString());
        await con.OpenAsync();

        await using var cmd = new SqlCommand();
        DbTransaction tran = await con.BeginTransactionAsync();
        cmd.Transaction = (SqlTransaction)tran;
        try
        {
            UpdateOrderFullfilledAt(orderId, DateTime.Now);
            int id = await InsertProductWarehouseRow(request, orderId);
            await tran.CommitAsync();
            return id;
        }
        catch (SqlException exc)
        { 
            await tran.RollbackAsync();
            return -1;
        }
        catch (Exception exc)
        {
            await tran.RollbackAsync();
            return -1;
        }
    }

    public async Task<int> AddProductToWarehouseSP(ProductWarehouseRequest request)
    {
        await using var con = new SqlConnection(provideConnectionString());
        await con.OpenAsync();

        await using var cmd = new SqlCommand("AddProductToWarehouse", con);
        cmd.CommandType = CommandType.StoredProcedure; 
        cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse); 
        cmd.Parameters.AddWithValue("@Amount", request.Amount); 
        cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
        
        var id = cmd.ExecuteScalarAsync();
        return Decimal.ToInt32((Decimal) await id);
    }
    
    private string provideConnectionString()
    {
        var connectionString = new SqlConnectionStringBuilder(_configuration["ConnectionStrings:localhostMSSQLServer"]);
        connectionString.UserID = _configuration["DbUserId"];
        connectionString.Password = _configuration["DbPassword"];
        return connectionString.ConnectionString;
    }
}