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
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Microsoft.Win32;
    using Ookii.Dialogs.Wpf;
    using Image = System.Drawing.Image;
    using Size = System.Drawing.Size;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private const string AssetFileFilter =
            "Game Cover/Boxart Asset File(defaultFilename) (GC*.asset)|GC*.asset|Background Asset File(defaultFilename) (BK*.asset)|BK*.asset|Icon/Banner Asset File(defaultFilename) (GL*.asset)|GL*.asset|Screenshot Asset File(defaultFilename) (SS*.asset)|SS*.asset|Aurora Asset Files (*.asset)|*.asset|All Files(*)|*";

        private const string ImageFileFilter =
            "All Supported Images|*.png;*.bmp;*.jpg;*.jpeg;*.gif;*.tif;*.tiff;|BMP (*.bmp)|*.bmp|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|GIF (*.gif)|*.gif|TIFF (*.tif;*.tiff)|*.tiff;*.tif|PNG (*.png)|*.png|All Files|*";

        private readonly BackgroundControl _background;
        private readonly UIElement[] _backgroundMenu;
        private readonly BoxartControl _boxart;
        private readonly UIElement[] _boxartMenu;
        private readonly IconBannerControl _iconBanner;
        private readonly UIElement[] _iconBannerMenu;
        private readonly ScreenshotsControl _screenshots;
        private readonly UIElement[] _screenshotsMenu;

        public MainWindow(IEnumerable<string> args) {
            InitializeComponent();
            foreach(var mitem in SettingsMenu.Items)
                ((MenuItem)mitem).IsEnabled = false;
            var ver = Assembly.GetAssembly(typeof(MainWindow)).GetName().Version;
            Title = string.Format(Title, ver.Major, ver.Minor);

            #region Boxart

            _boxart = new BoxartControl(this);
            BoxartTab.Content = _boxart;
            _boxartMenu = new[] {
                                    new MenuItem {
                                                     Header = "Save Cover To File"
                                                 },
                                    new MenuItem {
                                                     Header = "Select new Cover"
                                                 }
                                };
            ((MenuItem)_boxartMenu[0]).Click += _boxart.SaveImageToFileOnClick;
            ((MenuItem)_boxartMenu[1]).Click += _boxart.SelectNewCover;

            #endregion

            #region Background

            _background = new BackgroundControl(this);
            BackgroundTab.Content = _background;
            _backgroundMenu = new[] {
                                        new MenuItem {
                                                         Header = "Save Background To File"
                                                     },
                                        new MenuItem {
                                                         Header = "Select new Background"
                                                     }
                                    };
            ((MenuItem)_backgroundMenu[0]).Click += _background.SaveImageToFileOnClick;
            ((MenuItem)_backgroundMenu[1]).Click += _background.SelectNewBackground;

            #endregion

            #region Icon & Banner

            _iconBanner = new IconBannerControl(this);
            IconBannerTab.Content = _iconBanner;
            _iconBannerMenu = new UIElement[] {
                                                  new MenuItem {
                                                                   Header = "Save Icon To File"
                                                               },
                                                  new MenuItem {
                                                                   Header = "Select new Icon"
                                                               },
                                                  new Separator(),
                                                  new MenuItem {
                                                                   Header = "Save Banner To File"
                                                               },
                                                  new MenuItem {
                                                                   Header = "Select new Banner"
                                                               }
                                              };
            ((MenuItem)_iconBannerMenu[0]).Click += _iconBanner.SaveIconToFileOnClick;
            ((MenuItem)_iconBannerMenu[1]).Click += _iconBanner.SelectNewIcon;
            ((MenuItem)_iconBannerMenu[3]).Click += _iconBanner.SaveBannerToFileOnClick;
            ((MenuItem)_iconBannerMenu[4]).Click += _iconBanner.SelectNewBanner;

            #endregion

            #region Screenshots

            _screenshots = new ScreenshotsControl(this);
            ScreenshotsTab.Content = _screenshots;
            _screenshotsMenu = new[] {
                                         new MenuItem {
                                                          Header = "Save Screenshot To File"
                                                      },
                                         new MenuItem {
                                                          Header = "Replace Screenshot"
                                                      },
                                         new MenuItem {
                                                          Header = "Add new Screenshot(s)"
                                                      },
                                         new MenuItem {
                                                          Header = "Remove screenshot"
                                                      }
                                     };
            ((MenuItem)_screenshotsMenu[0]).Click += _screenshots.SaveImageToFileOnClick;
            ((MenuItem)_screenshotsMenu[1]).Click += _screenshots.SelectNewScreenshot;
            ((MenuItem)_screenshotsMenu[2]).Click += _screenshots.AddNewScreenshot;
            ((MenuItem)_screenshotsMenu[3]).Click += _screenshots.RemoveScreenshot;

            #endregion

            foreach(var arg in args.Where(File.Exists)) {
                if(VerifyAuroraMagic(arg))
                    LoadAuroraAsset(arg);
                else
                    LoadFsdAsset(arg);
            }
        }

        private static void SaveError(Exception ex) { File.AppendAllText("error.log", string.Format("[{0}]:{2}{1}{2}", DateTime.Now, ex, Environment.NewLine)); }

        private static void SaveFileError(string file, Exception ex) {
            SaveError(ex);
            MessageBox.Show(string.Format("ERROR: There was an error while trying to process {0}{1}See Error.log for more information", file, Environment.NewLine), "ERROR", MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }

        private void LoadAuroraAsset(string filename) {
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
                SaveError(ex);
                MessageBox.Show(string.Format("ERROR: While processing {0}{1}{2}{1}See error.log for more details about this error", filename, Environment.NewLine, ex.Message), "ERROR",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFsdAsset(string filename) {
            try {
                var asset = new FsdAsset(File.ReadAllBytes(filename));
                var img = asset.GetBoxart();
                if(img != null) {
                    _boxart.Load(img);
                    BoxartTab.IsSelected = true;
                }
                img = asset.GetBackground();
                if(img != null) {
                    _background.Load(img);
                    BackgroundTab.IsSelected = true;
                }
                img = asset.GetIcon();
                if(img != null) {
                    _iconBanner.Load(img, true);
                    IconBannerTab.IsSelected = true;
                }
                img = asset.GetBanner();
                if(img != null) {
                    _iconBanner.Load(img, false);
                    IconBannerTab.IsSelected = true;
                }
                var screenshots = asset.GetScreenshots();
                if(screenshots.Length > 0) {
                    foreach(var ss in screenshots) {
                        if(_screenshots.SpaceLeft())
                            _screenshots.Load(ss, false);
                        else {
                            MessageBox.Show("ERROR: Not enough space to fit all screenshots, please clear current screenshots and load the FSD asset again...", "ERROR", MessageBoxButton.OK,
                                            MessageBoxImage.Error);
                            return;
                        }
                    }
                    ScreenshotsTab.IsSelected = true;
                }
                else
                    MessageBox.Show(string.Format("ERROR: {0} Doesn't contain any Assets", filename), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(Exception ex) {
                SaveError(ex);
                MessageBox.Show(string.Format("ERROR: While processing {0}{1}{2}{1}See error.log for more details about this error", filename, Environment.NewLine, ex.Message), "ERROR",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAssetOnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                                             Title = "Select Asset(s) to load",
                                             Filter = AssetFileFilter,
                                             FilterIndex = 5,
                                             Multiselect = true
                                         };
            if(ofd.ShowDialog() != true)
                return;
            foreach(var fileName in ofd.FileNames) {
                if(VerifyAuroraMagic(fileName))
                    LoadAuroraAsset(fileName);
                else
                    LoadFsdAsset(fileName);
            }
        }

        private static bool VerifyAuroraMagic(string fileName) {
            using(var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using(var br = new BinaryReader(stream))
                    return br.ReadUInt32() == 0x41455852; /* RXEA in LittleEndian format */
            }
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

        private Image GetImage(string filename, Size newSize) {
            try {
                var ms = new MemoryStream(File.ReadAllBytes(filename));
                var img = Image.FromStream(ms);
                if(!img.Size.Equals(newSize) && AutoResizeImages.IsChecked) {
                    //TODO: Add option to honor aspect ratio
                    img = new Bitmap(img, newSize);
                }
                return img;
            }
            catch(Exception ex) {
                SaveFileError(filename, ex);
                return null;
            }
        }

        internal void DragDrop(UIElement sender, DragEventArgs e) {
            if(!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var askScreenshot = true;
            foreach(var t in files) {
                if(VerifyAuroraMagic(t))
                    LoadAuroraAsset(t);
                if(VerifyFsdMagic(t))
                    LoadFsdAsset(t);
                else if(Equals(sender, _boxart))
                    _boxart.Load(GetImage(t, new Size(900, 600)));
                else if(Equals(sender, _background))
                    _background.Load(GetImage(t, new Size(1280, 720)));
                else if(Equals(sender, _screenshots)) {
                    if(askScreenshot && _screenshots.SelectedExists()) { // Do we have a screenshot selected?
                        var res = MessageBox.Show(string.Format("Do you want to replace the current Screenshot with {0}?", t), "Replace screenshot?", MessageBoxButton.YesNoCancel,
                                                  MessageBoxImage.Question, MessageBoxResult.Cancel);
                        if(res == MessageBoxResult.Yes) {
                            _screenshots.Load(GetImage(t, new Size(1000, 562)), true); // We want to replace it
                            askScreenshot = false;
                        }
                        else if(res == MessageBoxResult.No && _screenshots.SpaceLeft()) // Do we have space for another screenshot?
                            _screenshots.Load(GetImage(t, new Size(1000, 562)), false);
                    }
                    else if(_screenshots.SpaceLeft()) {
                        askScreenshot = false; // The user probably want to add the remaining covers...
                        _screenshots.Load(GetImage(t, new Size(1000, 562)), true);
                    }
                    else
                        MessageBox.Show("There is no space left for new screenshots :(", "No space left", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if(Equals(sender, _iconBanner)) {
                    var res = MessageBox.Show(string.Format("Is {0} an Icon? (If you select no it's assumed it's a banner)", t), "Is this an icon?", MessageBoxButton.YesNoCancel,
                                              MessageBoxImage.Question, MessageBoxResult.Cancel);
                    switch(res) {
                        case MessageBoxResult.Yes:
                            _iconBanner.Load(GetImage(t, new Size(64, 64)), true);
                            break;
                        case MessageBoxResult.No:
                            _iconBanner.Load(GetImage(t, new Size(420, 96)), false);
                            break;
                    }
                }
            }
        }

        private static bool VerifyFsdMagic(string fileName) {
            using(var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using(var br = new BinaryReader(stream))
                    return br.ReadUInt32() == 0x41445346; /* FSDA in LittleEndian format */
            }
        }

        private void OnMainDrop(object sender, DragEventArgs e) { DragDrop(this, e); }

        internal static void SaveToFile(Image img, string title, string defaultFilename) {
            var sfd = new SaveFileDialog {
                                             Title = title,
                                             FileName = defaultFilename,
                                             Filter = ImageFileFilter
                                         };
            if(sfd.ShowDialog() != true)
                return;
            var fmt = ImageFormat.Png;
            var extension = Path.GetExtension(sfd.FileName);
            if(extension != null) {
                switch(extension.ToLower()) {
                    case ".png":
                        break; // already our default
                    case ".jpg":
                    case ".jpeg":
                        fmt = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        fmt = ImageFormat.Bmp;
                        break;
                    case ".tif":
                    case ".tiff":
                        fmt = ImageFormat.Tiff;
                        break;
                    case ".gif":
                        fmt = ImageFormat.Gif;
                        break;
                }
            }
            using(var ms = new MemoryStream()) {
                img.Save(ms, fmt);
                File.WriteAllBytes(sfd.FileName, ms.ToArray());
            }
        }

        public Image LoadImage(string title, string defaultFilename, Size newSize) {
            var ofd = new OpenFileDialog {
                                             Title = title,
                                             FileName = defaultFilename,
                                             Filter = ImageFileFilter
                                         };
            return ofd.ShowDialog() != true ? null : (GetImage(ofd.FileName, newSize));
        }

        public IEnumerable<Image> LoadImages(string title, string defaultFilename, Size newSize) {
            var ofd = new OpenFileDialog {
                                             Title = title,
                                             FileName = defaultFilename,
                                             Filter = ImageFileFilter,
                                             Multiselect = true
                                         };
            return ofd.ShowDialog() != true ? null : ofd.FileNames.Select(fileName => GetImage(fileName, newSize));
        }

        private void TabChanged(object sender, SelectionChangedEventArgs e) {
            EditMenu.Items.Clear();
            if(BoxartTab.IsSelected) {
                foreach(var element in _boxartMenu)
                    EditMenu.Items.Add(element);
            }
            else if(BackgroundTab.IsSelected) {
                foreach(var element in _backgroundMenu)
                    EditMenu.Items.Add(element);
            }
            else if(IconBannerTab.IsSelected) {
                foreach(var element in _iconBannerMenu)
                    EditMenu.Items.Add(element);
            }
            else if(ScreenshotsTab.IsSelected) {
                foreach(var element in _screenshotsMenu)
                    EditMenu.Items.Add(element);
            }
        }

        private void EditMenuOpened(object sender, RoutedEventArgs e) {
            if(BoxartTab.IsSelected)
                ((MenuItem)EditMenu.Items[0]).IsEnabled = _boxart.HavePreview;
            else if(BackgroundTab.IsSelected)
                ((MenuItem)EditMenu.Items[0]).IsEnabled = _background.HavePreview;
            else if(IconBannerTab.IsSelected) {
                ((MenuItem)EditMenu.Items[0]).IsEnabled = _iconBanner.HaveIcon;
                ((MenuItem)EditMenu.Items[3]).IsEnabled = _iconBanner.HaveBanner;
            }
            else if(ScreenshotsTab.IsSelected) {
                ((MenuItem)EditMenu.Items[0]).IsEnabled = _screenshots.HavePreview;
                ((MenuItem)EditMenu.Items[3]).IsEnabled = _screenshots.HavePreview;
            }
        }

        private void AdvancedModeCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = e.Handled = true; }

        private void AdvancedModeOnExecuted(object sender, ExecutedRoutedEventArgs e) {
            foreach(var mitem in SettingsMenu.Items)
                ((MenuItem)mitem).IsEnabled = true;
        }

        private void SaveAllAssetsOnClick(object sender, RoutedEventArgs e) {
            var ipd = new InputDialog(this, "Please specify TitleID:");
            if(ipd.ShowDialog() != true || string.IsNullOrWhiteSpace(ipd.Value))
                return;
            var fsd = new VistaFolderBrowserDialog {
                                                       Description = "Select where to save the asset files"
                                                   };
            if(fsd.ShowDialog(this) != true)
                return;
            var filename = Path.Combine(fsd.SelectedPath, string.Format("GC{0}.asset", ipd.Value));
            if(_boxart.HavePreview || !File.Exists(filename))
                _boxart.Save(filename);
            filename = Path.Combine(fsd.SelectedPath, string.Format("BK{0}.asset", ipd.Value));
            if(_background.HavePreview || !File.Exists(filename))
                _background.Save(filename);
            filename = Path.Combine(fsd.SelectedPath, string.Format("GL{0}.asset", ipd.Value));
            if(_iconBanner.HaveBanner || _iconBanner.HaveIcon || !File.Exists(filename))
                _iconBanner.Save(filename);
            filename = Path.Combine(fsd.SelectedPath, string.Format("SS{0}.asset", ipd.Value));
            if(_screenshots.HaveScreenshots || !File.Exists(filename))
                _screenshots.Save(filename);
        }
    }

    public static class CustomCommands {
        public static readonly RoutedUICommand AdvancedMode = new RoutedUICommand("AdvancedMode", "AdvancedMode", typeof(CustomCommands), new InputGestureCollection {
                                                                                                                                                                         new KeyGesture(Key.F12,
                                                                                                                                                                                        ModifierKeys
                                                                                                                                                                                            .Control)
                                                                                                                                                                     });
    }
}