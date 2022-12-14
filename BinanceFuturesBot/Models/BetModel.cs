using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using BinanceFuturesBot.Command;
using BinanceFuturesBot.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceFuturesBot.Models
{
    public class BetModel
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
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
        public BinanceClient? Client { get; set; }

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
            DateTime startTime = Time.AddHours(-1);
            DateTime endTime = Time.AddHours(1);

            List<IBinanceKline> klines = Klines(KlineInterval.FiveMinutes, startTime, endTime);
            List<(double x, double y)> points = GetUserTradesAsync(startTime, endTime);
            ChartWindow chartWindow = new ChartWindow(klines, points);
            chartWindow.Show();
        }
        public List<IBinanceKline> Klines(KlineInterval interval, DateTime startTime, DateTime endTime)
        {
            try
            {
                var result = Client.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol: Symbol, interval: interval, startTime: startTime, endTime: endTime).Result;
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
        private List<(double x, double y)> GetUserTradesAsync(DateTime startTime, DateTime endTime)
        {
            try
            {
                var result = Client.UsdFuturesApi.Trading.GetUserTradesAsync(symbol: Symbol, startTime: startTime, endTime: endTime).Result;
                if (!result.Success) WriteLog($"Failed GetUserTradesAsync {Symbol}: {result.Error?.Message}");
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
                WriteLog($"GetUserTradesAsync {Symbol}: {ex.Message}");
            }
            return null;
        }
        private void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(_pathLog + "_BETMODEL", $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
