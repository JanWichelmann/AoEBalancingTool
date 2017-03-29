using GenieLibrary;
using IORAMHelper;
using Microsoft.Win32;
using ScenarioLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public partial class TestScenarioWindow : INotifyPropertyChanged
	{
		#region Constants

		/// <summary>
		/// The version of the simulation file format.
		/// </summary>
		private const int Version = 1;

		#endregion

		#region Variables

		/// <summary>
		/// Random number generator for prettier positioning.
		/// </summary>
		private Random _rand = new Random();

		#endregion

		#region Functions

		/// <summary>
		/// Creates a new file selection window.
		/// </summary>
		/// <param name="balancingFile">The current balancing file.</param>
		/// <param name="genieFile">The base DAT file.</param>
		public TestScenarioWindow(BalancingFile balancingFile, GenieFile genieFile)
		{
			// Remember parameters
			BalancingFile = balancingFile;

			// Initialize controls
			InitializeComponent();
			DataContext = this;

			// Fill list boxes
			Researches1 = new ObservableCollection<ResearchEntry>(BalancingFile.ResearchEntries.Select(r => new ResearchEntry(r.Key, false, r.Value.DisplayName)));
			Researches2 = new ObservableCollection<ResearchEntry>(BalancingFile.ResearchEntries.Select(r => new ResearchEntry(r.Key, false, r.Value.DisplayName)));
			Units = BalancingFile.UnitEntries.ToDictionary(u => u.Key, u => $"[{u.Key}] {u.Value.DisplayName}");
			Duels = new ObservableCollection<Duel>();
			Civs = genieFile.Civs.Select((c, id) => new { c.Name, id }).Where(x => x.id > 0).ToDictionary(x => (short)x.id, x => x.Name.TrimEnd('\0'));
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

		/// <summary>
		/// Generates a unit at the given location.
		/// </summary>
		/// <param name="id">The instance ID of the unit.</param>
		/// <param name="unitId">The ID of the unit.</param>
		/// <param name="posX">The X position.</param>
		/// <param name="posY">The Y position.</param>
		/// <param name="rotation">Optional. The unit's initial rotation.</param>
		/// <returns></returns>
		private ScenarioLibrary.DataElements.Units.UnitEntry GenerateUnit(uint id, ushort unitId, int posX, int posY, int rotation = 0)
		{
			// Create rock unit
			return new ScenarioLibrary.DataElements.Units.UnitEntry()
			{
				Frame = 0,
				GarrisonId = -1,
				Id = id,
				PositionX = posX,
				PositionY = posY,
				PositionZ = 2,
				Rotation = rotation,
				UnitId = unitId,
				State = 2
			};
		}

		#endregion

		#region Event handlers

		private void _okButton_OnClick(object sender, RoutedEventArgs e)
		{
			// Show dialog
			SaveFileDialog dialog = new SaveFileDialog
			{
				Filter = "Scenario files (*.scx)|*.scx",
				Title = "Save generated simulation scenario..."
			};
			if(!(dialog.ShowDialog() ?? false))
				return;

			// Use base scenario
			uint nextUnitInstanceId = 0;
			Scenario s = new Scenario(new RAMBuffer(Properties.Resources.SimBase));

			// Set civs
			if(_civ1ComboBox.SelectedItem == null || _civ2ComboBox.SelectedItem == null)
			{
				// Error
				MessageBox.Show($"Please set the player civilizations.");
				return;
			}
			s.Header.PlayerData[0].CivId = (uint)(short)_civ1ComboBox.SelectedValue;
			s.Header.PlayerData[1].CivId = (uint)(short)_civ2ComboBox.SelectedValue;

			// Generate borders
			const int areaSize = 20;
			for(int i = 0; i < s.Map.MapWidth / areaSize; i++)
				for(int j = 0; j < s.Map.MapHeight / areaSize; j++)
				{
					for(int i2 = 0; i2 < areaSize; i2++)
					{
						s.Units.UnitSections[0].Units.Add(GenerateUnit(nextUnitInstanceId++, 623, areaSize * i + i2, areaSize * j + areaSize - 2, _rand.Next(0, 6)));
						s.Units.UnitSections[0].Units.Add(GenerateUnit(nextUnitInstanceId++, 623, areaSize * i + i2, areaSize * j + areaSize - 1, _rand.Next(0, 6)));
					}
					for(int j2 = 0; j2 < areaSize; j2++)
					{
						s.Units.UnitSections[0].Units.Add(GenerateUnit(nextUnitInstanceId++, 623, areaSize * i + areaSize - 2, areaSize * j + j2, _rand.Next(0, 6)));
						s.Units.UnitSections[0].Units.Add(GenerateUnit(nextUnitInstanceId++, 623, areaSize * i + areaSize - 1, areaSize * j + j2, _rand.Next(0, 6)));
					}
				}

			// Generate revealing units (handle borders)
			for(int i = 0; i < s.Map.MapWidth; i += (i >= s.Map.MapWidth - 6 ? 3 : 6))
				for(int j = 0; j < s.Map.MapHeight; j += (j >= s.Map.MapHeight - 6 ? 3 : 6))
					s.Units.UnitSections[1].Units.Add(GenerateUnit(nextUnitInstanceId++, 837, i, j));

			// Prepare initialization trigger
			ScenarioLibrary.DataElements.Triggers.Trigger initTrigger = new ScenarioLibrary.DataElements.Triggers.Trigger()
			{
				ConditionDisplayIndices = new List<int>(),
				Conditions = new List<ScenarioLibrary.DataElements.Triggers.Condition>(),
				Description = "\0",
				EffectDisplayIndices = new List<int>(),
				Effects = new List<ScenarioLibrary.DataElements.Triggers.Effect>(),
				Enabled = 1,
				Looping = 0,
				Name = "Init\0",
				ObjectiveDescriptionIndex = 0,
				ShowAsObjective = 0,
				State = 0,
				Unknown = 0
			};
			s.Triggers.TriggerData.Add(initTrigger);

			// Research checked technologies
			foreach(var res in Researches1)
				if(res.Checked)
					initTrigger.Effects.Add(ScenarioLibrary.DataElements.Triggers.Effect.CreateResearchTechnologyEffect(res.Id, 1));
			foreach(var res in Researches2)
				if(res.Checked)
					initTrigger.Effects.Add(ScenarioLibrary.DataElements.Triggers.Effect.CreateResearchTechnologyEffect(res.Id, 2));

			// Place duel units and generate battle triggers
			const int unitBorderDist = 5;
			int nextI = 0;
			int nextJ = 0;
			for(int d = 0; d < Duels.Count; ++d)
			{
				// Get duel
				Duel duel = Duels[d];

				// Create units
				var unitA = new ScenarioLibrary.DataElements.Units.UnitEntry()
				{
					Frame = 0,
					GarrisonId = -1,
					Id = nextUnitInstanceId++,
					PositionX = areaSize * nextI + (areaSize - 2) / 2,
					PositionY = areaSize * nextJ + unitBorderDist,
					PositionZ = 2,
					Rotation = 0,
					UnitId = (ushort)duel.Id1,
					State = 2
				};
				var unitB = new ScenarioLibrary.DataElements.Units.UnitEntry()
				{
					Frame = 0,
					GarrisonId = -1,
					Id = nextUnitInstanceId++,
					PositionX = areaSize * nextI + (areaSize - 2) / 2,
					PositionY = areaSize * nextJ + areaSize - 2 - unitBorderDist,
					PositionZ = 2,
					Rotation = 0,
					UnitId = (ushort)duel.Id2,
					State = 2
				};

				// Place units
				s.Units.UnitSections[1].Units.Add(unitA);
				s.Units.UnitSections[2].Units.Add(unitB);

				// Add renaming and attacking effects to init trigger
				initTrigger.Effects.Add(ScenarioLibrary.DataElements.Triggers.Effect.CreateRenamingEffect(unitA.Id, $"§{d}.A {duel.Name1}"));
				initTrigger.Effects.Add(ScenarioLibrary.DataElements.Triggers.Effect.CreateRenamingEffect(unitB.Id, $"§{d}.B {duel.Name2}"));
				initTrigger.Effects.Add(ScenarioLibrary.DataElements.Triggers.Effect.CreateTaskObjectEffect(unitA.Id, (int)unitB.Id, 1));
				initTrigger.Effects.Add(ScenarioLibrary.DataElements.Triggers.Effect.CreateTaskObjectEffect(unitB.Id, (int)unitA.Id, 2));

				// Create objective triggers
				var trigDestroyA = ScenarioLibrary.DataElements.Triggers.Trigger.CreateNew($"§{d}.A Destroy", "", true, false, false, 0);
				var trigDestroyB = ScenarioLibrary.DataElements.Triggers.Trigger.CreateNew($"§{d}.B Destroy", "", true, false, false, 0);
				var trigObjectiveADestroyed = ScenarioLibrary.DataElements.Triggers.Trigger.CreateNew($"§{d}.A Objective", $"§{d} {duel.Name1} vs. {duel.Name2} -> {duel.Name2} wins", false, false, true, 65535 - d);
				var trigObjectiveBDestroyed = ScenarioLibrary.DataElements.Triggers.Trigger.CreateNew($"§{d}.B Objective", $"§{d} {duel.Name1} vs. {duel.Name2} -> {duel.Name1} wins", false, false, true, 65535 - d);
				trigDestroyA.Conditions.Add(ScenarioLibrary.DataElements.Triggers.Condition.CreateDestroyObjectCondition(unitA.Id, 1));
				trigDestroyA.Effects.Add(ScenarioLibrary.DataElements.Triggers.Effect.CreateActivateTriggerEffect(s.Triggers.TriggerData.Count + 2));
				trigDestroyB.Conditions.Add(ScenarioLibrary.DataElements.Triggers.Condition.CreateDestroyObjectCondition(unitB.Id, 2));
				trigDestroyB.Effects.Add(ScenarioLibrary.DataElements.Triggers.Effect.CreateActivateTriggerEffect(s.Triggers.TriggerData.Count + 3));
				trigObjectiveADestroyed.Conditions.Add(ScenarioLibrary.DataElements.Triggers.Condition.CreateTimerCondition(65535));
				trigObjectiveBDestroyed.Conditions.Add(ScenarioLibrary.DataElements.Triggers.Condition.CreateTimerCondition(65535));
				trigDestroyA.ConditionDisplayIndices = Enumerable.Range(0, trigDestroyA.Conditions.Count).ToList();
				trigDestroyA.EffectDisplayIndices = Enumerable.Range(0, trigDestroyA.Effects.Count).ToList();
				trigDestroyB.ConditionDisplayIndices = Enumerable.Range(0, trigDestroyB.Conditions.Count).ToList();
				trigDestroyB.EffectDisplayIndices = Enumerable.Range(0, trigDestroyB.Effects.Count).ToList();
				trigObjectiveADestroyed.ConditionDisplayIndices = Enumerable.Range(0, trigObjectiveADestroyed.Conditions.Count).ToList();
				trigObjectiveBDestroyed.ConditionDisplayIndices = Enumerable.Range(0, trigObjectiveBDestroyed.Conditions.Count).ToList();
				s.Triggers.TriggerData.Add(trigDestroyA);
				s.Triggers.TriggerData.Add(trigDestroyB);
				s.Triggers.TriggerData.Add(trigObjectiveADestroyed);
				s.Triggers.TriggerData.Add(trigObjectiveBDestroyed);

				// Next position
				if(++nextJ >= s.Map.MapHeight / areaSize)
				{
					nextJ = 0;
					if(++nextI >= s.Map.MapWidth / areaSize)
						break;
				}
			}

			// Finalize init trigger
			initTrigger.EffectDisplayIndices = Enumerable.Range(0, initTrigger.Effects.Count).ToList();

			// Finalize trigger list
			s.Triggers.TriggerDisplayIndices = Enumerable.Range(0, s.Triggers.TriggerData.Count).ToList();

			// Save scenario
			s.Header.NextUnitIdToPlace = nextUnitInstanceId;
			s.WriteData(dialog.FileName);

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

		private void _loadSimulationButton_Click(object sender, RoutedEventArgs e)
		{
			// Show dialog
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filter = "Simulation files (*.balancingsim)|*.balancingsim",
				Title = "Load simulation..."
			};
			if(!(dialog.ShowDialog() ?? false))
				return;

			// Catch errors occuring while reading
			try
			{
				// Load file into buffer
				RAMBuffer buffer = new RAMBuffer(dialog.FileName);

				// Check version
				int version = buffer.ReadInteger();
				if(version > Version)
					throw new ArgumentException("The given file was created with a newer version of this program, please consider updating.");

				// Read civs
				_civ1ComboBox.SelectedValue = buffer.ReadShort();
				_civ2ComboBox.SelectedValue = buffer.ReadShort();

				// Merge tech lists
				int count1 = buffer.ReadInteger();
				HashSet<short> res1 = new HashSet<short>();
				for(int i = 0; i < count1; i++)
					res1.Add(buffer.ReadShort());
				foreach(var res in Researches1)
					res.Checked = res1.Contains(res.Id);
				int count2 = buffer.ReadInteger();
				HashSet<short> res2 = new HashSet<short>();
				for(int i = 0; i < count2; i++)
					res2.Add(buffer.ReadShort());
				foreach(var res in Researches2)
					res.Checked = res2.Contains(res.Id);

				// Read duels
				int count = buffer.ReadInteger();
				Duels.Clear();
				for(int i = 0; i < count; i++)
				{
					short id1 = buffer.ReadShort();
					short id2 = buffer.ReadShort();
					if(!Units.ContainsKey(id1) || !Units.ContainsKey(id2))
						continue;
					Duels.Add(new Duel(id1, Units[id1], id2, Units[id2]));
				}
			}
			catch(Exception ex)
			{
				// Error
				MessageBox.Show($"Unable to load given file: {ex.Message}");
			}
		}

		private void _saveSimulationButton_Click(object sender, RoutedEventArgs e)
		{
			// Show dialog
			SaveFileDialog dialog = new SaveFileDialog
			{
				Filter = "Simulation files (*.balancingsim)|*.balancingsim",
				Title = "Save simulation..."
			};
			if(!(dialog.ShowDialog() ?? false))
				return;

			// Create buffer
			RAMBuffer buffer = new RAMBuffer();

			// Write version
			buffer.WriteInteger(Version);

			// Write civ IDs
			buffer.WriteShort((short)(_civ1ComboBox.SelectedValue ?? (short)-1));
			buffer.WriteShort((short)(_civ2ComboBox.SelectedValue ?? (short)-1));

			// Run through techs and write IDs of checked ones
			var res1 = Researches1.Where(r => r.Checked).Select(r => r.Id).ToList();
			buffer.WriteInteger(res1.Count);
			res1.ForEach(buffer.WriteShort);
			var res2 = Researches2.Where(r => r.Checked).Select(r => r.Id).ToList();
			buffer.WriteInteger(res2.Count);
			res2.ForEach(buffer.WriteShort);

			// Write duels
			buffer.WriteInteger(Duels.Count);
			foreach(var duel in Duels)
			{
				buffer.WriteShort(duel.Id1);
				buffer.WriteShort(duel.Id2);
			}

			// Save
			try { buffer.Save(dialog.FileName); }
			catch(IOException ex)
			{
				// Error
				MessageBox.Show($"Unable to save simulation data: {ex.Message}");
			}
		}

		private void _addDuelButton_Click(object sender, RoutedEventArgs e)
		{
			// Check for selected items
			if(_duel1ComboBox.SelectedItem == null || _duel2ComboBox.SelectedItem == null)
				return;
			Duels.Add(new Duel(((KeyValuePair<short, string>)_duel1ComboBox.SelectedItem).Key, ((KeyValuePair<short, string>)_duel1ComboBox.SelectedItem).Value,
				((KeyValuePair<short, string>)_duel2ComboBox.SelectedItem).Key, ((KeyValuePair<short, string>)_duel2ComboBox.SelectedItem).Value));
		}

		private void _duelListBox_KeyDown(object sender, KeyEventArgs e)
		{
			// Remove selected duel
			if(e.Key != Key.Delete || _duelListBox.SelectedItem == null)
				return;
			Duels.Remove((Duel)_duelListBox.SelectedItem);
		}

		private void _allOn1Button_Click(object sender, RoutedEventArgs e)
		{
			// Check all techs
			foreach(var res in Researches1)
				res.Checked = true;
		}

		private void _allOff1Button_Click(object sender, RoutedEventArgs e)
		{
			// Uncheck all techs
			foreach(var res in Researches1)
				res.Checked = false;
		}

		private void _allOn2Button_Click(object sender, RoutedEventArgs e)
		{
			// Check all techs
			foreach(var res in Researches2)
				res.Checked = true;
		}

		private void _allOff2Button_Click(object sender, RoutedEventArgs e)
		{
			// Uncheck all techs
			foreach(var res in Researches2)
				res.Checked = false;
		}

		#endregion

		#region Properties

		#region Hidden fields

		private ObservableCollection<ResearchEntry> _researches1;
		private ObservableCollection<ResearchEntry> _researches2;
		private Dictionary<short, string> _units;
		private ObservableCollection<Duel> _duels;
		private Dictionary<short, string> _civs;

		#endregion

		/// <summary>
		/// The current balancing file.
		/// </summary>
		public BalancingFile BalancingFile { get; set; }

		/// <summary>
		/// Player 1 researches.
		/// </summary>
		public ObservableCollection<ResearchEntry> Researches1
		{
			get { return _researches1; }
			set
			{
				_researches1 = value;
				OnPropertyChanged(nameof(Researches1));
			}
		}

		/// <summary>
		/// Player 2 researches.
		/// </summary>
		public ObservableCollection<ResearchEntry> Researches2
		{
			get { return _researches2; }
			set
			{
				_researches2 = value;
				OnPropertyChanged(nameof(Researches2));
			}
		}

		/// <summary>
		/// Unit list.
		/// </summary>
		public Dictionary<short, string> Units
		{
			get { return _units; }
			set
			{
				_units = value;
				OnPropertyChanged(nameof(Units));
			}
		}

		/// <summary>
		/// The duels.
		/// </summary>
		public ObservableCollection<Duel> Duels
		{
			get { return _duels; }
			set
			{
				_duels = value;
				OnPropertyChanged(nameof(Duels));
			}
		}

		/// <summary>
		/// Civ list.
		/// </summary>
		public Dictionary<short, string> Civs
		{
			get { return _civs; }
			set
			{
				_civs = value;
				OnPropertyChanged(nameof(Civs));
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Implementation of PropertyChanged interface.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Sub types

		public class ResearchEntry : INotifyPropertyChanged
		{
			short _id;
			bool _checked;
			string _name;
			public short Id { get { return _id; } set { _id = value; OnPropertyChanged(nameof(Id)); } }
			public bool Checked { get { return _checked; } set { _checked = value; OnPropertyChanged(nameof(Checked)); } }
			public string Name { get { return _name; } set { _name = value; OnPropertyChanged(nameof(Name)); } }
			public ResearchEntry(short id, bool isChecked, string name)
			{ _id = id; _checked = isChecked; _name = name; }
			public event PropertyChangedEventHandler PropertyChanged;
			protected void OnPropertyChanged(string name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
		}

		public class Duel : INotifyPropertyChanged
		{
			short _id1;
			string _name1;
			short _id2;
			string _name2;
			public short Id1 { get { return _id1; } set { _id1 = value; OnPropertyChanged(nameof(Id1)); } }
			public string Name1 { get { return _name1; } set { _name1 = value; OnPropertyChanged(nameof(Name1)); } }
			public short Id2 { get { return _id2; } set { _id2 = value; OnPropertyChanged(nameof(Id2)); } }
			public string Name2 { get { return _name2; } set { _name2 = value; OnPropertyChanged(nameof(Name2)); } }
			public Duel(short id1, string name1, short id2, string name2)
			{ _id1 = id1; _name1 = name1; _id2 = id2; _name2 = name2; }
			public event PropertyChangedEventHandler PropertyChanged;
			protected void OnPropertyChanged(string name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
		}

		#endregion
	}
}