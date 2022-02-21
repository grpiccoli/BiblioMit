using BiblioMit.Extensions;
using BiblioMit.Models.Entities.Environmental;
using Microsoft.Build.Framework;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class PhylogeneticGroup
    {
        public PhylogeneticGroup() { }
        public PhylogeneticGroup(string value)
        {
            NormalizedName = value.ToUpperInvariant();
            Name = value.FirstCharToUpper();
        }
        public int Id { get; set; }
        [Required, DisallowNull]
        public string Name { get; private set; }
        [Required, DisallowNull]
        public string NormalizedName { get; private set; }
        public virtual ICollection<GenusPhytoplankton> GenusPhytoplanktons { get; } = new List<GenusPhytoplankton>();
    }
}
