using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using FileSystemUWP.Controls;
using FileSystemUWP.Models;
using StdOttStandard.Linq;
using StdOttUwp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static string GetNamePath(this IEnumerable<PathPart> parts)
        {
            return parts?.Select(p => p.Name).Join("\\") ?? string.Empty;
        }

        public static async Task<bool> TryAgain(string dialogMessage, string dialogTitle, Func<Task<bool>> func,
            BackgroundOperations operations, string operationText)
        {
            while (true)
            {
                Task<bool> task = func();
                operations?.Add(task, operationText);

                if (await task) return true;
                if (!await DialogUtils.ShowTwoOptionsAsync(dialogMessage, dialogTitle, "Yes", "No")) return false;
            }
        }

        public static async Task<bool> TryAgain(string dialogTitle, Func<Task<string>> func,
            BackgroundOperations operations, string operationText)
        {
            while (true)
            {
                Task<string> task = func();
                operations?.Add(task, operationText);

                string errorMessage = await task;
                if (string.IsNullOrWhiteSpace(errorMessage)) return true;
                if (!await DialogUtils.ShowTwoOptionsAsync(errorMessage, dialogTitle, "Yes", "No")) return false;
            }
        }
    }
}
