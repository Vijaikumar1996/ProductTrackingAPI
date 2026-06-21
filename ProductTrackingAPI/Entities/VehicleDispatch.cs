using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductTrackingAPI.Entities;

[Table("vehicle_dispatches")]
public class VehicleDispatch
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("sale_order_id")]
    public long SaleOrderId { get; set; }

    [Column("vehicle_number")]
    public string VehicleNumber { get; set; } = string.Empty;

    [Column("driver_name")]
    public string? DriverName { get; set; }

    [Column("driver_mobile")]
    public string? DriverMobile { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("created_by")]
    public long CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}