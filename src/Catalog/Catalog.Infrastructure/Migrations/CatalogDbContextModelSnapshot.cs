using System;
using Catalog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Catalog.Infrastructure.Migrations;

[DbContext(typeof(CatalogDbContext))]
partial class CatalogDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

        modelBuilder.Entity("Catalog.Domain.Products.Product", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<string>("Description")
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("character varying(1000)")
                .HasColumnName("description");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)")
                .HasColumnName("name");

            b.HasKey("Id")
                .HasName("pk_products");

            b.HasIndex("Name")
                .IsUnique()
                .HasDatabaseName("ix_products_name");

            b.ToTable("products");
        });
#pragma warning restore 612, 618
    }
}
