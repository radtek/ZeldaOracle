﻿using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ZeldaOracle.Common.Util;
using ZeldaWpf.Util;
using ZeldaWpf.Windows;
using WinFormsApplication = System.Windows.Forms.Application;

namespace ZeldaWpf {
	/// <summary>The application class.</summary>
	public partial class ZeldaWpfApp : Application {
		/// <summary>The last exception. Used to prevent multiple error windows for the same error.</summary>
		private static object lastException = null;
		/// <summary>True if an exception window is open.</summary>
		private static bool exceptionOpen;


		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		/// <summary>Constructs the app and sets up embedded assembly resolving.</summary>
		public ZeldaWpfApp() {
			Startup += OnAppStartup;
			AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssemblies;
			exceptionOpen = false;
		}


		//-----------------------------------------------------------------------------
		// Event Handlers
		//-----------------------------------------------------------------------------

		/// <summary>Resolves assemblies that may be embedded in the executable.</summary>
		private Assembly OnResolveAssemblies(object sender, ResolveEventArgs args) {
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			AssemblyName assemblyName = new AssemblyName(args.Name);

			string path = assemblyName.Name + ".dll";
			if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false) {
				path = string.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
			}

			using (Stream stream = Embedding.Get(path)) {
				if (stream == null)
					return null;

				byte[] assemblyRawBytes = new byte[stream.Length];
				stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
				return Assembly.Load(assemblyRawBytes);
			}
		}

		/// <summary>Hook the AppDomain and TaskScheduler unhandled exception events.</summary>
		private void OnAppStartup(object sender, StartupEventArgs e) {
			// Catch exceptions not in a UI thread
			DispatcherUnhandledException +=
				OnDispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException +=
				OnAppDomainUnhandledException;
			TaskScheduler.UnobservedTaskException +=
				OnTaskSchedulerUnobservedTaskException;
			WinFormsApplication.ThreadException +=
				OnWinFormsThreadException;
		}

		/// <summary>Show an exception window for an exception that occurred in a
		/// dispatcher thread.</summary>
		private void OnDispatcherUnhandledException(object sender,
			DispatcherUnhandledExceptionEventArgs e)
		{
			ShowErrorMessageBox(e.Exception);
			e.Handled = true;
		}

		/// <summary>Show an exception window for an exception that occurred in the
		/// AppDomain.</summary>
		private void OnAppDomainUnhandledException(object sender,
			UnhandledExceptionEventArgs e)
		{
			Dispatcher.Invoke(() => ShowErrorMessageBox(e.ExceptionObject));
		}

		/// <summary>Show an exception window for an exception that occurred in a task.</summary>
		private void OnTaskSchedulerUnobservedTaskException(object sender,
			UnobservedTaskExceptionEventArgs e)
		{
			Dispatcher.Invoke(() => ShowErrorMessageBox(e.Exception));
		}

		/// <summary>Show an exception window for an exception that occurred in winforms.</summary>
		private void OnWinFormsThreadException(object sender,
			ThreadExceptionEventArgs e)
		{
			Dispatcher.Invoke(() => ShowErrorMessageBox(e.Exception));
		}


		//-----------------------------------------------------------------------------
		// Internal Methods
		//-----------------------------------------------------------------------------

		/// <summary>The helper method for showing an error message.</summary>
		private void ShowErrorMessageBox(object ex) {
			// Ignore these thrown by the debugger
			if (ex is TaskCanceledException)
				return;
			if (ex != lastException && !exceptionOpen) {
				TimerEvents.PauseAll();
				lastException = ex;
				exceptionOpen = true;
				if (ErrorMessageBox.Show(ex))
					Environment.Exit(0);
				exceptionOpen = false;
				TimerEvents.ResumeAll();
			}
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		/// <summary>Gets if the exception window is open.</summary>
		public static bool IsExceptionOpen {
			get { return exceptionOpen; }
		}
	}
}
