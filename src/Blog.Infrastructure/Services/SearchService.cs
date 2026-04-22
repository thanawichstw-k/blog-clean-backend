using Blog.Application.DTOs;
using Blog.Application.Repositories;
using Blog.Application.Services;

namespace Blog.Infrastructure.Services;

public sealed class SearchService : ISearchService
{
    private readonly IArticleRepository _articleRepository;

    public SearchService(IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }

    public Task<PagedResultDto<ContentCardDto>> SearchAsync(
        ContentQueryDto query,
        CancellationToken cancellationToken = default)
    {
        return _articleRepository.QueryAsync(query, cancellationToken);
    }

    public Task<IReadOnlyList<SearchSuggestDto>> SuggestAsync(
        string q,
        int limit = 8,
        CancellationToken cancellationToken = default)
    {
        return _articleRepository.SuggestAsync(q, limit, cancellationToken);
    }
}