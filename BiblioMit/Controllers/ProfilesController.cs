﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BiblioMit.Models;
using BiblioMit.Services;
using BiblioMit.Models.ProfileViewModels;
//using Microsoft.AspNetCore.Http;
//using Amazon.S3;
//using Amazon.S3.Model;
//using System.IO;
using Microsoft.AspNetCore.Authorization;
using BiblioMit.Data;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class ProfilesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUser _userService;

        public ProfilesController(
            UserManager<ApplicationUser> userManager,
            IApplicationUser userService)
        {
            _userManager = userManager;
            _userService = userService;
        }

        [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Forums))]
        [HttpGet]
        public IActionResult Index()
        {
            var profiles = _userService.GetAll()
                .OrderByDescending(user => user.Rating)
                .Select(u => new ProfileModel
                {
                    Email = u.Email,
                    UserName = u.UserName,
                    ProfileImageUrl = u.ProfileImageUrl,
                    UserRating = u.Rating,
                    MemberSince = u.MemberSince
                });

            var model = new ProfileListModel
            {
                Profiles = profiles
            };

            return View(model);
        }
        [HttpGet]
        public IActionResult Details(string id)
        {
            var user = _userService.GetById(id);
            var userRoles = _userManager.GetRolesAsync(user).Result;

            var model = new ProfileModel()
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserRating = user.Rating,
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl,
                MemberSince = user.MemberSince,
                IsAdmin = userRoles.Contains("Administrador")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult UploadProfileImage(
            //IFormFile file
            )
        {
            var userId = _userManager.GetUserId(User);

            //var accessKey = "AKIAISMYGSV5LKLHP25A";
            //var secretKey = "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";

            //var client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.SAEast1);

            //var filePath = Path.GetTempFileName();

            //var stream = new FileStream(filePath, FileMode.Create);

            //var request = new PutObjectRequest
            //{
            //    BucketName = "bucketmit",
            //    Key = userId,
            //    FilePath = filePath
            //};

            //var url = "https://" + request.BucketName + ".s3.amazonaws.com/" + request.Key;

            //var response = client.PutObjectAsync(request).GetAwaiter().GetResult();

            //_userService.SetProfileImage(userId, new Uri(url));

            return RedirectToAction("Details","Profiles",new { id = userId });
        }
    }
}