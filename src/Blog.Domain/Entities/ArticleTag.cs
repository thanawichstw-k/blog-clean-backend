namespace Blog.Domain.Entities;

public class ArticleTag
{
    public long ArticleId { get; private set; }
    public Article Article { get; private set; } = default!;

    public long TagId { get; private set; }
    public Tag Tag { get; private set; } = default!;

    private ArticleTag() { }

    public ArticleTag(long articleId, long tagId)
    {
        ArticleId = articleId;
        TagId = tagId;
    }
}