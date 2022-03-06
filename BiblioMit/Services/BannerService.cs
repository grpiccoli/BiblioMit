using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models.Entities.Ads;
using BiblioMit.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace BiblioMit.Services
{
    public class BannerService : IBannerService
    {
        private readonly ApplicationDbContext _context;
        private const string _carouselId = "introCarousel";
        public BannerService(
            ApplicationDbContext context
            )
        {
            _context = context;
        }
        public Dictionary<string,string> ReadCarousel(bool activeOnly, bool shuffle, string lang)
        {
            string which = activeOnly ? "Active" : "All";
            string shuffled = shuffle ? "Shuffled" : string.Empty;
            using StreamReader r = new(Path.Combine("StaticFiles", "json", $"carousel{which}{shuffled}.{lang}.json"));
            string json = r.ReadToEnd();
            Dictionary<string, string>? model = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (model == null) throw new FileLoadException();
            return model;
        }
        public void UpdateJsons()
        {
            foreach(CultureInfo culture in Statics.SupportedCultures)
            {
                string lang = culture.TwoLetterISOLanguageName;
                Carousel carousel = GetCarousel(false, false, lang);
                //Banners Index
                using StreamWriter w = new(Path.Combine("StaticFiles", "json", $"carouselAll.{lang}.json"));
                w.Write(JsonSerializer.Serialize(carousel));

                Carousel carouselHome = GetCarousel(true, true, lang);
                //Home Index
                using StreamWriter wHome = new(Path.Combine("StaticFiles", "json", $"carouselActiveShuffled.{lang}.json"));
                wHome.Write(JsonSerializer.Serialize(carouselHome));
            }
        }
        private IQueryable<Banner> GetBanners() =>
            _context.Banners
            .Include(b => b.Imgs)
            .Include(b => b.Texts)
                .ThenInclude(b => b.Btns)
            .Include(b => b.Rgbs)
            .Include(b => b.Payments);
        private IQueryable<Banner> GetBannersShuffled() =>
            GetBanners().OrderBy(_ => Guid.NewGuid());
        private Carousel GetCarousel(bool activeOnly, bool shuffle, string lang)
        {
            string active = "active";
            IQueryable<Banner> model = shuffle ? GetBannersShuffled() : GetBanners();

            if (activeOnly)
            {
                model = model
                    .Where(b => !b.Payments.Any(p => p.DueDate < DateTime.Today && p.PaidDate == null));
            }

            Carousel modelo = new();
            int i = 0;
            foreach (Banner value in model)
            {
                string mask = "background-color: rgba(0, 0, 0, 0.6)";
                Caption text = value.Texts.FirstOrDefault(t => t.Lang.ToString() == lang) ?? new();
                modelo.Indicators += GetCarouselButton(i, active);
                if (value.Rgbs.Any())
                {
                    if (value.Rgbs.Count > 1 && string.IsNullOrWhiteSpace(value.MaskAngle))
                    {
                        string rgbas = string.Join(",", value.Rgbs.Select(r => $"rgba({r.R}, {r.G}, {r.B}, 0.6)"));
                        mask = $"background: linear-gradient({value.MaskAngle}, {rgbas})";
                    }
                    else
                    {
                        Rgb first = value.Rgbs.First();
                        mask = $"background-color: rgba({first.R}, {first.G}, {first.B}, 0.6)";
                    }
                }
                mask = $@".banner-{i} .mask {{ {mask}; }}";
                if (!string.IsNullOrWhiteSpace(text.Color))
                    mask += $@"[id=""{text.Id}""] {{ color:{text.Color} !important }}";

                modelo.Styles += string.Join(" ", value.Imgs.Select(img => 
$@"@media (max-width: {(int)img.Size}px) {{ .banner-{i} {{ background-image: url('../Home/GetBanner?f={img.FileName}'); }} }}")) + mask;
                string btns = text.Btns.Any() ?
                    string.Join("", text.Btns.Select(b =>
@$"<a id=""{b.Id}"" class=""btn-banner btn btn-outline-light btn-lg m-2"" href=""{b.Uri}"" role=""button"" rel=""nofollow"" target=""_blank"">{b.Title}</a>"))
                    : string.Empty;
                modelo.Items += GetCarouselItems(i, active, lang, text, btns);
                i++;
                active = string.Empty;
            }
            return modelo;
        }
        public string GetCarouselItems(int i, string active, string lang, Caption text, string btns) =>
@$"<div class=""carousel-item banner-{i} {active}""><div class=""mask""><div class=""d-flex justify-content-center align-items-center h-100""><div id=""{text.Id}"" data-lang=""{lang}"" data-position=""{text.Position.GetAttrName()}"" class=""caption text-white text-center {text.Position.GetAttrDescription()} d-none d-md-block""><h1 class=""mb-3"">{text.Title}</h1><h5 class=""mb-4"">{text.Subtitle}</h5>{btns}</div></div></div></div>";
        public string GetCarouselButton(int id, string active) =>
@$"<button type=""button"" data-bs-target=""#{_carouselId}"" data-bs-slide-to=""{id}"" class=""{active}""></button>";
    }
    public class Carousel
    {
        public string Indicators { get; internal set; } = null!;
        public string Items { get; internal set; } = null!;
        public string Styles { get; internal set; } = null!;
    }
}
