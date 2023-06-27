using FileSystemCommonUWP.API;
using System.Threading.Tasks;

namespace FileSystemUWP.Picker
{
    class NamePicking : TaskCompletionSource<string>
    {
        public string FolderPath { get; }

        public string Suggestion { get; }

        public ConflictType FolderConflictType { get; }

        public ConflictType FileConfilctType { get; }

        public Api Api { get; }

        public NamePicking(string folderPath, string suggestion, ConflictType folderConflictType,
            ConflictType fileConfilctType, Api api)
        {
            FolderPath = folderPath;
            Suggestion = suggestion;
            FolderConflictType = folderConflictType;
            FileConfilctType = fileConfilctType;
            Api = api;
        }

        public static NamePicking ForFile(Api api, string folderPath, string suggestion = null)
        {
            return new NamePicking(folderPath, suggestion, ConflictType.Error, ConflictType.Warning, api);
        }

        public static NamePicking ForFolder(Api api, string folderPath, string suggestion = null)
        {
            return new NamePicking(folderPath, suggestion, ConflictType.Error, ConflictType.Warning, api);
        }
    }
}
