using BiblioMit.Models.PostViewModels;

namespace BiblioMit.Models
{
    public class HomeIndexModel
    {
        public string? SearchQuery { get; set; }
        public IEnumerable<PostListingModel> LatestPosts { get; set; } = new List<PostListingModel>();
    }
}
