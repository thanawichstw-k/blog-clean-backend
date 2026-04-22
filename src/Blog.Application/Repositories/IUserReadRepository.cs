namespace Blog.Application.Repositories;

public interface IUserReadRepository
{
    Task<string?> GetDisplayNameAsync(long userId, CancellationToken ct = default);
}
