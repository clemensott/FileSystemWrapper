using FileSystemCommonUWP.API;
using System.Threading.Tasks;

namespace FileSystemUWP.API
{
    class ApiEdit : TaskCompletionSource<bool>
    {
        public bool IsAdd { get; }

        public Api Api { get; }

        public ApiEdit(Api api, bool isAdd)
        {
            Api = api;
            IsAdd = isAdd;
        }
    }
}
