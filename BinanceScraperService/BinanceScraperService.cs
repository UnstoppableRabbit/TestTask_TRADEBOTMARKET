using ArbitrageDomain.Config;
using ArbitrageDomain.Interfaces;
using ArbitrageDomain.Model;
using System.Globalization;
using System.Text.Json;

namespace BinanceScraperService
{
    public class BinanceScraperService : IScraperService
    {
        public string Name { get; }
        private BinanceScraperConfig _config;
        private readonly HttpClient _httpClient;
        public BinanceScraperService(BinanceScraperConfig config, HttpClient httpClient, string name)
        {
            _config = config;
            _httpClient = httpClient;
            Name = name;
        }

        public async Task<(string firstFutures, string secondFutures)> GetQuarterSymbols()
        {
            var url = "https://dapi.binance.com/dapi/v1/exchangeInfo";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            string firstFutures = null, secondFutures = null;

            foreach (var symbol in json.RootElement.GetProperty("symbols").EnumerateArray())
            {
                var baseAsset = symbol.GetProperty("baseAsset").GetString();
                var quoteAsset = symbol.GetProperty("quoteAsset").GetString();
                var contractType = symbol.GetProperty("contractType").GetString();
                var symbolName = symbol.GetProperty("symbol").GetString();

                if (baseAsset == _config.BaseAsset && quoteAsset == _config.QuoteAsset)
                {
                    if (contractType == _config.FirstContractType)
                        firstFutures = symbolName;
                    else if (contractType == _config.SecondContractType)
                        secondFutures = symbolName;
                }
            }

            return (firstFutures, secondFutures);
        }

        public async Task<List<Kline>> GetKlines(string symbol)
        {
            var result = new List<Kline>();
            var url = $"https://dapi.binance.com/dapi/v1/klines?symbol={symbol}&interval={_config.Interval}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            foreach(var kline in json.RootElement.EnumerateArray())
            result.Add(new Kline
            {
                OpenTime = kline[0].GetInt64(),
                OpenPrice = decimal.Parse(kline[1].GetString(), CultureInfo.InvariantCulture)
            });
            return result;
        }

        public async Task<List<PairSpread>> GetPricesAsync()
        {
            // Получаем символы для квартального и би-квартального фьючерса
            var (quarterSymbol, biQuarterSymbol) = await GetQuarterSymbols();

            // Получаем последнюю свечу для каждого символа
            var quarterKlines = await GetKlines(quarterSymbol);
            var biQuarterKlines = await GetKlines(biQuarterSymbol);
            var grouppedKlines = new Dictionary<long, (Kline, Kline)>();

            var allTimes = quarterKlines.Select(x => x.OpenTime)
                .Union(biQuarterKlines.Select(x => x.OpenTime))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            Kline? lastQuarter = null;
            Kline? lastBiQuarter = null;

            foreach (var time in allTimes)
            {
                var q = quarterKlines.FirstOrDefault(x => x.OpenTime == time);
                var bi = biQuarterKlines.FirstOrDefault(x => x.OpenTime == time);

                if (q != null) lastQuarter = q;
                if (bi != null) lastBiQuarter = bi;

                if (lastQuarter != null && lastBiQuarter != null)
                {
                    grouppedKlines[time] = (lastQuarter, lastBiQuarter);
                }
            }
            var result = new List<PairSpread>();
            foreach (var kline in grouppedKlines)
            {
                result.Add(new PairSpread 
                { 
                    Date = DateTimeOffset.FromUnixTimeMilliseconds(kline.Value.Item1.OpenTime).UtcDateTime, //тут можно привести дату к localTime если этого требует бизнес-логика
                    FirstFutures = _config.FirstFuturesCode,
                    SecondFutures = _config.SecondFuturesCode,
                    FirstFuturesPrice = kline.Value.Item1.OpenPrice,
                    SecondFuturesPrice = kline.Value.Item2.OpenPrice
                });

            }
            return result;
        }
    }
}
