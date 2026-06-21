namespace ProductTrackingAPI.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public string EntityId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public long? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
