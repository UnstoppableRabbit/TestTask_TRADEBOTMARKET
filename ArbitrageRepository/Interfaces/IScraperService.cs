using ArbitrageDomain.Model;

namespace ArbitrageDomain.Interfaces
{
    public interface IScraperService
    {
        string Name { get; }
        public Task<List<PairSpread>> GetPricesAsync();
    }
}
