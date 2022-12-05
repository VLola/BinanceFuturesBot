using BinanceFuturesBot.Command;
using BinanceFuturesBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace BinanceFuturesBot.ViewModels
{
    public class SettingsViewModel
    {
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
        private async void SaveLeverage(int? leverage)
        {
            await Task.Run(() =>
            {
                List<Task> tasks = new();
                foreach (var item in SettingsModel.Settings)
                {
                    Task task = item.SaveLeverage(leverage);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                MessageBox.Show("Leverages saved");
            });
        }
    }
}
