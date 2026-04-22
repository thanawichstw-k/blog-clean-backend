using System.Text.Json;

namespace Blog.Infrastructure.Seed;

public sealed class DbJsonRoot
{
    public List<ImportContentItem> Products { get; set; } = new();
    public List<ImportContentItem> Services { get; set; } = new();
    public List<ImportContentItem> Articles { get; set; } = new();
}

public sealed class ImportContentItem
{
    public string Id { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Excerpt { get; set; }
    public string? Date { get; set; }
    public string? Badge { get; set; }
    public int? ReadTime { get; set; }
    public string? Image { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public JsonElement Content { get; set; }
    public string ContentType { get; set; } = "article";
    public string SourceType { get; set; } = "blog";
    public string DisplayType { get; set; } = "standard";
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string Status { get; set; } = "published";
    public string Language { get; set; } = "th";
    public ImportAuthor? Author { get; set; }
    public bool Featured { get; set; }
    public bool Pinned { get; set; }
    public bool Searchable { get; set; } = true;
    public List<string> SearchKeywords { get; set; } = new();
    public string? SearchText { get; set; }
    public int PriorityScore { get; set; }
    public int PopularityScore { get; set; }
    public ImportStats? Stats { get; set; }
}

public sealed class ImportAuthor
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
}

public sealed class ImportStats
{
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Saves { get; set; }
}