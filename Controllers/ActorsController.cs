using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApi.Data;
using MovieApi.DTOs;

namespace MovieApi.Controllers
{
    [Route("api/movies/{movieId}/actors")]
    [ApiController]
    public class ActorsController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public ActorsController(MovieDbContext context) => _context = context;

        /// <summary>Hämtar alla skådespelare för en specifik film.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ActorDto>>> GetActorsForMovie(int movieId)
        {
            var movieExists = await _context.Movies.AnyAsync(m => m.Id == movieId);
            if (!movieExists) return NotFound($"Film med id {movieId} finns inte.");

            return await _context.Movies
                .Where(m => m.Id == movieId)
                .SelectMany(m => m.Actors)
                .Select(a => new ActorDto
                {
                    Id        = a.Id,
                    Name      = a.Name,
                    BirthYear = a.BirthYear
                })
                .ToListAsync();
        }

        /// <summary>Kopplar en befintlig skådespelare till en film.</summary>
        /// <remarks>Returnerar 409 om skådespelaren redan är kopplad till filmen.</remarks>
        [HttpPost("{actorId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddActorToMovie(int movieId, int actorId)
        {
            var movie = await _context.Movies
                .Include(m => m.Actors)
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null)
                return NotFound($"Film med id {movieId} finns inte.");

            var actor = await _context.Actors.FindAsync(actorId);
            if (actor == null)
                return NotFound($"Skådespelare med id {actorId} finns inte.");

            if (movie.Actors.Any(a => a.Id == actorId))
                return Conflict("Skådespelaren är redan kopplad till den här filmen.");

            movie.Actors.Add(actor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Tar bort kopplingen mellan en skådespelare och en film.</summary>
        [HttpDelete("{actorId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveActorFromMovie(int movieId, int actorId)
        {
            var movie = await _context.Movies
                .Include(m => m.Actors)
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null)
                return NotFound($"Film med id {movieId} finns inte.");

            var actor = movie.Actors.FirstOrDefault(a => a.Id == actorId);
            if (actor == null)
                return NotFound($"Skådespelare med id {actorId} är inte kopplad till den här filmen.");

            movie.Actors.Remove(actor);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
