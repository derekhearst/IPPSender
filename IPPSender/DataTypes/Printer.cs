using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IPPSender
{
	class Printer
	{


		string Nickname { get; set; } = "Test";



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
