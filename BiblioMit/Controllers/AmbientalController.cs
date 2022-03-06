﻿using BiblioMit.Data;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Semaforo;
using BiblioMit.Models.VM;
using BiblioMit.Models.VM.AmbientalVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Range = BiblioMit.Models.Range;

namespace BiblioMit.Controllers
{
    [Authorize]
    [ResponseCache(Duration = 60 * 60 * 24 * 365, VaryByQueryKeys = new string[] { "*" })]
    public class AmbientalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _dateFormat;
        private readonly IWebHostEnvironment _environment;
        public AmbientalController(
            ApplicationDbContext context,
            IWebHostEnvironment environment
            )
        {
            _context = context;
            _environment = environment;
            _dateFormat = "yyyy-MM-dd";
        }
        [HttpGet]
        public IActionResult GetContent([Bind("Name,Code,Commune,Province,Area")] Content model)
        {
            return PartialView("_GetContent", model);
        }
        [HttpGet]
        public IActionResult PullPlankton()
        {
            string file = Path.Combine(
                _environment.ContentRootPath,
                "StaticFiles",
                "html",
                "PullRecords.html");
            string[] htmlString = System.IO.File.ReadAllLines(file);
            return View("PullPlankton", string.Join("", htmlString));
        }
        [HttpGet]
        public JsonResult? TLData(int a, int psmb, int sp, int? v
            //, DateTime start, DateTime end
            )
        {
            //1 analysis, 2 psmb, 3 species, 4 size, 5 larva type, 6 rep stg, 7 sex
            if (a != 14 && a != 17 && !v.HasValue)
            {
                throw new ArgumentNullException($"error: {a}, {psmb}, {sp}, {v}");
            }

            if ((a == 13 || a == 15 || a == 16) && sp != 31)
            {
                return null;
            }

            Dictionary<int, int> psmbs = new()
            {
                //Quetalco
                { 20, 101990 },
                //Vilipulli
                { 21, 101017 },
                //Carahue
                { 22, 103633 }
            };
            //1 chorito, 2 cholga, 3 choro, 4 all
            Dictionary<int, int> sps = new()
            {
                { 31, 1 },
                { 32, 2 },
                { 33, 3 }
            };
            //TallaRange 0-7
            //LarvaType 0 D, 1 U, 2 O
            //0 101990 Quetalco, 1 101017 Vilipulli, 2 103633 Carahue 23 all
            IQueryable<AmData>? selection = new List<AmData>() as IQueryable<AmData>;
            switch (a)
            {
                //size
                case 11:
                    if (v.HasValue)
                    {
                        int range = v.Value % 10;
                        IQueryable<Talla> db = _context.Tallas;
                        if (range != 8)
                        {
                            db = db.Where(tl => tl.Range == (Range)range);
                        }

                        if (psmb != 23)
                        {
                            db = db.Where(tl => tl.SpecieSeed != null && tl.SpecieSeed.Seed != null && tl.SpecieSeed.Seed.Farm != null && tl.SpecieSeed.Seed.Farm.Code == psmbs[psmb]);
                        }

                        if (sp != 34)
                        {
                            db = db.Where(tl => tl.SpecieSeed != null && tl.SpecieSeed.SpecieId == sps[sp]);
                        }

                        selection = db
                            .GroupBy(tl => tl.SpecieSeed == null || tl.SpecieSeed.Seed == null ? DateTime.MinValue : tl.SpecieSeed.Seed.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData(
                                g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Math.Round(g.Average(tl => tl.Proportion))));
                    }
                    break;
                //larvae
                case 12:
                    if (v.HasValue)
                    {
                        int type = v.Value % 10;
                        IQueryable<Larva> db = _context.Larvas;
                        if (type != 3)
                        {
                            db = db.Where(tl => tl.LarvaType == (LarvaType)type);
                        }

                        if (psmb != 23)
                        {
                            db = db.Where(tl => tl.Larvae != null && tl.Larvae.Farm != null && tl.Larvae.Farm.Code == psmbs[psmb]);
                        }

                        if (sp != 34)
                        {
                            db = db.Where(tl => tl.SpecieId == sps[sp]);
                        }

                        selection = db
                            .GroupBy(tl => tl.Larvae == null ? DateTime.MinValue : tl.Larvae.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData(
                                g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Math.Round(g.Average(tl => tl.Count))));
                    }
                    break;
                //ig reproductores
                case 13:
                    if (v.HasValue)
                    {
                        IQueryable<Spawning> db = _context.Spawnings;
                        if (psmb != 23)
                        {
                            db = db.Where(tl => tl.Farm != null && tl.Farm.Code == psmbs[psmb]);
                        }

                        selection = db
                            .GroupBy(tl => tl.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData(
                                g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Math.Round(g.Average(tl => v.Value == 70 ? tl.FemaleIG : tl.MaleIG))));
                    }
                    break;
                //capture
                case 14:
                    if (true)
                    {
                        IQueryable<SpecieSeed> db = _context.SpecieSeeds;
                        if (psmb != 23)
                        {
                            db = db.Where(tl => tl.Seed != null && tl.Seed.Farm != null && tl.Seed.Farm.Code == psmbs[psmb]);
                        }

                        if (sp != 34)
                        {
                            db = db.Where(tl => tl.SpecieId == sps[sp]);
                        }

                        selection = db
                            .GroupBy(tl => tl.Seed == null ? DateTime.MinValue : tl.Seed.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData(
                                g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Math.Round(g.Average(tl => tl.Capture))));
                    }
                    break;
                //rep stage
                case 15:
                    if (v.HasValue)
                    {
                        int stage = v.Value % 10;
                        IQueryable<ReproductiveStage> db = _context.ReproductiveStages.Where(tl => tl.Stage == (Stage)stage);
                        if (psmb != 23)
                        {
                            db = db.Where(tl => tl.Spawning != null && tl.Spawning.Farm != null && tl.Spawning.Farm.Code == psmbs[psmb]);
                        }

                        selection = db
                            .GroupBy(tl => tl.Spawning == null ? DateTime.MinValue : tl.Spawning.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData(
                                g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Math.Round(g.Average(tl => tl.Proportion))));
                    }
                    break;
                //% sex
                case 16:
                    if (v.HasValue)
                    {
                        IQueryable<Spawning> db = _context.Spawnings;
                        if (psmb != 23)
                        {
                            db = db.Where(tl => tl.Farm != null && tl.Farm.Code == psmbs[psmb]);
                        }

                        selection = db
                            .GroupBy(tl => tl.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData(
                                g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Math.Round(g.Average(tl => v.Value == 70 ? tl.FemaleProportion : tl.MaleProportion))));
                    }
                    break;
                //% species
                case 17:
                    if (true)
                    {
                        IQueryable<SpecieSeed> db = _context.SpecieSeeds;
                        if (psmb != 23)
                        {
                            db = db.Where(tl => tl.Seed != null && tl.Seed.Farm != null && tl.Seed.Farm.Code == psmbs[psmb]);
                        }

                        if (sp != 34)
                        {
                            db = db.Where(tl => tl.SpecieId == sps[sp]);
                        }

                        selection = db
                            .GroupBy(tl => tl.Seed == null ? DateTime.MinValue : tl.Seed.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new AmData(
                                g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                                Math.Round(g.Average(tl => tl.Proportion))));
                    }
                    break;
            }
            return Json(selection);
        }
        [HttpGet]
        [ResponseCache(Duration = 60)]
        public IActionResult BuscarInformes(int id, string start, string end)
        {
            int order = id / 99_996 + 24_998 / 24_999;
            DateTime i = Convert.ToDateTime(start, CultureInfo.InvariantCulture);
            DateTime f = Convert.ToDateTime(end, CultureInfo.InvariantCulture).AddDays(1);
            IQueryable<PlanktonAssay> plankton = _context.PlanktonAssays.Where(p => p.SamplingDate >= i && p.SamplingDate <= f);
            plankton = order switch
            {
                0 => plankton.Where(e => e.Psmb != null && e.Psmb.Commune != null && e.Psmb.Commune.CatchmentAreaId == id),
                1 => plankton.Where(e => e.PsmbId == id),
                _ => plankton.Where(e => e.Psmb != null && e.Psmb.CommuneId == id)
            };
            return Json(plankton.Select(p => new { p.Id, SamplingDate = p.SamplingDate.ToShortDateString(), p.Temperature, p.Oxigen, p.Ph, p.Salinity }));
        }
        [HttpGet]
        [ResponseCache(Duration = 60)]
        public IActionResult CustomData(int area, int typeid, DateTime start, DateTime end)
        {
            end = end.AddDays(1);
            return Json(_context.Variables
            .AsNoTracking()
            .Where(v => v.VariableTypeId == typeid && v.PsmbId == area && v.Date >= start && v.Date <= end)
        .GroupBy(e => e.Date)
        .OrderBy(g => g.Key)
        .Select(g => new AmData(
            g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
            g.Average(p => p.Value))));
        }
        [AllowAnonymous]
        [ResponseCache(Duration = 60)]
        [HttpGet]
        public IActionResult Data(int area, char type, int id, DateTime start, DateTime end)
        {
            end = end.AddDays(1);
            int order = 0;
            if (area > 3)
            {
                order++;
            }

            if (area > 100000)
            {
                order++;
            }

            if (!(type == 'v' && id != 4))
            {
                IQueryable<Phytoplankton> phyto = _context.Phytoplanktons
                                .AsNoTracking()
                    .Where(e => e.PlanktonAssay != null && e.PlanktonAssay.SamplingDate >= start && e.PlanktonAssay.SamplingDate <= end);
                phyto = type switch
                {
                    'f' => phyto
                    .Where(p => p.Species != null && p.Species.Genus != null && p.Species.Genus.GroupId == id),
                    'g' => phyto
                    .Where(p => p.Species != null && p.Species.GenusId == id),
                    's' => phyto
                    .Where(p => p.SpeciesId == id),
                    _ => phyto
                };
                phyto = order switch
                {
                    0 => phyto.Where(e => e.PlanktonAssay != null && e.PlanktonAssay.Psmb != null && e.PlanktonAssay.Psmb.Commune != null && e.PlanktonAssay.Psmb.Commune.CatchmentAreaId == area),
                    1 => phyto.Where(e => e.PlanktonAssay != null && e.PlanktonAssay.PsmbId == area),
                    _ => phyto.Where(e => e.PlanktonAssay != null && e.PlanktonAssay.Psmb != null && e.PlanktonAssay.Psmb.CommuneId == area)
                };
                return Json(phyto
                    .GroupBy(e => e.PlanktonAssay == null ? DateTime.MinValue : e.PlanktonAssay.SamplingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AmData(
                        g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                        g.Average(p => p.C))));
            }
            else
            {
                IQueryable<PlanktonAssay> assays = _context.PlanktonAssays
                    .AsNoTracking()
                    .Where(e => e.SamplingDate >= start && e.SamplingDate <= end);
                assays = order switch
                {
                    0 => assays.Where(e => e.Psmb != null && e.Psmb.Commune != null && e.Psmb.Commune.CatchmentAreaId == area),
                    1 => assays.Where(e => e.PsmbId == area),
                    _ => assays.Where(e => e.Psmb != null && e.Psmb.CommuneId == area)
                };
                return Json(id switch
                {
                    //ph
                    1 => assays
                    .Where(a => a.Ph.HasValue)
                    .GroupBy(e => e.SamplingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AmData(
                        g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                        Math.Round(g.Where(a => a.Ph.HasValue).Select(a => a.Ph ?? 0).Average(a => a), 2))),
                    //ox
                    2 => assays
                    .Where(a => a.Oxigen.HasValue)
                    .GroupBy(e => e.SamplingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AmData(
                        g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                        Math.Round(g.Where(a => a.Oxigen.HasValue).Select(a => a.Oxigen ?? 0).Average(a => a), 2))),
                    //sal
                    3 => assays
                    .Where(a => a.Salinity.HasValue)
                    .GroupBy(e => e.SamplingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AmData(
                        g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                        Math.Round(g.Where(a => a.Salinity.HasValue).Select(a => a.Salinity ?? 0).Average(a => a), 2))),
                    //temp
                    _ => assays
                    .Where(a => a.Temperature.HasValue)
                    .GroupBy(e => e.SamplingDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new AmData(
                        g.Key.ToString(_dateFormat, CultureInfo.InvariantCulture),
                        Math.Round(g.Where(a => a.Temperature.HasValue).Select(a => a.Temperature ?? 0).Average(a => a), 2)))
                });
            }
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Graph()
        {
            DateTime minplank = _context.PlanktonAssays
                .AsNoTracking()
                .Min(e => e.SamplingDate);
            DateTime maxplank = _context.PlanktonAssays
                .AsNoTracking()
                .Max(e => e.SamplingDate);
            if (_context.Variables.Any())
            {
                DateTime mincustom = _context.Variables
                    .AsNoTracking()
                    .Min(e => e.Date);
                DateTime maxcustom = _context.Variables
                    .AsNoTracking()
                    .Max(e => e.Date);
                bool plankNewer = minplank > mincustom;
                ViewData["start"] = plankNewer ?
                    mincustom.ToString(_dateFormat, CultureInfo.InvariantCulture)
                    : minplank.ToString(_dateFormat, CultureInfo.InvariantCulture);
                ViewData["end"] = plankNewer ?
                    maxplank.ToString(_dateFormat, CultureInfo.InvariantCulture) :
                    maxcustom.ToString(_dateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                ViewData["start"] = minplank.ToString(_dateFormat, CultureInfo.InvariantCulture);
                ViewData["end"] = maxplank.ToString(_dateFormat, CultureInfo.InvariantCulture);
            }
            return View();
        }
    }
}