namespace ProductTrackingAPI.Interface
{
    public interface IAuditService
    {
        Task LogAsync(
            string action,
            string entityName,
            string entityId,
            string description,
            long? userId = null);
    }
}
