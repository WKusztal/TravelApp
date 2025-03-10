using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelApp.Models
{
    public class Follow
    {
        [Key]
        public int Id { get; set; }

        public int FollowerId { get; set; } 
        public int FollowingId { get; set; } 

        [ForeignKey("FollowerId")]
        public User? Follower { get; set; } 

        [ForeignKey("FollowingId")]
        public User? Following { get; set; }
    }
}
