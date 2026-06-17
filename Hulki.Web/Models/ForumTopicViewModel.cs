namespace Hulki.Web.Models;

public class ForumTopicViewModel
{
    public ForumTopic Topic { get; set; } = null!;
    public PagedResult<ForumPost> Posts { get; set; } = null!;
}