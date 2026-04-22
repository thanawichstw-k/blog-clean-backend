namespace Blog.Domain.Entities;

public class Comment
{
    public long Id { get; private set; }
    public long ArticleId { get; private set; }
    public Article Article { get; private set; } = default!;

    public long? UserId { get; private set; }
    public long? ParentId { get; private set; }
    public Comment? Parent { get; private set; }

    public string DisplayName { get; private set; } = default!;
    public string? Email { get; private set; }
    public string Body { get; private set; } = default!;
    public string Status { get; private set; } = "approved";
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    public ICollection<Comment> Replies { get; private set; } = new List<Comment>();

    private Comment() { }

    public Comment(long articleId, long? userId, long? parentId, string displayName, string? email, string body)
    {
        ArticleId = articleId;
        UserId = userId;
        ParentId = parentId;
        DisplayName = displayName;
        Email = email;
        Body = body;
    }
}