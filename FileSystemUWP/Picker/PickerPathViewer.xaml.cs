using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace FileSystemUWP.Picker
{
    public sealed partial class PickerPathViewer : UserControl
    {
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(string), typeof(PickerPathViewer),
                new PropertyMetadata(default(string), OnPathPropertyChanged));

        private static void OnPathPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PickerPathViewer s = (PickerPathViewer)sender;
            string newValue = (string)e.NewValue;

            s.SetFittingPath(newValue);
        }

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public PickerPathViewer()
        {
            this.InitializeComponent();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width != e.NewSize.Width)
            {
                SetFittingPath(Path);
            }
        }

        private static TextBlock GetCloneToMeasure(TextBlock tbl)
        {
            return new TextBlock()
            {
                Width = tbl.Width,
                Height = tbl.Height,
                MaxWidth = tbl.MaxWidth,
                MaxHeight = tbl.MaxHeight,
                MinWidth = tbl.MinWidth,
                MinHeight = tbl.MinHeight,
                MaxLines = tbl.MaxLines,
                LineHeight = tbl.LineHeight,
                FontSize = tbl.FontSize,
                Margin = tbl.Margin,
                Padding = tbl.Padding,
                TextWrapping = tbl.TextWrapping,
                TextTrimming = tbl.TextTrimming,
                TextReadingOrder = tbl.TextReadingOrder,
                FlowDirection = tbl.FlowDirection,
            };
        }

        private static string GetTrimmedPath(string path, int trimCount)
        {
            if (trimCount <= 0) return path;

            string start = path.Remove((int)Math.Floor((path.Length - trimCount) / 2d));
            string end = path.Substring((int)Math.Floor((path.Length + trimCount) / 2d));

            return $"{start}...{end}";
        }

        private void SetFittingPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                tblPath.Text = string.Empty;
                return;
            }

            TextBlock tbl = GetCloneToMeasure(tblPath);
            int index = 0;
            do
            {
                tbl.Text = GetTrimmedPath(path, index++);
                tbl.Measure(new Size(ActualWidth, double.MaxValue));
            } while (tbl.DesiredSize.Height >= tbl.LineHeight * 3);

            tblPath.Text = tbl.Text;
        }
    }
}
