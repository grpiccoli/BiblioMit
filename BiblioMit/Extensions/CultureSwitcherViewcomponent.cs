using BiblioMit.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace BiblioMit.Extensions
{
    public class CultureSwitcherViewcomponent : ViewComponent
    {
        private readonly IOptions<RequestLocalizationOptions> _localizationOptions;
        public CultureSwitcherViewcomponent(IOptions<RequestLocalizationOptions> localizationOptions) =>
            _localizationOptions = localizationOptions;
        public IViewComponentResult Invoke()
        {
            CultureInfo cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture ?? _localizationOptions.Value.DefaultRequestCulture.UICulture;
            CultureSwitcherModel model = new(cultureFeature, _localizationOptions.Value.SupportedUICultures ?? new List<CultureInfo>());
            return View(model);
        }
    }
}
