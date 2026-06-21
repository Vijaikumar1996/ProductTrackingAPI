using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductTrackingAPI.Entities;

[Table("hu_items")]
public class HuItem
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("sale_order_id")]
    public long SaleOrderId { get; set; }

    [Column("hu_number_barcode")]
    public string HuNumberBarcode { get; set; } = string.Empty;

    [Column("equipment_number")]
    public string? EquipmentNumber { get; set; }

    [Column("order_item_number")]
    public string? OrderItemNumber { get; set; }

    [Column("material_id")]
    public string? MaterialId { get; set; }

    [Column("material_description")]
    public string? MaterialDescription { get; set; }

    [Column("package_description")]
    public string? PackageDescription { get; set; }

    [Column("material_quantity")]
    public decimal? MaterialQuantity { get; set; }

    [Column("uom")]
    public string? Uom { get; set; }

    [Column("length")]
    public decimal? Length { get; set; }

    [Column("width")]
    public decimal? Width { get; set; }

    [Column("height")]
    public decimal? Height { get; set; }

    [Column("net_weight")]
    public decimal? NetWeight { get; set; }

    [Column("gross_weight")]
    public decimal? GrossWeight { get; set; }

    [Column("volume")]
    public decimal? Volume { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("status")]
    public string Status { get; set; } = "UPLOADED";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public SaleOrder SaleOrder { get; set; } = null!;
}