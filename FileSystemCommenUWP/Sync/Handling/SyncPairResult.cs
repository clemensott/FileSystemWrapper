using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FileSystemCommonUWP.Sync.Handling
{
    public class SyncPairResult: IEnumerable<SyncPairResultFile>
    {
        private readonly IDictionary<string, SyncPairResultFile> files;

        public int Id { get; set; }

        public SyncPairResult()
        {
            files = new Dictionary<string, SyncPairResultFile>();
        }

        public SyncPairResult(IEnumerable<SyncPairResultFile> files)
        {
            this.files = files.ToDictionary(f => f.RelativePath);
        }

        public bool ContainsFile(string relativePath)
        {
            return files.ContainsKey(relativePath);
        }

        public SyncPairResultFile GetFile(string relativePath)
        {
            return files[relativePath];
        }

        public bool TryGetFile(string relativePath, out SyncPairResultFile file)
        {
            return files.TryGetValue(relativePath, out file);
        }

        public void AddFile(SyncPairResultFile file)
        {
            files.Add(file.RelativePath, file);
        }

        public IEnumerator<SyncPairResultFile> GetEnumerator()
        {
            return files.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
