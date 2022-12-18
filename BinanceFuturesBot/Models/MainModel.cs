using Binance.Net.Enums;
using BinanceFuturesBot.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
        public ObservableCollection<KlineInterval> Intervals { get; set; } = new();
        public MainModel() {
            Intervals.Add(KlineInterval.OneMinute);
            Intervals.Add(KlineInterval.ThreeMinutes);
            Intervals.Add(KlineInterval.FiveMinutes);
            Intervals.Add(KlineInterval.FifteenMinutes);
        }
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
        private bool _isRun { get; set; }
        public bool IsRun
        {
            get { return _isRun; }
            set
            {
                _isRun = value;
                OnPropertyChanged("IsRun");
            }
        }
        private bool _isStart { get; set; }
        public bool IsStart
        {
            get { return _isStart; }
            set
            {
                _isStart = value;
                OnPropertyChanged("IsStart");
            }
        }
        private decimal _usdt { get; set; } = 11m;
        public decimal Usdt
        {
            get { return _usdt; }
            set
            {
                _usdt = value;
                OnPropertyChanged("Usdt");
            }
        }
        private decimal _stopLoss { get; set; } = 1m;
        public decimal StopLoss
        {
            get { return _stopLoss; }
            set
            {
                _stopLoss = value;
                OnPropertyChanged("StopLoss");
            }
        }
        private int _open { get; set; } = 14;
        public int Open
        {
            get { return _open; }
            set
            {
                _open = value;
                OnPropertyChanged("Open");
            }
        }
        private int _close { get; set; } = 3;
        public int Close
        {
            get { return _close; }
            set
            {
                _close = value;
                OnPropertyChanged("Close");
            }
        }
        private KlineInterval _interval { get; set; } = KlineInterval.OneMinute;
        public KlineInterval Interval
        {
            get { return _interval; }
            set
            {
                _interval = value;
                OnPropertyChanged("Interval");
            }
        }
    }
}
