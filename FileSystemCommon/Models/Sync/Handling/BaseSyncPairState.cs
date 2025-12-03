using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FileSystemCommon.Models.Sync.Handling
{
    public class BaseSyncPairState<TSyncPairFile> : IEnumerable<TSyncPairFile> where TSyncPairFile : IBaseSyncPairStateFile
    {
        private readonly IDictionary<string, TSyncPairFile> files;

        public BaseSyncPairState()
        {
            files = new Dictionary<string, TSyncPairFile>();
        }

        public BaseSyncPairState(IEnumerable<TSyncPairFile> files)
        {
            this.files = files.ToDictionary(f => f.RelativePath);
        }

        public bool ContainsFile(string relativePath)
        {
            return files.ContainsKey(relativePath);
        }

        public TSyncPairFile GetFile(string relativePath)
        {
            return files[relativePath];
        }

        public bool TryGetFile(string relativePath, out TSyncPairFile file)
        {
            return files.TryGetValue(relativePath, out file);
        }

        public void AddFile(TSyncPairFile file)
        {
            files.Add(file.RelativePath, file);
        }

        public IEnumerator<TSyncPairFile> GetEnumerator()
        {
            return files.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}