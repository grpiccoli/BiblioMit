using BiblioMit.Models;

namespace BiblioMit.Services
{
    public interface ICentre
    {
        IEnumerable<Psmb> GetFilteredCentres(int page, int rpp, string searchQuery);
    }
}
