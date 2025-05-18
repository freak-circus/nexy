namespace Nexy.Data
{
    public class ModelProfile
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<ModelPost> Posts { get; set; } = new();
        public List<UserMessage> Messages { get; set; } = new();
        public string OnlyLink { get; set; } = string.Empty;
    }
}