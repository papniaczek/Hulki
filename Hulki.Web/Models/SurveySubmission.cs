public class SurveySubmission
{
    public Guid Id { get; set; }
    public string AppUserId { get; set; }
    public Guid SurveyId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.Now;
}