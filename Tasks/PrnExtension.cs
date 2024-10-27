using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Graphics.Printing.PrintSupport;

namespace Tasks
{
	public sealed class PrnExtension : IBackgroundTask
	{
		BackgroundTaskDeferral taskDeferral;//Complete()d only when task is cancelled - lifetime unclear in docs?! OS cancels the task at some future point (~2mins) with reason SystemPolicy
		public void Run(IBackgroundTaskInstance taskInstance)
		{
			taskDeferral = taskInstance.GetDeferral();
			Log.Add("PrnExtension Run taskInstance: " + taskInstance);

			var psaTriggerDetails = taskInstance.TriggerDetails as PrintSupportExtensionTriggerDetails;
			var serviceSession = psaTriggerDetails.Session as PrintSupportExtensionSession;
			Log.Add("PrnExtension Run serviceSession.Printer.PrinterName: " + serviceSession.Printer.PrinterName);

			serviceSession.PrintDeviceCapabilitiesChanged += this.OnPdcChanged;
			serviceSession.PrintTicketValidationRequested += this.OnPrintTicketValidationRequested;
			taskInstance.Canceled += OnTaskCanceled;
			serviceSession.Start();
		}
		private void OnPdcChanged(PrintSupportExtensionSession session, PrintSupportPrintDeviceCapabilitiesChangedEventArgs args)
		{
			using (args.GetDeferral())
			{
				var pdc = args.GetCurrentPrintDeviceCapabilities();
				Log.Add("PrnExtension OnPdcChanged session.Printer.PrinterName: " + session.Printer.PrinterName);
					//+ ", args.GetCurrentPrintDeviceCapabilities(): " + args.GetCurrentPrintDeviceCapabilities().GetXml());
			}
		}
		private void OnPrinterSelected(PrintSupportExtensionSession session, PrintSupportPrinterSelectedEventArgs args)
		{
			Log.Add("PrnExtension OnPrinterSelected session.Printer.PrinterName: " + session.Printer.PrinterName);
		}
		private void OnPrintTicketValidationRequested(PrintSupportExtensionSession session, PrintSupportPrintTicketValidationRequestedEventArgs args)
		{
			using (args.GetDeferral())
			{
				Log.Add("PrnExtension OnPrintTicketValidationRequested session.Printer: " + session.Printer.PrinterName);
					//+ ", print ticket: " + args.PrintTicket.XmlNode.GetXml());
				args.SetPrintTicketValidationStatus(WorkflowPrintTicketValidationStatus.Resolved);
			}
		}
		private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
		{
			Log.Add("PrnExtension OnTaskCanceled reason: " + reason);
			this.taskDeferral.Complete();
		}
	}
}
