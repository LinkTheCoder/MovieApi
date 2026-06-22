using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieApi.Controllers;
using MovieApi.DTOs;
using MovieApi.Services;

namespace TestingMovieWebApi;

public class MoviesControllerTests
{
    // Skapar controller med mockad service
    private static (MoviesController controller, Mock<IMovieService> mock) Build()
    {
        var mock = new Mock<IMovieService>();
        return (new MoviesController(mock.Object), mock);
    }

    // ── GET /api/movies ───────────────────────────────────────────────────

    [Fact]
    public async Task GetMovies_Returns200_WithList()
    {
        var (sut, mock) = Build();
        var movies = new List<MovieDto>
        {
            new() { Id = 1, Title = "Inception", Year = 2010, Duration = 148, Genre = "Sci-Fi" }
        };
        mock.Setup(s => s.GetAllMoviesAsync(null, null, null)).ReturnsAsync(movies);

        var result = await sut.GetMovies(null, null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(movies, ok.Value);
    }

    // ── GET /api/movies/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task GetMovie_Found_Returns200()
    {
        var (sut, mock) = Build();
        var dto = new MovieDetailDto { Id = 1, Title = "Inception" };
        mock.Setup(s => s.GetMovieByIdAsync(1, false, false, false)).ReturnsAsync(dto);

        var result = await sut.GetMovie(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task GetMovie_NotFound_Returns404()
    {
        var (sut, mock) = Build();
        mock.Setup(s => s.GetMovieByIdAsync(99, false, false, false))
            .ReturnsAsync((MovieDetailDto?)null);

        var result = await sut.GetMovie(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ── GET /api/movies/{id}/details ──────────────────────────────────────

    [Fact]
    public async Task GetMovieDetails_Found_Returns200()
    {
        var (sut, mock) = Build();
        var dto = new MovieDetailDto { Id = 1, Title = "Inception" };
        mock.Setup(s => s.GetMovieDetailsAsync(1)).ReturnsAsync(dto);

        var result = await sut.GetMovieDetails(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task GetMovieDetails_NotFound_Returns404()
    {
        var (sut, mock) = Build();
        mock.Setup(s => s.GetMovieDetailsAsync(99))
            .ReturnsAsync((MovieDetailDto?)null);

        var result = await sut.GetMovieDetails(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ── POST /api/movies ──────────────────────────────────────────────────

    [Fact]
    public async Task PostMovie_ValidGenre_Returns201()
    {
        var (sut, mock) = Build();
        var createDto = new MovieCreateDto { Title = "Dune", Year = 2021, Duration = 155, GenreId = 1 };
        var created   = new MovieDto { Id = 5, Title = "Dune", Year = 2021, Duration = 155, Genre = "Sci-Fi" };
        mock.Setup(s => s.CreateMovieAsync(createDto)).ReturnsAsync((created, true));

        var result = await sut.PostMovie(createDto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdAt.StatusCode);
        Assert.Equal(created, createdAt.Value);
    }

    [Fact]
    public async Task PostMovie_InvalidGenre_Returns400()
    {
        var (sut, mock) = Build();
        var createDto = new MovieCreateDto { Title = "Dune", Year = 2021, Duration = 155, GenreId = 99 };
        mock.Setup(s => s.CreateMovieAsync(createDto))
            .ReturnsAsync(((MovieDto?)null, false));

        var result = await sut.PostMovie(createDto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ── PUT /api/movies/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task PutMovie_Valid_Returns204()
    {
        var (sut, mock) = Build();
        var updateDto = new MovieUpdateDto { Title = "Dune: Part Two", Year = 2024, Duration = 166, GenreId = 1 };
        mock.Setup(s => s.UpdateMovieAsync(1, updateDto)).ReturnsAsync((true, true));

        var result = await sut.PutMovie(1, updateDto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task PutMovie_MovieNotFound_Returns404()
    {
        var (sut, mock) = Build();
        var updateDto = new MovieUpdateDto { Title = "X", Year = 2020, Duration = 90, GenreId = 1 };
        mock.Setup(s => s.UpdateMovieAsync(99, updateDto)).ReturnsAsync((false, true));

        var result = await sut.PutMovie(99, updateDto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task PutMovie_InvalidGenre_Returns400()
    {
        var (sut, mock) = Build();
        var updateDto = new MovieUpdateDto { Title = "X", Year = 2020, Duration = 90, GenreId = 99 };
        mock.Setup(s => s.UpdateMovieAsync(1, updateDto)).ReturnsAsync((true, false));

        var result = await sut.PutMovie(1, updateDto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── DELETE /api/movies/{id} ───────────────────────────────────────────

    [Fact]
    public async Task DeleteMovie_Found_Returns204()
    {
        var (sut, mock) = Build();
        mock.Setup(s => s.DeleteMovieAsync(1)).ReturnsAsync(true);

        var result = await sut.DeleteMovie(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteMovie_NotFound_Returns404()
    {
        var (sut, mock) = Build();
        mock.Setup(s => s.DeleteMovieAsync(99)).ReturnsAsync(false);

        var result = await sut.DeleteMovie(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
