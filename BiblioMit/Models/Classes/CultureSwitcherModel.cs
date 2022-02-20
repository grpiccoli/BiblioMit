using System.Globalization;

namespace BiblioMit.Models
{
    public class CultureSwitcherModel
    {
        public CultureSwitcherModel(CultureInfo cultureInfo, IList<CultureInfo> cultureInfos)
        {
            CurrentUICulture = cultureInfo;
            SupportedCultures = cultureInfos;
        }
        public CultureInfo CurrentUICulture { get; set; }
        public ICollection<CultureInfo> SupportedCultures { get; internal set; }
    }
}
