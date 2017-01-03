using System.Windows.Data;

namespace AoEBalancingTool
{
	/// <summary>
	/// Extension that allows to use application settings in XAML in a more convenient way.
	/// Source: http://www.thomaslevesque.com/2008/11/18/wpf-binding-to-application-settings-using-a-markup-extension/
	/// </summary>
	public class SettingBindingExtension : Binding
	{
		public SettingBindingExtension() { Initialize(); }
		public SettingBindingExtension(string path) : base(path) { Initialize(); }
		private void Initialize()
		{
			Source = Properties.Settings.Default;
			Mode = BindingMode.TwoWay;
			UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
		}
	}
}
