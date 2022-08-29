using FileSystemCommon.Models.FileSystem.Content;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace FileSystemUWP.Picker
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class FileSystemSortSelector : Page
    {
        private readonly SortItem[] items;
        private FileSystemItemSortBy? sortBy;

        public event EventHandler SelectionChanged;

        public FileSystemItemSortBy? SortBy
        {
            get => sortBy;
            set
            {
                if (Equals(value, sortBy)) return;

                sortBy = value;
                lbx.SelectedIndex = items
                    .ToList()
                    .FindIndex(i => i.Type == SortBy?.Type && i.Direction == SortBy?.Direction);
            }
        }

        public FileSystemSortSelector()
        {
            this.InitializeComponent();

            items = new SortItem[]
            {
                new SortItem(FileSystemItemSortType.Name, FileSystemItemSortDirection.ASC, "Name ASC"),
                new SortItem(FileSystemItemSortType.Name, FileSystemItemSortDirection.DESC, "Name DESC"),
                new SortItem(FileSystemItemSortType.Size, FileSystemItemSortDirection.ASC, "Size ASC"),
                new SortItem(FileSystemItemSortType.Size, FileSystemItemSortDirection.DESC, "Size DESC"),
                new SortItem(FileSystemItemSortType.CreationTime, FileSystemItemSortDirection.ASC, "Creation time ASC"),
                new SortItem(FileSystemItemSortType.CreationTime, FileSystemItemSortDirection.DESC, "Creation time DESC"),
                new SortItem(FileSystemItemSortType.LastWriteTime, FileSystemItemSortDirection.ASC, "Last write time ASC"),
                new SortItem(FileSystemItemSortType.LastWriteTime, FileSystemItemSortDirection.DESC, "Last write time DESC"),
                new SortItem(FileSystemItemSortType.LastAccessTime, FileSystemItemSortDirection.ASC, "Last access time ASC"),
                new SortItem(FileSystemItemSortType.LastAccessTime, FileSystemItemSortDirection.DESC, "Last access time DESC"),
            };

            lbx.ItemsSource = items;
        }

        private void Lbx_Loaded(object sender, RoutedEventArgs e)
        {
            lbx.SelectionChanged += Lbx_SelectionChanged;
        }

        private void Lbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbx.SelectedItem is SortItem)
            {
                SortItem item = (SortItem)lbx.SelectedItem;
                SortBy = new FileSystemItemSortBy()
                {
                    Type = item.Type,
                    Direction = item.Direction,
                };

                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            else SortBy = null;
        }

        private struct SortItem
        {
            public FileSystemItemSortType Type { get; }

            public FileSystemItemSortDirection Direction { get; }

            public string Name { get; }

            public SortItem(FileSystemItemSortType type, FileSystemItemSortDirection direction, string name)
            {
                Type = type;
                Direction = direction;
                Name = name;
            }
        }
    }
}
