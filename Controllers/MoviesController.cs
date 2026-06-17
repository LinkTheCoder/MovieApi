using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApi.Data;
using MovieApi.DTOs;
using MovieApi.Models;

namespace MovieApi.Controllers
{
    // [ApiController] aktiverar automatisk modellvalidering:
    //   • Returnerar 400 BadRequest om [Required]/[Range]/[StringLength] bryts
    //   • Kräver inte manuell ModelState.IsValid-kontroll

    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public MoviesController(MovieDbContext context) => _context = context;

        // ── GET /api/movies ───────────────────────────────────────────────────
        // Frivilliga filter: ?title=dark  ?year=2008  ?genre=Action
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies(
            [FromQuery] string? title,
            [FromQuery] int?    year,
            [FromQuery] string? genre)
        {
            var query = _context.Movies.Include(m => m.Genre).AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(m => m.Title.Contains(title));

            if (year.HasValue)
                query = query.Where(m => m.Year == year.Value);

            if (!string.IsNullOrWhiteSpace(genre))
                query = query.Where(m => m.Genre.Name.ToLower() == genre.ToLower());

            return Ok(await query
                .Select(m => new MovieDto
                {
                    Id       = m.Id,
                    Title    = m.Title,
                    Year     = m.Year,
                    Duration = m.Duration,
                    Genre    = m.Genre.Name
                })
                .ToListAsync());
        }

        // ── GET /api/movies/{id} ──────────────────────────────────────────────
        // Frivilliga: ?withActors=true  ?withReviews=true  ?withDetails=true
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MovieDetailDto>> GetMovie(
            int  id,
            [FromQuery] bool withActors  = false,
            [FromQuery] bool withReviews = false,
            [FromQuery] bool withDetails = false)
        {
            var query = _context.Movies.Include(m => m.Genre).AsQueryable();

            if (withDetails) query = query.Include(m => m.MovieDetails);
            if (withReviews) query = query.Include(m => m.Reviews);
            if (withActors)  query = query.Include(m => m.Actors);

            var movie = await query.FirstOrDefaultAsync(m => m.Id == id);

            // 404 – resursen saknas
            if (movie == null) return NotFound();

            // 200 – lyckad hämtning
            return Ok(MapToDetailDto(movie, withActors, withReviews, withDetails));
        }

        // ── GET /api/movies/{id}/details ──────────────────────────────────────
        // Alltid full vy: filmdata + detaljer + recensioner + skådespelare
        // Använder ren LINQ Select-projektion – inga Include(), EF Core sköter joinerna
        [HttpGet("{id}/details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MovieDetailDto>> GetMovieDetails(int id)
        {
            var dto = await _context.Movies
                .Where(m => m.Id == id)
                .Select(m => new MovieDetailDto
                {
                    Id       = m.Id,
                    Title    = m.Title,
                    Year     = m.Year,
                    Duration = m.Duration,
                    Genre    = m.Genre.Name,

                    Details = m.MovieDetails == null ? null : new MovieDetailsDto
                    {
                        Synopsis = m.MovieDetails.Synopsis,
                        Language = m.MovieDetails.Language,
                        Budget   = m.MovieDetails.Budget
                    },

                    Reviews = m.Reviews
                        .Select(r => new ReviewDto
                        {
                            Id           = r.Id,
                            ReviewerName = r.ReviewerName,
                            Comment      = r.Comment,
                            Rating       = r.Rating
                        })
                        .ToList(),

                    Actors = m.Actors
                        .Select(a => new ActorDto
                        {
                            Id        = a.Id,
                            Name      = a.Name,
                            BirthYear = a.BirthYear
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            // 404 – resursen saknas
            if (dto == null) return NotFound();

            // 200 – lyckad hämtning
            return Ok(dto);
        }

        // ── POST /api/movies ──────────────────────────────────────────────────
        // [ApiController] ger automatiskt 400 om MovieCreateDto-valideringen misslyckas
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MovieDto>> PostMovie(MovieCreateDto dto)
        {
            // 400 – manuell kontroll att refererat GenreId existerar
            var genreExists = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!genreExists)
                return BadRequest($"Genre med id {dto.GenreId} finns inte.");

            var movie = new Movie
            {
                Title    = dto.Title,
                Year     = dto.Year,
                Duration = dto.Duration,
                GenreId  = dto.GenreId
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            await _context.Entry(movie).Reference(m => m.Genre).LoadAsync();

            // 201 – resursen skapad; Location-header pekar på GET /api/movies/{id}
            return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, new MovieDto
            {
                Id       = movie.Id,
                Title    = movie.Title,
                Year     = movie.Year,
                Duration = movie.Duration,
                Genre    = movie.Genre.Name
            });
        }

        // ── PUT /api/movies/{id} ──────────────────────────────────────────────
        // [ApiController] ger automatiskt 400 om MovieUpdateDto-valideringen misslyckas
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutMovie(int id, MovieUpdateDto dto)
        {
            // 404 – filmen saknas
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            // 400 – GenreId refererar till genre som inte finns
            var genreExists = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!genreExists)
                return BadRequest($"Genre med id {dto.GenreId} finns inte.");

            movie.Title    = dto.Title;
            movie.Year     = dto.Year;
            movie.Duration = dto.Duration;
            movie.GenreId  = dto.GenreId;

            await _context.SaveChangesAsync();

            // 204 – lyckad uppdatering, inget innehåll att returnera
            return NoContent();
        }

        // ── DELETE /api/movies/{id} ───────────────────────────────────────────
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            // 404 – filmen saknas
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            // 204 – lyckad borttagning, inget innehåll att returnera
            return NoContent();
        }

        // ── Privat hjälpmetod ─────────────────────────────────────────────────
        private static MovieDetailDto MapToDetailDto(
            Movie movie, bool withActors, bool withReviews, bool withDetails) => new()
        {
            Id       = movie.Id,
            Title    = movie.Title,
            Year     = movie.Year,
            Duration = movie.Duration,
            Genre    = movie.Genre.Name,
            Details  = withDetails && movie.MovieDetails != null
                ? new MovieDetailsDto
                {
                    Synopsis = movie.MovieDetails.Synopsis,
                    Language = movie.MovieDetails.Language,
                    Budget   = movie.MovieDetails.Budget
                }
                : null,
            Reviews = withReviews
                ? movie.Reviews.Select(r => new ReviewDto
                {
                    Id           = r.Id,
                    ReviewerName = r.ReviewerName,
                    Comment      = r.Comment,
                    Rating       = r.Rating
                }).ToList()
                : [],
            Actors = withActors
                ? movie.Actors.Select(a => new ActorDto
                {
                    Id        = a.Id,
                    Name      = a.Name,
                    BirthYear = a.BirthYear
                }).ToList()
                : []
        };
    }
}
