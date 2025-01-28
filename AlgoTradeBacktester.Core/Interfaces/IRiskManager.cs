using AlgoTradeBacktester.Core.Models;

namespace AlgoTradeBacktester.Core.Interfaces
{
    public interface IRiskManager
    {
        bool ShouldClosePosition(Position position, Candle currentCandle, out decimal exitPrice);
        decimal GetPositionSize(decimal accountBalance);
    }
}
