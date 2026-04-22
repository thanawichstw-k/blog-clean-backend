using Blog.Application.DTOs;
using Blog.Domain.Entities;

namespace Blog.Application.Repositories;

public interface IArticleRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<Article?> GetEntityBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Article?> GetEntityByLegacyIdAsync(string legacyId, CancellationToken cancellationToken = default);

    Task AddAsync(Article article, CancellationToken cancellationToken = default);
    Task UpdateAsync(Article article, CancellationToken cancellationToken = default);

    Task<Category?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default);

    Task<Tag?> GetTagBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task AddTagAsync(Tag tag, CancellationToken cancellationToken = default);

    Task ReplaceTagsAsync(long articleId, IReadOnlyCollection<long> tagIds, CancellationToken cancellationToken = default);

    Task<PagedResultDto<ContentCardDto>> QueryAsync(ContentQueryDto query, CancellationToken cancellationToken = default);
    Task<ContentDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentCardDto>> GetRelatedAsync(
    string slug,
    string mode = "default",
    int limit = 3,
    CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SearchSuggestDto>> SuggestAsync(string q, int limit = 8, CancellationToken cancellationToken = default);
}