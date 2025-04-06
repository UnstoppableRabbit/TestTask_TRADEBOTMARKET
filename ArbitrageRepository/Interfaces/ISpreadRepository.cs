using ArbitrageDomain.Model;

namespace ArbitrageDomain.Interfaces
{
    public interface ISpreadRepository
    {
        public void SaveSpread(PairSpread spread);
        public Task SaveSpreadAsync(PairSpread spread);
    }
}
