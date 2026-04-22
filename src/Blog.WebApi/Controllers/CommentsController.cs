using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    [HttpGet("{slug}")]
    public IActionResult GetByArticleSlug(string slug)
    {
        return Ok(Array.Empty<object>());
    }

    [HttpPost("{slug}")]
    public IActionResult Create(string slug)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            message = "Comment API is not implemented yet."
        });
    }
}