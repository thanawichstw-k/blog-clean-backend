using Blog.Application.Repositories;
using Blog.Application.Services;
using Blog.Application.UseCases;
using Blog.Infrastructure.Repositories;
using Blog.Infrastructure.Seed;
using Blog.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Blog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<ISearchService, SearchService>();

        // ยังไม่ register comment flow ตอนนี้
        // เพราะกำลังใช้ placeholder อยู่
        // เดี๋ยวค่อยใส่กลับตอนทำ comment จริง

        services.AddScoped<DbJsonImporter>();

        return services;
    }
}