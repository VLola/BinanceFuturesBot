using System;

namespace BinanceFuturesBot.Models
{
    public class BetModel
    {
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal Usdt { get; set; }
        public decimal Profit { get; set; }
        public decimal Commission { get; set; }
        public decimal Total { get; set; }
    }
}
