﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpIpp;
using SharpIpp.Model;

namespace IPPSender
{
	class Printer
	{
		public struct SupplyInfo
		{
			public SupplyInfo(string supplyname, int percent)
			{
				this.supplyname = supplyname;
				this.percent = percent;
			}
			public string supplyname;
			public int percent;
		}

		SharpIppClient ippCli = new();
		string Nickname { get; set; } = "Test";

		Uri IPPUri;
		public string IP {get;set;}
		


		//Printer State
		public string IPPName { get; set; } = "N\\A";
		public string IPPFirmwareInstalled { get; set; } = "N\\A";
		public string IPPPPM { get; set; } = "N\\A";
		public string IPPColorSupported { get; set; } = "N\\A";
		public string IPPPrinterState { get; set; } = "N\\A";
		public string IPPPrinterStateMessage { get; set; } = "N\\A";
		public List<SupplyInfo> IPPSupplyValues { get; set; } = new();
		//SupportedJobAttributes
		public List<string> IPPSupportedMedia { get; set; } = new();
		public List<string> IPPSupportedMediaSource { get; set; } = new();
		public List<string> IPPSupportedMediaType { get; set; } = new();
		public List<string> IPPSuppportedOutputBin { get; set; } = new();
		public List<string> IPPSupportedCollate { get; set; } = new();
		public List<Finishings> IPPSupportedFinishings { get; set; } = new();
		public List<string> IPPSupportedSides { get; set; } = new();

		public Printer() //dont use
		{

		}

		public Printer(string ip, string nickname)
		{
			this.IP = ip;
			this.IPPUri = new($"ipp://{ip}:631");
			this.Nickname = nickname;
		}

		public async Task RefreshValues()
		{
			if (!await CommonHelper.CheckIP(IP)) { return; }
			GetPrinterAttributesRequest req = new()
			{
				PrinterUri = IPPUri,
			};

			GetPrinterAttributesResponse response = await ippCli.GetPrinterAttributesAsync(req);
			if (response is null) { return; }
			IPPSupportedMedia.Clear();
			IPPSupportedMediaSource.Clear();
			IPPSupportedMediaType.Clear();
			IPPSuppportedOutputBin.Clear();
			IPPSupportedCollate.Clear();
			IPPSupportedFinishings.Clear();
			IPPSupportedSides.Clear();
			IPPSupplyValues.Clear();

			foreach (IppAttribute at in response.Sections[1].Attributes)
			{
				switch (at.Name)
				{
					case "media-supported":
						IPPSupportedMedia.Add(at.Value.ToString());
						break;
					case "media-source-supported":
						IPPSupportedMediaSource.Add(at.Value.ToString());
						break;
					case "media-type-supported":
						IPPSupportedMediaType.Add(at.Value.ToString());
						break;
					case "output-bin-supported":
						IPPSuppportedOutputBin.Add(at.Value.ToString());
						break;
					case "sides-supported":
						IPPSupportedSides.Add(at.Value.ToString());
						break;
					case "finishings-supported":
						IPPSupportedFinishings.Add((Finishings)at.Value);
						break;
					case "multiple-document-handling-supported":
						IPPSupportedCollate.Add(at.Value.ToString());
						break;
					case "printer-make-and-model":
						IPPName = at.Value.ToString();
						break;
					case "printer-firmware-version":
						IPPFirmwareInstalled = at.Value.ToString();
						break;
					case "pages-per-minute":
						IPPPPM = at.Value.ToString();
						break;
					case "color-supported":
						IPPColorSupported = at.Value.ToString();
						break;
					case "printer-state":
						IPPPrinterState = ((PrinterState)at.Value).ToString();
						break;
					case "printer-state-reasons":
						IPPPrinterStateMessage = at.Value.ToString();
						break;
					case "printer-supply":
						SupplyInfo info = new();
						foreach (string temp in at.Value.ToString().Split(";").ToList())
						{
							if (temp.Contains("level")) { info.percent = int.Parse(temp.Split("=")[1]); }
							else if (temp.Contains("colorantname")) { info.supplyname = temp.Split("=")[1]; } //the[1] gets the value after the =
						}
						IPPSupplyValues.Add(info);						
						break;
					default:
						break;
				}
				
			}
		}


		public static FileInfo SavePrinter(DirectoryInfo directoryToSaveTo, Printer printerToSave)
		{
			string json = JsonSerializer.Serialize(printerToSave);
			FileInfo file = new(directoryToSaveTo.FullName +"\\" + printerToSave.Nickname);
			File.WriteAllText(file.FullName, json);
			return file;
		}

		public static Printer ReadFromFile(FileInfo file)
		{
			string json = File.ReadAllText(file.FullName);
			return JsonSerializer.Deserialize<Printer>(json);
		}
	}
}
