using System;

namespace FileSystemCommonUWP.Sync.Handling.Progress
{
    class LatestValue<T>
    {
        private readonly object lockObj;

        public bool HasNewValue { get; private set; }

        public T Value { get; private set; }

        public LatestValue(T initialValue = default(T), bool hasNewValue = false)
        {
            lockObj = new object();
            Value = initialValue;
            HasNewValue = hasNewValue;
        }

        public void SetValue(T value)
        {
            lock (lockObj)
            {
                Value = value;
                HasNewValue = true;
            }
        }

        public void SetValue(Func<T, T> setter)
        {
            lock (lockObj)
            {
                Value = setter(Value);
                HasNewValue = true;
            }
        }

        public bool TryGetNewValue(out T value)
        {
            lock (lockObj)
            {
                value = Value;
                if (!HasNewValue) return false;

                HasNewValue = false;
                return true;
            }
        }
    }
}
