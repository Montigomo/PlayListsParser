using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace PlaylistParser
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static readonly string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
		public static readonly string AppTitle = $@"Playlist Parser - {Version}";


        internal static MemoryMappedFile sharedMemory;

		private static volatile Mutex _instanceMutex = null;
		private static string _appGuid = "{CF1A37DC-C651-4C8F-B739-4B6E214E0810}";
		private static string mutexName = $"mutex_{_appGuid}";
		public static string mmfName = $"mmf_{_appGuid}";

		protected override void OnStartup(StartupEventArgs e)
		{

			string[] commandLineArgs = System.Environment.GetCommandLineArgs();

			try
			{
				bool createdNew = false;

				_instanceMutex = new Mutex(true, mutexName, out createdNew);

				#region Open Existing App
				if (!createdNew)
				{
					IntPtr hWnd = System.IntPtr.Zero;
					lock (typeof(MainWindow))
					{
						using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(mmfName, MemoryMappedFileRights.Read))
						{
							using (MemoryMappedViewAccessor mmfReader = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
							{
								mmfReader.Read<IntPtr>(0, out hWnd);
							}
						}
					}
					//WindowActivator.Activate(hWnd);
					Application.Current.Shutdown();
					return;
				}
				#endregion

				lock (typeof(MainWindow))
				{
					sharedMemory = MemoryMappedFile.CreateNew(mmfName, 8, MemoryMappedFileAccess.ReadWrite);
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace + "\n\n", "Exception thrown");
				Application.Current.Shutdown();
				return;
			}
			finally
			{

			}
			base.OnStartup(e);

			//var t = new Dictionary<int, string>();
			//var i = t.Keys.FirstOrDefault(ind => ind == 3);
		}

		private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			string errorMessage =
			    "An application error occurred. If this error occurs again there seems to be a serious bug in the application, and you better close it.\n\n" +
			    $"Error:{e.Exception.Message}\n\nDo you want to continue?\n (if you click Yes you will continue with your work, if you click No the application will close)";

			//insert code to log exception here 
			if (MessageBox.Show(errorMessage, "Application UnhandledException Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
			{
				if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
				{
					Application.Current.Shutdown();
				}
			}
			e.Handled = true;
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
		    sharedMemory?.Dispose();
		}


	}
}
