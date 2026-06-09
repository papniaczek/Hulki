public class TherapyGoal
{
    public Guid Id { get; set; }
    public string AppUserId { get; set; }
    public string Title { get; set; }
    public DateTime Deadline { get; set; }
    public bool IsCompleted { get; set; }
}