using System;
using Windows.Foundation;
using Windows.Graphics.Printing.Workflow;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PrintSupportApp1
{
	public sealed partial class MainPage : Page
    {
        public MainPage()
        {
			Tasks.Log.Add("MainPage Constructed");
			this.InitializeComponent();
			InSettingsUI = false;
		}
		public void OnJobNotification(PrintWorkflowJobUISession session, PrintWorkflowJobNotificationEventArgs args)
		{
			using (args.GetDeferral())
			{
				Tasks.Log.Add("MainPage OnJobNotification, args.PrinterJob.Printer.PrinterName" + args.PrinterJob.Printer.PrinterName);
			}
		}
		Deferral pdlDeferral = null;
		public void OnPdlDataAvailable(PrintWorkflowJobUISession session, PrintWorkflowPdlDataAvailableEventArgs args)
		{
			pdlDeferral = args.GetDeferral();
			Tasks.Log.Add("MainPage OnPdlDataAvailable args.PrinterJob.Printer.PrinterName: " + args.PrinterJob.Printer.PrinterName);
		}
		public bool InSettingsUI {  get; set; }
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Tasks.Log.Add("MainPage Button_Click");
			if (null != pdlDeferral)
			{
				Tasks.Log.Add("MainPage Button_Click releasing pdl deferral");
				pdlDeferral.Complete();//otherwise seems to be auto-released some time later
			}
			Frame rootFrame = Window.Current.Content as Frame;
			if (rootFrame != null && rootFrame.CanGoBack)
			{
				rootFrame.GoBack();
			}
			else
			{
				if(InSettingsUI)
					(Application.Current as App).ExitSettings();
				else//JobUI - simply close the app(?)
					Application.Current.Exit();
			}
		}
	}
}
