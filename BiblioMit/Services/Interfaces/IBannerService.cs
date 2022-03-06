namespace BiblioMit.Services.Interfaces
{
    public interface IBannerService
    {
        void UpdateJsons();
        Dictionary<string, string> ReadCarousel(bool activeOnly, bool shuffle, string lang);
    }
}
