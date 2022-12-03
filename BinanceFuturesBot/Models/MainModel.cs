using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceFuturesBot.Models
{
    internal class MainModel
    {
        public ObservableCollection<SymbolModel> SymbolModels { get; set; } = new();
    }
}
