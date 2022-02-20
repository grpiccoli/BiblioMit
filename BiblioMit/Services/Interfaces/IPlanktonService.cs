namespace BiblioMit.Services.Interfaces
{
    public interface IPlanktonService
    {
        Task PullRecordsAsync(CancellationToken stoppingToken);
    }
}
