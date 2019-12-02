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

namespace PlaylistParser
{
	/// <summary>
	/// Interaction logic for SettingWindow.xaml
	/// </summary>
	public partial class SettingWindow : Window
	{
		public SettingWindow()
		{
			InitializeComponent();
			propertyGridMain.SelectedObject = AppSettings.Instance;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Owner != null)
			{
				//Left = (Owner.Left + (Owner.Width / 2)) - Width / 2;
				//Left = Owner.Left + Left;
				//Left = Left > 0 ? Left : 30;
				//Top = (Owner.Top);
			}
		}
	}
}
