using AlgoTradeBacktester.Core.Interfaces;
using AlgoTradeBacktester.Core.Models;
using AlgoTradeBacktester.Core.Constants;

namespace AlgoTradeBacktester.Services
{
    public class RiskManager : IRiskManager
    {
        public bool ShouldClosePosition(Position position, Candle currentCandle, out decimal exitPrice)
        {
            exitPrice = position.StopLossPrice;
            if (position == null) return false;

            if (position.IsLong)
            {
                return currentCandle.Low <= position.StopLossPrice;
            }
            else
            {
                return currentCandle.High >= position.StopLossPrice;
            }
        }

        public decimal GetPositionSize(decimal accountBalance)
        {
            decimal riskAmount = accountBalance * TradingConstants.RISK_PERCENTAGE_PER_TRADE;
            return riskAmount / TradingConstants.STOP_LOSS_PERCENTAGE;
        }
    }
}
