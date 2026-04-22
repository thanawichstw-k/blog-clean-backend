using Blog.Application.DTOs;
using Blog.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ContentCardDto>>> GetAll(
        [FromQuery] string? sourceType,
        [FromQuery] string? contentType,
        [FromQuery] string? category,
        [FromQuery] string? tag,
        [FromQuery] string? q,
        [FromQuery] string? displayType,
        [FromQuery] bool? featured,
        [FromQuery] bool? pinned,
        [FromQuery] string? sort = "latest",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var query = new ContentQueryDto
        {
            SourceType = sourceType,
            ContentType = contentType,
            Category = category,
            Tag = tag,
            Q = q,
            DisplayType = displayType,
            Featured = featured,
            Pinned = pinned,
            Sort = sort,
            Page = page,
            PageSize = pageSize
        };

        var result = await _articleService.QueryAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ContentDetailDto>> GetBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var result = await _articleService.GetBySlugAsync(slug, cancellationToken);
        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{slug}/related")]
    public async Task<ActionResult<IReadOnlyList<ContentCardDto>>> GetRelated(
    string slug,
    [FromQuery] string mode = "default",
    [FromQuery] int limit = 3,
    CancellationToken cancellationToken = default)
    {
        var result = await _articleService.GetRelatedAsync(slug, mode, limit, cancellationToken);
        return Ok(result);
    }
}