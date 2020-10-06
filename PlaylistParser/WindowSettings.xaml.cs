using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	/// Interaction logic for WindowSettings.xaml
	/// </summary>
	public partial class WindowSettings : Window
	{
		public WindowSettings()
		{
			InitializeComponent();
			Initialize();
		}

		private void Initialize()
		{
			PropertyGridMain.SelectedObject = AppSettings.Instance;

			//AppSettings.Instance.PropertyChanged += SettingsPropertyChanged;
		}

		//private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		//{
		//	switch (e.PropertyName)
		//	{
		//		case nameof(AppSettings.PlaylistsFolder):
		//			RefreshPlaylists();
		//			break;
		//		case nameof(AppSettings.PlsFilter):
		//			RefreshPlaylists();
		//			break;
		//		default:
		//			break;
		//	}
		//}

	}
}
