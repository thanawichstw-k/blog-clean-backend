using System.Text;
using System.Text.Json;
using Blog.Domain.Entities;
using Blog.Infrastructure.Data;
using Blog.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Seed;

public sealed class DbJsonImporter
{
    private readonly BlogDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;

    public DbJsonImporter(BlogDbContext dbContext, UserManager<AppUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task ImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            return;

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var root = JsonSerializer.Deserialize<DbJsonRoot>(json, options);
        if (root is null)
            return;

        var items = root.Articles
            .Concat(root.Products)
            .Concat(root.Services)
            .ToList();

        foreach (var item in items)
        {
            var authorUserId = await EnsureAuthorAsync(item.Author, cancellationToken);
            var categoryId = await EnsureCategoryAsync(item.Category, cancellationToken);
            var tagIds = await EnsureTagsAsync(item.Tags, cancellationToken);

            var contentJson = JsonDocument.Parse(item.Content.GetRawText());
            JsonDocument? searchKeywordsJson = null;

            if (item.SearchKeywords is { Count: > 0 })
            {
                searchKeywordsJson = JsonDocument.Parse(JsonSerializer.Serialize(item.SearchKeywords));
            }

            var existing = await _dbContext.Articles
                .Include(x => x.ArticleTags)
                .FirstOrDefaultAsync(x => x.LegacyId == item.Id, cancellationToken);

            if (existing is null)
            {
                var article = new Article(
                    legacyId: item.Id,
                    slug: item.Slug,
                    title: item.Title,
                    excerpt: item.Excerpt,
                    imageUrl: item.Image,
                    badge: item.Badge,
                    dateText: item.Date,
                    categoryId: categoryId,
                    authorId: authorUserId,
                    contentType: item.ContentType,
                    sourceType: item.SourceType,
                    displayType: item.DisplayType,
                    contentJson: contentJson,
                    language: item.Language,
                    readTime: item.ReadTime,
                    status: item.Status,
                    publishedAt: item.PublishedAt,
                    updatedAt: item.UpdatedAt,
                    isFeatured: item.Featured,
                    isPinned: item.Pinned,
                    isSearchable: item.Searchable,
                    searchKeywordsJson: searchKeywordsJson,
                    searchText: BuildSearchText(item),
                    priorityScore: item.PriorityScore,
                    popularityScore: item.PopularityScore,
                    viewCount: item.Stats?.Views ?? 0,
                    likeCount: item.Stats?.Likes ?? 0,
                    saveCount: item.Stats?.Saves ?? 0
                );

                await _dbContext.Articles.AddAsync(article, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await ReplaceTagsAsync(article.Id, tagIds, cancellationToken);
            }
            else
            {
                existing.UpdateBasicInfo(
                    title: item.Title,
                    excerpt: item.Excerpt,
                    imageUrl: item.Image,
                    badge: item.Badge,
                    dateText: item.Date,
                    categoryId: categoryId,
                    contentType: item.ContentType,
                    sourceType: item.SourceType,
                    displayType: item.DisplayType,
                    contentJson: contentJson,
                    language: item.Language,
                    readTime: item.ReadTime,
                    status: item.Status,
                    publishedAt: item.PublishedAt,
                    updatedAt: item.UpdatedAt,
                    isFeatured: item.Featured,
                    isPinned: item.Pinned,
                    isSearchable: item.Searchable,
                    searchKeywordsJson: searchKeywordsJson,
                    searchText: BuildSearchText(item),
                    priorityScore: item.PriorityScore,
                    popularityScore: item.PopularityScore,
                    viewCount: item.Stats?.Views ?? 0,
                    likeCount: item.Stats?.Likes ?? 0,
                    saveCount: item.Stats?.Saves ?? 0,
                    authorId: authorUserId
                );

                await _dbContext.SaveChangesAsync(cancellationToken);
                await ReplaceTagsAsync(existing.Id, tagIds, cancellationToken);
            }
        }
    }

    private async Task<long?> EnsureAuthorAsync(ImportAuthor? author, CancellationToken cancellationToken)
    {
        if (author is null || string.IsNullOrWhiteSpace(author.Slug))
            return null;

        var existing = await _userManager.Users
            .FirstOrDefaultAsync(x => x.UserName == author.Slug, cancellationToken);

        if (existing is not null)
            return existing.Id;

        var user = new AppUser
        {
            UserName = author.Slug,
            Email = $"{author.Slug}@local.test",
            EmailConfirmed = true,
            DisplayName = author.Name,
            Slug = author.Slug
        };

        var result = await _userManager.CreateAsync(user, "TempPassword#123");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Cannot create import author user: " +
                string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        return user.Id;
    }

    private async Task<long?> EnsureCategoryAsync(string? categorySlug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(categorySlug))
            return null;

        var existing = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Slug == categorySlug, cancellationToken);
        if (existing is not null)
            return existing.Id;

        var category = new Category(categorySlug, categorySlug);
        await _dbContext.Categories.AddAsync(category, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return category.Id;
    }

    private async Task<List<long>> EnsureTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken)
    {
        var result = new List<long>();

        foreach (var raw in tags.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var slug = Slugify(raw);
            var existing = await _dbContext.Tags.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

            if (existing is null)
            {
                existing = new Tag(raw, slug);
                await _dbContext.Tags.AddAsync(existing, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            result.Add(existing.Id);
        }

        return result;
    }

    private async Task ReplaceTagsAsync(long articleId, IReadOnlyCollection<long> tagIds, CancellationToken cancellationToken)
    {
        var oldTags = await _dbContext.ArticleTags.Where(x => x.ArticleId == articleId).ToListAsync(cancellationToken);
        _dbContext.ArticleTags.RemoveRange(oldTags);

        foreach (var tagId in tagIds.Distinct())
        {
            await _dbContext.ArticleTags.AddAsync(new ArticleTag(articleId, tagId), cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string BuildSearchText(ImportContentItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.SearchText))
            return item.SearchText;

        var parts = new List<string?>
        {
            item.Title,
            item.Excerpt,
            item.Category,
            item.Badge,
            string.Join(" ", item.Tags),
            string.Join(" ", item.SearchKeywords),
        };

        if (item.Content.ValueKind != JsonValueKind.Undefined &&
            item.Content.ValueKind != JsonValueKind.Null &&
            item.Content.TryGetProperty("blocks", out var blocks) &&
            blocks.ValueKind == JsonValueKind.Array)
        {
            foreach (var block in blocks.EnumerateArray())
            {
                if (!block.TryGetProperty("data", out var data)) continue;

                if (data.TryGetProperty("text", out var textProp))
                    parts.Add(textProp.GetString());

                if (data.TryGetProperty("caption", out var captionProp))
                    parts.Add(captionProp.GetString());

                if (data.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var itemNode in itemsProp.EnumerateArray())
                    {
                        parts.Add(itemNode.GetString());
                    }
                }
            }
        }

        return string.Join(" ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string Slugify(string value)
    {
        var text = value.Trim().ToLowerInvariant();
        var sb = new StringBuilder(text.Length);

        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
            }
            else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_' || ch == '/')
            {
                sb.Append('-');
            }
        }

        return sb.ToString().Trim('-');
    }
}