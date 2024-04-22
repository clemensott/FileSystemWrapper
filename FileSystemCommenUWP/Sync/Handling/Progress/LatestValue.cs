namespace FileSystemCommonUWP.Sync.Handling.Progress
{
    class LatestValue<T>
    {
        private readonly object lockObj;

        public bool HasNewValue { get; private set; }

        public T Value { get; private set; }

        public LatestValue()
        {
            lockObj = new object();
        }

        public void SetValue(T value)
        {
            lock (lockObj)
            {
                Value = value;
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
