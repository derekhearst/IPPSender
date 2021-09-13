using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace IPPSender
{
	class CommonHelper
	{
		public static void OpenPath(string uri)
		{
			Process.Start("explorer", uri);
		}

		public async static Task<bool> CheckIP(string ip)
		{
			try
			{
				string regexmatch = @"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$";
				var myRegex = Regex.Match(ip, regexmatch);
				if (myRegex.Success)
				{
					Ping pingSender = new();
					PingReply reply = await pingSender.SendPingAsync(ip, 1000);
					if (reply.Status == IPStatus.Success) { return true; }
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		public static List<string> PopulateFromPathOrSite(string path, string filter = "")
		{
			List<string> results = new();
			if (path.Contains("C:\\") || path.Contains("\\\\"))
			{
				DirectoryInfo directory;
				try
				{
					directory = new(path);
				}
				catch
				{
					results.Add($"{path} does not exist.");
					return results;
				}
				foreach (FileInfo file in directory.EnumerateFiles())
				{
					if (!file.Name.Contains(filter)) { continue; }
					results.Add(file.Name);
				}
				return results;
			}
			else if (path.Contains("http"))
			{
				results.Add("HTTP Links Not Supported");
				return results;
			}
			results.Add($"{path} is not valid.");
			return results;
		}




	}
}
