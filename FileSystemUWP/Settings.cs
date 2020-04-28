using StdOttUwp.ApplicationDataObjects;
using System;
using Windows.Storage;

namespace FileSystemUWP
{
    class Settings : AppDataContainerObject
    {
        private static Settings instance;

        public static Settings Current
        {
            get
            {
                if (instance == null) instance = new Settings();

                return instance;
            }
        }

        public string BaseUrl
        {
            get => GetValue<string>(nameof(BaseUrl));
            set => SetValue(nameof(BaseUrl), value);
        }

        public string Username
        {
            get => GetValue<string>(nameof(Username));
            set => SetValue(nameof(Username), value);
        }

        public string Password
        {
            get => GetValue<string>(nameof(Password));
            set => SetValue(nameof(Password), value);
        }

        public string FolderPath
        {
            get => GetValue<string>(nameof(FolderPath));
            set => SetValue(nameof(FolderPath), value);
        }

        public Guid TimerBackgroundTaskRegistrationId
        {
            get
            {
                string idText;
                Guid id;
                if (TryGetValue(nameof(TimerBackgroundTaskRegistrationId), out idText) &&
                    Guid.TryParse(idText, out id)) return id;

                return Guid.Empty;
            }
            set => SetValue(nameof(TimerBackgroundTaskRegistrationId), value.ToString());
        }

        public Guid ApplicationBackgroundTaskRegistrationId
        {
            get
            {
                string idText;
                Guid id;
                if (TryGetValue(nameof(ApplicationBackgroundTaskRegistrationId), out idText) &&
                    Guid.TryParse(idText, out id)) return id;

                return Guid.Empty;
            }
            set => SetValue(nameof(ApplicationBackgroundTaskRegistrationId), value.ToString());
        }

        public string SyncExceptionText
        {
            get => GetValue<string>(nameof(SyncExceptionText));
        }

        public DateTime SyncExceptionTime
        {
            get => new DateTime(GetValue<long>(nameof(SyncExceptionTime)));
        }

        public DateTime SyncTimerTime
        {
            get => new DateTime(GetValue<long>(nameof(SyncTimerTime)));
            set => SetValue(nameof(SyncTimerTime), value.Ticks);
        }

        private Settings() : base(ApplicationData.Current.LocalSettings)
        {
        }

        public void OnSyncException(Exception e)
        {
            if (SetValue(nameof(SyncExceptionText), e.ToString()))
            {
                SetValue(nameof(SyncExceptionTime), DateTime.Now.Ticks);
            }
        }
    }
}
