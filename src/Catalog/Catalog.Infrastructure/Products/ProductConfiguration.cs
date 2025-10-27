namespace Catalog.Infrastructure.Products;

using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id)
            .HasName("pk_products");

        builder.Property(product => product.Id)
            .ValueGeneratedNever();

        builder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(product => product.Name)
            .IsUnique()
            .HasDatabaseName("ix_products_name");

        builder.Property(product => product.Description)
            .IsRequired()
            .HasMaxLength(1000);
    }
}
