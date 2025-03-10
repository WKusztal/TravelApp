using System;

namespace TravelApp.Models
{
    public class Story
    {
        public int StoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public string Continent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public User? Author { get; set; }

        public ICollection<StoryImage>? Images { get; set; }

        public Story()
        {
            Images = new List<StoryImage>();
        }

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        
        public ICollection<Report>? Reports { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    }
}
