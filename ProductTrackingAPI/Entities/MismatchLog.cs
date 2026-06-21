using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductTrackingAPI.Entities;

[Table("mismatch_logs")]
public class MismatchLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("sale_order_id")]
    public long SaleOrderId { get; set; }

    [Column("stage")]
    public string Stage { get; set; } = string.Empty;

    [Column("expected_count")]
    public int ExpectedCount { get; set; }

    [Column("actual_count")]
    public int ActualCount { get; set; }

    [Column("missing_count")]
    public int MissingCount { get; set; }

    [Column("missing_hu_numbers")]
    public string? MissingHuNumbers { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}