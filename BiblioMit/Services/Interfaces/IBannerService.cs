using BiblioMit.Models.Entities.Ads;

namespace BiblioMit.Services.Interfaces
{
    public interface IBannerService
    {
        IQueryable<Banner> GetBanners();
        Task<List<Banner>> GetBannersAsync();
        Task<IList<Banner>> GetBannersShuffledAsync();
        Task<Carousel> GetCarouselAsync(bool activeOnly = true, bool shuffle = true);
    }
}
