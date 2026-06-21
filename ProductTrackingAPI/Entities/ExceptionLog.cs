namespace ProductTrackingAPI.Entities
{
    public class ExceptionLog
    {
        public long Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? StackTrace { get; set; }

        public string? InnerException { get; set; }
        public string? Path { get; set; }

        public string? Method { get; set; }

        public long? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
