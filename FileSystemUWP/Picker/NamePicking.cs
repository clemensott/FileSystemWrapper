using StdOttStandard.AsyncResult;

namespace FileSystemUWP.Picker
{
    class NamePicking : AsyncResult<string>
    {
        public string FolderPath { get; }

        public string Suggestion { get; }

        public ConflictType FolderConfictType { get; }

        public ConflictType FileConfilctType { get; }

        public Api Api { get; }

        public NamePicking(string folderPath, string suggestion, ConflictType folderConfictType,
            ConflictType fileConfilctType, Api api)
        {
            FolderPath = folderPath;
            Suggestion = suggestion;
            FolderConfictType = folderConfictType;
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
