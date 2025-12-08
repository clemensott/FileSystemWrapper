using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FileSystemCommon;
using FileSystemCommon.Models.Auth;
using FileSystemCommon.Models.Configuration;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Files.Change;
using FileSystemCommon.Models.FileSystem.Files.Many;
using FileSystemCommon.Models.FileSystem.Folders.Change;
using StdOttStandard.Linq;

namespace FileSystemCLI.Services;

public class Api : IDisposable
{
    public string BaseUrl { get; }

    public string Username { get; }

    public string Password { get; }

    public HttpClient Client { get; }

    public Config Config { get; private set; }

    public Api(string baseUrl, string username, string password)
    {
        BaseUrl = baseUrl;
        Username = username;
        Password = password;

        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

        Client = new HttpClient(clientHandler)
        {
            BaseAddress = new Uri(BaseUrl),
            DefaultRequestHeaders =
            {
                CacheControl = new CacheControlHeaderValue()
                {
                    NoCache = true,
                }
            },
        };
    }

    public Task<bool> Ping()
    {
        return Request(Utils.GetUri("/api/ping"), HttpMethod.Get);
    }

    public Task<bool> IsAuthorized()
    {
        return Request(Utils.GetUri("/api/ping/auth"), HttpMethod.Get);
    }

    public async Task<bool> Login()
    {
        Uri? uri = Utils.GetUri("/api/auth/login");
        if (uri == null) return false;

        LoginBody body = new LoginBody()
        {
            Username = Username,
            Password = Password,
        };

        try
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            string jsonBody = JsonSerializer.Serialize(body);
            StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await Client.PostAsync(uri, content);
            if (!response.IsSuccessStatusCode) return false;

            string[] rawCookies = response.Headers.Where(p => p.Key.ToLower() == "set-cookie").SelectMany(p => p.Value)
                .ToArray();
            foreach (string rawCookie in rawCookies)
            {
                Client.DefaultRequestHeaders.Add("Cookie", rawCookie);
            }

            return true;
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e);
            return false;
        }
    }

    public async Task LoadConfig()
    {
        Uri? uri = Utils.GetUri("/api/config");
        Config = await Request<Config>(uri, HttpMethod.Get);
    }

    public Task<FolderContent> FolderContent(string path,
        FileSystemItemSortType? sortType = null, FileSystemItemSortDirection? sortDirection = null)
    {
        List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
        values.Add(new KeyValuePair<string, string>("path", Utils.EncodePath(path)));
        if (sortType.HasValue)
        {
            values.Add(new KeyValuePair<string, string>("sortType", sortType.Value.ToString()));
        }

        if (sortDirection.HasValue)
        {
            values.Add(new KeyValuePair<string, string>("sortDirection", sortDirection.Value.ToString()));
        }

        Uri? uri = Utils.GetUri("/api/folders/content", values);
        return Request<FolderContent>(uri, HttpMethod.Get);
    }

    public Task<bool> FolderExists(string serverFolderPath)
    {
        if (string.IsNullOrWhiteSpace(serverFolderPath)) return Task.FromResult(false);

        Uri uri = Utils.GetUri("/api/folders/exists",
            KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(serverFolderPath)));
        return Request<bool>(uri, HttpMethod.Get);
    }

    public Task<bool> FileExists(string serverFilePath)
    {
        if (string.IsNullOrWhiteSpace(serverFilePath)) return Task.FromResult(false);

        Uri? uri = Utils.GetUri("/api/files/exists",
            KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(serverFilePath)));
        return Request<bool>(uri, HttpMethod.Get);
    }

    public async Task GetFilesExits(string[] paths, Func<FileExistsManyItem, Task> onFileExistsFunc)
    {
        FilesExistsManyBody body = new FilesExistsManyBody()
        {
            Paths = paths,
        };

        Uri? uri = Utils.GetUri("/api/files/existsMany");
        string jsonBody = JsonSerializer.Serialize(body);
        using HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        await RequestMany(uri, HttpMethod.Post, content, onFileExistsFunc);
    }

    public Task<FileItemInfo> GetFileInfo(string path)
    {
        Uri? uri = Utils.GetUri("/api/files/info", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
        return Request<FileItemInfo>(uri, HttpMethod.Get);
    }

    public async Task GetFilesInfo(string[] paths, Func<FileInfoManyItem, Task> onFileInfoFunc)
    {
        FilesInfoManyBody body = new FilesInfoManyBody()
        {
            Paths = paths,
        };

        Uri? uri = Utils.GetUri("/api/files/infoMany");
        string jsonBody = JsonSerializer.Serialize(body);
        using HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        await RequestMany(uri, HttpMethod.Post, content, onFileInfoFunc);
    }

    public Task<string> GetFileHash(string path, int? partialSize = null)
    {
        List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
        values.Add(new KeyValuePair<string, string>("path", Utils.EncodePath(path)));

        if (partialSize.HasValue)
        {
            values.Add(new KeyValuePair<string, string>("partialSize", partialSize.Value.ToString()));
        }

        Uri? uri = Utils.GetUri("/api/files/hash", values);
        return RequestString(uri, HttpMethod.Get);
    }

    public async Task GetFilesHash(string[] paths, int? partialSize, Func<FileHashManyItem, Task> onFileHashFunc)
    {
        FilesHashManyBody body = new FilesHashManyBody()
        {
            Paths = paths,
            PartialSize = partialSize,
        };

        Uri? uri = Utils.GetUri("/api/files/hashMany");
        string jsonBody = JsonSerializer.Serialize(body);
        using HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        await RequestMany(uri, HttpMethod.Post, content, onFileHashFunc);
    }

    public async Task<bool> UploadFile(string path, Stream stream)
    {
        Uri uri = Utils.GetUri("/api/files", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
        using MultipartFormDataContent content = new MultipartFormDataContent();

        content.Add(new StreamContent(stream), "FileContent", "file");
        return await Request(uri, HttpMethod.Post, content);
    }

    public Task<bool> DeleteFile(string path)
    {
        Uri? uri = Utils.GetUri("/api/files", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
        return Request(uri, HttpMethod.Delete);
    }

    public async Task DownloadFile(string serverFilePath, string localFilePath)
    {
        Uri? uri = Utils.GetUri("/api/files/download",
            KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(serverFilePath)));
        if (uri == null) return;

        using var writeStream = File.OpenWrite(localFilePath);
        using var readStream = await Client.GetStreamAsync(uri);

        await readStream.CopyToAsync(writeStream);
    }

    public Task<string> StartBigFileUpload(string path)
    {
        Uri uri = Utils.GetUri("/api/bigFile/start", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
        return RequestString(uri, HttpMethod.Post);
    }

    public async Task<bool> AppendBigFileUpload(string uuid, Stream stream)
    {
        Uri? uri = Utils.GetUri($"/api/bigFile/{uuid}/append");
        using MultipartFormDataContent content = new MultipartFormDataContent();

        content.Add(new StreamContent(stream), "PartialFile", "file.part");
        return await Request(uri, HttpMethod.Post, content);
    }

    public Task<bool> FinishBigFileUpload(string uuid)
    {
        Uri? uri = Utils.GetUri($"/api/bigFile/{uuid}/finish");
        return Request(uri, HttpMethod.Put);
    }

    public Task<bool> CancelBigFileUpload(string uuid)
    {
        Uri? uri = Utils.GetUri($"/api/bigFile/{uuid}");
        return Request(uri, HttpMethod.Delete);
    }

    public Task<bool> CreateFolder(string serverFolderPath)
    {
        Uri? uri = Utils.GetUri("/api/folders",
            KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(serverFolderPath)));
        return Request(uri, HttpMethod.Post);
    }

    public Task<FileChangeResult> GetFileChanges(string serverFolderPath, DateTime since, int page, int pageSize)
    {
        Uri? uri = Utils.GetUri("/api/folders/fileChanges",
            KeyValuePairsUtils.CreatePairs(
                "path", Utils.EncodePath(serverFolderPath),
                "since", since.ToString("yyyy-MM-dd HH:mm:ss"),
                "page", page.ToString(),
                "pageSize", pageSize.ToString()
            ));
        
        return Request<FileChangeResult>(uri, HttpMethod.Get);
    }

    public Task<FolderChangeResult> GetFolderChanges(string serverFolderPath, DateTime since, int page, int pageSize)
    {
        Uri? uri = Utils.GetUri("/api/folders/folderChanges",
            KeyValuePairsUtils.CreatePairs(
                "path", Utils.EncodePath(serverFolderPath),
                "since", since.ToString("yyyy-MM-dd HH:mm:ss"),
                "page", page.ToString(),
                "pageSize", pageSize.ToString()
            ));
        
        return Request<FolderChangeResult>(uri, HttpMethod.Get);
    }

    private async Task<bool> Request(Uri? uri, HttpMethod method, HttpContent? content = null)
    {
        if (uri == null) throw new Exception("No uri");

        using HttpRequestMessage request = new HttpRequestMessage(method, uri);
        if (content != null) request.Content = content;

        using HttpResponseMessage response = await Client.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    private async Task<TData> Request<TData>(Uri? uri, HttpMethod method)
    {
        string responseText;
        if (uri == null) throw new Exception("No uri");

        using (HttpRequestMessage request = new HttpRequestMessage(method, uri))
        {
            using (HttpResponseMessage response = await Client.SendAsync(request))
            {
                if (!response.IsSuccessStatusCode) throw new Exception("No success status code");
                if (response.Content.Headers.ContentType?.MediaType != "application/json")
                    throw new Exception("No json response");

                responseText = await response.Content.ReadAsStringAsync();
            }
        }

        TData? result = JsonSerializer.Deserialize<TData>(responseText, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        return result ?? throw new Exception("No json response");
    }

    private async Task<string> RequestString(Uri? uri, HttpMethod method)
    {
        if (uri == null) throw new Exception("No uri");

        using HttpRequestMessage request = new HttpRequestMessage(method, uri);
        using HttpResponseMessage response = await Client.SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new Exception("No success status code");

        return await response.Content.ReadAsStringAsync();
    }

    private async Task RequestMany<TItem>(Uri uri, HttpMethod method, HttpContent? content,
        Func<TItem, Task> onItemFunc)
    {
        string responseText = string.Empty;

        using HttpRequestMessage request = new HttpRequestMessage(method, uri);
        if (content != null) request.Content = content;

        using HttpResponseMessage response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode) throw new Exception("No success status code");

        Stream stream = await response.Content.ReadAsStreamAsync();

        const int capacity = 1000;
        byte[] buffer = new byte[capacity];
        while (true)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, capacity);
            if (bytesRead == 0) break;

            byte[] effectiveBuffer = bytesRead == buffer.Length ? buffer : buffer.Take(bytesRead).ToArray();
            responseText += Encoding.UTF8.GetString(effectiveBuffer);
            responseText = await TryParseResponse(responseText);
        }

        async Task<string> TryParseResponse(string text)
        {
            int endIndex = -1;

            while (endIndex + 1 < text.Length)
            {
                endIndex = text.IndexOf('}', endIndex + 1);
                if (endIndex == -1) break;

                string json = text.Substring(0, endIndex + 1);

                TItem? item;
                try
                {
                    item = JsonSerializer.Deserialize<TItem>(json);
                    text = text.Substring(json.Length);
                    endIndex = -1;

                    if (item is not null) await onItemFunc(item);
                    else Console.WriteLine($"No item response: {json}");
                }
                catch
                {
                }
            }

            return text;
        }
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}