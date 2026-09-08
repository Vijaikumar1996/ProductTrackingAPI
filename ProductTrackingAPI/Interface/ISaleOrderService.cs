using ProductTrackingAPI.DTOs;

namespace ProductTrackingAPI.Interface
{
    public interface ISaleOrderService
    {
        Task UploadAsync(
            UploadSaleOrderRequest request,
            long userId);

        Task<List<SaleOrderListDto>> GetOrdersAsync(
    string? saleOrderNo,
    DateOnly? fromDate,
    DateOnly? toDate,
    string? status);
        Task<SaleOrderDetailDto?>
 GetOrderDetailAsync(long saleOrderId);
    }
}
