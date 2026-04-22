namespace Blog.Application.DTOs;

public class CommentDto
{
    public long Id { get; set; }
    public string DisplayName { get; set; } = default!;
    public string? Email { get; set; }
    public string Body { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}

public class CreateCommentDto
{
    public string DisplayName { get; set; } = default!;
    public string? Email { get; set; }
    public string Body { get; set; } = default!;
    public long? ParentId { get; set; }
}