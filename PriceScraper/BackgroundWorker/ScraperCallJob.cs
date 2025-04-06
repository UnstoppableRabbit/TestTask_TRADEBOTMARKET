using ArbitrageDomain.Interfaces;
using ArbitrageDomain.Model;
using Newtonsoft.Json;
using Quartz;

namespace PriceScraper.Scheduler.BackgroundWorker
{
    public class ScraperCallJob : IJob
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ScraperCallJob> _logger;
        private readonly ISpreadRepository _spreadRepository;

        public ScraperCallJob(IHttpClientFactory httpClientFactory, ILogger<ScraperCallJob> logger, ISpreadRepository spreadRepository)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _spreadRepository = spreadRepository;
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
                _logger.LogInformation("Starting scraper call to {Url} at {Time}", url, DateTimeOffset.Now);

                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<List<PairSpread>>(response);

                _logger.LogInformation("Received {Count} items from {Url}", result.Count, url);

                foreach (var item in result)
                {
                    var anyToUpdate = await _spreadRepository.AnySpreadToUpdate(item);
                    if (anyToUpdate.Item1)
                    {
                        await _spreadRepository.UpdateSpreadAsync(anyToUpdate.Item2, item.FirstFuturesPrice, item.SecondFuturesPrice);
                        _logger.LogInformation("Updated spread for {Date}", item.Date);
                    }
                    else
                    {
                        if (!(await _spreadRepository.AnySpreadLikeThis(item)).Item1)
                        {
                            await _spreadRepository.SaveSpreadAsync(item);
                            _logger.LogInformation("Saved new spread for {Date}", item.Date);
                        }
                    }
                }

                _logger.LogInformation("Scraper call to {Url} succeeded at {Time}", url, DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling scraper {Url} at {Time}", url, DateTimeOffset.Now);
            }
        }
    }
}