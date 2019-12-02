using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace PlayListsParser
{
	/// <summary>
	/// Interaction logic for PgEditorFile.xaml
	/// </summary>
	public partial class PgEditorFolderRegex : UserControl, Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor
	{
		public PgEditorFolderRegex()
		{
			InitializeComponent();
			this.DataContext = this;
			Initialize();
		}

		public void Initialize()
		{
			var binding = new Binding()
			{
				Source = AppSettings.Instance.PlsFilterCollection,
				//Path = AppSettings.Instance.PlsFilterItems,
				Mode = BindingMode.OneWay
			};

			comboBoxMain.SetBinding(ItemsControl.ItemsSourceProperty, binding);

			//var bindingSelectedIndex = new Binding()
			//{
			//	Source = AppSettings.Instance.PlsFilterIndex,
			//	Path = new PropertyPath("PlayListsParser.AppSettings.Instance.PlsFilterIndex"),
			//	Mode = BindingMode.TwoWay
			//	//Mode = BindingMode.OneWay
			//};

			//comboBoxMain.SetBinding(Selector.SelectedIndexProperty, bindingSelectedIndex);

			comboBoxMain.SelectedIndex = AppSettings.Instance.PlsFilterIndex;
		}


		public bool Error { get; set; }


		public static readonly DependencyProperty ValueProperty =
				DependencyProperty.Register(
						"Value",
						typeof(string), typeof(PgEditorFolderRegex),
						new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string Value
		{
			get => (string)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}


		public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
		{
			Binding binding = new Binding("Value")
			{
				Source = propertyItem,
				Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
			};
			BindingOperations.SetBinding(this, PgEditorFolderRegex.ValueProperty, binding);
			return this;
		}

		private void comboBoxMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (comboBoxMain.SelectedIndex > -1)
				AppSettings.Instance.PlsFilterIndex = comboBoxMain.SelectedIndex;
		}

		private void _uc_Loaded(object sender, RoutedEventArgs e)
		{
			comboBoxMain.SelectedIndex = AppSettings.Instance.PlsFilterIndex;
		}

		private void CheckAndWriteValue()
		{
			if (comboBoxMain.Text != comboBoxMain.SelectedItem.ToString())
			{
				int index;
				AppSettings.Instance.PlsFilterCollection.Add(comboBoxMain.Text, out index);
				if (index > -1)
				{
					comboBoxMain.SelectedIndex = index;
				}
			}
		}

		private void comboBoxMain_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				CheckAndWriteValue();
			}
			else
			{
				// do stuff       
			}
		}
	}
}
