using BiblioMit.Models.Entities.Centres;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class Analysis
    {
        public int Id { get; set; }
        public int FarmId { get; set; }
        [AllowNull]
        public virtual Farm Farm { get; set; }
        public DateTime Date { get; set; }
    }
}
