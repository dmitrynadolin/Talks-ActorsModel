using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Talks.Data;

public class Measurement 
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id { get; set; } 
    public long MeterId { get; set; }
    public double Delta { get; set; }
    public double Value { get; set; }
    public DateTime Time { get; set; }
}