using Microsoft.AspNetCore.Mvc;
using BiblioMit.Models;
using Microsoft.AspNetCore.Authorization;
using BiblioMit.Models.PostViewModels;
using BiblioMit.Models.ForumViewModels;
using BiblioMit.Models.HomeViewModels;
using Microsoft.AspNetCore.Localization;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using System.Globalization;
using BiblioMit.Services;
using Microsoft.Extensions.Localization;
using BiblioMit.Models.VM;
using BiblioMit.Services.Interfaces;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using BiblioMit.Views.Components.Nav;

namespace BiblioMit.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IPuppet _puppet;
        private readonly IBannerService _banner;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IPost _postService;
        //private readonly INodeService _nodeService;
        private readonly DateTime _startDate;
        private readonly ILogger<HomeController> _logger;
        public HomeController(
            IPuppet puppet,
            IBannerService banner,
            SignInManager<ApplicationUser> signInManager,
            ILogger<HomeController> logger,
            IStringLocalizer<HomeController> localizer,
            IPost postService
            //INodeService nodeService
            )
        {
            _signInManager = signInManager;
            _startDate = new DateTime(2018, 8, 28);
            _banner = banner;
            _puppet = puppet;
            _localizer = localizer;
            _postService = postService;
            //_nodeService = nodeService;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Flowpaper(string n)
        {
            //var name = Request.Path.Value.Split("/").Last();
            var locale = _localizer["en_US"].Value;
            var model = n switch
            {
                "gallery" => new Flowpaper { Name = "colecci-n-virtual", Reload = 1516301843374, LocaleChain = locale },
                _ => new Flowpaper { Name = "MANUAL_DE_USO_BIBLIOMIT", Reload = 1512490982155, LocaleChain = locale }
            };
            return View("Flowpaper", model);
        }
        [HttpGet]
        public IActionResult GetBanner(string f)
        {
            var name = Regex.Replace(f, ".*/", "");

            var full = Path.Combine(Directory.GetCurrentDirectory(),
                                    "StaticFiles", "BannerImgs", name);

            return PhysicalFile(full, "image/jpg");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email,Password,RememberMe")] LoginDropdownModel Input)
        {
            var returnUrl = HttpContext.Request.Path.Value ?? "~/";
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("/Account/LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe, area = "Identity" });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("/Account/Lockout", new { area = "Identity" });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return RedirectToPage("/Account/Login", new { ReturnUrl = returnUrl, area = "Identity" });
                }
            }
            return RedirectToPage("/Account/Login", new { ReturnUrl = returnUrl, area = "Identity" });
        }
        [HttpGet]
        public IActionResult Manual()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Survey()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Terms()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Responses()
        {
            var uri = new Uri("https://docs.google.com/forms/d/e/1FAIpQLSdtgpabkbTL8eXZ1PJuyNzEkyAtX_eIdX7_84cO6aAMHxUKyQ/viewanalytics");
            var page = await _puppet
                        .GetPageAsync(uri)
                        .ConfigureAwait(false);
            var model = await page.GetContentAsync().ConfigureAwait(false);
            return View("Responses", model);
        }
        [HttpGet]
        public IActionResult Analytics()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetAnalyticsDataMonth()
        {
            using var service = GetService();
            var request = new GetReportsRequest
            {
                ReportRequests = new List<ReportRequest>
                {
                    new ReportRequest
                    {
                        ViewId = "180792983",
                        Metrics = new List<Metric>
                        {
                            new Metric
                            {
                                Expression = "ga:pageviews"
                            }
                        },
                        Dimensions = new List<Dimension>
                        {
                            new Dimension { Name = "ga:year" },
                            new Dimension { Name = "ga:month" }
                        },
                        DateRanges = new List<DateRange>
                        {
                            new DateRange
                            {
                                StartDate = _startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                EndDate =  "today"
                            }
                        }
                    }
                }
            };
            var batchRequest = service.Reports.BatchGet(request);
            var response = batchRequest.Execute();

            var result = response.Reports.First().Data.Rows
                .Select(r => new AmData
                (
                    date: string.Join("-", r.Dimensions),
                    value: int.Parse(r.Metrics.First().Values.First(), CultureInfo.InvariantCulture)
                ));
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAnalyticsData()
        {
            using var service = GetService();
            var request = new GetReportsRequest
            {
                ReportRequests = new List<ReportRequest>
                {
                    new ReportRequest
                    {
                        ViewId = "180792983",
                        Metrics = new List<Metric>
                        {
                            new Metric
                            {
                                Expression = "ga:pageviews"
                            }
                        },
                        DateRanges = new List<DateRange>
                        {
                            new DateRange
                            {
                                StartDate = _startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                EndDate =  "today"
                            }
                        }
                    }
                }
            };
            var batchRequest = service.Reports.BatchGet(request);
            var response = batchRequest.Execute();
            var cnt = response.Reports.First().Data.Totals.First().Values.First();
            return Json(cnt);
        }

        public static AnalyticsReportingService GetService()
        {
            var credential = GetCredential();

            return new AnalyticsReportingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "BiblioMit",
            });
        }

        public static GoogleCredential GetCredential()
        {
            using var stream = new FileStream("BiblioMit-cb7f4de3a209.json", FileMode.Open, FileAccess.Read);
            return GoogleCredential.FromStream(stream)
.CreateScoped(AnalyticsReportingService.Scope.AnalyticsReadonly);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        //public string Translate(string text, string to)
        //{
        //    var translated = _nodeService.Run("./wwwroot/js/translate.js", new string[] { text, to });
        //    return translated;
        //}

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Simac()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _banner.GetCarouselAsync().ConfigureAwait(false);
            return View(model);
        }

        [HttpGet]
        public IActionResult UserManage()
        {
            return RedirectToPage("/Account/Manage", new { area = "Identity" });
        }
        [HttpGet]
        public IActionResult Forum()
        {
            var model = BuildHomeIndexModel();
            return View(model);
        }
        [HttpGet]
        public IActionResult Results(string searchQuery)
        {
            var posts = _postService.GetFilteredPosts(searchQuery);
            var noResults = (!string.IsNullOrEmpty(searchQuery) && !posts.Any());
            var postListings = posts.Select(p => new PostListingModel
            {
                Id = p.Id,
                AuthorId = p.User.Id,
                AuthorName = p.User.UserName,
                AuthorRating = p.User.Rating,
                Title = p.Title,
                DatePosted = p.Created.ToString(CultureInfo.InvariantCulture),
                RepliesCount = p.Replies.Count(),
                Forum = BuildForumListing(p)
            });

            var model = new SearchResultModel
            {
                Posts = postListings,
                SearchQuery = searchQuery,
                EmptySearchResults = noResults
            };
            return View(model);
        }

        private static ForumListingModel BuildForumListing(Post p)
        {
            var forum = p.Forum;
            return new ForumListingModel
            {
                Id = forum.Id,
                ImageUrl = forum.ImageUrl,
                Name = forum.Title,
                Description = forum.Description
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(string searchQuery)
        {
            return RedirectToAction("Results", new { searchQuery });
        }

        private HomeIndexModel BuildHomeIndexModel()
        {
            return new HomeIndexModel
            {
                LatestPosts = _postService.GetLatestsPosts(5).Select(p => new PostListingModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    AuthorId = p.UserId,
                    AuthorName = p.User.UserName,
                    AuthorRating = p.User.Rating,
                    DatePosted = p.Created.ToString(CultureInfo.InvariantCulture),
                    RepliesCount = p.Replies.Count(),
                    Forum = GetForumListingForPost(p)
                }),
                SearchQuery = string.Empty
            };
        }

        private static ForumListingModel GetForumListingForPost(Post post)
        {
            var forum = post.Forum;
            return new ForumListingModel
            {
                Name = forum.Title,
                Id = forum.Id,
                ImageUrl = forum.ImageUrl
            };
        }
        [HttpGet]
        public IActionResult About()
        {
            ViewData["Message"] = _localizer["About BiblioMit"];

            return View();
        }
        [HttpGet]
        public IActionResult Contact()
        {
            ViewData["Message"] = _localizer["Contact"];

            return View();
        }
        [HttpGet]

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SetLanguage(string culture, Uri returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        HttpOnly = true,
                        Path = "/",
                        Secure = true
                    }
                );
            }

            if (returnUrl == null)
            {
                returnUrl = new Uri("~/", UriKind.Relative);
            }

            return LocalRedirect(returnUrl.ToString());
        }
    }
    public class VisitCount
    {
        public string Date { get; set; }
        public int Views { get; set; }
    }
}
