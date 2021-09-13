using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace IPPSender
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		DirectoryInfo appdataPath = new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
		DirectoryInfo pathToSavePrintersTo = null;
		DirectoryInfo pathToSaveJobsTo = null;
		Printer currentlyDisplayedPrinter;
		ObservableCollection<string> currentDisplayingJobs = new();
		List<PrintJob> printedJobs = new();
		System.Timers.Timer mainTimer = new(3000);
		public MainWindow()
		{
			InitializeComponent();
			pathToSavePrintersTo = appdataPath.CreateSubdirectory("DerekTools").CreateSubdirectory("IPPSender").CreateSubdirectory("Printers");
			pathToSaveJobsTo = appdataPath.CreateSubdirectory("DerekTools").CreateSubdirectory("IPPSender").CreateSubdirectory("Jobs");
			savedPrintersList.ItemsSource = CommonHelper.PopulateFromPathOrSite(pathToSavePrintersTo.FullName);
			printingListPrinters.ItemsSource = CommonHelper.PopulateFromPathOrSite(pathToSavePrintersTo.FullName);
			printingListJobs.ItemsSource = CommonHelper.PopulateFromPathOrSite(pathToSaveJobsTo.FullName);
			printingListActiveJobs.ItemsSource = currentDisplayingJobs;
			mainTimer.Start();
			mainTimer.Elapsed += MainTimer_Elapsed;
		}



		private void openPathSavedPrinters_Click(object sender, RoutedEventArgs e)
		{
			CommonHelper.OpenPath(pathToSavePrintersTo.FullName);
		}

		private async void addPrinter_Click(object sender, RoutedEventArgs e)
		{
			InstallPrinterPopup pop = new();
			pop.ShowDialog();
			if (pop.DialogResult.Value)
			{
				Printer pt = new(pop.ipaddress.Text, pop.nickname.Text);
				await pt.RefreshValues();
				Printer.SavePrinter(pathToSavePrintersTo, pt);
				savedPrintersList.ItemsSource = CommonHelper.PopulateFromPathOrSite(pathToSavePrintersTo.FullName);
				savedPrintersList.SelectedItem = pop.nickname.Text;
			}
		}

		private void UpdatePrinterDisplayValues(Printer p)
		{
			currPrinterIP.Text = p.IP;
			currPrinterLabel.Text = p.Nickname;
			currPrinterName.Text = p.IPPName;
			currPrinterNameInfo.Text = p.IPPPNameInfo;
			currPrinterFirmware.Text = p.IPPFirmwareInstalled;
			currPrinterPPM.Text = p.IPPPPM + " ppm";
			currPrinterColorSupported.Text = p.IPPColorSupported;
			currPrinterUUID.Text = p.IPPUUID;
			currPrinterLocation.Text = p.IPPLocation;
			currPrinterState.Text = p.IPPPrinterState;
			currPrinterStateMessage.Text = p.IPPPrinterStateMessage;
			currPrinterSupplyList.ItemsSource = p.IPPSupplyValues;
			currPrinterSupportedMedia.ItemsSource = p.IPPSupportedMedia;
			currPrinterSupportedMediaSource.ItemsSource = p.IPPSupportedMediaSource;
			currPrinterSupportedMediaType.ItemsSource = p.IPPSupportedMediaType;
			currPrinterSupportedOutputTray.ItemsSource = p.IPPSuppportedOutputBin;
			currPrinterSupportedFinishings.ItemsSource = p.IPPSupportedFinishings;
			currPrinterSupportedSides.ItemsSource = p.IPPSupportedSides;
		}

		private void deletePrinter_Click(object sender, RoutedEventArgs e)
		{
			foreach (string file in savedPrintersList.SelectedItems)
			{
				File.Delete(pathToSavePrintersTo + "\\" + file);
			}
			savedPrintersList.ItemsSource = CommonHelper.PopulateFromPathOrSite(pathToSavePrintersTo.FullName);
		}

		private void savedPrintersList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0) { return; } //handeling unselection
			foreach (FileInfo file in pathToSavePrintersTo.EnumerateFiles())
			{
				if (file.Name == e.AddedItems[0].ToString()) //getting first item selected
				{
					currentlyDisplayedPrinter = Printer.ReadFromFile(file);
					UpdatePrinterDisplayValues(currentlyDisplayedPrinter);
				}
			}
		}

		private async void currPrinterRefresh_Click(object sender, RoutedEventArgs e)
		{
			if (await currentlyDisplayedPrinter.RefreshValues())
			{
				UpdatePrinterDisplayValues(currentlyDisplayedPrinter);
			}
			else
			{
				MessageBox.Show("Cannot communicate with printer. IP Might have changed or printer is off.", "Cannot communicate with printer", MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}

		private async void currPrinterEditValues_Click(object sender, RoutedEventArgs e)
		{
			InstallPrinterPopup pop = new(currentlyDisplayedPrinter.Nickname, currentlyDisplayedPrinter.IP);
			pop.ShowDialog();
			if (pop.DialogResult.Value)
			{
				currentlyDisplayedPrinter.IP = pop.ipaddress.Text;
				currentlyDisplayedPrinter.Nickname = pop.nickname.Text;
				await currentlyDisplayedPrinter.RefreshValues();
				Printer.SavePrinter(pathToSavePrintersTo, currentlyDisplayedPrinter);
				savedPrintersList.ItemsSource = CommonHelper.PopulateFromPathOrSite(pathToSavePrintersTo.FullName);
				savedPrintersList.SelectedItem = pop.nickname.Text;
			}
		}

		private void printingPrintJobs_Click(object sender, RoutedEventArgs e)
		{
			PrintJob.JobParams myParams = new()
			{
				media = printingMedia.Text,
				duplexing = printingDuplex.Text,
				sourceTray = printingMediaSource.Text,
				outputTray = printingOutputBin.Text,
				copies = int.Parse(printingCopies.Text),
				collation = printingCollate.Text,
				finishing = (SharpIpp.Model.Finishings)Enum.Parse(typeof(SharpIpp.Model.Finishings), printingFinishings.Text),
				mediaAttributes = printingPaperAttributes.Text,
			};

			List<Printer> printersToSendTo = new();
			List<FileInfo> jobsToSend = new();
			foreach (string file in printingListPrinters.SelectedItems)
			{
				printersToSendTo.Add(Printer.ReadFromFile(new(pathToSavePrintersTo + "\\" + file))); //creates a printer for all selected printers
			}
			foreach (string file in printingListJobs.SelectedItems)
			{
				jobsToSend.Add(new(pathToSaveJobsTo + "\\" + file));
			}
			foreach (Printer tempPrinter in printersToSendTo)
			{
				foreach (FileInfo job in jobsToSend)
				{
					printedJobs.Add(tempPrinter.AddToQueue(job, myParams));
				}
			}
		}

		private void openPathToJobs(object sender, RoutedEventArgs e)
		{
			CommonHelper.OpenPath(pathToSaveJobsTo.FullName);
		}

		private async void printingCancelJobs_Click(object sender, RoutedEventArgs e)
		{
			foreach (string file in printingListPrinters.SelectedItems)
			{
				Printer p = Printer.ReadFromFile(new(pathToSavePrintersTo + "\\" + file));
				if (!await p.CancelJobs())
				{
					MessageBox.Show("Unable To Cancel Jobs", "Error When Cancelling", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void printingListPrinters_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			foreach (string file in printingListPrinters.SelectedItems)
			{
				Printer p = Printer.ReadFromFile(new(pathToSavePrintersTo + "\\" + file));
				printingMedia.ItemsSource = p.IPPSupportedMedia;
				printingDuplex.ItemsSource = p.IPPSupportedSides;
				printingMediaSource.ItemsSource = p.IPPSupportedMediaSource;
				printingOutputBin.ItemsSource = p.IPPSuppportedOutputBin;
				printingCollate.ItemsSource = p.IPPSupportedCollate;
				printingFinishings.ItemsSource = p.IPPSupportedFinishings;
				printingPaperAttributes.ItemsSource = p.IPPSupportedMediaType;
			}

			printingMedia.SelectedItem = "na_letter_8.5x11in";
			printingDuplex.SelectedIndex = 0;
			printingMediaSource.SelectedItem = "auto";
			printingOutputBin.SelectedIndex = 0;
			printingCollate.SelectedIndex = 0;
			printingFinishings.SelectedIndex = 0;
			printingPaperAttributes.SelectedItem = "stationery";
		}

		private void RefreshJobsList(object sender, RoutedEventArgs e)
		{
			printingListJobs.ItemsSource = CommonHelper.PopulateFromPathOrSite(pathToSaveJobsTo.FullName);
		}


		private void MainTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				currentDisplayingJobs.Clear();
				foreach (PrintJob pj in printedJobs)
				{
					currentDisplayingJobs.Insert(0, pj.ToString());
				}
			}));

		}
	}
}
