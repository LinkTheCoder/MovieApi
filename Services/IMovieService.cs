using MovieApi.DTOs;

namespace MovieApi.Services
{
    public interface IMovieService
    {
        Task<IEnumerable<MovieDto>> GetAllMoviesAsync(string? title = null, int? year = null, string? genre = null);
        Task<MovieDetailDto?> GetMovieByIdAsync(int id, bool withActors = false, bool withReviews = false, bool withDetails = false);
        Task<MovieDetailDto?> GetMovieDetailsAsync(int id);
        Task<(MovieDto? Movie, bool GenreFound)> CreateMovieAsync(MovieCreateDto dto);
        Task<(bool MovieFound, bool GenreFound)> UpdateMovieAsync(int id, MovieUpdateDto dto);
        Task<bool> DeleteMovieAsync(int id);
    }
}
