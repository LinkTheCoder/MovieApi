using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MovieApi.DTOs;
using MovieApi.Services;

namespace MovieApi.Controllers
{
    // [ApiController] aktiverar automatisk modellvalidering:
    //   • Returnerar 400 BadRequest om [Required]/[Range]/[StringLength] bryts
    //   • Kräver inte manuell ModelState.IsValid-kontroll

    [Route("api/v{version:apiVersion}/movies")]
    [ApiController]
    [ApiVersion("1.0")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _service;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(IMovieService service, ILogger<MoviesController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        /// <summary>Hämtar en lista med filmer.</summary>
        /// <remarks>Stöder filtrering via ?title=dark, ?year=2008 och ?genre=Action.</remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies(
            [FromQuery] string? title,
            [FromQuery] int?    year,
            [FromQuery] string? genre)
        {
            _logger.LogDebug("GetMovies anropat – filter: title={Title}, year={Year}, genre={Genre}", title, year, genre);

            var movies = await _service.GetAllMoviesAsync(title, year, genre);

            if (!movies.Any())
                _logger.LogWarning("Inga filmer hittades med de angivna filtren.");
            else
                _logger.LogInformation("Returnerar {Count} film(er).", movies.Count());

            return Ok(movies);
        }

        /// <summary>Hämtar en specifik film via ID.</summary>
        /// <remarks>Stöder inkludering via ?withActors, ?withReviews och ?withDetails.</remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MovieDetailDto>> GetMovie(
            int  id,
            [FromQuery] bool withActors  = false,
            [FromQuery] bool withReviews = false,
            [FromQuery] bool withDetails = false)
        {
            _logger.LogDebug("GetMovie anropat – id={Id}, withActors={WithActors}, withReviews={WithReviews}, withDetails={WithDetails}",
                id, withActors, withReviews, withDetails);

            var dto = await _service.GetMovieByIdAsync(id, withActors, withReviews, withDetails);

            // 404 - resursen saknas
            if (dto == null)
            {
                _logger.LogWarning("Film med id {Id} hittades inte.", id);
                return NotFound();
            }

            // 200 - lyckad hamtning
            _logger.LogInformation("Film med id {Id} hämtad.", id);
            return Ok(dto);
        }

        /// <summary>Hämtar fullständiga detaljer för en film via ID.</summary>
        [HttpGet("{id}/details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MovieDetailDto>> GetMovieDetails(int id)
        {
            _logger.LogDebug("GetMovieDetails anropat – id={Id}", id);

            var dto = await _service.GetMovieDetailsAsync(id);

            // 404 - resursen saknas
            if (dto == null)
            {
                _logger.LogWarning("Filmdetaljer för id {Id} hittades inte.", id);
                return NotFound();
            }

            // 200 - lyckad hamtning
            _logger.LogInformation("Filmdetaljer för id {Id} hämtade.", id);
            return Ok(dto);
        }

        /// <summary>Skapar en ny film.</summary>
        /// <remarks>Returnerar 400 om GenreId inte refererar till en befintlig genre.</remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MovieDto>> PostMovie(MovieCreateDto dto)
        {
            _logger.LogDebug("PostMovie anropat – titel={Title}, genreId={GenreId}", dto.Title, dto.GenreId);

            var (movie, genreFound) = await _service.CreateMovieAsync(dto);

            // 400 - GenreId refererar till genre som inte finns
            if (!genreFound)
            {
                _logger.LogWarning("PostMovie misslyckades – genre med id {GenreId} finns inte.", dto.GenreId);
                return BadRequest($"Genre med id {dto.GenreId} finns inte.");
            }

            // 201 - resursen skapad; Location-header pekar pa GET /api/movies/{id}
            _logger.LogInformation("Film skapad med id {Id}.", movie!.Id);
            return CreatedAtAction(nameof(GetMovie), new { id = movie!.Id }, movie);
        }

        /// <summary>Uppdaterar en befintlig film.</summary>
        /// <remarks>Returnerar 404 om filmen inte hittas, 400 om GenreId är ogiltigt.</remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutMovie(int id, MovieUpdateDto dto)
        {
            _logger.LogDebug("PutMovie anropat – id={Id}", id);

            var (movieFound, genreFound) = await _service.UpdateMovieAsync(id, dto);

            // 404 - filmen saknas
            if (!movieFound)
            {
                _logger.LogWarning("PutMovie – film med id {Id} hittades inte.", id);
                return NotFound();
            }

            // 400 - GenreId refererar till genre som inte finns
            if (!genreFound)
            {
                _logger.LogWarning("PutMovie – genre med id {GenreId} finns inte.", dto.GenreId);
                return BadRequest($"Genre med id {dto.GenreId} finns inte.");
            }

            // 204 - lyckad uppdatering, inget innehall att returnera
            _logger.LogInformation("Film med id {Id} uppdaterad.", id);
            return NoContent();
        }

        /// <summary>Tar bort en film.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            _logger.LogDebug("DeleteMovie anropat – id={Id}", id);

            var found = await _service.DeleteMovieAsync(id);

            // 404 - filmen saknas
            if (!found)
            {
                _logger.LogWarning("DeleteMovie – film med id {Id} hittades inte.", id);
                return NotFound();
            }

            // 204 - lyckad borttagning, inget innehall att returnera
            _logger.LogInformation("Film med id {Id} borttagen.", id);
            return NoContent();
        }
    }
}