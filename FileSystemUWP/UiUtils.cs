using FileSystemCommon;
using Windows.UI.Xaml.Controls;

namespace FileSystemUWP
{
    static class UiUtils
    {
        public static Symbol GetSymbol(this FileSystemItem item)
        {
            if (item.IsFolder) return Symbol.Folder;

            string contentType = Utils.GetContentType(item.Extension);
            switch (contentType.Split('/')[0])
            {
                case "text":
                    return Symbol.Document;

                case "audio":
                    return Symbol.Audio;

                case "video":
                    return Symbol.SlideShow;

                case "image":
                    return Symbol.Pictures;

                default:
                    return Symbol.Page;
            }
        }
    }
}
