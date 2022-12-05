using Binance.Net.Objects.Models.Futures;
using BinanceFuturesBot.Models;
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
