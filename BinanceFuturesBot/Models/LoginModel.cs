using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BinanceFuturesBot.Models
{
    internal class LoginModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private bool _isLogin { get; set; } = true;
        public bool IsLogin
        {
            get { return _isLogin; }
            set
            {
                _isLogin = value;
                OnPropertyChanged("IsLogin");
            }
        }
        private bool _isTrial { get; set; } = true;
        public bool IsTrial
        {
            get { return _isTrial; }
            set
            {
                _isTrial = value;
                OnPropertyChanged("IsTrial");
            }
        }
        private string _trialKey { get; set; } = "Valentyn";
        public string TrialKey
        {
            get { return _trialKey; }
            set
            {
                _trialKey = value;
                OnPropertyChanged("TrialKey");
            }
        }
        private string _apiKey { get; set; }
        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                _apiKey = value;
                OnPropertyChanged("ApiKey");
            }
        }
        private string _secretKey { get; set; }
        public string SecretKey
        {
            get { return _secretKey; }
            set
            {
                _secretKey = value;
                OnPropertyChanged("SecretKey");
            }
        }
    }
}
