using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PlayListsParser
{
    /// <summary>
    /// Interaction logic for PgEditorFile.xaml
    /// </summary>
    public partial class PgEditorFile : UserControl, Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor
    {
        public PgEditorFile()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", 
                typeof(string), typeof(PgEditorFile),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog() { IsFolderPicker = true, InitialDirectory = Value };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Value = dialog.FileName;
            }
        }

        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            Binding binding = new Binding("Value")
            {
                Source = propertyItem,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
            };
            BindingOperations.SetBinding(this, PgEditorFile.ValueProperty, binding);
            return this;
        }
    }
}
