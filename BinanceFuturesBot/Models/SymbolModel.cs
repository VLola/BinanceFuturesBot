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
        private int _open { get; set; }
        public int Open
        {
            get { return _open; }
            set
            {
                _open = value;
                OnPropertyChanged("Open");
            }
        }
        private int _close { get; set; }
        public int Close
        {
            get { return _close; }
            set
            {
                _close = value;
                OnPropertyChanged("Close");
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
        private decimal _stopLoss { get; set; }
        public decimal StopLoss
        {
            get { return _stopLoss; }
            set
            {
                _stopLoss = value;
                OnPropertyChanged("StopLoss");
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
