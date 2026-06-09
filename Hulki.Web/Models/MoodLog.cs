public class MoodLog
{
    public Guid Id { get; set; }
    public string AppUserId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public int MoodTypeId { get; set; }
    public virtual MoodType MoodType { get; set; }
}