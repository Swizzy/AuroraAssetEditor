// 
// 	IconBannerControl.xaml.cs
// 	AuroraAssetEditor
// 
// 	Created by Swizzy on 04/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraAssetEditor {
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using Microsoft.Win32;
    using Image = System.Drawing.Image;
    using Size = System.Drawing.Size;

    /// <summary>
    ///     Interaction logic for IconBannerControl.xaml
    /// </summary>
    public partial class IconBannerControl {
        private readonly MainWindow _main;
        private AuroraAsset.AssetFile _assetFile;
        private bool _haveBanner;
        private bool _haveIcon;

        public IconBannerControl(MainWindow main) {
            InitializeComponent();
            _main = main;
            _assetFile = new AuroraAsset.AssetFile();
            _haveBanner = false;
            _haveIcon = false;
        }

        public void Save() {
            var sfd = new SaveFileDialog();
            if(sfd.ShowDialog() == true)
                File.WriteAllBytes(sfd.FileName, _assetFile.FileData);
        }

        public void Reset() {
            //TODO: Set default banner
            _haveBanner = false;
            //TODO: Set default icon
            _haveIcon = false;
            _assetFile = new AuroraAsset.AssetFile();
        }

        public void Load(AuroraAsset.AssetFile asset) {
            _assetFile.SetIcon(asset);
            _assetFile.SetBanner(asset);
            SetPreview(_assetFile.GetIcon(), true);
            SetPreview(_assetFile.GetBanner(), false);
        }

        private void SetPreview(Image img, bool icon) {
            if(img == null) {
                if(icon) {
                    //TODO: Set default icon
                    _haveIcon = false;
                }
                else {
                    //TODO: Set default banner
                    _haveBanner = false;
                }
                return;
            }
            var bi = new BitmapImage();
            bi.BeginInit();
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();
            if(icon) {
                PreviewIcon.Source = bi;
                _haveIcon = true;
            }
            else {
                PreviewBanner.Source = bi;
                _haveBanner = true;
            }
        }

        public void Load(Image img, bool icon) {
            if(icon)
                _assetFile.SetIcon(img, _main.UseCompression.IsChecked);
            else
                _assetFile.SetBanner(img, _main.UseCompression.IsChecked);
            SetPreview(img, icon);
        }

        private void OnDragEnter(object sender, DragEventArgs e) { _main.OnDragEnter(sender, e); }

        private void OnDrop(object sender, DragEventArgs e) { _main.DragDrop(this, e); }

        private void SaveIconToFileOnClick(object sender, RoutedEventArgs e) { MainWindow.SaveToFile(_assetFile.GetIcon(), "Select where to save the Icon", "icon.png"); }

        private void SaveBannerToFileOnClick(object sender, RoutedEventArgs e) { MainWindow.SaveToFile(_assetFile.GetBanner(), "Select where to save the Banner", "banner.png"); }

        private void SelectNewIcon(object sender, RoutedEventArgs e) {
            var img = _main.LoadImage("Select new icon", "icon.png", new Size(64, 64));
            if(img != null)
                Load(img, true);
        }

        private void SelectNewBanner(object sender, RoutedEventArgs e) {
            var img = _main.LoadImage("Select new banner", "banner.png", new Size(420, 96));
            if(img != null)
                Load(img, true);
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {
            SaveIconContextMenuItem.IsEnabled = _haveIcon;
            SaveBannerContextMenuItem.IsEnabled = _haveBanner;
        }
    }
}