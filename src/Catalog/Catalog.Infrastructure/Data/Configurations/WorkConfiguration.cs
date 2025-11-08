using Catalog.Domain.Works;
using Catalog.Domain.Works.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Data.Configurations;

public class WorkConfiguration : IEntityTypeConfiguration<Work>
{
    public void Configure(EntityTypeBuilder<Work> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(w => w.AccessionNumber, an =>
        {
            an.Property(a => a.Value)
                .HasColumnName("AccessionNumber")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.OwnsOne(w => w.Title, title =>
        {
            title.Property(t => t.Value)
                .HasColumnName("Title")
                .IsRequired();
                
            title.OwnsMany(t => t.Translations, translation =>
            {
                translation.Property(tr => tr.Culture)
                    .HasMaxLength(10)
                    .IsRequired();
                translation.Property(tr => tr.Text)
                    .IsRequired();
            });
        });

        builder.OwnsOne(w => w.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .HasColumnName("Slug")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(w => w.Description, desc =>
        {
            desc.Property(d => d.Value)
                .HasColumnName("Description");
                
            desc.OwnsMany(d => d.Translations, translation =>
            {
                translation.Property(tr => tr.Culture)
                    .HasMaxLength(10)
                    .IsRequired();
                translation.Property(tr => tr.Text)
                    .IsRequired();
            });
        });

        builder.OwnsOne(w => w.Dimensions, dim =>
        {
            dim.Property(d => d.Height)
                .HasColumnName("Height")
                .HasColumnType("decimal(10,2)");
            dim.Property(d => d.Width)
                .HasColumnName("Width")
                .HasColumnType("decimal(10,2)");
            dim.Property(d => d.Depth)
                .HasColumnName("Depth")
                .HasColumnType("decimal(10,2)");
            dim.Property(d => d.Unit)
                .HasColumnName("MeasurementUnit")
                .HasMaxLength(20)
                .HasConversion<string>();
        });

        builder.HasMany(w => w.Assets)
            .WithOne()
            .HasForeignKey(a => a.WorkId);

        builder.Ignore(w => w.DomainEvents);
    }
}

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.WorkId)
            .IsRequired();

        builder.Property(a => a.FileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.OwnsOne(a => a.Caption, caption =>
        {
            caption.Property(c => c.Value)
                .HasColumnName("Caption");
                
            caption.OwnsMany(c => c.Translations, translation =>
            {
                translation.Property(tr => tr.Culture)
                    .HasMaxLength(10)
                    .IsRequired();
                translation.Property(tr => tr.Text)
                    .IsRequired();
            });
        });

        builder.Property(a => a.IsPrimary)
            .IsRequired();
    }
}