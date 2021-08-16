﻿using livelywpf.Helpers;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace livelywpf.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : System.Windows.Controls.Page
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private LibraryModel selectedTile;

        public LibraryView()
        {
            InitializeComponent();
            //uwp control also gets binded..
            this.DataContext = Program.LibraryVM;
        }

        private void GridControl_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            try
            {
                var a = ((FrameworkElement)e.OriginalSource).DataContext;
                selectedTile = (LibraryModel)a;
                customiseWallpaper.IsEnabled = selectedTile.LivelyPropertyPath != null;
            }
            catch
            {
                selectedTile = null;
                customiseWallpaper.IsEnabled = false;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTile != null)
            {
                LivelyGridControl_ContextMenuClick(sender, selectedTile);
            }
        }

        /// <summary>
        /// Not possible to do direct mvvm currently, putting the contextmenu inside datatemplate works but.. 
        /// the menu is opening only when right clicking on the DataTemplate content which is not covering completely the GridViewItem.
        /// So the workaround I did is set it outside of template and the datacontext is calculated in code behind.
        /// ref: https://github.com/microsoft/microsoft-ui-xaml/issues/911
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LivelyGridControl_ContextMenuClick(object sender, LibraryModel obj)
        {
            var s = sender as MenuItem;
            switch (s.Name)
            {
                case "previewWallpaper":
                    var prev = new WallpaperPreviewWindow(obj)
                    {
                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                        Owner = App.AppWindow
                    };
                    prev.Show();
                    break;
                case "showOnDisk":
                    Program.LibraryVM.WallpaperShowOnDisk(obj);
                    break;
                case "setWallpaper":
                    SetupDesktop.SetWallpaper(obj, Program.SettingsVM.Settings.SelectedDisplay);
                    break;
                case "exportWallpaper":
                    string savePath = "";
                    var saveFileDialog1 = new Microsoft.Win32.SaveFileDialog()
                    {
                        Title = "Select location to save the file",
                        Filter = "Lively/zip file|*.zip",
                        //title ending with '.' can have diff extension (example: parallax.js)
                        FileName = Path.ChangeExtension(obj.Title, ".zip"),
                    };
                    if (saveFileDialog1.ShowDialog() == true)
                    {
                        savePath = saveFileDialog1.FileName;
                    }
                    if (String.IsNullOrEmpty(savePath))
                    {
                        break;
                    }
                    Program.LibraryVM.WallpaperExport(obj, savePath);
                    break;
                case "deleteWallpaper":
                    var deleteView = new LibraryAboutView
                    {
                        DataContext = obj
                    };
                    var deleteFrame = new ModernWpf.Controls.Frame();
                    deleteFrame.Navigate(deleteView);
                    var result = await Helpers.DialogService.ShowConfirmationDialog(
                        (obj.LivelyInfo.IsAbsolutePath ?
                            Properties.Resources.DescriptionDeleteConfirmationLibrary : Properties.Resources.DescriptionDeleteConfirmation),
                        deleteFrame,
                        Properties.Resources.TextYes,
                        Properties.Resources.TextNo);
                    if (result == ContentDialogResult.Primary)
                    {
                        Program.LibraryVM.WallpaperDelete(obj);
                    }
                    break;
                case "customiseWallpaper":
                    var customiseFrame = new ModernWpf.Controls.Frame()
                    {
                        Margin = new Thickness(25, 0, 25, 25)
                    };
                    customiseFrame.Navigate(new Cef.LivelyPropertiesView(obj));
                    ScrollViewer scv = new ScrollViewer()
                    {
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Margin = new Thickness(0, 5, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    scv.Content = customiseFrame;
                    await Helpers.DialogService.ShowConfirmationDialog(
                        obj.Title.Length > 35 ? obj.Title.Substring(0, 35) + "..." : obj.Title,
                        scv,
                        Properties.Resources.TextClose);
                    break;
                case "convertVideo":
                    Program.LibraryVM.WallpaperVideoConvert(obj);
                    break;
                case "editWallpaper":
                    Program.LibraryVM.EditWallpaper(obj);
                    break;
                case "moreInformation":
                    var aboutView = new LibraryAboutView
                    {
                        DataContext = obj
                    };
                    var aboutFrame = new ModernWpf.Controls.Frame();
                    aboutFrame.Navigate(aboutView);
                    await Helpers.DialogService.ShowConfirmationDialog(
                        Properties.Resources.TitleAbout,
                        aboutFrame,
                        Properties.Resources.TextOK);
                    break;
            }
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void GridControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //keyboard disable.
            e.Handled = true;
        }
    }
}