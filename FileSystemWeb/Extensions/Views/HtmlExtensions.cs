namespace FileSystemWeb.Extensions.Views
{
    static class HtmlExtensions
    {
        public static string ToAttribute(this bool value, string trueValue = "")
        {
            return ToAttribute(value, (object)trueValue);
        }

        public static string ToAttribute(this bool value, object trueValue)
        {
            return value ? trueValue?.ToString() : null;
        }
    }
}
