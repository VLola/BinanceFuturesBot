using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BinanceFuturesBot.Models
{
    public class SymbolDetailModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
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
        private int _maxLeverage { get; set; }
        public int MaxLeverage
        {
            get { return _maxLeverage; }
            set
            {
                _maxLeverage = value;
                OnPropertyChanged("MaxLeverage");
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
        private PositionSide _positionSide { get; set; }
        public PositionSide PositionSide
        {
            get { return _positionSide; }
            set
            {
                _positionSide = value;
                OnPropertyChanged("PositionSide");
            }
        }
    }
}
