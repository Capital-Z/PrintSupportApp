using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Graphics.Printing.Workflow;

namespace Tasks
{
    public sealed class PrnWorkflow : IBackgroundTask
	{
		BackgroundTaskDeferral taskDeferral;
		public void Run(IBackgroundTaskInstance taskInstance)
		{
			taskDeferral = taskInstance.GetDeferral();
			Log.Add("PrnWorkflow Run taskInstance: " + taskInstance);

			var jobTriggerDetails = taskInstance.TriggerDetails as PrintWorkflowJobTriggerDetails;
			var workflowBackgroundSession = jobTriggerDetails.PrintWorkflowJobSession as PrintWorkflowJobBackgroundSession;
			Log.Add("PrnWorkflow Run workflowBackgroundSession.Status: " + (int)workflowBackgroundSession.Status);
			// Register for events
			workflowBackgroundSession.JobStarting += this.OnJobStarting;
			workflowBackgroundSession.PdlModificationRequested += this.OnPdlModificationRequested;
			// Start Firing events
			workflowBackgroundSession.Start();
		}

		private void OnJobStarting(PrintWorkflowJobBackgroundSession session, PrintWorkflowJobStartingEventArgs args)
		{
			using (args.GetDeferral())
			{
				Log.Add("PrnWorkflow OnJobStarting, session.Status: " + session.Status
					+ ", args.Configuration.SessionId: " + args.Configuration.SessionId);
				args.SetSkipSystemRendering();//OnPdlModificationRequested() not called otherwise(?)
			}
		}
		private async void OnPdlModificationRequested(PrintWorkflowJobBackgroundSession session, PrintWorkflowPdlModificationRequestedEventArgs args)
		{
			//This is the only way to invoke any UI during print(?), i.e. must do the PDL conversion in order to invoke Job UI
			using (args.GetDeferral())
			{
				Log.Add("PrnWorkflow OnPdlModificationRequested, session.Status: " + session.Status
					+ ", args.Configuration.SessionId: " + args.Configuration.SessionId);
				if (args.UILauncher.IsUILaunchEnabled())
				{
					PrintWorkflowUICompletionStatus status = await args.UILauncher.LaunchAndCompleteUIAsync();
					Log.Add("PrnWorkflow OnPdlModificationRequested UILauncher status: " + status);
					if (status != PrintWorkflowUICompletionStatus.Completed)
					{
						if (status != PrintWorkflowUICompletionStatus.UserCanceled)// UI launch failed? abort print job.
							args.Configuration.AbortPrintFlow(PrintWorkflowJobAbortReason.JobFailed);
						this.taskDeferral.Complete();
						return;
					}
				}
				Log.Add("PrnWorkflow OnPdlModificationRequested args.SourceContent.ContentType: " + args.SourceContent.ContentType);
				if (String.Equals(args.SourceContent.ContentType, "application/oxps", StringComparison.OrdinalIgnoreCase))
				{
					var xpsContent = args.SourceContent.GetInputStream();

					var printTicket = args.PrinterJob.GetJobPrintTicket();
					PrintWorkflowPdlTargetStream streamTarget = args.CreateJobOnPrinter("application/pdf");
					PrintWorkflowPdlConverter pdlConverter = args.GetPdlConverter(PrintWorkflowPdlConversionType.XpsToPdf);
					await pdlConverter.ConvertPdlAsync(printTicket, xpsContent, streamTarget.GetOutputStream());
					streamTarget.CompleteStreamSubmission(PrintWorkflowSubmittedStatus.Succeeded);
				}
				else
					args.Configuration.AbortPrintFlow(PrintWorkflowJobAbortReason.JobFailed);
				this.taskDeferral.Complete();
			}
		}
	}
}
