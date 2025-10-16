using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
