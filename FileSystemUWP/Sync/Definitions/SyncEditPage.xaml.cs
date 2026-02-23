using FileSystemCommon;
using FileSystemCommon.Models.Configuration;
using FileSystemCommon.Models.FileSystem;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommon.Models.Sync.Definitions;
using FileSystemCommonUWP.Sync.Definitions;
using FileSystemUWP.Picker;
using StdOttStandard.Linq;
using StdOttUwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP.Sync.Definitions
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SyncEditPage : Page
    {
        private SyncPairEdit edit;

        public SyncEditPage()
        {
            this.InitializeComponent();

            ecbMode.Names = new Dictionary<object, string>()
            {
                [SyncMode.TwoWay] = "Two way",
                [SyncMode.ServerToLocal] = "Server to local",
                [SyncMode.ServerToLocalCreateOnly] = "Server to local (create and override only)",
                [SyncMode.LocalToServer] = "Local to server",
                [SyncMode.LocalToServerCreateOnly] = "Local to server (create and override only)",
            };

            ecbCompareType.Names = new Dictionary<object, string>()
            {
                [SyncCompareType.Exists] = "Exists",
                [SyncCompareType.Size] = "Size",
                [SyncCompareType.Hash] = "SHA1 hash",
                [SyncCompareType.PartialHash] = "Partial SHA1 hash",
            };

            ecbConflictHandlingType.Names = new Dictionary<object, string>()
            {
                [SyncConflictHandlingType.PreferServer] = "Prefer server",
                [SyncConflictHandlingType.PreferLocal] = "Prefer local",
                [SyncConflictHandlingType.Ignore] = "Ignore",
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            edit = (SyncPairEdit)e.Parameter;

            tblTitlePrefix.Text = edit.IsAdd ? "Add" : "Edit";
            DataContext = edit.Sync;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back && !edit.Task.IsCompleted) edit.SetResult(false);

            base.OnNavigatedFrom(e);
        }

        private object ServerPathConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            PathPart[] serverPath = (PathPart[])value;
            return serverPath.GetNamePath(edit.Api.Config.DirectorySeparatorChar);
        }

        private object LinesConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            IEnumerable<string> lines = ((IEnumerable<string>)value)?.Where(l => !string.IsNullOrWhiteSpace(l)) ?? new string[0];

            return string.Join("\r\n", lines);
        }

        private object LinesConverter_ConvertBackEvent(object value, Type targetType, object parameter, string language)
        {
            IEnumerable<string> lines = ((string)value)?.Split('\r', '\n')
                .Select(l => l.Trim('\r', '\n')).Where(l => !string.IsNullOrWhiteSpace(l));
            return new ObservableCollection<string>(lines ?? new string[0]);
        }

        private async void IbnSelectServerFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderPicking picking = FolderPicking.Folder(edit.Api, edit.Sync.ServerPath?.LastOrDefault().Path);
            Frame.Navigate(typeof(PickerPage), picking);
            PathPart[] serverPath = await picking.Task;
            if (serverPath != null) edit.Sync.ServerPath = serverPath;
        }

        private async void IbnSelectLocalFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            StorageFolder localFolder = await picker.PickSingleFolderAsync();

            if (localFolder != null) edit.LocalFolder = localFolder;
        }

        private bool Validate()
        {
            SyncPair sync = edit.Sync;
            return !string.IsNullOrWhiteSpace(sync.Name)
                && !edit.InvalidNames.Contains(sync.Name)
                && edit.LocalFolder != null
                && sync.ServerPath != null
                && sync.ServerPath.Length > 0;
        }

        private async void AbbApply_Click(object sender, RoutedEventArgs e)
        {
            tblTitlePrefix.Focus(FocusState.Pointer);
            await Task.Delay(50);

            if (!Validate())
            {
                await DialogUtils.ShowSafeAsync("Form not valid. Please fill out all required fields.");
                return;
            }

            edit.SetResult(true);
            Frame.GoBack();
        }

        private void AbbCancel_Click(object sender, RoutedEventArgs e)
        {
            Focus(FocusState.Pointer);
            Frame.GoBack();
        }
    }
}
