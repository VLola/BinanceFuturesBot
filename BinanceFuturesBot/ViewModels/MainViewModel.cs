using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Spot;
using BinanceFuturesBot.Models;
using CryptoExchange.Net.CommonObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace BinanceFuturesBot.ViewModels
{
    internal class MainViewModel
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public MainModel MainModel { get; set; } = new();
        public LoginViewModel LoginViewModel { get; set; } = new();
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
        }

        private void LoginModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsLoginBinance")
            {
                if(LoginViewModel.LoginModel.IsLoginBinance)
                {
                    GetSumbolName();
                }
            }
        }

        private async void GetSumbolName()
        {
            await Task.Run(() => {
                List<string> list = new();
                List<BinanceFuturesUsdtSymbol> symbols = ListSymbols();
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
                    foreach (var symbol in symbols)
                    {
                        if(symbol.Name == it)
                        {
                            AddSymbol(symbol);
                        }
                    }
                }
            });
        }
        private void AddSymbol(BinanceFuturesUsdtSymbol symbol)
        {
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
                if (!result.Success) WriteLog($"Failed GetSumbolName {result.Error?.Message}");
                return result.Data.Symbols.ToList();
            }
            catch (Exception ex)
            {
                WriteLog($"Failed ListSymbols: {ex.Message}");
                return null;
            }
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
