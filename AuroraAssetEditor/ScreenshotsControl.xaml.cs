// 
// 	ScreenshotsControl.xaml.cs
// 	AuroraAssetEditor
// 
// 	Created by Swizzy on 04/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraAssetEditor {
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using Microsoft.Win32;
    using Image = System.Drawing.Image;
    using Size = System.Drawing.Size;

    /// <summary>
    ///     Interaction logic for ScreenshotsControl.xaml
    /// </summary>
    public partial class ScreenshotsControl {
        private readonly MainWindow _main;
        private AuroraAsset.AssetFile _assetFile;
        private Image[] _screenshots;
        private bool _havePreview;

        public ScreenshotsControl(MainWindow main) {
            InitializeComponent();
            _main = main;
            _assetFile = new AuroraAsset.AssetFile();
            _screenshots = new Image[AuroraAsset.AssetType.ScreenshotEnd - AuroraAsset.AssetType.ScreenshotStart];
            CBox.Items.Clear();
            for(var i = 0; i < _screenshots.Length; i++)
                CBox.Items.Add(new ScreenshotDisplay(i));
            CBox.SelectedIndex = 0;
        }

        public void Save() {
            var sfd = new SaveFileDialog();
            if(sfd.ShowDialog() == true)
                File.WriteAllBytes(sfd.FileName, _assetFile.FileData);
        }

        public void Reset() {
            SetPreview(null);
            _assetFile = new AuroraAsset.AssetFile();
            _screenshots = new Image[AuroraAsset.AssetType.ScreenshotEnd - AuroraAsset.AssetType.ScreenshotStart];
            CBox.SelectedIndex = 0;
        }

        public void Load(AuroraAsset.AssetFile asset) {
            _assetFile.SetScreenshots(asset);
            _screenshots = _assetFile.GetScreenshots();
            CBox_SelectionChanged(null, null);
        }

        private void SetPreview(Image img) {
            if(img == null) {
                PreviewImg.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Placeholders/screenshot.png", UriKind.Absolute));
                _havePreview = false;
                return;
            }
            var bi = new BitmapImage();
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            PreviewImg.Source = bi;
            _havePreview = true;
        }

        public void Load(Image img, bool replace) {
            var index = -1;
            if(replace) {
                var disp = CBox.SelectedItem as ScreenshotDisplay;
                if(disp == null) {
                    for(var i = 0; i < _screenshots.Length; i++) {
                        if(_screenshots[i] != null)
                            continue;
                        index = i;
                        break;
                    }
                }
                else
                    index = disp.Index;
            }
            else {
                for(var i = 0; i < _screenshots.Length; i++) {
                    if(_screenshots[i] != null)
                        continue;
                    index = i;
                    break;
                }
                if(index == -1)
                    return;
            }
            _assetFile.SetScreenshot(img, index + 1, _main.UseCompression.IsChecked);
            _screenshots[index] = img;
            SetPreview(img);
        }

        public bool SelectedExists() {
            var disp = CBox.SelectedItem as ScreenshotDisplay;
            if(disp != null)
                return _screenshots[disp.Index] != null; // If true, we want to ask if we should add new one or replace existing
            return false;
        }

        public bool SpaceLeft() {
            var ret = false;
            foreach(var t in _screenshots.Where(t => t == null))
                ret = true;
            return ret;
        }

        private void CBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var disp = CBox.SelectedItem as ScreenshotDisplay;
            if(disp == null)
                return;
            SetPreview(_screenshots[disp.Index]);
        }

        private void OnDragEnter(object sender, DragEventArgs e) { _main.OnDragEnter(sender, e); }

        private void OnDrop(object sender, DragEventArgs e) { _main.DragDrop(this, e); }

        private void RemoveScreenshot(object sender, RoutedEventArgs e) { Load(null, true); }

        private void SaveImageToFileOnClick(object sender, RoutedEventArgs e) {
            var disp = CBox.SelectedItem as ScreenshotDisplay;
            if(disp == null)
                return;
            MainWindow.SaveToFile(_screenshots[disp.Index], "Select where to save the Screenshot", string.Format("screenshot{0}.png", disp.Index));
        }

        private void SelectNewScreenshot(object sender, RoutedEventArgs e) {
            var img = _main.LoadImage("Select new screenshot", "screenshot.png", new Size(1000, 562));
            if(img != null)
                Load(img, true);
        }

        private void AddNewScreenshot(object sender, RoutedEventArgs e) {
            var img = _main.LoadImage("Select new screenshot", "screenshot.png", new Size(1000, 562));
            if(img != null)
                Load(img, false);
        }

        private class ScreenshotDisplay {
            private readonly int _index;

            public ScreenshotDisplay(int index) { _index = index; }

            public int Index { get { return _index; } }

            public override string ToString() { return string.Format("Screenshot {0}", _index + 1); }
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {
            SaveContextMenuItem.IsEnabled = _havePreview;
            RemoveContextMenuItem.IsEnabled = _havePreview;
        }
    }
}