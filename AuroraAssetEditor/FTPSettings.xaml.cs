// 
// 	FTPSettings.xaml.cs
// 	AuroraAssetEditor
// 
// 	Created by Swizzy on 10/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraAssetEditor {
    using System;
    using System.Linq;
    using System.Net.FtpClient;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Windows;

    /// <summary>
    ///     Interaction logic for FTPSettings.xaml
    /// </summary>
    public partial class FtpSettings {
        public FtpSettings() {
            InitializeComponent();
            App.FtpOperations.StatusChanged += (sender, args) => Dispatcher.Invoke(new Action(() => Status.Text = args.StatusMessage));
            ModeBox.Items.Clear();
            ModeBox.Items.Add(FtpDataConnectionType.PASVEX);
            ModeBox.Items.Add(FtpDataConnectionType.PASV);
            ModeBox.Items.Add(FtpDataConnectionType.PORT);
            if(!App.FtpOperations.HaveSettings) {
                ModeBox.SelectedIndex = 0;
                var ip = GetActiveIp();
                var index = ip.LastIndexOf('.');
                if(ip.Length > 0 && index > 0)
                    IpBox.Text = ip.Substring(0, index + 1);
            }
            else {
                IpBox.Text = App.FtpOperations.IpAddress;
                UserBox.Text = App.FtpOperations.Username;
                PassBox.Text = App.FtpOperations.Password;
                ModeBox.SelectedItem = App.FtpOperations.Mode;
            }
        }

        private static string GetActiveIp() {
            foreach(var unicastAddress in
                NetworkInterface.GetAllNetworkInterfaces().Where(f => f.OperationalStatus == OperationalStatus.Up).Select(f => f.GetIPProperties()).Where(
                                                                                                                                                          ipInterface =>
                                                                                                                                                          ipInterface.GatewayAddresses.Count > 0)
                                .SelectMany(
                                            ipInterface =>
                                            ipInterface.UnicastAddresses.Where(
                                                                               unicastAddress =>
                                                                               (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork) && (unicastAddress.IPv4Mask.ToString() != "0.0.0.0")))
                )
                return unicastAddress.Address.ToString();
            return "";
        }

        private void TestConnectionClick(object sender, RoutedEventArgs e) { App.FtpOperations.TestConnection(IpBox.Text, UserBox.Text, PassBox.Text, (FtpDataConnectionType)ModeBox.SelectedItem); }

        private void SaveSettingsClick(object sender, RoutedEventArgs e) {
            App.FtpOperations.SaveSettings();
            DialogResult = true;
        }
    }
}