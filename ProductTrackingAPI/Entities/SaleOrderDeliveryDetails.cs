using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductTrackingAPI.Entities;

[Table("sale_order_delivery_details")]
public class SaleOrderDeliveryDetail
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("sale_order_id")]
    public long SaleOrderId { get; set; }

    [Column("customer_name")]
    public string? CustomerName { get; set; }

    [Column("delivery_address")]
    public string? DeliveryAddress { get; set; }

    [Column("contact_person")]
    public string? ContactPerson { get; set; }

    [Column("contact_number")]
    public string? ContactNumber { get; set; }

    [Column("expected_delivery_date")]
    public DateTime? ExpectedDeliveryDate { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}