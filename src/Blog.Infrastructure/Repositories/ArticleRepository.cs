using System.Text.Json;
using Blog.Application.DTOs;
using Blog.Application.Repositories;
using Blog.Domain.Entities;
using Blog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public sealed class ArticleRepository : IArticleRepository
{
    private readonly BlogDbContext _dbContext;

    public ArticleRepository(BlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
        => _dbContext.Articles.AnyAsync(x => x.Slug == slug, cancellationToken);

    public Task<Article?> GetEntityBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => _dbContext.Articles
            .Include(x => x.ArticleTags)
            .ThenInclude(x => x.Tag)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<Article?> GetEntityByLegacyIdAsync(string legacyId, CancellationToken cancellationToken = default)
        => _dbContext.Articles.FirstOrDefaultAsync(x => x.LegacyId == legacyId, cancellationToken);

    public async Task AddAsync(Article article, CancellationToken cancellationToken = default)
        => await _dbContext.Articles.AddAsync(article, cancellationToken);

    public Task UpdateAsync(Article article, CancellationToken cancellationToken = default)
    {
        _dbContext.Articles.Update(article);
        return Task.CompletedTask;
    }

    public Task<Category?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => _dbContext.Categories.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public async Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default)
        => await _dbContext.Categories.AddAsync(category, cancellationToken);

    public Task<Tag?> GetTagBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => _dbContext.Tags.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public async Task AddTagAsync(Tag tag, CancellationToken cancellationToken = default)
        => await _dbContext.Tags.AddAsync(tag, cancellationToken);

    public async Task ReplaceTagsAsync(long articleId, IReadOnlyCollection<long> tagIds, CancellationToken cancellationToken = default)
    {
        var oldItems = await _dbContext.ArticleTags
            .Where(x => x.ArticleId == articleId)
            .ToListAsync(cancellationToken);

        _dbContext.ArticleTags.RemoveRange(oldItems);

        foreach (var tagId in tagIds.Distinct())
        {
            await _dbContext.ArticleTags.AddAsync(new ArticleTag(articleId, tagId), cancellationToken);
        }
    }

    public async Task<PagedResultDto<ContentCardDto>> QueryAsync(ContentQueryDto query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 12 : query.PageSize;
        var sort = (query.Sort ?? "latest").Trim().ToLowerInvariant();

        var dbQuery = _dbContext.Articles
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .Where(x => x.DeletedAt == null)
            .Where(x => x.Status == "published");

        if (!string.IsNullOrWhiteSpace(query.SourceType))
            dbQuery = dbQuery.Where(x => x.SourceType == query.SourceType);

        if (!string.IsNullOrWhiteSpace(query.ContentType))
            dbQuery = dbQuery.Where(x => x.ContentType == query.ContentType);

        if (!string.IsNullOrWhiteSpace(query.Category))
            dbQuery = dbQuery.Where(x => x.Category != null && x.Category.Slug == query.Category);

        if (!string.IsNullOrWhiteSpace(query.Tag))
            dbQuery = dbQuery.Where(x => x.ArticleTags.Any(t =>
                t.Tag.Slug == query.Tag || t.Tag.Name == query.Tag));

        if (!string.IsNullOrWhiteSpace(query.DisplayType))
            dbQuery = dbQuery.Where(x => x.DisplayType == query.DisplayType);

        if (query.Featured.HasValue)
            dbQuery = dbQuery.Where(x => x.IsFeatured == query.Featured.Value);

        if (query.Pinned.HasValue)
            dbQuery = dbQuery.Where(x => x.IsPinned == query.Pinned.Value);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var q = $"%{query.Q.Trim()}%";

            dbQuery = dbQuery.Where(x =>
                EF.Functions.ILike(x.Title, q) ||
                (x.Excerpt != null && EF.Functions.ILike(x.Excerpt, q)) ||
                (x.SearchText != null && EF.Functions.ILike(x.SearchText, q)) ||
                x.ArticleTags.Any(t =>
                    EF.Functions.ILike(t.Tag.Name, q) ||
                    EF.Functions.ILike(t.Tag.Slug, q)) ||
                (x.Category != null && EF.Functions.ILike(x.Category.Name, q)));
        }

        dbQuery = sort switch
        {
            "popular" => dbQuery.OrderByDescending(x => x.PopularityScore)
                                .ThenByDescending(x => x.PublishedAt)
                                .ThenByDescending(x => x.CreatedAt),

            "priority" => dbQuery.OrderByDescending(x => x.PriorityScore)
                                 .ThenByDescending(x => x.PublishedAt)
                                 .ThenByDescending(x => x.CreatedAt),

            "oldest" => dbQuery.OrderBy(x => x.PublishedAt)
                               .ThenBy(x => x.CreatedAt),

            "newest" or "latest" => dbQuery.OrderByDescending(x => x.PublishedAt)
                                           .ThenByDescending(x => x.CreatedAt),

            _ => dbQuery.OrderByDescending(x => x.PublishedAt)
                        .ThenByDescending(x => x.CreatedAt)
        };

        var total = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ContentCardDto>
        {
            Items = items.Select(MapCard).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<ContentDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Articles
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.DeletedAt == null, cancellationToken);

        if (entity is null)
            return null;

        return MapDetail(entity);
    }

    public async Task<IReadOnlyList<ContentCardDto>> GetRelatedAsync(
        string slug,
        string mode = "default",
        int limit = 3,
        CancellationToken cancellationToken = default)
    {
        var current = await _dbContext.Articles
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.DeletedAt == null, cancellationToken);

        if (current is null)
            return Array.Empty<ContentCardDto>();

        var currentTagIds = current.ArticleTags.Select(x => x.TagId).ToList();
        var normalizedMode = (mode ?? "default").Trim().ToLowerInvariant();

        var candidatesQuery = _dbContext.Articles
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .Where(x =>
                x.Id != current.Id &&
                x.DeletedAt == null &&
                x.SourceType == current.SourceType &&
                x.Status == "published");

        if (normalizedMode == "highlight")
        {
            candidatesQuery = candidatesQuery.Where(x => x.DisplayType == "highlight");
        }
        else if (normalizedMode == "featured")
        {
            candidatesQuery = candidatesQuery.Where(x => x.DisplayType == "featured");
        }

        var candidates = await candidatesQuery.ToListAsync(cancellationToken);

        var scored = candidates
            .Select(item =>
            {
                var score = 0;

                if (item.CategoryId == current.CategoryId)
                    score += 3;

                var sharedTags = item.ArticleTags.Count(t => currentTagIds.Contains(t.TagId));
                score += sharedTags * 2;

                if (normalizedMode == "default" && item.DisplayType == current.DisplayType)
                    score += 1;

                score += item.PriorityScore / 10;
                score += item.PopularityScore / 10;

                return new
                {
                    Item = item,
                    Score = score
                };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Item.PublishedAt)
            .Take(limit)
            .Select(x => MapCard(x.Item))
            .ToList();

        return scored;
    }

    public async Task<IReadOnlyList<SearchSuggestDto>> SuggestAsync(string q, int limit = 8, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Array.Empty<SearchSuggestDto>();

        var query = $"%{q.Trim()}%";

        var items = await _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.DeletedAt == null && x.IsSearchable)
            .Where(x =>
                EF.Functions.ILike(x.Title, query) ||
                EF.Functions.ILike(x.Slug, query))
            .OrderByDescending(x => x.PriorityScore)
            .ThenByDescending(x => x.PopularityScore)
            .Take(limit)
            .Select(x => new SearchSuggestDto
            {
                Slug = x.Slug,
                Title = x.Title,
                SourceType = x.SourceType,
                Image = x.ImageUrl
            })
            .ToListAsync(cancellationToken);

        return items;
    }

    private static ContentCardDto MapCard(Article x)
    {
        return new ContentCardDto
        {
            Id = x.LegacyId,
            Slug = x.Slug,
            Title = x.Title,
            Excerpt = x.Excerpt,
            Date = x.DateText,
            Badge = x.Badge,
            ReadTime = x.ReadTime,
            Image = x.ImageUrl,
            Category = x.Category?.Slug,
            Tags = x.ArticleTags
                .Where(t => t.Tag != null)
                .Select(t => t.Tag.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList(),
            Content = null, // list endpoint ไม่ต้อง deserialize content ทั้งก้อน
            ContentType = x.ContentType,
            SourceType = x.SourceType,
            DisplayType = x.DisplayType,
            PublishedAt = x.PublishedAt,
            UpdatedAt = x.UpdatedAt,
            Status = x.Status,
            Language = x.Language,
            Author = null,
            Featured = x.IsFeatured,
            Pinned = x.IsPinned,
            Searchable = x.IsSearchable,
            SearchKeywords = DeserializeStringList(x.SearchKeywordsJson),
            SearchText = x.SearchText,
            PriorityScore = x.PriorityScore,
            PopularityScore = x.PopularityScore,
            Stats = new StatsDto
            {
                Views = x.ViewCount,
                Likes = x.LikeCount,
                Saves = x.SaveCount
            }
        };
    }

    private static ContentDetailDto MapDetail(Article x)
    {
        return new ContentDetailDto
        {
            Id = x.LegacyId,
            Slug = x.Slug,
            Title = x.Title,
            Excerpt = x.Excerpt,
            Date = x.DateText,
            Badge = x.Badge,
            ReadTime = x.ReadTime,
            Image = x.ImageUrl,
            Category = x.Category?.Slug,
            Tags = x.ArticleTags
                .Where(t => t.Tag != null)
                .Select(t => t.Tag.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList(),
            Content = DeserializeJsonElement(x.ContentJson),
            ContentType = x.ContentType,
            SourceType = x.SourceType,
            DisplayType = x.DisplayType,
            PublishedAt = x.PublishedAt,
            UpdatedAt = x.UpdatedAt,
            Status = x.Status,
            Language = x.Language,
            Author = null,
            Featured = x.IsFeatured,
            Pinned = x.IsPinned,
            Searchable = x.IsSearchable,
            SearchKeywords = DeserializeStringList(x.SearchKeywordsJson),
            SearchText = x.SearchText,
            PriorityScore = x.PriorityScore,
            PopularityScore = x.PopularityScore,
            Stats = new StatsDto
            {
                Views = x.ViewCount,
                Likes = x.LikeCount,
                Saves = x.SaveCount
            }
        };
    }

    private static object? DeserializeJsonElement(JsonDocument? jsonDocument)
    {
        if (jsonDocument is null)
            return null;

        try
        {
            return JsonSerializer.Deserialize<object>(jsonDocument.RootElement.GetRawText());
        }
        catch
        {
            return null;
        }
    }

    private static List<string> DeserializeStringList(JsonDocument? jsonDocument)
    {
        if (jsonDocument is null)
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(jsonDocument.RootElement.GetRawText()) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}