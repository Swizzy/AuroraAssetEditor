﻿// 
// 	FtpAssetsControl.xaml.cs
// 	AuroraAssetEditor
// 
// 	Created by Swizzy on 13/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraAssetEditor.Controls {
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Net.FtpClient;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using AuroraAssetEditor.Classes;

    /// <summary>
    ///     Interaction logic for FtpAssetsControl.xaml
    /// </summary>
    public partial class FtpAssetsControl {
        private readonly ThreadSafeObservableCollection<FtpAsset> _assetsList = new ThreadSafeObservableCollection<FtpAsset>();
        private readonly BackgroundControl _background;
        private readonly BoxartControl _boxart;
        private readonly IconBannerControl _iconBanner;
        private readonly MainWindow _main;
        private readonly ScreenshotsControl _screenshots;
        private byte[] _buffer;
        private bool _isBusy, _isError;

        public FtpAssetsControl(MainWindow main, BoxartControl boxart, BackgroundControl background, IconBannerControl iconBanner, ScreenshotsControl screenshots) {
            InitializeComponent();
            _main = main;
            _boxart = boxart;
            _background = background;
            _iconBanner = iconBanner;
            _screenshots = screenshots;
            App.FtpOperations.StatusChanged += (sender, args) => Dispatcher.Invoke(new Action(() => Status.Text = args.StatusMessage));
            ModeBox.Items.Clear();
            ModeBox.Items.Add(FtpDataConnectionType.PASVEX);
            ModeBox.Items.Add(FtpDataConnectionType.PASV);
            ModeBox.Items.Add(FtpDataConnectionType.PORT);
            FtpAssetsBox.ItemsSource = _assetsList;
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

        private void TestConnectionClick(object sender, RoutedEventArgs e) {
            var ip = IpBox.Text;
            var user = UserBox.Text;
            var pass = PassBox.Text;
            var mode = (FtpDataConnectionType)ModeBox.SelectedItem;
            var bw = new BackgroundWorker();
            bw.DoWork += (o, args) => App.FtpOperations.TestConnection(ip, user, pass, mode);
            bw.RunWorkerCompleted += (o, args) => _main.BusyIndicator.Visibility = Visibility.Collapsed;
            _main.BusyIndicator.Visibility = Visibility.Visible;
            Status.Text = string.Format("Running a connection test to {0}", IpBox.Text);
            bw.RunWorkerAsync();
        }

        private void SaveSettingsClick(object sender, RoutedEventArgs e) { App.FtpOperations.SaveSettings(); }

        private void GetAssetsClick(object sender, RoutedEventArgs e) {
            _assetsList.Clear();
            if(!App.FtpOperations.HaveSettings)
                return;
            var bw = new BackgroundWorker();
            bw.DoWork += (o, args) => {
                             try {
                                 App.FtpOperations.NavigateToGameDataDir();
                                 foreach(var dir in App.FtpOperations.GetDirList()) {
                                     var path = dir;
                                     Dispatcher.Invoke(new Action(() => Status.Text = string.Format("Processing {0}", path)));
                                     var tmp = FtpAsset.BuildAsset(path);
                                     if(tmp != null)
                                         _assetsList.Add(tmp);
                                 }
                                 args.Result = true;
                             }
                             catch(Exception ex) {
                                 MainWindow.SaveError(ex);
                                 args.Result = false;
                             }
                         };
            bw.RunWorkerCompleted += (o, args) => {
                                         _main.BusyIndicator.Visibility = Visibility.Collapsed;
                                         if((bool)args.Result)
                                             Status.Text = "Finished grabbing FTP Assets information successfully...";
                                         else
                                             Status.Text = "There was an error, check error.log for more information...";
                                     };
            _main.BusyIndicator.Visibility = Visibility.Visible;
            Status.Text = "Grabbing FTP Assets information...";
            bw.RunWorkerAsync();
        }

        private void ProcessAsset(Task task, bool shouldHideWhenDone = true) {
            _isError = false;
            FtpAsset asset = null;
            Dispatcher.InvokeIfRequired(() => asset = FtpAssetsBox.SelectedItem as FtpAsset, DispatcherPriority.Normal);
            if(asset == null)
                return;
            var bw = new BackgroundWorker();
            bw.DoWork += (sender, args) => {
                             try {
                                 switch(task) {
                                     case Task.GetBoxart:
                                         _buffer = asset.GetBoxart();
                                         break;
                                     case Task.GetBackground:
                                         _buffer = asset.GetBackground();
                                         break;
                                     case Task.GetIconBanner:
                                         _buffer = asset.GetIconBanner();
                                         break;
                                     case Task.GetScreenshots:
                                         _buffer = asset.GetScreenshots();
                                         break;
                                     case Task.SetBoxart:
                                         asset.SaveAsBoxart(_buffer);
                                         break;
                                     case Task.SetBackground:
                                         asset.SaveAsBackground(_buffer);
                                         break;
                                     case Task.SetIconBanner:
                                         asset.SaveAsIconBanner(_buffer);
                                         break;
                                     case Task.SetScreenshots:
                                         asset.SaveAsScreenshots(_buffer);
                                         break;
                                 }
                                 args.Result = true;
                             }
                             catch(Exception ex) {
                                 MainWindow.SaveError(ex);
                                 args.Result = false;
                             }
                         };
            bw.RunWorkerCompleted += (sender, args) => {
                                         if(shouldHideWhenDone)
                                             Dispatcher.InvokeIfRequired(() => _main.BusyIndicator.Visibility = Visibility.Collapsed, DispatcherPriority.Normal);
                                         var isGet = true;
                                         if((bool)args.Result) {
                                             if(_buffer.Length > 0) {
                                                 var aurora = new AuroraAsset.AssetFile(_buffer);
                                                 switch(task) {
                                                     case Task.GetBoxart:
                                                         _boxart.Load(aurora);
                                                         Dispatcher.InvokeIfRequired(() => _main.BoxartTab.IsSelected = true, DispatcherPriority.Normal);
                                                         break;
                                                     case Task.GetBackground:
                                                         _background.Load(aurora);
                                                         Dispatcher.InvokeIfRequired(() => _main.BackgroundTab.IsSelected = true, DispatcherPriority.Normal);
                                                         break;
                                                     case Task.GetIconBanner:
                                                         _iconBanner.Load(aurora);
                                                         Dispatcher.InvokeIfRequired(() => _main.IconBannerTab.IsSelected = true, DispatcherPriority.Normal);
                                                         break;
                                                     case Task.GetScreenshots:
                                                         _screenshots.Load(aurora);
                                                         Dispatcher.InvokeIfRequired(() => _main.ScreenshotsTab.IsSelected = true, DispatcherPriority.Normal);
                                                         break;
                                                     default:
                                                         isGet = false;
                                                         break;
                                                 }
                                             }
                                             if(shouldHideWhenDone && isGet)
                                                 Dispatcher.InvokeIfRequired(() => Status.Text = "Finished grabbing assets from FTP", DispatcherPriority.Normal);
                                             else if (shouldHideWhenDone)
                                                 Dispatcher.InvokeIfRequired(() => Status.Text = "Finished saving assets to FTP", DispatcherPriority.Normal);
                                         }
                                         else {
                                             switch(task) {
                                                 case Task.GetBoxart:
                                                 case Task.GetBackground:
                                                 case Task.GetIconBanner:
                                                 case Task.GetScreenshots:
                                                     break;
                                                 default:
                                                     isGet = false;
                                                     break;
                                             }
                                             if (isGet)
                                                 Dispatcher.InvokeIfRequired(() => Status.Text = "Failed getting asset data... See error.log for more information...", DispatcherPriority.Normal);
                                             else
                                                 Dispatcher.InvokeIfRequired(() => Status.Text = "Failed saving asset data... See error.log for more information...", DispatcherPriority.Normal);
                                             _isError = true;
                                         }
                                         _isBusy = false;
                                     };
            Dispatcher.InvokeIfRequired(() => _main.BusyIndicator.Visibility = Visibility.Visible, DispatcherPriority.Normal);
            _isBusy = true;
            bw.RunWorkerAsync();
        }

        private void GetBoxartClick(object sender, RoutedEventArgs e) { ProcessAsset(Task.GetBoxart); }

        private void GetBackgroundClick(object sender, RoutedEventArgs e) { ProcessAsset(Task.GetBackground); }

        private void GetIconBannerClick(object sender, RoutedEventArgs e) { ProcessAsset(Task.GetIconBanner); }

        private void GetScreenshotsClick(object sender, RoutedEventArgs e) { ProcessAsset(Task.GetScreenshots); }

        private void GetFtpAssetsClick(object sender, RoutedEventArgs e) {
            var bw = new BackgroundWorker();
            bw.DoWork += (o, args) => {
                             ProcessAsset(Task.GetBoxart, false);
                             while(_isBusy)
                                 Thread.Sleep(100);
                             if(_isError)
                                 return;
                             ProcessAsset(Task.GetBackground, false);
                             while(_isBusy)
                                 Thread.Sleep(100);
                             if(_isError)
                                 return;
                             ProcessAsset(Task.GetIconBanner, false);
                             while(_isBusy)
                                 Thread.Sleep(100);
                             if(_isError)
                                 return;
                             ProcessAsset(Task.GetScreenshots);
                         };
            bw.RunWorkerCompleted += (o, args) => _main.BusyIndicator.Visibility = Visibility.Collapsed;
            bw.RunWorkerAsync();
        }

        private void SaveFtpAssetsClick(object sender, RoutedEventArgs e) {
            var bw = new BackgroundWorker();
            bw.DoWork += (o, args) => {
                             Dispatcher.InvokeIfRequired(() => _buffer = _boxart.GetData(), DispatcherPriority.Normal);
                             ProcessAsset(Task.SetBoxart, false);
                             while(_isBusy)
                                 Thread.Sleep(100);
                             if(_isError)
                                 return;
                             Dispatcher.InvokeIfRequired(() => _buffer = _background.GetData(), DispatcherPriority.Normal);
                             ProcessAsset(Task.SetBackground, false);
                             while(_isBusy)
                                 Thread.Sleep(100);
                             if(_isError)
                                 return;
                             Dispatcher.InvokeIfRequired(() => _buffer = _iconBanner.GetData(), DispatcherPriority.Normal);
                             ProcessAsset(Task.SetIconBanner, false);
                             while(_isBusy)
                                 Thread.Sleep(100);
                             if(_isError)
                                 return;
                             Dispatcher.InvokeIfRequired(() => _buffer = _screenshots.GetData(), DispatcherPriority.Normal);
                             ProcessAsset(Task.SetScreenshots);
                         };
            bw.RunWorkerCompleted += (o, args) => _main.BusyIndicator.Visibility = Visibility.Collapsed;
            _main.BusyIndicator.Visibility = Visibility.Visible;
            bw.RunWorkerAsync();
        }

        private void SaveBoxartClick(object sender, RoutedEventArgs e) {
            var bw = new BackgroundWorker();
            bw.DoWork += (o, args) => {
                             Dispatcher.InvokeIfRequired(() => _buffer = _boxart.GetData(), DispatcherPriority.Normal);
                             ProcessAsset(Task.SetBoxart);
                         };
            _main.BusyIndicator.Visibility = Visibility.Visible;
            bw.RunWorkerAsync();
        }

        private void SaveBackgroundClick(object sender, RoutedEventArgs e) {
            var bw = new BackgroundWorker();
            bw.DoWork += (o, args) => {
                             Dispatcher.InvokeIfRequired(() => _buffer = _background.GetData(), DispatcherPriority.Normal);
                             ProcessAsset(Task.SetBackground);
                         };
            _main.BusyIndicator.Visibility = Visibility.Visible;
            bw.RunWorkerAsync();
        }

        private void SaveIconBannerClick(object sender, RoutedEventArgs e) {
            var bw = new BackgroundWorker();
            bw.DoWork += (o, args) => {
                             Dispatcher.InvokeIfRequired(() => _buffer = _iconBanner.GetData(), DispatcherPriority.Normal);
                             ProcessAsset(Task.SetIconBanner);
                         };
            _main.BusyIndicator.Visibility = Visibility.Visible;
            bw.RunWorkerAsync();
        }

        private void SaveScreenshotsClick(object sender, RoutedEventArgs e) {
            var bw = new BackgroundWorker();
            bw.DoWork += (o, args) => {
                             Dispatcher.InvokeIfRequired(() => _buffer = _screenshots.GetData(), DispatcherPriority.Normal);
                             ProcessAsset(Task.SetScreenshots);
                         };
            _main.BusyIndicator.Visibility = Visibility.Visible;
            bw.RunWorkerAsync();
        }

        internal class FtpAsset {
            public string TitleId { get; private set; }

            public string DatabaseId { get; private set; }

            public string Path { get { return string.Format("{0}_{1}", TitleId, DatabaseId); } }

            public string Title { get; private set; }

            public static FtpAsset BuildAsset(string path) {
                if(!Regex.IsMatch(path, "^[0-9A-Fa-f]{8}_[0-9A-Fa-f]{8}$"))
                    return null;
                var ret = new FtpAsset {
                                           TitleId = path.Substring(0, 8),
                                           DatabaseId = path.Substring(9)
                                       };
                var unity = App.TitleCache.Where(title => title.TitleId == ret.TitleId).ToArray();
                ret.Title = unity.Length > 0 ? unity[0].Title : "N/A";
                return ret;
            }

            public void SaveAsBoxart(byte[] data) { App.FtpOperations.SendAssetData(string.Format("GC{0}.asset", TitleId), Path, data); }

            public void SaveAsBackground(byte[] data) { App.FtpOperations.SendAssetData(string.Format("BK{0}.asset", TitleId), Path, data); }

            public void SaveAsIconBanner(byte[] data) { App.FtpOperations.SendAssetData(string.Format("GL{0}.asset", TitleId), Path, data); }

            public void SaveAsScreenshots(byte[] data) { App.FtpOperations.SendAssetData(string.Format("SS{0}.asset", TitleId), Path, data); }

            public byte[] GetBoxart() { return App.FtpOperations.GetAssetData(string.Format("GC{0}.asset", TitleId), Path); }

            public byte[] GetBackground() { return App.FtpOperations.GetAssetData(string.Format("BK{0}.asset", TitleId), Path); }

            public byte[] GetIconBanner() { return App.FtpOperations.GetAssetData(string.Format("GL{0}.asset", TitleId), Path); }

            public byte[] GetScreenshots() { return App.FtpOperations.GetAssetData(string.Format("SS{0}.asset", TitleId), Path); }
        }

        private enum Task {
            GetBoxart,
            GetBackground,
            GetIconBanner,
            GetScreenshots,
            SetBoxart,
            SetBackground,
            SetIconBanner,
            SetScreenshots,
        }
    }
}