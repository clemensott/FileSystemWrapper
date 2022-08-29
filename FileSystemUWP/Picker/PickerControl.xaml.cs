using FileSystemCommon.Models.FileSystem;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemUWP.API;
using FileSystemUWP.Models;
using StdOttStandard.Linq;
using StdOttUwp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

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
            typeof(string), typeof(PickerControl), new PropertyMetadata(null));

        public static readonly DependencyProperty SortByProperty =
            DependencyProperty.Register(nameof(SortBy), typeof(FileSystemItemSortBy), typeof(PickerControl),
                new PropertyMetadata(default(FileSystemItemSortBy), OnSortByPropertyChanged));

        private static async void OnSortByPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            await ((PickerControl)sender).UpdateContent();
        }

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

        public static readonly DependencyProperty FileMenuFlyoutProperty = DependencyProperty
            .Register(nameof(FileMenuFlyout), typeof(MenuFlyout), typeof(PickerControl), new PropertyMetadata(null));

        public static readonly DependencyProperty FolderMenuFlyoutProperty = DependencyProperty
            .Register(nameof(FolderMenuFlyout), typeof(MenuFlyout), typeof(PickerControl), new PropertyMetadata(null));

        private long updateCount = 0;
        private string currentUpdatePath;
        private readonly FileSystemItemCollection currentItems;
        private ScrollViewer svrItems;

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

        public FileSystemItemSortBy SortBy
        {
            get => (FileSystemItemSortBy)GetValue(SortByProperty);
            set => SetValue(SortByProperty, value);
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

        public MenuFlyout FileMenuFlyout
        {
            get => (MenuFlyout)GetValue(FileMenuFlyoutProperty);
            set => SetValue(FileMenuFlyoutProperty, value);
        }

        public MenuFlyout FolderMenuFlyout
        {
            get => (MenuFlyout)GetValue(FolderMenuFlyoutProperty);
            set => SetValue(FolderMenuFlyoutProperty, value);
        }

        public PickerControl()
        {
            this.InitializeComponent();

            lvwItems.ItemsSource = currentItems = new FileSystemItemCollection();
        }

        private object Path_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            string path = (string)value;

            if (path == null) return string.Empty;
            return string.IsNullOrWhiteSpace(path) ? "Root" : path;
        }

        private object SymConverter_ConvertEvent(object value, Type targetType, object parameter, string language)
        {
            return UiUtils.GetSymbol((FileSystemItem)value);
        }

        private void LvwItems_Loaded(object sender, RoutedEventArgs e)
        {
            svrItems = FindVisualChild<ScrollViewer>(lvwItems);
        }

        public double GetVerticalScrollOffset()
        {
            return svrItems.VerticalOffset;
        }

        public void SetVerticalScrollOffset(double offset)
        {
            if (svrItems.ScrollableHeight < offset) offset = svrItems.ScrollableHeight;

            svrItems.ChangeView(null, offset, null, true);
        }

        private static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem) return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null) return childOfChild;
                }
            }
            return null;
        }

        internal FileSystemSortItem? GetCenteredFileSystemSortItem()
        {
            double scrolledFactor = svrItems.VerticalOffset / svrItems.ExtentHeight;
            int index = (int)Math.Floor(scrolledFactor * currentItems.Count);

            FileSystemItem item;
            if (currentItems.TryElementAt(index, out item))
            {
                return new FileSystemSortItem(item.IsFile, item.SortKeys);
            }
            return null;
        }

        internal void ScrollToFileItemName(FileSystemSortItem itemName)
        {
            FileSystemItem? item = currentItems.GetNearestItem(itemName);
            if (item.HasValue) lvwItems.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
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

            FolderContent content = api != null ?
                await api.FolderContent(path, SortBy.Type, SortBy.Direction) : null;
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
            if (e.HoldingState == HoldingState.Started)
            {
                OpenFlyout((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        private void SplItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == PointerDeviceType.Mouse)
            {
                OpenFlyout((FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
            }
        }

        private void OpenFlyout(FrameworkElement element, Point position)
        {
            FileSystemItem item = UwpUtils.GetDataContext<FileSystemItem>(element);
            MenuFlyout flyout = item.IsFile ? FileMenuFlyout : FolderMenuFlyout;
            if (FileMenuFlyout == null) return;

            FlyoutBase.SetAttachedFlyout(element, flyout);
            flyout.ShowAt(element, position);
        }

        public IEnumerable<FileSystemItem> GetCurrentItems()
        {
            return currentItems;
        }
    }
}
