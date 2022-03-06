using BiblioMit.Models.Entities.Histopathology;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class Photo
    {
        public int Id { get; set; }
        //Parent Id
        public int IndividualId { get; set; }
        //Related Parent Entities
        [AllowNull]
        public virtual Individual Individual { get; set; }
        //ATT
        public string Key { get; set; } = null!;
        public string? Comment { get; set; }
        public Magnification Magnification { get; set; }
    }
}
