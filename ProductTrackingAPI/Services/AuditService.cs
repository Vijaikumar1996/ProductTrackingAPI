using ProductTrackingAPI.Data;
using ProductTrackingAPI.Entities;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            string action,
            string entityName,
            string entityId,
            string description,
            long? userId = null)
        {
            try
            {
                var log = new AuditLog
                {
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Description = description,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditLogs.Add(log);

                await _context.SaveChangesAsync();
            }
            catch
            {

            }
            
        }
    }
}
