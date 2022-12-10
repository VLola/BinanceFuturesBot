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
        public ObservableCollection<BetModel> Statistics { get; set; } = new();

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
    }
}
