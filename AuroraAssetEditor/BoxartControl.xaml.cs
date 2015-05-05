﻿// 
// 	BoxartControl.xaml.cs
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

    /// <summary>
    ///     Interaction logic for BoxartControl.xaml
    /// </summary>
    public partial class BoxartControl {
        private readonly MainWindow _main;
        private AuroraAsset.AssetFile _assetFile;
        private MemoryStream _memoryStream;
        private bool _havePreview;

        public BoxartControl(MainWindow main) {
            InitializeComponent();
            _main = main;
            _assetFile = new AuroraAsset.AssetFile();
            //TODO: Set default cover
            _havePreview = false;
        }

        public void Save() {
            var sfd = new SaveFileDialog();
            if(sfd.ShowDialog() == true)
                File.WriteAllBytes(sfd.FileName, _assetFile.FileData);
        }

        public void Reset() {
            //TODO: Set default cover
            _havePreview = false;
            if(_memoryStream != null)
                _memoryStream.Close();
            _assetFile = new AuroraAsset.AssetFile();
        }

        public void Load(AuroraAsset.AssetFile asset) {
            _assetFile.SetBoxart(asset);
            SetPreview(_assetFile.GetBoxart());
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
            _assetFile.SetBoxart(img, _main.UseCompression.IsChecked);
            SetPreview(img);
        }

        private void OnDragEnter(object sender, DragEventArgs e) { _main.OnDragEnter(sender, e); }

        private void OnDrop(object sender, DragEventArgs e) { _main.DragDrop(this, e); }

        private void SaveImageToFileOnClick(object sender, RoutedEventArgs e) { MainWindow.SaveToFile(_assetFile.GetBoxart(), "Select where to save the Cover", "cover.png"); }

        private void SelectNewCover(object sender, RoutedEventArgs e) {
            var img = _main.LoadImage("Select new cover", "cover.png", new System.Drawing.Size(900, 600));
            if(img != null)
                Load(img);
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e) { SaveContextMenuItem.IsEnabled = _havePreview; }
    }
}