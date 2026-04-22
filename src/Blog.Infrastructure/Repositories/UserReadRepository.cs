using Blog.Application.Repositories;
using Blog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class UserReadRepository : IUserReadRepository
{
    private readonly BlogDbContext _db;
    public UserReadRepository(BlogDbContext db) => _db = db;

    public Task<string?> GetDisplayNameAsync(long userId, CancellationToken ct = default)
        => _db.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.DisplayName).FirstOrDefaultAsync(ct);
}
