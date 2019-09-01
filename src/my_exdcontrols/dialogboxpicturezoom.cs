using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace My
{


	/// <summary>
	/// Dialogue pour l'affichage d'images en grand écran.
	/// </summary>
	public class DialogBoxPictureZoom : Form
	{
	

		// ---------------------------------------------------------------------------
		// DECLARATIONS:
		
		private int _zoomFactor, _index;
		private PictureInfos[] _pictInfos;
		private PictureBox _pictBox;
		private ToolTip _toolTip;
		private bool _realSize, _adjustedSize;
		private Point _mouseDownPoint, _lastAutoScrollPosition, _deactivatedAutoScrollPosition;
		private int _backColorIndex;
		private Color[] _backColors;
		private ContextMenuStrip _mnuContext;
		private NumericUpDown _numCustomZoom;
		private ToolBoxColorDescription _toolBoxColor;


		// ---------------------------------------------------------------------------
		// PROPRIETES:
		
		
		/// <summary>
		/// Obtient le numéro de l'index de l'image affichée actuellement.
		/// </summary>
		public int CurrentIndex { get { return _index; } }
		
		/// <summary>
		/// Obtient ou définit si le mode AdjustedSize est activé. Ne fonctionne qu'en mode RealSize. Si c'est le cas, à chaque changement d'image, la taille de la fenêtre est ajustée. Sinon, la taille de la fenêtre est laissée telle quelle.
		/// </summary>
		public bool AdjustedSize {
			get { return _adjustedSize; }
			set { _adjustedSize = value; } }
		
		/// <summary>
		/// Obtient ou définit si l'image est affiché en taille réel (si false, l'image est affichée en mode zoom).
		/// </summary>
		public bool RealSize {
			get { return _realSize; }
			set { _realSize = value; SetPicture(_index); } }
	
		/// <summary>
		/// Obtient ou définit le facteur de zoom en pourcentage l'image est affichée en "taille réelle".
		/// </summary>
		public int ZoomFactor {
			get { return _zoomFactor; }
			set { _zoomFactor = (value<10 ? 10 : value); SetPicture(_index); } }
		
		/// <summary>
		/// Obtient ou définit la couleur de fond "Program" dans la liste des couleurs de fond.
		/// </summary>
		public Color ProgramCustomColor {
			get { return _backColors[4]; }
			set { _backColors[4] = value; BackColorIndex = BackColorIndex; } }

		/// <summary>
		/// Obtient ou définit la couleur de fond "User" dans la liste des couleurs de fond.
		/// </summary>
		public Color UserCustomColor {
			get { return _backColors[5]; }
			set { _backColors[5] = value; BackColorIndex = BackColorIndex; } }
		
		/// <summary>
		/// Obtient ou définit l'index de la couleur de fond courrante.
		/// </summary>
		public int BackColorIndex
		{
			get { return _backColorIndex; }
			set
			{
				_backColorIndex = value;
				if (_backColorIndex >= _backColors.Length) { _backColorIndex = 0; }
				_pictBox.BackColor = BackColor = _backColors[_backColorIndex];
				if (_mnuContext == null) { return; } int c = 0;
				foreach (ToolStripMenuItem mi in ((ToolStripMenuItem)_mnuContext.Items["backcolor"]).DropDownItems) {
					mi.Checked = (c++ == _backColorIndex); }
			}
		}


		// ---------------------------------------------------------------------------
		// CONSTRUCTEUR:


		/// <summary>
		/// Constructeur.
		/// </summary>
		public DialogBoxPictureZoom()
		{
		
			// Initialisation des variables:
			_backColors = new Color[]{Color.Black, Color.FromArgb(69,69,69), SystemColors.Control, Color.White, Color.Black,
				My.ExdControls.MySettings.DialogBoxPictureZoomDefaultBackColor};
		
			// Initialisation du form:
			AutoScroll = true;
			Icon = App.DefaultIcon;
			KeyPreview = true;
			StartPosition = FormStartPosition.CenterScreen;
			Width = MyForm.FormDefaultSize.Width;
			Height = MyForm.FormDefaultSize.Height;
			
			// Initialisation du PictureBox et du ToolTip:
			_pictBox = new PictureBox();
			_pictBox.SizeMode = PictureBoxSizeMode.Zoom;
			Controls.Add(_pictBox);
			_toolTip = new ToolTip();
			
			// Autres contrôles:
			_numCustomZoom = new NumericUpDown();
			_numCustomZoom.Minimum = 10;
			_numCustomZoom.Maximum = Int32.MaxValue;
			_toolBoxColor = new ToolBoxColorDescription();
			_toolBoxColor.KeyDowShow = true;
			_toolBoxColor.FormClosing += delegate(object sender, FormClosingEventArgs e) { e.Cancel = true; _toolBoxColor.Hide(); };
			
			// Raccourcis claviers sur Form:
			_pictBox.KeyDown += new KeyEventHandler(DialogBoxPictureZoom_KeyDown);
			// Empêche la fermeture: cache le form plutôt:
			FormClosing += delegate(object sender, FormClosingEventArgs e) { e.Cancel = true; this.Hide(); };
			// Centre l'image si la fenêtre est plus grande:
			Resize += new EventHandler(DialogBoxPictureZoom_Resize);
			// Lorsqu'on roule la souris:
			MouseWheel += new MouseEventHandler(DialogBoxPictureZoom_MouseWheel);
			// Sélectionne constamment le _pict:
			MouseMove += delegate { _pictBox.Select(); };
			_pictBox.MouseMove += delegate { _pictBox.Select(); };
			// Cache le ToolBoxColorDescription:
			VisibleChanged += delegate { if (!Visible && _toolBoxColor.Visible) { _toolBoxColor.Hide(); } };
			FormClosed += delegate { if (_toolBoxColor.Visible) { _toolBoxColor.Hide(); } };
			// Gestion du déplacement de l'image:
			// (Un right-MouseDown sur le PictureBox fait basculer l'affichage entre "taille réelle" et "pleine image à l'écran"):
			_pictBox.MouseMove += new MouseEventHandler(_pictBox_MouseMove);
			_pictBox.MouseDown += new MouseEventHandler(_pictBox_MouseDown);
			_pictBox.MouseUp += delegate { _pictBox.Cursor = Cursors.Default; };
			// Changement des couleurs de fond de _pict et du Form:
			_pictBox.BackColorChanged += delegate { BackColor = _pictBox.BackColor; };
			
			// Lorsqu'on désactive le form puis qu'on le réactive, la position du scroll revient à (0,0). On se sert
			// donc des événements suivants pour enregistrer la position et la restaurer (de plus, on rajoute dans
			// Show et ShowDialog des instructions de réinitialisation du point):
			Activated += delegate { AutoScrollPosition = new Point(-_deactivatedAutoScrollPosition.X, -_deactivatedAutoScrollPosition.Y); };
			Deactivate += delegate { _deactivatedAutoScrollPosition = AutoScrollPosition; };
			
			//  Valeurs par défaut:
			ZoomFactor = 100;
			RealSize = true;
			AdjustedSize = true;
			BackColorIndex = 0;
			
			// Menu contextuel:
			_mnuContext = new ContextMenuStrip();
			ToolStripMenuItem menuItem, subMenuItem;
			
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_NextPicture, null, menuItem_Click);
			menuItem.Name = "nextPicture";
			menuItem.ShortcutKeyDisplayString = "→/↓";
			_mnuContext.Items.Add(menuItem);
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_PreviousPicture, null, menuItem_Click);
			menuItem.Name = "previousPicture";
			menuItem.ShortcutKeyDisplayString = "←/↑";
			_mnuContext.Items.Add(menuItem);
			
			_mnuContext.Items.Add(new ToolStripSeparator());
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_ZoomMode, null, menuItem_Click);
			menuItem.Name = "zoomMode";
			menuItem.ShortcutKeyDisplayString = "Ctrl+R-Click";
			_mnuContext.Items.Add(menuItem);
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_RealSizeMode, null, menuItem_Click);
			menuItem.Name = "realSizeMode";
			menuItem.ShortcutKeyDisplayString = "Ctrl+R-Click";
			_mnuContext.Items.Add(menuItem);
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_AdjustedSizeMode, null, menuItem_Click);
			menuItem.Name = "adjustedSizeMode";
			menuItem.ShortcutKeys = Keys.Control | Keys.E;
			_mnuContext.Items.Add(menuItem);
			
			_mnuContext.Items.Add(new ToolStripSeparator());
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_Zoom);
			menuItem.Name = "zoom";
			_mnuContext.Items.Add(menuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_ZoomCustom, null, menuItem_Click);
				subMenuItem.Name = "zoomCustom";
				menuItem.DropDownItems.Add(subMenuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_AdjustToWidth, null, menuItem_Click);
				subMenuItem.Name = "zoomAdjustedToWidth";
				menuItem.DropDownItems.Add(subMenuItem);
				menuItem.DropDownItems.Add(new ToolStripSeparator());
				int[] values = new int[]{10,25,50,75,100,125,150,175,200,300};
				foreach (int i in values) {
					subMenuItem = new ToolStripMenuItem(String.Format("{0} %", i), null, menuItem_Click);
					subMenuItem.Name = "zoomPredefined"; subMenuItem.Tag = i;
					menuItem.DropDownItems.Add(subMenuItem); }
				
			_mnuContext.Items.Add(new ToolStripSeparator());
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_Rotate);
			menuItem.Name = "rotate";
			_mnuContext.Items.Add(menuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_Rotate180, null, menuItem_Click);
				subMenuItem.Name = "rotate180";
				menuItem.DropDownItems.Add(subMenuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_Rotate90ClockWise, null, menuItem_Click);
				subMenuItem.Name = "rotate90clockwise";
				menuItem.DropDownItems.Add(subMenuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_Rotate90CounterClockWise, null, menuItem_Click);
				subMenuItem.Name = "rotate90counterClockwise";
				menuItem.DropDownItems.Add(subMenuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_RotateNone, null, menuItem_Click);
				subMenuItem.Name = "rotateNone";
				menuItem.DropDownItems.Add(subMenuItem);
				
			_mnuContext.Items.Add(new ToolStripSeparator());
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_BackColor);
			menuItem.Name = "backcolor";
			_mnuContext.Items.Add(menuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_BackColorBlack, null, menuItem_Click);
				subMenuItem.Name = "backcolorBlack";
				menuItem.DropDownItems.Add(subMenuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_BackColorDarkGray, null, menuItem_Click);
				subMenuItem.Name = "backcolorDarkGray";
				menuItem.DropDownItems.Add(subMenuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_BackColorControlColor, null, menuItem_Click);
				subMenuItem.Name = "backcolorControlColor";
				menuItem.DropDownItems.Add(subMenuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_BackColorWhite, null, menuItem_Click);
				subMenuItem.Name = "backcolorWhite";
				menuItem.DropDownItems.Add(subMenuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_BackColorProgramColor, null, menuItem_Click);
				subMenuItem.Name = "backcolorProgramColor";
				menuItem.DropDownItems.Add(subMenuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_BackColorUserColor, null, menuItem_Click);
				subMenuItem.Name = "backcolorUserColor";
				menuItem.DropDownItems.Add(subMenuItem);
				subMenuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_BackColorChangeUserColor, null, menuItem_Click);
				subMenuItem.Name = "backcolorChangeUserColor";
				menuItem.DropDownItems.Add(subMenuItem);
			
			_mnuContext.Items.Add(new ToolStripSeparator());
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_ShowColorDescription, null, menuItem_Click);
			menuItem.Name = "showColorDescription";
			_mnuContext.Items.Add(menuItem);
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_OtherCommands, null, menuItem_Click);
			menuItem.Name = "otherCommands";
			menuItem.ShortcutKeys = Keys.F1;
			_mnuContext.Items.Add(menuItem);			
			menuItem = new ToolStripMenuItem(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_Hide, null, menuItem_Click);
			menuItem.Name = "hide";
			menuItem.ShortcutKeyDisplayString = "Escape";
			_mnuContext.Items.Add(menuItem);
			
			ContextMenuStrip = _mnuContext;

		}
		

		// ---------------------------------------------------------------------------
		// METHODES:
		
		
		/// <summary>
		/// Affiche l'image dont l'index correspond à l'argument. Modifie la taille et/ou la disposition du form en fonction des propriétés et change au besoin le titre de la fenêtre. Si index est supérieur à l'index maximum, il est remis à 0. S'il est inférieur à 0, il est remis à l'index maximum. Modifie la variable _index en conséquence.
		/// </summary>
		private void SetPicture(int index)
		{
		
			// Sort si _pictInfos n'est pas défini (lors de l'initialisation du form):
			if (_pictInfos == null) { return; }
		
			// Contrôle de l'index et sort si pas d'image dans le tableau::
			if (index >= _pictInfos.Length) { index = 0; }
			if (index < 0) { index = _pictInfos.Length - 1; }
			if ((_index = index) < 0) { return; }
			
			// Définit l'image dans le PictureBox en fonction de la rotation demandé:
			Bitmap bmp;
			if (_pictInfos[_index].ZoomRotation == RotateFlipType.RotateNoneFlipNone) {
				bmp = _pictInfos[_index].Picture; }
			else {
				bmp = new Bitmap(_pictInfos[_index].Picture);
				bmp.RotateFlip(_pictInfos[_index].ZoomRotation); }
			_pictBox.Image = bmp;
			
			// Contrôle le zoom:
			if (_zoomFactor < 10) { _zoomFactor = 10; }
			
			// Enlève l'événement:
			Resize -= DialogBoxPictureZoom_Resize;
			
			// Si pas mode RealSize:
			if (!_realSize)
			{
				_pictBox.Dock = DockStyle.Fill;
			}
			
			// Si mode RealSize:
			else
			{
				// Redim du PictureBox:
				_pictBox.Dock = DockStyle.None;
				try {
					_pictBox.Width = (int)(bmp.Width * _zoomFactor / 100.0);
					_pictBox.Height = (int)(bmp.Height * _zoomFactor / 100.0); }
				catch (Exception exc) { My.ErrorHandler.ShowError(exc); _zoomFactor = 10; this.Close(); }
				_pictBox.Location = new Point(0, 0);
				// Si AdjustedSize : Si le PictureBox fait plus de 90% de l'écran, maximise la fenêtre, sinon
				// met la tailel de la fenêtre à la taille du PictureBox:
				if (_adjustedSize)
				{
					int pH = _pictBox.ClientSize.Height, pW = _pictBox.ClientSize.Width;
					int sH = Screen.PrimaryScreen.WorkingArea.Height, sW = Screen.PrimaryScreen.WorkingArea.Width;
					if (pH > sH * 0.9 || pW > sW * 0.9) { WindowState = FormWindowState.Maximized; }
					else { WindowState = FormWindowState.Normal; ClientSize = _pictBox.Size; }
				}
				// Centre l'image si l'image n'est pas plus grande que la fenêtre:
				if (ClientSize.Width > _pictBox.Width) { AutoScrollPosition = new Point(0, 0);
					_pictBox.Left = (int)((ClientSize.Width - _pictBox.Width) / 2.0); }
				else { _pictBox.Left = AutoScrollPosition.X; }
				if (ClientSize.Height > _pictBox.Height) { AutoScrollPosition = new Point(0, 0);
					_pictBox.Top = (int)((ClientSize.Height - _pictBox.Height) / 2.0); }
				else { _pictBox.Top = AutoScrollPosition.Y; }
			}

			// Empêche le redimensionnement de l'image si mode AdjustedSize:
			FormBorderStyle = (_realSize && _adjustedSize ? FormBorderStyle.Fixed3D : FormBorderStyle.Sizable);

			// Légende de l'image dans le ToolTip:
			_toolTip.SetToolTip(_pictBox,
				(!String.IsNullOrEmpty(_pictInfos[_index].ToolTipText) ? _pictInfos[_index].ToolTipText + "\n\n" : String.Empty)
				+ My.ExdControls.MyResources.DialogBoxPictureZoom_ToolTipHelp);
			
			// Texte du form:
			string mode;
			if (_realSize) {
				mode = String.Format("{0} {1}%", My.ExdControls.MyResources.DialogBoxPictureZoom_FormText_RealSizeMode, _zoomFactor);
				if (_adjustedSize) { mode += String.Format(" - {0}", My.ExdControls.MyResources.DialogBoxPictureZoom_FormText_AdjustedSizeMode); } }
			else {
				mode = String.Format("{0}", My.ExdControls.MyResources.DialogBoxPictureZoom_FormText_ZoomMode); }
			string text = String.Format("{0} - {1}", mode, My.App.Title);
			if (!String.IsNullOrEmpty(_pictInfos[_index].Name)) { text = String.Format("\"{0}\" - {1}", _pictInfos[_index].Name, text); }
			Text = text;

			// Remet l'événement:
			Resize += DialogBoxPictureZoom_Resize;

			// Gestion des menus:
			if (_zoomFactor < _numCustomZoom.Minimum) { _numCustomZoom.Value = _numCustomZoom.Minimum; }
			else { _numCustomZoom.Value = _zoomFactor; }
			((ToolStripMenuItem)_mnuContext.Items["zoomMode"]).Checked = !_realSize;
			((ToolStripMenuItem)_mnuContext.Items["realSizeMode"]).Checked = _realSize;
			((ToolStripMenuItem)_mnuContext.Items["adjustedSizeMode"]).Checked = _adjustedSize;
			_mnuContext.Items["adjustedSizeMode"].Enabled = _realSize;
			((ToolStripMenuItem)((ToolStripMenuItem)_mnuContext.Items["rotate"]).DropDownItems["rotate180"]).Checked = 
				(_pictInfos[_index].ZoomRotation == RotateFlipType.Rotate180FlipNone);
			((ToolStripMenuItem)((ToolStripMenuItem)_mnuContext.Items["rotate"]).DropDownItems["rotate90clockwise"]).Checked = 
				(_pictInfos[_index].ZoomRotation == RotateFlipType.Rotate90FlipNone);
			((ToolStripMenuItem)((ToolStripMenuItem)_mnuContext.Items["rotate"]).DropDownItems["rotate90counterClockwise"]).Checked = 
				(_pictInfos[_index].ZoomRotation == RotateFlipType.Rotate270FlipNone);
			((ToolStripMenuItem)((ToolStripMenuItem)_mnuContext.Items["rotate"]).DropDownItems["rotateNone"]).Checked = 
				(_pictInfos[_index].ZoomRotation == RotateFlipType.RotateNoneFlipNone);

		}


		/// <summary>
		/// Méthode d'affichage du form. Il faut passer les tableaux ainsi que l'index de l'image à afficher.
		/// </summary>
		public void Show(PictureInfos[] pictInfos, int index)
		{
			_pictInfos = pictInfos;
			_deactivatedAutoScrollPosition = new Point(0, 0);
			SetPicture(index);
			base.Show();
		}


		/// <summary>
		/// Voir la méthode Show.
		/// </summary>
		public void ShowDialog(PictureInfos[] pictInfos, int index)
		{
			_pictInfos = pictInfos;
			_deactivatedAutoScrollPosition = new Point(0, 0);
			SetPicture(index);
			base.ShowDialog();
		}


		/// <summary>
		/// Raccourcis claviers : Flèches pour changer d'image, Alt+Flèches pour changer le zoom, etc..
		/// </summary>
		protected void DialogBoxPictureZoom_KeyDown(object sender, KeyEventArgs e)
		{
			
			// Supprime la gestion des touches (ce qui évite que Alt=RButton + ShiftKey) ne viennent déranger
			// les raccourcis quand on l'utilise pour un wheel (sinon, ça ce met en mode "menu"):
			if (e.KeyCode != Keys.F4 || e.Modifiers != Keys.Alt) { e.SuppressKeyPress = true; e.Handled = true; }
			
			// Si Echap, sort:
			if (e.KeyCode == Keys.Escape) { this.Close(); }
			
			// Basculement entre les images:
			if (e.Modifiers == Keys.None && (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down))
				{ SetPicture(_index + 1); }
			if (e.Modifiers == Keys.None && (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up))
				{ SetPicture(_index - 1); }

			// Changement du facteur de zoom:
			if (_realSize && (e.Modifiers == Keys.Alt || e.Modifiers == (Keys.Alt | Keys.Control))) {
				int mov = 10;
				if (e.Modifiers == (e.Modifiers | Keys.Control)) { mov = 1; }
				if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down) { _zoomFactor += mov; SetPicture(_index); }
				if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up) { _zoomFactor -= mov; SetPicture(_index); } }
			
		}
		
		
		/// <summary>
		/// Gestion du mouvement de la roulette de la souris:
		/// </summary>
		protected void DialogBoxPictureZoom_MouseWheel(object sender, MouseEventArgs e)
		{
		
			// Changement du facteur de zoom:
			if (_realSize && (Control.ModifierKeys == Keys.Alt || Control.ModifierKeys == (Keys.Alt | Keys.Control))) {
				int mov = 10;
				if (Control.ModifierKeys == (Control.ModifierKeys | Keys.Control)) { mov = 1; }
				if (e.Delta < 0) { _zoomFactor -= mov; SetPicture(_index); }
				else { _zoomFactor += mov; SetPicture(_index); }
				return; }
			
			// Déroulement horizontal:
			if (Control.ModifierKeys == Keys.Control && _realSize)
			{
				if (e.Delta < 0)
					{ AutoScrollPosition = new Point(Math.Abs(AutoScrollPosition.X) + 120, Math.Abs(AutoScrollPosition.Y) - 120); }
				else
					{ AutoScrollPosition = new Point(Math.Abs(AutoScrollPosition.X) - 120, Math.Abs(AutoScrollPosition.Y) + 120); }
				return;
			}
			
			// Changement d'image si pas RealSize:
			if (!_realSize)
			{
				if (e.Delta < 0) { SetPicture(_index + 1); }
				else { SetPicture(_index - 1); }
			}
			
		}
		
		
		/// <summary>
		/// Permet de déplacer l'image par simple "glisser-déplacer" de la souris, avec bouton gauche de la souris.
		/// </summary>
		private void _pictBox_MouseMove(object sender, MouseEventArgs e)
		{
			// Sort si pas RealSize, ou si pas bouton gauche enfoncé:
			if (e.Button != MouseButtons.Left ||!_realSize) { return; }
			// Enregistre les nouvelles positions de la souris, et contrôles les valeurs limites:
			_lastAutoScrollPosition.X = _lastAutoScrollPosition.X + (_mouseDownPoint.X - e.X);
			_lastAutoScrollPosition.Y = _lastAutoScrollPosition.Y + (_mouseDownPoint.Y - e.Y);
			if (_lastAutoScrollPosition.X < 0) { _lastAutoScrollPosition.X = 0; }
			if (_lastAutoScrollPosition.Y < 0) { _lastAutoScrollPosition.Y = 0; }
			if (_lastAutoScrollPosition.X > _pictBox.Width) { _lastAutoScrollPosition.X = _pictBox.Width; }
			if (_lastAutoScrollPosition.Y > _pictBox.Height) { _lastAutoScrollPosition.Y = _pictBox.Height; }
			// Déplace la scroll position:
			AutoScrollPosition = _lastAutoScrollPosition;
		}


		/// <summary>
		/// Lorsque l'utilisateur presse sur le bouton gauche, enregistre la position du curseur pour pouvoir calculer ensuite le déplacement de l'image si l'utilisateur bouge la souris en maintenant le bouton appuyer. Si bouton droit, change entre RealSize et non RealSize.
		/// </summary>
		private void _pictBox_MouseDown(object sender, MouseEventArgs e)
		{
			// Si bouton de gauche et Ctrl enfoncé, on change le mode:
			if (Control.ModifierKeys == Keys.Control && e.Button == MouseButtons.Left) {
				RealSize = !RealSize; }
			// Sinon, si RealSize et si bouton gauche, prépare le déplacement de l'image:
			else if (e.Button == MouseButtons.Left && _realSize) {
				_mouseDownPoint = e.Location;
				_pictBox.Cursor = Cursors.SizeAll; }
		}
		
		
		/// <summary>
		/// Replace l'image pour l'adapter à la nouvelle taille.
		/// </summary>
		private void DialogBoxPictureZoom_Resize(object sender, EventArgs e)
			{ SetPicture(_index); }
	
	
		/// <summary>
		/// Gestion du menu contextuel.
		/// </summary>
		private void menuItem_Click(object sender, EventArgs e)
		{
			string name = ((ToolStripMenuItem)sender).Name;
			switch (name)
			{
				case "nextPicture":
					SetPicture(_index + 1); break;
				case "previousPicture":
					SetPicture(_index - 1); break;
				case "zoomMode":
					_realSize = false; SetPicture(_index); break;
				case "realSizeMode":
					_realSize = true; SetPicture(_index); break;
				case "adjustedSizeMode":
					_adjustedSize = !_adjustedSize; SetPicture(_index); break;
				case "zoomPredefined":
					_zoomFactor = (int)((ToolStripMenuItem)sender).Tag; SetPicture(_index); break;
				case "zoomAdjustedToWidth":
					_zoomFactor = (int)(100 * ClientSize.Width / _pictInfos[_index].Picture.Width); SetPicture(_index); break;
				case "zoomCustom":
					if (My.DialogBoxes.ShowDialogInputCtrl(My.ExdControls.MyResources.DialogBoxPictureZoom_menu_ZoomCustom, _numCustomZoom)
						== DialogBoxClickResult.OK) { _zoomFactor = (int)_numCustomZoom.Value; SetPicture(_index); } break;
				case "rotate180":
					_pictInfos[_index].ZoomRotation = RotateFlipType.Rotate180FlipNone; SetPicture(_index); break;
				case "rotate90clockwise":
					_pictInfos[_index].ZoomRotation = RotateFlipType.Rotate90FlipNone; SetPicture(_index); break;
				case "rotate90counterClockwise":
					_pictInfos[_index].ZoomRotation = RotateFlipType.Rotate270FlipNone; SetPicture(_index); break;
				case "rotateNone":
					_pictInfos[_index].ZoomRotation = RotateFlipType.RotateNoneFlipNone; SetPicture(_index); break;
				case "backcolorBlack":
					BackColorIndex = 0; break;
				case "backcolorDarkGray":
					BackColorIndex = 1; break;
				case "backcolorControlColor":
					BackColorIndex = 2; break;
				case "backcolorWhite":
					BackColorIndex = 3; break;
				case "backcolorProgramColor":
					BackColorIndex = 4; break;
				case "backcolorUserColor":
					BackColorIndex = 5; break;
				case "backcolorChangeUserColor":
					ColorDialog dialog = new ColorDialog(); dialog.Color = _backColors[5];
					if (dialog.ShowDialog() == DialogResult.OK) { _backColors[5] = dialog.Color; }  break;
				case "showColorDescription":
					if (_toolBoxColor.Visible) { _toolBoxColor.Hide(); } else { _toolBoxColor.Show(); }
					break;
				case "otherCommands":
					My.DialogBoxes.ShowDialogMessage(My.ExdControls.MyResources.DialogBoxPictureZoom_Help); break;
				case "hide":
					Close(); break;
			}
		}


	}
	
	
	
}
