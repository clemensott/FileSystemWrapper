using FileSystemCommon;
using FileSystemCommon.Models.Auth;
using FileSystemCommon.Models.Configuration;
using FileSystemCommon.Models.FileSystem.Content;
using FileSystemCommon.Models.FileSystem.Files;
using FileSystemCommon.Models.FileSystem.Folders;
using Newtonsoft.Json;
using StdOttStandard.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace FileSystemUWP.API
{
    public class Api
    {
        public string Name { get; set; }

        public string BaseUrl { get; set; }

        public string Username { get; set; }

        public string[] RawCookies { get; set; }

        public Config Config { get; private set; }

        public Api()
        {
            Config = new Config()
            {
                DirectorySeparatorChar = '/',
                AltDirectorySeparatorChar = '/',
            };
        }

        internal Api Clone()
        {
            return new Api()
            {
                Name = Name,
                BaseUrl = BaseUrl,
                Username = Username,
                RawCookies = RawCookies?.ToArray(),
                Config = new Config()
                {
                    DirectorySeparatorChar = Config?.DirectorySeparatorChar ?? '/',
                    AltDirectorySeparatorChar = Config?.AltDirectorySeparatorChar ?? '/',
                },
            };
        }

        public Task<bool> Ping()
        {
            return Request(GetUri("/api/ping"), HttpMethod.Get);
        }

        public Task<bool> IsAuthorized()
        {
            return Request(GetUri("/api/ping/auth"), HttpMethod.Get);
        }

        public async Task<bool> Login(LoginBody body)
        {
            Uri uri = GetUri("/api/auth/login");
            if (uri == null) return false;

            try
            {
                using (HttpClient client = GetClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri))
                    {
                        request.Content = new HttpStringContent(JsonConvert.SerializeObject(body), UnicodeEncoding.Utf8, "application/json");

                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            if (!response.IsSuccessStatusCode) return false;

                            RawCookies = response.Headers.Where(p => p.Key.ToLower() == "set-cookie").Select(p => p.Value).ToArray();
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> LoadConfig()
        {
            Uri uri = GetUri("/api/config");
            Config = await Request<Config>(uri, HttpMethod.Get);
            return Config != null;
        }

        public Task<bool> FolderExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return Task.FromResult(false);

            Uri uri = GetUri("/api/folders/exists", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            return Request<bool>(uri, HttpMethod.Get);
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
            Uri uri = GetUri("/api/folders/content", values);
            return Request<FolderContent>(uri, HttpMethod.Get);
        }

        public Task<FolderItemInfo> GetFolderInfo(string path)
        {
            Uri uri = GetUri("/api/folders/info", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            return Request<FolderItemInfo>(uri, HttpMethod.Get);
        }

        public Task<FolderItemInfoWithSize> GetFolderInfoWithSize(string path)
        {
            Uri uri = GetUri("/api/folders/infoWithSize", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            return Request<FolderItemInfoWithSize>(uri, HttpMethod.Get);
        }

        public Task<bool> CreateFolder(string path)
        {
            Uri uri = GetUri("/api/folders", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            return Request(uri, HttpMethod.Post);
        }

        public Task<bool> DeleteFolder(string path, bool recrusive)
        {
            IEnumerable<KeyValuePair<string, string>> values = KeyValuePairsUtils
                .CreatePairs("path", path, "recrusive", recrusive.ToString());
            return Request(GetUri("/api/folders", values), HttpMethod.Delete);
        }

        public Task<bool> FileExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return Task.FromResult(false);

            Uri uri = GetUri("/api/files/exists", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            return Request<bool>(uri, HttpMethod.Get);
        }

        public Task<FileItemInfo> GetFileInfo(string path)
        {
            Uri uri = GetUri("/api/files/info", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            return Request<FileItemInfo>(uri, HttpMethod.Get);
        }

        public Task<string> GetFileHash(string path)
        {
            Uri uri = GetUri("/api/files/hash", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            return RequestString(uri, HttpMethod.Get);
        }

        public Task<bool> CopyFile(string srcPath, string destPath)
        {
            IEnumerable<KeyValuePair<string, string>> values = KeyValuePairsUtils
                .CreatePairs("srcPath", Utils.EncodePath(srcPath), "destPath", Utils.EncodePath(srcPath));
            return Request(GetUri("/api/files/copy", values), HttpMethod.Post);
        }

        public Task<bool> MoveFile(string srcPath, string destPath)
        {
            IEnumerable<KeyValuePair<string, string>> values = KeyValuePairsUtils
                .CreatePairs("srcPath", Utils.EncodePath(srcPath), "destPath", Utils.EncodePath(srcPath));
            return Request(GetUri("/api/files/move", values), HttpMethod.Post);
        }

        public async Task<bool> UploadFile(string path, IInputStream stream)
        {
            Uri uri = GetUri("/api/files", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            using (HttpMultipartFormDataContent content = new HttpMultipartFormDataContent())
            {
                content.Add(new HttpStreamContent(stream), "FileContent", "file");
                return await Request(uri, HttpMethod.Post, content);
            }
        }

        public Task<bool> DeleteFile(string path)
        {
            Uri uri = GetUri("/api/files", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            return Request(uri, HttpMethod.Delete);
        }

        public Task<IRandomAccessStreamWithContentType> GetFileRandomAccessStream(string path)
        {
            Uri uri = GetUri("/api/files", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            return RequestRandmomAccessStream(uri, HttpMethod.Get);
        }

        public async Task DownloadFile(string path, StorageFile destFile)
        {
            Uri uri = GetUri("/api/files/download", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            if (uri == null) return;

            using (IRandomAccessStream fileStream = await destFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (HttpClient client = GetClient())
                {
                    using (IInputStream downloadStream = await client.GetInputStreamAsync(uri))
                    {
                        const uint capacity = 1_000_000;
                        Windows.Storage.Streams.Buffer buffer = new Windows.Storage.Streams.Buffer(capacity);
                        while (true)
                        {
                            await downloadStream.ReadAsync(buffer, capacity, InputStreamOptions.None);
                            if (buffer.Length == 0) break;
                            await fileStream.WriteAsync(buffer);
                        }
                    }
                }
            }
        }

        public Task<string> StartBigFileUpload(string path)
        {
            Uri uri = GetUri("/api/bigFile/start", KeyValuePairsUtils.CreatePairs("path", Utils.EncodePath(path)));
            return RequestString(uri, HttpMethod.Post);
        }

        public async Task<bool> AppendBigFileUpload(string uuid, IBuffer buffer)
        {
            Uri uri = GetUri($"/api/bigFile/{uuid}/append");
            using (HttpMultipartFormDataContent content = new HttpMultipartFormDataContent())
            {
                content.Add(new HttpBufferContent(buffer), "PartialFile", "file.part");
                return await Request(uri, HttpMethod.Post, content);
            }
        }

        public Task<bool> FinishBigFileUpload(string uuid)
        {
            Uri uri = GetUri($"/api/bigFile/{uuid}/finsih");
            return Request(uri, HttpMethod.Put);
        }

        public Task<bool> CancelBigFileUpload(string uuid)
        {
            Uri uri = GetUri($"/api/bigFile/{uuid}");
            return Request(uri, HttpMethod.Delete);
        }

        private async Task<TData> Request<TData>(Uri uri, HttpMethod method)
        {
            string responseText;
            if (uri == null) return default(TData);

            try
            {
                using (HttpClient client = GetClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(method, uri))
                    {
                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            if (!response.IsSuccessStatusCode ||
                                response.Content.Headers.ContentType.MediaType != "application/json") return default(TData);

                            responseText = await response.Content.ReadAsStringAsync();
                        }
                    }
                }

                return JsonConvert.DeserializeObject<TData>(responseText);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return default(TData);
            }
        }

        private async Task<string> RequestString(Uri uri, HttpMethod method)
        {
            if (uri == null) return null;

            try
            {
                using (HttpClient client = GetClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(method, uri))
                    {
                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            if (!response.IsSuccessStatusCode) return null;

                            return await response.Content.ReadAsStringAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        private async Task<IRandomAccessStreamWithContentType> RequestRandmomAccessStream(Uri uri, HttpMethod method)
        {
            if (uri == null) return null;

            try
            {
                return await HttpRandomAccessStream.CreateAsync(GetClient(), uri);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        private async Task<bool> Request(Uri uri, HttpMethod method, IHttpContent content = null)
        {
            if (uri == null) return false;

            try
            {
                using (HttpClient client = GetClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(method, uri))
                    {
                        if (content != null) request.Content = content;

                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            return response.IsSuccessStatusCode;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }
        }

        public Uri GetUri(string resource, IEnumerable<KeyValuePair<string, string>> values = null)
        {
            if (string.IsNullOrWhiteSpace(BaseUrl)) return null;

            IEnumerable<string> queryPairs = values?.Select(p => string.Format("{0}={1}", p.Key, WebUtility.UrlEncode(p.Value)));
            string query = string.Join("&", queryPairs ?? Enumerable.Empty<string>());
            string url = string.Format("{0}/{1}?{2}", BaseUrl?.TrimEnd('/'), resource?.TrimStart('/'), query);

            try
            {
                return new Uri(url);
            }
            catch
            {
                return null;
            }
        }

        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient(GetFilter());
            return client;
        }

        private HttpBaseProtocolFilter GetFilter()
        {
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);

            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
            filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;

            //if (RawCookies != null && RawCookies.Length > 0)
            //{
            //    foreach (string rawCookie in RawCookies)
            //    {
            //        HttpCookie cookie = ParseCookie(rawCookie, BaseUrl);
            //        if (cookie != null) filter.CookieManager.SetCookie(cookie);
            //    }
            //}

            return filter;
        }

        private HttpCookie[] GetCookiesOfWebsite(string domain)
        {
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            return filter.CookieManager.GetCookies(new Uri(domain)).ToArray();
        }

        private static HttpCookie ParseCookie(string raw, string domain)
        {
            bool? secure = null, httpOnly = null;
            string cookieName = null, cookieValue = null,
                rawMaxAge = null, rawExpires = null, path = "/", samesite;

            foreach (string pair in raw.Split(';').Select(p => p.Trim()))
            {
                string key, value;

                int index = pair.IndexOf('=');
                if (index == -1)
                {
                    key = pair;
                    value = null;
                }
                else
                {
                    key = pair.Remove(index);
                    value = pair.Substring(index + 1);
                }

                switch (key.ToLower())
                {
                    case "expires":
                        rawExpires = value;
                        break;

                    case "max-age":
                        rawMaxAge = value;
                        break;

                    case "domain":
                        domain = value;
                        break;

                    case "path":
                        path = value;
                        break;

                    case "secure":
                        secure = true;
                        break;

                    case "httponly":
                        httpOnly = true;
                        break;

                    case "samesite":
                        samesite = value;
                        break;

                    default:
                        if (cookieName == null)
                        {
                            cookieName = key;
                            cookieValue = value;
                        }
                        break;
                }
            }

            DateTime expires;
            if (DateTime.TryParse(rawExpires, out expires) && expires < DateTime.Now) return null;

            try
            {
                HttpCookie cookie = new HttpCookie(cookieName, domain, path);
                cookie.Value = cookieValue;

                if (secure.HasValue) cookie.Secure = secure.Value;
                if (httpOnly.HasValue) cookie.HttpOnly = httpOnly.Value;

                return cookie;
            }
            catch
            {
                return null;
            }
        }
    }
}
