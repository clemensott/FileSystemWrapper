using FileSystemCommon;
using FileSystemCommon.Models.Configuration;
using FileSystemCommon.Models.FileSystem;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Folders;
using FileSystemCommonUWP.Sync.Definitions;
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
        private readonly IDictionary<string, string> folderPaths;

        public SyncEditPage()
        {
            this.InitializeComponent();

            folderPaths = new Dictionary<string, string>()
            {
                { "", "" },
            };

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
                [SyncConflictHandlingType.Igonre] = "Igonre",
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            edit = (SyncPairEdit)e.Parameter;
            char directorySeparatorChar = edit.Api.Config.DirectorySeparatorChar;

            PathPart[] serverPath = edit.Sync.ServerPath;
            if (serverPath != null)
            {
                for (int i = 0; i < serverPath.Length; i++)
                {
                    string path = serverPath
                        .Take(i + 1)
                        .GetNamePath(directorySeparatorChar);
                    folderPaths[path] = serverPath[i].Path;
                }
            }

            tblTitlePrefix.Text = edit.IsAdd ? "Add" : "Edit";
            DataContext = edit.Sync;
            asbServerPath.Text = edit.Sync.ServerPath.GetNamePath(directorySeparatorChar);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back && !edit.Task.IsCompleted) edit.SetResult(false);

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
            Config config = edit.Api.Config;
            sinServerPathValid.Symbol = Symbol.Help;

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput ||
                args.Reason == AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
            {
                string actualParentPath;
                string folderName = string.IsNullOrWhiteSpace(sender.Text) ?
                    string.Empty : sender.Text.Split(config.DirectorySeparatorChar, config.AltDirectorySeparatorChar).Last();
                string searchKey = folderName.ToLower();
                string parentPath = string.IsNullOrWhiteSpace(sender.Text) ?
                    string.Empty : config.GetParentPath(sender.Text).TrimEnd(config.DirectorySeparatorChar, config.AltDirectorySeparatorChar);
                FolderContent content = folderPaths.TryGetValue(parentPath, out actualParentPath) ?
                    await edit.Api.FolderContent(actualParentPath) : null;

                if (content?.Folders != null)
                {
                    foreach (FolderSortItem folder in content.Folders)
                    {
                        string path = content.Path
                            .GetChildPathParts(folder)
                            .GetNamePath(config.DirectorySeparatorChar)
                            .TrimEnd(config.DirectorySeparatorChar, config.AltDirectorySeparatorChar);
                        folderPaths[path] = folder.Path;
                    }

                    FolderSortItem currentFolder;
                    if (folderName.Length == 0) edit.Sync.ServerPath = content.Path;
                    else if (content.Folders.TryFirst(f => f.Name == folderName, out currentFolder) ||
                        content.Folders.TrySingle(f => f.Name.ToLower() == searchKey, out currentFolder))
                    {
                        edit.Sync.ServerPath = content.Path.GetChildPathParts(currentFolder).ToArray();
                    }

                    sender.ItemsSource = string.IsNullOrWhiteSpace(searchKey) ?
                        content.Folders : content.Folders.Where(f => f.Name.ToLower().Contains(searchKey));
                }
                else
                {
                    sender.ItemsSource = null;
                    edit.Sync.ServerPath = content?.Path;
                }
            }

            string actualPath;
            string namePath = sender.Text.TrimEnd(config.DirectorySeparatorChar);
            bool exists = folderPaths.TryGetValue(namePath, out actualPath) && await edit.Api.FolderExists(actualPath);
            sinServerPathValid.Symbol = exists ? Symbol.Accept : Symbol.Dislike;
        }

        private void AsbServerPath_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Config config = edit.Api.Config;
            FolderSortItem suggestion = args.ChosenSuggestion is FolderSortItem ? (FolderSortItem)args.ChosenSuggestion : (FolderSortItem)sender.Items[0];
            string parentPath = config.GetParentPath(sender.Text);

            sender.Text = config.JoinPaths(parentPath, suggestion.Name) + config.DirectorySeparatorChar;
            sender.Focus(FocusState.Keyboard);
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
