using AlgoTradeBacktester.Core.Models;

namespace AlgoTradeBacktester.Core.Interfaces
{
    public interface IStrategyService
    {
        BacktestResult Backtest(List<Candle> candles);
    }
}
