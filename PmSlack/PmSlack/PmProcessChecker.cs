using Slack.Webhooks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace PmSlack
{
	public class PmProcessChecker
	{
		public class PmProcess
		{
			public string Path;
			public DateTime StartTime;
			public Process Process;
		}

		private List<PmProcess> _oldPmProcessList;

		public PmProcessChecker()
		{
			_oldPmProcessList = GetNotiProcesses();
		}

		public void Check()
		{
			var configNoti = SlackNotiConfig.Config.Notis;

			List<PmProcess> curPmProcessList = GetNotiProcesses();
			if (configNoti.Count != _oldPmProcessList.Count)
			{
				Console.WriteLine("configNoti.Count != _oldPmProcessList.Count");
				return;
			}
			if (configNoti.Count != curPmProcessList.Count)
			{
				Console.WriteLine("configNoti.Count != curPmProcessList.Count");
				return;
			}

			for (int i = 0; i < configNoti.Count; ++i)
			{
				var noti = configNoti[i];
				var o = _oldPmProcessList[i];
				var c = curPmProcessList[i];

				if (c == null)
				{
					// process not exist

					if (o != null)
					{
						// process terminated in this timer step

						var log = string.Format("Process terminated! (path={0})", o.Path);
						Console.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log));
						LeaveSlackLog(noti.Url, noti.Channel, noti.UserName, noti.AppName, log);
					}
					else
					{
						// process terminated and not started yet
					}
				}
				else if (o == null)
				{
					// restarted
				}
				else
				{
					// check process restarted

					if (o.Process.Id != c.Process.Id || o.Process.HasExited != c.Process.HasExited)
					{
						var log = string.Format("Process restarted! (path={0})", o.Path);
						LeaveSlackLog(noti.Url, noti.Channel, noti.UserName, noti.AppName, log);
					}
				}
			}

			_oldPmProcessList = curPmProcessList;
		}

		private void LeaveSlackLog(string url, string channel, string userName, string appName, string log)
		{
			if (string.IsNullOrEmpty(url))
				return;

			var message = new SlackMessage()
			{
				Channel = channel,
				Text = string.Format("{0} -  {1}", appName, log),
				Username = userName,
			};
			var client = new SlackClient(url);
			client.Post(message);
		}

		private List<PmProcess> GetNotiProcesses()
		{
			var configNoti = SlackNotiConfig.Config.Notis;
			var pmProcesses = GetPmProcesses();

			var notiProcesses = new List<PmProcess>();
			foreach (var n in configNoti)
			{
				var noti = n as SlackNotiElement;
				var path = NormalizePath(noti.ProcessPath);
				var pp = pmProcesses.Where(p => p.Path == path).OrderBy(p => p.Process.Id).FirstOrDefault();
				notiProcesses.Add(pp);

				// DEBUG
				//Console.WriteLine("{0} - p={1} ",
				//	DateTime.Now.ToLongTimeString(),
				//	pp.Path);
				// DEBUG
			}
			return notiProcesses;
		}

		private List<PmProcess> GetPmProcesses()
		{
			var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
			using (var searcher = new ManagementObjectSearcher(wmiQueryString))
			using (var results = searcher.Get())
			{
				var query =
					from p in Process.GetProcesses()
					join mo in results.Cast<ManagementObject>()
					on p.Id equals (int)(uint)mo["ProcessId"]
					select new PmProcess
					{
						Path = NormalizePath((string)mo["ExecutablePath"]),
						Process = p,
						//StartTime = p.StartTime
					};
				return query.Where(p => string.IsNullOrEmpty(p.Path) == false).ToList();
			}
		}

		private static string NormalizePath(string path)
		{
			if (path == null)
				return string.Empty;
			return Path.GetFullPath(new Uri(path).LocalPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.ToUpperInvariant();
		}
	}
}
