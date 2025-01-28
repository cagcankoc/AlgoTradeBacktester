using AlgoTradeBacktester.Core.Models;

namespace AlgoTradeBacktester.Core.Interfaces
{
    public interface IExchangeService
    {
        Task<List<Candle>> GetHistoricalDataAsync(string symbol, string interval, DateTime startTime, DateTime endTime);
    }
}
