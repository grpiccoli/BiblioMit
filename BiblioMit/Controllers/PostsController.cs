using Microsoft.AspNetCore.Mvc;
using BiblioMit.Models;
using BiblioMit.Models.PostViewModels;
using BiblioMit.Models.ReplyViewModels;
using Microsoft.AspNetCore.Identity;
using BiblioMit.Services;
using Microsoft.AspNetCore.Authorization;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly IPost _postService;
        private readonly IApplicationUser _userService;
        private readonly IForum _forumService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostsController(
            IPost postService,
            IForum forumService,
            IApplicationUser userService,
            UserManager<ApplicationUser> userManager)
        {
            _postService = postService;
            _forumService = forumService;
            _userManager = userManager;
            _userService = userService;
        }
        [HttpGet]
        public IActionResult Index(int Id)
        {
            var post = _postService.GetById(Id);

            var replies = BuildPostReplies(post.Replies);

            var model = new PostIndexModel(
                post.Title,
                post.User.Id,
                post.User.Email,
                post.User.UserName,
                post.Forum.Title,
                post.User.ProfileImageUrl,
                replies
                )
            {
                Id = post.Id,
                AuthorRating = post.User.Rating,
                Created = post.Created,
                PostContent = post.Content,
                ForumId = post.ForumId,
            };
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create(int id)
        {
            var forum = _forumService.GetbyId(id);
            var model = new NewPostModel(
                forum.Title,
                forum.ImageUrl
            )
            { 
                AuthorName = User.Identity?.Name,
                ForumId = forum.Id
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> AddPost(NewPostModel model)
        {
            if (model == null) return NotFound();
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
            var post = BuildPost(model, user);

            await _postService.Add(post).ConfigureAwait(false);

            await _userService.UpdateUserRating(userId, typeof(Post)).ConfigureAwait(false);

            return RedirectToAction("Index", "Posts", new { id = post.Id } );
        }

        private static Post BuildPost(NewPostModel model, ApplicationUser user)
        {
            return new Post
            {
                Title = model.Title,
                Content = model.Content,
                Created = DateTime.Now,
                User = user,
                ForumId = model.ForumId
            };
        }

        private static IEnumerable<PostReplyModel> BuildPostReplies(IEnumerable<PostReply> replies)
        {
            return replies.Select(r => new PostReplyModel
            {
                Id = r.Id,
                AuthorName = r.User.UserName,
                AuthorId = r.User.Id,
                AuthorImageUrl = r.User.ProfileImageUrl,
                AuthorRating = r.User.Rating,
                Created = r.Created,
                ReplyContent = r.Content
            });
        }
    }
}