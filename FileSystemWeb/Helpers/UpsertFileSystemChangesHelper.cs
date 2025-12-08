using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FileSystemWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace FileSystemWeb.Helpers;

public static class UpsertFileSystemChangesHelper
{
    public static async Task UpsertFileChanges(this DbContext context, params FileChange[] changes)
    {
        object[][] values = changes
            .Select(c => new object[]
            {
                c.Path,
                (int)c.ChangeType,
                c.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fffffff")
            })
            .ToArray();
        await Upsert(context, "FileChanges", values);
    }

    public static async Task UpsertFolderChanges(this DbContext context, params FolderChange[] changes)
    {
        object[][] values = changes
            .Select(c => new object[]
            {
                c.Path,
                (int)c.ChangeType,
                c.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fffffff")
            })
            .ToArray();
        await Upsert(context, "FolderChanges", values);
    }

    private static async Task Upsert(DbContext context, string tableName, object[][] values)
    {
        int valueIndex = 0;
        string valuesFormat = string.Join(',',
            values.Select(v => "(" + string.Join(",", v.Select(_ => $"{{{valueIndex++}}}").ToArray()) + ")").ToArray());
        string format =
            $"INSERT INTO {tableName} (Path, ChangeType, Timestamp) VALUES {valuesFormat} ON CONFLICT (Path) DO UPDATE SET ChangeType = excluded.ChangeType, Timestamp = excluded.Timestamp;";
        FormattableString sql = FormattableStringFactory.Create(format, values.SelectMany(v => v).ToArray());
        await context.Database.ExecuteSqlAsync(sql);
    }
}