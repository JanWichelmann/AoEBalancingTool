using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AoEBalancingTool
{
	/// <summary>
	/// Allows selection of a base DAT file and corresponding DLL files.
	/// </summary>
	public partial class FileSelectionWindow : INotifyPropertyChanged
	{
		#region Variables

		#endregion

		#region Functions

		/// <summary>
		/// Creates a new file selection window.
		/// </summary>
		public FileSelectionWindow()
		{
			// Initialize controls
			InitializeComponent();
			DataContext = this;

			// Read values from application settings
			BaseGenieFilePath = Properties.Settings.Default.BaseGenieFilePath;
			LanguageDllFilePath = Properties.Settings.Default.LanguageDllFilePath;
			LanguageX1DllFilePath = Properties.Settings.Default.LanguageX1DllFilePath;
			LanguageX1P1DllFilePath = Properties.Settings.Default.LanguageX1P1DllFilePath;
		}

		#endregion

		#region Event handlers

		private void _okButton_OnClick(object sender, RoutedEventArgs e)
		{
			// Update application settings
			Properties.Settings.Default.BaseGenieFilePath = BaseGenieFilePath;
			Properties.Settings.Default.LanguageDllFilePath = LanguageDllFilePath;
			Properties.Settings.Default.LanguageX1DllFilePath = LanguageX1DllFilePath;
			Properties.Settings.Default.LanguageX1P1DllFilePath = LanguageX1P1DllFilePath;

			// Set dialog result and close
			DialogResult = true;
			Close();
		}

		private void _cancelButton_OnClick(object sender, RoutedEventArgs e)
		{
			// Set dialog result and close
			DialogResult = false;
			Close();
		}

		private void _browseBaseGenieFileButton_OnClick(object sender, RoutedEventArgs e)
		{
			// Create and show dialog
			var openFileDialog = new System.Windows.Forms.OpenFileDialog
			{
				FileName = File.Exists(BaseGenieFilePath) ? BaseGenieFilePath : "",
				Filter = "Genie database files (*.dat)|*.dat",
				Title = "Select base DAT file..."
			};
			if(openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// Save selected file
			BaseGenieFilePath = openFileDialog.FileName;
		}

		private void _browseLanguageDllFileButton_OnClick(object sender, RoutedEventArgs e)
		{
			// Create and show dialog
			var openFileDialog = new System.Windows.Forms.OpenFileDialog
			{
				FileName = File.Exists(LanguageDllFilePath) ? LanguageDllFilePath : "",
				Filter = "String resource files (*.dll)|*.dll",
				Title = "Select LANGUAGE.DLL file..."
			};
			if(openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// Save selected file
			LanguageDllFilePath = openFileDialog.FileName;
		}

		private void _browseLanguageX1DllFileButton_OnClick(object sender, RoutedEventArgs e)
		{
			// Create and show dialog
			var openFileDialog = new System.Windows.Forms.OpenFileDialog
			{
				FileName = File.Exists(LanguageX1DllFilePath) ? LanguageX1DllFilePath : "",
				Filter = "String resource files (*.dll)|*.dll",
				Title = "Select language_x1.dll file..."
			};
			if(openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// Save selected file
			LanguageX1DllFilePath = openFileDialog.FileName;
		}

		private void _browseLanguageX1P1DllFileButton_OnClick(object sender, RoutedEventArgs e)
		{
			// Create and show dialog
			var openFileDialog = new System.Windows.Forms.OpenFileDialog
			{
				FileName = File.Exists(LanguageX1P1DllFilePath) ? LanguageX1P1DllFilePath : "",
				Filter = "String resource files (*.dll)|*.dll",
				Title = "Select language_x1_p1.dll file..."
			};
			if(openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// Save selected file
			LanguageX1P1DllFilePath = openFileDialog.FileName;
		}

		#endregion

		#region Properties

		#region Hidden fields

		private string _baseGenieFilePath;
		private string _languageDllFilePath;
		private string _languageX1DllFilePath;
		private string _languageX1P1DllFilePath;

		#endregion

		/// <summary>
		/// The file path of the base DAT file.
		/// </summary>
		public string BaseGenieFilePath
		{
			get { return _baseGenieFilePath; }
			set
			{
				_baseGenieFilePath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BaseGenieFilePath)));
			}
		}

		/// <summary>
		/// The file path of the LANGUAGE.DLL file.
		/// </summary>
		public string LanguageDllFilePath
		{
			get { return _languageDllFilePath; }
			set
			{
				_languageDllFilePath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LanguageDllFilePath)));
			}
		}

		/// <summary>
		/// The file path of the language_x1.dll file.
		/// </summary>
		public string LanguageX1DllFilePath
		{
			get { return _languageX1DllFilePath; }
			set
			{
				_languageX1DllFilePath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LanguageX1DllFilePath)));
			}
		}

		/// <summary>
		/// The file path of the language_x1_p1.dll file.
		/// </summary>
		public string LanguageX1P1DllFilePath
		{
			get { return _languageX1P1DllFilePath; }
			set
			{
				_languageX1P1DllFilePath = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LanguageX1P1DllFilePath)));
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Implementation of PropertyChanged interface.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

	}
}