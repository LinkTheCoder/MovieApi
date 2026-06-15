namespace MovieApi.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Duration { get; set; }

        // N:1 — en film tillhör en genre
        public int GenreId { get; set; }
        public Genre Genre { get; set; } = null!;

        // 1:1 — en film har ett MovieDetails-objekt
        public MovieDetails? MovieDetails { get; set; }

        // 1:M — en film kan ha många recensioner
        public ICollection<Review> Reviews { get; set; } = [];

        // N:M — en film kan ha många skådespelare (via MovieActor)
        public ICollection<Actor> Actors { get; set; } = [];
    }
}
