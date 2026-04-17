using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities;

public class Project
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreationDate { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}