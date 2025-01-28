using Binance.Net.Clients;
using Microsoft.Extensions.Logging;
using AlgoTradeBacktester.Core.Models;
using AlgoTradeBacktester.Core.Interfaces;
using Binance.Net.Enums;

namespace AlgoTradeBacktester.Services
{
    public class ExchangeService : IExchangeService
    {
        private readonly BinanceRestClient _client;
        private readonly ILogger<ExchangeService> _logger;

        public ExchangeService(ILogger<ExchangeService> logger)
        {
            _logger = logger;
            _client = new BinanceRestClient();
        }

        public async Task<List<Candle>> GetHistoricalDataAsync(
            string symbol,
            string interval,
            DateTime startTime,
            DateTime endTime)
        {
            try
            {
                var klines = await _client.UsdFuturesApi.ExchangeData.GetKlinesAsync(
                    symbol,
                    ConvertStringToKlineInterval(interval),
                    startTime,
                    endTime,
                    limit: 1500);

                if (!klines.Success)
                {
                    _logger.LogError($"Failed to get historical data: {klines.Error?.Message}");
                    return new List<Candle>();
                }

                return klines.Data.Select(k => new Candle
                {
                    OpenTime = k.OpenTime,
                    Interval = interval,
                    Open = k.OpenPrice,
                    High = k.HighPrice,
                    Low = k.LowPrice,
                    Close = k.ClosePrice,
                    Volume = k.Volume
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching historical data");
                throw;
            }
        }

        private static KlineInterval ConvertStringToKlineInterval(string interval)
        {
            return interval.ToLower() switch
            {
                "1s" => KlineInterval.OneSecond,
                "1m" => KlineInterval.OneMinute,
                "3m" => KlineInterval.ThreeMinutes,
                "5m" => KlineInterval.FiveMinutes,
                "15m" => KlineInterval.FifteenMinutes,
                "30m" => KlineInterval.ThirtyMinutes,
                "1h" => KlineInterval.OneHour,
                "2h" => KlineInterval.TwoHour,
                "4h" => KlineInterval.FourHour,
                "6h" => KlineInterval.SixHour,
                "8h" => KlineInterval.EightHour,
                "12h" => KlineInterval.TwelveHour,
                "1d" => KlineInterval.OneDay,
                "3d" => KlineInterval.ThreeDay,
                "1w" => KlineInterval.OneWeek,
                "1M" => KlineInterval.OneMonth,
                _ => throw new ArgumentException($"Invalid interval: {interval}")
            };
        }
    }
}
