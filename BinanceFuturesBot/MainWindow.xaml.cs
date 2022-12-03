using BinanceFuturesBot.ViewModels;
using System.Windows;

namespace BinanceFuturesBot
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
