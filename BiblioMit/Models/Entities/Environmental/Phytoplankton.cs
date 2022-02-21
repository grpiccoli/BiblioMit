using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class Phytoplankton
    {
        public int PlanktonAssayId { get; set; }
        [AllowNull]
        public virtual PlanktonAssay PlanktonAssay { get; set; }
        public int SpeciesId { get; set; }
        [AllowNull]
        public virtual SpeciesPhytoplankton Species { get; set; }
        [Display(Name = "Relative Abundance Scale", ShortName = "E.A.R.")]
        public Ear? EAR { get; set; }
        [Display(Name = "Toxicity concentration", Description = "cel/mL", ShortName = "C.")]
        public double C { get; set; }
    }
}
