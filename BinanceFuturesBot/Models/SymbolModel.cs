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
            }
        }
        private int _leverage { get; set; }
        public int Leverage
        {
            get { return _leverage; }
            set
            {
                _leverage = value;
                OnPropertyChanged("Leverage");
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
    }
}
