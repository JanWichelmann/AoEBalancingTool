using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AoEBalancingTool
{
	/// <summary>
	/// Main window.
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{
		#region Constants

		/// <summary>
		/// The base string of the window title.
		/// </summary>
		private const string WindowTitlePrefix = "Age of Empires II Balancing Tool";

		#endregion

		#region Variables

		/// <summary>
		/// The current balancing file.
		/// </summary>
		private BalancingFile _balancingFile = null;

		/// <summary>
		/// The base genie file being edited.
		/// </summary>
		private GenieLibrary.GenieFile _genieFile = null;

		/// <summary>
		/// The language DLL files used for name retrieval.
		/// </summary>
		private List<string> _languageFiles = null;

		/// <summary>
		/// The path of the current balancing file.
		/// </summary>
		private string _balancingFilePath = "";

		/// <summary>
		/// The currently selected unit entry.
		/// </summary>
		private KeyValuePair<short, UnitEntry> _selectedUnitEntry;

		#endregion

		#region Functions

		/// <summary>
		/// Constructor.
		/// </summary>
		public MainWindow()
		{
			// Initialize controls
			InitializeComponent();

			// Initialize resource type list
			ResourceTypes = new List<KeyValuePair<short, string>>();
			string[] resourceTypes = Properties.Resources.ResourceTypes.Split('\n');
			foreach(string resourceTypeEntry in resourceTypes)
			{
				// Split current entry into ID and value
				string[] resourceTypeEntryParts = resourceTypeEntry.Split('=');
				ResourceTypes.Add(new KeyValuePair<short, string>(short.Parse(resourceTypeEntryParts[0]), resourceTypeEntryParts[1].Trim()));
			}
			_cost1TypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ResourceTypes)) {Source = this});
			_cost2TypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ResourceTypes)) {Source = this});
			_cost3TypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ResourceTypes)) {Source = this});

			// Set data source
			DataContext = this;
		}

		/// <summary>
		/// Raises the property change event.
		/// </summary>
		/// <param name="name">The name of the changed property.</param>
		protected void OnPropertyChanged(string name)
		{
			// Raise event
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		#endregion

		#region Event handlers

		private void _aoe2DataFolderButton_Click(object sender, RoutedEventArgs e)
		{
			// Create and show dialog
			System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			if(Directory.Exists(_aoe2DataFolderTextBox.Text))
				folderBrowserDialog.SelectedPath = _aoe2DataFolderTextBox.Text;
			folderBrowserDialog.Description = "Pick DATA folder...";
			if(folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				_aoe2DataFolderTextBox.Text = folderBrowserDialog.SelectedPath;
		}

		private void _loadDatButton_Click(object sender, RoutedEventArgs e)
		{
			// Catch errors
			_languageFiles = new List<string>();
			try
			{
				// Find language files
				if(File.Exists(System.IO.Path.Combine(_aoe2DataFolderTextBox.Text, "language_x1_p1.dll")))
					_languageFiles.Add("language_x1_p1.dll");
				if(File.Exists(System.IO.Path.Combine(_aoe2DataFolderTextBox.Text, "language_x1.dll")))
					_languageFiles.Add("language_x1.dll");
				if(File.Exists(System.IO.Path.Combine(_aoe2DataFolderTextBox.Text, "LANGUAGE.dll")))
					_languageFiles.Add("LANGUAGE.dll");

				// Load genie file
				_genieFile =
					new GenieLibrary.GenieFile(GenieLibrary.GenieFile.DecompressData(new IORAMHelper.RAMBuffer(System.IO.Path.Combine(_aoe2DataFolderTextBox.Text, "empires2_x1_p1.dat"))));
			}
			catch(IOException ex)
			{
				// Error
				MessageBox.Show($"Unable to load given genie file: {ex.Message}");
				return;
			}

			// Create balancing data object
			BalancingFile = new BalancingFile(_genieFile, _languageFiles.ToArray());

			// Enable UI controls
			EnableEditorPanel = true;
		}

		private void _exportDatButton_Click(object sender, RoutedEventArgs e)
		{
			// Show save file dialog
			var saveFileDialog = new System.Windows.Forms.SaveFileDialog
			{
				Filter = "Genie database files (*.dat)|*.dat",
				Title = "Export DAT file..."
			};
			if(saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// Catch errors on export process
			try
			{
				// Reload internal genie file, apply changes and save
				GenieLibrary.GenieFile exportFile =
					new GenieLibrary.GenieFile(GenieLibrary.GenieFile.DecompressData(new IORAMHelper.RAMBuffer(System.IO.Path.Combine(_aoe2DataFolderTextBox.Text, "empires2_x1_p1.dat"))));
				BalancingFile.WriteChangesToGenieFile(exportFile);
				IORAMHelper.RAMBuffer exportFileBuffer = new IORAMHelper.RAMBuffer();
				exportFile.WriteData(exportFileBuffer);
				GenieLibrary.GenieFile.CompressData(exportFileBuffer).Save(saveFileDialog.FileName);
			}
			catch(IOException ex)
			{
				// Error
				MessageBox.Show($"Unable to export modified DAT file: {ex.Message}");
			}
		}

		private void _loadFileButton_Click(object sender, RoutedEventArgs e)
		{
			// Create and show dialog
			var openFileDialog = new System.Windows.Forms.OpenFileDialog
			{
				FileName = _balancingFilePath,
				Filter = "Balancing data file (*.balancing)|*.balancing",
				Title = "Load balancing data file..."
			};
			if(openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// Catch IO errors
			try
			{
				// Load file
				BalancingFile = new BalancingFile(_genieFile, openFileDialog.FileName, _languageFiles.ToArray());
				_balancingFilePath = openFileDialog.FileName;

				// Update window title
				CurrentWindowTitle = WindowTitlePrefix + " [" + _balancingFilePath + "]";
			}
			catch(IOException ex)
			{
				// Error
				MessageBox.Show($"Unable to load given file: {ex.Message}");
			}
		}

		private void _saveFileButton_Click(object sender, RoutedEventArgs e)
		{
			// Path selected?
			if(_balancingFilePath == "")
			{
				// Show "Save as..." dialog instead
				_saveFileAsButton_Click(sender, e);
				return;
			}

			// Catch IO errors
			try
			{
				// Save file
				BalancingFile.Save(_balancingFilePath);
			}
			catch(IOException ex)
			{
				// Error
				MessageBox.Show($"Unable to save balancing data: {ex.Message}");
			}
		}

		private void _saveFileAsButton_Click(object sender, RoutedEventArgs e)
		{
			// Show save file dialog
			var saveFileDialog = new System.Windows.Forms.SaveFileDialog
			{
				FileName = _balancingFilePath,
				Filter = "Balancing data file (*.balancing)|*.balancing",
				Title = "Save balancing data file..."
			};
			if(saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// Set file path and call save function
			_balancingFilePath = saveFileDialog.FileName;
			_saveFileButton_Click(sender, e);

			// Update window title
			CurrentWindowTitle = $"{WindowTitlePrefix} [{_balancingFilePath}]";
		}

		#endregion

		#region Events

		/// <summary>
		/// Implementation of PropertyChanged interface.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Properties

		#region Hidden fields

		private string _currentWindowTitle = WindowTitlePrefix;
		private bool _enableEditorPanel = false;

		#endregion

		/// <summary>
		/// The current window title.
		/// </summary>
		public string CurrentWindowTitle
		{
			get { return _currentWindowTitle; }
			set
			{
				_currentWindowTitle = value;
				OnPropertyChanged(nameof(CurrentWindowTitle));
			}
		}

		/// <summary>
		/// Controls the availability of the editing panel.
		/// </summary>
		public bool EnableEditorPanel
		{
			get { return _enableEditorPanel; }
			set
			{
				_enableEditorPanel = value;
				OnPropertyChanged(nameof(EnableEditorPanel));
			}
		}

		/// <summary>
		/// The current balancing file.
		/// </summary>
		public BalancingFile BalancingFile
		{
			get { return _balancingFile; }
			set
			{
				_balancingFile = value;
				OnPropertyChanged(nameof(BalancingFile));
				SelectedUnitEntry = BalancingFile.UnitEntries.First();
			}
		}

		/// <summary>
		/// The currently selected unit entry.
		/// </summary>
		public KeyValuePair<short, UnitEntry> SelectedUnitEntry
		{
			get { return _selectedUnitEntry; }
			set
			{
				_selectedUnitEntry = value;
				OnPropertyChanged(nameof(SelectedUnitEntry));
			}
		}

		/// <summary>
		/// The resource type list for the cost fields.
		/// </summary>
		public List<KeyValuePair<short, string>> ResourceTypes { get; }

		#endregion
	}
}