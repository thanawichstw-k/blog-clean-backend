using Blog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Data.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.ArticleId)
            .HasColumnName("article_id")
            .IsRequired();

        builder.Property(x => x.Url)
            .HasColumnName("url")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Alt)
            .HasColumnName("alt");

        builder.Property(x => x.Caption)
            .HasColumnName("caption");

        builder.Property(x => x.Position)
            .HasColumnName("position")
            .IsRequired();

        builder.HasIndex(x => new { x.ArticleId, x.Position });

        builder.HasOne(x => x.Article)
            .WithMany(x => x.MediaAssets)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}