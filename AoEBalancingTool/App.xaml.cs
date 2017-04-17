using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace AoEBalancingTool
{
	/// <summary>
	/// Main app class.
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			// Magic to force .NET 4 behaviour when entering floating point numbers
			FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			// Save settings
			AoEBalancingTool.Properties.Settings.Default.Save();
		}
	}
}
