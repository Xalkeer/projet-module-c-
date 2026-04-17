using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DAL.Enums;

namespace DAL.Entities;

public class Task
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; }

    [Required]
    public Status Status { get; set; } = Status.ToDo;


    [ForeignKey("ProjectId")]
    public virtual Project Project { get; set; } = null!;
    public DateTime? DueDate { get; set; }
    public ICollection<String> Commentaires { get; set; } = new List<String>();
}