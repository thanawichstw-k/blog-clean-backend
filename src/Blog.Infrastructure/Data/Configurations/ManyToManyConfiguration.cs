using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Data.Configurations;

public class ManyToManyConfiguration :
    IEntityTypeConfiguration<ArticleTag>,
    IEntityTypeConfiguration<ArticleRelated>
{
    public void Configure(EntityTypeBuilder<ArticleTag> builder)
    {
        builder.ToTable("article_tags");

        builder.HasKey(x => new { x.ArticleId, x.TagId });

        builder.Property(x => x.ArticleId)
            .HasColumnName("article_id");

        builder.Property(x => x.TagId)
            .HasColumnName("tag_id");

        builder.HasOne(x => x.Article)
            .WithMany(x => x.ArticleTags)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.ArticleTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TagId);
    }

    public void Configure(EntityTypeBuilder<ArticleRelated> builder)
    {
        builder.ToTable("article_related");

        builder.HasKey(x => new { x.ArticleId, x.RelatedId });

        builder.Property(x => x.ArticleId)
            .HasColumnName("article_id");

        builder.Property(x => x.RelatedId)
            .HasColumnName("related_id");

        builder.Property(x => x.Position)
            .HasColumnName("position")
            .IsRequired();

        builder.HasOne(x => x.Article)
            .WithMany(x => x.RelatedArticles)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Related)
            .WithMany()
            .HasForeignKey(x => x.RelatedId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}