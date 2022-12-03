using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BinanceFuturesBot.Models
{
    public class LoginModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public ObservableCollection<string> Users { get; set; } = new();
        private string _selectedUser { get; set; }
        public string SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                OnPropertyChanged("SelectedUser");
            }
        }
        private bool _isLoginBinance { get; set; } = false;
        public bool IsLoginBinance
        {
            get { return _isLoginBinance; }
            set
            {
                _isLoginBinance = value;
                OnPropertyChanged("IsLoginBinance");
            }
        }
        private bool _isTrial { get; set; } = false;
        public bool IsTrial
        {
            get { return _isTrial; }
            set
            {
                _isTrial = value;
                OnPropertyChanged("IsTrial");
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
        private bool _isTestnet { get; set; }
        public bool IsTestnet
        {
            get { return _isTestnet; }
            set
            {
                _isTestnet = value;
                OnPropertyChanged("IsTestnet");
            }
        }
        private bool _isSave { get; set; }
        public bool IsSave
        {
            get { return _isSave; }
            set
            {
                _isSave = value;
                OnPropertyChanged("IsSave");
            }
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
    }
}
