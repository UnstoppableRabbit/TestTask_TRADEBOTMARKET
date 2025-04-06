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
        private ILogger<BinanceScraperService> _logger;
        public BinanceScraperService(BinanceScraperConfig config, HttpClient httpClient, string name, ILogger<BinanceScraperService> logger)
        {
            _config = config;
            _httpClient = httpClient;
            Name = name;
            _logger = logger;
        }

        public async Task<(string firstFutures, string secondFutures)> GetQuarterSymbols()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching symbols: {ex.Message}");
                throw; 
            }
        }

        public async Task<List<Kline>> GetKlines(string symbol)
        {
            try
            {
                var result = new List<Kline>();
                var url = $"https://dapi.binance.com/dapi/v1/klines?symbol={symbol}&interval={_config.Interval}";
                var response = await _httpClient.GetStringAsync(url);
                var json = JsonDocument.Parse(response);

                foreach (var kline in json.RootElement.EnumerateArray())
                    result.Add(new Kline
                    {
                        OpenTime = kline[0].GetInt64(),
                        OpenPrice = decimal.Parse(kline[1].GetString(), CultureInfo.InvariantCulture)
                    });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching klines for symbol {symbol}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PairSpread>> GetPricesAsync()
        {
            try
            {
                var (firstSymbol, secondSymbol) = await GetQuarterSymbols();

                var firstKlines = await GetKlines(firstSymbol);
                var secondKlines = await GetKlines(secondSymbol);
                var grouppedKlines = new Dictionary<long, (Kline, Kline)>();

                var allTimes = firstKlines.Select(x => x.OpenTime)
                    .Union(secondKlines.Select(x => x.OpenTime))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                Kline? lastQuarter = null;
                Kline? lastBiQuarter = null;

                foreach (var time in allTimes)                                      //тут я группирую свечи соответственно условию (Важно учесть ситуацию, когда в новом измеряемом временном промежутке отсутствуют данные по одному или обоим фьючерсам: в этом случае необходимо использовать последнюю доступную цену за предыдущий период)
                {
                    var q = firstKlines.FirstOrDefault(x => x.OpenTime == time);
                    var bi = secondKlines.FirstOrDefault(x => x.OpenTime == time);

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
            catch (Exception ex)
            {
                _logger.LogError($"Error while fetching prices for futures: {ex.Message}");
                throw; 
            }
        }
    }
}
