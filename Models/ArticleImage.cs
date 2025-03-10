using System.ComponentModel.DataAnnotations;

namespace TravelApp.Models
{
    public class ArticleImage
    {
        [Key]
        public int Id { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public int ArticleId { get; set; }
        public Article? Article { get; set; }
    }
}
