namespace Blog.Domain.Entities;

public class Category
{
    public long Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public long? ParentId { get; private set; }
    public Category? Parent { get; private set; }

    public ICollection<Article> Articles { get; private set; } = new List<Article>();

    private Category() { }

    public Category(string name, string slug, long? parentId = null)
    {
        Name = name;
        Slug = slug;
        ParentId = parentId;
    }

    public void Rename(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }
}