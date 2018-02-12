using System;
using Microsoft.EntityFrameworkCore;

namespace server.Models
{
    public class AnnotationContext: DbContext
    {
        public DbSet<Annotation> Annotations { get; set; }
        public DbSet<Highlight> Highlights { get; set; }
        public DbSet<Text> Texts { get; set; }
        public DbSet<DocumentAnnotation> DocumentAnnotations { get; set; }

        public AnnotationContext()
        {
        }
        public AnnotationContext(DbContextOptions options) : base(options)
        {}
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Highlight>(entity =>
            {
                entity.OwnsOne(h => h.Start);
                entity.OwnsOne(h => h.End);
            });

        }
    }
}