using Binance.Net.Enums;
using Binance.Net.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BinanceFuturesBot.Models
{
    public class SymbolModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public List<IBinanceKline> Klines { get; set; } = new();
        public List<(double x, double y)> Points { get; set; } = new();
        private StrategyModel _strategyModel { get; set; } = new();

        public void SaveStrategy(string name,int number, bool isRun)
        {
            _strategyModel.Name = name;
            _strategyModel.Number = number;
            _strategyModel.IsRun = isRun;
        }
        private void CheckChanges()
        {
            if (_isRun == _strategyModel.IsRun && _number == _strategyModel.Number) IsNotSaved = false;
            else IsNotSaved = true;
        }
        private bool _isRun { get; set; }
        public bool IsRun
        {
            get { return _isRun; }
            set
            {
                _isRun = value;
                OnPropertyChanged("IsRun"); 
                CheckChanges();
            }
        }
        private int _number { get; set; }
        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                OnPropertyChanged("Number");
                CheckChanges();
            }
        }
        private int _numberAlgorithm { get; set; }
        public int NumberAlgorithm
        {
            get { return _numberAlgorithm; }
            set
            {
                _numberAlgorithm = value;
                OnPropertyChanged("NumberAlgorithm");
            }
        }
        private bool _isNotSaved { get; set; }
        public bool IsNotSaved
        {
            get { return _isNotSaved; }
            set
            {
                _isNotSaved = value;
                OnPropertyChanged("IsNotSaved");
            }
        }
        private int _open { get; set; } = 3;
        public int Open
        {
            get { return _open; }
            set
            {
                _open = value;
                OnPropertyChanged("Open");
            }
        }
        private int _close { get; set; } = 1;
        public int Close
        {
            get { return _close; }
            set
            {
                _close = value;
                OnPropertyChanged("Close");
            }
        }
        private decimal _stopLoss { get; set; } = 0.05m;
        public decimal StopLoss
        {
            get { return _stopLoss; }
            set
            {
                _stopLoss = value;
                OnPropertyChanged("StopLoss");
            }
        }
        private string _name { get; set; }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
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
        private decimal _minQuantity { get; set; }
        public decimal MinQuantity
        {
            get { return _minQuantity; }
            set
            {
                _minQuantity = value;
                OnPropertyChanged("MinQuantity");
            }
        }
        private decimal _stepSize { get; set; }
        public decimal StepSize
        {
            get { return _stepSize; }
            set
            {
                _stepSize = value;
                OnPropertyChanged("StepSize");
            }
        }
        private decimal _tickSize { get; set; }
        public decimal TickSize
        {
            get { return _tickSize; }
            set
            {
                _tickSize = value;
                OnPropertyChanged("TickSize");
                int index = value.ToString().IndexOf("1");
                if (index == 0) RoundPrice = index;
                else RoundPrice = index - 1;
            }
        }
        private int _roundPrice { get; set; }
        public int RoundPrice
        {
            get { return _roundPrice; }
            set
            {
                _roundPrice = value;
                OnPropertyChanged("RoundPrice");
            }
        }
        private decimal _price { get; set; }
        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged("Price");
            }
        }
        private bool _isWait { get; set; }
        public bool IsWait
        {
            get { return _isWait; }
            set
            {
                _isWait = value;
                OnPropertyChanged("IsWait");
            }
        }
        private bool _isOpenOrder { get; set; }
        public bool IsOpenOrder
        {
            get { return _isOpenOrder; }
            set
            {
                _isOpenOrder = value;
                OnPropertyChanged("IsOpenOrder");
            }
        }
        private decimal _priceStopLoss { get; set; }
        public decimal PriceStopLoss
        {
            get { return _priceStopLoss; }
            set
            {
                _priceStopLoss = value;
                OnPropertyChanged("PriceStopLoss");
            }
        }
        private KlineInterval _klineInterval { get; set; }
        public KlineInterval KlineInterval
        {
            get { return _klineInterval; }
            set
            {
                _klineInterval = value;
                OnPropertyChanged("KlineInterval");
            }
        }
        private int _interval { get; set; } = 1;
        public int Interval
        {
            get { return _interval; }
            set
            {
                _interval = value;
                OnPropertyChanged("Interval");
                if (value == 1) KlineInterval = KlineInterval.OneMinute;
                else if (value == 3) KlineInterval = KlineInterval.ThreeMinutes;
                else if (value == 5) KlineInterval = KlineInterval.FiveMinutes;
                else if (value == 15) KlineInterval = KlineInterval.FifteenMinutes;
            }
        }
    }
}
