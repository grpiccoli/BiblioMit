using BiblioMit.Controllers;
using BiblioMit.Data;
using BiblioMit.Services.Interfaces;
using Microsoft.Extensions.Localization;
using System.Text.Json;
using System.Globalization;
using System.Reflection;
using BiblioMit.Extensions;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Models.VM;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Centres;

namespace BiblioMit.Services
{
    public class UpdateJsons : IUpdateJsons, IDisposable
    {
        private bool _disposed;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly string _jsonPath;
        private readonly IStringLocalizer<UpdateJsons> _localizer;
        public UpdateJsons(
            IWebHostEnvironment environment,
            ApplicationDbContext context,
            IStringLocalizer<UpdateJsons> localizer
            )
        {
            _localizer = localizer;
            _context = context;
            _environment = environment;
            _jsonPath = Path.Combine(
                _environment.ContentRootPath,
                "StaticFiles",
                "json");
        }
        public void SeedUpdate()
        {
            WriteJson(nameof(CuencaData));
            WriteJson(nameof(ComunaData));
            WriteJson(nameof(OceanVarList));
            WriteJson(nameof(CuencaList));
            WriteJson(nameof(ComunaList));
            WriteJson(nameof(CustomVarList));
            WriteJson(nameof(TLList));

            WriteJson(nameof(RegionList));

            WriteJson(nameof(GetPhotos));

            CenterUpdate();
            PlanktonUpdate();
        }
        public void CenterUpdate()
        {
            WriteJson(nameof(ComunaResearchList));
            WriteJson(nameof(ComunaFarmList));
            WriteJson(nameof(ProvinciaResearchList));
            WriteJson(nameof(ProvinciaFarmList));
            WriteJson(nameof(InstitutionList));
            WriteJson(nameof(CompanyList));
            WriteJson(nameof(ResearchList));
            WriteJson(nameof(FarmList));
            WriteJson(nameof(ResearchData));
            WriteJson(nameof(FarmData));
        }
        public void PlanktonUpdate()
        {
            WriteJson(nameof(PsmbData));
            WriteJson(nameof(PsmbList));
            WriteJson(nameof(GroupVarList));
            WriteJson(nameof(GenusVarList));
            WriteJson(nameof(SpeciesVarList));
        }
        private void WriteJson(string function)
        {
            Type magicType = GetType();
            MethodInfo? magicMethod = magicType.GetMethod(function);
            if (magicMethod != null)
            {
                var json = (string?)magicMethod.Invoke(this, Array.Empty<object>());
                if (!string.IsNullOrWhiteSpace(json))
                {
                    string name = CultureInfo.InvariantCulture.TextInfo.ToLower(function);
                    File.WriteAllText(
                        Path.Combine(
                            _jsonPath,
                            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                            $"{name}.json"),
                        json);
                }
            }
        }
        private string GetPhotos()
        {
            var photos = _context.Photos
                            .Include(p => p.Individual)
                                .ThenInclude(i => i.Sampling)
                            .AsNoTracking()
                            .ToList();

            List<NanoGalleryElement> gallery = photos.Select(photo => 
            new NanoGalleryElement(
                $"Photos/GetImg?f={photo.Key}&d=DB",
                $"Photos/GetImg?f={photo.Key}&d=DB/Thumbs",
                photo.Comment,
                $"{photo.IndividualId}{photo.Id}")
            {
                AlbumId = photo.IndividualId.ToString(CultureInfo.InvariantCulture)
            }).ToList();

            gallery.AddRange(photos.Select(p => p.Individual)
            .Select(i => new NanoGalleryElement(
                $"Photos/GetImg?f={i.Photos.First().Key}&d=DB",
                $"Photos/GetImg?f={i.Photos.First().Key}&d=DB/Thumbs",
                i.Id.ToString(CultureInfo.InvariantCulture),
                i.Id.ToString(CultureInfo.InvariantCulture)
                )
            {
                AlbumId = i.SamplingId.ToString(CultureInfo.InvariantCulture),
                Kind = "album"
            }));

            gallery.AddRange(photos.Select(p => p.Individual.Sampling)
            .Select(s => new NanoGalleryElement(
                $"Photos/GetImg?f={s.Individuals.First().Photos.First().Key}&d=DB",
                $"Photos/GetImg?f={s.Individuals.First().Photos.First().Key}&d=DB/Thumbs",
                s.Id.ToString(CultureInfo.InvariantCulture),
                s.Id.ToString(CultureInfo.InvariantCulture)
                )
            {
                Kind = "album"
            }));

            return JsonSerializer.Serialize(gallery, JsonCase.CamelMin);
        }
        private string CuencaList()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            ChoicesGroup data = new()
            {
                Label = _localizer["Catchment Areas"],
                Choices = _context.CatchmentAreas
                .AsNoTracking()
                .Select(c => new ChoicesItem
                {
                    Value = c.Id,
                    Label = singlabel + c.Name
                })
            };
            return JsonSerializer.Serialize(data, JsonCase.CamelMin);
        }
        private string CustomVarList() => JsonSerializer.Serialize(new ChoicesGroup
        {
            Label = _localizer["Custom Variables"],
            Choices = _context.VariableTypes
            .AsNoTracking()
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id,
                        Label = com.Name + " (" + com.Units + ")"
                    })
        }, JsonCase.CamelMin);
        private string RegionList() => JsonSerializer.Serialize(new ChoicesGroup
        {
            Label = _localizer["Regions"],
            Choices = _context.Regions
            .AsNoTracking()
                    .Select(com => new ChoicesItem
                    {
                        Value = com.Id,
                        Label = com.Name
                    })
        }, JsonCase.CamelMin);
        private string ComunaList()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            IQueryable<ChoicesGroup> data = Comuna(singlabel);
            return JsonSerializer.Serialize(data, JsonCase.CamelMin);
        }
        private IQueryable<ChoicesGroup> Comuna(string singlabel)
            => _context.CatchmentAreas
    .AsNoTracking()
    .Select(c => new ChoicesGroup
    {
        Label = singlabel + c.Name,
        Choices = c.Communes
            .Select(com => new ChoicesItem
            {
                Value = com.Id,
                Label = com.Name + " " + c.Name
            })
    });
        private string ProvinciaFarmList() => JsonSerializer.Serialize(Provincia(), JsonCase.CamelMin);
        private string ComunaFarmList()
        {
            var singlabel = _localizer["Catchment Area"] + " ";
            return JsonSerializer.Serialize(Comuna(singlabel), JsonCase.CamelMin);
        }
        private string ProvinciaResearchList() => JsonSerializer.Serialize(_context.Regions
            .AsNoTracking()
            .Select(c => new ChoicesGroup
            {
                Label = c.Name,
                Choices = c.Provinces
                .Where(c => c.Communes.Any(c => c.Psmbs.Any(p => p.Discriminator == Models.Entities.Centres.PsmbType.ResearchCentre)))
                        .Select(com => new ChoicesItem
                        {
                            Value = com.Id,
                            Label = com.Name + ", " + c.Name
                        })
            }), JsonCase.CamelMin);
        private string ComunaResearchList() => JsonSerializer.Serialize(_context.Provinces
            .AsNoTracking()
            .Select(c => new ChoicesGroup
            {
                Label = c.Name,
                Choices = c.Communes
                .Where(c => c.Psmbs.Any(p => p.Discriminator == Models.Entities.Centres.PsmbType.ResearchCentre))
                        .Select(com => new ChoicesItem
                        {
                            Value = com.Id,
                            Label = com.Name + ", " + c.Name
                        })
            }), JsonCase.CamelMin);
        public IQueryable<ChoicesGroup> Provincia() =>
    _context.Regions
    .AsNoTracking()
    .Select(c => new ChoicesGroup
    {
        Label = c.Name,
        Choices = c.Provinces
            .Select(com => new ChoicesItem
            {
                Value = com.Id,
                Label = com.Name + ", " + c.Name
            })
    });
        private string CompanyList() => JsonSerializer.Serialize(_context.Companies
    .AsNoTracking()
            .Where(p => p.Id > 900_000 && p.Psmbs.Any(f => f.Discriminator == Models.Entities.Centres.PsmbType.Farm && f.PolygonId.HasValue))
            .Select(p => new ChoicesItem
            {
                Value = p.Id,
                Label = p.BusinessName + " (" + p.GetRUT() + ")"
            }), JsonCase.CamelMin);
        private string ResearchList() => JsonSerializer.Serialize(
            _context.ResearchCentres.AsNoTracking()
            .Where(p => p.PolygonId.HasValue)
            .Select(p => new ChoicesItem
            {
                Value = p.CompanyId,
                Label = p.Name + " (" + p.Acronym + ")"
            }),
            JsonCase.CamelMin);
        private string InstitutionList() => JsonSerializer.Serialize(_context.Companies
            .AsNoTracking()
                    .Where(p => p.Psmbs.Any(p => p.Discriminator == Models.Entities.Centres.PsmbType.ResearchCentre))
                    .Select(p => new ChoicesItem
                    {
                        Value = p.Id,
                        Label = p.BusinessName + " (" + p.Acronym + ")"
                    }), JsonCase.CamelMin);
        private static string OceanVarList() =>
            JsonSerializer.Serialize(
                Blazor.Variable.t.Enum2ChoicesGroup("v").FirstOrDefault(),
                JsonCase.CamelMin);
        private string TLList() =>
    JsonSerializer.Serialize(new List<object>
    {
                new ChoicesGroup
                {
                    Label = _localizer["Analysis"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 14,
                            Label = _localizer["Capture per Species"]
                        },
                        new ChoicesItem{
                            Value = 17,
                            Label = "% "+_localizer["Species"]
                        },
                        new ChoicesItem{
                            Value = 11,
                            Label = _localizer["% Size per Species"]
                        },
                        new ChoicesItem{
                            Value = 12,
                            Label = _localizer["Larvae"]
                        },
                        new ChoicesItem{
                            Value = 13,
                            Label = _localizer["IG Spawners"]
                        },
                        new ChoicesItem{
                            Value = 15,
                            Label = "% "+_localizer["Reproductive Stage"]
                        },
                        new ChoicesItem{
                            Value = 16,
                            Label = "% "+_localizer["Sex"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = "PSMBs",
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 20,
                            Label = "10219 Quetalco"
                        },
                        new ChoicesItem{
                            Value = 21,
                            Label = "10220 Vilipulli"
                        },
                        new ChoicesItem{
                            Value = 22,
                            Label = "10431 Carahue"
                        },
                        new ChoicesItem{
                            Value = 23,
                            Label = _localizer["All PSMBs"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Species"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 31,
                            Label = "Chorito (<i>Mytilus chilensis</i>)"
                        },
                        new ChoicesItem{
                            Value = 32,
                            Label = "Cholga (<i>Aulacomya atra</i>)"
                        },
                        new ChoicesItem{
                            Value = 33,
                            Label = "Choro (<i>Choromytilus chorus</i>)"
                        },
                        new ChoicesItem{
                            Value = 34,
                            Label = _localizer["All Species"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Size (%)"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 40,
                            Label = "0 - 1 (mm)"
                        },
                        new ChoicesItem{
                            Value = 41,
                            Label = "1 - 2 (mm)"
                        },
                        new ChoicesItem{
                            Value = 42,
                            Label = "2 - 5 (mm)"
                        },
                        new ChoicesItem{
                            Value = 43,
                            Label = "5 - 10 (mm)"
                        },
                        new ChoicesItem{
                            Value = 44,
                            Label = "10 - 15 (mm)"
                        },
                        new ChoicesItem{
                            Value = 45,
                            Label = "15 - 20 (mm)"
                        },
                        new ChoicesItem{
                            Value = 46,
                            Label = "20 - 25 (mm)"
                        },
                        new ChoicesItem{
                            Value = 47,
                            Label = "25 - 30 (mm)"
                        },
                        new ChoicesItem{
                            Value = 48,
                            Label = _localizer["All sizes"]
                        },
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Larva Type (count)"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 50,
                            Label = _localizer["D-Larva"]
                        },
                        new ChoicesItem{
                            Value = 51,
                            Label = _localizer["Umbanate Larva"]
                        },
                        new ChoicesItem{
                            Value = 52,
                            Label = _localizer["Eyed Larva"]
                        },
                        new ChoicesItem{
                            Value = 53,
                            Label = _localizer["Total Larvae"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Reproductive Stage"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 60,
                            Label = _localizer["Maturing"]
                        },
                        new ChoicesItem{
                            Value = 61,
                            Label = _localizer["Mature"]
                        },
                        new ChoicesItem{
                            Value = 62,
                            Label = _localizer["Spawned"]
                        },
                        new ChoicesItem{
                            Value = 63,
                            Label = _localizer["Spawning"]
                        }
                    }
                },
                new ChoicesGroup
                {
                    Label = _localizer["Sex"],
                    Choices = new List<ChoicesItem>{
                        new ChoicesItem{
                            Value = 70,
                            Label = _localizer["Female"]
                        },
                        new ChoicesItem{
                            Value = 71,
                            Label = _localizer["Male"]
                        }
                    }
                }
    }, JsonCase.CamelMin);
        private string CuencaData()
        {
            var title = _localizer["Catchment Area"] + " ";
            IQueryable<GMapPolygon> data = _context.CatchmentAreas
                .AsNoTracking()
                .Select(c => new GMapPolygon
                {
                    Id = c.Id,
                    Name = title + c.Name,
                    Position = c.Polygon.Vertices.Select(o =>
                    new GMapCoordinate
                    {
                        Lat = o.Latitude,
                        Lng = o.Longitude
                    })
                });
            return JsonSerializer.Serialize(data, JsonCase.CamelMin);
        }
        private string ComunaData()
        {
            var title = _localizer["Commune"] + " ";
            IQueryable<GMapMultiPolygon> data = _context.Communes
                .AsNoTracking()
                .Where(com => com.CatchmentAreaId.HasValue)
                .Select(com => new GMapMultiPolygon
                {
                    Id = com.Id,
                    Name = title + com.Name,
                    Provincia = com.Province.Name,
                    Position = com.Polygons
                    .Select(p => p.Vertices.Select(o => new GMapCoordinate
                    {
                        Lat = o.Latitude,
                        Lng = o.Longitude
                    }))
                });
            return JsonSerializer.Serialize(data, JsonCase.CamelMin);
        }
        public string FarmList() => JsonSerializer.Serialize(_context.Farms
    .AsNoTracking()
            .Where(p => p.PolygonId.HasValue)
            .Select(p => new ChoicesItem
            {
                Value = p.Id,
                Label = p.Code + " " + p.Name
            }), JsonCase.CamelMin);
        public string ResearchData() => JsonSerializer.Serialize(_context.ResearchCentres
    .AsNoTracking()
            .Where(c => c.PolygonId.HasValue && c.CommuneId.HasValue)
            .Select(c => new GMapPolygonCentre
            {
                Id = c.Id,
                Name = c.Name + " (" + c.Acronym + ")",
                Comuna = c.CommuneNN.Name,
                ComunaId = c.CommuneIdNN,
                Provincia = c.CommuneNN.Province.Name,
                Region = c.CommuneNN.Province.Region.Name,
                BusinessName = c.CompanyNN.BusinessName,
                Rut = c.CompanyIdNN,
                Position = c.PolygonNN
                .Vertices.Select(o => new GMapCoordinate
                {
                    Lat = o.Latitude,
                    Lng = o.Longitude
                })
            }), JsonCase.CamelMin);
        public string FarmData() => JsonSerializer.Serialize(_context.PsmbAreas
    .AsNoTracking()
    .Where(c => c.PolygonId.HasValue && c.CommuneId.HasValue)
    .Select(c => new GMapPolygonCentre
    {
        Id = c.Id,
        Name = c.Code + " " + c.Name ?? "",
        Comuna = c.CommuneNN.Name,
        ComunaId = c.CommuneIdNN,
        Provincia = c.CommuneNN.Province.Name,
        Code = c.Code,
    //BusinessName = c.Company.BusinessName ?? "",
    //Rut = c.CompanyId.Value,
    Position = c.PolygonNN
                .Vertices.Select(o => new GMapCoordinate
                {
                    Lat = o.Latitude,
                    Lng = o.Longitude
                })
    }), JsonCase.CamelMin);
        public string PsmbData() => JsonSerializer.Serialize(SelectPsmbs(_context.PsmbAreas
            .AsNoTracking()
            .Where(c => c.CommuneNN.CatchmentAreaId.HasValue && c.PolygonId.HasValue && c.PlanktonAssays.Any())), JsonCase.CamelMin);
        private static IQueryable<GMapPolygon> SelectPsmbs(IQueryable<PsmbArea> psmbs) =>
    psmbs.Select(c => new GMapPolygon
    {
        Id = c.Id,
        Name = c.Code + " " + c.Name,
        Comuna = c.CommuneNN.Name,
        Provincia = c.CommuneNN.Province.Name,
        Code = c.Code,
        Position = c.PolygonNN
                .Vertices.Select(o => new GMapCoordinate
                {
                    Lat = o.Latitude,
                    Lng = o.Longitude
                })
    });
        public string PsmbList() => JsonSerializer.Serialize(_context.Communes
    .AsNoTracking()
        .Where(c => c.CatchmentAreaId.HasValue)
        .Select(c => new ChoicesGroup
        {
            Label = c.Name,
            Choices = c.Psmbs
            .Where(p => p.PolygonId.HasValue && p.PlanktonAssays.Any())
            .Select(p => new ChoicesItem
            {
                Value = p.Id,
                Label = p.Code + " " + p.Name + " " + c.Name
            })
        }), JsonCase.CamelMin);
        public string GroupVarList()
        {
            var group = " (" + _localizer["Group"] + ")";
            return JsonSerializer.Serialize(new ChoicesGroup
            {
                Label = _localizer["Phylogenetic Groups (Cel/mL)"],
                Choices = _context.PhylogeneticGroups
                .AsNoTracking()
                .Select(p => new ChoicesItem
                {
                    Value = "f" + p.Id,
                    Label = p.Name + group
                })
            }, JsonCase.CamelMin);
        }
        public string GenusVarList()
        {
            var genus = " (" + _localizer["Genus"] + ")";
            return JsonSerializer.Serialize(new ChoicesGroup
            {
                Label = _localizer["Genera (Cel/mL)"],
                Choices = _context.GenusPhytoplanktons
                .AsNoTracking()
                .Select(p => new ChoicesItem
                {
                    Value = "g" + p.Id,
                    Label = p.Name + genus
                })
            }, JsonCase.CamelMin);
        }
        public string SpeciesVarList()
        {
            var sp = " (" + _localizer["Species"] + ")";
            return JsonSerializer.Serialize(new ChoicesGroup
            {
                Label = _localizer["Species"]+" (Cel/mL)",
                Choices = _context.SpeciesPhytoplanktons
                .AsNoTracking()
                .Select(p => new ChoicesItem
                {
                    Value = "s" + p.Id,
                    Label = p.Genus.Name + " " + p.Name + sp
                })
            });
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _context?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }
    }
}
