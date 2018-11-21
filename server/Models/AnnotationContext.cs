using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DocumentAnnotation.Models;

namespace DocumentAnnotation.Models
{
    public class AnnotationContext : IdentityDbContext<AppUser>
    {
        public DbSet<Annotation> Annotations { get; set; }
        public DbSet<Highlight> Highlights { get; set; }
        public DbSet<TextData> Texts { get; set; }
        public DbSet<Document> DocumentAnnotations { get; set; }
        public DbSet<LinkShortener> LinkShorteners { get; set; }

        public AnnotationContext()
        {
        }

        public AnnotationContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Highlight>(entity =>
            {
                entity.OwnsOne(h => h.Location);
                entity.Property(e => e.HighlightId).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<Annotation>(entity =>
            {
                entity.Property(e => e.AnnotationId).ValueGeneratedOnAdd();
                entity.HasMany(e => e.Highlights)
                    .WithOne(e => e.Annotation)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Document>(entity => { entity.OwnsOne(da => da.LastLocation); });
        }
    }
}