// 
// 	ScreenshotsControl.xaml.cs
// 	AuroraAssetEditor
// 
// 	Created by Swizzy on 04/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraAssetEditor {
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using Microsoft.Win32;
    using Image = System.Drawing.Image;

    /// <summary>
    ///     Interaction logic for ScreenshotsControl.xaml
    /// </summary>
    public partial class ScreenshotsControl {
        private readonly MainWindow _main;
        private AuroraAsset.AssetFile _assetFile;
        private MemoryStream _memoryStream;
        private Image[] _screenshots;

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
            PreviewImg.Source = null;
            if(_memoryStream != null)
                _memoryStream.Close();
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
                PreviewImg.Source = null;
                return;
            }
            if(_memoryStream != null)
                _memoryStream.Close();
            var bi = new BitmapImage();
            _memoryStream = new MemoryStream();
            img.Save(_memoryStream, ImageFormat.Bmp);
            _memoryStream.Seek(0, SeekOrigin.Begin);
            bi.BeginInit();
            bi.StreamSource = _memoryStream;
            bi.EndInit();
            PreviewImg.Source = bi;
        }

        public void Load(Image img, bool replace) {
            if(replace) {
                var disp = CBox.SelectedItem as ScreenshotDisplay;
                int index = 0;
                if(disp == null) {
                    //TODO: Finish this up
                }
            }
        }

        public bool SelectedExists() {
            var disp = CBox.SelectedItem as ScreenshotDisplay;
            if(disp != null)
                return _screenshots[disp.Index] != null; // If true, we want to ask if we should add new one or replace existing
            return false;
        }

        public bool SpaceLeft() {
            var ret = false;
            foreach (var t in _screenshots.Where(t => t == null))
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

        private class ScreenshotDisplay {
            private readonly int _index;

            public ScreenshotDisplay(int index) { _index = index; }

            public int Index { get { return _index; } }

            public override string ToString() { return string.Format("Screenshot {0}", _index + 1); }
        }
    }
}