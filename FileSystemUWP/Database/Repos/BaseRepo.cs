using System;

namespace FileSystemUWP.Database.Repos
{
    abstract class BaseRepo : IDisposable
    {
        protected ISqlExecuteService sqlExecuteService;

        public BaseRepo(ISqlExecuteService sqlExecuteService)
        {
            this.sqlExecuteService = sqlExecuteService;
        }

        public void Dispose()
        {
            sqlExecuteService.Dispose();
        }
    }
}
