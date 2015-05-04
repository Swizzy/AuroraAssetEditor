// 
// 	MainWindow.xaml.cs
// 	AuroraAssetEditor
// 
// 	Created by Swizzy on 04/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraAssetEditor {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using Microsoft.Win32;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        internal const string AssetFileFilter =
            "Game Cover/Boxart Asset File(s) (GC*.asset)|GC*.asset|Background Asset File(s) (BK*.asset)|BK*.asset|Icon/Banner Asset File(s) (GL*.asset)|GL*.asset|Screenshot Asset File(s) (SS*.asset)|SS*.asset|Aurora Asset Files (*.asset)|*.asset|All Files(*.*)|*.*";

        internal const string ImageFileFilter = "Image File(s) (*.png, *.bmp, *.jpeg, *.jpg, *.gif)|*.png;*.bmp;*.jpeg;*.jpg;*.gif|All Files (*.*)|*.*";
        private readonly BackgroundControl _background;
        private readonly BoxartControl _boxart;
        private readonly IconBannerControl _iconBanner;
        private readonly ScreenshotsControl _screenshots;

        public MainWindow(IEnumerable<string> args) {
            InitializeComponent();
            var ver = Assembly.GetAssembly(typeof(MainWindow)).GetName().Version;
            Title = string.Format(Title, ver.Major, ver.Minor);
            _boxart = new BoxartControl(this);
            BoxartTab.Content = _boxart;
            _background = new BackgroundControl(this);
            BackgroundTab.Content = _background;
            _screenshots = new ScreenshotsControl(this);
            ScreenshotsTab.Content = _screenshots;
            _iconBanner = new IconBannerControl(this);
            IconBannerTab.Content = _iconBanner;
            foreach(var arg in args.Where(File.Exists))
                LoadAsset(arg);
        }

        private static void SaveError(Exception ex) { File.AppendAllText("error.log", string.Format("[{0}]:{2}{1}{2}", DateTime.Now, ex, Environment.NewLine)); }

        private static void SaveFileError(string file, Exception ex) {
            SaveError(ex);
            MessageBox.Show(string.Format("ERROR: There was an error while trying to process {0}{1}See Error.log for more information", file, Environment.NewLine), "ERROR", MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }

        private bool LoadAsset(string filename, bool showError = true) {
            try {
                var asset = new AuroraAsset.AssetFile(File.ReadAllBytes(filename));
                if(asset.HasBoxArt) {
                    _boxart.Load(asset);
                    BoxartTab.IsSelected = true;
                }
                else if(asset.HasBackground) {
                    _background.Load(asset);
                    BackgroundTab.IsSelected = true;
                }
                else if(asset.HasScreenshots) {
                    _screenshots.Load(asset);
                    ScreenshotsTab.IsSelected = true;
                }
                else if(asset.HasIconBanner) {
                    _iconBanner.Load(asset);
                    IconBannerTab.IsSelected = true;
                }
                else
                    MessageBox.Show(string.Format("ERROR: {0} Doesn't contain any Assets", filename), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(Exception ex) {
                if(showError)
                    MessageBox.Show(string.Format("ERROR: While processing {0}{1}{2}", filename, Environment.NewLine, ex.Message), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void LoadAssetOnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                                             Title = "Select Asset to load",
                                             Filter = AssetFileFilter,
                                             FilterIndex = 5
                                         };
            if(ofd.ShowDialog() != true)
                return;
            LoadAsset(ofd.FileName);
        }

        private void CreateNewOnClick(object sender, RoutedEventArgs e) {
            _boxart.Reset();
            _background.Reset();
            _screenshots.Reset();
            _iconBanner.Reset();
        }

        private void SaveBoxartOnClick(object sender, RoutedEventArgs e) { _boxart.Save(); }

        private void SaveBackgroundOnClick(object sender, RoutedEventArgs e) { _background.Save(); }

        private void SaveScreenshotsOnClick(object sender, RoutedEventArgs e) { _screenshots.Save(); }

        private void SaveIconBannerOnClick(object sender, RoutedEventArgs e) { _iconBanner.Save(); }

        private void ExitOnClick(object sender, RoutedEventArgs e) { Close(); }

        internal void OnDragEnter(object sender, DragEventArgs e) {
            if(e.Data.GetDataPresent(DataFormats.FileDrop) && (e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None; // Ignore this one
        }

        internal void DragDrop(UIElement sender, DragEventArgs e) {
            if(!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach(var t in files.Where(t => !LoadAsset(t, false))) {
                if(Equals(sender, _boxart)) {
                    try {
                        _boxart.Load(Image.FromFile(t));
                    }
                    catch(Exception ex) {
                        SaveFileError(t, ex);
                    }
                }
                else if(Equals(sender, _background)) {
                    try
                    {
                        _background.Load(Image.FromFile(t));
                    }
                    catch (Exception ex)
                    {
                        SaveFileError(t, ex);
                    }
                }
                else if(Equals(sender, _screenshots)) {
                    //TODO: Implement other handling
                }
                else if(Equals(sender, _iconBanner)) {
                    //TODO: Implement other handling
                }
            }
        }

        private void OnMainDrop(object sender, DragEventArgs e) { DragDrop(this, e); }
    }
}