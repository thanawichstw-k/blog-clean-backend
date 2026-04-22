using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Data.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.LegacyId)
            .HasColumnName("legacy_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.LegacyId)
            .IsUnique();

        builder.Property(x => x.Slug)
            .HasColumnName("slug")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Excerpt)
            .HasColumnName("excerpt");

        builder.Property(x => x.ImageUrl)
            .HasColumnName("image_url");

        builder.Property(x => x.Badge)
            .HasColumnName("badge")
            .HasMaxLength(100);

        builder.Property(x => x.DateText)
            .HasColumnName("date_text")
            .HasMaxLength(100);

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id");

        builder.Property(x => x.AuthorId)
            .HasColumnName("author_id");

        builder.Property(x => x.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SourceType)
            .HasColumnName("source_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DisplayType)
            .HasColumnName("display_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ContentJson)
            .HasColumnName("content_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Language)
            .HasColumnName("language")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.ReadTime)
            .HasColumnName("read_time");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(x => x.IsFeatured)
            .HasColumnName("is_featured")
            .IsRequired();

        builder.Property(x => x.IsPinned)
            .HasColumnName("is_pinned")
            .IsRequired();

        builder.Property(x => x.IsSearchable)
            .HasColumnName("is_searchable")
            .IsRequired();

        builder.Property(x => x.SearchKeywordsJson)
            .HasColumnName("search_keywords_json")
            .HasColumnType("jsonb");

        builder.Property(x => x.SearchText)
            .HasColumnName("search_text");

        builder.Property(x => x.PriorityScore)
            .HasColumnName("priority_score")
            .IsRequired();

        builder.Property(x => x.PopularityScore)
            .HasColumnName("popularity_score")
            .IsRequired();

        builder.Property(x => x.ViewCount)
            .HasColumnName("view_count")
            .IsRequired();

        builder.Property(x => x.LikeCount)
            .HasColumnName("like_count")
            .IsRequired();

        builder.Property(x => x.SaveCount)
            .HasColumnName("save_count")
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Articles)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Comments)
            .WithOne(x => x.Article)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.MediaAssets)
            .WithOne(x => x.Article)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SourceType, x.Status, x.PublishedAt });
        builder.HasIndex(x => new { x.CategoryId, x.PublishedAt });
        builder.HasIndex(x => new { x.DisplayType, x.PublishedAt });
        builder.HasIndex(x => new { x.IsFeatured, x.PublishedAt });
        builder.HasIndex(x => new { x.IsPinned, x.PublishedAt });
    }
}