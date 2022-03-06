using BiblioMit.Data;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Centres;
using BiblioMit.Models.VM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BiblioMit.Controllers
{
    [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Centres))]
    public class CentresController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CentresController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMap(PsmbType type, int[] c, int[] i)
        {
            if (c == null || i == null)
            {
                throw new ArgumentNullException($"Argument c {c} and i {i} cannot be null");
            }

            IQueryable<Psmb> list = type switch
            {
                PsmbType.Farm => GetEntitiesAsync<Farm>(c, i),
                PsmbType.ResearchCentre => GetEntitiesAsync<ResearchCentre>(c, i),
                _ => GetEntitiesAsync<Psmb>(c, i)
            };
            return Json(list
                .Select(f =>
                    new GMapPolygonCentre
                    {
                        Id = f.Code,
                        Name = f.Name,
                        BusinessName = f.Company != null ? f.Company.BusinessName : null,
                        Rut = f.CommuneId != null ? f.CommuneId.Value : 0,
                        Comuna = f.Commune != null ? f.Commune.Name : null,
                        ComunaId = f.CommuneId != null ? f.CommuneId.Value : 0,
                        Provincia = f.Commune != null && f.Commune.Province != null ? f.Commune.Province.Name : null,
                        Region = f.Commune != null && f.Commune.Province != null && f.Commune.Province.Region != null ? f.Commune.Province.Region.Name : null,
                        Code = f.Code,
                        Position = f.Polygon != null ? f.Polygon.Vertices.OrderBy(o => o.Order).Select(o =>
                        new GMapCoordinate
                        {
                            Lat = o.Latitude,
                            Lng = o.Longitude
                        }) : null,
                    }));
        }
        private IQueryable<Psmb> GetPsmbs<TEntity>(int[] c, int[] i)
            where TEntity : Psmb
        {
            ViewData["c"] = string.Join(",", c);
            ViewData["i"] = string.Join(",", i);
            return GetEntitiesAsync<TEntity>(c, i);
        }
        private IQueryable<TEntity> GetEntitiesAsync<TEntity>(int[] c, int[] i)
            where TEntity : Psmb
        {
            IQueryable<TEntity> centres = _context.Set<TEntity>()
                        .Where(a => a.PolygonId.HasValue
                && a.CompanyId.HasValue);
            foreach (int sc in c)
            {
                centres = centres.Where(a => a.CommuneId.HasValue && a.CommuneId.Value == sc);
            }
            foreach (int si in i)
            {
                centres = centres.Where(a => a.CommuneId.HasValue && a.CommuneId.Value == si);
            }
            return centres;
        }
        [HttpGet]
        public IActionResult Producers(int[] c, int[] i) => View(GetPsmbs<Farm>(c, i));
        // GET: Centres
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Research(int[] c, int[] i) => View(GetPsmbs<ResearchCentre>(c, i));
        // GET: Centres/Details/5
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Farm? centre = await _context.Farms
                .Include(c => c.Company)
                .Include(c => c.Polygon)
                .Include(c => c.Contacts)
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (centre is null)
            {
                return NotFound();
            }

            return View(centre);
        }

        // GET: Centres/Create
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Create()
        {
            ViewData["ComunaId"] = new SelectList(_context.Communes, "Id", "Name");

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id");
            return View();
        }

        // POST: Centres/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Create([Bind("Id,ComunaId,Type,Url,Acronym,CompanyId,Name,Address")] ResearchCentre centre)
        {
            if (centre is null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Add(centre);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return RedirectToAction("Index");
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", centre.CompanyId);
            return View(centre);
        }

        // GET: Centres/Edit/5
        [Authorize(Roles = nameof(RoleData.Administrator) + "," + nameof(RoleData.Editor), Policy = nameof(UserClaims.Centres))]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Farm? centre = await _context.Farms.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (centre is null)
            {
                return NotFound();
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", centre.CompanyId);
            ViewData["ComunaId"] = new SelectList(_context.Communes, "Id", "Name", centre.CommuneId);
            return View(centre);
        }

        // POST: Centres/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = nameof(RoleData.Administrator) + "," + nameof(RoleData.Editor), Policy = nameof(UserClaims.Centres))]
        public async Task<IActionResult> Edit(int id, [Bind("ComunaId,Type,Url,Acronym,CompanyId,Name,Address")] ResearchCentre centre)
        {
            if (id != centre.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(centre);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CentreExists(centre.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id", centre.CompanyId);
            return View(centre);
        }

        // GET: Centres/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            bool isAuthorized = User.IsInRole(RoleData.Administrator.ToString());
            if (!isAuthorized)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            Farm? centre = await _context.Farms
                .Include(c => c.Company)
                .Include(c => c.Commune)
                    .ThenInclude(c => c.Province)
                    .ThenInclude(c => c.Region)
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (centre is null)
            {
                return NotFound();
            }

            return View(centre);
        }

        // POST: Centres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Farm? centre = await _context.Farms.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (centre is not null)
            {
                _context.Farms.Remove(centre);
            }

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction("Index");
        }

        private bool CentreExists(int id)
        {
            return _context.Farms.Any(e => e.Id == id);
        }
    }
}
