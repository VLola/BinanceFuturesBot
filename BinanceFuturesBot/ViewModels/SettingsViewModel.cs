using BinanceFuturesBot.Command;
using BinanceFuturesBot.Models;
using ScottPlot.Plottable;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace BinanceFuturesBot.ViewModels
{
    public class SettingsViewModel
    {
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public SettingsModel SettingsModel { get; set; } = new();
        private RelayCommand? _saveMaxLeverageCommand;
        public RelayCommand SaveMaxLeverageCommand
        {
            get
            {
                return _saveMaxLeverageCommand ?? (_saveMaxLeverageCommand = new RelayCommand(obj => {
                    SaveLeverage(null);
                }));
            }
        }
        private RelayCommand? _saveLeverageCommand;
        public RelayCommand SaveLeverageCommand
        {
            get
            {
                return _saveLeverageCommand ?? (_saveLeverageCommand = new RelayCommand(obj => {
                    SaveLeverage(SettingsModel.Leverage);
                }));
            }
        }
        private RelayCommand? _saveSelectedMaxLeverageCommand;
        public RelayCommand SaveSelectedMaxLeverageCommand
        {
            get
            {
                return _saveSelectedMaxLeverageCommand ?? (_saveSelectedMaxLeverageCommand = new RelayCommand(obj => {
                    SaveSelectedLeverage(null);
                }));
            }
        }
        private RelayCommand? _saveSelectedLeverageCommand;
        public RelayCommand SaveSelectedLeverageCommand
        {
            get
            {
                return _saveSelectedLeverageCommand ?? (_saveSelectedLeverageCommand = new RelayCommand(obj => {
                    SaveSelectedLeverage(SettingsModel.Leverage);
                }));
            }
        }
        private async void SaveLeverage(int? leverage)
        {
            await Task.Run(() =>
            {
                try
                {
                    List<Task> tasks = new();
                    foreach (var item in SettingsModel.Settings)
                    {
                        Task task = item.SaveLeverage(leverage);
                        tasks.Add(task);
                    }
                    Task.WaitAll(tasks.ToArray());
                    MessageBox.Show("Leverages saved");
                }
                catch (Exception ex)
                {
                    WriteLog($"Failed SaveLeverage: {ex.Message}");
                }
            });
        }
        private async void SaveSelectedLeverage(int? leverage)
        {
            await Task.Run(async () =>
            {
                try
                {
                    if (SettingsModel.SelectedSymbol != null)
                    {
                        await SettingsModel.SelectedSymbol.SaveLeverage(leverage);
                        MessageBox.Show("Leverage saved");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"Failed SaveSelectedLeverage: {ex.Message}");
                }
            });
        }
        private void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(_pathLog + "_SETTINGS_LOG", $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
