using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using BinanceFuturesBot.Models;
using CryptoExchange.Net.CommonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceFuturesBot.ViewModels
{
    public class BetViewModel
    {
        public BetModel BetModel { get; set; } = new();
        public BinanceClient? Client { get; set; }
        public BetViewModel(BinanceFuturesUsdtTrade binanceFuturesUsdtTrade, BinanceClient? client) {
            Client = client;
            Load(binanceFuturesUsdtTrade);
        }
        private void Load(BinanceFuturesUsdtTrade binanceFuturesUsdtTrade)
        {
            BetModel.Time = binanceFuturesUsdtTrade.Timestamp;
            BetModel.Symbol = binanceFuturesUsdtTrade.Symbol;
            BetModel.Price = binanceFuturesUsdtTrade.Price;
            BetModel.Quantity = binanceFuturesUsdtTrade.Quantity;
            BetModel.Usdt = binanceFuturesUsdtTrade.Quantity * binanceFuturesUsdtTrade.Price;
            BetModel.Profit = binanceFuturesUsdtTrade.RealizedPnl;
            BetModel.Commission = binanceFuturesUsdtTrade.Fee;
            BetModel.Total = (binanceFuturesUsdtTrade.RealizedPnl - binanceFuturesUsdtTrade.Fee);
            BetModel.OrderSide = binanceFuturesUsdtTrade.Side;
        }
    }
}
