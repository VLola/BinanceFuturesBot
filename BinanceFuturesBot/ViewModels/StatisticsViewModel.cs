using Binance.Net.Clients;
using BinanceFuturesBot.Command;
using BinanceFuturesBot.Models;
using System.Linq;
using System;
using System.Windows;
using System.IO;
using Newtonsoft.Json;

namespace BinanceFuturesBot.ViewModels
{
    public class StatisticsViewModel
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        private RelayCommand? _showStatisticsCommand;
        public RelayCommand ShowStatisticsCommand
        {
            get
            {
                return _showStatisticsCommand ?? (_showStatisticsCommand = new RelayCommand(obj => {
                    ShowStatistics();
                }));
            }
        }
        public BinanceClient? Client { get; set; }
        public StatisticsModel StatisticsModel { get; set; } = new();
        private void ShowStatistics()
        {
            try
            {
                var result = Client.UsdFuturesApi.Trading.GetUserTradesAsync("BTCUSDT").Result;
                if (!result.Success) WriteLog($"Failed ShowStatistics {result.Error?.Message}");
                else
                {
                    foreach (var item in result.Data.ToList())
                    {
                        StatisticsModel.Statistics.Add(new BetModel()
                        {
                            Time = item.Timestamp,
                            Symbol = item.Symbol,
                            Price = item.Price,
                            Quantity = item.Quantity,
                            Usdt = item.Quantity * item.Price,
                            Profit = item.RealizedPnl,
                            Commission = item.Fee,
                            Total = item.RealizedPnl - item.Fee,
                            OrderSide = item.Side
                        });
                        StatisticsModel.SumTotal += (item.RealizedPnl - item.Fee);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog($"ShowStatistics: {ex.Message}");
            }
        }
        private void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(_pathLog + "_STATISTICS_LOG", $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
