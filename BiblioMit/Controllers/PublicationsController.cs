using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using BiblioMit.Models.VM;
using BiblioMit.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BiblioMit.Controllers
{
    [AllowAnonymous]
    public class PublicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INodeService _node;
        private readonly TextInfo _TI;

        public PublicationsController(
            ApplicationDbContext context,
            INodeService node)
        {
            _context = context;
            _node = node;
            _TI = new CultureInfo("es-CL", false).TextInfo;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(
            //string[] val, //array of filter:value
            string[] src, //List of engines to search
            string q, //search value
            string srt = "source", //value to sort by
            int pg = 1, //page
            int trpp = 20, //results per page
            bool asc = false //ascending or descending sort
            )
        {
            Stopwatch stopWatch = new();
            stopWatch.Start();

            #region Variables
            string order = asc ? "asc" : "desc";

            ViewData[nameof(srt)] = srt;
            ViewData[nameof(pg)] = pg;
            ViewData[nameof(q)] = q;
            ViewData[nameof(asc)] = asc;
            ViewData[nameof(trpp)] = trpp;
            IEnumerable<PublicationVM> publications = new List<PublicationVM>();
            #endregion
            #region universities dictionary
            Dictionary<string, string> ues = new()
            {
                { "uchile", "Universidad de Chile" },
                { "ula", "Universidad Los Lagos" },
                //{"utal","Universidad de Talca"},
                { "umag", "Universidad de Magallanes" },
                //{"ust", "Universidad Santo Tom\u00E1s"},
                { "ucsc", "Universidad Cat\u00F3lica de la Sant\u00EDsima Concepci\u00F3n" },
                { "uct", "Universidad Cat\u00F3lica de Temuco" },
                { "uach", "Universidad Austral de Chile" },
                { "udec", "Universidad de Concepci\u00F3n" },
                { "pucv", "Pontificia Universidad Cat\u00F3lica de Valpara\u00EDso" },
                { "puc", "Pontificia Universidad Cat\u00F3lica" },
            };
            #endregion
            #region diccionario Proyectos conicyt
            Dictionary<string, string> conicyt = new()
            {
                { "FONDECYT", "Fondo Nacional de Desarrollo Cient\u00EDfico y Tecnol\u00F3gico" },
                { "FONDEF", "Fondo de Fomento al Desarrollo Cient\u00EDfico y Tecnol\u00F3gico" },
                { "FONDAP", "Fondo de Financiamiento de Centros de Investigaci\u00F3n en \u00C1reas Prioritarias" },
                { "PIA", "Programa de Investigaci\u00F3n Asociativa" },
                { "REGIONAL", "Programa Regional de Investigaci\u00F3n Cient\u00EDfica y Tecnol\u00F3gica" },
                { "BECAS", "Programa Regional de Investigaci\u00F3n Cient\u00EDfica y Tecnol\u00F3gica" },
                { "CONICYT", "Programa Regional de Investigaci\u00F3n Cient\u00EDfica y Tecnol\u00F3gica" },
                { "PROYECTOS", "Programa Regional de Investigaci\u00F3n Cient\u00EDfica y Tecnol\u00F3gica" },
            };
            #endregion
            #region diccionario de Proyectos
            Dictionary<string, string> proj = conicyt.Concat(new Dictionary<string, string>() {
                    //{"FAP","Fondo de Administraci�n Pesquero"},//"subpesca"
                    {"FIPA","Fondo de Investigaci\u00F3n Pesquera y de Acuicultura"},//"subpesca"
                    {"CORFO","Corporaci\u00F3n de Fomento a la Producci\u00F3n"}//"corfo"
                }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            #endregion
            #region Art�culos Indexados
            Dictionary<string, string> gs = new()
            { { "gscholar", "Google Acad\u00E9mico" } };
            #endregion
            #region Patentes
            Dictionary<string, string> gp = new()
            { { "gpatents", "Google Patentes" } };
            #endregion
            ViewData[nameof(ues)] = ues;
            ViewData[nameof(proj)] = proj;
            ViewData[nameof(gs)] = gs;
            ViewData[nameof(gp)] = gp;
            Dictionary<string, string> repos = ues.Concat(proj).Concat(gs).Concat(gp).ToDictionary(x => x.Key, x => x.Value);
            if (src != null && src.Any())
            {
                ViewData["srcs"] = src;
                if (src[0].Contains(',', StringComparison.Ordinal)) src = src[0].Split(',');
                ViewData[nameof(src)] = src;

                if (!string.IsNullOrWhiteSpace(q))
                {
                    int ggl, rpp = (int)Math.Ceiling((double)trpp / src.Length);
                    string sort_by, srt_uach, srt_utal = srt;

                    switch (srt)
                    {
                        case "title":
                            sort_by = "dc.title_sort";
                            ggl = 0;
                            srt_uach = "ftitre";
                            break;
                        case "date":
                            sort_by = "dc.date.issued_dt";
                            ggl = 1;
                            srt_uach = "udate";
                            break;
                        default:
                            sort_by = "score";
                            srt_utal = "rnk";
                            ggl = 0;
                            srt_uach = "sdxscore";
                            break;
                    }
                    (IEnumerable<PublicationVM>, string, int)[]? pubs = await GetPubsAsync(src, q, rpp, pg, sort_by, order, srt_uach, ggl).ConfigureAwait(false);
                    var Publications = pubs.SelectMany(x => x.Item1);
                    //repositories where any results
                    Dictionary<Typep, Dictionary<string, int>> Results = pubs.Where(x => x.Item1.Any())
                        //group by type of result
                        .GroupBy(x => x.Item1.First().Typep)
                        //select groupped dictionaries of acronym + total results per repository
                        .Select(x => new KeyValuePair<Typep, Dictionary<string, int>>(x.Key, x.ToDictionary(x => x.Item2, x => x.Item3)))
                        .ToDictionary(x => x.Key, x => x.Value);

                    var chartData = new List<List<ChartResultsItem>>();
                    Dictionary<Typep, int> counts = new();
                    foreach (var r in Results)
                    {
                        IEnumerable<Color> gradient = GetGradients(Color.DarkGreen, Color.LightGreen, r.Value.Count);
                        counts.Add(r.Key, 0);
                        var t = r.Value.Select((value, i) =>
                        {
                            counts[r.Key] += value.Value;
                            var name = "";
                            if (ues.ContainsKey(value.Key))
                            {
                                name = ues[value.Key]
                                    .Replace("Universidad", "U.", StringComparison.Ordinal)
                                    .Replace("Cat�lica", "C.", StringComparison.Ordinal);
                            }
                            else if (repos.ContainsKey(value.Key))
                            {
                                name = repos[value.Key];
                            }
                            return new ChartResultsItem
                            {
                                Repositorio = $"{name} ({value.Key})",
                                Resultados = value.Value,
                                Color = ColorToHex(gradient.ElementAt(i))
                            };
                        }).ToList();
                        if (chartData.Any()) chartData[0].AddRange(t);
                        chartData.Add(t);
                    }
                    int low1, NoPages;
                    ViewData["NoPages"] = NoPages = Results.Any() ?
                        (int)Math.Ceiling(Results.Values.Sum(v => (double)v.Values.Aggregate((b, r) => b > r ? b : r) / rpp)) : 1;

                    ViewData["counts"] = counts;
                    ViewData["chartData"] = System.Text.Json.JsonSerializer.Serialize(chartData);
                    ViewData["arrow"] = asc ? "&#x25BC;" : "&#x25B2;";
                    ViewData["prevDisabled"] = pg == 1 ? "disabled" : "";
                    ViewData["nextDisabled"] = pg == NoPages ? "disabled" : "";
                    ViewData["low"] = low1 = pg > 6 ? pg - 5 : 1;
                    ViewData["high"] = NoPages > low1 + 6 ? low1 + 6 : NoPages;

                    publications = $"{srt}{order}" switch
                    {
                        "dateasc" => Publications.OrderBy(p => p.Date.Year),
                        "datedesc" => Publications.OrderByDescending(p => p.Date.Year),
                        "titleasc" => Publications.OrderBy(p => p.Title),
                        "titledesc" => Publications.OrderByDescending(p => p.Title),
                        _ =>
                            asc ?
                                Publications.OrderBy(p => p.Source) :
                                Publications.OrderByDescending(p => p.Source)
                    };
                }
            }
            else
            {
                ViewData["srcs"] = string.Empty;
                ViewData[nameof(src)] = Array.Empty<string>();
            }
            stopWatch.Stop();
            ViewData["runtime"] = stopWatch.ElapsedMilliseconds;
            ViewData["interval"] = Convert.ToInt32(stopWatch.ElapsedMilliseconds / 500);
            return View(publications);
        }
        [Authorize(Roles = nameof(RoleData.Administrator))]
        [HttpGet]
        public IActionResult Translate(string text, string lang)
        {
            var result = _node.Run("./wwwroot/js/translate.js", new string[] { text, lang });
            return Json(result);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Agenda(
            int? pg, //page
            int? trpp, //results per page
            string[] src, //List of engines to search
            string stt,
            string[] fund
            )
        {
            Stopwatch stopWatch = new();
            stopWatch.Start();

            #region Variables
            if (!pg.HasValue) pg = 1;
            if (!trpp.HasValue) trpp = 20;
            if (string.IsNullOrWhiteSpace(stt)) stt = "abierto";
            ViewData[nameof(src)] = src;
            ViewData["srcs"] = string.Join(",", src);
            ViewData[nameof(pg)] = pg;
            ViewData[nameof(trpp)] = trpp;
            ViewData["any"] = false;
            var Agendas = new List<AgendaVM>();
            #endregion

            //FIA //FIC //FOPA //FAP //FIP //FIPA
            var conicyt1 = new Dictionary<string, Uri>()
            {
                { "fondap", new Uri($"http://www.conicyt.cl/fondap/category/concursos/?estado={stt}") },
                { "becasconicyt", new Uri($"http://www.conicyt.cl/becasconicyt/category/fichas-concursos/?estado={stt}") },
                { "fondecyt", new Uri($"http://www.conicyt.cl/fondecyt/category/concursos/fondecyt-regular/?estado={stt}") },
                { "fondequip", new Uri($"http://www.conicyt.cl/fondequip/category/concursos/?estado={stt}") }
            };

            var conicyt2 = new Dictionary<string, Uri>()
            {
                { "fondef", new Uri("http://www.conicyt.cl/fondef/") },
                { "fonis", new Uri("http://www.conicyt.cl/fonis/") },
                { "pia", new Uri("http://www.conicyt.cl/pia/") },
                { "regional", new Uri("http://www.conicyt.cl/regional/") },
                { "informacioncientifica", new Uri("http://www.conicyt.cl/informacioncientifica/") },
                { "pai", new Uri("http://www.conicyt.cl/pai/") },
                { "pci", new Uri("http://www.conicyt.cl/pci/") },
                { "explora", new Uri("http://www.conicyt.cl/explora/") }
            };

            // p�ginas CONICYT 1
            var conicyt1_funds = fund.Intersect(conicyt1.Keys);
            if (conicyt1_funds.Any())
            {
                foreach (string fondo in conicyt1_funds)
                {
                    using IHtmlDocument? bc_doc = await GetDoc(conicyt1[fondo]).ConfigureAwait(false);
                    if (bc_doc == null) continue;
                    var co = GetCo("conicyt");
                    Agendas.AddRange(from n in bc_doc.QuerySelectorAll("div.lista_concurso")
                                     let cells = n.Children
                                     let title = cells?.ElementAt(0)?.QuerySelector("a")
                                     select new AgendaVM
                                     {
                                         Company = co,
                                         Fund = fondo.ToUpperInvariant() + " ("
                                         + bc_doc.QuerySelector("a[rel='home'] span")?.TextContent + ")",
                                         Title = title?.InnerHtml,
                                         MainUrl = GetUri(title),
                                         Start = GetDateAgenda(cells[1]),
                                         End = GetDateAgenda(cells[2])
                                     });
                }
            }

            //p�ginas CONICYT 2
            var conicyt2_funds = fund.Intersect(conicyt2.Keys);
            //$postParams = @{valtab='evaluacion';blogid='20'}
            //Invoke-WebRequest -UseBasicParsing http://www.conicyt.cl/fondef/wp-content/themes/fondef/ajax/getpostconcursos.php -Method POST -Body $postParams

            if (conicyt2_funds.Any())
            {
                foreach (string fondo in conicyt2_funds)
                {
                    var values = new Dictionary<string, string>
                            {
                                { "valtab", stt },
                                { "blogid", "20" }
                            };
                    using var content = new FormUrlEncodedContent(values);
                    using HttpClient bc = new();
                    using HttpResponseMessage response = await bc
                        .PostAsync(new Uri($"{conicyt2[fondo]}wp-content/themes/fondef/ajax/getpostconcursos.php"), content).ConfigureAwait(false);
                    Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var bc_doc = await GetDoc(stream).ConfigureAwait(false);
                    var bc_entrys = bc_doc.QuerySelectorAll("div > a");

                    if (bc_entrys is null) { continue; }
                    string Fund = "";
                    string Acrn = fondo.ToUpperInvariant();
                    Regex ress1 = new(@"[\d-]+");
                    string[] formats = { "yyyy", "yyyy-MM", "d-MM-yyyy" };
                    foreach (IElement entry in bc_entrys)
                    {
                        var Entry = new AgendaVM()
                        {
                            Company = _context.Companies.FirstOrDefault(c => c.Acronym == "CONICYT"),
                            Fund = Acrn + " (" + Fund + ")",
                            Title = entry.QuerySelector("h4")?.InnerHtml,
                            MainUrl = GetUri(entry),
                        };
                        string? text = entry.QuerySelector("p")?.Text();
                        if (text != null)
                        {
                            var parsed = DateTime.TryParseExact(ress1.Match(text).ToString(),
                                                    formats,
                                                    CultureInfo.InvariantCulture,
                                                    DateTimeStyles.None,
                                                    out DateTime Date);
                            if (parsed) Entry.End = Date;
                            Agendas.Add(Entry);
                        }
                    }
                }
            }

            //CORFO DIVIdIR POR REGION Y ACTOR?
            Regex ress = new(@"corfo\d+");
            var corfo_funds = fund.Where(item => ress.IsMatch(item));
            if (corfo_funds.Any())
            {
                foreach (string fondo in corfo_funds)
                {
                    var corfo = "https://www.corfo.cl/sites/cpp/programas-y-convocatorias?p=1456407859853-1456408533016-1456408024098-1456408533181&at=&et=&e=&o=&buscar_resultado=&bus=&r=";
                    var num = fondo.Replace("corfo", "", StringComparison.Ordinal);
                    using HttpClient bc = new();
                    using HttpResponseMessage bc_result = await bc.GetAsync(new Uri(corfo + num)).ConfigureAwait(false);
                    var stream = await bc_result.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var bc_doc = await GetDoc(stream).ConfigureAwait(false);
                    var bc_entrys = bc_doc.QuerySelectorAll("div.col-sm-12.areas > a");

                    foreach (IElement entry in bc_entrys)
                    {
                        if (entry.InnerHtml.Contains("Cerradas", StringComparison.Ordinal))
                        {
                            if (stt == "abierto" || stt == "proximo")
                            {
                                continue;
                            }

                            if ((entry.InnerHtml.Contains("En Evaluaci�n", StringComparison.Ordinal) && stt != "evaluacion")
                                || (!entry.InnerHtml.Contains("En Evaluaci�n", StringComparison.Ordinal) && stt == "evaluacion"))
                            {
                                continue;
                            }
                        }

                        var Entry = new AgendaVM()
                        {
                            Company = _context.Companies.FirstOrDefault(c => c.Acronym == "CORFO"),
                            Fund = "CORFO",
                            Title = entry.QuerySelector("h4")?.InnerHtml,
                            MainUrl = new Uri(new Uri(corfo + num), entry.Attributes["href"]?.Value),
                            Description = entry.QuerySelector("div.col-md-9.col-sm-8")?.Text()
                        };

                        Regex ress2 = new(@"[\d\/]+");
                        string[] formats = { "dd/MM/yyyy" };
                        string? text = entry.QuerySelector("li:nth-child(3)")?.Text();
                        if (text != null)
                        {
                            var parsed = DateTime.TryParseExact(ress2.Match(text).ToString(),
                                                    formats,
                                                    CultureInfo.InvariantCulture,
                                                    DateTimeStyles.None,
                                                    out DateTime Date);
                            if (parsed) Entry.End = Date;
                            Agendas.Add(Entry);
                        }
                    }
                }
            }

            ViewData["any"] = Agendas.Count > 0;
            ViewData["fund"] = fund;
            ViewData["conicyt1"] = conicyt1;
            ViewData["conicyt2"] = conicyt2;
            ViewData["stt"] = string.IsNullOrEmpty(stt) ? "" : stt.ToString(CultureInfo.InvariantCulture);
            ViewData["regiones"] = _context.Regions.ToList();
            //render
            stopWatch.Stop();
            ViewData["runtime"] = stopWatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture);
            ViewData["interval"] = Convert.ToInt32(stopWatch.ElapsedMilliseconds / 500);
            return View(Agendas);
        }

        private Task<(IEnumerable<PublicationVM>, string, int)[]> GetPubsAsync(
            string[] src, string q, int rpp, int pg, string sortBy, string order, string srtUach, int ggl)
        {
            var uchile = GetUchileAsync
                (
                src,
                //23s
                //sort_by   dc.date.issued_dt   dc.title_sort   score
                //order     asc                 desc
                new Uri($"http://repositorio.uchile.cl/discover?filtertype_1=type&filter_relational_operator_1=equals&filter_1=Tesis&submit_apply_filter=&query={q}&rpp={rpp}&page={pg}&sort_by={sortBy}&order={order}"),
                "p.pagination-info", 2,
                "div#aspect_discovery_SimpleSearch_div_search-results > div",
                "a",
                "span.ds-dc_contributor_author-authority",
                "div.artifact-info > span.publisher-date > span.date"
                );

            var ula = GetUlaAsync
                (
                src,
                new Uri($"http://medioteca.ulagos.cl/biblioscripts/titulo_claves.idc?texto={q}"),
                "font[face='Arial']:has(> a)",
                "a",
                "font > small:only-child",
                pg, rpp
                );

            var umag = GetUmagAsync
                (
                src,
                new Uri($"http://www.bibliotecadigital.umag.cl/discover?query={q}&rpp={rpp}&page={pg}"),
                "h2.lineMid > span:has(> span)", 1,
                "div.artifact-description",
                "div.artifact-title > a",
                "div.artifact-info > span.author > span",
                "div.artifact-info > span.publisher-date > span.date"
                );

            var ucsc = GetUcscAsync
                (
                src,
                //24s
                //sort_by   dc.date.issued_dt   dc.title_sort   score
                //order     asc                 desc
                new Uri($"http://repositoriodigital.ucsc.cl/discover?scope=25022009/6&submit=&query={q}&rpp={rpp}&page={pg}&sort_by={sortBy}&order={order}"),
                "p.pagination-info", 2,
                "div.ds-static-div.primary > div > div.artifact-description",
                "a",
                "div.artifact-info > span.author.h4 > small > span",
                "div.artifact-info > span.publisher-date.h4 > small > span.date"
                );

            var uct = GetUctAsync
                (
                src,
                //31s
                //sort_by   dc.date.issued_dt   dc.title_sort   score
                //order desc asc
                new Uri("http://repositoriodigital.uct.cl/discover?rpp=" +
                    $"{rpp}&page={pg}&query={q}&sort_by={sortBy}&order={_TI.ToLower(order)}"),
                "p.pagination-info", 2,
                "div#aspect_discovery_SimpleSearch_div_search-results > div",
                "a",
                "a[href*='img']",
                "a[href*='img']",
                "a[href*='img']"
                );

            var uach = GetUachAsync
                (
                src,
                //14s
                //sf        ftitre      fauteur     contributeur        udate       sdxscore
                new Uri("http://cybertesis.uach.cl/sdx/uach/resultats-filtree.xsp?biblio_op=or&figures_op=or&tableaux_op=or&citations_op=or&notes_op=or&base=documents&position=2&texte_op=or&titres=" +
                    $"{q}&tableaux={q}&figures={q}&biblio={q}&notes={q}&citations={q}&texte={q}&hpp={rpp}&p={pg}&sf={srtUach}"),
                "div[align='left']:has(> b.label)", 0,
                "td.ressource[valign='top'][align='left']",
                "td:not([valign='top']) > div > a", "span.url > a",
                "span.auteur",
                "span.date"
                );

            var udec = GetUdecAsync
            (
                src,
                //18s
                //sort_by   dc.date.issued_dt   dc.title_sort   score
                //order     asc                 desc
                new Uri("http://repositorio.udec.cl/discover?group_by=none&etal=0&rpp=" +
                                $"{rpp}&page={pg}&query={q}&sort_by={sortBy}&order={order}"),
                "h2.ds-div-head:has(> span)", 1,
                "ul.ds-artifact-list > ul > li > div.artifact-description",
                "div.artifact-title > a",
                "div.artifact-info > span.author > span",
                "div.artifact-info > span.publisher-date > span.date"
            );

            var pucv = GetPucvAsync
                (
                    src,
                    new Uri($"http://opac.pucv.cl/cgi-bin/wxis.exe/iah/scripts/?IsisScript=iah.xis&lang=es&base=BDTESIS&nextAction=search&exprSearch={q}&isisFrom={(pg - 1) * rpp + 1}"),
                    "div.rowResult > div.columnB:has(> a) > b", 0,
                    "div.contain:has(> div.selectCol)",
                    "tr > td > font > b > font > font",
                    "a[href*='pdf']", "a[href*='img']",
                    "tr > td > font > b:only-child",
                    "a[href*='indexSearch=AU']",
                    rpp
                );

            var puc = GetPucAsync
            (
                src,
                //15s
                //sort_by   dc.date.issued_dt   dc.title_sort   score
                //order     asc                 desc
                new Uri($"https://repositorio.uc.cl/discover?scope=11534/1&group_by=none&etal=0&rpp={rpp}&page={pg}&query={q}&sort_by={sortBy}&order={order}&submit=Go"),
                "//h2[@class='ds-div-head' and span]", 1,
                "//ul[@class='ds-artifact-list']/ul/li/div[@class='artifact-description']",
                ".//div[@class='artifact-title']/a",
                ".//div[@class='artifact-info']/span[@class='publisher-date']/span[@class='date']",
                ".//div[@class='artifact-info']/span[@class='author']/span"
            );

            var fondecyt = GetConicyt(src, "FONDECYT", "108045", rpp, sortBy, order, pg, q);
            var fondef = GetConicyt(src, "FONDEF", "108046", rpp, sortBy, order, pg, q);
            var fondap = GetConicyt(src, "FONDAP", "108044", rpp, sortBy, order, pg, q);
            var pia = GetConicyt(src, "PIA", "108042", rpp, sortBy, order, pg, q);
            var regional = GetConicyt(src, "REGIONAL", "108050", rpp, sortBy, order, pg, q);
            var becas = GetConicyt(src, "BECAS", "108040", rpp, sortBy, order, pg, q);
            var conicyt = GetConicyt(src, "CONICYT", "108088", rpp, sortBy, order, pg, q);
            var proyectos = GetConicyt(src, "PROYECTOS", "93475", rpp, sortBy, order, pg, q);

            var fipa = GetFipaAsync(src,
                new Uri($"http://subpesca-engine.newtenberg.com/mod/find/cgi/find.cgi?action=query&engine=SwisheFind&rpp={rpp}&cid=514&stid=&iid=613&grclass=&pnid=&pnid_df=&pnid_tf=&pnid_search=678,682,683,684,681,685,510,522,699,679&limit=200&searchon=&channellink=w3:channel&articlelink=w3:article&pvlink=w3:propertyvalue&notarticlecid=&use_cid_owner_on_links=&show_ancestors=1&show_pnid=1&cids=514&keywords={q}&start={(pg - 1) * rpp}&group=0&expanded=1&searchmode=undefined&prepnidtext=&javascript=1"),
                "p.PP", 2, "li > a");

            var corfo = GetCorfoAsync(src,
                //order     DESC            ASC
                //sort_by   dc.title_sort
                //group_by=none
                new Uri("http://repositoriodigital.corfo.cl/discover?query=" +
                    $"{q}&rpp={rpp}&page={pg}&group_by=none&etal=0&sort_by={sortBy.Replace(".issued", "", StringComparison.Ordinal)}&order={order.ToUpperInvariant()}"),
                "p.pagination-info", 2,
                "div.artifact-description", "a",
                "span.author > small",
                "span.date", "div.abstract"
                );

            var gscholar = GetGscholarAsync(src,
                new Uri($"https://scholar.google.com/scholar?q={q}&start={rpp * (pg - 1) + 1}&scisbd={ggl}"),
                "div.gs_ab_mdw:has(> b)",
                "div.gs_ri",
                "a",
                "h3.gs_rt",
                "div.gs_a", rpp, "gscholar"
                );

            var gpatents = GetGscholarAsync(src,
                new Uri($"https://scholar.google.cl/scholar?as_q={q}" +
                    "&as_epq=&as_oq=&as_eq=&as_occt=any&as_sauthors=&as_publication=Google+Patents&as_ylo=&as_yhi=&btnG=&hl=en&as_sdt=0%2C5&as_vis=1" +
                    $"&start={rpp * (pg - 1) + 1}&scisbd={ggl}"),
                "div.gs_ab_mdw:has(> b)",
                "div.gs_ri",
                "a",
                "h3.gs_rt",
                "div.gs_a", rpp, "gpatents"
                );

            return Task.WhenAll(
                uchile, ula, umag, ucsc, uct, uach, udec, pucv, puc,
                fondecyt, fondef, fondap, pia, regional, becas, conicyt, proyectos, fipa, corfo
                , gscholar, gpatents
                );
        }

        public static async Task<(IEnumerable<PublicationVM>, string, int)> GetGscholarAsync(string[] src,
Uri url, string NoResultsSelect, string nodeSelect, string quriSelect, string titleSelect,
string dateSelect, int rpp, string acronym)
        {
            if (src.Contains(acronym))
            {
                try
                {
                    Regex resss = new(@"([0-9]+,)*[0-9]+");
                    Regex yr = new(@"[0-9]{4}");
                    Regex aut = new(@"\A(?:(?![0-9]{4}).)*");
                    var co = new Company
                    {
                        Id = 55555555,
                        Address = "1600 Amphitheatre Parkway, Mountain View, CA"
                    };
                    co.SetAcronym(acronym);
                    co.SetBusinessName("Google Inc");
                    using IHtmlDocument? doc = await GetDoc(url).ConfigureAwait(false);
                    if (doc is null) return (new List<PublicationVM>(), acronym, 0);
                    return (from n in doc.QuerySelectorAll(nodeSelect).Take(rpp)
                            let t = n?.QuerySelector(titleSelect)?.TextContent
                            select new PublicationVM()
                            {
                                Source = acronym,
                                Uri = GetUri(n.QuerySelector(quriSelect)),
                                Title = t?[(t.LastIndexOf(']')
                                + 1)..],
                                Typep = Typep.Article,
                                Company = co,
                                Date = GetDateGS(n, dateSelect),
                                Authors = GetAuthorsGS(n, dateSelect)
                            }, acronym, GetNoResultsGS(doc, NoResultsSelect));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetCorfoAsync(string[] src,
Uri url, string NoResultsSelect, int NoResultsPos, string nodeSelect, string quriSelect,
string authorSelect, string dateSelect, string abstractSelect)
        {
            var acronym = "CORFO";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(60706000);
                    using var doc = await GetDoc(url).ConfigureAwait(false);
                    if (doc is null) return (new List<PublicationVM>(), acronym, 0);
                    return (from n in doc.QuerySelectorAll(nodeSelect)
                            let t = n?.QuerySelector(quriSelect)
                            select new PublicationVM()
                            {
                                Source = acronym,
                                Uri = GetUri(url, t),
                                Title = t?.TextContent,
                                Typep = Typep.Project,
                                Company = co,
                                Date = GetDate(n, dateSelect),
                                Authors = GetAuthorsCorfo(n, authorSelect),
                                Abstract = GetAbstract(n, abstractSelect)
                            }, acronym, GetNoResults(doc, NoResultsSelect, NoResultsPos));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetFipaAsync(string[] src,
Uri url, string NoResultsSelect, int NoResultsPos, string nodeSelect)
        {
            var acronym = "FIPA";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(60719000);
                    using var doc = await GetDoc(url).ConfigureAwait(false);
                    if (doc is null) return (new List<PublicationVM>(), acronym, 0);
                    return (from n in doc.QuerySelectorAll(nodeSelect)
                            select new PublicationVM()
                            {
                                Source = acronym,
                                Title = n.TextContent,
                                Typep = Typep.Project,
                                Uri = GetUri(new Uri("http://www.subpesca.cl/fipa/613/w3-article-88970.html"), n),
                                Company = co,
                            }, acronym, GetNoResults(doc, NoResultsSelect, NoResultsPos));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        public static string ColorToHex(Color color)
        {
            return "#" + color.R.ToString("X2", CultureInfo.InvariantCulture) +
                         color.G.ToString("X2", CultureInfo.InvariantCulture) +
                         color.B.ToString("X2", CultureInfo.InvariantCulture);
        }

        public static IEnumerable<Color> GetGradients(Color start, Color end, int steps)
        {
            if (steps > 2)
            {
                Color stepper = Color.FromArgb((byte)((end.A - start.A) / (steps - 1)),
                               (byte)((end.R - start.R) / (steps - 1)),
                               (byte)((end.G - start.G) / (steps - 1)),
                               (byte)((end.B - start.B) / (steps - 1)));

                for (int i = 0; i < steps; i++)
                {
                    yield return Color.FromArgb(start.A + (stepper.A * i),
                                                start.R + (stepper.R * i),
                                                start.G + (stepper.G * i),
                                                start.B + (stepper.B * i));
                }
            }
            else
            {
                yield return start;
                yield return end;
                yield break;
            }
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetConicyt(string[] src,
string acronym, string parameter, int rpp, string sortBy, string order, int? pg, string q)
        {
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(60915000);
                    var url = new Uri($"http://repositorio.conicyt.cl/handle/10533/{parameter}/discover?query={q}&page={pg - 1}&rpp={rpp}&sort_by={sortBy}&order={order}");
                    using var doc = await GetDoc(url).ConfigureAwait(false);
                    if (doc is null) return (new List<PublicationVM>(), acronym, 0);
                    return (doc.QuerySelectorAll("div.row.ds-artifact-item")
.Select(n => new PublicationVM()
{
    Source = acronym,
    Title = n.QuerySelector("h4.title-list")?.Text(),
    Typep = Typep.Project,
    Uri = GetUri(url, n.QuerySelector("div.artifact-description > a")),
    Authors = GetAuthors(n, "span.ds-dc_contributor_author-authority"),
    Date = GetDate(n, "span.date"),
    Company = co,
    Journal = GetJournalConicyt(n)
}), acronym, GetNoResults(doc, "p.pagination-info", 2));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetPucAsync(string[] src,
Uri url, string NoResultsSelect, int NoResultsPos, string nodeSelect, string quriSelect,
string dateSelect, string authorSelect)
        {
            var acronym = "puc";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(acronym);
                    var doc = await GetDocStream(url).ConfigureAwait(false);
                    return (from n in doc.QuerySelectorAll(nodeSelect)
                            let t = n?.QuerySelector(quriSelect)
                            select new PublicationVM()
                            {
                                Source = acronym,
                                Title = t?.Text(),
                                Typep = Typep.Thesis,
                                Uri = GetUri(url, t),
                                Authors = GetAuthors(n, authorSelect),
                                Date = GetDate(n, dateSelect),
                                Company = co,
                            }, acronym, GetNoResults(doc, NoResultsSelect, NoResultsPos));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetPucvAsync(string[] src,
Uri url, string NoResultsSelect, int NoResultsPos, string nodeSelect, string dateSelect,
string quriSelect, string quriSelectAlt, string titleSelect, string authorSelect, int rpp)
        {
            var acronym = "pucv";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(acronym);
                    using var doc = await GetDocStream(url).ConfigureAwait(false);
                    return (from n in doc.QuerySelectorAll(nodeSelect).Take(rpp)
                            let date = n.QuerySelector(dateSelect)?.Text()
                            select new PublicationVM()
                            {
                                Typep = Typep.Thesis,
                                Source = acronym,
                                Title = n.QuerySelector(titleSelect)?.TextContent,
                                Uri = GetUri(url, n.QuerySelector(quriSelect), n.QuerySelector(quriSelectAlt)),
                                Authors = GetAuthors(n, authorSelect),
                                Date = GetDate(date, date.Length - 4),
                                Company = co,
                            }, acronym, GetNoResults(doc, NoResultsSelect, NoResultsPos));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetUdecAsync(string[] src,
Uri url, string NoResultsSelect, int NoResultsPos, string nodeSelect, string quriSelect, string authorSelect, string dateSelect)
        {
            var acronym = "udec";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(acronym);
                    IHtmlDocument? doc = await GetDoc(url).ConfigureAwait(false);
                    if (doc is null) return (new List<PublicationVM>(), acronym, 0);
                    return (
    from n in doc.QuerySelectorAll(nodeSelect)
    let m = n.QuerySelector(quriSelect)
    select new PublicationVM()
    {
        Typep = Typep.Thesis,
        Source = acronym,
        Title = m?.TextContent,
        Uri = GetUri(url, m),
        Authors = GetAuthors(n, authorSelect),
        Company = co,
        Date = GetDate(n, dateSelect)
    }, acronym, GetNoResults(doc, NoResultsSelect, NoResultsPos));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetUachAsync(string[] src,
    Uri url, string NoResultsSelect, int NoResultsPos, string nodeSelect, string titleSelect,
    string quriSelect, string authorSelect, string dateSelect)
        {
            var acronym = "uach";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(acronym);
                    using var doc = await GetDoc(url).ConfigureAwait(false);
                    if (doc is null) return (new List<PublicationVM>(), acronym, 0);
                    return (doc.QuerySelectorAll(nodeSelect).Select(n => new PublicationVM()
                    {
                        Source = acronym,
                        Title = n.QuerySelector(titleSelect)?.TextContent,
                        Uri = GetUri(url, n.QuerySelector(quriSelect)),
                        Authors = GetAuthors(n, authorSelect),
                        Typep = Typep.Thesis,
                        Company = co,
                        Date = GetDate(n, dateSelect)
                    }), acronym, GetNoResults(doc, NoResultsSelect, NoResultsPos));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetUctAsync(string[] src,
Uri url, string NoResultsSelect, int NoResultsPos, string nodeSelect, string quriSelect, string journalSelect, string authorSelect, string dateSelect)
        {
            var acronym = "uct";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(acronym);
                    Regex regex = new("[a-zA-Z]");
                    using var doc = await GetDoc(url).ConfigureAwait(false);
                    if (doc is null) return (new List<PublicationVM>(), acronym, 0);
                    return (
from n in doc.QuerySelectorAll(nodeSelect)
let m = n.QuerySelector(quriSelect)
let j = n.QuerySelector(journalSelect)?.TextContent
let d = n.QuerySelector(dateSelect)?.TextContent
select new PublicationVM()
{
    //otros
    Typep = Typep.Thesis,
    Source = acronym,
    Title = m?.TextContent,
    Uri = GetUri(url, m),
    Journal = j,
    Authors = GetAuthors(n, authorSelect, regex),
    Company = co,
    Date = GetDate(d, j.LastIndexOf(",", StringComparison.Ordinal) + 2)
}, acronym, GetNoResults(doc, NoResultsSelect, NoResultsPos));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetUcscAsync(string[] src,
Uri url, string NoResultsSelect, int NoResultsPos, string nodeSelect, string quriSelect, string authorSelect, string dateSelect)
        {
            var acronym = "ucsc";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(acronym);
                    using var doc = await GetDoc(url).ConfigureAwait(false);
                    if (doc is null) return (new List<PublicationVM>(), acronym, 0);
                    return (
    from n in doc.QuerySelectorAll(nodeSelect)
    let m = n.QuerySelector(quriSelect)
    select new PublicationVM()
    {
        Source = acronym,
        Title = m?.TextContent,
        Uri = GetUri(url, m),
        Authors = GetAuthors(n, authorSelect),
        Typep = Typep.Thesis,
        Company = co,
        Date = GetDate(n, dateSelect)
    }, acronym, GetNoResults(doc, NoResultsSelect, NoResultsPos));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetUmagAsync(string[] src,
    Uri url, string NoResultsSelect, int NoResultsPos, string nodeSelect, string quriSelect, string authorSelect, string dateSelect)
        {
            var acronym = "umag";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(acronym);
                    using var doc = await GetDoc(url).ConfigureAwait(false);
                    if (doc is null) return (new List<PublicationVM>(), acronym, 0);
                    return (from n in doc.QuerySelectorAll(nodeSelect)
                            let m = n.QuerySelector(quriSelect)
                            select new PublicationVM()
                            {
                                Source = acronym,
                                Title = m?.TextContent,
                                Uri = GetUri(url, m),
                                Authors = GetAuthors(n, authorSelect),
                                Typep = Typep.Thesis,
                                Company = co,
                                Date = GetDate(n, dateSelect)
                            }, acronym, GetNoResults(doc, NoResultsSelect, NoResultsPos));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetUchileAsync(string[] src,
            Uri url, string NoResultsSelect, int NoResultsPos, string nodeSelect, string quriSelect, string authorSelect, string dateSelect)
        {
            var acronym = "uchile";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(acronym);
                    using var doc = await GetDoc(url).ConfigureAwait(false);
                    if (doc is null) return (new List<PublicationVM>(), acronym, 0);
                    return (doc.QuerySelectorAll(nodeSelect).Select(n => new PublicationVM()
                    {
                        Source = acronym,
                        Title = n.TextContent,
                        Uri = GetUri(url, n.QuerySelector(quriSelect)),
                        Authors = GetAuthors(n, authorSelect),
                        //Typep = GetTypep(n.QuerySelector("span.tipo_obra").Text().ToLower()),
                        Typep = Typep.Thesis,
                        Company = co,
                        Date = GetDate(n, dateSelect)
                    }), acronym, GetNoResults(doc, NoResultsSelect, NoResultsPos));
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        private async Task<(IEnumerable<PublicationVM>, string, int)> GetUlaAsync(string[] src,
    Uri url, string nodeSelect, string quriSelect, string authorSelect, int? pg, int rpp)
        {
            string acronym = "ula";
            if (src.Contains(acronym))
            {
                try
                {
                    var co = GetCo(acronym);
                    using var doc = await GetDocStream(url).ConfigureAwait(false);
                    var num = doc.QuerySelectorAll(nodeSelect);
                    if (pg == null) pg = 1;
                    return (num.Skip(rpp * (pg.Value - 1)).Take(rpp).Select(n => new PublicationVM()
                    {
                        Typep = Typep.Thesis,
                        Source = acronym,
                        Title = n.QuerySelector(quriSelect)?.TextContent ?? string.Empty,
                        Uri = GetUri(url, n.QuerySelector(quriSelect)),
                        Authors = GetAuthors(n, authorSelect),
                        Company = co,
                    }), acronym, num.Length);
                }
                catch (DomException de)
                {
                    Console.WriteLine(de.Message);
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return (new List<PublicationVM>(), acronym, 0);
        }

        public static (string?, string?) GetJournalDoi(IElement node, string acronym)
        {
            string doi = "https://dx.doi.org/";
            switch (acronym)
            {
                case "uchile":
                    string titls = node.QuerySelector("h4.discoUch span")?.Attributes["title"]?.Value ?? string.Empty;
                    Collection<int> indexes = titls.AllIndexesOf("rft_id") ?? new Collection<int>();
                    if (indexes.Count == 3)
                    {
                        return (QueryHelpers.ParseQuery(titls[indexes[0]..indexes[1]])["rft_id"],
                        doi + QueryHelpers.ParseQuery(titls[indexes[1]..indexes[2]])["rft_id"].ToString().Replace("doi: ", "", StringComparison.Ordinal));
                    }
                    return (null, null);
                default:
                    return (null, null);
            }
        }

        public static string? GetJournalConicyt(IElement node)
        {
            var items = node.QuerySelectorAll("#code");
            if (items.Any())
            {
                var journal = "N� de Proyecto: " + items[0].TextContent;
                if (items.Length > 3)
                {
                    return journal + " Instituci�n Responsable: " + items[3].TextContent;
                }
                return journal;
            }
            return null;
        }

        public static Typep GetTypep(string type)
        {
            return type switch
            {
                "tesis" => Typep.Thesis,
                "art�culo" => Typep.Article,
                _ => Typep.Unknown
            };
        }

        public static Uri GetUri(Uri rep, IElement? link)
        {
            if (link != null)
            {
                return new Uri(rep, link?.Attributes["href"]?.Value ?? string.Empty);
            }
            else
            {
                return new Uri("");
            }
        }

        public static Uri GetUri(Uri rep, IElement? link, IElement? backlink)
        {
            return new Uri(rep, backlink?.Attributes["href"]?.Value ?? link?.Attributes["href"]?.Value ?? string.Empty);
        }

        public static Uri GetUri(IElement? link)
        {
            return new Uri(link?.Attributes["href"]?.Value ?? string.Empty);
        }

        public static async Task<IHtmlDocument?> GetDoc(Uri rep)
        {
            try
            {
                var parser = new HtmlParser();
                using HttpClient hc = new();
                return await parser.ParseDocumentAsync(await hc.GetStringAsync(rep).ConfigureAwait(false)).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
        public static async Task<IHtmlDocument> GetDoc(Stream stream)
        {
            var parser = new HtmlParser();
            using HttpClient hc = new();
            return await parser.ParseDocumentAsync(stream).ConfigureAwait(false);
        }
        public static async Task<IHtmlDocument> GetDocStream(Uri rep)
        {
            var parser = new HtmlParser();
            using HttpClient hc = new();
            return await parser.ParseDocumentAsync(await hc.GetStreamAsync(rep).ConfigureAwait(false)).ConfigureAwait(false);
        }
        private Company GetCo(string u) =>
            _context.Companies.FirstOrDefault(c => c.Acronym == u) ?? new Company();

        private Company GetCo(int rut) =>
            _context.Companies.FirstOrDefault(c => c.Id == rut) ?? new Company();

        public static int GetNoResultsGS(IHtmlDocument doc, string selector)
        {
            Regex res = new(@"([0-9]+,)*[0-9]+");
            string? match = doc.QuerySelector(selector)?.TextContent;
            if (match is null) return 0;
            bool parsed = int.TryParse(res.Match(match).Value.Replace(",", "", StringComparison.Ordinal), out int result);
            return parsed ? result : 0;
        }

        public static int GetNoResults(IHtmlDocument doc, string selector, int pos)
        {
            Regex res = new(@"[\d\.,]+");
            string? match = doc.QuerySelector(selector)?.TextContent;
            if (match is null) return 0;
            bool parsed = int.TryParse(res.Matches(match)[pos].Value, out int result);
            return parsed ? result : 0;
        }
        public static DateTime GetDateGS(IElement node, string selector)
        {
            Regex res = new(@"[\d]+");
            string[] formats = { "yyyy" };
            string? match = node.QuerySelector(selector)?.TextContent;
            if (match is null) return new DateTime();
            bool parsed = DateTime.TryParseExact(res.Match(match).Value,
                                        formats,
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out DateTime Date);
            return parsed ? Date : new DateTime();
        }

        public static DateTime GetDate(IElement node, string selector)
        {
            Regex res = new(@"[\d\-]+");
            string[] formats = { "yyyy", "yyyy-MM", "yyyy-MM-dd" };
            string? match = node.QuerySelector(selector)?.TextContent;
            if (match is null) return new DateTime();
            var parsed = DateTime.TryParseExact(res.Match(match).Value,
                                    formats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out DateTime Date);
            return parsed ? Date : new DateTime();
        }

        public static DateTime GetDateAgenda(IElement node)
        {
            string[] formats = { "dd 'de' MMMM 'de'  yyyy" };
            Regex ress1 = new(@"\d[\dA-Za-z\s]+\d");
            var parsed = DateTime.TryParseExact(ress1.Match(node.TextContent).Value,
                formats,
                CultureInfo.GetCultureInfo("es-CL"),
                DateTimeStyles.None,
                out DateTime Date);
            return parsed ? Date : new DateTime();
        }

        public static DateTime GetDate(string journal, int start)
        {
            string[] formats = { "yyyy" };
            var parsed = DateTime.TryParseExact(journal.AsSpan(start, 4),
                                    formats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out DateTime Date);
            return parsed ? Date : new DateTime();
        }

        public static string GetAbstract(IElement node, string selector) =>
            node.QuerySelector(selector)?.TextContent ?? string.Empty;

        public static IEnumerable<AuthorVM> GetAuthorsGS(IElement node, string selector)
        {
            Regex aut = new(@"\A(?:(?![0-9]{4}).)*");
            string[] formats = { "yyyy", "yyyy-MM", "yyyy-MM-dd" };
            string? match = node.QuerySelector(selector)?.TextContent;
            if (match is null) return new List<AuthorVM>();
            return aut.Match(match).Value.Trim().Trim('-').Split(',')
                .Select(a => a.Split(' '))
                .Select(nn =>
                new AuthorVM
                {
                    Last = nn[0],
                    Name = nn.Length > 1 ? nn[1] : ""
                });
        }

        public static IEnumerable<AuthorVM> GetAuthorsCorfo(IElement node, string selector) =>
            node.QuerySelector(selector)?.TextContent.Split(';')
                .Select(nn =>
                new AuthorVM
                {
                    Name = nn
                }) ?? new List<AuthorVM>();

        public static IEnumerable<AuthorVM> GetAuthors(IElement node, string selector) =>
            node.QuerySelectorAll(selector)
                .Select(a => a.TextContent.TrimEnd('.').Split(','))
                .Select(nn =>
                new AuthorVM
                {
                    Last = nn[0],
                    Name = nn.Length > 1 ? nn[1] : ""
                }) ?? new List<AuthorVM>();
        public static IEnumerable<AuthorVM> GetAuthors(IElement node, string selector, Regex filter) =>
            node.QuerySelectorAll(selector)
                .Where(i => filter.IsMatch(i.TextContent))
                .Select(a => a.TextContent.Split(','))
                .Select(nn =>
                new AuthorVM
                {
                    Last = nn[0],
                    Name = nn.Length > 1 ? nn[1] : ""
                });
    }
    public class ChartResultsItem
    {
        public string? Repositorio { get; set; }
        public int Resultados { get; set; }
        public string? Color { get; set; }
    }

}
