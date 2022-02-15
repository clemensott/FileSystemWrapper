﻿using FileSystemUWP.SettingsStorage;
using FileSystemUWP.Sync.Definitions;
using FileSystemUWP.Sync.Handling;
using StdOttStandard.Linq;
using StdOttUwp.BackPress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
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
        private static readonly string syncsFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "syncs.xml");

        private readonly ViewModel viewModel;

        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.EnteredBackground += OnEnteredBackground;

            viewModel = new ViewModel();
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
            Task loadViewModelTaks = Task.CompletedTask;

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
                    loadViewModelTaks = LoadViewModel();
                }
                // Sicherstellen, dass das aktuelle Fenster aktiv ist
                Window.Current.Activate();
            }

            await loadViewModelTaks;
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
                await StoreViewModel();
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
                await Store.LoadInto(syncsFilePath, viewModel);
            }
            catch (Exception e)
            {
                Settings.Current.OnSyncException(new Exception("Load viewModel error", e));
            }
            finally
            {
                viewModel.IsLoaded = true;
            }
        }

        public static Task SaveViewModel()
        {
            return ((App)Current).StoreViewModel();
        }

        private async Task StoreViewModel()
        {
            try
            {
                await Store.Save(syncsFilePath, viewModel);
            }
            catch (Exception e)
            {
                Settings.Current.OnSyncException(new Exception("Store viewModel error", e));
            }
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            BackgroundTaskDeferral deferral = args.TaskInstance.GetDeferral();
            BackgroundTaskHelper.Current.IsRunning = true;

            try
            {
                //if (args.TaskInstance.Task.Name == BackgroundTaskHelper.TimerBackgroundTaskBuilderName)
                //{
                //    Settings.Current.SyncTimerTime = DateTime.Now;
                //    await BackgroundTaskHelper.Current.Start(viewModel.Syncs, viewModel.Api);
                //}

                Queue<SyncPairHandler> syncs = BackgroundTaskHelper.Current.Queue;
                while (syncs.Count > 0)
                {
                    SyncPairHandler handler = syncs.Dequeue();
                    if (!await handler.Api.IsAuthorized()) continue;
                    await handler.Start();

                    SyncPair sync;
                    if (!handler.IsTestRun && handler.State == SyncPairHandlerState.Finished &&
                        viewModel.Servers.SelectMany(s => s.Syncs).TryFirst(s => s.Token == handler.Token, out sync))
                    {
                        sync.Result = handler.NewResult.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Settings.Current.OnSyncException(e);
            }
            finally
            {
                BackgroundTaskHelper.Current.IsRunning = false;

                await SaveViewModel();
                deferral.Complete();
            }
        }
    }
}
