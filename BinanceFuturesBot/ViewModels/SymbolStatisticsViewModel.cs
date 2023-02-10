using Binance.Net.Clients;
using BinanceFuturesBot.Command;
using BinanceFuturesBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BinanceFuturesBot.ViewModels
{
    public class SymbolStatisticsViewModel
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
        public SymbolStatisticsModel SymbolStatisticsModel { get; set; } = new();
        private async void ShowStatistics()
        {
            try
            {
                await Task.Run(() =>
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        SymbolStatisticsModel.Statistics.Clear();
                        SymbolStatisticsModel.SumTotal = 0m;
                    }));
                    List<Task> tasks = new();
                    DateTime startTime = SymbolStatisticsModel.StartTime;
                    DateTime endTime;

                    if (SymbolStatisticsModel.IsEndTime)
                    {
                        endTime = SymbolStatisticsModel.EndTime;
                    }
                    else
                    {
                        endTime = DateTime.UtcNow;
                    }
                    if (endTime > DateTime.UtcNow)
                    {
                        endTime = DateTime.UtcNow;
                        SymbolStatisticsModel.EndTime = endTime;
                    }

                    if (endTime > startTime)
                    {
                        while (true)
                        {
                            if ((endTime - startTime) <= TimeSpan.FromDays(7))
                            {
                                foreach (var symbol in SymbolStatisticsModel.Symbols)
                                {
                                    Task task = GetUserTradesAsync(symbol, startTime, endTime);
                                    tasks.Add(task);
                                }
                                break;
                            }

                            foreach (var symbol in SymbolStatisticsModel.Symbols)
                            {
                                Task task = GetUserTradesAsync(symbol, startTime, startTime.AddDays(7));
                                tasks.Add(task);
                            }
                            startTime = startTime.AddDays(7);
                        }

                        Task.WaitAll(tasks.ToArray());

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
                        if (result.Data.ToList().Count > 0)
                        {
                            foreach (var item in result.Data.ToList())
                            {
                                SymbolBetsModel? symbolBetsModel = SymbolStatisticsModel.Statistics.FirstOrDefault(symbol => symbol.Name == item.Symbol);
                                if (symbolBetsModel == null)
                                {
                                    decimal total = item.RealizedPnl - item.Fee;
                                    symbolBetsModel = new SymbolBetsModel();
                                    symbolBetsModel.Name = item.Symbol;
                                    symbolBetsModel.Total = total;
                                    SymbolStatisticsModel.SumTotal += total;
                                    App.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        SymbolStatisticsModel.Statistics.Add(symbolBetsModel);
                                    }));
                                }
                                else
                                {
                                    decimal total = item.RealizedPnl - item.Fee;
                                    symbolBetsModel.Total += total;
                                    SymbolStatisticsModel.SumTotal += total;
                                }
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
                File.AppendAllText(_pathLog + "_SYMBOL_STATISTICS_LOG", $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
