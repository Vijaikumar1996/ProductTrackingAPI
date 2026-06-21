namespace ProductTrackingAPI.Interface;

public interface IEmailService
{
    Task SendMismatchEmailAsync(
        long saleOrderId,
        string saleOrderNo,
        string stage,
        int expectedCount,
        int actualCount,
        List<string> missingHuNumbers);
}