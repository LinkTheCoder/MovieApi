using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApi.Data;
using MovieApi.DTOs;

namespace MovieApi.Controllers
{
    [Route("api/v{version:apiVersion}/movies/{movieId}/reviews")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ReviewsController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public ReviewsController(MovieDbContext context) => _context = context;

        /// <summary>Hämtar recensioner för en film.</summary>
        /// <remarks>Stöder filtrering via ?minRating och ?maxRating.</remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews(
            int movieId,
            [FromQuery] int? minRating,
            [FromQuery] int? maxRating)
        {
            var movieExists = await _context.Movies.AnyAsync(m => m.Id == movieId);
            if (!movieExists)
                return NotFound($"Film med id {movieId} finns inte.");

            var query = _context.Reviews
                .Where(r => r.MovieId == movieId)
                .AsQueryable();

            if (minRating.HasValue)
                query = query.Where(r => r.Rating >= minRating.Value);

            if (maxRating.HasValue)
                query = query.Where(r => r.Rating <= maxRating.Value);

            return await query
                .Select(r => new ReviewDto
                {
                    Id           = r.Id,
                    ReviewerName = r.ReviewerName,
                    Comment      = r.Comment,
                    Rating       = r.Rating
                })
                .ToListAsync();
        }

        /// <summary>Hämtar en specifik recension.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReviewDto>> GetReview(int movieId, int id)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.MovieId == movieId);

            if (review == null) return NotFound();

            return new ReviewDto
            {
                Id           = review.Id,
                ReviewerName = review.ReviewerName,
                Comment      = review.Comment,
                Rating       = review.Rating
            };
        }
    }
}
