using System;

namespace BinanceFuturesBot.Models
{
    public class BetModel
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal Quantity { get; set; }
        public decimal Usdt { get; set; }
        public decimal Profit { get; set; }
        public decimal Commission { get; set; }
        public decimal Total { get; set; }
    }
}
