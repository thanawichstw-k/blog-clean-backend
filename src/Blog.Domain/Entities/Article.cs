using System.Text.Json;

namespace Blog.Domain.Entities;

public class Article
{
    public long Id { get; private set; }

    public string LegacyId { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string? Excerpt { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? Badge { get; private set; }
    public string? DateText { get; private set; }

    public long? CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public long? AuthorId { get; private set; }

    public string ContentType { get; private set; } = "article";
    public string SourceType { get; private set; } = "blog";
    public string DisplayType { get; private set; } = "standard";

    public JsonDocument ContentJson { get; private set; } = JsonDocument.Parse("{}");
    public string Language { get; private set; } = "th";
    public int? ReadTime { get; private set; }

    public string Status { get; private set; } = "published";
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; private set; }

    public bool IsFeatured { get; private set; }
    public bool IsPinned { get; private set; }
    public bool IsSearchable { get; private set; } = true;

    public JsonDocument? SearchKeywordsJson { get; private set; }
    public string? SearchText { get; private set; }

    public int PriorityScore { get; private set; }
    public int PopularityScore { get; private set; }

    public int ViewCount { get; private set; }
    public int LikeCount { get; private set; }
    public int SaveCount { get; private set; }

    public ICollection<ArticleTag> ArticleTags { get; private set; } = new List<ArticleTag>();
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public ICollection<ArticleRelated> RelatedArticles { get; private set; } = new List<ArticleRelated>();
    public ICollection<MediaAsset> MediaAssets { get; private set; } = new List<MediaAsset>();

    private Article() { }

    public Article(
        string legacyId,
        string slug,
        string title,
        string? excerpt,
        string? imageUrl,
        string? badge,
        string? dateText,
        long? categoryId,
        long? authorId,
        string contentType,
        string sourceType,
        string displayType,
        JsonDocument contentJson,
        string language,
        int? readTime,
        string status,
        DateTimeOffset? publishedAt,
        DateTimeOffset? updatedAt,
        bool isFeatured,
        bool isPinned,
        bool isSearchable,
        JsonDocument? searchKeywordsJson,
        string? searchText,
        int priorityScore,
        int popularityScore,
        int viewCount,
        int likeCount,
        int saveCount)
    {
        LegacyId = legacyId;
        Slug = slug;
        Title = title;
        Excerpt = excerpt;
        ImageUrl = imageUrl;
        Badge = badge;
        DateText = dateText;
        CategoryId = categoryId;
        AuthorId = authorId;
        ContentType = contentType;
        SourceType = sourceType;
        DisplayType = displayType;
        ContentJson = contentJson;
        Language = language;
        ReadTime = readTime;
        Status = status;
        PublishedAt = publishedAt;
        UpdatedAt = updatedAt;
        IsFeatured = isFeatured;
        IsPinned = isPinned;
        IsSearchable = isSearchable;
        SearchKeywordsJson = searchKeywordsJson;
        SearchText = searchText;
        PriorityScore = priorityScore;
        PopularityScore = popularityScore;
        ViewCount = viewCount;
        LikeCount = likeCount;
        SaveCount = saveCount;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateBasicInfo(
        string title,
        string? excerpt,
        string? imageUrl,
        string? badge,
        string? dateText,
        long? categoryId,
        string contentType,
        string sourceType,
        string displayType,
        JsonDocument contentJson,
        string language,
        int? readTime,
        string status,
        DateTimeOffset? publishedAt,
        DateTimeOffset? updatedAt,
        bool isFeatured,
        bool isPinned,
        bool isSearchable,
        JsonDocument? searchKeywordsJson,
        string? searchText,
        int priorityScore,
        int popularityScore,
        int viewCount,
        int likeCount,
        int saveCount,
        long? authorId)
    {
        Title = title;
        Excerpt = excerpt;
        ImageUrl = imageUrl;
        Badge = badge;
        DateText = dateText;
        CategoryId = categoryId;
        ContentType = contentType;
        SourceType = sourceType;
        DisplayType = displayType;
        ContentJson = contentJson;
        Language = language;
        ReadTime = readTime;
        Status = status;
        PublishedAt = publishedAt;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
        IsFeatured = isFeatured;
        IsPinned = isPinned;
        IsSearchable = isSearchable;
        SearchKeywordsJson = searchKeywordsJson;
        SearchText = searchText;
        PriorityScore = priorityScore;
        PopularityScore = popularityScore;
        ViewCount = viewCount;
        LikeCount = likeCount;
        SaveCount = saveCount;
        AuthorId = authorId;
    }

    public void Publish()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("Article title is required.");

        if (ContentJson.RootElement.ValueKind == JsonValueKind.Undefined ||
            ContentJson.RootElement.ValueKind == JsonValueKind.Null)
            throw new InvalidOperationException("Article content is required.");

        Status = "published";
        PublishedAt ??= DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTimeOffset.UtcNow;
    }
}