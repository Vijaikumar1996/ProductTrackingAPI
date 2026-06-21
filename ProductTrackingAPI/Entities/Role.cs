using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductTrackingAPI.Entities;

[Table("roles")]
public class Role
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}