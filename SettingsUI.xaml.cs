using Windows.Foundation;
using Windows.Graphics.Printing.Workflow;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VirtPrnApp
{
	public sealed partial class SettingsUI : Page
	{
		public Deferral settingsDeferral { get; set; }
		public SettingsUI()
		{
			Tasks.Log.Add("SettingsPage Constructed");
			this.InitializeComponent();
			settingsDeferral = null;
		}

		public void VirtualPrinterUIDataAvailable(PrintWorkflowJobUISession session, PrintWorkflowVirtualPrinterUIEventArgs args)
		{
			Tasks.Log.Add("SettingsPage VirtualPrinterUIDataAvailable");
			using (args.GetDeferral())
			{
				string jobTitle = args.Configuration.JobTitle;
				string sourceApplicationName = args.Configuration.SourceAppDisplayName;
				string printerName = args.Printer.PrinterName;

				// Get pdl stream and content type
				IInputStream pdlContent = args.SourceContent.GetInputStream();
				string contentType = args.SourceContent.ContentType;
				this.ShowPrintPreview(jobTitle, pdlContent, contentType);
			}
		}

		internal void OnJobNotification(PrintWorkflowJobUISession sender, PrintWorkflowJobNotificationEventArgs args)
		{
			Tasks.Log.Add("SettingsPage OnJobNotification with job status " + args.PrinterJob.GetJobStatus().ToString());
		}

		internal void OnPdlDataAvailable(PrintWorkflowJobUISession sender, PrintWorkflowPdlDataAvailableEventArgs args)
		{
			Tasks.Log.Add("SettingsPage OnPdlDataAvailable with job status " + args.PrinterJob.GetJobStatus().ToString());
		}

		private void ShowPrintPreview(string jobTitle, IInputStream pdlContent, string contentType)
		{
			Tasks.Log.Add("SettingsPage ShowPrintPreview with contentType " + contentType);
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Tasks.Log.Add("MainPage Button_Click");
			Frame rootFrame = Window.Current.Content as Frame;
			if (rootFrame != null && rootFrame.CanGoBack)
			{
				rootFrame.GoBack();
			}
			else
			{
				if (null != settingsDeferral)
					settingsDeferral.Complete();
				else//JobUI - simply close the app(?)
					Application.Current.Exit();
			}
		}
	}
}
