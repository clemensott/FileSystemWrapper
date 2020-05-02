using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
            };

            ecbConflictHandlingType.Names = new Dictionary<object, string>()
            {
                [SyncConflictHandlingType.PreferServer] = "Prefer server",
                [SyncConflictHandlingType.PreferLocal] = "Prefer local",
                [SyncConflictHandlingType.Igonre] = "Igonre",
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            edit = (SyncPairEdit)e.Parameter;
            DataContext = edit.Sync;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back && !edit.HasResult) edit.SetValue(false);

            base.OnNavigatedFrom(e);
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

        private async void AsbServerPath_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            sinServerPathValid.Symbol = Symbol.Help;

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string searchKey = string.IsNullOrWhiteSpace(sender.Text) ?
                    null : Path.GetFileName(sender.Text).ToLower();
                string parentPath = string.IsNullOrWhiteSpace(sender.Text) ?
                    null : Path.GetDirectoryName(sender.Text);
                IEnumerable<string> dirs = await edit.Api.ListFolders(parentPath);

                if (dirs != null)
                {
                    dirs = dirs.Select(d => Path.GetFileName(d));

                    sender.ItemsSource = string.IsNullOrWhiteSpace(searchKey) ?
                        dirs : dirs.Where(d => d.ToLower().Contains(searchKey));
                }
                else sender.ItemsSource = null;
            }

            bool exists = await edit.Api.FolderExists(sender.Text);
            sinServerPathValid.Symbol = exists ? Symbol.Accept : Symbol.Dislike;
        }

        private void AsbServerPath_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string suggestion = args.ChosenSuggestion as string ?? (string)sender.Items[0];
            string parentPath = Path.GetDirectoryName(sender.Text);

            sender.Text = Path.Combine(parentPath, suggestion.TrimEnd('\\') + '\\');
            sender.Focus(FocusState.Keyboard);
        }

        private async void TbxServerPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            sinServerPathValid.Symbol = Symbol.Help;

            TextBox tbx = (TextBox)sender;
            bool exists = await edit.Api.FolderExists(tbx.Text);

            sinServerPathValid.Symbol = exists ? Symbol.Accept : Symbol.Dislike;
        }

        private async void IbnSelectLocalFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            StorageFolder localFolder = await picker.PickSingleFolderAsync();

            try
            {
                if (localFolder != null) edit.Sync.LocalFolder = localFolder;
            }
            catch { }
        }

        private void AbbApply_Click(object sender, RoutedEventArgs e)
        {
            edit.SetValue(true);
            Frame.GoBack();
        }

        private void AbbCancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
