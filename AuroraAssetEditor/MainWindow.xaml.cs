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
    using Microsoft.Win32;
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
                                             Title = "Select Asset(s) to load",
                                             Filter = AssetFileFilter,
                                             FilterIndex = 5,
                                             Multiselect = true
                                         };
            if(ofd.ShowDialog() != true)
                return;
            foreach(var fileName in ofd.FileNames)
                LoadAsset(fileName);
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
            foreach(var t in files.Where(t => !LoadAsset(t, false))) {
                if(Equals(sender, _boxart))
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
    }
}