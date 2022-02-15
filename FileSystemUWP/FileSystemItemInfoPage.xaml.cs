﻿using FileSystemCommon.Models.FileSystem;
using FileSystemUWP.API;
using StdOttStandard.Linq;
using StdOttUwp;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class FileSystemItemInfoPage : Page
    {
        private static FileSystemItemInfoPageViewModel viewModel;
        private bool leftPage;
        private Api api;

        public FileSystemItemInfoPage()
        {
            this.InitializeComponent();

            DataContext = viewModel = new FileSystemItemInfoPageViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (FileSystemItem item, Api api) = ((FileSystemItem, Api))e.Parameter;

            this.api = api;
            viewModel.Item = item;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            leftPage = true;
            base.OnNavigatedFrom(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadInfos();
        }

        private object PathConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            return ((PathPart[])value).Select(p => p.Name).Join("\\");
        }

        private object SizeCon_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            string[] endings = new string[] { "B", "kB", "MB", "GB", "TB", "PB", "EB" };
            double size = Convert.ToDouble((long)value);
            string ending = endings.Last();

            for (int i = 0; i < endings.Length; i++)
            {
                if (size < 1024)
                {
                    ending = endings[i];
                    break;
                }

                size = size / 1024.0;
            }

            if (size < 10) return string.Format("{0} {1}", Math.Round(size, 2), ending);
            if (size < 100) return string.Format("{0} {1}", Math.Round(size, 1), ending);

            return string.Format("{0} {1}", Math.Round(size, 0), ending);
        }

        private void AbbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AbbRefresh_Click(object sender, RoutedEventArgs e)
        {
            await ReloadInfos();
        }

        private async Task ReloadInfos()
        {
            try
            {
                viewModel.IsReloading = true;

                object info = viewModel.Item.IsFile ?
                    (object)await api.GetFileInfo(viewModel.Item.FullPath) :
                    await api.GetFolderInfo(viewModel.Item.FullPath);


                if (info != null) viewModel.Info = info;
                else if (!leftPage)
                {
                    await DialogUtils.ShowSafeAsync("Loading infos failed");
                    Frame.GoBack();
                }

            }
            catch (Exception exc)
            {
                if (leftPage) return;

                await DialogUtils.ShowSafeAsync(exc.Message, "Load infos error");
                Frame.GoBack();
            }
            finally
            {
                viewModel.IsReloading = false;
            }
        }

        class FileSystemItemInfoPageViewModel : INotifyPropertyChanged
        {
            private bool isReloading;
            private FileSystemItem item;
            private object info;

            public bool IsReloading
            {
                get => isReloading;
                set
                {
                    if (value == isReloading) return;

                    isReloading = value;
                    OnPropertyChanged(nameof(IsReloading));
                }
            }

            public FileSystemItem Item
            {
                get => item;
                set
                {
                    if (value.Equals(item)) return;

                    item = value;
                    OnPropertyChanged(nameof(Item));
                }
            }

            public object Info
            {
                get => info;
                set
                {
                    if (value == info) return;

                    info = value;
                    OnPropertyChanged(nameof(Info));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
