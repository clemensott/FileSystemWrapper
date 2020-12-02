using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using StdOttStandard.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace FileSystemUWP.Picker
{
    public sealed partial class PickerControl : UserControl
    {
        public static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.Register("IsUpdating",
            typeof(bool), typeof(PickerControl), new PropertyMetadata(false));

        public static readonly DependencyProperty CurrentFolderProperty = DependencyProperty.Register("CurrentFolder",
            typeof(FileSystemItem?), typeof(PickerControl), new PropertyMetadata(default(FileSystemItem?), OnCurrentFolderPropertyChanged));

        private static void OnCurrentFolderPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PickerControl s = (PickerControl)sender;
            FileSystemItem? value = (FileSystemItem?)e.NewValue;

            s.CurrentFolderNamePath = value?.PathParts.GetNamePath();
        }

        public static readonly DependencyProperty CurrentFolderNamePathProperty = DependencyProperty.Register("CurrentFolderNamePath",
            typeof(string), typeof(PickerControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ApiProperty = DependencyProperty.Register("Api",
            typeof(Api), typeof(PickerControl), new PropertyMetadata(null, OnApiPropertyChanged));

        private static async void OnApiPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PickerControl s = (PickerControl)sender;
            await s.UpdateCurrentFolderItems(true);
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(FileSystemItemViewType), typeof(PickerControl),
                new PropertyMetadata(FileSystemItemViewType.None, OnTypePropertyChanged));

        private static async void OnTypePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PickerControl s = (PickerControl)sender;
            await s.UpdateCurrentFolderItems(true);
        }

        public static readonly DependencyProperty FlyoutMenuItemsProperty = DependencyProperty.Register("FlyoutMenuItems",
            typeof(IEnumerable<FlyoutMenuItem>), typeof(PickerControl), new PropertyMetadata(null));

        private long updateCount = 0;
        private string currentUpdatePath;
        private readonly FileSystemItemCollection currentItems;

        public event EventHandler<FileSystemItem> FileSelected;

        public bool IsUpdating
        {
            get => (bool)GetValue(IsUpdatingProperty);
            private set => SetValue(IsUpdatingProperty, value);
        }

        public FileSystemItem? CurrentFolder
        {
            get => (FileSystemItem?)GetValue(CurrentFolderProperty);
            private set => SetValue(CurrentFolderProperty, value);
        }

        public string CurrentFolderNamePath
        {
            get => (string)GetValue(CurrentFolderNamePathProperty);
            private set => SetValue(CurrentFolderNamePathProperty, value);
        }

        public Api Api
        {
            get => (Api)GetValue(ApiProperty);
            set => SetValue(ApiProperty, value);
        }

        public FileSystemItemViewType Type
        {
            get => (FileSystemItemViewType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public IEnumerable<FlyoutMenuItem> FlyoutMenuItems
        {
            get => (IEnumerable<FlyoutMenuItem>)GetValue(FlyoutMenuItemsProperty);
            set => SetValue(FlyoutMenuItemsProperty, value);
        }

        public PickerControl()
        {
            this.InitializeComponent();

            lvwItems.ItemsSource = currentItems = new FileSystemItemCollection();
        }

        private object Path_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            string path = (string)value;

            return string.IsNullOrWhiteSpace(path) ? "Root" : path;
        }

        private object SymConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            return UiUtils.GetSymbol((FileSystemItem)value);
        }

        public Task SetParent()
        {
            PathPart[] currentPath = CurrentFolder?.PathParts;
            if (currentPath == null || currentPath.Length == 0) return Task.CompletedTask;

            string newPath = currentPath.Length == 1 ? string.Empty : currentPath[currentPath.Length - 2].Path;
            return SetCurrentFolder(newPath);
        }

        public Task SetCurrentFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) path = string.Empty;

            return UpdateCurrentFolderItems(path, true);
        }

        public Task UpdateCurrentFolderItems(bool clearItems = false)
        {
            return UpdateCurrentFolderItems(CurrentFolder?.FullPath, clearItems);
        }

        private async Task UpdateCurrentFolderItems(string path, bool clearItems)
        {
            long currentCount = ++updateCount;

            if (clearItems) currentItems.Clear();

            IsUpdating = true;

            await UpdateContent(path);

            if (currentCount == updateCount) IsUpdating = false;
        }

        public Task UpdateContent()
        {
            return UpdateContent(CurrentFolder?.FullPath);
        }

        public async Task UpdateContent(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) path = string.Empty;
            currentUpdatePath = path;
            Api api = Api;

            if (!Type.HasFlag(FileSystemItemViewType.Folders) || api == null) return;

            FolderContent content = api != null ? await api.FolderContent(path) : null;
            if (path != currentUpdatePath) return;

            currentItems.SetFolders((content?.Folders).ToNotNull()
                .Select(f => FileSystemItem.FromFolder(f, content.Path)));
            currentItems.SetFiles((content?.Files).ToNotNull()
                .Select(f => FileSystemItem.FromFile(f, content.Path)));
            CurrentFolder = content != null ? (FileSystemItem?)FileSystemItem.FromFolderContent(content) : null;
        }

        private async void LvwItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Selector lbx = (Selector)sender;

            if (!(lbx.SelectedItem is FileSystemItem)) return;

            FileSystemItem item = (FileSystemItem)lbx.SelectedItem;
            lbx.SelectedItem = null;

            if (item.IsFolder) await SetCurrentFolder(item.FullPath);
            else if (item.IsFile) FileSelected?.Invoke(this, item);
        }

        private void SplItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            OpenFlyout((FrameworkElement)sender);
        }

        private void SplItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            OpenFlyout((FrameworkElement)sender);
        }

        private void OpenFlyout(FrameworkElement element)
        {
            if (FlyoutMenuItems == null) return;

            FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(element);
            if (flyout == null) FlyoutBase.SetAttachedFlyout(element, CreateFlyout(FlyoutMenuItems));

            FlyoutBase.ShowAttachedFlyout(element);
        }

        private static MenuFlyout CreateFlyout(IEnumerable<FlyoutMenuItem> items)
        {
            MenuFlyout flyout = new MenuFlyout();

            foreach (FlyoutMenuItem srcItem in items)
            {
                MenuFlyoutItem destItem = new MenuFlyoutItem()
                {
                    Icon = srcItem.Symbol.HasValue ? new SymbolIcon(srcItem.Symbol.Value) : null,
                    Text = srcItem.Text ?? string.Empty,
                };

                destItem.Click += (sender, e) => srcItem.Raise((FileSystemItem)((FrameworkElement)sender).DataContext);

                flyout.Items.Add(destItem);
            }

            return flyout;
        }

        public IEnumerable<FileSystemItem> GetCurrentItems()
        {
            return currentItems;
        }
    }
}
