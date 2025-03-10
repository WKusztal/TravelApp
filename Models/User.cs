using System;

namespace TravelApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string Role { get; set; } = "User";
        public string? AvatarPath { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }

        public ICollection<Follow> Followers { get; set; } = new List<Follow>();
        public ICollection<Follow> Following { get; set; } = new List<Follow>();

        public ICollection<User> FollowersUsers => Followers.Select(f => f.Follower!).ToList();
        public ICollection<User> FollowingUsers => Following.Select(f => f.Following!).ToList();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}