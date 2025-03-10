using System;

namespace TravelApp.Models
{
    public class Reaction
    {
        public int ReactionId { get; set; }
        public int StoryId { get; set; }
        public string? UserName { get; set; }
        public bool IsLike { get; set; }
    }

}