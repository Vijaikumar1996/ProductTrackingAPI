using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductTrackingAPI.Entities;

[Table("sale_order_transport_details")]
public class SaleOrderTransportDetail
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("sale_order_id")]
    public long SaleOrderId { get; set; }

    [Column("inbound_vehicle_number")]
    public string? InboundVehicleNumber { get; set; }

    [Column("inbound_driver_name")]
    public string? InboundDriverName { get; set; }

    [Column("inbound_driver_mobile")]
    public string? InboundDriverMobile { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}