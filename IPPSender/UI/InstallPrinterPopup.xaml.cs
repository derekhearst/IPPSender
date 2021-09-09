using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IPPSender
{
	/// <summary>
	/// Interaction logic for InstallPrinterPopup.xaml
	/// </summary>
	public partial class InstallPrinterPopup : Window
	{
		string nick { get { return ipaddress.Text; } }
		string ip { get { return nickname.Text; } }
		public InstallPrinterPopup()
		{
			InitializeComponent();
		}
		public InstallPrinterPopup(string nick, string ip)
		{
			InitializeComponent();
			ipaddress.Text = ip;
			nickname.Text = nick;
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private async void doneButton_Click(object sender, RoutedEventArgs e)
		{
			doneButton.IsEnabled = false;
			if (await CommonHelper.CheckIP(ipaddress.Text))
			{
				DialogResult = true;
			}
			else
			{
				MessageBox.Show("Cannot communicate with inputed IP");
			}
			doneButton.IsEnabled = true;
		}

		private void nickname_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (nickname.Text.Contains("Input"))
			{
				nickname.Text = "";
			}
		}

		private void ipaddress_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (ipaddress.Text.Contains("Input"))
			{
				ipaddress.Text = "";
			}
		}
	}
}
