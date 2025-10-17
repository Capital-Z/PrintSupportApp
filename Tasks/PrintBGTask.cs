using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Graphics.Printing.PrintTicket;
using Windows.Graphics.Printing.Workflow;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Tasks
{
	public sealed class PrintBGTask : IBackgroundTask
	{
		private BackgroundTaskDeferral taskDeferral = null;
		//private IppPrintDevice? printDevice = null;

		public void Run(IBackgroundTaskInstance taskInstance)
		{
			Log.Add("PrintBGTask Run");
			taskDeferral = taskInstance.GetDeferral();
			var virtualPrinterDetails = taskInstance.TriggerDetails as PrintWorkflowVirtualPrinterTriggerDetails;
			if (virtualPrinterDetails == null)
			{
				Log.Add("Error: virtualPrinterDetails is null.");
				return;
			}
			PrintWorkflowVirtualPrinterSession session = virtualPrinterDetails.VirtualPrinterSession;
			if (session == null)
			{
				Log.Add("Error: VirtualPrinterSession is null.");
				taskDeferral?.Complete();
				return;
			}
			session.VirtualPrinterDataAvailable += VirtualPrinterDataAvailable;
			// Get print device for the session
			//printDevice = session.Printer;

			// Make sure to register all the event handlers before PrintWorkflowVirtualPrinterSession.Start is called.
			session.Start();
		}

		private async void VirtualPrinterDataAvailable(PrintWorkflowVirtualPrinterSession sender, PrintWorkflowVirtualPrinterDataAvailableEventArgs args)
		{
			Log.Add("PrintBGTask VirtualPrinterDataAvailable print ticket " + args.GetJobPrintTicket().XmlNode);
			PrintWorkflowSubmittedStatus jobStatus = PrintWorkflowSubmittedStatus.Failed;
			try
			{
				WorkflowPrintTicket printTicket = args.GetJobPrintTicket();
				if (args.UILauncher.IsUILaunchEnabled())
				{
					// LaunchAndCompleteUIAsync will launch the UI and wait for it to complete before returning
					PrintWorkflowUICompletionStatus status = await args.UILauncher.LaunchAndCompleteUIAsync();

					if (status == PrintWorkflowUICompletionStatus.Completed)
					{
						Log.Add("PrintBGTask VirtualPrinterDataAvailable after UI:Completed");
						//await this.ProcessContent(args);
						jobStatus = PrintWorkflowSubmittedStatus.Succeeded;
					}
					else if (status == PrintWorkflowUICompletionStatus.UserCanceled)
					{
						Log.Add("PrintBGTask VirtualPrinterDataAvailable after UI:UserCanceled");
						// Log user cancellation and cleanup here.
						jobStatus = PrintWorkflowSubmittedStatus.Canceled;
					}
				}
			}
			finally
			{
				args.CompleteJob(jobStatus);
				taskDeferral?.Complete();
			}
		}

		async Task ProcessContent(PrintWorkflowVirtualPrinterDataAvailableEventArgs args)
		{
					// Process and write pdl contents to target file
			PrintWorkflowPdlSourceContent sourceContent = args.SourceContent;
			StorageFile targetFile = await args.GetTargetFileAsync();
			Log.Add("PrintBGTask ProcessContent target file " + targetFile.Path);
			IRandomAccessStream outputStream = await targetFile.OpenAsync(FileAccessMode.ReadWrite);
			// Copy XPS input stream to target file output stream.
			await RandomAccessStream.CopyAndCloseAsync(sourceContent.GetInputStream(), outputStream.GetOutputStreamAt(0));
		}
	}
}
