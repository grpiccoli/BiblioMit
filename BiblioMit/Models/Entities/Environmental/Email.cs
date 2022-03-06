using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class Email : Indexed
    {
        public int Id { get; set; }
        [Required, DisallowNull]
        public string Address { get; set; } = null!;
        public virtual ICollection<PlanktonAssayEmail> PlanktonAssayEmails { get; } = new List<PlanktonAssayEmail>();
    }
}
