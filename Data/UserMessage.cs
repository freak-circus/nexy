namespace Nexy.Data
{
    public class UserMessage
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid ModelId { get; set; }
        public ModelProfile Model { get; set; } = null!;
        public string MessageText { get; set; } = string.Empty;
        public bool IsFromUser { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}