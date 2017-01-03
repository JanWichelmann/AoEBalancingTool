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
		private void Application_Exit(object sender, ExitEventArgs e)
		{
			// Save settings
			AoEBalancingTool.Properties.Settings.Default.Save();
		}
	}
}
