using FileSystemCommonUWP;
using FileSystemUWP.Models;
using FileSystemUWP.SettingsStorage;
using FileSystemUWP.Sync.Handling;
using StdOttUwp.BackPress;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
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
        private static readonly Random rnd = new Random();
        private readonly ViewModel viewModel;

        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.EnteredBackground += OnEnteredBackground;
            this.UnhandledException += OnUnhandledException;

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

            Frame rootFrame = Window.Current.Content as Frame;
            Task loadViewModelTask = Task.CompletedTask;
            Task loadSyncPairContainersTask = BackgroundTaskHelper.Current.LoadContainers();

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
                    loadViewModelTask = LoadViewModel();
                }
                // Sicherstellen, dass das aktuelle Fenster aktiv ist
                Window.Current.Activate();
            }

            await loadViewModelTask;
            await loadSyncPairContainersTask;
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

        private async Task LoadViewModel()
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        string fileName = Settings.Current.SaveFileName;
                        if (string.IsNullOrWhiteSpace(fileName)) fileName = "syncs.xml";
                        await Store.LoadInto(GetSaveFilePath(fileName), viewModel);
                        break;
                    }
                    catch (Exception e)
                    {
                        Settings.Current.OnStorageException(new Exception("Load viewModel error", e));
                    }
                }
            }
            finally
            {
                viewModel.IsLoaded = true;
            }
        }

        public static Task SaveViewModel(string debug)
        {
            return ((App)Current).StoreViewModel(debug);
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

                    string fileName = GetNextSaveFileName();
                    await Store.Save(GetSaveFilePath(fileName), viewModel);
                    Settings.Current.SaveFileName = fileName;
                    break;
                }
                catch (Exception e)
                {
                    Settings.Current.OnStorageException(new Exception("Store viewModel error", e));
                }
            }
        }

        private static string GetNextSaveFileName()
        {
            string fileName;
            do
            {
                fileName = $"data_{rnd.Next(1, 5)}.xml";
            } while (fileName == Settings.Current.SaveFileName);
            return fileName;
        }

        private static string GetSaveFilePath(string fileName)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, fileName);
        }

        //protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        //{
        //    BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();
        //    BackgroundTaskHelper.Current.IsRunning = true;

        //    if (!viewModel.IsLoaded)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"on background1: {viewModel.IsLoaded} | {viewModel.Servers.Count}");
        //        await LoadViewModel();
        //        System.Diagnostics.Debug.WriteLine($"on background1.1: {viewModel.IsLoaded} | {viewModel.Servers.Count}");
        //    }

        //    try
        //    {
        //        System.Diagnostics.Debug.WriteLine($"on background2: {viewModel.IsLoaded} | {viewModel.Servers.Count}");
        //        //if (args.TaskInstance.Task.Name == BackgroundTaskHelper.TimerBackgroundTaskBuilderName)
        //        //{
        //        //    Settings.Current.SyncTimerTime = DateTime.Now;
        //        //    await BackgroundTaskHelper.Current.Start(viewModel.Syncs, viewModel.Api);
        //        //}

        //        Queue<SyncPairCommunicator> syncs = BackgroundTaskHelper.Current.Queue;
        //        System.Diagnostics.Debug.WriteLine($"on background3: {syncs.Count}");
        //        while (syncs.Count > 0)
        //        {
        //            SyncPairCommunicator communicator = syncs.Dequeue();
        //            if (!await communicator.Api.IsAuthorized() && await communicator.Api.LoadConfig()) continue;

        //            await communicator.Start();

        //            SyncPair sync;
        //            if (!communicator.Request.IsTestRun && communicator.Sync.State == SyncPairHandlerState.Finished &&
        //                viewModel.Servers.SelectMany(s => s.Syncs).TryFirst(s => s.Token == communicator.Request.Token, out sync))
        //            {
        //                throw new Exception("Implement saving result");
        //                //sync.Result = communicator.NewResult.ToArray();
        //            }
        //        }
        //        System.Diagnostics.Debug.WriteLine($"on background4: {viewModel.IsLoaded} | {viewModel.Servers.Count}");
        //    }
        //    catch (Exception e)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"on background4: {viewModel.IsLoaded} | {viewModel.Servers.Count} | {e.Message}");
        //        Settings.Current.OnSyncException(e);
        //    }
        //    finally
        //    {
        //        System.Diagnostics.Debug.WriteLine($"on background4: {viewModel.IsLoaded} | {viewModel.Servers.Count}");
        //        BackgroundTaskHelper.Current.IsRunning = false;

        //        await StoreViewModel("sync finished");
        //        deferral.Complete();
        //    }
        //}
    }
}
