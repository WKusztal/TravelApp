using System;
using System.ComponentModel.DataAnnotations;

namespace TravelApp.Models
{
    public class Article
    {
        [Key]
        public int ArticleId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<ArticleImage> Images { get; set; } = new List<ArticleImage>();
        
        public Article()
        {
            Images = new List<ArticleImage>();
        }
    }
}
