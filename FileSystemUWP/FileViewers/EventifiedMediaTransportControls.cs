using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FileSystemUWP.FileViewers
{
    class EventifiedMediaTransportControls : MediaTransportControls
    {
        private bool isCastButtonEnabled, isCastButtonVisable,
            isBackToWindowButtonEnabled, isBackToWindowButtonVisable;

        public event EventHandler PlayPauseOnLeftClicked;
        public event EventHandler VolumeMuteClicked;
        public event EventHandler CCSelectionClicked;
        public event EventHandler AudioTracksSelectionClicked;
        public event EventHandler StopClicked;
        public event EventHandler SkipBackwardClicked;
        public event EventHandler PreviousTrackClicked;
        public event EventHandler RewindClicked;
        public event EventHandler PlayPauseClicked;
        public event EventHandler FastForwardClicked;
        public event EventHandler NextTrackClicked;
        public event EventHandler SkipForwardClicked;
        public event EventHandler PlaybackRateClicked;
        public event EventHandler ZoomClicked;
        public event EventHandler CastClicked;
        public event EventHandler FullWindowClicked;
        public event EventHandler MoreClicked;
        public event EventHandler AnyPlayPauseClicked;
        public event EventHandler BackToWindowClicked;

        public bool IsCastButtonEnabled
        {
            get => isCastButtonEnabled;
            set
            {
                isCastButtonEnabled = value;
                if (CastButton != null) CastButton.IsEnabled = value;
            }
        }

        public bool IsCastButtonVisable
        {
            get => isCastButtonVisable;
            set
            {
                isCastButtonVisable = value;
                if (CastButton != null)
                {
                    CastButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public bool IsBackToWindowButtonEnabled
        {
            get => isBackToWindowButtonEnabled;
            set
            {
                isBackToWindowButtonEnabled = value;
                if (BackToWindowButton != null) BackToWindowButton.IsEnabled = value;
            }
        }

        public bool IsBackToWindowButtonVisable
        {
            get => isBackToWindowButtonVisable;
            set
            {
                isBackToWindowButtonVisable = value;
                if (BackToWindowButton != null)
                {
                    BackToWindowButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public AppBarButton PlayPauseButtonOnLeft { get; private set; }

        public AppBarButton VolumeMuteButton { get; private set; }

        public AppBarButton CCSelectionButton { get; private set; }

        public AppBarButton AudioTracksSelectionButton { get; private set; }

        public AppBarButton StopButton { get; private set; }

        public AppBarButton SkipBackwardButton { get; private set; }

        public AppBarButton PreviousTrackButton { get; private set; }

        public AppBarButton RewindButton { get; private set; }

        public AppBarButton PlayPauseButton { get; private set; }

        public AppBarButton FastForwardButton { get; private set; }

        public AppBarButton NextTrackButton { get; private set; }

        public AppBarButton SkipForwardButton { get; private set; }

        public AppBarButton PlaybackRateButton { get; private set; }

        public AppBarButton ZoomButton { get; private set; }

        public AppBarButton CastButton { get; private set; }

        public AppBarButton FullWindowButton { get; private set; }

        public Button MoreButton { get; private set; }

        public AppBarButton BackToWindowButton { get; private set; }

        public EventifiedMediaTransportControls()
        {
            //    Loaded += OnLoaded;
            //}

            //private async void OnLoaded(object sender, RoutedEventArgs e)
            //{
            //    await System.Threading.Tasks.Task.Delay(2000);
            //    var button = FindVisualChild<FrameworkElement>(this);
            //    var array = button.ToArray();
            //    System.Diagnostics.Debug.WriteLine(string.Join("\n", array));
        }

        private static IEnumerable<string> FindVisualChild<childItem>(DependencyObject obj, int depth = 0)
            where childItem : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem cast)
                {
                    string whiteSpaces = "".PadLeft(depth, ' ');
                    string name = string.IsNullOrWhiteSpace(cast.Name) ? "<None>" : cast.Name;
                    yield return $"{whiteSpaces}{name} | {cast.GetType().Name} | {cast.Visibility}";
                }

                {
                    foreach (var item in FindVisualChild<childItem>(child, depth + 1))
                    {
                        yield return item;
                    }
                }
            }
        }

        protected override void OnApplyTemplate()
        {
            PlayPauseButtonOnLeft = (AppBarButton)GetTemplateChild("PlayPauseButtonOnLeft");
            VolumeMuteButton = (AppBarButton)GetTemplateChild("VolumeMuteButton");
            CCSelectionButton = (AppBarButton)GetTemplateChild("CCSelectionButton");
            AudioTracksSelectionButton = (AppBarButton)GetTemplateChild("AudioTracksSelectionButton");
            StopButton = (AppBarButton)GetTemplateChild("StopButton");
            SkipBackwardButton = (AppBarButton)GetTemplateChild("SkipBackwardButton");
            PreviousTrackButton = (AppBarButton)GetTemplateChild("PreviousTrackButton");
            RewindButton = (AppBarButton)GetTemplateChild("RewindButton");
            PlayPauseButton = (AppBarButton)GetTemplateChild("PlayPauseButton");
            FastForwardButton = (AppBarButton)GetTemplateChild("FastForwardButton");
            NextTrackButton = (AppBarButton)GetTemplateChild("NextTrackButton");
            SkipForwardButton = (AppBarButton)GetTemplateChild("SkipForwardButton");
            PlaybackRateButton = (AppBarButton)GetTemplateChild("PlaybackRateButton");
            ZoomButton = (AppBarButton)GetTemplateChild("ZoomButton");
            CastButton = (AppBarButton)GetTemplateChild("CastButton");
            FullWindowButton = (AppBarButton)GetTemplateChild("FullWindowButton");
            MoreButton = (Button)GetTemplateChild("MoreButton");

            if (PlayPauseButtonOnLeft != null) PlayPauseButtonOnLeft.Click += PlayPauseButtonOnLeft_Click;
            if (VolumeMuteButton != null) VolumeMuteButton.Click += VolumeMuteButton_Click;
            if (CCSelectionButton != null) CCSelectionButton.Click += CCSelectionButton_Click;
            if (AudioTracksSelectionButton != null) AudioTracksSelectionButton.Click += AudioTracksSelectionButton_Click;
            if (StopButton != null) StopButton.Click += StopButton_Click;
            if (SkipBackwardButton != null) SkipBackwardButton.Click += SkipBackwardButton_Click;
            if (PreviousTrackButton != null) PreviousTrackButton.Click += PreviousTrackButton_Click;
            if (RewindButton != null) RewindButton.Click += RewindButton_Click;
            if (PlayPauseButton != null) PlayPauseButton.Click += PlayPauseButton_Click;
            if (FastForwardButton != null) FastForwardButton.Click += FastForwardButton_Click;
            if (NextTrackButton != null) NextTrackButton.Click += NextTrackButton_Click;
            if (SkipForwardButton != null) SkipForwardButton.Click += SkipForwardButton_Click;
            if (PlaybackRateButton != null) PlaybackRateButton.Click += PlaybackRateButton_Click;
            if (ZoomButton != null) ZoomButton.Click += ZoomButton_Click;
            if (CastButton != null)
            {
                CastButton.Click += CastButton_Click;
                CastButton.Loaded += CastButton_Loaded;
                CastButton.LayoutUpdated += CastButton_LayoutUpdated;
            }
            if (FullWindowButton != null) FullWindowButton.Click += FullWindowButton_Click;
            if (MoreButton != null) MoreButton.Click += MoreButton_Click;

            base.OnApplyTemplate();
        }

        private void CreateAdditionalElements()
        {
            if (BackToWindowButton == null && PlayPauseButtonOnLeft != null && CastButton != null)
            {
                BackToWindowButton = InsertAppBarButtonButtonBefore(Symbol.BackToWindow,
                    "BackToWindowButton", CastButton, PlayPauseButtonOnLeft);
                if (BackToWindowButton != null)
                {
                    BackToWindowButton.IsEnabled = IsBackToWindowButtonEnabled;
                    BackToWindowButton.Visibility = IsBackToWindowButtonVisable ? Visibility.Visible : Visibility.Collapsed;
                    BackToWindowButton.Click += BackToWindowButton_Click;
                }
            }
        }

        private AppBarButton InsertAppBarButtonButtonBefore(Symbol symbol, string name, UIElement before, AppBarButton template)
        {
            Panel panel = (Panel)FindParentOf(this, before);
            if (panel == null) return null;

            AppBarButton button = new AppBarButton()
            {
                Icon = new SymbolIcon(symbol),
                Name = name,
                Width = template.Width,
                Height = template.Height,
                Background = new SolidColorBrush(((SolidColorBrush)template.Background).Color),
                BorderBrush = new SolidColorBrush(((SolidColorBrush)template.BorderBrush).Color),
                HorizontalAlignment = template.HorizontalAlignment,
                VerticalAlignment = template.VerticalAlignment,
                IsCompact = template.IsCompact,
                IsTextScaleFactorEnabled = template.IsTextScaleFactorEnabled,
                UseSystemFocusVisuals = template.UseSystemFocusVisuals,
            };
            panel.Children.Insert(panel.Children.IndexOf(before), button);

            return button;
        }

        private static DependencyObject FindParentOf(DependencyObject obj, DependencyObject element)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child == element)
                {
                    return obj;
                }

                var parent = FindParentOf(child, element);
                if (parent != null) return parent;
            }

            return null;
        }

        private void PlayPauseButtonOnLeft_Click(object sender, object e)
        {
            PlayPauseOnLeftClicked?.Invoke(this, EventArgs.Empty);
            AnyPlayPauseClicked?.Invoke(this, EventArgs.Empty);
        }

        private void VolumeMuteButton_Click(object sender, object e)
        {
            VolumeMuteClicked?.Invoke(this, EventArgs.Empty);
        }

        private void CCSelectionButton_Click(object sender, object e)
        {
            CCSelectionClicked?.Invoke(this, EventArgs.Empty);
        }

        private void AudioTracksSelectionButton_Click(object sender, object e)
        {
            AudioTracksSelectionClicked?.Invoke(this, EventArgs.Empty);
        }

        private void StopButton_Click(object sender, object e)
        {
            StopClicked?.Invoke(this, EventArgs.Empty);
        }

        private void SkipBackwardButton_Click(object sender, object e)
        {
            SkipBackwardClicked?.Invoke(this, EventArgs.Empty);
        }

        private void PreviousTrackButton_Click(object sender, object e)
        {
            PreviousTrackClicked?.Invoke(this, EventArgs.Empty);
        }

        private void RewindButton_Click(object sender, object e)
        {
            RewindClicked?.Invoke(this, EventArgs.Empty);
        }

        private void PlayPauseButton_Click(object sender, object e)
        {
            PlayPauseClicked?.Invoke(this, EventArgs.Empty);
            AnyPlayPauseClicked?.Invoke(this, EventArgs.Empty);
        }

        private void FastForwardButton_Click(object sender, object e)
        {
            FastForwardClicked?.Invoke(this, EventArgs.Empty);
        }

        private void NextTrackButton_Click(object sender, object e)
        {
            NextTrackClicked?.Invoke(this, EventArgs.Empty);
        }

        private void SkipForwardButton_Click(object sender, object e)
        {
            SkipForwardClicked?.Invoke(this, EventArgs.Empty);
        }

        private void PlaybackRateButton_Click(object sender, object e)
        {
            PlaybackRateClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ZoomButton_Click(object sender, object e)
        {
            ZoomClicked?.Invoke(this, EventArgs.Empty);
        }

        private void CastButton_Click(object sender, object e)
        {
            CastClicked?.Invoke(this, EventArgs.Empty);
        }

        private void CastButton_Loaded(object sender, RoutedEventArgs e)
        {
            CastButton.IsEnabled = IsCastButtonEnabled;
            CastButton.Visibility = IsCastButtonVisable ? Visibility.Visible : Visibility.Collapsed;

            CreateAdditionalElements();
        }

        private void CastButton_LayoutUpdated(object sender, object e)
        {
            if (IsCastButtonVisable && CastButton.Visibility != Visibility.Visible)
            {
                CastButton.Visibility = Visibility.Visible;
            }
            else if (!IsCastButtonVisable && CastButton.Visibility != Visibility.Collapsed)
            {
                CastButton.Visibility = Visibility.Collapsed;
            }

            CreateAdditionalElements();
        }

        private void FullWindowButton_Click(object sender, object e)
        {
            FullWindowClicked?.Invoke(this, EventArgs.Empty);
        }

        private void MoreButton_Click(object sender, object e)
        {
            MoreClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BackToWindowButton_Click(object sender, RoutedEventArgs e)
        {
            BackToWindowClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
