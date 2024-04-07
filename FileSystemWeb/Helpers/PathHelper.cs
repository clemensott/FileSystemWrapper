using FileSystemCommon;
using FileSystemCommon.Models.FileSystem;
using StdOttStandard.Models.HttpExceptions;

namespace FileSystemWeb.Helpers
{
    static class PathHelper
    {
        public static string DecodeAndValidatePath(string path)
        {
            return Utils.DecodePath(path ?? string.Empty) ?? throw new BadRequestHttpException("Path encoding error", code: 2001);
        }

        public static string GetParentPath(PathPart[] parts)
        {
            if (parts.Length == 0) return null;
            if (parts.Length == 1) return string.Empty;

            return parts[^2].Path;
        }
    }
}
