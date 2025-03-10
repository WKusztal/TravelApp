using System.ComponentModel.DataAnnotations;

namespace TravelApp.Models
{
    public class StoryImage
    {
        [Key]
        public int Id { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public int StoryId { get; set; }
        public Story? Story { get; set; }
    }
}
