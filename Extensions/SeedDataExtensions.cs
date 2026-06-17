using Microsoft.EntityFrameworkCore;
using MovieApi.Data;
using MovieApi.Models;

namespace MovieApi.Extensions
{
    public static class SeedDataExtensions
    {
        public static async Task SeedData(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MovieDbContext>();

            if (await context.Genres.AnyAsync()) return;

            // ── Genres ────────────────────────────────────────────────────────
            var action   = new Genre { Name = "Action" };
            var drama    = new Genre { Name = "Drama" };
            var sciFi    = new Genre { Name = "Sci-Fi" };
            var thriller = new Genre { Name = "Thriller" };
            var crime    = new Genre { Name = "Crime" };

            await context.Genres.AddRangeAsync(action, drama, sciFi, thriller, crime);
            await context.SaveChangesAsync();

            // ── Actors ────────────────────────────────────────────────────────
            var christianBale       = new Actor { Name = "Christian Bale",       BirthYear = 1974 };
            var heathLedger         = new Actor { Name = "Heath Ledger",         BirthYear = 1979 };
            var leonardoDiCaprio    = new Actor { Name = "Leonardo DiCaprio",    BirthYear = 1974 };
            var josephGordonLevitt  = new Actor { Name = "Joseph Gordon-Levitt", BirthYear = 1981 };
            var marionCotillard     = new Actor { Name = "Marion Cotillard",     BirthYear = 1975 };
            var morganFreeman       = new Actor { Name = "Morgan Freeman",       BirthYear = 1937 };
            var timRobbins          = new Actor { Name = "Tim Robbins",          BirthYear = 1958 };
            var johnTravolta        = new Actor { Name = "John Travolta",        BirthYear = 1954 };
            var marlonBrando        = new Actor { Name = "Marlon Brando",        BirthYear = 1924 };
            var alPacino            = new Actor { Name = "Al Pacino",            BirthYear = 1940 };

            await context.Actors.AddRangeAsync(
                christianBale, heathLedger, leonardoDiCaprio, josephGordonLevitt, marionCotillard,
                morganFreeman, timRobbins, johnTravolta, marlonBrando, alPacino);
            await context.SaveChangesAsync();

            // ── Movies + MovieDetails + Reviews + Actors (N:M) ───────────────
            var movies = new List<Movie>
            {
                new Movie
                {
                    Title    = "The Dark Knight",
                    Year     = 2008,
                    Duration = 152,
                    Genre    = action,
                    MovieDetails = new MovieDetails
                    {
                        Synopsis = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
                        Language = "English",
                        Budget   = 185_000_000m
                    },
                    Reviews = new List<Review>
                    {
                        new Review { ReviewerName = "Anna Karlsson",   Comment = "Heath Ledgers Joker är oförglömlig. En av de bästa filmerna någonsin.",   Rating = 5 },
                        new Review { ReviewerName = "Erik Lindqvist",  Comment = "Mörk, komplex och helt fantastisk. Nolans mästerverk.",                  Rating = 5 },
                        new Review { ReviewerName = "Sara Nilsson",    Comment = "Bra film men lite för lång och mörk för min smak.",                      Rating = 3 }
                    },
                    Actors = new List<Actor> { christianBale, heathLedger }
                },

                new Movie
                {
                    Title    = "Inception",
                    Year     = 2010,
                    Duration = 148,
                    Genre    = sciFi,
                    MovieDetails = new MovieDetails
                    {
                        Synopsis = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O.",
                        Language = "English",
                        Budget   = 160_000_000m
                    },
                    Reviews = new List<Review>
                    {
                        new Review { ReviewerName = "Maja Persson",    Comment = "Hjärnan smälter men på bästa möjliga sätt. Genialisk film.",              Rating = 5 },
                        new Review { ReviewerName = "Oscar Holm",      Comment = "Komplicerat men väldigt imponerande. Såg den tre gånger.",                Rating = 4 },
                        new Review { ReviewerName = "Lina Bergström",  Comment = "Förlorade tråden i mitten men slutet är suveränt.",                      Rating = 4 }
                    },
                    Actors = new List<Actor> { leonardoDiCaprio, josephGordonLevitt, marionCotillard }
                },

                new Movie
                {
                    Title    = "The Shawshank Redemption",
                    Year     = 1994,
                    Duration = 142,
                    Genre    = drama,
                    MovieDetails = new MovieDetails
                    {
                        Synopsis = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
                        Language = "English",
                        Budget   = 25_000_000m
                    },
                    Reviews = new List<Review>
                    {
                        new Review { ReviewerName = "Johan Magnusson",  Comment = "Den perfekta filmen finns. Det är denna. Rör mig varje gång.",           Rating = 5 },
                        new Review { ReviewerName = "Karin Jonsson",    Comment = "Morgan Freeman är gudomlig. Hoppfull och vacker berättelse.",            Rating = 5 },
                        new Review { ReviewerName = "David Eriksson",   Comment = "Lite långsam i början men belönar tålamodet fullständigt.",              Rating = 4 }
                    },
                    Actors = new List<Actor> { morganFreeman, timRobbins }
                },

                new Movie
                {
                    Title    = "Pulp Fiction",
                    Year     = 1994,
                    Duration = 154,
                    Genre    = crime,
                    MovieDetails = new MovieDetails
                    {
                        Synopsis = "The lives of two mob hitmen, a boxer, a gangster and his wife, and a pair of diner bandits intertwine in four tales of violence and redemption.",
                        Language = "English",
                        Budget   = 8_000_000m
                    },
                    Reviews = new List<Review>
                    {
                        new Review { ReviewerName = "Felix Strand",     Comment = "Tarantino på topp. Dialogen är oslagbar och strukturen briljant.",       Rating = 5 },
                        new Review { ReviewerName = "Ida Söderberg",    Comment = "Inte för alla, men absolut ett mästerverk för rätt publik.",             Rating = 4 },
                        new Review { ReviewerName = "Nils Hansson",     Comment = "Våldsamt och rörigt men ändå helt beroendeframkallande.",                Rating = 4 }
                    },
                    Actors = new List<Actor> { johnTravolta }
                },

                new Movie
                {
                    Title    = "The Godfather",
                    Year     = 1972,
                    Duration = 175,
                    Genre    = crime,
                    MovieDetails = new MovieDetails
                    {
                        Synopsis = "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.",
                        Language = "English",
                        Budget   = 6_000_000m
                    },
                    Reviews = new List<Review>
                    {
                        new Review { ReviewerName = "Emma Lindberg",    Comment = "Tidlöst mästerverk. Varje scen är perfekt komponerad.",                  Rating = 5 },
                        new Review { ReviewerName = "Lars Pettersson",  Comment = "Förstår varför den rankas som en av tidernas bästa filmer.",             Rating = 5 },
                        new Review { ReviewerName = "Hanna Wiklund",    Comment = "Lång men varje minut är motiverad. Brando är enastående.",               Rating = 5 }
                    },
                    Actors = new List<Actor> { marlonBrando, alPacino }
                }
            };

            await context.Movies.AddRangeAsync(movies);
            await context.SaveChangesAsync();
        }
    }
}
