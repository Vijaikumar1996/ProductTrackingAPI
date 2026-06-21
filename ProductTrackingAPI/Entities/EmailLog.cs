using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductTrackingAPI.Entities;

[Table("email_logs")]
public class EmailLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("sale_order_id")]
    public long SaleOrderId { get; set; }

    [Column("stage")]
    public string? Stage { get; set; }

    [Column("recipients")]
    public string Recipients { get; set; } = string.Empty;

    [Column("subject")]
    public string Subject { get; set; } = string.Empty;

    [Column("sent_at")]
    public DateTime SentAt { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("error_message")]
    public string? ErrorMessage { get; set; }
}