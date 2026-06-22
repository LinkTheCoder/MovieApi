using Microsoft.EntityFrameworkCore;
using MovieApi.DTOs;
using MovieApi.Interfaces;
using MovieApi.Models;

namespace MovieApi.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieDbContext _db;

        public MovieService(IMovieDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<MovieDto>> GetAllMoviesAsync(string? title = null, int? year = null, string? genre = null)
        {
            var query = _db.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(m => m.Title.Contains(title));

            if (year.HasValue)
                query = query.Where(m => m.Year == year.Value);

            if (!string.IsNullOrWhiteSpace(genre))
                query = query.Where(m => m.Genre.Name.ToLower() == genre.ToLower());

            return await query
                .Select(m => new MovieDto
                {
                    Id       = m.Id,
                    Title    = m.Title,
                    Year     = m.Year,
                    Duration = m.Duration,
                    Genre    = m.Genre.Name
                })
                .ToListAsync();
        }

        public async Task<MovieDetailDto?> GetMovieByIdAsync(int id, bool withActors = false, bool withReviews = false, bool withDetails = false)
        {
            return await _db.Movies
                .Where(m => m.Id == id)
                .Select(m => new MovieDetailDto
                {
                    Id       = m.Id,
                    Title    = m.Title,
                    Year     = m.Year,
                    Duration = m.Duration,
                    Genre    = m.Genre.Name,
                    Details  = withDetails && m.MovieDetails != null
                        ? new MovieDetailsDto
                        {
                            Synopsis = m.MovieDetails.Synopsis,
                            Language = m.MovieDetails.Language,
                            Budget   = m.MovieDetails.Budget
                        }
                        : null,
                    Reviews = withReviews
                        ? m.Reviews.Select(r => new ReviewDto
                        {
                            Id           = r.Id,
                            ReviewerName = r.ReviewerName,
                            Comment      = r.Comment,
                            Rating       = r.Rating
                        }).ToList()
                        : new List<ReviewDto>(),
                    Actors = withActors
                        ? m.Actors.Select(a => new ActorDto
                        {
                            Id        = a.Id,
                            Name      = a.Name,
                            BirthYear = a.BirthYear
                        }).ToList()
                        : new List<ActorDto>()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<MovieDetailDto?> GetMovieDetailsAsync(int id)
        {
            return await _db.Movies
                .Where(m => m.Id == id)
                .Select(m => new MovieDetailDto
                {
                    Id       = m.Id,
                    Title    = m.Title,
                    Year     = m.Year,
                    Duration = m.Duration,
                    Genre    = m.Genre.Name,
                    Details  = m.MovieDetails == null ? null : new MovieDetailsDto
                    {
                        Synopsis = m.MovieDetails.Synopsis,
                        Language = m.MovieDetails.Language,
                        Budget   = m.MovieDetails.Budget
                    },
                    Reviews = m.Reviews.Select(r => new ReviewDto
                    {
                        Id           = r.Id,
                        ReviewerName = r.ReviewerName,
                        Comment      = r.Comment,
                        Rating       = r.Rating
                    }).ToList(),
                    Actors = m.Actors.Select(a => new ActorDto
                    {
                        Id        = a.Id,
                        Name      = a.Name,
                        BirthYear = a.BirthYear
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(MovieDto? Movie, bool GenreFound)> CreateMovieAsync(MovieCreateDto dto)
        {
            var genre = await _db.Genres
                .FirstOrDefaultAsync(g => g.Id == dto.GenreId);

            if (genre == null)
                return (null, false);

            var movie = new Movie
            {
                Title    = dto.Title,
                Year     = dto.Year,
                Duration = dto.Duration,
                GenreId  = dto.GenreId
            };

            _db.Movies.Add(movie);
            await _db.SaveChangesAsync();

            return (new MovieDto
            {
                Id       = movie.Id,
                Title    = movie.Title,
                Year     = movie.Year,
                Duration = movie.Duration,
                Genre    = genre.Name
            }, true);
        }

        public async Task<(bool MovieFound, bool GenreFound)> UpdateMovieAsync(int id, MovieUpdateDto dto)
        {
            var movie = await _db.Movies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return (false, true);

            var genreExists = await _db.Genres
                .AnyAsync(g => g.Id == dto.GenreId);

            if (!genreExists)
                return (true, false);

            movie.Title    = dto.Title;
            movie.Year     = dto.Year;
            movie.Duration = dto.Duration;
            movie.GenreId  = dto.GenreId;

            await _db.SaveChangesAsync();
            return (true, true);
        }

        public async Task<bool> DeleteMovieAsync(int id)
        {
            var movie = await _db.Movies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return false;

            _db.Movies.Remove(movie);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
