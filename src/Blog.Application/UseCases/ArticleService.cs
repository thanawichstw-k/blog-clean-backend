using Blog.Application.DTOs;
using Blog.Application.Repositories;
using Blog.Application.Services;

namespace Blog.Application.UseCases;

public sealed class ArticleService : IArticleService
{
    private readonly IArticleRepository _articleRepository;

    public ArticleService(IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }

    public Task<PagedResultDto<ContentCardDto>> QueryAsync(ContentQueryDto query, CancellationToken cancellationToken = default)
        => _articleRepository.QueryAsync(query, cancellationToken);

    public Task<ContentDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => _articleRepository.GetBySlugAsync(slug, cancellationToken);

    public Task<IReadOnlyList<ContentCardDto>> GetRelatedAsync(
        string slug,
        string mode = "default",
        int limit = 3,
        CancellationToken cancellationToken = default)
        => _articleRepository.GetRelatedAsync(slug, mode, limit, cancellationToken);
}