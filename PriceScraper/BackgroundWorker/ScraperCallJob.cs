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
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<List<PairSpread>>(response);
                foreach (var item in result) 
                {
                    var anyLikeThis = await _spreadRepository.AnySpreadToUpdate(item);
                    if (anyLikeThis.Item1)                                  //здесь я, в случае если в прошлый раз не удалось корректно получить цену на 1 из фьючерсов,
                                                                            //и при этом данные по этой дате пришли в этой итерации переписываю их в базе
                       await _spreadRepository.UpdateSpreadAsync(anyLikeThis.Item2, item.FirstFuturesPrice, item.SecondFuturesPrice); 
                    else
                        await _spreadRepository.SaveSpreadAsync(item);
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