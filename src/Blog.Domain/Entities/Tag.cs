namespace Blog.Domain.Entities;

public class Tag
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;

    public ICollection<ArticleTag> ArticleTags { get; private set; } = new List<ArticleTag>();

    private Tag() { }

    public Tag(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }

    public void Rename(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }
}