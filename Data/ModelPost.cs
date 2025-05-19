namespace Nexy.Data
{
    public class ModelPost
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public ModelProfile Model { get; set; } = null!;
        public string ImageUrl { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; } = new Random().Next(500, 10000);
        public List<Guid>? RealLikes { get; set; }
        public bool IsNsfw { get; set; }
    }
}