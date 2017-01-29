using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DRSLibrary;
using GenieLibrary;
using GenieLibrary.DataElements;
using IORAMHelper;
using SLPLoader;
using MessageBox = System.Windows.MessageBox;
using BitmapLibrary;
using System.Windows.Media.Composition;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace AoEBalancingTool
{
	/// <summary>
	/// Renders the current projectile positions.
	/// </summary>
	public partial class ProjectileWindow : INotifyPropertyChanged
	{
		#region Constants

		/// <summary>
		/// The distance of the top and bottom corners of a tile to its center.
		/// </summary>
		private const double TileVerticalOffset = 11 * 2.2360679775; // = 11 * sqrt(5)

		/// <summary>
		/// The distance of the left and right corners of a tile to its center.
		/// </summary>
		private const double TileHorizontalOffset = 22 * 2.2360679775; // = 22 * sqrt(5)

		/// <summary>
		/// The length of a tile's side.
		/// </summary>
		private const double TileSideLength = 55; // = sqrt((11 * sqrt(5))^2 + (22 * sqrt(5))^2)

		/// <summary>
		/// The background color of the projectile area shapes.
		/// </summary>
		private readonly Color ProjectileAreaColor = Color.FromArgb(255, 49, 247, 73);

		/// <summary>
		/// The background color of the projectile area shapes with transparency.
		/// </summary>
		private readonly Color ProjectileAreaTransparentColor = Color.FromArgb(100, 49, 247, 73);

		#endregion

		#region Variables

		/// <summary>
		/// The default 50500 palette used for rendering the SLP frames.
		/// </summary>
		private static readonly ColorTable Pal50500 = new ColorTable(new JASCPalette(new RAMBuffer(Properties.Resources.pal50500)));

		/// <summary>
		/// The base genie file where the rendering data is derived from.
		/// </summary>
		private GenieFile _genieFile = null;

		/// <summary>
		/// 1st priority DRS file.
		/// </summary>
		private DRSFile _drs1;

		/// <summary>
		/// 2nd priority DRS file.
		/// </summary>
		private DRSFile _drs2;

		/// <summary>
		/// The path to the 1st priority DRS file.
		/// </summary>
		private string _drs1Path = "";

		/// <summary>
		/// The path to the 2nd priority DRS file.
		/// </summary>
		private string _drs2Path = "";

		/// <summary>
		/// Tells whether DRS files are loaded.
		/// </summary>
		private bool _drsFilesLoaded;

		/// <summary>
		/// The currently selected unit.
		/// </summary>
		private KeyValuePair<short, UnitEntry> _currentUnitEntry;

		/// <summary>
		/// The data of the currently selected unit.
		/// </summary>
		private Civ.Unit _currentUnitData;

		/// <summary>
		/// The rendered graphic currently selected unit.
		/// </summary>
		private Graphic _currentUnitGraphic;

		/// <summary>
		/// The graphic deltas being rendered alongside the base graphic.
		/// </summary>
		private readonly List<Graphic.GraphicDelta> _renderedDeltas = new List<Graphic.GraphicDelta>();

		/// <summary>
		/// The image objects drawn onto the render panel. The actual bitmaps are only assigned to these image objects.
		/// </summary>
		private readonly List<Image> _drawnImages = new List<Image>();

		/// <summary>
		/// The already computed unit frames for caching purposes, indexed by their frame ID.
		/// </summary>
		private readonly Dictionary<int, ImageSource> _precomputedUnitFrames = new Dictionary<int, ImageSource>();

		/// <summary>
		/// The SLPs used by the current graphic.
		/// </summary>
		private readonly Dictionary<ushort, SLPFile> _slps = new Dictionary<ushort, SLPFile>();

		/// <summary>
		/// The maximum frame offsets in directions left, top, right and bottom.
		/// </summary>
		private OffsetData _maxFrameOffsets = new OffsetData(0, 0, 0, 0);

		/// <summary>
		/// The anchors of the unit frames.
		/// </summary>
		private readonly List<Point> _anchors = new List<Point>();

		/// <summary>
		/// The shapes drawn at the frame anchor points.
		/// </summary>
		private readonly List<Ellipse> _anchorPointShapes = new List<Ellipse>();

		/// <summary>
		/// The shapes drawn at the projectile spawn areas.
		/// </summary>
		private readonly List<Rectangle> _projectileAreaShapes = new List<Rectangle>();

		/// <summary>
		/// The offsets for additional user-defined translations of the unit angle frames.
		/// </summary>
		private readonly List<Point> _userDefinedFrameOffsets = new List<Point>();

		/// <summary>
		/// The angle index of the currently user dragged item.
		/// </summary>
		private int _currentDragItemIndex = -1;

		/// <summary>
		/// The last mouse move update position.
		/// </summary>
		private Point _lastMouseMovePoint;

		#endregion

		#region Functions

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectileWindow()
		{
			// Initialize controls
			InitializeComponent();
			DataContext = this;

			// Read values from application settings
			Drs1Path = Properties.Settings.Default.Drs1Path;
			Drs2Path = Properties.Settings.Default.Drs2Path;
		}

		/// <summary>
		/// Updates the render panel when a new unit is loaded.
		/// </summary>
		private void UpdateUnitRenderData()
		{
			// Are any DRS files loaded?
			if(_drs1 == null && _drs2 == null || _genieFile == null)
				return;

			// If no unit is given or the unit does not have projectile data, clear panel
			if(CurrentUnitEntry.Key < 0 || CurrentUnitEntry.Value?.ProjectileCount == null)
			{
				// Clear and abort
				_renderPanel.Children.Clear();
				return;
			}

			// Clear lists
			_renderPanel.Children.Clear();
			_renderedDeltas.Clear();
			_drawnImages.Clear();
			_precomputedUnitFrames.Clear();
			_slps.Clear();
			_anchors.Clear();
			_anchorPointShapes.Clear();
			_projectileAreaShapes.Clear();

			// Reset frame offsets
			_maxFrameOffsets = new OffsetData(0, 0, 0, 0);

			// Find unit data
			// There must be at least one civ where this unit is defined, else the BalancingFile class would consist only of null members
			foreach(Civ c in _genieFile.Civs)
				if(c.UnitPointers[CurrentUnitEntry.Key] > 0)
				{
					// Unit is defined, use data
					_currentUnitData = c.Units[CurrentUnitEntry.Key];
					break;
				}

			// Find graphic
			if(_currentUnitData.Type50?.AttackGraphic >= 0 && _genieFile.GraphicPointers[_currentUnitData.Type50.AttackGraphic] > 0)
				_currentUnitGraphic = _genieFile.Graphics[_currentUnitData.Type50.AttackGraphic];
			else if(_currentUnitData.StandingGraphic1 >= 0 && _genieFile.GraphicPointers[_currentUnitData.StandingGraphic1] > 0)
				_currentUnitGraphic = _genieFile.Graphics[_currentUnitData.StandingGraphic1];
			else
			{
				// Abort, no graphic available
				_currentUnitGraphic = null;
				return;
			}

			// Find deltas; use only deltas that have graphic ID -1 or an existing SLP
			foreach(var currDelta in _currentUnitGraphic.Deltas)
				if(currDelta.GraphicID == -1 ||
					(currDelta.GraphicID >= 0 && _genieFile.GraphicPointers[currDelta.GraphicID] > 0 &&
					((_drs1?.ResourceExists((uint)_genieFile.Graphics[currDelta.GraphicID].SLP) ?? false)
					|| (_drs2?.ResourceExists((uint)_genieFile.Graphics[currDelta.GraphicID].SLP) ?? false))))
					_renderedDeltas.Add(currDelta);

			// Load needed SLPs
			Action<ushort> loadSlpFromDrs = (slpId) =>
			{
				// Get SLP from DRS by priority
				if(_slps.ContainsKey(slpId))
					return;
				if(_drs1?.ResourceExists(slpId) ?? false)
					_slps[slpId] = new SLPFile(new RAMBuffer(_drs1.GetResourceData(slpId)));
				else if(_drs2?.ResourceExists(slpId) ?? false)
					_slps[slpId] = new SLPFile(new RAMBuffer(_drs2.GetResourceData(slpId)));
			};
			if(_currentUnitGraphic.SLP >= 0)
				loadSlpFromDrs((ushort)_currentUnitGraphic.SLP);
			foreach(var currDelta in _renderedDeltas)
				if(currDelta.GraphicID >= 0)
					loadSlpFromDrs((ushort)_genieFile.Graphics[currDelta.GraphicID].SLP);

			// Create shape objects depending on angle count (ignore mirrored sides)
			// Each shape gets an event handler for mouse events, such that it can be dragged around by the used
			int effectiveAngleCount = (_currentUnitGraphic.MirroringMode > 0 ? _currentUnitGraphic.AngleCount / 2 + 1 : _currentUnitGraphic.AngleCount);
			for(int a = 0; a < effectiveAngleCount; ++a)
			{
				// Create image
				Image img = new Image
				{
					Tag = a
				};
				img.MouseLeftButtonDown += _shape_OnMouseLeftButtonDown;
				img.MouseLeftButtonUp += _shape_OnMouseLeftButtonUp;
				_drawnImages.Add(img);
				_renderPanel.Children.Add(img);

				// Create anchor point list items
				_anchors.Add(new Point());

				// Create anchor point shape
				Ellipse anchorEllipse = new Ellipse
				{
					Fill = Brushes.Magenta,
					Width = 3,
					Height = 3,
					Tag = a
				};
				anchorEllipse.MouseLeftButtonDown += _shape_OnMouseLeftButtonDown;
				anchorEllipse.MouseLeftButtonUp += _shape_OnMouseLeftButtonUp;
				_anchorPointShapes.Add(anchorEllipse);
				_renderPanel.Children.Add(anchorEllipse);

				// Create projectile area shape
				Rectangle projectileRectangle = new Rectangle
				{
					Fill = new SolidColorBrush(ProjectileAreaTransparentColor),
					Width = TileSideLength,
					Height = TileSideLength,
					Tag = a
				};
				projectileRectangle.MouseLeftButtonDown += _shape_OnMouseLeftButtonDown;
				projectileRectangle.MouseLeftButtonUp += _shape_OnMouseLeftButtonUp;
				projectileRectangle.RenderTransform = new RotateTransform(a * 45, projectileRectangle.Width / 2, projectileRectangle.Height / 2);
				_projectileAreaShapes.Add(projectileRectangle);
				_renderPanel.Children.Add(projectileRectangle);
			}

			// Initialize user defined offsets
			while(_userDefinedFrameOffsets.Count < effectiveAngleCount)
				_userDefinedFrameOffsets.Add(new Point());

			// Show initial frame
			UpdateDisplayedFrames();
		}

		/// <summary>
		/// Updates the currently displayed frames when the projectile frame delay is changed.
		/// </summary>
		private void UpdateDisplayedFrames()
		{
			// Are any DRS files loaded? Are there drawable frames?
			if(_drs1 == null && _drs2 == null || _drawnImages.Count == 0)
				return;

			// Ensure frame delay is in range
			int frameDelay = CurrentUnitEntry.Value.ProjectileFrameDelay % _currentUnitGraphic.FrameCount;

			// Generate and assign frames
			for(short a = 0; a < _drawnImages.Count; ++a)
			{
				// Get frame
				int frameId = frameDelay + a * _currentUnitGraphic.FrameCount;
				if(_currentUnitGraphic.SLP >= 0 && _slps.ContainsKey((ushort)_currentUnitGraphic.SLP))
				{
					// Load and store bitmap
					if(!_precomputedUnitFrames.ContainsKey(frameId))
					{
						// TODO Deltas
						_precomputedUnitFrames[frameId] =
							_slps[(ushort)_currentUnitGraphic.SLP].getFrameAsBitmap
							(
								(uint)frameId,
								Pal50500,
								SLPFile.Masks.Graphic,
								System.Drawing.Color.FromArgb(0, 0, 0, 0),
								System.Drawing.Color.FromArgb(100, 100, 100, 100)
							).ToImageSource();
					}

					// Get frame anchors
					int frameAnchorX = _slps[(ushort)_currentUnitGraphic.SLP]._frameInformationHeaders[frameId].AnchorX;
					int frameAnchorY = _slps[(ushort)_currentUnitGraphic.SLP]._frameInformationHeaders[frameId].AnchorY;

					// Assign to drawn image objects
					_drawnImages[a].Source = _precomputedUnitFrames[frameId];
					_drawnImages[a].Width = _precomputedUnitFrames[frameId].Width;
					_drawnImages[a].Height = _precomputedUnitFrames[frameId].Height;
					_anchors[a] = new Point(frameAnchorX, frameAnchorY); // Frame anchor point

					// Calculate bounds including the anchor point
					_maxFrameOffsets.Left = Math.Max(_maxFrameOffsets.Left, frameAnchorX);
					_maxFrameOffsets.Right = Math.Max(_maxFrameOffsets.Right, (int)_precomputedUnitFrames[frameId].Width - frameAnchorX);
					_maxFrameOffsets.Top = Math.Max(_maxFrameOffsets.Top, frameAnchorY);
					_maxFrameOffsets.Bottom = Math.Max(_maxFrameOffsets.Bottom, (int)_precomputedUnitFrames[frameId].Width - frameAnchorY);
				}
				else
					_drawnImages[a].Source = null;
			}

			// Update positions
			UpdatePositions();
		}

		/// <summary>
		/// Updates the positions of all rendered elements.
		/// </summary>
		private void UpdatePositions()
		{
			// Calculate frame bounds
			int maxFrameWidth = _maxFrameOffsets.Left + _maxFrameOffsets.Right;
			int maxFrameHeight = _maxFrameOffsets.Top + _maxFrameOffsets.Bottom;
			if(maxFrameWidth == 0)
				maxFrameWidth = 1; // Prevent division by zero

			// Reposition images and shapes
			int framesPerRow = (int)_renderPanel.ActualWidth / maxFrameWidth;
			for(int i = 0; i < _drawnImages.Count; ++i)
			{
				// Get image and its anchor
				Image img = _drawnImages[i];
				Point anchor = _anchors[i];
				Point userFrameOffset = _userDefinedFrameOffsets[i];

				// Update image position
				Canvas.SetLeft(img, (i % framesPerRow) * maxFrameWidth + userFrameOffset.X + _maxFrameOffsets.Left - anchor.X);
				Canvas.SetTop(img, (i / framesPerRow) * maxFrameHeight + userFrameOffset.Y + _maxFrameOffsets.Top - anchor.Y);

				// Update anchor point position
				Canvas.SetLeft(_anchorPointShapes[i], (i % framesPerRow) * maxFrameWidth + userFrameOffset.X + _maxFrameOffsets.Left);
				Canvas.SetTop(_anchorPointShapes[i], (i / framesPerRow) * maxFrameHeight + userFrameOffset.Y + _maxFrameOffsets.Top);

				// Update projectile area position
				Point projPos = new Point();
				int angle = i;
				if(_currentUnitGraphic.AngleCount > 8)
					angle /= (_currentUnitGraphic.AngleCount / 8); // Normalize axis index, such that at most 8 axis exist
				if(angle % 2 == 0)
				{
					// Determine X and Y
					double projX = 0.0;
					double projY = -_currentUnitEntry.Value.ProjectileGraphicDisplacementZ;
					if(angle == 0 || angle == 4)
					{
						// Direction down/up
						projX -= (angle - 2) / 2 * _currentUnitEntry.Value.ProjectileGraphicDisplacementX;
						projY -= (angle - 2) / 2 * _currentUnitEntry.Value.ProjectileGraphicDisplacementY;
					}
					else if(angle == 2 || angle == 6)
					{
						// Direction left/right
						projX += (angle - 4) / 2 * _currentUnitEntry.Value.ProjectileGraphicDisplacementY;
						projY += (angle - 4) / 2 * _currentUnitEntry.Value.ProjectileGraphicDisplacementX;
					}

					// Calculate final position
					projPos.X = (float)(projX * TileHorizontalOffset);
					projPos.Y = (float)(projY * TileVerticalOffset);
				}
				else
				{
					// Determine tile distance
					if(angle == 1 || angle == 5)
					{
						// Direction left down/right up
						double proj1 = (angle - 3) / 2 * _currentUnitEntry.Value.ProjectileGraphicDisplacementY;
						double proj2 = (angle - 3) / 2 * _currentUnitEntry.Value.ProjectileGraphicDisplacementX;

						// Calculate final position
						projPos.X = (float)((proj1 + proj2) * TileHorizontalOffset);
						projPos.Y = (float)((-(proj1 - proj2) - _currentUnitEntry.Value.ProjectileGraphicDisplacementZ) * TileVerticalOffset);
					}
					else if(angle == 3 || angle == 7)
					{
						// Direction left up/right down
						double proj1 = -(angle - 5) / 2 * _currentUnitEntry.Value.ProjectileGraphicDisplacementX;
						double proj2 = (angle - 5) / 2 * _currentUnitEntry.Value.ProjectileGraphicDisplacementY;

						// Calculate final position
						projPos.X = (float)((proj2 - proj1) * TileHorizontalOffset);
						projPos.Y = (float)((proj2 + proj1 - _currentUnitEntry.Value.ProjectileGraphicDisplacementZ) * TileVerticalOffset);
					}
				}

				// Update projectile area
				if(_currentUnitEntry.Value.ProjectileSpawningAreaWidth < 0.1f && _currentUnitEntry.Value.ProjectileSpawningAreaHeight < 0.1f)
					_projectileAreaShapes[i].Fill = new SolidColorBrush(ProjectileAreaColor);
				else
					_projectileAreaShapes[i].Fill = new SolidColorBrush(ProjectileAreaTransparentColor);
				_projectileAreaShapes[i].Width = Math.Max(_currentUnitEntry.Value.ProjectileSpawningAreaWidth * TileSideLength, 0.1 * TileSideLength);
				_projectileAreaShapes[i].Height = Math.Max(_currentUnitEntry.Value.ProjectileSpawningAreaHeight * TileSideLength, 0.1 * TileSideLength);
				_projectileAreaShapes[i].RenderTransform = new RotateTransform(i * 45, _projectileAreaShapes[i].Width / 2, _projectileAreaShapes[i].Height / 2);
				Canvas.SetLeft(_projectileAreaShapes[i], (i % framesPerRow) * maxFrameWidth + userFrameOffset.X + _maxFrameOffsets.Left + projPos.X - _projectileAreaShapes[i].Width / 2);
				Canvas.SetTop(_projectileAreaShapes[i], (i / framesPerRow) * maxFrameHeight + userFrameOffset.Y + _maxFrameOffsets.Top + projPos.Y - _projectileAreaShapes[i].Height / 2);
			}
		}

		#endregion

		#region Event Handlers

		private void _browseDrs1Button_OnClick(object sender, RoutedEventArgs e)
		{
			// Create and show dialog
			var openFileDialog = new OpenFileDialog
			{
				FileName = File.Exists(Drs1Path) ? Drs1Path : "",
				Filter = "Resource files (*.drs)|*.drs",
				Title = "Select 1st priority DRS file..."
			};
			if(openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// Save selected file
			Drs1Path = openFileDialog.FileName;
		}

		private void _browseDrs2Button_OnClick(object sender, RoutedEventArgs e)
		{
			// Create and show dialog
			var openFileDialog = new OpenFileDialog
			{
				FileName = File.Exists(Drs2Path) ? Drs2Path : "",
				Filter = "Resource files (*.drs)|*.drs",
				Title = "Select 2nd priority DRS file..."
			};
			if(openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			// Save selected file
			Drs2Path = openFileDialog.FileName;
		}

		private void _loadDrsButton_OnClick(object sender, RoutedEventArgs e)
		{
			// Load DRS files
			try
			{
				// Load files
				if(File.Exists(Drs1Path))
					_drs1 = new DRSFile(Drs1Path);
				if(File.Exists(Drs2Path))
					_drs2 = new DRSFile(Drs2Path);
			}
			catch(IOException ex)
			{
				// Show error
				MessageBox.Show($"Unable to load DRS file: {ex.Message}");
				return;
			}
			finally
			{
				// If files are loaded, enable UI, else disable
				DrsFilesLoaded = _drs1 != null || _drs2 != null;
			}

			// Update application settings
			Properties.Settings.Default.Drs1Path = Drs1Path;
			Properties.Settings.Default.Drs2Path = Drs2Path;
		}

		private void _shape_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			// Check item index
			int itemIndex = (int)((FrameworkElement)sender).Tag;
			if(_currentDragItemIndex >= 0)
				return;

			// Set currently dragged item
			_lastMouseMovePoint = e.GetPosition(_renderPanel);
			_currentDragItemIndex = itemIndex;
		}

		private void _renderPanel_OnMouseMove(object sender, MouseEventArgs e)
		{
			// Check item index
			if(_currentDragItemIndex < 0)
				return;

			// Move item
			Point mousePos = e.GetPosition(_renderPanel);
			_userDefinedFrameOffsets[_currentDragItemIndex] = new Point(_userDefinedFrameOffsets[_currentDragItemIndex].X - (_lastMouseMovePoint.X - mousePos.X),
				_userDefinedFrameOffsets[_currentDragItemIndex].Y - (_lastMouseMovePoint.Y - mousePos.Y));
			_lastMouseMovePoint = mousePos;
			UpdatePositions();
		}

		private void _shape_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			// Also called by the render panel for proper cleanup.

			// Reset currently dragged item
			_currentDragItemIndex = -1;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The path to the 1st priority DRS file.
		/// </summary>
		public string Drs1Path
		{
			get { return _drs1Path; }
			set
			{
				_drs1Path = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Drs1Path)));
			}
		}

		/// <summary>
		/// The path to the 2nd priority DRS file.
		/// </summary>
		public string Drs2Path
		{
			get { return _drs2Path; }
			set
			{
				_drs2Path = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Drs2Path)));
			}
		}

		/// <summary>
		/// Tells whether DRS files are loaded.
		/// </summary>
		public bool DrsFilesLoaded
		{
			get { return _drsFilesLoaded; }
			private set
			{
				_drsFilesLoaded = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrsFilesLoaded)));
			}
		}

		/// <summary>
		/// The currently selected unit.
		/// </summary>
		public KeyValuePair<short, UnitEntry> CurrentUnitEntry
		{
			get { return _currentUnitEntry; }
			set
			{
				_currentUnitEntry = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentUnitEntry)));

				// Subscribe to update events
				if(value.Value.ProjectileFrameDelay != null)
					value.Value.ProjectileFrameDelay.PropertyChanged += (sender, e) =>
					{
						if(e.PropertyName == nameof(value.Value.ProjectileFrameDelay.Value))
							UpdateDisplayedFrames();
					};
				if(value.Value.ProjectileGraphicDisplacementX != null)
					value.Value.ProjectileGraphicDisplacementX.PropertyChanged += (sender, e) => UpdatePositions();
				if(value.Value.ProjectileGraphicDisplacementY != null)
					value.Value.ProjectileGraphicDisplacementY.PropertyChanged += (sender, e) => UpdatePositions();
				if(value.Value.ProjectileGraphicDisplacementZ != null)
					value.Value.ProjectileGraphicDisplacementZ.PropertyChanged += (sender, e) => UpdatePositions();
				if(value.Value.ProjectileSpawningAreaWidth != null)
					value.Value.ProjectileSpawningAreaWidth.PropertyChanged += (sender, e) => UpdatePositions();
				if(value.Value.ProjectileSpawningAreaHeight != null)
					value.Value.ProjectileSpawningAreaHeight.PropertyChanged += (sender, e) => UpdatePositions();

				// Render unit
				UpdateUnitRenderData();
			}
		}

		/// <summary>
		/// The base genie file where the rendering data is derived from.
		/// </summary>
		public GenieFile GenieFile
		{
			get { return _genieFile; }
			set
			{
				_genieFile = value;
				UpdateUnitRenderData();
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Implementation of PropertyChanged interface.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Helper types

		/// <summary>
		/// Stores offset values.
		/// </summary>
		class OffsetData
		{
			public int Left { get; set; }
			public int Right { get; set; }
			public int Top { get; set; }
			public int Bottom { get; set; }

			public OffsetData(int left, int right, int top, int bottom)
			{
				Left = left;
				Right = right;
				Top = top;
				Bottom = bottom;
			}
		}

		#endregion
	}
}