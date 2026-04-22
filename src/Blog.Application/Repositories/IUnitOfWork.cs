namespace Blog.Application.Repositories;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
