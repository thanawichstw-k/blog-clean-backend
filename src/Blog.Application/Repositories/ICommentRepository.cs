using Blog.Domain.Entities;

namespace Blog.Application.Repositories;

public interface ICommentRepository
{
    Task<IEnumerable<Comment>> ListApprovedAsync(long articleId, CancellationToken ct = default);
    Task<long> AddAsync(Comment entity, CancellationToken ct = default);
    Task<bool> ArticleAllowsCommentsAsync(long articleId, CancellationToken ct = default);
}
