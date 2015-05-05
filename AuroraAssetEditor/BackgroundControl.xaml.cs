// 
// 	BackgroundControl.xaml.cs
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
    ///     Interaction logic for BackgroundControl.xaml
    /// </summary>
    public partial class BackgroundControl {
        private readonly MainWindow _main;
        private AuroraAsset.AssetFile _assetFile;
        private MemoryStream _memoryStream;
        private bool _havePreview;

        public BackgroundControl(MainWindow main) {
            InitializeComponent();
            _main = main;
            _assetFile = new AuroraAsset.AssetFile();
            //TODO: Set default background
            _havePreview = false;
        }

        public void Save() {
            var sfd = new SaveFileDialog();
            if(sfd.ShowDialog() == true)
                File.WriteAllBytes(sfd.FileName, _assetFile.FileData);
        }

        public void Reset() {
            //TODO: Set default background
            _havePreview = false;
            if(_memoryStream != null)
                _memoryStream.Close();
            _assetFile = new AuroraAsset.AssetFile();
        }

        public void Load(AuroraAsset.AssetFile asset) {
            _assetFile.SetBackground(asset);
            SetPreview(_assetFile.GetBackground());
        }

        private void SetPreview(Image img) {
            if(img == null)
                return;
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
            _havePreview = true;
        }

        public void Load(Image img) {
            _assetFile.SetBackground(img, _main.UseCompression.IsChecked);
            SetPreview(img);
        }

        private void OnDragEnter(object sender, DragEventArgs e) { _main.OnDragEnter(sender, e); }

        private void OnDrop(object sender, DragEventArgs e) { _main.DragDrop(this, e); }

        private void SaveImageToFileOnClick(object sender, RoutedEventArgs e) { MainWindow.SaveToFile(_assetFile.GetBackground(), "Select where to save the Background", "background.png"); }

        private void SelectNewCover(object sender, RoutedEventArgs e) {
            var img = _main.LoadImage("Select new background", "background.png", new Size(1280, 720));
            if(img != null)
                Load(img);
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e) { SaveContextMenuItem.IsEnabled = _havePreview; }
    }
}