using Microsoft.EntityFrameworkCore;
using MovieApi.Models;

namespace MovieApi.Interfaces
{
    public interface IMovieDbContext
    {
        DbSet<Movie> Movies { get; set; }
        DbSet<Genre> Genres { get; set; }
        DbSet<MovieDetails> MovieDetails { get; set; }
        DbSet<Review> Reviews { get; set; }
        DbSet<Actor> Actors { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
