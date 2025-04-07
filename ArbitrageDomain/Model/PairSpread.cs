using ArbitrageDomain.Model.Enums;

namespace ArbitrageDomain.Model
{
    public class PairSpread
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Futures FirstFutures { get; set; }
        public Futures SecondFutures { get; set; }
        public decimal FirstFuturesPrice { get; set; }
        public decimal SecondFuturesPrice { get; set; }
        public decimal SpreadValue { get; set; }
    }
}
