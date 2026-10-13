using Microsoft.EntityFrameworkCore;

namespace mvc.DAL
{
    public class MovieContext : DbContext
    {
        public MovieContext(DbContextOptions<MovieContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieTranslation> MovieTranslations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>()
                .Navigation(m => m.Translations)
                .AutoInclude(); // переклади завжди завантажуються разом із фільмом

            modelBuilder.Entity<MovieTranslation>(entity =>
            {
                entity.Property(t => t.Culture).HasMaxLength(10);

                entity.HasOne(t => t.Movie)
                      .WithMany(m => m.Translations)
                      .HasForeignKey(t => t.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(t => new { t.MovieId, t.Culture }).IsUnique();
            });
        }
    }
}
