using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelApp.Models
{
    public class Report
{
    [Key]
    public int Id { get; set; }

    public int StoryId { get; set; }
    public Story? Story { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; } 

    [Required]
    public string Reason { get; set; } = string.Empty; 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
}

}
