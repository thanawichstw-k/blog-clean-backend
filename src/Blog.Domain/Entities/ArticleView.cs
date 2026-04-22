namespace Blog.Domain.Entities;

public class ArticleView
{
    public long Id { get; set; }
    public long ArticleId { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    public string? ViewerKey { get; set; }
}
