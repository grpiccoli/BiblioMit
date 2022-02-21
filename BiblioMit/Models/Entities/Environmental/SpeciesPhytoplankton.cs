using BiblioMit.Models.Entities.Environmental;
using Microsoft.Build.Framework;
using System.Diagnostics.CodeAnalysis;
namespace BiblioMit.Models
{
    public class SpeciesPhytoplankton
    {
        public SpeciesPhytoplankton() { }
        public SpeciesPhytoplankton(string value)
        {
            NormalizedName = value.ToUpperInvariant();
            Name = value.ToLowerInvariant();
        }
        public int Id { get; set; }
        [Required, DisallowNull]
        public string Name { get; private set; }
        [Required, DisallowNull]
        public string NormalizedName { get; private set; }
        //public string GetFullName()
        //{
        //    var n = string.IsNullOrWhiteSpace(Name) ? "sp" : Name;
        //    return $"{Genus.Name} {n}";
        //}
        [Required]
        public int GenusId { get; set; }
        [AllowNull]
        public virtual GenusPhytoplankton Genus { get; set; }
        public virtual ICollection<Phytoplankton> Phytoplanktons { get; } = new List<Phytoplankton>();
    }
}
