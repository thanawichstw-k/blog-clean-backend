namespace Blog.Application.Repositories;

public interface ITagRepository
{
    Task<IEnumerable<string>> GetNamesByArticleIdAsync(long articleId, CancellationToken ct = default);
}
