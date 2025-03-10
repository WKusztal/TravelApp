namespace TravelApp.Models
{
    public class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Story> Stories { get; set; } = new List<Story>();
    }
}
