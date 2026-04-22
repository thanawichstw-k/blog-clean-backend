namespace Blog.Domain.Entities;

public class ArticleRelated
{
    public long ArticleId { get; private set; }
    public Article Article { get; private set; } = default!;

    public long RelatedId { get; private set; }
    public Article Related { get; private set; } = default!;

    public int Position { get; private set; }

    private ArticleRelated() { }

    public ArticleRelated(long articleId, long relatedId, int position = 0)
    {
        ArticleId = articleId;
        RelatedId = relatedId;
        Position = position;
    }
}