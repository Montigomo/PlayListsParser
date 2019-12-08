using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PlaylistParser
{
	public class TextBoxStreamWriter : TextWriter
	{
		readonly TextBox _output;

		public TextBoxStreamWriter(TextBox output)
		{
			_output = output;
		}

		public override void Write(char value)
		{
			base.Write(value);
			_output.Dispatcher.BeginInvoke(DispatcherPriority.Background,
				new Action(() => _output.AppendText(value.ToString())));
			// When character data is written, append it to the text box.
		}

		public override Encoding Encoding => Encoding.UTF8;

	}
}
