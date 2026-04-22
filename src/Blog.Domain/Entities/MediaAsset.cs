namespace Blog.Domain.Entities;

public class MediaAsset
{
    public long Id { get; private set; }
    public long ArticleId { get; private set; }
    public Article Article { get; private set; } = default!;
    public string Url { get; private set; } = default!;
    public string Type { get; private set; } = "image";
    public string? Alt { get; private set; }
    public string? Caption { get; private set; }
    public int Position { get; private set; }

    private MediaAsset() { }

    public MediaAsset(
        long articleId,
        string url,
        string type = "image",
        string? alt = null,
        string? caption = null,
        int position = 0)
    {
        ArticleId = articleId;
        Url = url;
        Type = type;
        Alt = alt;
        Caption = caption;
        Position = position;
    }
}