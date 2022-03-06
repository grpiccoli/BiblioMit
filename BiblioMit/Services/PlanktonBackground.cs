using BiblioMit.Services.Interfaces;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Globalization;
//using System.Threading;
//using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public class PlanktonArguments
    {
        public bool Run { get; set; }
    }
    public partial class PlanktonBackground : IHostedService, IDisposable
    {
        private bool _disposed;
        private readonly ILogger _logger;
        private readonly Timer _timer;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();
        private readonly PlanktonArguments _arguments;
        public PlanktonBackground(
            IServiceProvider services,
            PlanktonArguments arguments,
            ILogger<PlanktonBackground> logger)
        {
            _arguments = arguments;
            _executingTask = Task.CompletedTask;
            if (_arguments.Run)
            {
                _timer = new Timer(FetchAssays, null, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(-1));
            }
            else
            {
                _timer = new Timer(FetchAssays, null, TimeToNextSaturdayMidnight(), TimeSpan.FromMilliseconds(-1));
            }
            Services = services;
            _logger = logger;
        }
        public IServiceProvider Services { get; }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            LogPlanktonServiceRunning(_logger);
            //TimeSpan time = TimeToNextSaturdayMidnight();
            //_timer = new Timer(FetchAssays, null, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));
            //_timer = new Timer(FetchAssays, null, TimeToNextSaturdayMidnight(), TimeSpan.FromMilliseconds(-1));

            return Task.CompletedTask;
        }
        private void FetchAssays(object? state)
        {
            _timer.Change(Timeout.Infinite, 0);
            _executingTask = FetchAssaysAsync(_stoppingCts.Token);
        }
        private async Task FetchAssaysAsync(CancellationToken stoppingToken)
        {
            using IServiceScope scope = Services.CreateScope();
            IPlanktonService scopedProcessingService = scope.ServiceProvider.GetRequiredService<IPlanktonService>();

            try
            {
                await scopedProcessingService.PullRecordsAsync(stoppingToken).ConfigureAwait(false);
                _timer.Change(TimeToNextSaturdayMidnight(), TimeSpan.FromMilliseconds(-1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        // noop
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            LogPlanktonServiceStopped(_logger);
            _timer.Change(Timeout.Infinite, 0);

            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer.Dispose();
                _stoppingCts?.Dispose();
            }
            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            _disposed = true;
        }
        private static TimeSpan TimeToNextSaturdayMidnight()
        {
            DateTime now = DateTime.Now;

            int daysUntilNextSaturday = (DayOfWeek.Saturday - now.DayOfWeek + 7) % 7;

            if (daysUntilNextSaturday == 0)
            {
                daysUntilNextSaturday = 7;
            }

            TimeSpan midnight = TimeSpan.FromDays(1);

            TimeSpan TimeSpanTilmidnight = midnight - now.TimeOfDay;

            return TimeSpan.FromDays(daysUntilNextSaturday) + TimeSpanTilmidnight;
        }
        [LoggerMessage(27, LogLevel.Information, "Plankton Service running is working.")]
#pragma warning disable IDE0060 // Remove unused parameter
        static partial void LogPlanktonServiceRunning(ILogger logger);
        [LoggerMessage(28, LogLevel.Information, "Plankton Service stopped.")]
        static partial void LogPlanktonServiceStopped(ILogger logger);
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
