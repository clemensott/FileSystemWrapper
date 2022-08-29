using FileSystemCommon.Models.FileSystem.Content;
using FileSystemUWP.Picker;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FileSystemUWP.Util
{
    class FileSystemSortSelectorDialog
    {
        private readonly FileSystemSortSelector selector;
        private readonly ContentDialog dialog;

        public FileSystemSortSelectorDialog(FileSystemItemSortBy sortBy)
        {
            selector = new FileSystemSortSelector()
            {
                SortBy = sortBy,
            };
            selector.SelectionChanged += Selector_SelectionChanged;

            dialog = new ContentDialog()
            {
                Content = selector,
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "Cancel",
                IsSecondaryButtonEnabled = false,
            };
        }

        private void Selector_SelectionChanged(object sender, EventArgs e)
        {
            dialog.Hide();
        }

        private async Task<FileSystemItemSortBy?> Start()
        {
            ContentDialogResult result = await dialog.ShowAsync();

            return result == ContentDialogResult.None ? selector.SortBy : null;
        }

        public static Task<FileSystemItemSortBy?> Start(FileSystemItemSortBy sortBy)
        {
            FileSystemSortSelectorDialog dialog = new FileSystemSortSelectorDialog(sortBy);
            return dialog.Start();
        }
    }
}
