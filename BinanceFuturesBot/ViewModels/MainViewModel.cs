﻿using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Spot;
using BinanceFuturesBot.Command;
using BinanceFuturesBot.Models;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BinanceFuturesBot.ViewModels
{
    internal class MainViewModel
    {
        List<(int number, int open, int close, int interval, decimal sl, int algorithm)> Strategies = new();
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        private string _pathStrategies = $"{Directory.GetCurrentDirectory()}/strategies/";
        public MainModel MainModel { get; set; } = new();
        public LoginViewModel LoginViewModel { get; set; } = new();
        public SettingsViewModel SettingsViewModel { get; set; } = new();
        public StatisticsViewModel StatisticsViewModel { get; set; } = new();
        public SymbolStatisticsViewModel SymbolStatisticsViewModel { get; set; } = new();
        private RelayCommand? _startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new RelayCommand(obj => {
                    StartAsync();
                }));
            }
        }
        private RelayCommand? _closeBetsCommand;
        public RelayCommand CloseBetsCommand
        {
            get
            {
                return _closeBetsCommand ?? (_closeBetsCommand = new RelayCommand(obj => {
                    CloseBetsAsync();
                }));
            }
        }
        public MainViewModel()
        {
            if (!Directory.Exists(_pathLog)) Directory.CreateDirectory(_pathLog);
            if (!Directory.Exists(_pathStrategies)) Directory.CreateDirectory(_pathStrategies);
            LoginViewModel.LoginModel.PropertyChanged += LoginModel_PropertyChanged;
            MainModel.PropertyChanged += MainModel_PropertyChanged;
            LoadStrategies();
        }
        private void LoadStrategies()
        {
            int[] intervals = { 5, 15 };
            int[] algorithms = { 1, 2, 3, 4 };
            decimal[] stopLosses = { 0.5m, 1m, 1.5m, 2m };
            int number = 0;
            foreach (var interval in intervals)
            {
                foreach (var algorithm in algorithms)
                {
                    foreach (var stopLoss in stopLosses)
                    {
                        for (int i = 2; i < 20; i++)
                        {
                            for (int j = 0; j < 30; j++)
                            {
                                Strategies.Add((number, i, j, interval, stopLoss, algorithm));
                                number++;
                            }
                        }
                    }
                }
            }
        }
        private async void StartAsync()
        {
            await Task.Run(async () => {
                try
                {
                    MainModel.IsStart = true;
                    foreach (var item in MainModel.Symbols)
                    {
                        item.StartAsync();
                        await Task.Delay(100);
                    }
                    MessageBox.Show("Start ok");
                }
                catch (Exception ex)
                {
                    WriteLog($"StartAsync: {ex.Message}");
                }
            });
        }
        private void MainModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRun")
            {
                Task.Run(() => {
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
                        WriteLog($"MainModel_PropertyChanged IsRun: {ex.Message}");
                    }
                });
            }
            else if (e.PropertyName == "Usdt")
            {
                Task.Run(() => {
                    try
                    {
                        decimal usdt = MainModel.Usdt;
                        foreach (var item in MainModel.Symbols)
                        {
                            item.SymbolModel.Usdt = usdt;
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"MainModel_PropertyChanged Usdt: {ex.Message}");
                    }
                });
            }
        }

        private void LoginModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsLoginBinance")
            {
                if(LoginViewModel.LoginModel.IsLoginBinance)
                {
                    StatisticsViewModel.Client = LoginViewModel.Client;
                    SymbolStatisticsViewModel.Client = LoginViewModel.Client;
                    CheckOpenOrders();
                    GetSumbolName();
                    BalanceFutureAsync();
                    SubscribeToAccountAsync();
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
                            int bets = 0;
                            foreach (var item in result.Data.ToList())
                            {
                                if (item.Quantity != 0m)
                                {
                                    bets++;
                                    SymbolViewModel? symbol = MainModel.Symbols.FirstOrDefault(it => it.SymbolModel.Name == item.Symbol);
                                    if (symbol != null && symbol.SymbolModel.IsRun)
                                    {
                                        CloseOrder(symbol);
                                    }
                                }
                            }
                            MainModel.Bets = bets;
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
        private async void CloseBetsAsync()
        {
            await Task.Run(async () => {
                try
                {
                    var result = await LoginViewModel.Client.UsdFuturesApi.Account.GetPositionInformationAsync();
                    if (!result.Success)
                    {
                        WriteLog($"Failed CloseOpenOrdersAsync: {result.Error?.Message}");
                    }
                    else
                    {
                        foreach (var item in result.Data.ToList())
                        {
                            if (item.Quantity != 0m)
                            {
                                await CloseBetAsync(item.Symbol);
                            }
                        }
                        CheckOpenOrders();
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"CloseOpenOrdersAsync: {ex.Message}");
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
                        await CloseBetAsync(symbolViewModel.SymbolModel.Name);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"Failed CloseOrder: {ex.Message}");
                }
            });
        }
        private async Task CloseBetAsync(string name)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var result = await LoginViewModel.Client.UsdFuturesApi.Account.GetPositionInformationAsync(symbol: name);
                    if (!result.Success)
                    {
                        WriteLog($"Failed CloseBet: {result.Error?.Message}");
                    }
                    else
                    {
                        WriteLog("CloseBet:");
                        decimal quantity = result.Data.ToList()[0].Quantity;
                        if (quantity != 0m)
                        {
                            if (quantity > 0m)
                            {
                                OpenOrder(name, OrderSide.Sell, quantity);
                            }
                            else
                            {
                                OpenOrder(name, OrderSide.Buy, -quantity);
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
        public void OpenOrder(string name, OrderSide side, decimal quantity)
        {
            try
            {
                var result = LoginViewModel.Client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: name, side: side, type: FuturesOrderType.Market, quantity: quantity, positionSide: PositionSide.Both).Result;
                if (!result.Success)
                {
                    WriteLog($"Failed OpenOrder: {result.Error.Message}");
                }
                else
                {
                    WriteLog($"OpenOrder: {JsonConvert.SerializeObject(result.Data)}");
                }
            }
            catch (Exception eX)
            {
                WriteLog($"OpenOrder {eX.Message}");
            }
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
                    if (it.Name.EndsWith("USDT"))
                    {
                        list.Add(it.Name);
                    }
                }
                list.Sort();

                if (!File.Exists(_pathStrategies + "config")) File.WriteAllText(_pathStrategies + "config", "[]");

                string json = File.ReadAllText(_pathStrategies + "config");
                List<StrategyModel>? strategies = JsonConvert.DeserializeObject<List<StrategyModel>>(json);

                foreach (var it in list)
                {
                    int number = 0;
                    bool isRun = false;
                    if(strategies != null)
                    {
                        StrategyModel? strategyModel = strategies.FirstOrDefault(strategy => strategy.Name == it);
                        if(strategyModel != null)
                        {
                            number = strategyModel.Number;
                            isRun = strategyModel.IsRun;
                        }
                    }
                    BinanceFuturesUsdtSymbol symbol = symbols.FirstOrDefault(x => x.Name == it);
                    BinancePositionDetailsUsdt detail = details.FirstOrDefault(x => x.Symbol == it);
                    int maxLeverage = brakets.FirstOrDefault(x => x.Symbol == it).Brackets.ToList()[0].InitialLeverage;
                    AddSymbol(symbol, detail, maxLeverage, number, isRun);
                }
                MainModel.IsLoad = true;
            });
        }
        private void AddSymbol(BinanceFuturesUsdtSymbol symbol, BinancePositionDetailsUsdt detail, int maxLeverage, int number, bool isRun)
        {
            SymbolDetailViewModel symbolDetailViewModel = new(LoginViewModel.Client, detail, maxLeverage);
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                SettingsViewModel.SettingsModel.Settings.Add(symbolDetailViewModel);
            }));

            SymbolViewModel symbolViewModel = new(LoginViewModel.Client, LoginViewModel.SocketClient, symbol, number, isRun, Strategies);
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainModel.Symbols.Add(symbolViewModel);
            }));
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                StatisticsViewModel.StatisticsModel.Symbols.Add(symbol.Name);
            }));
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                SymbolStatisticsViewModel.SymbolStatisticsModel.Symbols.Add(symbol.Name);
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

        #region - Balance (Async) -
        private async void BalanceFutureAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var result = LoginViewModel.Client.UsdFuturesApi.Account.GetAccountInfoAsync().Result;
                    if (!result.Success)
                    {
                        WriteLog($"Failed Success BalanceFutureAsync: {result.Error?.Message}");
                    }
                    else
                    {
                        MainModel.Balance = result.Data.TotalMarginBalance;
                    }
                });
            }
            catch (Exception ex)
            {
                WriteLog($"Failed BalanceFutureAsync: {ex.Message}");
            }
        }
        #endregion

        private async void SubscribeToAccountAsync()
        {
            var listenKey = await LoginViewModel.Client.UsdFuturesApi.Account.StartUserStreamAsync();
            if (!listenKey.Success)
            {
                WriteLog($"Failed to start user stream: listenKey");
            }
            else
            {
                KeepAliveUserStreamAsync(listenKey.Data);
                WriteLog($"Listen Key Created");
                var result = await LoginViewModel.SocketClient.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync(listenKey: listenKey.Data,
                    onLeverageUpdate => { },
                    onMarginUpdate => { },
                    onAccountUpdate =>
                    {
                        MainModel.Balance = onAccountUpdate.Data.UpdateData.Balances.ToList()[0].CrossWalletBalance;
                    },
                    onOrderUpdate => { },
                    onListenKeyExpired => { });
                if (!result.Success)
                {
                    WriteLog($"Failed UserDataUpdates: {result.Error?.Message}");
                }
            }
        }
        private async void KeepAliveUserStreamAsync(string listenKey)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    var result = await LoginViewModel.Client.UsdFuturesApi.Account.KeepAliveUserStreamAsync(listenKey);
                    if (!result.Success) WriteLog($"Failed KeepAliveUserStreamAsync: {result.Error?.Message}");
                    else
                    {
                        WriteLog("Success KeepAliveUserStreamAsync");
                    }
                    await Task.Delay(900000);
                }
            });
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
