using ProductTrackingAPI.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("can_access_web")]
    public bool CanAccessWeb { get; set; }

    [Column("can_access_scanner")]
    public bool CanAccessScanner { get; set; }

    public ICollection<UserRole> UserRoles { get; set; }
        = new List<UserRole>();
}