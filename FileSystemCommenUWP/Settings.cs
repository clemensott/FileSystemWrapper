using StdOttUwp.ApplicationDataObjects;
using System;
using Windows.Storage;

namespace FileSystemCommonUWP
{
    public class Settings : AppDataContainerObject
    {
        const char syncRunTokensSeparator = '|';

        private static Settings instance;

        public static Settings Current
        {
            get
            {
                if (instance == null) instance = new Settings();

                return instance;
            }
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

        public AppDataExceptionObject StorageException
        {
            get => DeserialzeObject<AppDataExceptionObject>(nameof(StorageException));
        }

        public AppDataExceptionObject UnhandledException
        {
            get => DeserialzeObject<AppDataExceptionObject>(nameof(UnhandledException));
        }

        public AppDataExceptionObject SyncException
        {
            get => DeserialzeObject<AppDataExceptionObject>(nameof(SyncException));
        }

        public DateTime SyncTimerTime
        {
            get => new DateTime(GetValue<long>(nameof(SyncTimerTime)));
            set => SetValue(nameof(SyncTimerTime), value.Ticks);
        }

        private Settings() : base(ApplicationData.Current.LocalSettings)
        {
        }

        public void OnStorageException(Exception e)
        {
            SerialzeObject(nameof(StorageException), (AppDataExceptionObject)e);
        }

        public void OnUnhandledException(Exception e)
        {
            SerialzeObject(nameof(UnhandledException), (AppDataExceptionObject)e);
        }

        public void OnSyncException(Exception e)
        {
            SerialzeObject(nameof(SyncException), (AppDataExceptionObject)e);
        }
    }
}
