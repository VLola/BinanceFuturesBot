using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using BinanceFuturesBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceFuturesBot.ViewModels
{
    public class SymbolViewModel
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public BinanceClient? Client { get; set; }
        public BinanceSocketClient? SocketClient { get; set; }
        public SymbolModel SymbolModel { get; set; } = new();
        public SymbolViewModel(BinanceClient? client, BinanceSocketClient? socketClient, BinanceFuturesUsdtSymbol symbol) {
            SymbolModel.Name = symbol.Name;
            SymbolModel.MinQuantity = symbol.LotSizeFilter.MinQuantity;
            SymbolModel.StepSize = symbol.LotSizeFilter.StepSize;
            SymbolModel.TickSize = symbol.PriceFilter.TickSize;
            Client = client;
            SocketClient = socketClient;
            Load();
            SymbolModel.PropertyChanged += SymbolModel_PropertyChanged;
        }

        private void SymbolModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Price")
            {
                if (SymbolModel.IsOpenOrder)
                {
                    if(SymbolModel.Price > SymbolModel.PriceStopLoss)
                    {

                    }
                }
            }
        }

        private async void Load()
        {
            await Task.Run(() =>
            {
                SymbolModel.Klines = Klines(KlineInterval.FiveMinutes, 50);
                SymbolModel.Price = SymbolModel.Klines[SymbolModel.Klines.Count - 1].ClosePrice;
                StartKlineAsync();
            });
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
            if (!SymbolModel.IsOpenOrder) {
                CheckStrategy();
            }
        }
        private async void CheckStrategy()
        {
            await Task.Run(() =>
            {
                int i = SymbolModel.Klines.Count - 3;
                decimal sum = 0m;
                for (int j = i; j > (i - 30); j--)
                {
                    sum += (SymbolModel.Klines[j].HighPrice - SymbolModel.Klines[j].LowPrice);
                }
                decimal average = (sum / 30);
                if ((SymbolModel.Klines[i + 1].HighPrice - SymbolModel.Klines[i + 1].LowPrice) > (average * SymbolModel.Open))
                {
                    if (SymbolModel.Klines[i + 1].ClosePrice > SymbolModel.Klines[i + 1].OpenPrice)
                    {
                        SymbolModel.PriceStopLoss = SymbolModel.Klines[i + 1].ClosePrice + (SymbolModel.Klines[i + 1].ClosePrice * (SymbolModel.StopLoss / 100));
                        SymbolModel.IsOpenOrder = true;
                        StartStrategy();
                    }
                }
            });
        }
        private async void StartStrategy()
        {
            await Task.Run(async () =>
            {
                await Task.Delay(300000 * SymbolModel.Close);
                SymbolModel.IsOpenOrder = false;
            });
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
