using Microsoft.AspNetCore.Identity;

namespace Blog.Infrastructure.Identity;

public class AppUser : IdentityUser<long>
{
    public string DisplayName { get; set; } = default!;
    public string? Slug { get; set; }
}