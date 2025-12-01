using System.Collections;

namespace FileSystemCLI.Models;

public class SyncPairState: IEnumerable<SyncPairStateFileModel>
{
    private readonly IDictionary<string, SyncPairStateFileModel> files;

    public SyncPairState()
    {
        files = new Dictionary<string, SyncPairStateFileModel>();
    }

    public SyncPairState(IEnumerable<SyncPairStateFileModel> files)
    {
        this.files = files.ToDictionary(f => f.RelativePath);
    }

    public bool ContainsFile(string relativePath)
    {
        return files.ContainsKey(relativePath);
    }

    public SyncPairStateFileModel GetFile(string relativePath)
    {
        return files[relativePath];
    }

    public bool TryGetFile(string relativePath, out SyncPairStateFileModel file)
    {
        return files.TryGetValue(relativePath, out file);
    }

    public void AddFile(SyncPairStateFileModel file)
    {
        files.Add(file.RelativePath, file);
    }

    public IEnumerator<SyncPairStateFileModel> GetEnumerator()
    {
        return files.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}