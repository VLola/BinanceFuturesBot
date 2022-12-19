using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using BinanceFuturesBot.Command;
using BinanceFuturesBot.Models;
using CryptoExchange.Net.CommonObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceFuturesBot.ViewModels
{
    public class SymbolViewModel
    {
        private RelayCommand? _openOrderCommand;
        public RelayCommand OpenOrderCommand
        {
            get
            {
                return _openOrderCommand ?? (_openOrderCommand = new RelayCommand(obj => {
                    int i = SymbolModel.Klines.Count - 1;
                    SymbolModel.PriceStopLoss = RoundPrice(SymbolModel.Klines[i].ClosePrice + (SymbolModel.Klines[i].ClosePrice * (SymbolModel.StopLoss / 100)));
                    OpenBet();
                }));
            }
        }
        private RelayCommand? _closeOrderCommand;
        public RelayCommand CloseOrderCommand
        {
            get
            {
                return _closeOrderCommand ?? (_closeOrderCommand = new RelayCommand(obj => {
                    CloseBetAsync();
                }));
            }
        }
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
            SymbolModel.PropertyChanged += SymbolModel_PropertyChanged;
        }

        private void SymbolModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Price")
            {
                if (SymbolModel.IsOpenOrder && SymbolModel.IsRun)
                {
                    if(SymbolModel.Price > SymbolModel.PriceStopLoss)
                    {
                        WriteLog("CloseBetAsync SymbolModel_PropertyChanged:");
                        CloseBetAsync();
                    }
                }
            }
        }

        public async void StartAsync(KlineInterval klineInterval)
        {
            await Task.Run(() =>
            {
                SymbolModel.KlineInterval = klineInterval;
                SymbolModel.Klines = Klines(SymbolModel.KlineInterval, 50);

                SymbolModel.Price = SymbolModel.Klines[SymbolModel.Klines.Count - 1].ClosePrice;
                StartKlineAsync();
            });
        }
        public async void StartKlineAsync()
        {
            try
            {
                var result = await SocketClient.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(SymbolModel.Name, SymbolModel.KlineInterval, Message =>
                {
                    SymbolModel.Price = Message.Data.Data.ClosePrice;
                    if (Message.Data.Data.OpenTime == SymbolModel.Klines[SymbolModel.Klines.Count - 1].OpenTime) UpdateKline(Message.Data.Data);
                    else AddKline(Message.Data.Data);
                });
                if (!result.Success)
                {
                    WriteLog($"Failed StartKlineAsync: {result.Error?.Message}");
                }
            }
            catch (Exception eX)
            {
                WriteLog($"StartKlineAsync {eX.Message}");
            }
        }
        private void UpdateKline(IBinanceKline binanceKline)
        {
            SymbolModel.Klines[SymbolModel.Klines.Count - 1] = binanceKline;
        }
        private void AddKline(IBinanceKline binanceKline)
        {
            SymbolModel.Klines.Add(binanceKline);
            if (!SymbolModel.IsWait && SymbolModel.IsRun) {
                CheckStrategyAsync();
            }
        }
        private async void CheckStrategyAsync()
        {
            await Task.Run(() =>
            {
                try
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
                            SymbolModel.PriceStopLoss = RoundPrice(SymbolModel.Klines[i + 1].ClosePrice + (SymbolModel.Klines[i + 1].ClosePrice * (SymbolModel.StopLoss / 100)));
                            SymbolModel.IsWait = true;
                            OpenBetAsync();
                        }
                    }
                }
                catch (Exception eX)
                {
                    WriteLog($"CheckStrategyAsync {eX.Message}");
                }
            });
        }
        private async void OpenBetAsync()
        {
            await Task.Run(async () =>
            {
                OpenBet();
                await Task.Delay((60000 * SymbolModel.Interval * (SymbolModel.Close + 1)));
                WriteLog("CloseBetAsync strategy:");
                await CloseBetAsync();
                SymbolModel.IsOpenOrder = false;
                SymbolModel.IsWait = false;
            });
        }

        public void OpenBet()
        {
            WriteLog("OpenBet:");
            decimal quantity = RoundQuantity(SymbolModel.Usdt / SymbolModel.Price);
            OpenOrder(OrderSide.Sell, quantity);
        }
        private async Task CloseBetAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    var result = await Client.UsdFuturesApi.Account.GetPositionInformationAsync(symbol: SymbolModel.Name);
                    if (!result.Success)
                    {
                        WriteLog($"Failed CloseBetAsync: {result.Error?.Message}");
                    }
                    else
                    {
                        decimal quantity = result.Data.ToList()[0].Quantity;
                        if (quantity != 0m)
                        {
                            if (quantity > 0m)
                            {
                                CloseOrder(OrderSide.Sell, quantity);
                            }
                            else
                            {
                                CloseOrder(OrderSide.Buy, -quantity);
                            }
                        }
                    }
                }
                catch (Exception eX)
                {
                    WriteLog($"CloseBetAsync {eX.Message}");
                }
            });
        }

        public void OpenOrder(OrderSide side, decimal quantity)
        {
            try
            {
                var result = Client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: SymbolModel.Name, side: side, type: FuturesOrderType.Market, quantity: quantity, positionSide: PositionSide.Both).Result;
                if (!result.Success)
                {
                    WriteLog($"Failed OpenOrder: {result.Error.Message}");
                }
                else
                {
                    SymbolModel.IsOpenOrder = true;
                    WriteLog($"OpenOrder: {JsonConvert.SerializeObject(result.Data)}");
                    OpenStopLoss(side, quantity);
                    SymbolModel.Points.Add((DateTime.UtcNow.ToOADate(), Decimal.ToDouble(SymbolModel.Price)));
                }
            }
            catch (Exception eX)
            {
                WriteLog($"OpenOrder {eX.Message}");
            }
        }
        public void CloseOrder(OrderSide side, decimal quantity)
        {
            try
            {
                var result = Client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: SymbolModel.Name, side: side, type: FuturesOrderType.Market, quantity: quantity, positionSide: PositionSide.Both).Result;
                if (!result.Success)
                {
                    WriteLog($"Failed CloseOrder: {result.Error.Message}");
                }
                else
                {
                    SymbolModel.IsOpenOrder = false;
                    WriteLog($"CloseOrder: {JsonConvert.SerializeObject(result.Data)}");
                    SymbolModel.Points.Add((DateTime.UtcNow.ToOADate(), Decimal.ToDouble(SymbolModel.Price)));
                }
                CancelAllOrdersAsync();
            }
            catch (Exception eX)
            {
                WriteLog($"CloseOrder {eX.Message}");
            }
        }
        private async void CancelAllOrdersAsync()
        {
            var result = await Client.UsdFuturesApi.Trading.CancelAllOrdersAsync(symbol: SymbolModel.Name);
            if (!result.Success)
            {
                WriteLog($"Failed CancelAllOrdersAsync: {result.Error?.Message}");
            }
            else
            {
                WriteLog("CancelAllOrdersAsync");
            }
        }
        public void OpenStopLoss(OrderSide side, decimal quantity)
        {
            try
            {
                if (side == OrderSide.Buy)
                {
                    side = OrderSide.Sell;
                }
                else {
                    side = OrderSide.Buy;
                }
                var result = Client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: SymbolModel.Name, side: side, type: FuturesOrderType.StopMarket, quantity: quantity, positionSide: PositionSide.Both, stopPrice: SymbolModel.PriceStopLoss, closePosition: true).Result;
                if (!result.Success)
                {
                    WriteLog($"Failed OpenStopLoss: {result.Error.Message}");
                }
                else
                {
                    WriteLog($"OpenStopLoss: {JsonConvert.SerializeObject(result.Data)}");
                }
            }
            catch (Exception eX)
            {
                WriteLog($"OpenStopLoss {eX.Message}");
            }
        }
        private decimal RoundQuantity(decimal quantity)
        {
            decimal quantity_final = 0m;
            if (SymbolModel.StepSize == 0.001m) quantity_final = Math.Round(quantity, 3);
            else if (SymbolModel.StepSize == 0.01m) quantity_final = Math.Round(quantity, 2);
            else if (SymbolModel.StepSize == 0.1m) quantity_final = Math.Round(quantity, 1);
            else if (SymbolModel.StepSize == 1m) quantity_final = Math.Round(quantity, 0);
            if (quantity_final < SymbolModel.MinQuantity) return SymbolModel.MinQuantity;
            return quantity_final;
        }

        private decimal RoundPrice(decimal price)
        {
            return Math.Round(price, SymbolModel.RoundPrice);
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
            catch (Exception eX)
            {
                WriteLog($"Klines {eX.Message}");
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
