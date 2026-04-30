namespace ECommerceProject.Business.Services.Abstract;

public interface IStockReservationService
{
    Task<bool> TryReserveAsync(int userId, int productId, int quantity, int actualStock);
    Task ReleaseAsync(int userId, int productId);
    Task ReleaseManyAsync(int userId, IEnumerable<int> productIds);
    Task<int> GetReservedQuantityAsync(int productId);
    Task<int> GetAvailableStockAsync(int productId, int actualStock);
}
