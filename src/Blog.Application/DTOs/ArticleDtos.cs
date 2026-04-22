namespace Blog.Application.DTOs;

public class AuthorDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
}

public class StatsDto
{
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Saves { get; set; }
}

public class ContentCardDto
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

    public object? Content { get; set; }

    public string ContentType { get; set; } = default!;
    public string SourceType { get; set; } = default!;
    public string DisplayType { get; set; } = default!;

    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string Status { get; set; } = default!;
    public string Language { get; set; } = "th";

    public AuthorDto? Author { get; set; }

    public bool Featured { get; set; }
    public bool Pinned { get; set; }
    public bool Searchable { get; set; }

    public List<string> SearchKeywords { get; set; } = new();
    public string? SearchText { get; set; }

    public int PriorityScore { get; set; }
    public int PopularityScore { get; set; }

    public StatsDto Stats { get; set; } = new();
}

public class ContentDetailDto : ContentCardDto
{
}

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}

public class ContentQueryDto
{
    public string? SourceType { get; set; }
    public string? ContentType { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public string? Q { get; set; }
    public string? DisplayType { get; set; }
    public bool? Featured { get; set; }
    public bool? Pinned { get; set; }
    public string? Sort { get; set; } = "latest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class SearchSuggestDto
{
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string SourceType { get; set; } = default!;
    public string? Image { get; set; }
}