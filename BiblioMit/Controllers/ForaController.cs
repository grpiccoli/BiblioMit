using BiblioMit.Data;
using BiblioMit.Models;
using BiblioMit.Models.ForumViewModels;
using BiblioMit.Models.PostViewModels;
//using Amazon.S3;
//using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class ForaController : Controller
    {
        private readonly IForum _forumService;
        private readonly IPost _postService;

        public ForaController(
            IForum forumService,
            IPost postService)
        {
            _forumService = forumService;
            _postService = postService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            IEnumerable<ForumListingModel> forums = _forumService.GetAll()
                .Select(f => new ForumListingModel
                {
                    Id = f.Id,
                    Name = f.Title,
                    Description = f.Description,
                    NumberOfPosts = f.Posts?.Count ?? 0,
                    NumberOfUsers = _forumService.GetActiveUsers(f.Id).Count(),
                    ImageUrl = f.ImageUrl,
                    HasRecentPost = _forumService.HasRecentPost(f.Id)
                });

            ForumIndexModel model = new()
            {
                ForumListing = forums.OrderBy(f => f.Name)
            };

            return View(model);
        }
        [HttpGet]
        public IActionResult Topic(int id, string searchQuery)
        {
            Forum forum = _forumService.GetbyId(id);

            IEnumerable<Post> posts = _postService.GetFilteredPosts(forum, searchQuery);

            IEnumerable<PostListingModel> postListings = posts.Select(p => new PostListingModel
            {
                Id = p.Id,
                AuthorId = p.UserId,
                AuthorRating = p.User is not null ? p.User.Rating : 0,
                AuthorName = p.User?.UserName,
                Title = p.Title,
                DatePosted = p.Created.ToString(CultureInfo.InvariantCulture),
                RepliesCount = p.Replies.Count(),
                Forum = BuildForumListing(p)
            });

            ForumTopicModel model = new()
            {
                Post = postListings,
                Forum = BuildForumListing(forum)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Search(int id, string searchQuery)
        {
            return RedirectToAction("Topic", new { id, searchQuery });
        }

        [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Forums))]
        [HttpGet]
        public IActionResult Create()
        {
            AddForumModel model = new();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Forums))]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> AddForum(AddForumModel model)
        {
            string imageUri = "/images/ico/bibliomit.svg";

            if (model?.ImageUpload != null)
            {
                imageUri = UploadForumImage(model.ImageUpload);
            }

            Forum forum = new()
            {
                Title = model?.Title,
                Description = model?.Description,
                Created = DateTime.Now,
                ImageUrl = new Uri(imageUri)
            };

            await _forumService.Create(forum).ConfigureAwait(false);

            return RedirectToAction("Index", "Fora");
        }

        private static string UploadForumImage(IFormFile file)
        {
            //string userId = _userManager.GetUserId(User);

            //string filePath = Path.GetTempFileName();

            //FileStream stream = new(filePath, FileMode.Create);

            //string accessKey = "AKIAISMYGSV5LKLHP25A";
            //string secretKey = "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";

            //var client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.SAEast1);

            //PutObjectRequest request = new()
            //{
            //    BucketName = "bucketmit",
            //    Key = userId,
            //    FilePath = filePath
            //};

            //var url = "https://" + request.BucketName + ".s3.amazonaws.com/" + request.Key;

            //var response = client.PutObjectAsync(request).GetAwaiter().GetResult();

            //_userService.SetProfileImage(userId, new Uri(url));

            //return url;
            return file.FileName;
        }

        private static ForumListingModel BuildForumListing(Post p)
        {
            Forum? forum = p.Forum;

            if (forum == null)
            {
                return new ForumListingModel();
            }

            return BuildForumListing(forum);
        }

        private static ForumListingModel BuildForumListing(Forum forum)
        {
            return new ForumListingModel
            {
                Id = forum.Id,
                Name = forum.Title,
                Description = forum.Description,
                ImageUrl = forum.ImageUrl
            };
        }
    }
}