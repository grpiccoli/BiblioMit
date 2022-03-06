using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Digest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Localization;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class ColumnasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<ColumnasController> _localizer;
        public ColumnasController(ApplicationDbContext context,
            IStringLocalizer<ColumnasController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }
        [HttpGet]
        public IActionResult Index(string srt, string[] val, int? pg, int? rpp, bool? asc) =>
            RedirectToAction(nameof(Columnas), new { pg, rpp, srt, asc, val });
        // GET: Columnas
        [HttpGet]
        public IActionResult Columnas(
            string[] val, int pg = 1, int rpp = 20, bool asc = false, string srt = "ExcelId")
        {
            IQueryable<Registry> pre = _context.Registries.Pre();
            ViewData = _context.Registries.ViewData(pre, pg, rpp, srt, asc, val);
            ViewData["ExcelId"] = new MultiSelectList(
                from InputFile e in _context.InputFiles
                select new
                { e.Id, e.ClassName }, "Id", "ClassName",
                ViewData["Filters"] is IDictionary<string, List<string>> Filters && Filters.ContainsKey("ExcelId") ?
                Filters["ExcelId"] : null);

            IIncludableQueryable<Registry, ICollection<Header>> regs = 
                _context.Registries.Include(r => r.InputFile).Include(r => r.Headers);

            bool two2five = User.Claims.Any(c => c.Value == UserClaims.Digest.ToString());
            bool one = User.Claims.Any(c => c.Value == UserClaims.PSMB.ToString());

            if (one && two2five)
            {
                return View(regs.Where(r => r.InputFileId < 6));
            }
            else if (two2five)
            {
                return View(regs.Where(r => r.InputFileId > 1 && r.InputFileId < 6));
            }
            else if (one)
            {
                return View(regs.Where(r => r.InputFileId == 1));
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> Editar(int id, string description, string headers, string conversion, int places, string sep, bool negative)
        {
            if (string.IsNullOrWhiteSpace(sep))
            {
                throw new ArgumentException(_localizer["error"]);
            }

            char separator = sep[0];
            Registry? model = await _context.Registries
                .FindAsync(id).ConfigureAwait(false);
            if (model == null)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            IQueryable<Header> heads = _context.Headers.Where(h => h.RegistryId == id);
            model.Description = string.IsNullOrWhiteSpace(description) ? string.Empty : description;
            model.Operation = string.IsNullOrWhiteSpace(conversion) ? string.Empty : conversion;
            model.DecimalPlaces = places;
            model.DecimalSeparator = separator;
            model.DeleteAfter2ndNegative = negative;
            string[] all = headers.Split(";;");
            if (all.Any())
            {
                _context.Headers.RemoveRange(heads);
                IEnumerable<Header> newh = all.Select(a => { Header h = new() { RegistryId = id }; h.SetName(a); return h; });
                _context.Headers.AddRange(newh);
            }
            _context.Registries.Update(model);
            int result = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Json(result);
        }
    }
}
