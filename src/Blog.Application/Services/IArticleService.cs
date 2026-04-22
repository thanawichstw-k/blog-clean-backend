using Blog.Application.DTOs;

namespace Blog.Application.Services;

public interface IArticleService
{
    Task<PagedResultDto<ContentCardDto>> QueryAsync(ContentQueryDto query, CancellationToken cancellationToken = default);
    Task<ContentDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentCardDto>> GetRelatedAsync(
    string slug,
    string mode = "default",
    int limit = 3,
    CancellationToken cancellationToken = default);
}