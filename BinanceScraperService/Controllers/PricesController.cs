using BinanceScraperService.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BinanceScraperService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricesController : ControllerBase
    {
        private readonly ScraperFactory _scraperFactory;
        public PricesController(ScraperFactory scraperFactory)
        {
            _scraperFactory = scraperFactory;
        }

        [HttpGet("{pairName}")]
        public async Task<IActionResult> GetSpreads(string pairName)
        {
            var scraper = _scraperFactory.Get(pairName);
            if (scraper == null)
                return NotFound($"Scraper with name '{pairName}' not found.");

            var data = await scraper.GetPricesAsync();
            return Ok(data);
        }
    }
}
