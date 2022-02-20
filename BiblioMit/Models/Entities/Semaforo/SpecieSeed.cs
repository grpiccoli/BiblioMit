using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class SpecieSeed
    {
        public int Id { get; set; }
        public int SpecieId { get; set; }
        public virtual Specie Specie { get; set; } = new Specie();
        public int SeedId { get; set; }
        public virtual Seed Seed { get; set; } = new Seed();
        public int Capture { get; set; }
        [Range(0, 100)]
        public double Proportion { get; set; }
        public virtual ICollection<Talla> Tallas { get; } = new List<Talla>();
    }
}
