using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace server.Models
{
    public class AnnotationContext: IdentityDbContext<AppUser>
    {
        public DbSet<Annotation> Annotations { get; set; }
        public DbSet<Highlight> Highlights { get; set; }
        public DbSet<TextData> Texts { get; set; }
        public DbSet<DocumentAnnotation> DocumentAnnotations { get; set; }

        public AnnotationContext()
        {
        }
        public AnnotationContext(DbContextOptions options) : base(options)
        {}
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Highlight>(entity =>
            {
                entity.OwnsOne(h => h.Start);
                entity.OwnsOne(h => h.End);
            });

        }
    }
}