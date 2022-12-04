using Binance.Net.Objects.Models.Spot;
using BinanceFuturesBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                foreach (var it in ListSymbols())
                {
                    list.Add(it.Symbol);
                }
                list.Sort();
                foreach (var it in list)
                {
                    //BTSUSDT CVCUSDT FTTUSDT RAYUSDT SCUSDT TLMUSDT SRMUSDT
                    if (it.EndsWith("USDT") && it != "BTSUSDT" && it != "CVCUSDT" && it != "FTTUSDT" && it != "RAYUSDT" && it != "SCUSDT" && it != "TLMUSDT" && it != "SRMUSDT")
                    {
                        AddSymbol(it);
                    }
                }
            });
        }
        private async void AddSymbol(string name)
        {
            await Task.Run(() =>
            {
                SymbolViewModel symbolViewModel = new(LoginViewModel.Client, LoginViewModel.SocketClient, name);
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MainModel.Symbols.Add(symbolViewModel);
                }));
            });
        }
        private List<BinancePrice> ListSymbols()
        {
            try
            {
                var result = LoginViewModel.Client.UsdFuturesApi.ExchangeData.GetPricesAsync().Result;
                if (!result.Success) WriteLog($"Failed Success ListSymbols: {result.Error?.Message}");
                return result.Data.ToList();
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
