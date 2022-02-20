using BiblioMit.Models.Entities.Centres;

namespace BiblioMit.Models
{
    public class PsmbArea : Psmb
    {
        public virtual ICollection<Farm> Farms { get; } = new List<Farm>();
    }
}
