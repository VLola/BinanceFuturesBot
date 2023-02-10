using Binance.Net.Enums;
using BinanceFuturesBot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BinanceFuturesBot.Models
{
    public class StatisticsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public List<BetViewModel> ListStatistics { get; set; } = new();
        public ObservableCollection<BetViewModel> Statistics { get; set; } = new();
        public List<string> Symbols { get; set; } = new();
        private decimal _sumTotal { get; set; }
        public decimal SumTotal
        {
            get { return _sumTotal; }
            set
            {
                _sumTotal = value;
                OnPropertyChanged("SumTotal");
                if (value >= 0m) IsPositive = true;
                else IsPositive = false;
            }
        }
        private bool _isPositive { get; set; }
        public bool IsPositive
        {
            get { return _isPositive; }
            set
            {
                _isPositive = value;
                OnPropertyChanged("IsPositive");
            }
        }
        private DateTime _startTime { get; set; } = DateTime.UtcNow.AddDays(-6);
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                OnPropertyChanged("StartTime");
            }
        }
        private DateTime _endTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
                OnPropertyChanged("EndTime");
            }
        }
        private bool _isEndTime { get; set; }
        public bool IsEndTime
        {
            get { return _isEndTime; }
            set
            {
                _isEndTime = value;
                OnPropertyChanged("IsEndTime");
            }
        }
        private int _requests { get; set; }
        public int Requests
        {
            get { return _requests; }
            set
            {
                _requests = value;
                OnPropertyChanged("Requests");
            }
        }
    }
}
