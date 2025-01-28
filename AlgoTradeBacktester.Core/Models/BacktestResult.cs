namespace AlgoTradeBacktester.Core.Models
{
    public class BacktestResult
    {
        public decimal InitialBalance { get; set; }
        public List<Position> Positions { get; set; } = new();
        public decimal TotalPnL { get; set; }
        public int WinningTrades { get; set; }
        public int LosingTrades { get; set; }
        public decimal WinRate => (decimal)WinningTrades / (WinningTrades + LosingTrades);
        public decimal TotalFees { get; set; }
        public decimal FinalBalance => InitialBalance + TotalPnL - TotalFees;
        public decimal TotalProfit => TotalPnL - TotalFees;
    }
}
