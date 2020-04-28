using FileSystemCommon.Model;
using Newtonsoft.Json;
using StdOttStandard.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace FileSystemUWP
{
    public class Api
    {
        public string Password { get; set; }

        public string BaseUrl { get; set; }

        public Api()
        {
        }

        public Api(string password, string baseUrl) : this()
        {
            Password = password;
            BaseUrl = baseUrl;
        }

        public Task<bool> Ping()
        {
            return Request(GetUri("/api/ping"), HttpMethod.Get);
        }

        public Task<bool> FolderExists(string path)
        {
            return Request<bool>(GetUriWithPath("/api/folders/exists", path), HttpMethod.Get);
        }

        public Task<List<string>> ListFolders(string path)
        {
            return Request<List<string>>(GetUriWithPath("/api/folders/listfolders", path), HttpMethod.Get);
        }

        public Task<List<string>> ListFiles(string path)
        {
            return Request<List<string>>(GetUriWithPath("/api/folders/listfiles", path), HttpMethod.Get);
        }

        public Task<FolderItemInfo> GetFolderInfo(string path)
        {
            return Request<FolderItemInfo>(GetUriWithPath("/api/folders/info", path), HttpMethod.Get);
        }

        public Task<bool> CreateFolder(string path)
        {
            return Request(GetUriWithPath("/api/folders", path), HttpMethod.Post);
        }

        public Task<bool> DeleteFolder(string path, bool recrusive)
        {
            IEnumerable<KeyValuePair<string, string>> values =
                KeyValuePairsUtils.CreatePairs("path", path, "recrusive", recrusive.ToString());

            return Request(GetUri("/api/folders", values), HttpMethod.Delete);
        }

        public Task<bool> FileExists(string path)
        {
            return Request<bool>(GetUriWithPath("/api/files/exists", path), HttpMethod.Get);
        }

        public Task<FileItemInfo> GetFileInfo(string path)
        {
            return Request<FileItemInfo>(GetUriWithPath("/api/files/info", path), HttpMethod.Get);
        }

        public Task<string> GetFileHash(string path)
        {
            return RequestString(GetUriWithPath("/api/files/hash", path), HttpMethod.Get);
        }

        public Task<bool> CopyFile(string srcPath, string destPath)
        {
            IEnumerable<KeyValuePair<string, string>> values =
                KeyValuePairsUtils.CreatePairs("srcPath", srcPath, "destPath", destPath);

            return Request(GetUri("/api/files/copy", values), HttpMethod.Post);
        }

        public Task<bool> MoveFile(string srcPath, string destPath)
        {
            IEnumerable<KeyValuePair<string, string>> values =
                KeyValuePairsUtils.CreatePairs("srcPath", srcPath, "destPath", destPath);

            return Request(GetUri("/api/files/move", values), HttpMethod.Post);
        }

        public Task<bool> WriteFile(string path, IInputStream stream)
        {
            return Request(GetUriWithPath("/api/files", path), HttpMethod.Post, stream);
        }

        public Task<bool> DeleteFile(string path)
        {
            return Request(GetUriWithPath("/api/files", path), HttpMethod.Delete);
        }

        public Task<IRandomAccessStreamWithContentType> GetFileRandomAccessStream(string path)
        {
            return RequestRandmomAccessStream(GetUriWithPath("/api/files", path), HttpMethod.Get);
        }

        public Task<IInputStream> GetFileInputStream(string path)
        {
            return RequestInputStream(GetUriWithPath("/api/files", path), HttpMethod.Get);
        }

        private async Task<TData> Request<TData>(Uri uri, HttpMethod method)
        {
            string responseText;

            try
            {
                using (HttpClient client = GetClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(method, uri))
                    {
                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            if (!response.IsSuccessStatusCode) return default(TData);

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

        private async Task<IInputStream> RequestInputStream(Uri uri, HttpMethod method)
        {
            try
            {
                using (HttpClient client = GetClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(method, uri))
                    {
                        using (HttpResponseMessage response = await client.SendRequestAsync(request))
                        {
                            if (!response.IsSuccessStatusCode) return null;

                            return await response.Content.ReadAsInputStreamAsync();
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

        private async Task<bool> Request(Uri uri, HttpMethod method, IInputStream body = null)
        {
            try
            {
                using (HttpClient client = GetClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(method, uri))
                    {
                        if (body != null) request.Content = new HttpStreamContent(body);

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

        public Uri GetUri(string resource)
        {
            return GetUri(resource, null);
        }

        public Uri GetUriWithPath(string resource, string path)
        {
            KeyValuePair<string, string>[] values = new[] { new KeyValuePair<string, string>("path", path) };

            return GetUri(resource, KeyValuePairsUtils.CreatePairs("path", path));
        }

        public Uri GetUri(string resource, IEnumerable<KeyValuePair<string, string>> values)
        {
            IEnumerable<string> queryPairs = values?.Select(p => string.Format("{0}={1}", p.Key, WebUtility.UrlEncode(p.Value)));
            string query = string.Join("&", queryPairs ?? Enumerable.Empty<string>());
            string url = string.Format("{0}/{1}?{2}", BaseUrl?.TrimEnd('/'), resource?.TrimStart('/'), query);

            return new Uri(url);
        }

        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient(GetFilter());
            client.DefaultRequestHeaders.Add("password", Password);
            return client;
        }

        private static HttpBaseProtocolFilter GetFilter()
        {
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);

            return filter;
        }
    }
}
