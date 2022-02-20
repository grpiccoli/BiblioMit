namespace BiblioMit.Models.ForumViewModels
{
    public class ForumIndexModel
    {
        public IEnumerable<ForumListingModel> ForumListing { get; set; } = new List<ForumListingModel>();
    }
}
