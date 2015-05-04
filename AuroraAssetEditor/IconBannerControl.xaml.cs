// 
// 	IconBannerControl.xaml.cs
// 	AuroraAssetEditor
// 
// 	Created by Swizzy on 04/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraAssetEditor {
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using Microsoft.Win32;

    /// <summary>
    ///     Interaction logic for IconBannerControl.xaml
    /// </summary>
    public partial class IconBannerControl {
        private readonly MainWindow _main;
        private AuroraAsset.AssetFile _assetFile;
        private MemoryStream _bannerStream;
        private MemoryStream _iconStream;

        public IconBannerControl(MainWindow main) {
            InitializeComponent();
            _main = main;
            _assetFile = new AuroraAsset.AssetFile();
        }

        public void Save() {
            var sfd = new SaveFileDialog();
            if(sfd.ShowDialog() == true)
                File.WriteAllBytes(sfd.FileName, _assetFile.FileData);
        }

        public void Reset() {
            PreviewBanner.Source = null;
            PreviewIcon.Source = null;
            if(_iconStream != null)
                _iconStream.Close();
            if(_bannerStream != null)
                _bannerStream.Close();
            _assetFile = new AuroraAsset.AssetFile();
        }

        public void Load(AuroraAsset.AssetFile asset) {
            _assetFile.SetIcon(asset);
            _assetFile.SetBanner(asset);
            SetPreview(_assetFile.GetIcon(), true);
            SetPreview(_assetFile.GetBanner(), false);
        }

        private void SetPreview(Image img, bool icon) {
            if(img == null)
                return;
            var bi = new BitmapImage();
            bi.BeginInit();
            if(icon) {
                if(_iconStream != null)
                    _iconStream.Close();
                _iconStream = new MemoryStream();
                img.Save(_iconStream, ImageFormat.Bmp);
                _iconStream.Seek(0, SeekOrigin.Begin);
                bi.StreamSource = _iconStream;
            }
            else {
                if(_bannerStream != null)
                    _bannerStream.Close();
                _bannerStream = new MemoryStream();
                img.Save(_bannerStream, ImageFormat.Bmp);
                _bannerStream.Seek(0, SeekOrigin.Begin);
                bi.StreamSource = _bannerStream;
            }
            bi.EndInit();
            if(icon)
                PreviewIcon.Source = bi;
            else
                PreviewBanner.Source = bi;
        }

        public void Load(Image img, bool icon) {
            if(icon)
                _assetFile.SetIcon(img);
            else
                _assetFile.SetBanner(img);
            SetPreview(img, icon);
        }

        private void OnDragEnter(object sender, DragEventArgs e) { _main.OnDragEnter(sender, e); }

        private void OnDrop(object sender, DragEventArgs e) { _main.DragDrop(this, e); }
    }
}