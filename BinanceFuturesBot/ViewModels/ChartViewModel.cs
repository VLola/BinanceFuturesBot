﻿using Binance.Net.Interfaces;
using BinanceFuturesBot.Models;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceFuturesBot.ViewModels
{
    public class ChartViewModel
    {
        public FinancePlot financePlot { get; set; }
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
                    financePlot = ChartModel.MyPlot.Plot.AddCandlesticks(oHLCs.ToArray());
                    ChartModel.MyPlot.Plot.RenderUnlock();
                    ChartModel.MyPlot.Refresh();
                }));
            });
        }
    }
}