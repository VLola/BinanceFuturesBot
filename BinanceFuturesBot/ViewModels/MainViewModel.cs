using BinanceFuturesBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceFuturesBot.ViewModels
{
    internal class MainViewModel
    {
        public MainModel MainModel { get; set; } = new();
        public LoginModel LoginModel { get; set; } = new();
        public MainViewModel() { }

    }
}
