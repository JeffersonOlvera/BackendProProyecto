using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Synopsis { get; set; }

        [Range(1, 600)]
        public int Duration { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [StringLength(255)]
        public string Image { get; set; }

        [ForeignKey("Genre")]
        public int GenreId { get; set; }
        public Genre? Genre { get; set; }

        [ForeignKey("Director")]
        public int DirectorId { get; set; }
        public Director? Director { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
