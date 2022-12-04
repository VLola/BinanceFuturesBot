using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using BinanceFuturesBot.Models;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace BinanceFuturesBot.ViewModels
{
    public class SymbolViewModel
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public BinanceClient? Client { get; set; }
        public BinanceSocketClient? SocketClient { get; set; }
        public SymbolModel SymbolModel { get; set; } = new();
        public SymbolViewModel(BinanceClient? client, BinanceSocketClient? socketClient, string name) {
            SymbolModel.Name = name;
            Client = client;
            SocketClient = socketClient;
            Load();
        }
        private void Load()
        {
            SymbolModel.Klines = Klines(KlineInterval.FiveMinutes, 50);
            SymbolModel.Price = SymbolModel.Klines[SymbolModel.Klines.Count - 1].ClosePrice;
            StartKlineAsync();
        }
        public void StartKlineAsync()
        {
            SocketClient.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(SymbolModel.Name, KlineInterval.FiveMinutes, Message =>
            {
                SymbolModel.Price = Message.Data.Data.ClosePrice;
                if (Message.Data.Data.OpenTime == SymbolModel.Klines[SymbolModel.Klines.Count - 1].OpenTime) UpdateKline(Message.Data.Data);
                else AddKline(Message.Data.Data);
            });
        }
        private void UpdateKline(IBinanceKline binanceKline)
        {
            SymbolModel.Klines[SymbolModel.Klines.Count - 1] = binanceKline;
        }
        private void AddKline(IBinanceKline binanceKline)
        {
            SymbolModel.Klines.Add(binanceKline);
        }
        public List<IBinanceKline> Klines(KlineInterval interval, int limit)
        {
            try
            {
                var result = Client.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol: SymbolModel.Name, interval: interval, limit: limit).Result;
                if (!result.Success) WriteLog("Error Klines");
                else
                {
                    return result.Data.ToList();
                }
            }
            catch (Exception e)
            {
                WriteLog($"Klines {e.Message}");
            }
            return null;
        }
        private void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(_pathLog + SymbolModel.Name, $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
