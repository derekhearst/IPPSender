using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using SharpIpp;
namespace IPPSender
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		DirectoryInfo appdataPath = new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
		DirectoryInfo pathToSavePrintersTo = null;

		public MainWindow()
		{
			InitializeComponent();
			pathToSavePrintersTo = appdataPath.CreateSubdirectory("DerekTools").CreateSubdirectory("IPPSender").CreateSubdirectory("Printers");
			foreach(FileInfo fi in pathToSavePrintersTo.EnumerateFiles())
			{
				savedPrintersList.ItemsSource = fi.Name;
			}
			
		}

		private void openPathSavedPrinters_Click(object sender, RoutedEventArgs e)
		{
			CommonHelper.OpenPath(pathToSavePrintersTo.FullName);
		}

		private void addPrinter_Click(object sender, RoutedEventArgs e)
		{
			Printer printer = new();
			savedPrintersList.SelectedItem = Printer.SavePrinter(pathToSavePrintersTo, printer);
		}

		private void deletePrinter_Click(object sender, RoutedEventArgs e)
		{
			foreach(string file in savedPrintersList.Items)
			{
				File.Delete(pathToSavePrintersTo +"\\"+ file);
			}
		}
	}
}
