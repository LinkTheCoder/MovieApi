using Microsoft.AspNetCore.Mvc;
using MovieApi.DTOs;
using MovieApi.Services;

namespace MovieApi.Controllers
{
    // [ApiController] aktiverar automatisk modellvalidering:
    //   • Returnerar 400 BadRequest om [Required]/[Range]/[StringLength] bryts
    //   • Kräver inte manuell ModelState.IsValid-kontroll

    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _service;

        public MoviesController(IMovieService service) => _service = service;

        /// <summary>Hämtar en lista med filmer.</summary>
        /// <remarks>Stöder filtrering via ?title=dark, ?year=2008 och ?genre=Action.</remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies(
            [FromQuery] string? title,
            [FromQuery] int?    year,
            [FromQuery] string? genre)
        {
            return Ok(await _service.GetAllMoviesAsync(title, year, genre));
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
            var dto = await _service.GetMovieByIdAsync(id, withActors, withReviews, withDetails);

            // 404 - resursen saknas
            if (dto == null) return NotFound();

            // 200 - lyckad hamtning
            return Ok(dto);
        }

        /// <summary>Hämtar fullständiga detaljer för en film via ID.</summary>
        [HttpGet("{id}/details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MovieDetailDto>> GetMovieDetails(int id)
        {
            var dto = await _service.GetMovieDetailsAsync(id);

            // 404 - resursen saknas
            if (dto == null) return NotFound();

            // 200 - lyckad hamtning
            return Ok(dto);
        }

        /// <summary>Skapar en ny film.</summary>
        /// <remarks>Returnerar 400 om GenreId inte refererar till en befintlig genre.</remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MovieDto>> PostMovie(MovieCreateDto dto)
        {
            var (movie, genreFound) = await _service.CreateMovieAsync(dto);

            // 400 - GenreId refererar till genre som inte finns
            if (!genreFound)
                return BadRequest($"Genre med id {dto.GenreId} finns inte.");

            // 201 - resursen skapad; Location-header pekar pa GET /api/movies/{id}
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
            var (movieFound, genreFound) = await _service.UpdateMovieAsync(id, dto);

            // 404 - filmen saknas
            if (!movieFound) return NotFound();

            // 400 - GenreId refererar till genre som inte finns
            if (!genreFound) return BadRequest($"Genre med id {dto.GenreId} finns inte.");

            // 204 - lyckad uppdatering, inget innehall att returnera
            return NoContent();
        }

        /// <summary>Tar bort en film.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var found = await _service.DeleteMovieAsync(id);

            // 404 - filmen saknas
            if (!found) return NotFound();

            // 204 - lyckad borttagning, inget innehall att returnera
            return NoContent();
        }
    }
}