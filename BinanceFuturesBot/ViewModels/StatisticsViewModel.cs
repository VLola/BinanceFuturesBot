using Binance.Net.Clients;
using BinanceFuturesBot.Command;
using BinanceFuturesBot.Models;
using System.Linq;
using System;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using CryptoExchange.Net.CommonObjects;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Collections.Generic;

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
        private async void ShowStatistics()
        {
            try
            {
                await Task.Run(() =>
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        StatisticsModel.ListStatistics.Clear();
                        StatisticsModel.Statistics.Clear();
                        StatisticsModel.SumTotal = 0m;
                    }));
                    List<Task> tasks = new();
                    DateTime startTime = StatisticsModel.StartTime;
                    DateTime endTime;

                    if (StatisticsModel.IsEndTime)
                    {
                        endTime = StatisticsModel.EndTime;
                    }
                    else
                    {
                        endTime = DateTime.UtcNow;
                    }
                    if (endTime > DateTime.UtcNow)
                    {
                        endTime = DateTime.UtcNow;
                        StatisticsModel.EndTime = endTime;
                    }

                    if(endTime > startTime) {
                        while (true)
                        {
                            if ((endTime - startTime) <= TimeSpan.FromDays(7))
                            {
                                foreach (var symbol in StatisticsModel.Symbols)
                                {
                                    Task task = GetUserTradesAsync(symbol, startTime, endTime);
                                    tasks.Add(task);
                                }
                                break;
                            }

                            foreach (var symbol in StatisticsModel.Symbols)
                            {
                                Task task = GetUserTradesAsync(symbol, startTime, startTime.AddDays(7));
                                tasks.Add(task);
                            }
                            startTime = startTime.AddDays(7);
                        }

                        Task.WaitAll(tasks.ToArray());
                        StatisticsModel.ListStatistics.Sort((x, y) => y.Time.CompareTo(x.Time));
                        foreach (var item in StatisticsModel.ListStatistics)
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                StatisticsModel.Statistics.Add(item);
                                StatisticsModel.SumTotal += item.Total;
                            }));
                        }
                        MessageBox.Show("Ok");
                    }
                    else
                    {
                        MessageBox.Show("Error");
                    }
                });
            }
            catch (Exception ex)
            {
                WriteLog($"ShowStatistics: {ex.Message}");
            }
        }
        private async Task GetUserTradesAsync(string symbol, DateTime startTime, DateTime endTime)
        {
            try
            {
                await Task.Run(async () =>
                {
                    var result = await Client.UsdFuturesApi.Trading.GetUserTradesAsync(symbol: symbol, startTime: startTime, endTime: endTime);
                    if (!result.Success) WriteLog($"Failed GetUserTradesAsync {symbol}: {result.Error?.Message}");
                    else
                    {
                        if(result.Data.ToList().Count > 0)
                        {
                            foreach (var item in result.Data.ToList())
                            {

                                App.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    StatisticsModel.ListStatistics.Add(new BetModel()
                                    {
                                        Time = item.Timestamp,
                                        Symbol = item.Symbol,
                                        Price = item.Price,
                                        Quantity = item.Quantity,
                                        Usdt = item.Quantity * item.Price,
                                        Profit = item.RealizedPnl,
                                        Commission = item.Fee,
                                        Total = (item.RealizedPnl - item.Fee),
                                        OrderSide = item.Side,
                                        Client = Client
                                    });
                                }));
                            }
                        }
                    }

                });
            }
            catch (Exception ex)
            {
                WriteLog($"GetUserTradesAsync {symbol}: {ex.Message}");
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
