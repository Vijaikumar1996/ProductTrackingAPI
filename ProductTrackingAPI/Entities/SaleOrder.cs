using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductTrackingAPI.Entities;

[Table("sale_orders")]
public class SaleOrder
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("sale_order_no")]
    public string SaleOrderNo { get; set; } = string.Empty;

    [Column("shipment_date", TypeName = "date")]
    public DateOnly ShipmentDate { get; set; }

    [Column("file_name")]
    public string? FileName { get; set; }

    [Column("expected_hu_count")]
    public int ExpectedHuCount { get; set; }

    [Column("received_hu_count")]
    public int ReceivedHuCount { get; set; }

    [Column("loaded_hu_count")]
    public int LoadedHuCount { get; set; }

    [Column("delivered_hu_count")]
    public int DeliveredHuCount { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("created_by")]
    public long CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Receiving

    [Column("received_by")]
    public long? ReceivedBy { get; set; }

    [Column("received_at")]
    public DateTime? ReceivedAt { get; set; }

    // Loading

    [Column("loaded_by")]
    public long? LoadedBy { get; set; }

    [Column("loaded_at")]
    public DateTime? LoadedAt { get; set; }

    // Delivery

    [Column("delivered_by")]
    public long? DeliveredBy { get; set; }

    [Column("delivered_at")]
    public DateTime? DeliveredAt { get; set; }
}