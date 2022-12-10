using Binance.Net.Enums;
using System;

namespace BinanceFuturesBot.Models
{
    public class BetModel
    {
        public DateTime Time { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal Usdt { get; set; }
        public OrderSide OrderSide { get; set; }
        public decimal Profit { get; set; }
        public decimal Commission { get; set; }
        private decimal _total { get; set; }
        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                if (value >= 0) IsPositive = true;
            }
        }
        public bool IsPositive { get; set; }
    }
}
