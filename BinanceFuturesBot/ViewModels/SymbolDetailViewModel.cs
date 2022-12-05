using Binance.Net.Clients;
using Binance.Net.Objects.Models.Futures;
using BinanceFuturesBot.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceFuturesBot.ViewModels
{
    public class SymbolDetailViewModel
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public SymbolDetailModel SymbolDetailModel { get; set; } = new();
        public BinanceClient? Client { get; set; }
        public SymbolDetailViewModel(BinanceClient? client, BinancePositionDetailsUsdt detail, int maxLeverage) {
            Client = client;
            SymbolDetailModel.Name = detail.Symbol;
            SymbolDetailModel.MaxLeverage = maxLeverage;
            SymbolDetailModel.Leverage = detail.Leverage;
            SymbolDetailModel.PositionSide = detail.PositionSide;
        }
        private async void LoadLeverage()
        {
            var result = await Client.UsdFuturesApi.Account.GetPositionInformationAsync(symbol: SymbolDetailModel.Name);
            if (!result.Success)
            {
                WriteLog($"Failed LoadLeverage: {result.Error?.Message}");
            }
            else
            {
                SymbolDetailModel.Leverage = result.Data.ToList().FirstOrDefault().Leverage;
            }
        }
        public async Task SaveLeverage(int? lev)
        {
            await Task.Run(async () =>
            {
                int leverage;

                if (lev == null) leverage = SymbolDetailModel.MaxLeverage;
                else leverage = (int)lev;

                if (leverage <= SymbolDetailModel.MaxLeverage && leverage != SymbolDetailModel.Leverage)
                {
                    var result = await Client.UsdFuturesApi.Account.ChangeInitialLeverageAsync(symbol: SymbolDetailModel.Name, leverage: leverage);
                    if (!result.Success)
                    {
                        WriteLog($"Failed LoadLeverage: {result.Error?.Message}");
                    }
                    else
                    {
                        SymbolDetailModel.Leverage = result.Data.Leverage;
                    }
                }
            });
        }
        private void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(_pathLog + SymbolDetailModel.Name, $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
