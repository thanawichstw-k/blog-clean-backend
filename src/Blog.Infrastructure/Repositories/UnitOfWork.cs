using Blog.Application.Repositories;
using Blog.Infrastructure.Data;

namespace Blog.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BlogDbContext _db;
    public UnitOfWork(BlogDbContext db) => _db = db;
    public Task<int> CommitAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
