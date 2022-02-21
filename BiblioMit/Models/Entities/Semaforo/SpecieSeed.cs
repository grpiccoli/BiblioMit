using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class SpecieSeed
    {
        public int Id { get; set; }
        public int SpecieId { get; set; }
        [AllowNull]
        public virtual Specie Specie { get; set; }
        public int SeedId { get; set; }
        [AllowNull]
        public virtual Seed Seed { get; set; }
        public int Capture { get; set; }
        [Range(0, 100)]
        public double Proportion { get; set; }
        public virtual ICollection<Talla> Tallas { get; } = new List<Talla>();
    }
}
