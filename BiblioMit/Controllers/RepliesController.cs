﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BiblioMit.Models;
using Microsoft.AspNetCore.Identity;
using BiblioMit.Models.ReplyViewModels;
using BiblioMit.Services;
using Microsoft.AspNetCore.Authorization;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class RepliesController : Controller
    {
        private readonly IPost _postService;
        private readonly IApplicationUser _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RepliesController(
            IPost postService,
            IApplicationUser userService,
            UserManager<ApplicationUser> userManager)
        {
            _postService = postService;
            _userManager = userManager;
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var post = _postService.GetById(id);
            var user = await _userManager.FindByNameAsync(User.Identity.Name).ConfigureAwait(false);

            var model = new PostReplyModel
            {
                PostContent = post.Content,
                PostTitle = post.Title,
                PostId = post.Id,
                AuthorId = user.Id,
                AuthorName = User.Identity.Name,
                AuthorRating = user.Rating,

                ForumName = post.Forum.Title,
                ForumId = post.ForumId,
                ForumImageUrl = post.Forum.ImageUrl,

                Created = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> AddReply(PostReplyModel model)
        {
            if (model == null) return NotFound();
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            var reply = BuildReply(model, user) as PostReply;

            await _postService.AddReply(reply).ConfigureAwait(false);
            await _userService.UpdateUserRating(userId, typeof(PostReply)).ConfigureAwait(false);

            return RedirectToAction("Index", "Post", new { id = model.PostId });
        }

        private object BuildReply(PostReplyModel model, ApplicationUser user)
        {
            var post = _postService.GetById(model.PostId);

            return new PostReply
            {
                Post = post,
                Content = model.ReplyContent,
                Created = DateTime.Now,
                User = user
            };
        }
    }
}