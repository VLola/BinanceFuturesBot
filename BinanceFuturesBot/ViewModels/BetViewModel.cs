using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using BinanceFuturesBot.Command;
using BinanceFuturesBot.Models;
using BinanceFuturesBot.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BinanceFuturesBot.ViewModels
{
    public class BetViewModel
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public BetModel BetModel { get; set; } = new();
        public BinanceClient? Client { get; set; }
        public BetViewModel(BinanceFuturesUsdtTrade binanceFuturesUsdtTrade, BinanceClient? client, KlineInterval klineInterval) {
            Client = client;
            Load(binanceFuturesUsdtTrade, klineInterval);
        }
        private void Load(BinanceFuturesUsdtTrade binanceFuturesUsdtTrade, KlineInterval klineInterval)
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
            BetModel.Interval = klineInterval;
        }

        private RelayCommand? _showChartCommand;
        public RelayCommand ShowChartCommand
        {
            get
            {
                return _showChartCommand ?? (_showChartCommand = new RelayCommand(obj => {
                    ShowChart();
                }));
            }
        }
        private void ShowChart()
        {
            DateTime startTime = BetModel.Time.AddHours(-1);
            DateTime endTime = BetModel.Time.AddHours(1);

            List<IBinanceKline> klines = Klines(BetModel.Interval, startTime, endTime);
            List<(double x, double y)> points = GetUserTrades(startTime, endTime);

            int interval = 1;
            if (BetModel.Interval == KlineInterval.ThreeMinutes) interval = 3;
            else if (BetModel.Interval == KlineInterval.FiveMinutes) interval = 5;
            else if (BetModel.Interval == KlineInterval.FifteenMinutes) interval = 15;

            ChartWindow chartWindow = new ChartWindow(klines, points, interval);
            chartWindow.Show();
        }
        public List<IBinanceKline> Klines(KlineInterval interval, DateTime startTime, DateTime endTime)
        {
            try
            {
                var result = Client.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol: BetModel.Symbol, interval: interval, startTime: startTime, endTime: endTime).Result;
                if (!result.Success) WriteLog("Error Klines");
                else
                {
                    return result.Data.ToList();
                }
            }
            catch (Exception eX)
            {
                WriteLog($"Klines {eX.Message}");
            }
            return null;
        }
        private List<(double x, double y)> GetUserTrades(DateTime startTime, DateTime endTime)
        {
            try
            {
                var result = Client.UsdFuturesApi.Trading.GetUserTradesAsync(symbol: BetModel.Symbol, startTime: startTime, endTime: endTime).Result;
                if (!result.Success) WriteLog($"Failed GetUserTrades: {result.Error?.Message}");
                else
                {
                    if (result.Data.ToList().Count > 0)
                    {
                        List<(double x, double y)> list = new();

                        foreach (var item in result.Data.ToList())
                        {
                            list.Add((item.Timestamp.ToOADate(), Decimal.ToDouble(item.Price)));
                        }
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog($"GetUserTrades: {ex.Message}");
            }
            return null;
        }
        private void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(_pathLog + "_BETMODEL", $"{DateTime.Now} {BetModel.Symbol} {text}\n");
            }
            catch { }
        }
    }
}
