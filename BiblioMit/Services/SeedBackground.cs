namespace BiblioMit.Services
{
    public partial class SeedBackground : IHostedService
    {
        private readonly ILogger _logger;
        //private Timer _timer;
        public SeedBackground(
            IServiceProvider services,
            ILogger<SeedBackground> logger)
        {
            Services = services;
            _logger = logger;
        }
        public IServiceProvider Services { get; }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            LogSeedStarted(_logger);

            using IServiceScope scope = Services.CreateScope();
            ISeed scopedProcessingService = scope.ServiceProvider.GetRequiredService<ISeed>();

            try
            {
                await scopedProcessingService.Seed().ConfigureAwait(false);
            }
            catch (DirectoryNotFoundException ex)
            {
                LogError(_logger, ex.Message);
                throw;
            }
        }
        // noop
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        [LoggerMessage(31, LogLevel.Information, "Consume Scoped Service Hosted Service is working.")]
#pragma warning disable IDE0060 // Remove unused parameter
        static partial void LogSeedStarted(ILogger logger);
        [LoggerMessage(32, LogLevel.Error, "{message}.")]
        static partial void LogError(ILogger logger, string message);
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
