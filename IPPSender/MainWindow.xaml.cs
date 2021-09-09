using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
namespace IPPSender
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		DirectoryInfo appdataPath = new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
		DirectoryInfo pathToSavePrintersTo = null;
		Printer currentlyDisplayedPrinter;
		public MainWindow()
		{
			InitializeComponent();
			pathToSavePrintersTo = appdataPath.CreateSubdirectory("DerekTools").CreateSubdirectory("IPPSender").CreateSubdirectory("Printers");
			savedPrintersList.ItemsSource = CommonHelper.PopulateFromPathOrSite(pathToSavePrintersTo.FullName);
		}

		private void openPathSavedPrinters_Click(object sender, RoutedEventArgs e)
		{
			CommonHelper.OpenPath(pathToSavePrintersTo.FullName);
		}

		private async void addPrinter_Click(object sender, RoutedEventArgs e)
		{
			InstallPrinterPopup pop = new();
			pop.ShowDialog();
			if(pop.DialogResult.Value)
			{
				Printer pt = new(pop.ipaddress.Text, pop.nickname.Text);
				await pt.RefreshValues();
				Printer.SavePrinter(pathToSavePrintersTo, pt);
				savedPrintersList.ItemsSource = CommonHelper.PopulateFromPathOrSite(pathToSavePrintersTo.FullName);
			}
		}

		private void deletePrinter_Click(object sender, RoutedEventArgs e)
		{
			foreach(string file in savedPrintersList.Items)
			{
				File.Delete(pathToSavePrintersTo +"\\"+ file);
			}
			savedPrintersList.ItemsSource = CommonHelper.PopulateFromPathOrSite(pathToSavePrintersTo.FullName);
		}

		private void savedPrintersList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if(e.AddedItems.Count == 0) { return; } //handeling unselection
			foreach(FileInfo file in pathToSavePrintersTo.EnumerateFiles())
			{
				if (file.Name.Contains(e.AddedItems[0].ToString())) //getting first item selected
				{
					currentlyDisplayedPrinter = Printer.ReadFromFile(file);
				}
			}
		}
	}
}
