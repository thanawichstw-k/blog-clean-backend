using Blog.Application.DTOs;

namespace Blog.Application.Services;

public interface ISearchService
{
    Task<PagedResultDto<ContentCardDto>> SearchAsync(ContentQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SearchSuggestDto>> SuggestAsync(string q, int limit = 8, CancellationToken cancellationToken = default);
}