using StdOttUwp.ApplicationDataObjects;
using System;
using System.Linq;
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

        public string SaveFileName
        {
            get => GetValue<string>(nameof(SaveFileName));
            set => SetValue(nameof(SaveFileName), value);
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

        public string[] SyncRunTokens
        {
            get
            {
                string text;
                return TryGetValue(nameof(SyncRunTokens), out text) ? text.Split(syncRunTokensSeparator) : new string[0];
            }
            set
            {
                if (value.Any(token => token.Contains(syncRunTokensSeparator)))
                {
                    throw new ArgumentException("Run tokens must not contain the separator char");
                }

                SetValue(nameof(SyncRunTokens), string.Join(syncRunTokensSeparator.ToString(), value));
            }
        }

        public string BackgroundCMDs
        {
            get => GetValue<string>(nameof(BackgroundCMDs), null);
            set => SetValue(nameof(BackgroundCMDs), value);
        }

        public string ForegroundCMDs
        {
            get => GetValue<string>(nameof(BackgroundCMDs), null);
            set => SetValue(nameof(BackgroundCMDs), value);
        }

        public string CurrentSyncRunToken
        {
            get => GetValue<string>(nameof(CurrentSyncRunToken), null);
            set => SetValue(nameof(CurrentSyncRunToken), value);
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
