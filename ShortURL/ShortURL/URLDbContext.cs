using Microsoft.EntityFrameworkCore;

namespace ShortURL
{
    public class URLDbContext : DbContext
    {
        public URLDbContext(DbContextOptions<URLDbContext> options) : base(options)
        {
        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortenedUrl>()
                .HasIndex(u => u.Code)
                .IsUnique();
        }
    }
}