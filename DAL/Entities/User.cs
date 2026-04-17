using System.ComponentModel.DataAnnotations;
using DAL.Enums;

namespace DAL.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    public Role Role { get; set; } = Role.User;
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}