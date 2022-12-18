using Binance.Net.Interfaces;
using BinanceFuturesBot.Models;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BinanceFuturesBot.Views
{
    public partial class ChartWindow : Window
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public FinancePlot financePlot { get; set; }
        public ScatterPlot scatterPlot { get; set; }
        public WpfPlot MyPlot { get; set; } = new();
        public ChartWindow(List<IBinanceKline> klines, List<(double x, double y)> points, int interval)
        {
            InitializeComponent();
            DataContext = this;
            MyPlot.Dispatcher.Invoke(new Action(() =>
            {
                MyPlot.Plot.RenderLock();
                MyPlot.Plot.Style(ScottPlot.Style.Gray2);
                //MyPlot.Plot.XAxis.TickLabelFormat("HH:mm:ss", dateTimeFormat: true);
                MyPlot.Plot.XAxis.Hide();
                MyPlot.Plot.XAxis2.Hide();
                MyPlot.Plot.YAxis.Hide();
                MyPlot.Plot.YAxis2.Hide();
                MyPlot.Configuration.LeftClickDragPan = true;
                MyPlot.Configuration.RightClickDragZoom = false;
                MyPlot.Configuration.Pan = false;
                MyPlot.Plot.RenderUnlock();
            }));
            Load(klines, points, interval);
        }
        public async void Load(List<IBinanceKline> klines, List<(double x, double y)> points, int interval)
        {
            await Task.Run(() =>
            {
                try
                {
                    List<OHLC> oHLCs = klines.Select(item => new OHLC(
                        open: Decimal.ToDouble(item.OpenPrice),
                        high: Decimal.ToDouble(item.HighPrice),
                        low: Decimal.ToDouble(item.LowPrice),
                        close: Decimal.ToDouble(item.ClosePrice),
                        timeStart: item.OpenTime,
                        timeSpan: TimeSpan.FromMinutes(interval),
                        volume: Decimal.ToDouble(item.Volume)
                    )).ToList();

                    MyPlot.Dispatcher.Invoke(new Action(() =>
                    {
                        MyPlot.Plot.RenderLock();

                        MyPlot.Plot.Remove(financePlot);
                        MyPlot.Plot.Remove(scatterPlot);
                        financePlot = MyPlot.Plot.AddCandlesticks(oHLCs.ToArray());
                        if(points.Count > 0) scatterPlot = MyPlot.Plot.AddScatterPoints(xs: points.Select(it => it.x).ToArray(), ys: points.Select(it => it.y).ToArray(), color: Color.Orange, markerSize: 7);
                        MyPlot.Plot.RenderUnlock();
                        MyPlot.Refresh();
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
                File.AppendAllText(_pathLog + "_CHART_WINDOW_LOG", $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
