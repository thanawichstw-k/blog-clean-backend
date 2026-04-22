using Blog.Application.Repositories;
using Blog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly BlogDbContext _db;
    public TagRepository(BlogDbContext db) => _db = db;

    public async Task<IEnumerable<string>> GetNamesByArticleIdAsync(long articleId, CancellationToken ct = default)
    {
        return await _db.ArticleTags.AsNoTracking().Where(at => at.ArticleId == articleId)
            .Join(_db.Tags.AsNoTracking(), at => at.TagId, t => t.Id, (at, t) => t.Name)
            .ToListAsync(ct);
    }
}
