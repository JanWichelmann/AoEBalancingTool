using GenieLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

		/// <summary>
		/// The currently selected research entry.
		/// </summary>
		private KeyValuePair<short, ResearchEntry> _selectedResearchEntry;

		/// <summary>
		/// The window last used for the base file selection. As windows cannot be reopened, a new one has to be created every time a file is loaded.
		/// </summary>
		private FileSelectionWindow _currentFileSelectionWindow = null;

		/// <summary>
		/// The currently displayed projectile rendering window.
		/// </summary>
		private ProjectileWindow _projectileWindow = null;

		/// <summary>
		/// The current mapping file. Used internally by the BalancingFile objects.
		/// </summary>
		private MappingFile _mappingFile = null;

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
			_unitCost1TypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ResourceTypes)) { Source = this });
			_unitCost2TypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ResourceTypes)) { Source = this });
			_unitCost3TypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ResourceTypes)) { Source = this });
			_researchCost1TypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ResourceTypes)) { Source = this });
			_researchCost2TypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ResourceTypes)) { Source = this });
			_researchCost3TypeComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ResourceTypes)) { Source = this });

			// Initialize armor class list
			ArmorClasses = new ObservableCollection<KeyValuePair<ushort, string>>();
			string[] armorClasses = Properties.Resources.ArmorClasses.Split('\n');
			if(File.Exists("ArmorClasses.txt"))
				armorClasses = File.ReadAllText("ArmorClasses.txt").Split('\n');
			foreach(string armorClassEntry in armorClasses)
			{
				// Split current entry into ID and value
				string[] armorClassEntryParts = armorClassEntry.Split('=');
				ArmorClasses.Add(new KeyValuePair<ushort, string>(ushort.Parse(armorClassEntryParts[0]), armorClassEntryParts[1].Trim()));
			}
			_attacksGridClassColumn.ItemsSource = ArmorClasses;
			_armorsGridClassColumn.ItemsSource = ArmorClasses;

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

		private void _loadDatButton_Click(object sender, RoutedEventArgs e)
		{
			// Show window (create new first, as closed windows cannot be reopened)
			_currentFileSelectionWindow = new FileSelectionWindow();
			if(!(_currentFileSelectionWindow.ShowDialog() ?? false))
				return;

			// Check whether genie file exists
			if(!File.Exists(_currentFileSelectionWindow.BaseGenieFilePath))
			{
				// Error
				MessageBox.Show($"The given genie file path is invalid: '{_currentFileSelectionWindow.BaseGenieFilePath}'");
				return;
			}

			// Catch errors
			_languageFiles = new List<string>();
			try
			{
				// Find language files
				if(File.Exists(_currentFileSelectionWindow.LanguageX1P1DllFilePath))
					_languageFiles.Add(_currentFileSelectionWindow.LanguageX1P1DllFilePath);
				if(File.Exists(_currentFileSelectionWindow.LanguageX1DllFilePath))
					_languageFiles.Add(_currentFileSelectionWindow.LanguageX1DllFilePath);
				if(File.Exists(_currentFileSelectionWindow.LanguageDllFilePath))
					_languageFiles.Add(_currentFileSelectionWindow.LanguageDllFilePath);

				// Load genie file
				_genieFile = new GenieLibrary.GenieFile(GenieLibrary.GenieFile.DecompressData(new IORAMHelper.RAMBuffer(_currentFileSelectionWindow.BaseGenieFilePath)));

			}
			catch(IOException ex)
			{
				// Error
				MessageBox.Show($"Unable to load given genie file: {ex.Message}");
				return;
			}

			// Check for mapping requirement
			_mappingFile = null;
			if(!string.IsNullOrWhiteSpace(_currentFileSelectionWindow.MappingFilePath) && File.Exists(_currentFileSelectionWindow.MappingFilePath))
			{
				// Read mapping
				_mappingFile = new MappingFile(new IORAMHelper.RAMBuffer(_currentFileSelectionWindow.MappingFilePath));
				if(!_mappingFile.CheckCompabilityToGenieFile(_genieFile))
					MessageBox.Show($"The given mapping file does not match the given DAT file.");
			}
			else if(_genieFile.Researches.Exists(r => r.Name.StartsWith("#BDep")))
				MessageBox.Show($"This file was probably created using an editor that dynamically reassigns unit and research IDs.\nIt is strongly recommended to reload using a valid mapping file.");

			// Create balancing data object
			BalancingFile = new BalancingFile(_genieFile, _languageFiles.ToArray(), _mappingFile);
			_balancingFilePath = "";

			// Set filterable lists
			UnitEntryList = new GenericCollectionView<KeyValuePair<short, UnitEntry>>(CollectionViewSource.GetDefaultView(_balancingFile.UnitEntries));
			OnPropertyChanged(nameof(UnitEntryList));

			// Update child windows
			if(_projectileWindow != null)
				_projectileWindow.GenieFile = _genieFile;

			// Reset window title
			CurrentWindowTitle = WindowTitlePrefix;

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
					new GenieLibrary.GenieFile(GenieLibrary.GenieFile.DecompressData(new IORAMHelper.RAMBuffer(_currentFileSelectionWindow.BaseGenieFilePath)));
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
				BalancingFile = new BalancingFile(_genieFile, openFileDialog.FileName, _languageFiles.ToArray(), _mappingFile);
				_balancingFilePath = openFileDialog.FileName;
			}
			catch(IOException ex)
			{
				// Error
				MessageBox.Show($"Unable to load given file: {ex.Message}");
			}
			catch(KeyNotFoundException)
			{
				// Error
				MessageBox.Show($"The given file and the current DAT are incompatible.");
			}

			// Set filterable lists
			UnitEntryList = new GenericCollectionView<KeyValuePair<short, UnitEntry>>(CollectionViewSource.GetDefaultView(_balancingFile.UnitEntries));
			OnPropertyChanged(nameof(UnitEntryList));

			// Update window title
			CurrentWindowTitle = WindowTitlePrefix + " [" + _balancingFilePath + "]";
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

		private void _filterUnitEntriesTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			// Filter
			if(string.IsNullOrWhiteSpace(_filterUnitEntriesTextBox.Text))
				UnitEntryList.Filter = null;
			else
				UnitEntryList.Filter = ue => ue.Value.DisplayName.IndexOf(_filterUnitEntriesTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
			UnitEntryList.Refresh();
		}

		private void _showProjectileWindowButton_Click(object sender, RoutedEventArgs e)
		{
			// Show window
			_projectileWindow = new ProjectileWindow
			{
				CurrentUnitEntry = SelectedUnitEntry,
				GenieFile = _genieFile
			};
			_projectileWindow.Show();
			OnPropertyChanged(nameof(ProjectileWindowIsVisible));
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e)
		{
			// Close all open windows
			if(_projectileWindow?.IsVisible ?? false)
				_projectileWindow.Close();
		}

		private void FloatingPointTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			TextBox senderTB = (TextBox)sender;
			if(float.TryParse(senderTB.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
				if(e.Key == Key.Down)
					senderTB.Text = (val - 0.1f).ToString(CultureInfo.InvariantCulture);
				else if(e.Key == Key.Up)
					senderTB.Text = (val + 0.1f).ToString(CultureInfo.InvariantCulture);
		}

		private void _exportTestScenarioButton_Click(object sender, RoutedEventArgs e)
		{
			// Show dialog
			new TestScenarioWindow(_balancingFile, _genieFile).ShowDialog();
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
				// Update internal field and notify controls
				_selectedUnitEntry = value;
				OnPropertyChanged(nameof(SelectedUnitEntry));

				// Update windows using the unit data
				if(_projectileWindow != null)
					_projectileWindow.CurrentUnitEntry = value;
			}
		}

		/// <summary>
		/// The currently selected research entry.
		/// </summary>
		public KeyValuePair<short, ResearchEntry> SelectedResearchEntry
		{
			get { return _selectedResearchEntry; }
			set
			{
				_selectedResearchEntry = value;
				OnPropertyChanged(nameof(SelectedResearchEntry));
			}
		}

		/// <summary>
		/// The filterable list of units.
		/// </summary>
		public GenericCollectionView<KeyValuePair<short, UnitEntry>> UnitEntryList { get; private set; }

		/// <summary>
		/// The resource type list for the cost fields.
		/// </summary>
		public List<KeyValuePair<short, string>> ResourceTypes { get; }

		/// <summary>
		/// The armor class list for the attacks and armors fields.
		/// </summary>
		public ObservableCollection<KeyValuePair<ushort, string>> ArmorClasses { get; }

		/// <summary>
		/// Determines whether the projectile window is visible.
		/// </summary>
		public bool ProjectileWindowIsVisible => _projectileWindow?.IsVisible ?? false;

		#endregion

	}
}