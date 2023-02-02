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
        private bool _isLoad { get; set; }
        public bool IsLoad
        {
            get { return _isLoad; }
            set
            {
                _isLoad = value;
                OnPropertyChanged("IsLoad");
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
    }
}
