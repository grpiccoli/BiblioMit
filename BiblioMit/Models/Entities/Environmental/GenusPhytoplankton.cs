using BiblioMit.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models.Entities.Environmental
{
    public class GenusPhytoplankton
    {
        public GenusPhytoplankton() { }
        public GenusPhytoplankton(string value)
        {
            NormalizedName = value.ToUpperInvariant();
            Name = value.FirstCharToUpper();
        }
        public int Id { get; set; }
        [Required, DisallowNull]
        public string Name { get; private set; }
        [Required, DisallowNull]
        public string NormalizedName { get; private set; } = null!;
        [Required]
        public int GroupId { get; set; }
        [AllowNull]
        public virtual PhylogeneticGroup Group { get; set; }
        public virtual ICollection<SpeciesPhytoplankton> SpeciesPhytoplanktons { get; } = new List<SpeciesPhytoplankton>();
    }
}
