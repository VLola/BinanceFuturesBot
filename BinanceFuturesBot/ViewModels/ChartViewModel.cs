using BinanceFuturesBot.Models;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceFuturesBot.ViewModels
{
    public class ChartViewModel
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public FinancePlot financePlot { get; set; }
        public ScatterPlot scatterPlot { get; set; }
        public ChartModel ChartModel { get; set; } = new();
        public ChartViewModel() {
            ChartModel.MyPlot.Dispatcher.Invoke(new Action(() =>
            {
                ChartModel.MyPlot.Plot.RenderLock();
                ChartModel.MyPlot.Plot.Style(ScottPlot.Style.Gray2);
                //ChartModel.MyPlot.Plot.XAxis.TickLabelFormat("HH:mm:ss", dateTimeFormat: true);
                ChartModel.MyPlot.Plot.XAxis.Hide();
                ChartModel.MyPlot.Plot.XAxis2.Hide();
                ChartModel.MyPlot.Plot.YAxis.Hide();
                ChartModel.MyPlot.Plot.YAxis2.Hide();
                ChartModel.MyPlot.Configuration.LeftClickDragPan = true;
                ChartModel.MyPlot.Configuration.RightClickDragZoom = false;
                ChartModel.MyPlot.Configuration.Pan = false;
                ChartModel.MyPlot.Plot.RenderUnlock();
            }));
        }
        public async void Load(SymbolModel symbolModel)
        {
            await Task.Run(() =>
            {
                try
                {
                    List<OHLC> oHLCs = symbolModel.Klines.Select(item => new OHLC(
                        open: Decimal.ToDouble(item.OpenPrice),
                        high: Decimal.ToDouble(item.HighPrice),
                        low: Decimal.ToDouble(item.LowPrice),
                        close: Decimal.ToDouble(item.ClosePrice),
                        timeStart: item.OpenTime,
                        timeSpan: TimeSpan.FromMinutes(5),
                        volume: Decimal.ToDouble(item.Volume)
                    )).ToList();

                    ChartModel.MyPlot.Dispatcher.Invoke(new Action(() =>
                    {
                        ChartModel.MyPlot.Plot.RenderLock();

                        ChartModel.MyPlot.Plot.Remove(financePlot);
                        ChartModel.MyPlot.Plot.Remove(scatterPlot);
                        financePlot = ChartModel.MyPlot.Plot.AddCandlesticks(oHLCs.ToArray());
                        if(symbolModel.Points.Count > 0) scatterPlot = ChartModel.MyPlot.Plot.AddScatterPoints(xs: symbolModel.Points.Select(it => it.x).ToArray(), ys: symbolModel.Points.Select(it => it.y).ToArray(), color: Color.Orange, markerSize: 7);
                        ChartModel.MyPlot.Plot.RenderUnlock();
                        ChartModel.MyPlot.Refresh();
                    }));
                }
                catch (Exception ex)
                {
                    WriteLog($"Failed Load: {ex.Message}");
                }
            });
        }
        private void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(_pathLog + "_CHART_LOG", $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
