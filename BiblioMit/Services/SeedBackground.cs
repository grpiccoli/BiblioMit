using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

            using var scope = Services.CreateScope();
            var scopedProcessingService = scope.ServiceProvider.GetRequiredService<ISeed>();

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
        static partial void LogSeedStarted(ILogger logger);
        [LoggerMessage(32, LogLevel.Error, "{message}.")]
        static partial void LogError(ILogger logger, string message);
    }
}
