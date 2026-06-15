using Microsoft.EntityFrameworkCore;
using MovieApi.Models;

namespace MovieApi.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options) { }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<MovieDetails> MovieDetails => Set<MovieDetails>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Actor> Actors => Set<Actor>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // N:1 — Movie tillhör en Genre (normalisering)
            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Genre)
                .WithMany(g => g.Movies)
                .HasForeignKey(m => m.GenreId);

            // 1:1 — Movie har ett MovieDetails
            modelBuilder.Entity<Movie>()
                .HasOne(m => m.MovieDetails)
                .WithOne(md => md.Movie)
                .HasForeignKey<MovieDetails>(md => md.MovieId);

            modelBuilder.Entity<MovieDetails>()
                .Property(md => md.Budget)
                .HasPrecision(18, 2);

            // 1:M — Movie har många Reviews
            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Reviews)
                .WithOne(r => r.Movie)
                .HasForeignKey(r => r.MovieId);

            // N:M — Movie <-> Actor via join-tabellen MovieActor
            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Actors)
                .WithMany(a => a.Movies)
                .UsingEntity("MovieActor");
        }
    }
}
