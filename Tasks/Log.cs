using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;

namespace Tasks
{
	public static class Log
	{
		public static void Add(string msg)
		{//Plain log file. Recommended is to use Windows Event Logs for production
			try
			{
				/*
				StorageFolder folder = ApplicationData.Current.TemporaryFolder;
				StorageFile logFile = await folder.CreateFileAsync("log.log", CreationCollisionOption.OpenIfExists);
				await FileIO.AppendTextAsync(logFile, DateTime.Now + ": " + msg + Environment.NewLine);
				*/
				//C:\Users\<user>\AppData\Local\Packages\<package family name>\TempState\log.log
				var pthLog = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "log.log");
				using (FileStream fs = new FileStream(pthLog, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
				{
					fs.Seek(0, SeekOrigin.End);
					using (StreamWriter writer = new StreamWriter(fs))
					{
						writer.WriteLine(DateTime.Now + "[" + Process.GetCurrentProcess().Id + "] " + msg);
					}
				}
				/*Async using storage API - return Task and make sure to await all calls otherwise last log line(s) might be missing
				StorageFolder folder = ApplicationData.Current.TemporaryFolder;
				StorageFile logFile = await folder.CreateFileAsync("log.log", CreationCollisionOption.OpenIfExists);
				// Open the file with shared write access
				using (IRandomAccessStream stream = await logFile.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.AllowReadersAndWriters))
				{
					stream.Seek(stream.Size);
					using (StreamWriter writer = new StreamWriter(stream.AsStreamForWrite()))
					{
						writer.WriteLineAsync(DateTime.Now.ToString() + ": " + msg).Wait();
					}
				}
				*/
			}
			catch (Exception)
			{// Handle the exception, e.g., log it as event
			}
		}
	}
}