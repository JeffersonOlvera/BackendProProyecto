using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Biography { get; set; }

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
