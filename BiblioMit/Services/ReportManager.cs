using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace BiblioMit.Services
{
    public static class ReportManager
    {
        /// <summary>
        /// Intializes and returns Analytics Reporting Service Instance using the parameters stored in key file
        /// </summary>
        /// <param name="keyFileName"></param>
        /// <returns></returns>
        private static AnalyticsReportingService GetAnalyticsReportingServiceInstance(string keyFileName)
        {
            string[] scopes = { AnalyticsReportingService.Scope.AnalyticsReadonly }; //Read-only access to Google Analytics
            GoogleCredential credential;
            using (var stream = new FileStream(keyFileName, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
            }
            // Create the  Analytics service.
            return new AnalyticsReportingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GA Reporting data extraction example",
            });
        }

        /// <summary>
        /// Fetches all required reports from Google Analytics
        /// </summary>
        /// <param name="reportRequests"></param>
        /// <returns></returns>
        public static GetReportsResponse GetReport(GetReportsRequest getReportsRequest)
        {
            string? config = System.Configuration.ConfigurationManager.AppSettings["KeyFileName"];
            if (string.IsNullOrWhiteSpace(config)) throw new MissingFieldException($"missing KeyFileName entry in csproj config file");
            using var analyticsService = GetAnalyticsReportingServiceInstance(config);
            return analyticsService.Reports.BatchGet(getReportsRequest).Execute();
        }
    }
}
