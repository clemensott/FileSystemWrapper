﻿using FileSystemCommon;
using FileSystemCommonUWP.API;
using FileSystemUWP.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace FileSystemUWP.FileViewers
{
    public sealed partial class FileControl : UserControl
    {
        public event EventHandler<IsFullScreenChangedEventArgs> IsFullScreenChanged;
        public event EventHandler MinimizePlayerClicked;

        public FileSystemItem Source { get; private set; }

        public Api Api { get; set; }

        public FileControl()
        {
            this.InitializeComponent();
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (!Source.Equals(args.NewValue)) PrepareForActivation();
        }

        private async void BtnSelectContentType_Click(object sender, RoutedEventArgs e)
        {
            string mainType, contentType;
            if (rbnMedia.IsChecked == true)
            {
                mainType = "video";
                contentType = tbxContentType.Text;
            }
            else if (rbnImage.IsChecked == true) mainType = contentType = "image";
            else if (rbnText.IsChecked == true) mainType = contentType = "text";
            else return;

            await Activate(mainType, contentType);
        }

        public Task Activate(bool resumePlayback)
        {
            string contentType = Utils.GetContentType(Source.Extension);
            string mainType = contentType.Split('/')[0];

            return Activate(mainType, contentType, resumePlayback);
        }

        private async Task Activate(string mainType, string contentType, bool resumePlayback = false)
        {
            if (resumePlayback && (mainType == "audio" || mainType == "video") && MediaPlayback.Current.Player != null)
            {
                SetContent(new MediaControl(MediaPlayback.Current.Player));
                return;
            }
            if (IsMainTypeImpelmented(mainType)) SetLoading(contentType);
            else
            {
                SetSelect("ContentType is not supported");
                return;
            }

            IRandomAccessStreamWithContentType stream = await Api.GetFileRandomAccessStream(Source.FullPath);

            if (stream == null)
            {
                SetSelect("Loading stream failed");
                bool ping = await Api.IsAuthorized();

                if (!ping) SetSelect("No connection to server");
                return;
            }

            try
            {
                switch (mainType)
                {
                    case "text":
                        TextControl textControl = new TextControl();
                        StreamReader reader = new StreamReader(stream.AsStream());
                        textControl.SetSource(await reader.ReadToEndAsync());
                        SetContent(textControl);
                        break;

                    case "audio":
                    case "video":
                        MediaPlayer player = MediaPlayback.Current.SetSource(stream, Source.Name, contentType);
                        SetContent(new MediaControl(player));
                        break;

                    case "image":
                        ImageControl imageControl = new ImageControl();
                        await imageControl.SetSource(stream);
                        SetContent(imageControl);
                        break;

                    default:
                        SetSelect("ContentType is not supported. Logic error!");
                        break;
                }
            }
            catch (Exception e)
            {
                SetSelect(e.Message);
            }
        }

        public void Deactivate()
        {
            PrepareForActivation();
        }

        private void PrepareForActivation()
        {
            Source = (FileSystemItem)DataContext;
            string contentType = Utils.GetContentType(Source.Extension);
            string mainType = contentType.Split('/')[0];

            main.Children.Clear();
            tblName.Text = Source.Name;

            if (IsMainTypeImpelmented(mainType)) SetLoading(contentType);
            else SetSelect("ContentType is not supported");
        }

        private static bool IsMainTypeImpelmented(string mainType)
        {
            switch (mainType)
            {
                case "video":
                case "audio":
                case "image":
                case "text":
                    return true;

                default:
                    return false;
            }
        }

        private void SetLoading(string contentType)
        {
            tblContentType.Text = contentType;
            prgLoading.IsActive = true;
            splLoading.Visibility = Visibility.Visible;
            splContentTypeSelector.Visibility = Visibility.Collapsed;
        }

        private void SetSelect(string errorMessage)
        {
            tblError.Text = errorMessage;
            prgLoading.IsActive = false;
            splLoading.Visibility = Visibility.Collapsed;
            splContentTypeSelector.Visibility = Visibility.Visible;
        }

        private void SetContent(UIElement content)
        {
            if (main.Children.Count > 0 && main.Children[0] is MediaControl oldMedia)
            {
                oldMedia.IsFullScreenChanged -= Media_IsFullScreenChanged;
                oldMedia.MinimizePlayerClicked -= Media_MinimizePlayerClicked;
            }

            if (content is MediaControl newMedia)
            {
                newMedia.IsFullScreenChanged += Media_IsFullScreenChanged;
                newMedia.MinimizePlayerClicked += Media_MinimizePlayerClicked;
            }

            main.Children.Clear();
            main.Children.Add(content);

            splLoading.Visibility = Visibility.Collapsed;
            prgLoading.IsActive = false;
        }

        private void Media_IsFullScreenChanged(object sender, IsFullScreenChangedEventArgs e)
        {
            IsFullScreenChanged?.Invoke(this, e);
        }

        private void Media_MinimizePlayerClicked(object sender, EventArgs e)
        {
            MinimizePlayerClicked?.Invoke(this, e);
        }
    }
}
