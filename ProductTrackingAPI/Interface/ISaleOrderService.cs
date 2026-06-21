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
    DateTime? fromDate,
    DateTime? toDate,
    string? status);
        Task<SaleOrderDetailDto?>
 GetOrderDetailAsync(long saleOrderId);
    }
}
