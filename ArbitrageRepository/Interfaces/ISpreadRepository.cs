using ArbitrageDomain.Model;

namespace ArbitrageDomain.Interfaces
{
    public interface ISpreadRepository
    {
        public void SaveSpread(PairSpread spread);
        public Task SaveSpreadAsync(PairSpread spread);
        public void UpdateSpread(int id, decimal firstFuturesPrice, decimal secondFuturesPrice);
        public Task UpdateSpreadAsync(int id, decimal firstFuturesPrice, decimal secondFuturesPrice);
        public Task<(bool, int)> AnySpreadLikeThis(PairSpread spread);
    }
}
