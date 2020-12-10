using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FileSystemUWP.Controls
{
    public class BackgroundOperations : ObservableCollection<KeyValuePair<Task, string>>
    {
        public void Add(Task task, string text)
        {
            Add(new KeyValuePair<Task, string>(task, text));
        }

        protected override void InsertItem(int index, KeyValuePair<Task, string> item)
        {
            if (item.Key.IsCompleted) return;

            AwaitTask(item.Key);
            base.InsertItem(index, item);
        }

        private async void AwaitTask(Task task)
        {
            try
            {
                await task;
            }
            catch { }

            Remove(this.FirstOrDefault(p => p.Key == task));
        }
    }
}
