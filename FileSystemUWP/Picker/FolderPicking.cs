using FileSystemCommonUWP.API;
using System.Threading.Tasks;

namespace FileSystemUWP.Picker
{
    class FolderPicking : TaskCompletionSource<string>
    {
        public Api Api { get; }

        public string SuggestedStartLocation { get; set; }

        public string SuggestedFileName { get; }

        public FileSystemPickType Type { get; }

        public FolderPicking(Api api, string suggestedStartLocation, string suggestedFileName, FileSystemPickType type)
        {
            Api = api;
            SuggestedStartLocation = suggestedStartLocation;
            SuggestedFileName = suggestedFileName;
            Type = type;
        }

        public static FolderPicking FileOpen(Api api, string suggestedStartLocation = null)
        {
            return new FolderPicking(api, suggestedStartLocation, null, FileSystemPickType.FileOpen);
        }

        public static FolderPicking FileSave(Api api, string suggestedStartLocation = null, string suggestedFileName = null)
        {
            return new FolderPicking(api, suggestedStartLocation, suggestedFileName, FileSystemPickType.FileSave);
        }

        public static FolderPicking Folder(Api api, string suggestedStartLocation = null)
        {
            return new FolderPicking(api, suggestedStartLocation, null, FileSystemPickType.Folder);
        }
    }
}
