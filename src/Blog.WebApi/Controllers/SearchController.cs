using Blog.Application.DTOs;
using Blog.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ContentCardDto>>> Search(
        [FromQuery] string q,
        [FromQuery] string? sourceType,
        [FromQuery] string? contentType,
        [FromQuery] string? category,
        [FromQuery] string? tag,
        [FromQuery] string? sort = "latest",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var query = new ContentQueryDto
        {
            Q = q,
            SourceType = sourceType,
            ContentType = contentType,
            Category = category,
            Tag = tag,
            Sort = sort,
            Page = page,
            PageSize = pageSize
        };

        var result = await _searchService.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("suggest")]
    public async Task<ActionResult<IReadOnlyList<SearchSuggestDto>>> Suggest(
        [FromQuery] string q,
        [FromQuery] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var result = await _searchService.SuggestAsync(q, limit, cancellationToken);
        return Ok(result);
    }
}