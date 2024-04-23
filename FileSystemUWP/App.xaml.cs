using FileSystemCommonUWP;
using FileSystemCommonUWP.Database;
using FileSystemCommonUWP.Database.Servers;
using FileSystemUWP.Models;
using FileSystemUWP.Sync.Handling;
using StdOttUwp.BackPress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FileSystemUWP
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    sealed partial class App : Application
    {
        private readonly ViewModel viewModel;

        public AppDatabase Database { get; private set; }

        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.EnteredBackground += OnEnteredBackground;
            this.UnhandledException += OnUnhandledException;
            this.Suspending += OnSuspending;

            viewModel = new ViewModel();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Settings.Current.OnUnhandledException(e.Exception);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            BackPressHandler.Current.Activate();
            BackgroundTaskHelper.Current.RegisterTimerBackgroundTask();

            await LoadDatabase();
            Task startBackgroundHelperTask = BackgroundTaskHelper.Current.Start(Database);

            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                // Den Frame im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                    // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                    // übergeben werden
                    rootFrame.Navigate(typeof(MainPage), viewModel);
                }
                // Sicherstellen, dass das aktuelle Fenster aktiv ist
                Window.Current.Activate();
                await LoadViewModel();
            }

            await startBackgroundHelperTask;
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private async void OnEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Deferral deferral = e.GetDeferral();
            try
            {
                await Task.Delay(100); // Wait a moment to give others the chance to save some stuff in the ViewModel

                if (viewModel.IsLoaded) await StoreViewModel("enter background");
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async Task LoadDatabase()
        {
            if (Database == null) Database = await AppDatabase.OpenSqlite();
        }

        private async Task LoadViewModel()
        {
            IList<ServerInfo> servers = await Database.Servers.SelectServers();
            int? currentServerId = await Database.Servers.SelectCurrentServerId();
            viewModel.InjectData(servers.Select(s => new Server(s)), currentServerId);
        }

        private async Task StoreViewModel(string debug)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (!viewModel.IsLoaded)
                    {
                        Settings.Current.OnStorageException(new Exception($"ViewModel not loaded from {debug}. {viewModel.Servers.Count} servers"));
                        return;
                    }
                    if (viewModel.Servers.Count == 0)
                    {
                        Settings.Current.OnStorageException(new Exception($"No Servers stored from: {debug}"));
                    }

                    foreach (Server server in viewModel.Servers)
                    {
                        await Database.Servers.UpdateServer(server.ToInfo());
                    }

                    await Database.Servers.UpdateCurrentServer(viewModel.CurrentServer?.Id);
                    break;
                }
                catch (Exception e)
                {
                    Settings.Current.OnStorageException(new Exception("Store viewModel error", e));
                }
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            Database?.Dispose();
            BackgroundTaskHelper.Current.Dispose();
        }
    }
}
