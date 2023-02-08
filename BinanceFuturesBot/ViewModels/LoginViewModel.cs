using Binance.Net.Clients;
using Binance.Net.Objects;
using BinanceFuturesBot.Command;
using BinanceFuturesBot.Models;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Windows;

namespace BinanceFuturesBot.ViewModels
{
    public class LoginViewModel
    {
        string site = "http://valiklola2-001-site1.etempurl.com/";
        string _pathUsers = Directory.GetCurrentDirectory() + "/users/";
        public BinanceClient? Client { get; set; }
        public BinanceSocketClient? SocketClient { get; set; }
        private RelayCommand? _saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(obj => {
                    Save();
                }));
            }
        }
        private RelayCommand? _loginCommand;
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand ?? (_loginCommand = new RelayCommand(obj => {
                    Login();
                }));
            }
        }
        public LoginModel LoginModel { get; set; } = new();
        public LoginViewModel()
        {
            if (!Directory.Exists(_pathUsers)) Directory.CreateDirectory(_pathUsers);
            LoadUsers();
        }
        private void Login()
        {
            try
            {
                if (!String.IsNullOrEmpty(LoginModel.SelectedUser) && File.Exists(_pathUsers + LoginModel.SelectedUser))
                {
                    string json = File.ReadAllText(_pathUsers + LoginModel.SelectedUser);
                    User? user = JsonConvert.DeserializeObject<User>(json);
                    if (user != null)
                    {
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri(site);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response = client.GetAsync($"api/User/Get?trialKey={user.TrialKey}&&apiKey={user.ApiKey}&&macAddress={GetMacAddress()}").Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            if (result == "true")
                            {
                                LoginModel.IsTrial = true;
                                LoginBinance(user.IsTestnet, user.ApiKey, user.SecretKey);
                            }
                            else
                            {
                                MessageBox.Show("Login failed");
                            }
                        }
                        else
                        {
                            MessageBox.Show($"{(int)response.StatusCode}, {response.ReasonPhrase}");
                        }
                    }
                }
            }
            catch {
                MessageBox.Show("Connection error");
            }
        }
        private void LoginBinance(bool testnet, string apiKey, string secretKey)
        {
            if (testnet)
            {
                // ------------- Test Api ----------------
                BinanceClientOptions clientOption = new();
                clientOption.UsdFuturesApiOptions.BaseAddress = "https://testnet.binancefuture.com";
                Client = new(clientOption);

                BinanceSocketClientOptions socketClientOption = new BinanceSocketClientOptions();
                socketClientOption.UsdFuturesStreamsOptions.AutoReconnect = true;
                socketClientOption.UsdFuturesStreamsOptions.ReconnectInterval = TimeSpan.FromMinutes(1);
                socketClientOption.UsdFuturesStreamsOptions.BaseAddress = "wss://stream.binancefuture.com";
                SocketClient = new BinanceSocketClient(socketClientOption);
                // ------------- Test Api ----------------
            }
            else
            {
                // ------------- Real Api ----------------
                Client = new();
                SocketClient = new();
                // ------------- Real Api ----------------
            }

            try
            {
                Client.SetApiCredentials(new ApiCredentials(apiKey, secretKey));
                SocketClient.SetApiCredentials(new ApiCredentials(apiKey, secretKey));

                if (CheckLogin())
                {
                    LoginModel.IsLoginBinance = true;
                }
                else {
                    LoginModel.IsTrial = false;
                    MessageBox.Show("Login binance failed!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool CheckLogin()
        {
            try
            {
                var result = Client.UsdFuturesApi.Account.GetAccountInfoAsync().Result;
                if (!result.Success) return false;
                else return true;
            }
            catch
            {
                return false;
            }
        }
        private void Save()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(site);
                Client loginClient = new();
                loginClient.MacAddress = GetMacAddress();
                loginClient.ApiKey = LoginModel.ApiKey;
                var response = client.PostAsJsonAsync("api/User/Add", loginClient).Result;
                if (response.IsSuccessStatusCode)
                {
                    User user = new();
                    user.TrialKey = response.Content.ReadAsStringAsync().Result;
                    user.Name = LoginModel.Name;
                    user.ApiKey = LoginModel.ApiKey;
                    user.SecretKey = LoginModel.SecretKey;
                    user.IsTestnet = LoginModel.IsTestnet;
                    if(!Directory.Exists(_pathUsers))Directory.CreateDirectory(_pathUsers);
                    File.WriteAllText(_pathUsers + user.Name, JsonConvert.SerializeObject(user));
                    LoadUsers();
                }
                else MessageBox.Show("Error added trial");
            }
        }
        private string GetMacAddress()
        {
            string macAddresses = "";
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddresses;
        }
        private void LoadUsers()
        {
            LoginModel.Users.Clear();
            if (!Directory.Exists(_pathUsers)) Directory.CreateDirectory(_pathUsers);
            if (LoginModel.Users.Count > 0) LoginModel.Users.Clear();
            foreach (var item in new DirectoryInfo(_pathUsers).GetFiles().Select(item => item.Name))
            {
                LoginModel.Users.Add(item);
            }
            if (LoginModel.Users.Count > 0) LoginModel.SelectedUser = LoginModel.Users[0];
        }
    }
}
