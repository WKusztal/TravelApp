using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelApp.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public virtual User User { get; set; } = null!;  

        [ForeignKey("Story")]
        public int StoryId { get; set; }

        [Required]
        public virtual Story Story { get; set; } = null!;

        public string UserName { get; set; } = string.Empty;
    }
}
