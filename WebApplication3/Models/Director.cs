using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Director
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Nationality { get; set; }

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
