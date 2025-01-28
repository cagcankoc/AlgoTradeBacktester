namespace AlgoTradeBacktester.Core.Constants
{
    public static class TradingConstants
    {
        public const decimal INITIAL_BALANCE = 1000m; // in usd
        public const string BTCUSDT = "BTCUSDT"; // main futures trading pair
        public const decimal TRADING_FEE = 0.0004m; // 0.04% entry and exit fees
        public const decimal STOP_LOSS_PERCENTAGE = 0.02m; // %2 stop loss
        public const decimal RISK_PERCENTAGE_PER_TRADE = 0.05m; // %5 risk per trade
    }
}
