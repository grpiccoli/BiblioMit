using BiblioMit.Models.Entities.Digest;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class SeedDeclaration : NotProduction
    {
        public int OriginId { get; set; }
        [AllowNull]
        public virtual Origin Origin { get; set; }
        [NotMapped]
        public string? CompanyName { get; set; }
    }
}
