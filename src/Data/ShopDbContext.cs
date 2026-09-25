using ElectronicsShop.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsShop.Data
{
    public partial class ShopDbContext : DbContext
    {
        public ShopDbContext()
        {
        }
        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(e => e.Id).HasName("PK_categories");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_categories_name");
            });
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.ToTable("brands");
                entity.HasKey(e => e.Id).HasName("PK_brands");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_brands_name");
            });
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("tags");
                entity.HasKey(e => e.Id).HasName("PK_tags");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_tags_name");
            });
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.Id).HasName("PK_products");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(2000).IsRequired();
                entity.Property(e => e.Price).HasColumnName("price").HasPrecision(12, 2);
                entity.Property(e => e.Stock).HasColumnName("stock");
                entity.Property(e => e.Rating).HasColumnName("rating").HasPrecision(2, 1);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("date");
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.BrandId).HasColumnName("brand_id");
                entity.HasIndex(e => e.CategoryId).HasDatabaseName("IX_products_category_id");
                entity.HasIndex(e => e.BrandId).HasDatabaseName("IX_products_brand_id");
                entity.HasOne(d => d.Category).WithMany(p => p.Products).HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_products_categories");
                entity.HasOne(d => d.Brand).WithMany(p => p.Products).HasForeignKey(d => d.BrandId)
                    .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_products_brands");
                entity.HasMany(d => d.Tags).WithMany(p => p.Products)
                    .UsingEntity<Dictionary<string, object>>("ProductTag",
                        r => r.HasOne<Tag>().WithMany().HasForeignKey("TagId").HasConstraintName("FK_product_tags_tags"),
                        l => l.HasOne<Product>().WithMany().HasForeignKey("ProductId").HasConstraintName("FK_product_tags_products"),
                        j =>
                        {
                            j.ToTable("product_tags");
                            j.HasKey("ProductId", "TagId").HasName("PK_product_tags");
                            j.IndexerProperty<int>("ProductId").HasColumnName("product_id");
                            j.IndexerProperty<int>("TagId").HasColumnName("tag_id");
                            j.HasIndex(new[] { "TagId" }).HasDatabaseName("IX_product_tags_tag_id");
                        });
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
