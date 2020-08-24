using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RedditIsBad.Data;
using RedditIsBad.Utility;

namespace RedditIsBad.Pages
{
    public class UploadVideoModel : PageModel
    {
        [BindProperty]
        public string Title { get; set; }
        [BindProperty]
        public IFormFile FileUpload { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAmazonS3 _amazonS3;
        private readonly ApplicationDbContext _context;

        private const string BucketName = "redditisdumb-video-uploads";

        public UploadVideoModel(
            IHttpContextAccessor httpContextAccessor,
            IAmazonS3 amazonS3,
            ApplicationDbContext context)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _amazonS3 = amazonS3;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // 1. upload video to s3
                // TODO check stream processing
                await using var memoryStream = new MemoryStream();
                FileUpload.CopyTo(memoryStream);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = memoryStream,
                    Key = Title,
                    BucketName = BucketName,
                    CannedACL = S3CannedACL.PublicRead
                };

                var ftu = new TransferUtility(_amazonS3);
                await ftu.UploadAsync(uploadRequest);

                // 2. save s3 link in db
                var curUser = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var video = new Video
                {
                    S3Url = $"{Constants.VideoUploadS3BaseUrl}/{Title}",
                    Title = this.Title,
                    UserId = curUser
                };
                _context.Add(video);
                await _context.SaveChangesAsync();
                return new RedirectToPageResult("/Index");

            }
            catch (Exception e)
            {
                // idk handle later
                throw;
            }
        }
    }
}
