using Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Graphics.Printing.PrintSupport;
using Windows.Graphics.Printing.Workflow;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VirtPrnApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

		protected override void OnActivated(IActivatedEventArgs args)
		{
			Tasks.Log.Add("App OnActivated with args.Kind=" + args.Kind.ToString());
			switch (args.Kind)
			{
				case ActivationKind.PrintSupportSettingsUI:
					{
						var settingsEventArgs = args as PrintSupportSettingsActivatedEventArgs;
						if (settingsEventArgs == null)
						{
							Tasks.Log.Add("Error: PrintSupportSettingsActivatedEventArgs is null.");

							return;
						}
						/*Crashes:
						var settingsSession = settingsEventArgs.Session;
						Tasks.Log.Add("App OnActivated SettingsUI with settingsSession.LaunchKind=" + settingsSession.LaunchKind);
						*/
						var settsUI = InitFrame(args.PreviousExecutionState);
						if(settsUI != null)
							settsUI.settingsDeferral = settingsEventArgs.GetDeferral();
						break;
					}
				case ActivationKind.PrintSupportJobUI:
					{
						var settsUI = InitFrame(args.PreviousExecutionState);
						if (null != settsUI)
						{
							var workflowSettingsUIEventArgs = args as PrintWorkflowJobActivatedEventArgs;
							if (workflowSettingsUIEventArgs == null)
							{
								Tasks.Log.Add("Error: PrintWorkflowJobActivatedEventArgs is null.");
								return;
							}
							var session = workflowSettingsUIEventArgs.Session;
							session.PdlDataAvailable += settsUI.OnPdlDataAvailable;
							session.JobNotification += settsUI.OnJobNotification;
							session.VirtualPrinterUIDataAvailable += settsUI.VirtualPrinterUIDataAvailable;
							session.Start();// Start firing events
						}
						break;
					}
			}
		}

		private SettingsUI InitFrame(ApplicationExecutionState prevExecState)
		{
			SettingsUI ret = null;
			var rootFrame = new Frame();
			if (!rootFrame.Navigate(typeof(SettingsUI)))
				Tasks.Log.Add("Error: Failed to navigate rootFrame to SettingsUI.");
			else
			{
				ret = rootFrame.Content as SettingsUI;
				if (null == ret)
					Tasks.Log.Add("Error: rootFrame.Content SettingsUI is null after navigation.");
				else
				{
					if (ApplicationExecutionState.Terminated == prevExecState)
					{
						//TODO: Load state from previously suspended application
					}
				}
			}
			Window.Current.Content = rootFrame;
			Window.Current.Activate();
			return ret;
		}
		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
			Log.Add("App OnLaunched");
			Frame rootFrame = Window.Current.Content as Frame;
			if (rootFrame == null)
				InitFrame(e.PreviousExecutionState);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
