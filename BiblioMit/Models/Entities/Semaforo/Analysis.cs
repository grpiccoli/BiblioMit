using BiblioMit.Models.Entities.Centres;

namespace BiblioMit.Models
{
    public class Analysis
    {
        public int Id { get; set; }
        public int FarmId { get; set; }
        public virtual Farm? Farm { get; set; }
        public DateTime Date { get; set; }
    }
}
