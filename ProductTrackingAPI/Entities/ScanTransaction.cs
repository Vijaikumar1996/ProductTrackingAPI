using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductTrackingAPI.Entities;

[Table("scan_transactions")]
public class ScanTransaction
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("hu_item_id")]
    public long HuItemId { get; set; }

    [Column("vehicle_dispatch_id")]
    public long? VehicleDispatchId { get; set; }

    [Column("stage")]
    public string Stage { get; set; } = string.Empty;

    [Column("scanned_by")]
    public long ScannedBy { get; set; }

    [Column("scanned_at")]
    public DateTime ScannedAt { get; set; }

    public HuItem HuItem { get; set; } = null!;

    public User User { get; set; } = null!;
}