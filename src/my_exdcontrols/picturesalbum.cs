using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using My.ExdControls;

namespace My
{


	/// <summary>
	/// Contient des éléments d'informations pour la gestion des images dans PictureAlbum.
	/// </summary>
	[Serializable()]
	public class PictureInfos : ISerializable
	{
	
		// ---------------------------------------------------------------------------
		// PROPRIETES:

		/// <summary>
		/// Obtient la version (utile en interne pour la sérialisation).
		/// </summary>
		public static int Version { get { return 2; } }
		
		/// <summary>
		/// Obtient ou définit l'image.
		/// </summary>
		public Bitmap Picture { get; set; }
		
		/// <summary>
		/// Obtient ou définit le nom de l'image.
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// Obtient ou définit le nom du fichier.
		/// </summary>
		public string Filename { get; set; }
		
		/// <summary>
		/// Obtient ou définit la légende.
		/// </summary>
		public string Caption { get; set; }
		
		/// <summary>
		/// Obtient ou définit la rotation de l'image pour l'affichage en zoom.
		/// </summary>
		public RotateFlipType ZoomRotation { get; set; }
		
		/// <summary>
		/// Obtient le texte d'un éventuel ToolTip, à partir de la légende et du nom de fichier.
		/// </summary>
		public string ToolTipText
		{
			get
			{
				if (String.IsNullOrEmpty(Name + Caption + Filename)) { return MyResources.Album_NoPictureProperties; }
				string result = String.Empty;
				if (!String.IsNullOrEmpty(Name)) { result += String.Format("{0}\n\n", Name); }
				if (!String.IsNullOrEmpty(Caption)) { result += String.Format("{0}\n\n", Caption); }
				if (String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Caption)) { result += String.Format("({0})", Filename); }
				return result.Trim();
			}
		}

		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS:
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public PictureInfos()
		{
			Picture = null;
			Name = String.Empty;
			Filename = String.Empty;
			Caption = String.Empty;
			ZoomRotation = RotateFlipType.RotateNoneFlipNone;
		}
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public PictureInfos(Bitmap bmp, string name, string filename, string caption)
		{
			Picture = bmp;
			Name = (name == null ? String.Empty : name);
			Filename = (filename == null ? String.Empty : filename);
			Caption = (caption == null ? String.Empty : caption);
			ZoomRotation = RotateFlipType.RotateNoneFlipNone;
		}

		/// <summary>
		/// Constructeur pour la désérialisation. Appel de this() pour valeur par défaut.
		/// </summary>
		protected PictureInfos(SerializationInfo info, StreamingContext context) : this()
		{
			// Récupère la version, puis charge en fonction de la version:
			int version = info.GetInt32("version");
			if (version >= 1) {
				Caption = info.GetString("caption");
				Filename = info.GetString("filename");
				Picture = (Bitmap)info.GetValue("picture", typeof(Bitmap));
				Name = info.GetString("name"); }
			if (version >= 2) {
				ZoomRotation = (RotateFlipType)info.GetValue("zoomRotation", typeof(RotateFlipType)); }
				
		}


		// ---------------------------------------------------------------------------
		// METHODES:

		/// <summary>
		/// Méthode pour sérialisation.
		/// </summary>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new System.ArgumentNullException("info");
			info.AddValue("version", Version);
			info.AddValue("caption", Caption);
			info.AddValue("filename", Filename);
			info.AddValue("picture", Picture);
			info.AddValue("name", Name);
			info.AddValue("zoomRotation", ZoomRotation);
		}
		
	
	}


	// ===========================================================================


	/// <summary>
	/// Affiche sur un panel un album photo, dans lequel il est possible d'ajouter, de supprimer, d'enregistrer sur disque (éventuellement en changeant de format) des photos. La résolution des vignettes peut être changé, et il est possible de voir les images en grand format. Il est également possible d'enregistrer pour chaque photo une légende. Il est possible de sérialiser l'objet dans un fichier : Toutes les photos (au format et à la taille originale) sont sauvées dans le fichier sérialisé, ainsi que la résolution des vignettes, les légendes, etc., etc.
	/// </summary>
	[Serializable()]
	public class PictureAlbum : Panel, ISerializable
	{
	
		// ---------------------------------------------------------------------------
		// DECLARATIONS:
		
		// Contrôles:
		protected TableLayoutPanel _tlpMain;
		protected DragAndDropListView _list;
		protected TrackBar _track;
		protected ContextMenuStrip _mnuList, _mnuPicture;
		protected Label _lblCaption;
		protected ToolTip _toolTip;
		protected DialogBoxPictureZoom _zoomForm;
		
		// Variables:
		protected PictureInfos[] _pictInfos;
		protected bool _locked;
		protected string _albumCaption;
		
		// Variables réservées:
		protected Color __albumBackColor, __albumForeColor;
		protected bool __showNames;
		
		// Evénements:
		
		/// <summary>
		/// Evénement qui se déclenche lorsque l'utilisateur modifie l'album (déplacement d'images, ajout, suppression, mais pas changer de résolution des vignettes, etc.).
		/// </summary>
		public event EventHandler AlbumChanged;


		// ---------------------------------------------------------------------------
		// PROPRIETES:

		/// <summary>
		/// Version de l'album (utile pour la sérialisation, en interne).
		/// </summary>
		public static int Version { get { return 1; } }

		/// <summary>
		/// Obtient ou définit la légende de l'album.
		/// </summary>
		public string AlbumCaption {
			get { return _albumCaption; }
			set { _albumCaption = value; _lblCaption.Text = _albumCaption; } }

		/// <summary>
		/// Obtient les images sélectionnées, ou un tableau vide si pas de sélection.
		/// </summary>
		public PictureInfos[] SelectedPictureInfos
		{
			get
			{
				int l = _list.SelectedItems.Count;
				PictureInfos[] result = new PictureInfos[l];
				for (int i=0; i<l; i++) { result[i] = (PictureInfos)_list.SelectedItems[i].Tag; }
				return result;
			}
		}
	
		/// <summary>
		/// Obtient ou définit si l'album peut être modifier par l'utilisateur (ajout, suppression, déplacement de photos).
		/// </summary>
		public bool Locked
		{
			get
			{
				return _locked;
			}
			set
			{
				_locked = value;
				// Bloque ou débloque les menus qui permettent de modifier les données:
				_mnuList.Items["addPictures"].Enabled = !value;
				_mnuPicture.Items["addPictures"].Enabled = !value;
				_mnuPicture.Items["deleteSelectedPictures"].Enabled = !value;
				_mnuPicture.Items["deleteAllPictures"].Enabled = !value;
			}
		}

		/// <summary>
		/// Obtient tous les PictureInfos, dans l'ordre de la liste (réorganisation avant le renvoi).
		/// </summary>
		public PictureInfos[] PicturesInfosArray {
			get { RebuildPictureInfosArray(); return _pictInfos; } }
		
		/// <summary>
		/// Obtient ou définit s'il faut afficher ou non les noms des images dans la liste.
		/// </summary>
		public bool ShowNames {
			get { return __showNames; }
			set { ShowNamesInList(value); } }
		
		/// <summary>
		/// Obtient ou définit les éléments de la liste sélectionnés.
		/// </summary>
		public int[] SelectedIndex
		{
			get
			{
				int l = _list.SelectedItems.Count;
				int[] result = new int[l];
				for (int i=0; i<l; i++) { result[i] = _list.SelectedItems[i].Index; }
				return result;
			}
			set
			{
				int l = value.Length; int len = _list.Items.Count;
				for (int i=0; i<len; i++) { _list.Items[i].Selected = false; }
				for (int i=0; i<l; i++) { _list.Items[value[i]].Selected = true; }
			}
		}


		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS:


		/// <summary>
		/// Initialisation des contrôles, etc.
		/// </summary>
		public PictureAlbum ()
		{
		
			// Initialisation des variables:
			_pictInfos = new PictureInfos[0];
			_zoomForm = new DialogBoxPictureZoom();
				
			// Initialisation des contrôles
			
			_list = new DragAndDropListView();
			_list.Dock = DockStyle.Fill;
			_list.View = View.LargeIcon;
			_list.BorderStyle = BorderStyle.None;
			_list.ShowItemToolTips = true;
			_list.Activation = ItemActivation.Standard;
			_list.InsertionMark.Color = MySettings.PictureAlbumInsertionMarkColor;

			_lblCaption = new Label();
			_lblCaption.Dock = DockStyle.Fill;
			_lblCaption.AutoSize = false;
			_lblCaption.AutoEllipsis = true;
			_lblCaption.TextAlign = ContentAlignment.MiddleLeft;
			_lblCaption.Text = String.Empty;
			
			_toolTip = new ToolTip();
			_toolTip.SetToolTip(_lblCaption, String.Empty);
			
			_track = new TrackBar();
			_track.Dock = DockStyle.Fill;
			_track.Minimum = 16;
			_track.Maximum = 256;
			_track.Value = 256;
			_track.LargeChange = 20;
			_track.SmallChange = 10;
			_track.TickStyle = TickStyle.None;
			
			TableLayoutPanel tlpTools = new TableLayoutPanel();
			tlpTools.Dock = DockStyle.Fill;
			tlpTools.RowCount = 1;
			tlpTools.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			tlpTools.ColumnCount = 2;
			tlpTools.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
			tlpTools.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
			
			tlpTools.Controls.Add(_lblCaption, 0, 0);
			tlpTools.Controls.Add(_track, 1, 0);
			
			_tlpMain = new TableLayoutPanel();
			_tlpMain.Dock = DockStyle.Fill;
			_tlpMain.RowCount = 2;
			_tlpMain.RowStyles.Add( new RowStyle(SizeType.Percent, 100));
			_tlpMain.RowStyles.Add( new RowStyle(SizeType.Absolute, 30));
			_tlpMain.ColumnCount = 1;
			_tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			_tlpMain.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
			_tlpMain.Padding = new Padding(0, 1, 0, 0);

			_tlpMain.Controls.Add(_list, 0, 0);
			_tlpMain.Controls.Add(tlpTools, 0, 1);
			
			// Ajout des contrôles à soi-même:
			Controls.Add(_tlpMain);
			Dock = DockStyle.Fill;
			
			// Menu contextuel pour les images:
			
			_mnuPicture = new ContextMenuStrip();
			
			ToolStripMenuItem menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_SelectedPicturesProperties;
			menuItem.Name = "selectedPicturesProperties";
			menuItem.Click += delegate { ChangeSelectedPicturesProperties(); };
			_mnuPicture.Items.Add(menuItem);
			
			_mnuPicture.Items.Add(new ToolStripSeparator());
			
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_AddPictures;
			menuItem.Name = "addPictures";
			menuItem.Click += delegate { AddPicture(); };
			_mnuPicture.Items.Add(menuItem);
			
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_DeleteSelectedPictures;
			menuItem.Name = "deleteSelectedPictures";
			menuItem.Click += delegate { DeleteSelectedPictures(true); };
			_mnuPicture.Items.Add(menuItem);
			
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_DeleteAllPictures;
			menuItem.Name = "deleteAllPictures";
			menuItem.Click += delegate { DeleteAllPictures(true); };
			_mnuPicture.Items.Add(menuItem);
			
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_SaveSelectedPictures;
			menuItem.Name = "saveSelectedPicture";
			_mnuPicture.Items.Add(menuItem);
			
			ToolStripMenuItem subMenuItem = new ToolStripMenuItem();
			subMenuItem.Text = MyResources.Album_menu_ConvertAndSaveSelectedPictures;
			subMenuItem.Name = "convertAndSaveSelectedPictures";
			subMenuItem.Click += delegate { ConvertAndSaveSelectedPictures(); };
			menuItem.DropDownItems.Add(subMenuItem);
			
			subMenuItem = new ToolStripMenuItem();
			subMenuItem.Text = MyResources.Album_menu_SavePicturesUsingExistingNames;
			subMenuItem.Name = "savePicturesUsingExistingNames";
			subMenuItem.Click += delegate { SaveSelectedPicturesUsingExistingNamesAndFormats(); };
			menuItem.DropDownItems.Add(subMenuItem);
			
			subMenuItem = new ToolStripMenuItem();
			subMenuItem.Text = MyResources.Album_menu_SavePicturesUsingNewNames;
			subMenuItem.Name = "savePicturesUsingNewNames";
			subMenuItem.Click += delegate { SaveSelectedPicturesUsingNewNamesAndFormat(); };
			menuItem.DropDownItems.Add(subMenuItem);
			
			_mnuPicture.Items.Add(new ToolStripSeparator());
			
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_Zoom;
			menuItem.Name = "zoom";
			menuItem.Click += delegate {
				if (_list.SelectedItems.Count == 0) { return; }
				RebuildPictureInfosArray();
				_zoomForm.Show(_pictInfos, _list.SelectedItems[0].Index); };
			_mnuPicture.Items.Add(menuItem);
			
			_mnuPicture.Items.Add(new ToolStripSeparator());
						
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_AlbumProperties;
			menuItem.Name = "albumProperties";
			menuItem.Click += delegate { ChangeAlbumProperties(); };
			_mnuPicture.Items.Add(menuItem);
			
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_ShowNames;
			menuItem.Name = "showNames";
			menuItem.Click += delegate { ShowNamesInList(!__showNames); };
			_mnuPicture.Items.Add(menuItem);
			
			_mnuPicture.Items.Add(new ToolStripSeparator());
						
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_ChangeColors;
			menuItem.Name = "changeColors";
			_mnuPicture.Items.Add(menuItem);
			
			subMenuItem = new ToolStripMenuItem();
			subMenuItem.Text = MyResources.Album_menu_ChangeForeColor;
			subMenuItem.Name = "changeForeColor";
			subMenuItem.Click += delegate { ChangeAlbumColors(false); };
			menuItem.DropDownItems.Add(subMenuItem);
			
			subMenuItem = new ToolStripMenuItem();
			subMenuItem.Text = MyResources.Album_menu_ChangeBackColor;
			subMenuItem.Name = "changeBackColor";
			subMenuItem.Click += delegate { ChangeAlbumColors(true); };
			menuItem.DropDownItems.Add(subMenuItem);

			// Menu contextuel pour la liste:
			
			_mnuList = new ContextMenuStrip();
			
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_AddPictures;
			menuItem.Name = "addPictures";
			menuItem.Click += delegate { AddPicture(); };
			_mnuList.Items.Add(menuItem);

			_mnuList.Items.Add(new ToolStripSeparator());
						
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_AlbumProperties;
			menuItem.Name = "albumProperties";
			menuItem.Click += delegate { ChangeAlbumProperties(); };
			_mnuList.Items.Add(menuItem);

			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_ShowNames;
			menuItem.Name = "showNames";
			menuItem.Click += delegate { ShowNamesInList(!__showNames); };
			_mnuList.Items.Add(menuItem);
						
			_mnuList.Items.Add(new ToolStripSeparator());
						
			menuItem = new ToolStripMenuItem();
			menuItem.Text = MyResources.Album_menu_ChangeColors;
			menuItem.Name = "changeColors";
			_mnuList.Items.Add(menuItem);
			
			subMenuItem = new ToolStripMenuItem();
			subMenuItem.Text = MyResources.Album_menu_ChangeForeColor;
			subMenuItem.Name = "changeForeColor";
			subMenuItem.Click += delegate { ChangeAlbumColors(false); };
			menuItem.DropDownItems.Add(subMenuItem);
			
			subMenuItem = new ToolStripMenuItem();
			subMenuItem.Text = MyResources.Album_menu_ChangeBackColor;
			subMenuItem.Name = "changeBackColor";
			subMenuItem.Click += delegate { ChangeAlbumColors(true); };
			menuItem.DropDownItems.Add(subMenuItem);

			// Evénements:
			
			_track.ValueChanged += delegate { if (Control.MouseButtons == MouseButtons.Left) { return; } ResizePictures(_track.Value); };
			_track.MouseUp += delegate(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Left) ResizePictures(_track.Value); };
			//_track.ValueChanged += delegate { ResizePictures(_track.Value); };
			_lblCaption.DoubleClick += delegate { ChangeAlbumProperties(); };
			_list.MouseDoubleClick += new MouseEventHandler(_list_MouseDoubleClick);
			// Ajout de photos extérieures par drag and drop:
			_list.ExternalDataDragged += new ExternalDataDraggedEventHandler(_list_ExternalDataDragged);
			_list.ExternalDataDropped += new ExternalDataDroppedEventHandler(_list_ExternalDataDropped);
			_list.MouseUp += new MouseEventHandler(_list_MouseUp);
			// ToolTip sur AlbumCaption quand texte change:
			_lblCaption.TextChanged += delegate { _toolTip.SetToolTip(_lblCaption, _lblCaption.Text); };
			// Lorsque l'on survole la liste, on la sélectionne:
			_list.MouseEnter += delegate { _list.Select(); };
			// Changement de taille:
			_list.Resize += delegate { CenterList(); };
			// Pour centrer les images, on change la marge de gauche de la liste, découvrant ainsi le _tlpMain:
			_tlpMain.MouseDown += new MouseEventHandler(_tlpMain_MouseDown);
			// Coupe l'affichage de la légende au premier saut de ligne:
			_lblCaption.TextChanged += delegate { int index = _lblCaption.Text.IndexOf("\n");
					if (index > -1) { _lblCaption.Text = _lblCaption.Text.Substring(0, index); } };
			
			// Locked en mode débogage:
			#if DEBUG
			_list.KeyDown += delegate(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.L && e.Modifiers == Keys.Control) { Locked = !Locked; } };
			#endif

			// Valeurs par défaut:
			
			ShowNamesInList(MySettings.PictureAlbumDefaultShowNames);
			_track.Value = MySettings.PictureAlbumDefaultThumbnailSize;
			ChangeAlbumColors(true, MySettings.PictureAlbumDefaultBackColor);
			ChangeAlbumColors(false, MySettings.PictureAlbumDefaultForeColor);
			
			// Initialisation de l'ImageList de la liste:
			ResizePictures(_track.Value);
						
		}

		
		/// <summary>
		/// Constructeur pour la désérialisation. Ne pas oublier d'appeler this(), sinon les contrôles ne sont pas définis !
		/// </summary>
		protected PictureAlbum(SerializationInfo info, StreamingContext context) : this()
		{
		
			// S'il y a une version, c'est qu'on est dans la nouvelle version:
			int version = -1; try { version = info.GetInt32("version"); } catch { ; }
			
			// Ancienne version
			if (version == -1)
			{
				_albumCaption = info.GetString("albumName");
				// Comme dit dans getObjectData, on désérialise un bmp après l'autre, ce qui fonctionne (et on les ajoute à l'objet):
				int l = info.GetInt32("arrayLength");
				string[] filenames = (string[])info.GetValue("fileNames", typeof(string[]));
				string[] captions = (string[])info.GetValue("captions", typeof(string[]));
				_pictInfos = new PictureInfos[l];
				for (int i=0; i<l; i++) {
					_pictInfos[i] = new PictureInfos();
					_pictInfos[i].Picture = (Bitmap)info.GetValue("picture" + i.ToString(), typeof(Bitmap));
					_pictInfos[i].Filename = filenames[i];
					_pictInfos[i].Caption = captions[i];
					_pictInfos[i].Name = (String.IsNullOrEmpty(filenames[i]) ? String.Empty : Path.GetFileNameWithoutExtension(filenames[i])); }
				try {
					//_zoomForm.RealSize = info.GetBoolean("zoomForm_RealSize");
					_zoomForm.ZoomFactor = info.GetInt32("zoomForm_ZoomFactor"); }
				catch { ; }
			}
			
			// Version 1:
			if (version == 1)
			{
				int l = info.GetInt32("count");
				_pictInfos = new PictureInfos[l];
				for (int i=0; i<l; i++) {
					_pictInfos[i] = (PictureInfos)info.GetValue(String.Format("pictureInfo_{0}", i), typeof(PictureInfos)); }
				_albumCaption = info.GetString("albumCaption");
				if (!MySettings.PictureAlbumIgnoreSavedPictureSize) { _track.Value = info.GetInt32("picturesSize"); }
				ShowNames = info.GetBoolean("showNames");
				ChangeAlbumColors(true, (Color)info.GetValue("albumBackColor", typeof(Color)));
				ChangeAlbumColors(false, (Color)info.GetValue("albumForeColor", typeof(Color)));
				_zoomForm.ProgramCustomColor = (Color)info.GetValue("zoomForm_ProgramCustomColor", typeof(Color));
				_zoomForm.UserCustomColor = (Color)info.GetValue("zoomForm_UserCustomColor", typeof(Color));
				_zoomForm.BackColorIndex = info.GetInt32("zoomForm_BackColorIndex");
				_zoomForm.ZoomFactor = info.GetInt32("zoomForm_ZoomFactor");
				//_zoomForm.RealSize = info.GetBoolean("zoomForm_RealSize");
			}
			
			// Procédure commune:
			_lblCaption.Text = _albumCaption;
			foreach (PictureInfos i in _pictInfos) { AddPicture(i, -1); }
			foreach (ListViewItem item in _list.Items) { item.Selected = false; }

		}

		// ---------------------------------------------------------------------------
		// METHODES:
	
	
		/// <summary>
		/// Créer un bitmap vignette de taille size contenant l'image bmp réduite pour qu'elle rentre dans la vignette. Les marges sont dans la couleur BackColor (si Empty, aucune couleur n'est définie).
		/// </summary>
		public static Bitmap GetThumbnail(Bitmap bmp, int size, Color BackColor)
		{
			// Obtient les dimensions de l'image d'origine:
			int h = bmp.Height, w = bmp.Width;
			// Créer la vignette:
			float k, newH, newW;
			Bitmap thumb = new Bitmap(size, size);
			using (Graphics g = Graphics.FromImage(thumb))
			{
				if (BackColor != Color.Empty) { g.Clear(BackColor); }
				// Si la hauteur et la largeur sont identiques, copie simplement:
				if (h == w)
				{
					g.DrawImage(bmp, new RectangleF(0, 0, size, size));
				}
				// Sinon, redimensionne l'image en fonction de la largeur ou bien de la hauteur:
				else if (w > h)
				{
					k = (float)size / (float)w;
					newW = w * k; newH = h * k;
					g.DrawImage(bmp, new RectangleF(0, (size - newH) / 2, newW, newH));
				}
				else
				{
					k = (float)size / (float)h;
					newW = w * k; newH = h * k;
					g.DrawImage(bmp, new RectangleF((size - newW) / 2, 0, newW, newH));
				}
			}
			// Retourne la vignette:
			return thumb;
		}

		
		/// <summary>
		/// Change la taille des images.
		/// </summary>
		protected void ResizePictures(int size)
		{
			// Recréer l'ImageList pour l'adapter à la nouvelle taille.
			_list.LargeImageList = new ImageList();
			_list.LargeImageList.ImageSize = new Size(size, size);
			_list.LargeImageList.ColorDepth = ColorDepth.Depth32Bit;
			foreach (PictureInfos info in _pictInfos) {
				_list.LargeImageList.Images.Add(GetThumbnail(info.Picture, size, Color.Empty)); }
			CenterList();
		}


		/// <summary>
		/// Change les propriétés Left et Width de la liste pour centrer les images.
		/// </summary>
		private void CenterList()
		{
			// Sort si le contrôle n'est pas encore insérer:
			if (_tlpMain.TopLevelControl == null) { return; }
			// Nombre d'image affichées (le -6 est la somme de l'ajustement (trouvé empiriquement) entre _tlpMain.ClientSize et la liste,
			// et de la marge de droite de la liste, qui vaut 3):
			int scrollWidth = SystemInformation.VerticalScrollBarWidth;
			int iconSpacing = SystemInformation.IconHorizontalSpacing - SystemInformation.IconSize.Width;
			int nb = (int)Math.Floor((double)(_tlpMain.ClientSize.Width - scrollWidth - 6) / (_track.Value + iconSpacing));
			// Définit les propriétés:
			int left = (_tlpMain.ClientSize.Width - scrollWidth - 6 - ((_track.Value + iconSpacing) * nb)) / 2 + 3;
			if (nb > 0 && left > 0) { _list.Left = left; _list.Width = _tlpMain.ClientSize.Width - _list.Left - 3; }
			else { _list.Left = 0; _list.Width = _tlpMain.ClientSize.Width; }
		}


		/// <summary>
		/// Reconstruit le tableau _pictInfos, en mettant les images dans l'ordre de la liste.
		/// </summary>
		protected void RebuildPictureInfosArray()
		{
			int l = _list.Items.Count;
			_pictInfos = new PictureInfos[l];
			for (int i=0; i<l; i++) { _pictInfos[i] = (PictureInfos)_list.Items[i].Tag; }
		}


		/// <summary>
		/// Déclenche l'événement AlbumChanged.
		/// </summary>
		protected virtual void OnAlbumChanged()
			{ if (AlbumChanged != null) { AlbumChanged(this, new EventArgs()); } }


		/// <summary>
		/// Méthode pour sérialisation.
		/// </summary>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new System.ArgumentNullException("info");
			info.AddValue("version", Version);
			// Sérialiser telle quel le tableau donne à la désérialisation un tableau remplit de null
			// (exactement comme pour un tableau de Bitmap). Donc on enregistrer chq él séparément:
			RebuildPictureInfosArray(); // Remet le tableau dans l'ordre de la liste.
			int l = _pictInfos.Length;
			info.AddValue("count", l);
			for (int i=0; i<l; i++) {
				info.AddValue(String.Format("pictureInfo_{0}", i), _pictInfos[i], typeof(PictureInfos)); }
			info.AddValue("albumCaption", _albumCaption); _lblCaption.Text = _albumCaption;
			info.AddValue("picturesSize", _track.Value);
			info.AddValue("showNames", __showNames);
			info.AddValue("zoomForm_ZoomFactor", _zoomForm.ZoomFactor);
			info.AddValue("zoomForm_RealSize", _zoomForm.RealSize);
			info.AddValue("albumBackColor", __albumBackColor, typeof(Color));
			info.AddValue("albumForeColor", __albumForeColor, typeof(Color));
			info.AddValue("zoomForm_ProgramCustomColor", _zoomForm.ProgramCustomColor, typeof(Color));
			info.AddValue("zoomForm_UserCustomColor", _zoomForm.UserCustomColor, typeof(Color));
			info.AddValue("zoomForm_BackColorIndex", _zoomForm.BackColorIndex);
		}


		/// <summary>
		/// Ajoute une image à la liste, avec les infos passés en argument. Lance l'événement AlbumChanged. index indique l'index de l'élément avant lequel il faut insérer la nouvelle image. Si négatif, l'insère à la fin.
		/// </summary>
		public virtual void AddPicture(PictureInfos info, int index)
		{
			// Ajoute à la liste:
			ListViewItem item; int imgIndex = -1;
			try { _list.LargeImageList.Images.Add(GetThumbnail(info.Picture, _track.Value, Color.Empty));
				imgIndex = _list.LargeImageList.Images.Count - 1; }
			catch { ; }
			string itemText = (__showNames ? info.Name : String.Empty);
			if (index < 0) { item = _list.Items.Add(itemText, imgIndex); }
			else { item = _list.Items.Insert(index, itemText, imgIndex); }
			item.ToolTipText = info.ToolTipText;
			item.Tag = info;
			// Sélectionne l'item:
			item.Selected = true;
			item.Focused = true;
			item.EnsureVisible();
			// Ajoute au tableau:
			int l = _pictInfos.Length;
			Array.Resize(ref _pictInfos, l + 1);
			_pictInfos[l] = info;
			// Lance l'événement:
			OnAlbumChanged();
		}

		/// <summary>
		/// Ajoute une image en demandant à l'user. Lance l'événement AlbumChanged.
		/// </summary>
		public virtual void AddPicture()
		{
			OpenFileDialog openDialog = My.FilesAndStreams.GetMyOpenFileDialog();
			openDialog.Multiselect = true;
			openDialog.Filter = "Pictures (*.jpg,*.bmp,*.gif,*.png,*.tif,*.wmf,*.emf)|*.jpg;*.bmp;*.gif;*.png;*.tif;*.wmf;*.emf";
			if (openDialog.ShowDialog() == DialogResult.OK) {
				string[] fileNames = openDialog.FileNames;
				foreach (string i in fileNames) { AddPicture(i, -1); } }
		}

		/// <summary>
		/// Ajoute l'image dont le nom de fichier est passé en argument. Lance l'événement AlbumChanged.
		/// </summary>
		public virtual bool AddPicture(string filename, int index)
		{
			Bitmap bmp;
			try { bmp = new Bitmap(filename); }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc, String.Format(My.ExdControls.MyResources.Album_errorMsg_ErrorWhenOpeningPicture, filename));
				return false; }
			PictureInfos info = new PictureInfos(bmp, Path.GetFileNameWithoutExtension(filename), filename, String.Empty);
			AddPicture(info, index);
			return true;
		}

		/// <summary>
		/// Ajoute l'image passée en argument. Lance l'événement AlbumChanged.
		/// </summary>
		public virtual void AddPicture(Bitmap bmp, int index)
		{
			PictureInfos info = new PictureInfos(bmp, My.ExdControls.MyResources.Album_defaultMsg_NoName, String.Empty, String.Empty);
			AddPicture(info, index);
		}


		/// <summary>
		/// Change les propriétés de l'album via un dialogue. Lance l'événement AlbumChanged si pas Locked (sinon affichage sans modification possible).
		/// </summary>
		protected virtual void ChangeAlbumProperties()
		{
			// Contrôles:
			TextBox txtCaption = new TextBox();
			txtCaption.Multiline = true;
			txtCaption.AcceptsReturn = true;
			txtCaption.Text = _albumCaption;
			txtCaption.ScrollBars = ScrollBars.Vertical;
			CheckBox chkShowNames = new CheckBox();
			chkShowNames.Checked = __showNames;
			NumericUpDown numSize = new NumericUpDown();
			numSize.Minimum = 16;
			numSize.Maximum = 256;
			numSize.DecimalPlaces = 0;
			numSize.Value = _track.Value;
			txtCaption.ReadOnly = _locked;
			// Affiche la boîte de dialogue:
			MyFormMultilines dialog = new MyFormMultilines();
			dialog.AddButtonsCollection(DialogBoxButtons.OKCancel, 1, true);
			dialog.SetDialogMessage(MyResources.Album_dialog_AlbumProperties);
			dialog.SetDialogIcon(DialogBoxIcon.Information);
			dialog.AddLine(MyResources.Album_dialog_AlbumProperties_Caption, txtCaption, 130);
			dialog.AddLine(MyResources.Album_dialog_AlbumProperties_ShowNames, chkShowNames);
			dialog.AddLine(MyResources.Album_dialog_AlbumProperties_ThumbSize, numSize);
			if (dialog.ShowDialog() == DialogBoxClickResult.Cancel) { dialog.Dispose(); return; }
			// Met à jour les propriétés:
			if (!_locked && _albumCaption != txtCaption.Text) {
				_albumCaption = txtCaption.Text; _lblCaption.Text = _albumCaption;
				// Le contrôle a changé:
				OnAlbumChanged(); }
			ShowNamesInList(chkShowNames.Checked);
			if (_track.Value != (int)numSize.Value) { _track.Value = (int)numSize.Value; }
		}


		/// <summary>
		/// Modifie ou affiche les propriétés des images sélectionnées, via un dialogue. Lance l'événement AlbumChanged si pas Locked (sinon, affichage sans modification possible).
		/// </summary>
		protected virtual void ChangeSelectedPicturesProperties()
		{
			// Message si plusieurs images sélectionnées:
			if (_list.SelectedItems.Count == 0) { return; }
			if (_list.SelectedItems.Count > 1 && My.DialogBoxes.ShowDialogQuestion(My.ExdControls.MyResources.
						Album_dialog_ConfirmModifiyPropertiesForSeveralPictures) == DialogBoxClickResult.No) { return; }
			// Contrôles:
			TextBox txtName = new TextBox();
			TextBox txtFilename = new TextBox();
			TextBox txtCaption = new TextBox();
			txtCaption.AcceptsReturn = true;
			txtCaption.Multiline = true;
			txtCaption.ScrollBars = ScrollBars.Vertical;
			txtName.ReadOnly = txtFilename.ReadOnly = txtCaption.ReadOnly = _locked;
			TextBox txtFormat = new TextBox();
			TextBox txtSize = new TextBox();
			txtSize.ReadOnly = txtFormat.ReadOnly = true;
			// Texte par défaut du premier élément sélectionné:
			PictureInfos info = (PictureInfos)_list.SelectedItems[0].Tag;
			txtName.Text = info.Name;
			txtFilename.Text = info.Filename;
			txtCaption.Text = info.Caption;
			txtFormat.Text = String.Format("{0} - {1}", My.GeneralParser.GetImageFormatDescription(info.Picture.RawFormat),
				info.Picture.PixelFormat.ToString());
			txtSize.Text = String.Format("W: {0} - H: {1}", info.Picture.Width, info.Picture.Height);
			// Si plusieurs éléments sélectionnés, on regarde si les propriétés sont toutes identiques:
			if (_list.SelectedItems.Count > 1)
			{
				bool stopName, stopFilename, stopCaption, stopFormat, stopSize; string tmp;
				stopName = stopFilename = stopCaption = stopFormat = stopSize = false;
				foreach (ListViewItem item in _list.SelectedItems) {
					info = (PictureInfos)item.Tag;
					if (!stopName && txtName.Text != info.Name) { txtName.Text = String.Empty; stopName = true; }
					if (!stopFilename && txtFilename.Text != info.Filename) { txtFilename.Text = String.Empty; stopFilename = true; }
					if (!stopCaption && txtCaption.Text != info.Caption) { txtCaption.Text = String.Empty; stopCaption = true; } }
					tmp = String.Format("{0} - {1}", My.GeneralParser.GetImageFormatDescription(info.Picture.RawFormat),
						info.Picture.PixelFormat.ToString());
					if (!stopFormat && txtFormat.Text != tmp) { txtFormat.Text = String.Empty; stopFormat = true; }
					tmp = String.Format("W: {0} - H: {1}", info.Picture.Width, info.Picture.Height);
					if (!stopSize && txtSize.Text != tmp) { txtSize.Text = String.Empty; stopSize = true; }
				// Modification de rouge à blanc par double-click:
				txtName.DoubleClick += delegate { txtName.BackColor = (txtName.BackColor==Color.Red ? Color.White : Color.Red); }; 
				txtFilename.DoubleClick += delegate { txtFilename.BackColor = (txtFilename.BackColor==Color.Red ? Color.White : Color.Red); };
				txtCaption.DoubleClick += delegate { txtCaption.BackColor = (txtCaption.BackColor==Color.Red ? Color.White : Color.Red); };
				txtName.TextChanged += delegate { txtName.BackColor = Color.White; };
				txtFilename.TextChanged += delegate { txtFilename.BackColor = Color.White; };
				txtCaption.TextChanged += delegate { txtCaption.BackColor = Color.White; };
				txtName.BackColor = txtFilename.BackColor = txtCaption.BackColor = Color.Red;
			}
			// Affiche la boîte de dialogue:
			MyFormMultilines dialog = new MyFormMultilines();
			if (_locked) { dialog.AddButtonsCollection(DialogBoxButtons.OK, 0, true); }
			else { dialog.AddButtonsCollection(DialogBoxButtons.OKCancel, 1, true); }
			if (_list.SelectedItems.Count == 1) { dialog.SetDialogMessage(MyResources.Album_dialog_SelectedPicturesProperties); }
			else { dialog.SetDialogMessage(MyResources.Album_dialog_SelectedPicturesProperties_SeveralPictures); }
			dialog.SetDialogIcon(DialogBoxIcon.Information);
			dialog.AddLine(MyResources.Album_dialog_PictureProperties_Name, txtName);
			dialog.AddLine(MyResources.Album_dialog_PictureProperties_Filename, txtFilename);
			dialog.AddLine(MyResources.Album_dialog_PictureProperties_Caption, txtCaption, 100);
			dialog.AddLine(MyResources.Album_dialog_PictureProperties_Format, txtFormat);
			dialog.AddLine(MyResources.Album_dialog_PictureProperties_Size, txtSize);
			if (dialog.ShowDialog() == DialogBoxClickResult.Cancel || _locked) { dialog.Dispose(); return; }
			// Met à jour les propriétés:
			foreach (ListViewItem item in _list.SelectedItems) {
				if (txtName.BackColor != Color.Red) { ((PictureInfos)item.Tag).Name = txtName.Text; item.Text = txtName.Text; }
				if (txtFilename.BackColor != Color.Red) { ((PictureInfos)item.Tag).Filename = txtFilename.Text; }
				if (txtCaption.BackColor != Color.Red) { ((PictureInfos)item.Tag).Caption = txtCaption.Text; }
				item.ToolTipText = ((PictureInfos)item.Tag).ToolTipText; }
			// Le contrôle a changé:
			OnAlbumChanged();
		}
		

		/// <summary>
		/// Propose pour chaque image sélectionnée, une boîte de dialogue permettant de changer, éventuellement, le nom d'image par défaut, et de convertir l'image dans un autre format.
		/// </summary>
		protected virtual void ConvertAndSaveSelectedPictures()
		{
			foreach (ListViewItem item in _list.SelectedItems)
			{
				PictureInfos info = (PictureInfos)item.Tag;
				My.FilesAndStreams.SaveBitmap(info.Picture, info.Filename);
			}
		}


		/// <summary>
		/// Enregistre les images sélectionnées, en utilisant les noms et format preexistant.
		/// </summary>
		protected virtual void SaveSelectedPicturesUsingExistingNamesAndFormats()
		{
			// Sort si pas de sélection:
			if (_list.SelectedItems.Count == 0) { return; }
			// Demande le nom du dossier dans lequel enregistrer:
			string dir = My.FilesAndStreams.MyFolderDialog();
			if (String.IsNullOrEmpty(dir)) { return; }
			// Pour toutes les images sélectionnées
			int c = 1;
			foreach (ListViewItem item in _list.SelectedItems)
			{
				PictureInfos info = (PictureInfos)item.Tag;
				// Choisi le nom (si le fichier existe déjà, le renomme):
				int d = 1; bool firstPass = true; string filename = String.Empty;
				while (String.IsNullOrEmpty(filename) || File.Exists(filename))
				{
					if (firstPass && !String.IsNullOrEmpty(info.Filename)) {
						filename = Path.GetFileNameWithoutExtension(info.Filename); firstPass = false; }
					else if (!String.IsNullOrEmpty(info.Filename)) {
						filename = String.Format("{0} {1:00}", Path.GetFileNameWithoutExtension(info.Filename), d++); }
					else {
						filename = String.Format("picture {0:00}", c++); }
					filename = Path.Combine(dir, filename + "." + My.GeneralParser.GetExtensionFromImageFormat(info.Picture.RawFormat));
				}
				// Sauve l'image:
				try { info.Picture.Save(filename, info.Picture.RawFormat); }
				catch (Exception exc) { My.ErrorHandler.ShowError(exc); }
			}
		}


		/// <summary>
		/// Enregistre les images sélectionnées en choisissant un nouveau nom (commun, avec des numéros), et un format de sortie commun.
		/// </summary>
		protected virtual void SaveSelectedPicturesUsingNewNamesAndFormat()
		{
			int l;
			if ((l = _list.SelectedItems.Count) == 0) { return; }
			Bitmap[] pictures = new Bitmap[l];
			for (int i=0; i<l; i++) { pictures[i] = ((PictureInfos)_list.SelectedItems[i].Tag).Picture; }
			My.FilesAndStreams.SaveBitmap(pictures);
		}


		/// <summary>
		/// Supprime les images sélectionnées. Lance l'événement AlbumChanged.
		/// </summary>
		protected virtual void DeleteSelectedPictures(bool askUser)
		{
			if (askUser && DialogBoxes.ShowDialogQuestion(MyResources.Album_dialog_DeleteSelectedPictures) == DialogBoxClickResult.No) { return; }
			while (_list.SelectedItems.Count > 0) {
				_list.LargeImageList.Images.RemoveAt(_list.SelectedItems[0].ImageIndex);
				_list.Items.Remove(_list.SelectedItems[0]); }
			RebuildPictureInfosArray();
			OnAlbumChanged();
		}


		/// <summary>
		/// Supprime toutes les images. Si askUser vaut true, demande confirmation à l'user. Lance l'événement AlbumChanged.
		/// </summary>
		protected virtual void DeleteAllPictures(bool askUser)
		{
			if (askUser && DialogBoxes.ShowDialogQuestion(MyResources.Album_dialog_DeleteAllPictures) == DialogBoxClickResult.No) { return; }
			_list.Items.Clear();
			_pictInfos = new PictureInfos[0];
			_list.LargeImageList.Images.Clear();
			OnAlbumChanged();
		}
		
		/// <summary>
		/// Supprime toutes les images. Lance l'événement AlbumChanged.
		/// </summary>
		public virtual void DeleteAllPictures()
			{ DeleteAllPictures(false); }
		
		/// <summary>
		/// Change les couleurs de l'album, en demandant à l'utilisateur.
		/// </summary>
		protected virtual void ChangeAlbumColors(bool backColor)
		{
			ColorDialog dialog = new ColorDialog();
			dialog.AnyColor = true;
			dialog.Color = (backColor ? __albumBackColor : __albumForeColor);
			if (dialog.ShowDialog() == DialogResult.Cancel) { return; }
			ChangeAlbumColors(backColor, dialog.Color);
		}
		
		
		/// <summary>
		/// Change la couleur de l'album.
		/// </summary>
		public virtual void ChangeAlbumColors(bool backColor, Color color)
		{
			if (backColor) {
				__albumBackColor = _lblCaption.BackColor = _list.BackColor = _tlpMain.BackColor = color;
				_zoomForm.ProgramCustomColor = color; }
			else {
				__albumForeColor = _lblCaption.ForeColor = _list.ForeColor = color; }
		}
	
	
		/// <summary>
		/// Affiche ou masque les noms des images (i.e. le texte des items). Modifie la valeur de __showNames.
		/// </summary>
		protected void ShowNamesInList(bool value)
		{
			__showNames = ((ToolStripMenuItem)_mnuPicture.Items["showNames"]).Checked =
					((ToolStripMenuItem)_mnuList.Items["showNames"]).Checked = value; 
			foreach (ListViewItem item in _list.Items) { item.Text = (__showNames ? ((PictureInfos)item.Tag).Name : String.Empty); }
		}

	
		// ---------------------------------------------------------------------------
		// GESTIONNAIRES D'EVENEMENT:


		/// <summary>
		/// Affiche le menu contextuel de l'élément ou de la liste.
		/// </summary>
		void _list_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right) { return; }
			if (_list.SelectedItems.Count > 0) { _mnuPicture.Show(_list, e.Location); }
			else { _mnuList.Show(_list, e.Location); }
		}


		/// <summary>
		/// Dépose de fichier dans la liste: charge les images. Lance l'événement AlbumChanged.
		/// </summary>
		private void _list_ExternalDataDropped(object sender, ExternalDataDroppedEventArgs e)
		{
			// Désélectionne tout:
			_list.SelectedItems.Clear();
			string[] list = (string[])e.Args.Data.GetData(DataFormats.FileDrop);
			foreach (string s in list) { AddPicture(s, e.TargetIndex++); }
			OnAlbumChanged();
		}

		/// <summary>
		/// Autorise le drag and drop si c'est un ou des fichiers. Sinon, ne modifie pas la propriété de retour. Si Locked, annule.
		/// </summary>
		private void _list_ExternalDataDragged(object sender, DragEventArgs e)
		{
			if (_locked) { e.Effect = DragDropEffects.None; }
			else if (e.Data.GetDataPresent(DataFormats.FileDrop)) { e.Effect = DragDropEffects.Copy; }
		}


		/// <summary>
		/// Pour center les images, on change la marge de gauche de la liste, ce qui découvre le _tlpMain. Il faut donc gérer les clics sur cette partie découverte comme s'ils étaient sur la liste.
		/// </summary>
		private void _tlpMain_MouseDown(object sender, MouseEventArgs e)
		{
			_list.SelectedItems.Clear();
			if (e.Button == MouseButtons.Right) { _mnuList.Show(_tlpMain, e.Location); }
		}
		

		/// <summary>
		/// Affiche l'image en plein écran lorsque l'utilisateur double-clique sur une image.
		/// </summary>
		protected void _list_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (_list.Items.Count == 0) { return; }
			ListViewItem item = _list.GetItemAt(e.X, e.Y);
			if (item != null) { RebuildPictureInfosArray(); _zoomForm.Show(_pictInfos, item.Index); }
		}


	}



}
