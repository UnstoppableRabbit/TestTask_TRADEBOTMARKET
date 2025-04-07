using ArbitrageDomain.Model.Enums;

namespace ArbitrageDomain.Config
{
    public class BinanceScraperConfig
    {
        public string QuoteAsset { get; set; }
        public string BaseAsset { get; set; }
        public Futures FirstFuturesCode { get; set; }
        public string FirstContractType { get; set; }
        public Futures SecondFuturesCode { get; set; }
        public string SecondContractType { get; set; }
        public string Interval { get; set; }
    }
}
