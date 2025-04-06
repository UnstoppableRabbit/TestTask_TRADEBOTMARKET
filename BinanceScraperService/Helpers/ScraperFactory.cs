using ArbitrageDomain.Interfaces;

namespace BinanceScraperService.Helpers
{
    public class ScraperFactory
    {
        private readonly IEnumerable<IScraperService> _scrapers;

        public ScraperFactory(IEnumerable<IScraperService> scrapers)
        {
            _scrapers = scrapers;
        }

        public IScraperService? Get(string name)
        {
            return _scrapers.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
