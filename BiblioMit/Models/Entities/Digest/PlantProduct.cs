using BiblioMit.Models.Entities.Centres;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class PlantProduct
    {
        public int PlantId { get; set; }
        [AllowNull]
        public virtual Plant Plant { get; set; }
        public int ProductId { get; set; }
        [AllowNull]
        public virtual Product? Product { get; set; }
    }
}
