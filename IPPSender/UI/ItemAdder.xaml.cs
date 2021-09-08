using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;

namespace IPPSender
{
	/// <summary>
	/// Interaction logic for ItemAdder.xaml
	/// </summary>
	public partial class ItemAdder : UserControl
	{
		public ItemAdder()
		{
			InitializeComponent();
		}

		public void AddSearchFolder(DirectoryInfo directory)
		{
			
			items.ItemsSource = directory.GetFiles();
		}
	}
}
