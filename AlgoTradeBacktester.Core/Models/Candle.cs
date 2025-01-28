namespace AlgoTradeBacktester.Core.Models
{
    public class Candle
    {
        public DateTime OpenTime { get; set; }
        public string Interval { get; set; } = string.Empty;
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }
}
