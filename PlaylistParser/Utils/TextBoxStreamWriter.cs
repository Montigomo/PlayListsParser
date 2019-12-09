using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		//public override void Write(char value)
		//{
		//	base.Write(value);
		//	_output.Dispatcher.BeginInvoke(DispatcherPriority.Normal,	new Action(() => _output.AppendText(value.ToString())));

		//}

		public override void WriteLine(string value)
		{
			base.WriteLine(value + Environment.NewLine);
			_output.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => _output.AppendText(value + Environment.NewLine)));

		}


		public override Encoding Encoding => Encoding.UTF8;

	}

  class TextBoxTraceListener : TraceListener
  {
    private TextBox tBox;

    public TextBoxTraceListener(TextBox box)
    {
      this.tBox = box;
    }

    public override void Write(string msg)
    {
      //allows tBox to be updated from different thread
      tBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal,new Action(() =>   tBox.AppendText(msg)));
    }

    public override void WriteLine(string msg)
    {
      Write(msg + "\r\n");
    }
  }

}
