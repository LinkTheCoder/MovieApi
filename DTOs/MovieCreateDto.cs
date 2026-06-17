using System.ComponentModel.DataAnnotations;

namespace MovieApi.DTOs
{
    public class MovieCreateDto
    {
        [Required(ErrorMessage = "Titel är obligatorisk.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "Titel måste vara mellan 1 och 150 tecken.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "År är obligatoriskt.")]
        [Range(1888, 2100, ErrorMessage = "Året måste vara mellan 1888 och 2100.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Speltid är obligatorisk.")]
        [Range(1, 600, ErrorMessage = "Speltid måste vara mellan 1 och 600 minuter.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "GenreId är obligatoriskt.")]
        [Range(1, int.MaxValue, ErrorMessage = "Ange ett giltigt GenreId.")]
        public int GenreId { get; set; }
    }
}
