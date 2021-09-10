using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpIpp;
using SharpIpp.Model;
using System.IO;
namespace IPPSender
{
	class PrintJob
	{
		public struct JobParams
		{
			public string media { get; set; }
			public string mediaAttributes { get; set; }
			public int copies { get; set; }
			public string collation { get; set; }
			public string duplexing { get; set; }
			public string sourceTray { get; set; }
			public string outputTray { get; set; }
			public Finishings finishing { get; set; }
		}

		private SharpIppClient cli;
		public bool HasBeenSent { get; set; }
		public string JobID { get; set; }
		public Uri JobUri { get; set; }
		public string JobStatus { get; set; }
		public string JobMesage { get; set; }
		public DateTime StartedAt { get; set; }
		public DateTime EndedAt { get; set; }
		public int SecondsToComplete { get; set; }
		public int PagesComplete { get; set; }
		public int PPM { get; set; }

		private System.Timers.Timer mainTimer = new(2000);
		private List<int> pagesCompleted = new();
		private List<int> timebetween = new();
		private PrintJobRequest PrintReq;

		public PrintJob(SharpIppClient cli, PrintJobRequest PrintReq)
		{
			this.cli = cli;
			this.PrintReq = PrintReq;
			
		}
		override
		public string ToString()
		{
			return $"Job Sent: {HasBeenSent} | Job ID: {JobID} | State : {JobStatus} | Messsage : {JobMesage} | StopWatch : {SecondsToComplete} Seconds | Pages Printed : {PagesComplete} | PPM : {PPM}";
		}

		public async Task<bool> TrySend()
		{
			PrintJobResponse res;
			try
			{
				res = await cli.PrintJobAsync(PrintReq);
				JobUri = new(res.JobUri);
			}
			catch
			{
				HasBeenSent = false;
				return false;
			}
			HasBeenSent = true;
			mainTimer.Start();
			mainTimer.Elapsed += MainTimer_Elapsed;
			return true;
		}

		public async Task<bool> TryCancel(Uri printerUri)
		{
			try
			{
				CancelJobRequest req = new()
				{
					PrinterUri = printerUri,
					JobUrl = JobUri
				};
				CancelJobResponse res = await cli.CancelJobAsync(req);
				mainTimer.Stop();
				return true;
			}
			catch
			{
				return false;
			}
			
			

		}

		private async void MainTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(!await TryRefreshJobDetails())
			{
				mainTimer.Stop();
			}
		}

		public async Task<bool> TryRefreshJobDetails()
		{
			GetJobAttributesRequest req = new()
			{
				PrinterUri = PrintReq.PrinterUri,
				JobUrl = JobUri
			};
			GetJobAttributesResponse res;
			try
			{
				res = await cli.GetJobAttributesAsync(req);
			}
			catch(Exception e)
			{
				System.Diagnostics.Trace.WriteLine(e.Message);
				return false;
			} 

			timebetween.Clear();
			pagesCompleted.Add(res.JobAttributes.JobImpressionsCompleted.Value);

			for (int i = 0; i < pagesCompleted.Count - 1; i++)
			{
				timebetween.Add(pagesCompleted[i + 1] - pagesCompleted[i]);
			}
			if (timebetween.Count > 2)
			{
				double average = timebetween.Average();
				average = average / 2;
				PPM = (int)(average * 60);
			}

			JobID = res.JobAttributes.JobId.ToString() ?? "Null";
			JobStatus = res.JobAttributes.JobState.ToString() ?? "Null";
			JobMesage = res.JobAttributes.JobStateReasons[0].ToString() ?? "Null";
			StartedAt = res.JobAttributes.TimeAtProcessing ?? DateTime.Now;
			PagesComplete = res.JobAttributes.JobImpressionsCompleted ?? 0;
			if (res.JobAttributes.TimeAtCompleted is not null)
			{
				EndedAt = res.JobAttributes.TimeAtCompleted.Value;
				SecondsToComplete = (int)res.JobAttributes.TimeAtCompleted.Value.Subtract(res.JobAttributes.TimeAtProcessing.Value).TotalSeconds;
			}
			else
			{
				EndedAt = DateTime.Now;
				SecondsToComplete = 9999;
			}
			if(JobStatus is "Completed" or "Canceled")
			{
				return false;
			}
			return true;
		}
	}
}
