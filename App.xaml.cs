using Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Graphics.Printing.PrintSupport;
using Windows.Graphics.Printing.Workflow;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PrintSupportApp1
{
	public sealed partial class App : Application
    {
		Deferral settingsDeferral;
		public App()
        {
			Tasks.Log.Add("App Constructed");
            this.InitializeComponent();
		}
		protected override void OnActivated(IActivatedEventArgs args)
		{
			Tasks.Log.Add("App OnActivated with args.Kind=" + args.Kind.ToString());
			switch(args.Kind)
			{
				case ActivationKind.PrintSupportSettingsUI:
					{
						var settingsEventArgs = args as PrintSupportSettingsActivatedEventArgs;
						this.settingsDeferral = settingsEventArgs.GetDeferral();
						PrintSupportSettingsUISession settingsSession = settingsEventArgs.Session;
						Tasks.Log.Add("App OnActivated SettingsUI with settingsSession.LaunchKind=" + settingsSession.LaunchKind);

						var rootFrame = new Frame();
						rootFrame.Navigate(typeof(MainPage));
						(rootFrame.Content as MainPage).InSettingsUI = true;
						Window.Current.Content = rootFrame;
						Window.Current.Activate();
						break;
					}
				case ActivationKind.PrintSupportJobUI:
					{
						var rootFrame = new Frame();
						rootFrame.Navigate(typeof(MainPage));//"possibly" make different page for JobUI
						Window.Current.Content = rootFrame;

						var workflowJobUIEventArgs = args as PrintWorkflowJobActivatedEventArgs;
						PrintWorkflowJobUISession session = workflowJobUIEventArgs.Session;
						Tasks.Log.Add("App OnActivated JobUI with session.Status=" + session.Status);

						var jobUI = rootFrame.Content as MainPage;
						session.PdlDataAvailable += jobUI.OnPdlDataAvailable;
						session.JobNotification += jobUI.OnJobNotification;
						session.Start();

						Window.Current.Activate();
						break;
					}
			}
		}
		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{//not in docs but called when user clicks "Open printer ppp" in printer settings (maybe show About page)
			Log.Add("App OnLaunched");
			Frame rootFrame = Window.Current.Content as Frame;
			if (rootFrame == null)
			{
				rootFrame = new Frame();
				Window.Current.Content = rootFrame;
			}

			rootFrame.Navigate(typeof(MainPage));
			Window.Current.Activate();
		}

		internal void ExitSettings()
		{
			settingsDeferral.Complete();
		}
	}
}
