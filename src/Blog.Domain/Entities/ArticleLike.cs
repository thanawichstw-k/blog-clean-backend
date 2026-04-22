namespace Blog.Domain.Entities;

public class ArticleLike
{
    public long Id { get; set; }
    public long ArticleId { get; set; }
    public long? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
