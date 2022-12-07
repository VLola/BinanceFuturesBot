﻿using Binance.Net.Objects.Models.Futures;
using BinanceFuturesBot.Models;
using CryptoExchange.Net.CommonObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceFuturesBot.ViewModels
{
    internal class MainViewModel
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public MainModel MainModel { get; set; } = new();
        public LoginViewModel LoginViewModel { get; set; } = new();
        public SettingsViewModel SettingsViewModel { get; set; } = new();
        public ChartViewModel ChartViewModel { get; set; } = new();
        public MainViewModel()
        {
            if (!Directory.Exists(_pathLog)) Directory.CreateDirectory(_pathLog);
            LoginViewModel.LoginModel.PropertyChanged += LoginModel_PropertyChanged;
            MainModel.PropertyChanged += MainModel_PropertyChanged;
        }

        private void MainModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedSymbol")
            {
                ChartViewModel.Load(MainModel.SelectedSymbol.SymbolModel);
            }
            else if (e.PropertyName == "IsRun")
            {
                RunAllSymbolsAsync();
            }
        }

        private void LoginModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsLoginBinance")
            {
                if(LoginViewModel.LoginModel.IsLoginBinance)
                {
                    CheckOpenOrders();
                    GetSumbolName();
                }
            }
        }
        private async void CheckOpenOrders()
        {
            await Task.Run(async() => {
                while (true)
                {
                    try
                    {
                        var result = await LoginViewModel.Client.UsdFuturesApi.Account.GetPositionInformationAsync();
                        if (!result.Success)
                        {
                            WriteLog($"Failed CheckOpenOrders: {result.Error?.Message}");
                        }
                        else
                        {
                            foreach (var item in result.Data.ToList())
                            {
                                if (item.Quantity != 0m)
                                {
                                    SymbolViewModel? symbol = MainModel.Symbols.FirstOrDefault(it => it.SymbolModel.Name == item.Symbol);
                                    if (symbol != null && symbol.SymbolModel.IsRun)
                                    {
                                        CloseOrder(symbol);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"CheckOpenOrders: {ex.Message}");
                    }
                    
                    await Task.Delay(60000);
                }
            });
        }
        private async void CloseOrder(SymbolViewModel symbolViewModel)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(10000);
                    if (!symbolViewModel.SymbolModel.IsOpenOrder)
                    {
                        await symbolViewModel.CloseBetAsync();
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"Failed CloseOrder: {ex.Message}");
                }
            });
        }
        private async void RunAllSymbolsAsync()
        {
            await Task.Run(() => {
                try
                {
                    bool isRun = MainModel.IsRun;
                    foreach (var item in MainModel.Symbols)
                    {
                        item.SymbolModel.IsRun = isRun;
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"RunAllSymbolsAsync: {ex.Message}");
                }
            });
        }
        private async void GetSumbolName()
        {
            await Task.Run(() => {
                List<string> list = new();
                List<BinanceFuturesUsdtSymbol> symbols = ListSymbols();
                List<BinancePositionDetailsUsdt> details = SymbolsDetail();
                List<BinanceFuturesSymbolBracket> brakets = ListBrackets();
                foreach (var it in symbols)
                {
                    //BTSUSDT CVCUSDT FTTUSDT RAYUSDT SCUSDT TLMUSDT SRMUSDT BTCSTUSDT
                    if (it.Name.EndsWith("USDT") && it.Name != "BTSUSDT" && it.Name != "CVCUSDT" && it.Name != "FTTUSDT" && it.Name != "RAYUSDT" && it.Name != "SCUSDT" && it.Name != "TLMUSDT" && it.Name != "SRMUSDT" && it.Name != "BTCSTUSDT")
                    {
                        list.Add(it.Name);
                    }
                }
                list.Sort();
                foreach (var it in list)
                {
                    BinanceFuturesUsdtSymbol symbol = symbols.FirstOrDefault(x => x.Name == it);
                    BinancePositionDetailsUsdt detail = details.FirstOrDefault(x => x.Symbol == it);
                    int maxLeverage = brakets.FirstOrDefault(x => x.Symbol == it).Brackets.ToList()[0].InitialLeverage;
                    AddSymbol(symbol, detail, maxLeverage);
                }
            });
        }
        private void AddSymbol(BinanceFuturesUsdtSymbol symbol, BinancePositionDetailsUsdt detail, int maxLeverage)
        {
            SymbolDetailViewModel symbolDetailViewModel = new(LoginViewModel.Client, detail, maxLeverage);
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                SettingsViewModel.SettingsModel.Settings.Add(symbolDetailViewModel);
            }));

            SymbolViewModel symbolViewModel = new(LoginViewModel.Client, LoginViewModel.SocketClient, symbol);
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainModel.Symbols.Add(symbolViewModel);
            }));
        }
        private List<BinanceFuturesUsdtSymbol> ListSymbols()
        {
            try
            {
                var result = LoginViewModel.Client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync().Result;
                if (!result.Success) WriteLog($"Failed ListSymbols {result.Error?.Message}");
                return result.Data.Symbols.ToList();
            }
            catch (Exception ex)
            {
                WriteLog($"ListSymbols: {ex.Message}");
            }
            return null;
        }
        private List<BinancePositionDetailsUsdt> SymbolsDetail()
        {
            try
            {
                var result = LoginViewModel.Client.UsdFuturesApi.Account.GetPositionInformationAsync().Result;
                if (!result.Success) WriteLog($"Failed SymbolsDetail: {result.Error?.Message}");
                else return result.Data.ToList();
            }
            catch (Exception ex)
            {
                WriteLog($"Failed SymbolsDetail: {ex.Message}");
            }
            return null;
        }
        private List<BinanceFuturesSymbolBracket> ListBrackets()
        {
            try
            {
                var result = LoginViewModel.Client.UsdFuturesApi.Account.GetBracketsAsync().Result;
                if (!result.Success) WriteLog($"Failed ListBrackets {result.Error?.Message}");
                return result.Data.ToList();
            }
            catch (Exception ex)
            {
                WriteLog($"Failed ListBrackets: {ex.Message}");
            }
            return null;
        }
        private void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(_pathLog + "_MAIN_LOG", $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
