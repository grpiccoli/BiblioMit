using BiblioMit.Models.PostViewModels;

namespace BiblioMit.Models.ForumViewModels
{
    public class ForumTopicModel
    {
        public ForumListingModel Forum { get; set; } = null!;
        public IEnumerable<PostListingModel> Post { get; set; } = Enumerable.Empty<PostListingModel>();
        public string? SearchQuery { get; set; }
    }
}
