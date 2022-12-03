using BinanceFuturesBot.Command;
using BinanceFuturesBot.Models;
using CryptoExchange.Net;
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
        string site = "http://vburomanastia-001-site1.itempurl.com/";
        string _pathUsers = Directory.GetCurrentDirectory() + "/users/";
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
            if (!String.IsNullOrEmpty(LoginModel.SelectedUser) && File.Exists(_pathUsers + LoginModel.SelectedUser)) { 
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
                        if(result == "true")
                        {
                            LoginModel.IsTrial = true;
                            LoginModel.IsLoginBinance = true;
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
        private void Save()
        {
            using (var client = new HttpClient())
            {
                string trial = "";
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
