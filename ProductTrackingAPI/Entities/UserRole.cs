using System.ComponentModel.DataAnnotations.Schema;

namespace ProductTrackingAPI.Entities;

[Table("user_roles")]
public class UserRole
{
    [Column("user_id")]
    public long UserId { get; set; }

    [Column("role_id")]
    public long RoleId { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}