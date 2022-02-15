using System.Threading.Tasks;

namespace FileSystemUWP.API
{
    class ApiEdit : TaskCompletionSource<bool>
    {
        public Api Api { get; }

        public ApiEdit(Api api)
        {
            Api = api;
        }
    }
}
