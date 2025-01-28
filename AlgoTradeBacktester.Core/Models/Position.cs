namespace AlgoTradeBacktester.Core.Models
{
    public class Position
    {
        public DateTime EntryTime { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal Size { get; set; }
        public bool IsLong { get; set; }
        public decimal StopLossPrice { get; set; }
        public DateTime? ExitTime { get; set; }
        public decimal? ExitPrice { get; set; }
        public decimal? PnL { get; set; }
        public decimal? Fee { get; set; }
        public bool IsStopLoss { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal? FinalBalance { get; set; }
    }
}
