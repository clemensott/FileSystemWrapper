using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace FileSystemCommonUWP
{
    class HttpRandomAccessStream : IRandomAccessStreamWithContentType
    {
        private HttpClient client;
        private IInputStream inputStream;
        private ulong size;
        private string etagHeader;
        private string lastModifiedHeader;
        private readonly Uri requestedUri;

        // No public constructor, factory methods instead to handle async tasks.
        private HttpRandomAccessStream(HttpClient client, Uri uri)
        {
            this.client = client;
            requestedUri = uri;
            Position = 0;
        }

        static public IAsyncOperation<HttpRandomAccessStream> CreateAsync(HttpClient client, Uri uri)
        {
            HttpRandomAccessStream randomStream = new HttpRandomAccessStream(client, uri);

            return AsyncInfo.Run(async (cancellationToken) =>
            {
                await randomStream.SendRequesAsync().ConfigureAwait(false);
                return randomStream;
            });
        }

        private async Task SendRequesAsync()
        {
            System.Diagnostics.Debug.Assert(inputStream == null);

            HttpRequestMessage request = null;
            request = new HttpRequestMessage(HttpMethod.Get, requestedUri);

            request.Headers.Add("Range", String.Format("bytes={0}-", Position));

            if (!string.IsNullOrEmpty(etagHeader))
            {
                request.Headers.Add("If-Match", etagHeader);
            }

            if (!string.IsNullOrEmpty(lastModifiedHeader))
            {
                request.Headers.Add("If-Unmodified-Since", lastModifiedHeader);
            }

            HttpResponseMessage response = await client.SendRequestAsync(request,
                HttpCompletionOption.ResponseHeadersRead);

            if (response.Content.Headers.ContentType != null)
            {
                this.ContentType = response.Content.Headers.ContentType.MediaType;
            }

            size = response.Content.Headers.ContentLength.Value;

            if (response.StatusCode != HttpStatusCode.PartialContent && Position != 0)
            {
                throw new Exception("HTTP server did not reply with a '206 Partial Content' status.");
            }

            if (!response.Headers.ContainsKey("Accept-Ranges"))
            {
                throw new Exception(String.Format(
                    "HTTP server does not support range requests: {0}",
                    "http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.5"));
            }

            if (String.IsNullOrEmpty(etagHeader) && response.Headers.ContainsKey("ETag"))
            {
                etagHeader = response.Headers["ETag"];
            }

            if (String.IsNullOrEmpty(lastModifiedHeader) && response.Content.Headers.ContainsKey("Last-Modified"))
            {
                lastModifiedHeader = response.Content.Headers["Last-Modified"];
            }

            if (response.Content.Headers.ContainsKey("Content-Type"))
            {
                ContentType = response.Content.Headers["Content-Type"];
            }

            inputStream = await response.Content.ReadAsInputStreamAsync().AsTask().ConfigureAwait(false);
        }

        public string ContentType { get; private set; }

        public bool CanRead => true;

        public bool CanWrite => false;

        public IRandomAccessStream CloneStream()
        {
            // If there is only one MediaPlayerElement using the stream, it is safe to return itself.
            return this;
        }

        public IInputStream GetInputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }

        public IOutputStream GetOutputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }

        public ulong Position { get; private set; }

        public void Seek(ulong position)
        {
            if (Position != position)
            {
                if (inputStream != null)
                {
                    inputStream.Dispose();
                    inputStream = null;
                }

                Position = position;
            }
        }

        public ulong Size
        {
            get => size;
            set => throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (inputStream != null)
            {
                inputStream.Dispose();
                inputStream = null;
            }
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return AsyncInfo.Run<IBuffer, uint>(async (cancellationToken, progress) =>
            {
                progress.Report(0);

                try
                {
                    if (inputStream == null)
                    {
                        await SendRequesAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    throw;
                }

                IBuffer result = await inputStream.ReadAsync(buffer, count, options).AsTask(cancellationToken, progress).ConfigureAwait(false);

                // Move position forward.
                Position += result.Length;

                return result;
            });
        }

        public IAsyncOperation<bool> FlushAsync()
        {
            throw new NotImplementedException();
        }

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}
