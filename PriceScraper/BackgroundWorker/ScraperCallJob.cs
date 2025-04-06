using Quartz;

namespace PriceScraper.Scheduler.BackgroundWorker
{
    public class ScraperCallJob : IJob
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ScraperCallJob> _logger;

        public ScraperCallJob(IHttpClientFactory httpClientFactory, ILogger<ScraperCallJob> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var url = context.JobDetail.JobDataMap.GetString("Url");
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogWarning("Job executed without a valid URL.");
                return;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Scraper call to {Url} succeeded at {Time}", url, DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling scraper {Url} at {Time}", url, DateTimeOffset.Now);
            }
        }
    }
}