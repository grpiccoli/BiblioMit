using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class PlanktonAssayEmail : Indexed
    {
        [Required]
        public int EmailId { get; set; }
        [AllowNull]
        public virtual Email Email { get; set; }
        [Required]
        public int PlanktonAssayId { get; set; }
        [AllowNull]
        public virtual PlanktonAssay PlanktonAssay { get; set; }
    }
}
