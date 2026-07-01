using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApi.Data;
using MovieApi.DTOs;

namespace MovieApi.Controllers
{
    [Route("api/v{version:apiVersion}/movies/{movieId}/actors")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ActorsController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly ILogger<ActorsController> _logger;

        public ActorsController(MovieDbContext context, ILogger<ActorsController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        /// <summary>Hämtar alla skådespelare för en specifik film.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ActorDto>>> GetActorsForMovie(int movieId)
        {
            _logger.LogDebug("GetActorsForMovie anropat – movieId={MovieId}", movieId);

            var movieExists = await _context.Movies.AnyAsync(m => m.Id == movieId);
            if (!movieExists)
            {
                _logger.LogWarning("GetActorsForMovie – film med id {MovieId} finns inte.", movieId);
                return NotFound($"Film med id {movieId} finns inte.");
            }

            var actors = await _context.Movies
                .Where(m => m.Id == movieId)
                .SelectMany(m => m.Actors)
                .Select(a => new ActorDto
                {
                    Id        = a.Id,
                    Name      = a.Name,
                    BirthYear = a.BirthYear
                })
                .ToListAsync();

            _logger.LogInformation("Returnerar {Count} skådespelare för film id {MovieId}.", actors.Count, movieId);
            return actors;
        }

        /// <summary>Kopplar en befintlig skådespelare till en film.</summary>
        /// <remarks>Returnerar 409 om skådespelaren redan är kopplad till filmen.</remarks>
        [HttpPost("{actorId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddActorToMovie(int movieId, int actorId)
        {
            _logger.LogDebug("AddActorToMovie anropat – movieId={MovieId}, actorId={ActorId}", movieId, actorId);

            var movie = await _context.Movies
                .Include(m => m.Actors)
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null)
            {
                _logger.LogWarning("AddActorToMovie – film med id {MovieId} finns inte.", movieId);
                return NotFound($"Film med id {movieId} finns inte.");
            }

            var actor = await _context.Actors.FindAsync(actorId);
            if (actor == null)
            {
                _logger.LogWarning("AddActorToMovie – skådespelare med id {ActorId} finns inte.", actorId);
                return NotFound($"Skådespelare med id {actorId} finns inte.");
            }

            if (movie.Actors.Any(a => a.Id == actorId))
            {
                _logger.LogWarning("AddActorToMovie – skådespelare {ActorId} är redan kopplad till film {MovieId}.", actorId, movieId);
                return Conflict("Skådespelaren är redan kopplad till den här filmen.");
            }

            movie.Actors.Add(actor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Skådespelare {ActorId} kopplad till film {MovieId}.", actorId, movieId);
            return NoContent();
        }

        /// <summary>Tar bort kopplingen mellan en skådespelare och en film.</summary>
        [HttpDelete("{actorId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveActorFromMovie(int movieId, int actorId)
        {
            _logger.LogDebug("RemoveActorFromMovie anropat – movieId={MovieId}, actorId={ActorId}", movieId, actorId);

            var movie = await _context.Movies
                .Include(m => m.Actors)
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null)
            {
                _logger.LogWarning("RemoveActorFromMovie – film med id {MovieId} finns inte.", movieId);
                return NotFound($"Film med id {movieId} finns inte.");
            }

            var actor = movie.Actors.FirstOrDefault(a => a.Id == actorId);
            if (actor == null)
            {
                _logger.LogWarning("RemoveActorFromMovie – skådespelare {ActorId} är inte kopplad till film {MovieId}.", actorId, movieId);
                return NotFound($"Skådespelare med id {actorId} är inte kopplad till den här filmen.");
            }

            movie.Actors.Remove(actor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Skådespelare {ActorId} borttagen från film {MovieId}.", actorId, movieId);
            return NoContent();
        }
    }
}
