using BiblioMit.Models.PostViewModels;

namespace BiblioMit.Models.HomeViewModels
{
    public class SearchResultModel
    {
        public IEnumerable<PostListingModel> Posts { get; set; } = new List<PostListingModel>();
        public string? SearchQuery { get; set; }
        public bool EmptySearchResults { get; set; }
    }
}
