using BinanceFuturesBot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BinanceFuturesBot.Models
{
    public class MainModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public ObservableCollection<SymbolViewModel> Symbols { get; set; } = new();
        private SymbolViewModel _selectedSymbol { get; set; }
        public SymbolViewModel SelectedSymbol
        {
            get { return _selectedSymbol; }
            set
            {
                _selectedSymbol = value;
                OnPropertyChanged("SelectedSymbol");
            }
        }
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
