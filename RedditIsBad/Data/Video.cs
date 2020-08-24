using Microsoft.AspNetCore.Identity;

namespace RedditIsBad.Data
{
    public class Video
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string S3Url { get; set; }


        public string UserId { get; set; }
        public IdentityUser UploadingUser { get; set; }
    }
}
