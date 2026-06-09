public class Notification
{
    public Guid Id { get; set; }
    public string AppUserId { get; set; }
    public string Content { get; set; }
    public bool IsRead { get; set; }
}