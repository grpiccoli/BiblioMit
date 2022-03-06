using BiblioMit.Models;
using BiblioMit.Models.ForumViewModels;
using BiblioMit.Models.HomeViewModels;
using BiblioMit.Models.PostViewModels;
using BiblioMit.Models.VM;
using BiblioMit.Services;
using BiblioMit.Services.Interfaces;
using BiblioMit.Views.Components.Nav;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PuppeteerSharp;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BiblioMit.Controllers
{
    [AllowAnonymous]
    [ResponseCache(Duration = 60 * 60 * 24 * 365, VaryByQueryKeys = new string[] { "*" })]
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
        private readonly IWebHostEnvironment _env;
        public HomeController(
            IWebHostEnvironment env,
            IPuppet puppet,
            IBannerService banner,
            SignInManager<ApplicationUser> signInManager,
            ILogger<HomeController> logger,
            IStringLocalizer<HomeController> localizer,
            IPost postService
            //INodeService nodeService
            )
        {
            _env = env;
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
            string locale = _localizer["en_US"].Value;
            Flowpaper model = n switch
            {
                "gallery" => new Flowpaper { Name = "colecci-n-virtual", Reload = 1516301843374, LocaleChain = locale },
                _ => new Flowpaper { Name = "MANUAL_DE_USO_BIBLIOMIT", Reload = 1512490982155, LocaleChain = locale }
            };
            return View("Flowpaper", model);
        }
        [HttpGet]
        [ResponseCache(Duration = 60)]
        public IActionResult GetBanner(string f)
        {
            string name = Regex.Replace(f, ".*/", "");
            string full = Path.Combine(_env.ContentRootPath,
                                    "StaticFiles", "BannerImgs", name);
            return PhysicalFile(full, "image/jpg");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Login([Bind("Email,Password,RememberMe")] LoginDropdownModel Input)
        {
            string returnUrl = HttpContext.Request.Path.Value ?? "~/";
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
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
            Uri uri = new ("https://docs.google.com/forms/d/e/1FAIpQLSdtgpabkbTL8eXZ1PJuyNzEkyAtX_eIdX7_84cO6aAMHxUKyQ/viewanalytics");
            Page page = await _puppet
                        .GetPageAsync(uri)
                        .ConfigureAwait(false);
            string model = await page.GetContentAsync().ConfigureAwait(false);
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
            using AnalyticsReportingService service = GetService();
            GetReportsRequest request = new()
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
            ReportsResource.BatchGetRequest batchRequest = service.Reports.BatchGet(request);
            GetReportsResponse response = batchRequest.Execute();

            IEnumerable<AmData> result = response.Reports.First().Data.Rows
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
            using AnalyticsReportingService service = GetService();
            GetReportsRequest request = new()
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
            ReportsResource.BatchGetRequest batchRequest = service.Reports.BatchGet(request);
            GetReportsResponse response = batchRequest.Execute();
            string cnt = response.Reports.First().Data.Totals.First().Values.First();
            return Json(cnt);
        }

        public static AnalyticsReportingService GetService()
        {
            GoogleCredential credential = GetCredential();

            return new AnalyticsReportingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "BiblioMit",
            });
        }

        public static GoogleCredential GetCredential()
        {
            using FileStream stream = new("BiblioMit-cb7f4de3a209.json", FileMode.Open, FileAccess.Read);
            return GoogleCredential.FromStream(stream)
.CreateScoped(AnalyticsReportingService.Scope.AnalyticsReadonly);
        }

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
        [ResponseCache(Duration = 60)]
        public IActionResult Index()
        {
            return View(_banner.ReadCarousel(true, true, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName));
        }

        [HttpGet]
        public IActionResult UserManage()
        {
            return RedirectToPage("/Account/Manage", new { area = "Identity" });
        }
        [HttpGet]
        public IActionResult Forum()
        {
            HomeIndexModel model = BuildHomeIndexModel();
            return View(model);
        }
        [HttpGet]
        public IActionResult Results(string searchQuery)
        {
            IEnumerable<Post> posts = _postService.GetFilteredPosts(searchQuery);
            bool noResults = (!string.IsNullOrEmpty(searchQuery) && !posts.Any());
            IEnumerable<PostListingModel> postListings = posts.Select(p => new PostListingModel
            {
                Id = p.Id,
                AuthorId = p.UserId,
                AuthorName = p.User?.UserName,
                AuthorRating = p.User != null ? p.User.Rating : 0,
                Title = p.Title,
                DatePosted = p.Created.ToString(CultureInfo.InvariantCulture),
                RepliesCount = p.Replies.Count(),
                Forum = BuildForumListing(p)
            });

            SearchResultModel model = new()
            {
                Posts = postListings,
                SearchQuery = searchQuery,
                EmptySearchResults = noResults
            };
            return View(model);
        }

        private static ForumListingModel BuildForumListing(Post p)
        {
            Forum? forum = p.Forum;
            return new ForumListingModel
            {
                Id = forum != null ? forum.Id : 0,
                ImageUrl = forum?.ImageUrl,
                Name = forum?.Title,
                Description = forum?.Description
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
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
                    AuthorName = p.User?.UserName,
                    AuthorRating = p.User != null ? p.User.Rating : 0,
                    DatePosted = p.Created.ToString(CultureInfo.InvariantCulture),
                    RepliesCount = p.Replies.Count(),
                    Forum = GetForumListingForPost(p)
                }),
                SearchQuery = string.Empty
            };
        }

        private static ForumListingModel GetForumListingForPost(Post post)
        {
            Forum forum = post.Forum;
            return new ForumListingModel
            {
                Name = forum?.Title,
                Id = forum != null ? forum.Id : 0,
                ImageUrl = forum?.ImageUrl
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
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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
}
