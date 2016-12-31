using System;
using System.Collections.Generic;
using System.Linq;
using GenieLibrary.DataElements;

namespace AoEBalancingTool
{
	/// <summary>
	/// Contains the balancing data diffs.
	/// The save/load mechanism relies on unchanged unit types between saving and loading, else there may be load errors.
	/// </summary>
	internal class BalancingFile
	{
		#region Constants

		/// <summary>
		/// The version of the balancing file format.
		/// </summary>
		private const int Version = 1;

		#endregion

		#region Variables

		/// <summary>
		/// The modified units, indexed by their IDs.
		/// </summary>
		private readonly Dictionary<short, UnitEntry> _unitEntries;

		#endregion

		#region Properties

		#endregion

		#region Functions

		/// <summary>
		/// Creates a new empty balancing data object.
		/// </summary>
		/// <param name="genieFile">The genie file containing the base values the diffs are build upon.</param>
		public BalancingFile(GenieLibrary.GenieFile genieFile)
		{
			// Initialize unit list with base values
			_unitEntries = new Dictionary<short, UnitEntry>();
			foreach(Civ c in genieFile.Civs)
			{
				// Check for units not contained in the unit entry list
				foreach(KeyValuePair<int, Civ.Unit> unitData in c.Units)
				{
					// Unit already contained in unit entry list?
					if(_unitEntries.ContainsKey((short)unitData.Key))
						continue;

					// Create entry
					UnitEntry ue = new UnitEntry();
					ue.HitPoints = new DiffElement<short>(ue, unitData.Value.HitPoints);
					ue.Speed = new DiffElement<float>(ue, unitData.Value.Speed);
					if(unitData.Value.DeadFish != null)
						ue.RotationSpeed = new DiffElement<float>(ue, unitData.Value.DeadFish.RotationSpeed);
					ue.LineOfSight = new DiffElement<float>(ue, unitData.Value.LineOfSight);
					if(unitData.Value.Bird != null)
						ue.SearchRadius = new DiffElement<float>(ue, unitData.Value.Bird.SearchRadius);
					if(unitData.Value.Type50 != null)
						ue.MinRange = new DiffElement<float>(ue, unitData.Value.Type50.MinRange);
					if(unitData.Value.Type50 != null)
						ue.MaxRange = new DiffElement<float>(ue, unitData.Value.Type50.MaxRange);
					if(unitData.Value.Type50 != null)
						ue.DisplayedRange = new DiffElement<float>(ue, unitData.Value.Type50.DisplayedRange);
					if(unitData.Value.Type50 != null)
						ue.ReloadTime = new DiffElement<float>(ue, unitData.Value.Type50.ReloadTime);
					if(unitData.Value.Type50 != null)
						ue.DisplayedReloadTime = new DiffElement<float>(ue, unitData.Value.Type50.DisplayedReloadTime);
					if(unitData.Value.Type50 != null)
						ue.BlastRadius = new DiffElement<float>(ue, unitData.Value.Type50.BlastRadius);
					if(unitData.Value.Type50 != null)
						ue.Attacks = new DiffElement<UnitEntry.AttackArmorEntryList>(ue, new UnitEntry.AttackArmorEntryList(unitData.Value.Type50.Attacks.Select(at => new UnitEntry.AttackArmorEntry
						{
							ArmorClass = at.Key,
							Amount = at.Value
						})));
					if(unitData.Value.Type50 != null)
						ue.DisplayedAttack = new DiffElement<short>(ue, unitData.Value.Type50.DisplayedAttack);
					if(unitData.Value.Creatable != null)
						ue.ProjectileCount = new DiffElement<float>(ue, unitData.Value.Creatable.ProjectileCount);
					if(unitData.Value.Creatable != null)
						ue.ProjectileCountOnFullGarrison = new DiffElement<byte>(ue, unitData.Value.Creatable.ProjectileCountOnFullGarrison);
					if(unitData.Value.Type50 != null)
						ue.ProjectileFrameDelay = new DiffElement<short>(ue, unitData.Value.Type50.ProjectileFrameDelay);
					if(unitData.Value.Type50 != null)
						ue.ProjectileAccuracyPercent = new DiffElement<short>(ue, unitData.Value.Type50.ProjectileAccuracyPercent);
					if(unitData.Value.Type50 != null)
						ue.ProjectileDispersion = new DiffElement<float>(ue, unitData.Value.Type50.ProjectileDispersion);
					if(unitData.Value.Type50 != null)
						ue.ProjectileGraphicDisplacementX = new DiffElement<float>(ue, unitData.Value.Type50.ProjectileGraphicDisplacement[0]);
					if(unitData.Value.Type50 != null)
						ue.ProjectileGraphicDisplacementY = new DiffElement<float>(ue, unitData.Value.Type50.ProjectileGraphicDisplacement[1]);
					if(unitData.Value.Type50 != null)
						ue.ProjectileGraphicDisplacementZ = new DiffElement<float>(ue, unitData.Value.Type50.ProjectileGraphicDisplacement[2]);
					if(unitData.Value.Creatable != null)
						ue.ProjectileSpawningAreaWidth = new DiffElement<float>(ue, unitData.Value.Creatable.ProjectileSpawningAreaWidth);
					if(unitData.Value.Creatable != null)
						ue.ProjectileSpawningAreaHeight = new DiffElement<float>(ue, unitData.Value.Creatable.ProjectileSpawningAreaHeight);
					if(unitData.Value.Creatable != null)
						ue.ProjectileSpawningAreaRandomness = new DiffElement<float>(ue, unitData.Value.Creatable.ProjectileSpawningAreaRandomness);
					if(unitData.Value.Type50 != null)
						ue.Armors = new DiffElement<UnitEntry.AttackArmorEntryList>(ue, new UnitEntry.AttackArmorEntryList(unitData.Value.Type50.Armors.Select(at => new UnitEntry.AttackArmorEntry
						{
							ArmorClass = at.Key,
							Amount = at.Value
						})));
					if(unitData.Value.Type50 != null)
						ue.DisplayedMeleeArmor = new DiffElement<short>(ue, unitData.Value.Type50.DisplayedMeleeArmor);
					if(unitData.Value.Creatable != null)
						ue.DisplayedPierceArmor = new DiffElement<short>(ue, unitData.Value.Creatable.DisplayedPierceArmor);
					ue.GarrisonCapacity = new DiffElement<byte>(ue, unitData.Value.GarrisonCapacity);
					if(unitData.Value.Building != null)
						ue.GarrisonHealRateFactor = new DiffElement<float>(ue, unitData.Value.Building.GarrisonHealRateFactor);
					if(unitData.Value.Creatable != null)
						ue.TrainTime = new DiffElement<short>(ue, unitData.Value.Creatable.TrainTime);
					if(unitData.Value.Creatable != null)
						ue.Cost1 = new DiffElement<UnitEntry.ResourceCostEntry>(ue, new UnitEntry.ResourceCostEntry
						{
							Amount = unitData.Value.Creatable.ResourceCosts[0].Amount,
							Paid = unitData.Value.Creatable.ResourceCosts[0].Paid,
							ResourceType = unitData.Value.Creatable.ResourceCosts[0].Type
						});
					if(unitData.Value.Creatable != null)
						ue.Cost2 = new DiffElement<UnitEntry.ResourceCostEntry>(ue, new UnitEntry.ResourceCostEntry
						{
							Amount = unitData.Value.Creatable.ResourceCosts[1].Amount,
							Paid = unitData.Value.Creatable.ResourceCosts[1].Paid,
							ResourceType = unitData.Value.Creatable.ResourceCosts[1].Type
						});
					if(unitData.Value.Creatable != null)
						ue.Cost3 = new DiffElement<UnitEntry.ResourceCostEntry>(ue, new UnitEntry.ResourceCostEntry
						{
							Amount = unitData.Value.Creatable.ResourceCosts[2].Amount,
							Paid = unitData.Value.Creatable.ResourceCosts[2].Paid,
							ResourceType = unitData.Value.Creatable.ResourceCosts[2].Type
						});

					// Save unit entry
					_unitEntries[(short)unitData.Key] = ue;
				}
			}
		}

		/// <summary>
		/// Loads the balancing file at the given path.
		/// </summary>
		/// <param name="genieFile">The genie file containing the base values the diffs are build upon.</param>
		/// <param name="path">The path to the balancing file.</param>
		public BalancingFile(GenieLibrary.GenieFile genieFile, string path)
			: this(genieFile)
		{
			// Load file into buffer
			IORAMHelper.RAMBuffer buffer = new IORAMHelper.RAMBuffer(path);

			// Check version
			int version = buffer.ReadInteger();
			if(version > Version)
				throw new ArgumentException("The given file was created with a newer version of this program, please consider updating.");

			// Read unit entries
			int unitEntryCount = buffer.ReadInteger();
			for(int i = 0; i < unitEntryCount; ++i)
			{
				// Read entry and merge with existing entry
				short unitId = buffer.ReadShort();
				if(_unitEntries.ContainsKey(unitId))
					_unitEntries[unitId].Read(buffer);
			}
		}

		/// <summary>
		/// Saves the balancing file at the given path.
		/// </summary>
		/// <param name="path">The path where the balancing file shall be saved.</param>
		public void Save(string path)
		{
			// Create buffer
			IORAMHelper.RAMBuffer buffer = new IORAMHelper.RAMBuffer();

			// Write version
			buffer.WriteInteger(Version);

			// Run through unit list and save unit entries
			buffer.WriteInteger(_unitEntries.Count);
			foreach(KeyValuePair<short, UnitEntry> ue in _unitEntries)
			{
				// Are there any changes? => Omit units with no modifications
				if(ue.Value.ModifiedFieldsCount == 0)
					continue;

				// Save ID
				buffer.WriteShort(ue.Key);

				// Save entry data
				ue.Value.Save(buffer);
			}
		}

		/// <summary>
		/// Writes the modifications into the given genie file.
		/// </summary>
		/// <param name="genieFile">The genie file to be modified.</param>
		public void WriteChangesToGenieFile(GenieLibrary.GenieFile genieFile)
		{
			// Apply changes to each civ
			foreach(Civ c in genieFile.Civs)
			{
				// Apply each unit entry
				foreach(KeyValuePair<short, UnitEntry> ue in _unitEntries)
				{
					// Check whether civ has unit
					if(!c.Units.ContainsKey(ue.Key))
						continue;

					// Get corresponding unit
					var unitData = c.Units[ue.Key];

					// Apply all modified members
					if(ue.Value.HitPoints?.Modified ?? false)
						unitData.HitPoints = ue.Value.HitPoints;
					if(ue.Value.Speed?.Modified ?? false)
						unitData.Speed = ue.Value.Speed;
					if(ue.Value.RotationSpeed?.Modified ?? false)
						unitData.DeadFish.RotationSpeed = ue.Value.RotationSpeed;
					if(ue.Value.LineOfSight?.Modified ?? false)
						unitData.LineOfSight = ue.Value.LineOfSight;
					if(ue.Value.SearchRadius?.Modified ?? false)
						unitData.Bird.SearchRadius = ue.Value.SearchRadius;
					if(ue.Value.MinRange?.Modified ?? false)
						unitData.Type50.MinRange = ue.Value.MinRange;
					if(ue.Value.MaxRange?.Modified ?? false)
						unitData.Type50.MaxRange = ue.Value.MaxRange;
					if(ue.Value.DisplayedRange?.Modified ?? false)
						unitData.Type50.DisplayedRange = ue.Value.DisplayedRange;
					if(ue.Value.ReloadTime?.Modified ?? false)
						unitData.Type50.ReloadTime = ue.Value.ReloadTime;
					if(ue.Value.DisplayedReloadTime?.Modified ?? false)
						unitData.Type50.DisplayedReloadTime = ue.Value.DisplayedReloadTime;
					if(ue.Value.BlastRadius?.Modified ?? false)
						unitData.Type50.BlastRadius = ue.Value.BlastRadius;
					if(ue.Value.Attacks?.Modified ?? false)
						unitData.Type50.Attacks = ue.Value.Attacks.Value.ToDictionary(at => at.ArmorClass, at => at.Amount);
					if(ue.Value.DisplayedAttack?.Modified ?? false)
						unitData.Type50.DisplayedAttack = ue.Value.DisplayedAttack;
					if(ue.Value.ProjectileCount?.Modified ?? false)
						unitData.Creatable.ProjectileCount = ue.Value.ProjectileCount;
					if(ue.Value.ProjectileCountOnFullGarrison?.Modified ?? false)
						unitData.Creatable.ProjectileCountOnFullGarrison = ue.Value.ProjectileCountOnFullGarrison;
					if(ue.Value.ProjectileFrameDelay?.Modified ?? false)
						unitData.Type50.ProjectileFrameDelay = ue.Value.ProjectileFrameDelay;
					if(ue.Value.ProjectileAccuracyPercent?.Modified ?? false)
						unitData.Type50.ProjectileAccuracyPercent = ue.Value.ProjectileAccuracyPercent;
					if(ue.Value.ProjectileDispersion?.Modified ?? false)
						unitData.Type50.ProjectileDispersion = ue.Value.ProjectileDispersion;
					if(ue.Value.ProjectileGraphicDisplacementX?.Modified ?? false)
						unitData.Type50.ProjectileGraphicDisplacement[0] = ue.Value.ProjectileGraphicDisplacementX;
					if(ue.Value.ProjectileGraphicDisplacementY?.Modified ?? false)
						unitData.Type50.ProjectileGraphicDisplacement[1] = ue.Value.ProjectileGraphicDisplacementY;
					if(ue.Value.ProjectileGraphicDisplacementZ?.Modified ?? false)
						unitData.Type50.ProjectileGraphicDisplacement[2] = ue.Value.ProjectileGraphicDisplacementZ;
					if(ue.Value.ProjectileSpawningAreaWidth?.Modified ?? false)
						unitData.Creatable.ProjectileSpawningAreaWidth = ue.Value.ProjectileSpawningAreaWidth;
					if(ue.Value.ProjectileSpawningAreaHeight?.Modified ?? false)
						unitData.Creatable.ProjectileSpawningAreaHeight = ue.Value.ProjectileSpawningAreaHeight;
					if(ue.Value.ProjectileSpawningAreaRandomness?.Modified ?? false)
						unitData.Creatable.ProjectileSpawningAreaRandomness = ue.Value.ProjectileSpawningAreaRandomness;
					if(ue.Value.Armors?.Modified ?? false)
						unitData.Type50.Armors = ue.Value.Armors.Value.ToDictionary(am => am.ArmorClass, am => am.Amount);
					if(ue.Value.DisplayedMeleeArmor?.Modified ?? false)
						unitData.Type50.DisplayedMeleeArmor = ue.Value.DisplayedMeleeArmor;
					if(ue.Value.DisplayedPierceArmor?.Modified ?? false)
						unitData.Creatable.DisplayedPierceArmor = ue.Value.DisplayedPierceArmor;
					if(ue.Value.GarrisonCapacity?.Modified ?? false)
						unitData.GarrisonCapacity = ue.Value.GarrisonCapacity;
					if(ue.Value.GarrisonHealRateFactor?.Modified ?? false)
						unitData.Building.GarrisonHealRateFactor = ue.Value.GarrisonHealRateFactor;
					if(ue.Value.TrainTime?.Modified ?? false)
						unitData.Creatable.TrainTime = ue.Value.TrainTime;
					if(ue.Value.Cost1?.Modified ?? false)
						unitData.Creatable.ResourceCosts[0] = new GenieLibrary.IGenieDataElement.ResourceTuple<short, short, short>
						{
							Amount = ue.Value.Cost1.Value.Amount,
							Paid = ue.Value.Cost1.Value.Paid,
							Type = ue.Value.Cost1.Value.ResourceType
						};
					if(ue.Value.Cost2?.Modified ?? false)
						unitData.Creatable.ResourceCosts[1] = new GenieLibrary.IGenieDataElement.ResourceTuple<short, short, short>
						{
							Amount = ue.Value.Cost2.Value.Amount,
							Paid = ue.Value.Cost2.Value.Paid,
							Type = ue.Value.Cost2.Value.ResourceType
						};
					if(ue.Value.Cost3?.Modified ?? false)
						unitData.Creatable.ResourceCosts[2] = new GenieLibrary.IGenieDataElement.ResourceTuple<short, short, short>
						{
							Amount = ue.Value.Cost3.Value.Amount,
							Paid = ue.Value.Cost3.Value.Paid,
							Type = ue.Value.Cost3.Value.ResourceType
						};
				}
			}
		}

		#endregion

		#region Sub classes

		/// <summary>
		/// Interface for classes containing DiffElement objects.
		/// This interface provides a property to count modified elements.
		/// </summary>
		public interface IDiffElementContainer
		{
			/// <summary>
			/// The count of modified public fields.
			/// </summary>
			int ModifiedFieldsCount { get; set; }
		}

		/// <summary>
		/// Defines one unit data element.
		/// The internal modification counter should be kept consistent!
		/// </summary>
		public class UnitEntry : IDiffElementContainer
		{
			#region Fields

			#endregion

			#region Public Fields

			// Main stats
			public DiffElement<short> HitPoints;
			public DiffElement<float> Speed;
			public DiffElement<float> RotationSpeed;
			public DiffElement<float> LineOfSight;
			public DiffElement<float> SearchRadius;

			// Attack values
			public DiffElement<float> MinRange;
			public DiffElement<float> MaxRange;
			public DiffElement<float> DisplayedRange;
			public DiffElement<float> ReloadTime;
			public DiffElement<float> DisplayedReloadTime;
			public DiffElement<float> BlastRadius;
			public DiffElement<AttackArmorEntryList> Attacks;
			public DiffElement<short> DisplayedAttack;

			// Projectile data
			public DiffElement<float> ProjectileCount;
			public DiffElement<byte> ProjectileCountOnFullGarrison;
			public DiffElement<short> ProjectileFrameDelay;
			public DiffElement<short> ProjectileAccuracyPercent;
			public DiffElement<float> ProjectileDispersion;
			public DiffElement<float> ProjectileGraphicDisplacementX;
			public DiffElement<float> ProjectileGraphicDisplacementY;
			public DiffElement<float> ProjectileGraphicDisplacementZ;
			public DiffElement<float> ProjectileSpawningAreaWidth;
			public DiffElement<float> ProjectileSpawningAreaHeight;
			public DiffElement<float> ProjectileSpawningAreaRandomness;

			// Armor values
			public DiffElement<AttackArmorEntryList> Armors;
			public DiffElement<short> DisplayedMeleeArmor;
			public DiffElement<short> DisplayedPierceArmor;

			// Garrison values
			public DiffElement<byte> GarrisonCapacity;
			public DiffElement<float> GarrisonHealRateFactor;

			// Creation values
			public DiffElement<short> TrainTime;
			public DiffElement<ResourceCostEntry> Cost1;
			public DiffElement<ResourceCostEntry> Cost2;
			public DiffElement<ResourceCostEntry> Cost3;

			#endregion

			#region Properties

			/// <summary>
			/// The count of modified public fields.
			/// Should be kept consistent with the actual "modified" flags!
			/// </summary>
			public int ModifiedFieldsCount { get; set; } = 0;

			#endregion

			#region Functions

			#region Read

			/// <summary>
			/// Reads the whole unit entry from the given buffer.
			/// </summary>
			/// <param name="buffer">The buffer containing the unit entry data.</param>
			public void Read(IORAMHelper.RAMBuffer buffer)
			{
				// Reset counter
				ModifiedFieldsCount = 0;

				// Read members
				ReadMember(buffer, ref HitPoints);
				ReadMember(buffer, ref Speed);
				ReadMember(buffer, ref RotationSpeed);
				ReadMember(buffer, ref LineOfSight);
				ReadMember(buffer, ref SearchRadius);

				ReadMember(buffer, ref MinRange);
				ReadMember(buffer, ref MaxRange);
				ReadMember(buffer, ref DisplayedRange);
				ReadMember(buffer, ref ReloadTime);
				ReadMember(buffer, ref DisplayedReloadTime);
				ReadMember(buffer, ref BlastRadius);
				ReadMember(buffer, ref Attacks);
				ReadMember(buffer, ref DisplayedAttack);

				ReadMember(buffer, ref ProjectileCount);
				ReadMember(buffer, ref ProjectileCountOnFullGarrison);
				ReadMember(buffer, ref ProjectileFrameDelay);
				ReadMember(buffer, ref ProjectileAccuracyPercent);
				ReadMember(buffer, ref ProjectileDispersion);
				ReadMember(buffer, ref ProjectileGraphicDisplacementX);
				ReadMember(buffer, ref ProjectileGraphicDisplacementY);
				ReadMember(buffer, ref ProjectileGraphicDisplacementZ);
				ReadMember(buffer, ref ProjectileSpawningAreaWidth);
				ReadMember(buffer, ref ProjectileSpawningAreaHeight);
				ReadMember(buffer, ref ProjectileSpawningAreaRandomness);

				ReadMember(buffer, ref Armors);
				ReadMember(buffer, ref DisplayedMeleeArmor);
				ReadMember(buffer, ref DisplayedPierceArmor);

				ReadMember(buffer, ref GarrisonCapacity);
				ReadMember(buffer, ref GarrisonHealRateFactor);

				ReadMember(buffer, ref TrainTime);
				ReadMember(buffer, ref Cost1);
				ReadMember(buffer, ref Cost2);
				ReadMember(buffer, ref Cost3);
			}

			/// <summary>
			/// Reads a byte member from the given buffer.
			/// The modification flag is read also, the internal modification counter is incremented.
			/// </summary>
			/// <param name="buffer">The buffer where the member shall be read from.</param>
			/// <param name="member">The member where the read data shall be stored.</param>
			private void ReadMember(IORAMHelper.RAMBuffer buffer, ref DiffElement<byte> member)
			{
				// Member must be defined
				if(member == null)
					return;

				// Read member if modified
				if(buffer.ReadByte() == 1)
				{
					// Read member
					member.Value = buffer.ReadByte();

					// Increment counter
					++ModifiedFieldsCount;
				}
			}

			/// <summary>
			/// Reads a short member from the given buffer.
			/// The modification flag is read also.
			/// </summary>
			/// <param name="buffer">The buffer where the member shall be read from.</param>
			/// <param name="member">The member where the read data shall be stored.</param>
			private void ReadMember(IORAMHelper.RAMBuffer buffer, ref DiffElement<short> member)
			{
				// Member must be defined
				if(member == null)
					return;

				// Read member if modified
				if(buffer.ReadByte() == 1)
				{
					// Read member
					member.Value = buffer.ReadShort();

					// Increment counter
					++ModifiedFieldsCount;
				}
			}

			/// <summary>
			/// Reads a floating point member from the given buffer.
			/// The modification flag is read also.
			/// </summary>
			/// <param name="buffer">The buffer where the member shall be read from.</param>
			/// <param name="member">The member where the read data shall be stored.</param>
			private void ReadMember(IORAMHelper.RAMBuffer buffer, ref DiffElement<float> member)
			{
				// Member must be defined
				if(member == null)
					return;

				// Read member if modified
				if(buffer.ReadByte() == 1)
				{
					// Read member
					member.Value = buffer.ReadFloat();

					// Increment counter
					++ModifiedFieldsCount;
				}
			}

			/// <summary>
			/// Reads a attack/armor entry list member from the given buffer.
			/// The modification flag is read also.
			/// </summary>
			/// <param name="buffer">The buffer where the member shall be read from.</param>
			/// <param name="member">The member where the read data shall be stored.</param>
			private void ReadMember(IORAMHelper.RAMBuffer buffer, ref DiffElement<AttackArmorEntryList> member)
			{
				// Member must be defined
				if(member == null)
					return;

				// Read member if modified
				if(buffer.ReadByte() == 1)
				{
					// Read entries
					int count = buffer.ReadInteger();
					member.Value = new AttackArmorEntryList(count);
					for(int i = 0; i < count; ++i)
						member.Value.Add(new AttackArmorEntry
						{
							ArmorClass = buffer.ReadUShort(),
							Amount = buffer.ReadUShort()
						});

					// Increment counter
					++ModifiedFieldsCount;
				}
			}

			/// <summary>
			/// Reads a resource cost member from the given buffer.
			/// The modification flag is read also.
			/// </summary>
			/// <param name="buffer">The buffer where the member shall be read from.</param>
			/// <param name="member">The member where the read data shall be stored.</param>
			private void ReadMember(IORAMHelper.RAMBuffer buffer, ref DiffElement<ResourceCostEntry> member)
			{
				// Member must be defined
				if(member == null)
					return;

				// Read member if modified
				if(buffer.ReadByte() == 1)
				{
					// Read member values
					member.Value.ResourceType = buffer.ReadShort();
					member.Value.Amount = buffer.ReadShort();
					member.Value.Paid = buffer.ReadShort();

					// Increment counter
					++ModifiedFieldsCount;
				}
			}

			#endregion

			#region Save

			/// <summary>
			/// Saves the whole unit entry into the given buffer.
			/// </summary>
			/// <param name="buffer">The buffer for the members to written to.</param>
			public void Save(IORAMHelper.RAMBuffer buffer)
			{
				// Write members
				SaveMember(buffer, HitPoints);
				SaveMember(buffer, Speed);
				SaveMember(buffer, RotationSpeed);
				SaveMember(buffer, LineOfSight);
				SaveMember(buffer, SearchRadius);

				SaveMember(buffer, MinRange);
				SaveMember(buffer, MaxRange);
				SaveMember(buffer, DisplayedRange);
				SaveMember(buffer, ReloadTime);
				SaveMember(buffer, DisplayedReloadTime);
				SaveMember(buffer, BlastRadius);
				SaveMember(buffer, Attacks);
				SaveMember(buffer, DisplayedAttack);

				SaveMember(buffer, ProjectileCount);
				SaveMember(buffer, ProjectileCountOnFullGarrison);
				SaveMember(buffer, ProjectileFrameDelay);
				SaveMember(buffer, ProjectileAccuracyPercent);
				SaveMember(buffer, ProjectileDispersion);
				SaveMember(buffer, ProjectileGraphicDisplacementX);
				SaveMember(buffer, ProjectileGraphicDisplacementY);
				SaveMember(buffer, ProjectileGraphicDisplacementZ);
				SaveMember(buffer, ProjectileSpawningAreaWidth);
				SaveMember(buffer, ProjectileSpawningAreaHeight);
				SaveMember(buffer, ProjectileSpawningAreaRandomness);

				SaveMember(buffer, Armors);
				SaveMember(buffer, DisplayedMeleeArmor);
				SaveMember(buffer, DisplayedPierceArmor);

				SaveMember(buffer, GarrisonCapacity);
				SaveMember(buffer, GarrisonHealRateFactor);

				SaveMember(buffer, TrainTime);
				SaveMember(buffer, Cost1);
				SaveMember(buffer, Cost2);
				SaveMember(buffer, Cost3);
			}

			/// <summary>
			/// Saves the given byte member into the given buffer.
			/// The modification flag is written also.
			/// </summary>
			/// <param name="buffer">The buffer for the member to be written to.</param>
			/// <param name="member">The member to be written.</param>
			private static void SaveMember(IORAMHelper.RAMBuffer buffer, DiffElement<byte> member)
			{
				// Member must be defined
				if(member == null)
					return;

				// Write modification flag
				buffer.WriteByte((byte)(member.Modified ? 1 : 0));

				// Write member
				if(member.Modified)
					buffer.WriteByte(member);
			}

			/// <summary>
			/// Saves the given short member into the given buffer.
			/// The modification flag is written also.
			/// </summary>
			/// <param name="buffer">The buffer for the member to written to.</param>
			/// <param name="member">The member to be written.</param>
			private static void SaveMember(IORAMHelper.RAMBuffer buffer, DiffElement<short> member)
			{
				// Member must be defined
				if(member == null)
					return;

				// Write modification flag
				buffer.WriteByte((byte)(member.Modified ? 1 : 0));

				// Write member
				if(member.Modified)
					buffer.WriteShort(member);
			}

			/// <summary>
			/// Saves the given floating point member into the given buffer.
			/// The modification flag is written also.
			/// </summary>
			/// <param name="buffer">The buffer for the member to written to.</param>
			/// <param name="member">The member to be written.</param>
			private static void SaveMember(IORAMHelper.RAMBuffer buffer, DiffElement<float> member)
			{
				// Member must be defined
				if(member == null)
					return;

				// Write modification flag
				buffer.WriteByte((byte)(member.Modified ? 1 : 0));

				// Write member
				if(member.Modified)
					buffer.WriteFloat(member);
			}

			/// <summary>
			/// Saves the given attack/armor entry list member into the given buffer.
			/// The modification flag is written also.
			/// </summary>
			/// <param name="buffer">The buffer for the member to written to.</param>
			/// <param name="member">The member to be written.</param>
			private static void SaveMember(IORAMHelper.RAMBuffer buffer, DiffElement<AttackArmorEntryList> member)
			{
				// Member must be defined
				if(member == null)
					return;

				// Write modification flag
				buffer.WriteByte((byte)(member.Modified ? 1 : 0));

				// Write member
				if(member.Modified)
				{
					// Write count
					buffer.WriteInteger(member.Value.Count);

					// Write members
					foreach(AttackArmorEntry aae in member.Value)
					{
						// Write member fields
						buffer.WriteUShort(aae.ArmorClass);
						buffer.WriteUShort(aae.Amount);
					}
				}
			}

			/// <summary>
			/// Saves the given resource cost member into the given buffer.
			/// The modification flag is written also.
			/// </summary>
			/// <param name="buffer">The buffer for the member to written to.</param>
			/// <param name="member">The member to be written.</param>
			private static void SaveMember(IORAMHelper.RAMBuffer buffer, DiffElement<ResourceCostEntry> member)
			{
				// Member must be defined
				if(member == null)
					return;

				// Write modification flag
				buffer.WriteByte((byte)(member.Modified ? 1 : 0));

				// Write member
				if(member.Modified)
				{
					// Write member fields
					buffer.WriteShort(member.Value.ResourceType);
					buffer.WriteShort(member.Value.Amount);
					buffer.WriteShort(member.Value.Paid);
				}
			}

			#endregion

			#endregion

			#region Sub classes

			/// <summary>
			/// Contains attack/armor class entries. This is simply a list overload with equality comparing functionality, as needed for the DiffElement class.
			/// </summary>
			public class AttackArmorEntryList : List<AttackArmorEntry>, IEquatable<AttackArmorEntryList>
			{
				public AttackArmorEntryList(int capacity)
					: base(capacity) { }
				public AttackArmorEntryList(IEnumerable<AttackArmorEntry> collection)
					: base(collection) { }
				public bool Equals(AttackArmorEntryList other) => other != null && new HashSet<AttackArmorEntry>(this).SetEquals(other);
			}

			/// <summary>
			/// Represents an attack/armor class entry.
			/// </summary>
			public class AttackArmorEntry
			{
				public ushort ArmorClass;
				public ushort Amount;

				public override int GetHashCode() => ArmorClass << 16 | Amount;
			}

			/// <summary>
			/// Represents an resource cost entry.
			/// </summary>
			public class ResourceCostEntry : IEquatable<ResourceCostEntry>
			{
				public short ResourceType;
				public short Amount;
				public short Paid;

				public bool Equals(ResourceCostEntry other) => other != null && ResourceType == other.ResourceType && Amount == other.Amount && Paid == other.Paid;
			}

			#endregion
		}

		/// <summary>
		/// Represents an arbitrary data element including a bool variable determining whether the element is in a "modified" state.
		/// The base value from the base genie file is used for automatic setting of the "modified" flag.
		/// </summary>
		/// <typeparam name="T">The type of the data element.</typeparam>
		public class DiffElement<T> where T : IEquatable<T>
		{
			/// <summary>
			/// The object containing the diff element.
			/// </summary>
			private readonly IDiffElementContainer _owningObject;

			/// <summary>
			/// Determines whether the data element has been modified.
			/// </summary>
			private bool _modified;

			/// <summary>
			/// Determines whether the data element has been modified.
			/// </summary>
			public bool Modified
			{
				get { return _modified; }
				private set
				{
					// Update counter in owning class, if the flag has changed
					if(_modified != value)
						if(value)
							++_owningObject.ModifiedFieldsCount;
						else
							--_owningObject.ModifiedFieldsCount;

					// Update flag
					_modified = value;
				}
			}

			/// <summary>
			/// The value stored in this instance.
			/// </summary>
			private T _value;

			/// <summary>
			/// The value stored in this instance.
			/// </summary>
			public T Value
			{
				get { return _value; }
				set
				{
					// Update value
					_value = value;

					// Set "modified" flag properly
					Modified = _value.Equals(_baseValue);
				}
			}

			/// <summary>
			/// The base value of the data element stored in this instance.
			/// </summary>
			private readonly T _baseValue;

			/// <summary>
			/// Initializes a new data diff element object with the given base value.
			/// </summary>
			/// <param name="owningObject">The object containing the diff element.</param>
			/// <param name="baseValue">The base value of the diff element.</param>
			public DiffElement(IDiffElementContainer owningObject, T baseValue)
			{
				// Save owning object
				_owningObject = owningObject;

				// Remember base value, set as current value
				_baseValue = baseValue;
				_value = baseValue;

				// Element is not modified
				Modified = false;
			}

			/// <summary>
			/// Returns the current value stored in the given data diff element object.
			/// </summary>
			/// <param name="diffElement">The data diff element object whose value shall be retrieved.</param>
			public static implicit operator T(DiffElement<T> diffElement)
			{
				// Return internal data element
				return diffElement._value;
			}

			public override string ToString()
			{
				// TODO delete, for easier debugging only
				return $"_value = {_value}";// [{typeof(T)}]";
			}
		}

		#endregion
	}
}