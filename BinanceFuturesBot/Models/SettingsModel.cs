using Binance.Net.Enums;
using BinanceFuturesBot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BinanceFuturesBot.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public ObservableCollection<SymbolDetailViewModel> Settings { get; set; } = new();

        private int _leverage { get; set; } = 20;
        public int Leverage
        {
            get { return _leverage; }
            set
            {
                _leverage = value;
                OnPropertyChanged("Leverage");
            }
        }
    }
}
