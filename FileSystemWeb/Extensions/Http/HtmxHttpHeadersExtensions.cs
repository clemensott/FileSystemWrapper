using Microsoft.AspNetCore.Http;
using System.Linq;

namespace FileSystemWeb.Extensions.Http
{
    static class HtmxHttpHeadersExtensions
    {
        private static bool IsHtmxHeader(HttpRequest httpRequest, string header)
        {
            return httpRequest.Headers.TryGetValue(header, out var values)
                && values.Any(v => bool.TryParse(v, out bool isRequest) && isRequest);
        }

        public static bool IsHtmxRequest(this HttpRequest request)
        {
            return IsHtmxHeader(request, "HX-Request");
        }

        public static bool IsHtmxBoosted(this HttpRequest request)
        {
            return IsHtmxHeader(request, "HX-Boosted");
        }

        public static bool IsHtmxHistoryRestoreRequest(this HttpRequest request)
        {
            return IsHtmxHeader(request, "HX-History-Restore-Request");
        }

        private static string GetHtmxValue(HttpRequest request, string header)
        {
            return request.Headers.TryGetValue(header, out var values)
                && values.Count > 0 ? values.First() : null;
        }

        public static string GetHtmxCurrentURL(this HttpRequest httpRequest)
        {
            return GetHtmxValue(httpRequest, "HX-Current-URL");
        }

        public static string GetHtmxPrompt(this HttpRequest httpRequest)
        {
            return GetHtmxValue(httpRequest, "HX-Prompt");
        }

        public static string GetHtmxTarget(this HttpRequest httpRequest)
        {
            return GetHtmxValue(httpRequest, "HX-Target");
        }

        public static string GetHtmxTriggerName(this HttpRequest httpRequest)
        {
            return GetHtmxValue(httpRequest, "HX-Trigger-Name");
        }

        public static string GetHtmxTrigger(this HttpRequest httpRequest)
        {
            return GetHtmxValue(httpRequest, "HX-Trigger");
        }

        private static void SetHtmxHeader(HttpResponse httpResponse, string header, string value)
        {
            httpResponse.Headers.Add(header, value);
        }

        public static void SetHtmxLocation(this HttpResponse httpResponse, string location)
        {
            SetHtmxHeader(httpResponse, "HX-Location", location);
        }

        public static void SetHtmxPushUrl(this HttpResponse httpResponse, string url)
        {
            SetHtmxHeader(httpResponse, "HX-Push-Url", url);
        }

        public static void SetHtmxRedirect(this HttpResponse httpResponse, string redirect)
        {
            SetHtmxHeader(httpResponse, "HX-Redirect", redirect);
        }

        public static void SetHtmxRefresh(this HttpResponse httpResponse, bool refresh = true)
        {
            SetHtmxHeader(httpResponse, "HX-Refresh", refresh.ToString());
        }

        public static void SetHtmxReplaceUrl(this HttpResponse httpResponse, string replaceUrl)
        {
            SetHtmxHeader(httpResponse, "HX-Replace-Url", replaceUrl);
        }

        public static void SetHtmxReswap(this HttpResponse httpResponse, string reswap)
        {
            SetHtmxHeader(httpResponse, "HX-Reswap", reswap);
        }

        public static void SetHtmxRetarget(this HttpResponse httpResponse, string retarget)
        {
            SetHtmxHeader(httpResponse, "HX-Retarget", retarget);
        }

        public static void SetHtmxReselect(this HttpResponse httpResponse, string reselect)
        {
            SetHtmxHeader(httpResponse, "HX-Reselect", reselect);
        }
    }
}
