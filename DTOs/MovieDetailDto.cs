namespace MovieApi.DTOs
{
    public class MovieDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Duration { get; set; }
        public string Genre { get; set; } = string.Empty;

        public MovieDetailsDto? Details { get; set; }
        public List<ReviewDto> Reviews { get; set; } = [];
        public List<ActorDto> Actors { get; set; } = [];
    }
}
