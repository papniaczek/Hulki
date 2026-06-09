public class Survey
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public virtual ICollection<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();
}