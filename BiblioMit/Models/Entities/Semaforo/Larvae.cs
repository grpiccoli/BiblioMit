using BiblioMit.Models.Entities.Centres;

namespace BiblioMit.Models
{
    public class Larvae
    {
        public int Id { get; set; }
        public int FarmId { get; set; }
        public virtual Farm? Farm { get; set; }
        public DateTime Date { get; set; }
        public virtual ICollection<Larva> Larva { get; } = new List<Larva>();
    }
}
