using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace My
{






	// ---------------------------------------------------------------------------
	// TYPES
	// ---------------------------------------------------------------------------




	#region TYPES



	/// <summary>
	/// Enumération des boutons prédéfinis des DialogBox.
	/// </summary>
	public enum DialogBoxButtons
	{
		OK,
		OKCancel,
		Cancel,
		YesNo,
		YesNoCancel
	}


	/// <summary>
	/// Enumération des icônes de message des DialogBox.
	/// </summary>
	public enum DialogBoxIcon
	{
		None,
		Connection,
		Error,
		Exclamation,
		Forbidden,
		Help,
		Information,
		Locked,
		Question,
		Search,
		Waiting
	}


	/// <summary>
	/// Enumération des boutons de retour pour les boutons prédéfinis des DialogBox.
	/// </summary>
	public enum DialogBoxClickResult
	{
		None,
		OK,
		Cancel,
		Yes,
		No
	}
	
	/// <summary>
	/// Enumération qui, lorsqu'elle est dans le Tag d'un Button, définit si le bouton est un AcceptButton et/ou un CancelButton.
	/// </summary>
	public enum DialogBoxTagButton
	{
		None,
		Accept,
		Cancel,
		AcceptCancel
	}
	

	// ---------------------------------------------------------------------------


	/// <summary>
	/// Collection de boutons pour MyFormButtons. Les boutons sont redimmensionnés à la taille par défaut, et leurs propriétés Top et Left sont modifiées en fonction de leur position. Le bouton d'index 0 est le bouton le plus à gauche.
	/// </summary>
	public class ButtonsCollection : IEnumerator
	{
	
		// Largeur bouton et distance entre boutons:
		private static readonly int _distance;
		// Décalage des boutons par rapport au haut du panel:
		private static readonly int _top;
		// Décalage des boutons par rapport au côté droit du panel:
		private static readonly int _rightMargin;
		// Autres variables:
		private Button[] _buttons;
		private int _enumeratorIndex, _defaultButton;
		private int _panelWidth;
		
		/// <summary>
		/// Obtient ou définit le bouton par défaut dans la collection.
		/// </summary>
		public int DefaultButton {
			get { return _defaultButton; }
			set { _defaultButton = value; } }
		
		/// <summary>
		/// Obtient le nombre de boutons.
		/// </summary>
		public int Count { get { return _buttons.Length; } }
		
		/// <summary>
		/// Obtient la largeur du panel.
		/// </summary>
		public int PanelWidth { get { return _panelWidth; } }
		
		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static ButtonsCollection()
		{
			_distance = My.MyForm.ButtonDefaultSize.Width + 10;
			_top = 6;
			_rightMargin = 20;
		}
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public ButtonsCollection()
			{ _buttons = new Button[0]; }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public ButtonsCollection(int def, params Button[] cmds) : this()
			{ Add(def, cmds); }
		
		/// <summary>
		/// Ajoute un bouton.
		/// </summary>
		public void Add(Button cmd)
		{
			// Ajoute au tableau:
			int l = _buttons.Length;
			Array.Resize(ref _buttons, l + 1);
			_buttons[l] = cmd;
			// Positionne dans le panel:
			_panelWidth = (l + 1) * _distance + _rightMargin;
			cmd.Size = My.MyForm.ButtonDefaultSize;
			cmd.Top = _top;
			int left = 0;
			for (int i=l; i>=0; i--) { _buttons[i].Left = left; left += _distance; }
		}
		
		/// <summary>
		/// Ajoute des boutons.
		/// </summary>
		public void Add(int def, params Button[] cmds)
			{ _defaultButton = def; foreach (Button cmd in cmds) { Add(cmd); } }
		
		/// <summary>
		/// Ajoute un bouton.
		/// </summary>
		public Button Add(string text, EventHandler click)
			{ return Add(text, click, false, false); }

		/// <summary>
		/// Ajoute un bouton.
		/// </summary>
		public Button Add(string text, EventHandler click, bool isAccept, bool isCancel)
		{
			Button cmd = new Button();
			cmd.Text = text;
			cmd.Click += click;
			if (isAccept && isCancel) { cmd.Tag = DialogBoxTagButton.AcceptCancel; }
			else if (isAccept) { cmd.Tag = DialogBoxTagButton.Accept; }
			else if (isCancel) { cmd.Tag = DialogBoxTagButton.Cancel; }
			Add(cmd);
			return cmd;
		}

		/// <summary>
		/// Obtient un bouton de la collection.
		/// </summary>
		public Button GetButton(int index)
			{ return _buttons[index]; }
		
		// METHODES D'ENUMARATION:
		public IEnumerator GetEnumerator() { _enumeratorIndex = -1; return this; }
		public bool MoveNext() { return ++_enumeratorIndex < _buttons.Length; }
		public void Reset() 	{ _enumeratorIndex = -1; }
		public object Current { get { return _buttons[_enumeratorIndex]; } }
	
	}
	

	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Classe de paramètres d'événément pour DialogBoxProgressShown.
	/// </summary>
	public class DialogBoxProgressShownEventArgs : EventArgs
	{
		public object[] Parameters { get; set; }
		public DialogBoxProgress Dialog { get; set; }
		public object Result { get; set; }
		public DialogBoxProgressShownEventArgs(DialogBoxProgress dlg, object[] parameters)
			{ Dialog = dlg; Parameters = parameters; }
	}
	
	/// <summary>
	/// Délégué pour l'événement DialogBoxProgressShown.
	/// </summary>
	public delegate void DialogBoxProgressShownEventHandler(object sender, DialogBoxProgressShownEventArgs e);

	/// <summary>
	/// Classe de paramètres d'événément pour DialogBoxProgressCanceled.
	/// </summary>
	public class DialogBoxProgressCanceledEventArgs : EventArgs
	{
		public DialogBoxProgress Dialog { get; set; }
		public object Result { get; set; }
		public DialogBoxProgressCanceledEventArgs(DialogBoxProgress dlg, object result)
			{ Dialog = dlg; Result = result; }
	}
	
	/// <summary>
	/// Délégué pour l'événement DialogBoxProgressShown.
	/// </summary>
	public delegate void DialogBoxProgressCanceledEventHandler(object sender, DialogBoxProgressCanceledEventArgs e);



	#endregion TYPES




	// ===========================================================================



	/// <summary>
	/// Hérite de MyForm. Ajoute une dans _tlpForm une ligne, en bas, pour l'introduction de boutons.
	/// </summary>
	public abstract class MyFormButtons : MyForm
	{


		// ---------------------------------------------------------------------------
		// DECLARATIONS:
		
		private int _defaultButton;
		private ButtonsCollection[] _buttonsColl;
		private Panel _pnlButtons;
		private int _currentButtonsColl;
		protected DialogBoxClickResult _clickResult;


		// ---------------------------------------------------------------------------
		// PROPRIETES:

		/// <summary>
		/// Obtient ou définit le bouton par défaut de gauche à droite.
		/// </summary>
		public int DefaultButton {
			get { return _defaultButton; }
			set { _defaultButton = value; } }

		/// <summary>
		/// Valeur du bouton sur lequel l'utilisateur a cliqué pour sortir de la boîte. Ne fonctionne que pour les boutons par défaut.
		/// </summary>
		public DialogBoxClickResult ClickResult {
			get { return _clickResult; }
			set { _clickResult = value; } }

		/// <summary>
		/// Obtient le tableau des collections des boutons.
		/// </summary>
		public ButtonsCollection[] ButtonsCollections { get { return _buttonsColl; } }	
		
		/// <summary>
		/// Obtient l'index de la collection de boutons actuellement affichée.
		/// </summary>
		public int CurrentButtonsCollection { get { return _currentButtonsColl; } }	
		
		
		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS:


		/// <summary>
		/// Constructeur.
		/// </summary>
		public MyFormButtons(bool showTitleBar=true) : base(showTitleBar)
		{
		
			// Initialisation des variables:
			_buttonsColl = new ButtonsCollection[0];
			_clickResult = DialogBoxClickResult.None;
			_defaultButton = 0;
			_currentButtonsColl = -1;
			
			// Intialisation de la barre de boutons:
			_pnlButtons = new Panel();
			_pnlButtons.Dock = DockStyle.Right;
			_pnlButtons.Height = 40;

			// Ajoute la barre de boutons au _tlpForm:
			_tlpForm.RowCount = 3;
			_tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
			_tlpForm.Controls.Add(_pnlButtons, 0, 2);

			// Evenement Paint:
			_tlpForm.Paint += delegate(object sender, PaintEventArgs e) {
				PointF startPt = new PointF(_tlpForm.Left, _tlpForm.ClientSize.Height - _tlpForm.RowStyles[2].Height);
				PointF endPt = new PointF(_tlpForm.Right, _tlpForm.ClientSize.Height - _tlpForm.RowStyles[2].Height);
				DrawShadedLine(e.Graphics, startPt, endPt); };

		}


		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES:
		
		
		/// <summary>
		/// Ajout une collection de boutons à la collection des collections. Si show est vrai, affiche les boutons dans le form:
		/// </summary>
		public void AddButtonsCollection(ButtonsCollection coll, bool show)
		{
			// Ajoute au tableau:
			int l = _buttonsColl.Length;
			Array.Resize(ref _buttonsColl, l + 1);
			_buttonsColl[l] = coll;
			// Place sur le panel:
			if (show) { ShowButtonsCollection(l); }
		}
		
		/// <summary>
		/// Ajoute une collection prédéfinie de boutons. Si show est vrai, affiche les boutons dans le form:
		/// </summary>
		public void AddButtonsCollection(DialogBoxButtons type, int defButton, bool show)
		{
			// Nouvelle collection:
			ButtonsCollection coll = new ButtonsCollection();
			coll.DefaultButton = defButton;
			// Ajoute les bons boutons:
			Button cmd;
			switch (type)
			{
				case DialogBoxButtons.OK:
					cmd = new Button();
					cmd.Text = MyResources.Common_cmd_OK;
					cmd.Click += delegate { _clickResult = DialogBoxClickResult.OK; Hide(); };
					cmd.Tag = DialogBoxTagButton.AcceptCancel;
					coll.Add(cmd);
					break;
				case DialogBoxButtons.Cancel:
					cmd = new Button();
					cmd.Text = MyResources.Common_cmd_Cancel;
					cmd.Click += delegate { _clickResult = DialogBoxClickResult.Cancel; Hide(); };
					cmd.Tag = DialogBoxTagButton.AcceptCancel;
					coll.Add(cmd);
					break;
				case DialogBoxButtons.OKCancel:
					cmd = new Button();
					cmd.Text = MyResources.Common_cmd_Cancel;
					cmd.Click += delegate { _clickResult = DialogBoxClickResult.Cancel; Hide(); };
					cmd.Tag = DialogBoxTagButton.Cancel;
					coll.Add(cmd);
					cmd = new Button();
					cmd.Text = MyResources.Common_cmd_OK;
					cmd.Click += delegate { _clickResult = DialogBoxClickResult.OK; Hide(); };
					cmd.Tag = DialogBoxTagButton.Accept;
					coll.Add(cmd);
					break;
				case DialogBoxButtons.YesNo:
					cmd = new Button();
					cmd.Text = MyResources.Common_cmd_No;
					cmd.Click += delegate { _clickResult = DialogBoxClickResult.No; Hide(); };
					cmd.Tag = DialogBoxTagButton.Cancel;
					coll.Add(cmd);
					cmd = new Button();
					cmd.Text = MyResources.Common_cmd_Yes;
					cmd.Click += delegate { _clickResult = DialogBoxClickResult.Yes; Hide(); };
					cmd.Tag = DialogBoxTagButton.Accept;
					coll.Add(cmd);
					break;
				case DialogBoxButtons.YesNoCancel:
					cmd = new Button();
					cmd.Text = MyResources.Common_cmd_Cancel;
					cmd.Click += delegate { _clickResult = DialogBoxClickResult.Cancel; Hide(); };
					cmd.Tag = DialogBoxTagButton.Cancel;
					coll.Add(cmd);
					cmd = new Button();
					cmd.Text = MyResources.Common_cmd_No;
					cmd.Click += delegate { _clickResult = DialogBoxClickResult.No; Hide(); };
					coll.Add(cmd);
					cmd = new Button();
					cmd.Text = MyResources.Common_cmd_Yes;
					cmd.Click += delegate { _clickResult = DialogBoxClickResult.Yes; Hide(); };
					cmd.Tag = DialogBoxTagButton.Accept;
					coll.Add(cmd);
					break;
			}
			// Ajoute au tableau:
			AddButtonsCollection(coll, show);
		}

		/// <summary>
		/// Affiche sur le panel des boutons la collection désignée par l'index. Ne fait rien si elle est déjà affichée.
		/// </summary>
		public void ShowButtonsCollection(int index)
		{
			if (index == _currentButtonsColl) { return; }
			// Vide le form:
			while (_pnlButtons.Controls.Count > 0) { _pnlButtons.Controls.RemoveAt(0); }
			_pnlButtons.Width = _buttonsColl[index].PanelWidth;
			AcceptButton = null; CancelButton = null;
			// Ajoute les nouveaux boutons:
			foreach (Button cmd in _buttonsColl[index]) {
				_pnlButtons.Controls.Add(cmd);
				if (cmd.Tag is DialogBoxTagButton)
				{
					switch ((DialogBoxTagButton)cmd.Tag) {
						case DialogBoxTagButton.Accept: AcceptButton = cmd; break;
						case DialogBoxTagButton.Cancel: CancelButton = cmd; break;
						case DialogBoxTagButton.AcceptCancel: AcceptButton = cmd; CancelButton = cmd; break; }
				}
			}
			// Sélectionne le bon bouton:
			_buttonsColl[index].GetButton(_buttonsColl[index].DefaultButton).Select();
			_currentButtonsColl = index;
		}
		
		/// <summary>
		/// Obtient la collection spécifié.
		/// </summary>
		public virtual ButtonsCollection GetButtonsCollection(int index)
			{ return _buttonsColl[index]; }
		
		/// <summary>
		/// Affiche le form.
		/// </summary>
		public virtual new DialogBoxClickResult ShowDialog()
		{
			_clickResult = DialogBoxClickResult.None;
			base.ShowDialog();
			return _clickResult;
		}

		/// <summary>
		/// Affiche le form.
		/// </summary>
		public virtual new DialogBoxClickResult ShowDialog(IWin32Window owner)
		{
			_clickResult = DialogBoxClickResult.None;
			base.ShowDialog(owner);
			return _clickResult;
		}


	}



	// ===========================================================================



	/// <summary>
	/// Hérite de MyFormButtons. Ajoute une marge à gauche, avec une icône, dans _tlpBase. Les contrôles sont alors à ajouter dans le _tlpBase, première ligne et deuxième colonne, soit _tlpBase.Controls.Add(control, 1, 0).
	/// </summary>
	public abstract class MyFormIcon : MyFormButtons
	{


		// ---------------------------------------------------------------------------
		// DECLARATIONS:
		
		/// <summary>
		/// Contrôle sur lequel est affiché l'image. Il est normalement remplit avec une image prédéfinie, mais on peut y mettre une autre image.
		/// </summary>
		private PictureBox _pictIconMessage;
		private DialogBoxIcon _lastIcon;


		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS:

		/// <summary>
		/// Constructeur.
		/// </summary>
		public MyFormIcon(bool showTitleBar=true) : base(showTitleBar)
		{
			// Ajoute une colonne à _tlpBase, et insère le PictureBox:
			_tlpBase.ColumnCount = 2;
			_tlpBase.ColumnStyles.Insert(0, new ColumnStyle(SizeType.Absolute, 55F));
			_pictIconMessage = new PictureBox();
			_pictIconMessage.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			_pictIconMessage.Size = new Size(48, 48);
			_pictIconMessage.Margin = new Padding(0);
			_pictIconMessage.SizeMode = PictureBoxSizeMode.StretchImage;
			_tlpBase.Controls.Add(_pictIconMessage, 0, 0);
		}
		

		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES:
		
		/// <summary>
		/// Affiche une image prédéfinie dans le PictureBox. Si l'image est la même que déjà affichée, ne fait rien.
		/// </summary>
		public virtual void SetDialogIcon(DialogBoxIcon icon)
		{
			if (_lastIcon == icon) { return; }
			_lastIcon = icon;
			switch (icon)
			{
				case DialogBoxIcon.None:
					_pictIconMessage.Image = null; break;
				case My.DialogBoxIcon.Connection:
					_pictIconMessage.Image = MyResources.DialogBox_img_Connection_png; break;
				case My.DialogBoxIcon.Error:
					_pictIconMessage.Image = MyResources.DialogBox_img_Error_png; break;
				case My.DialogBoxIcon.Exclamation:
					_pictIconMessage.Image = MyResources.DialogBox_img_Exclamation_png; break;
				case My.DialogBoxIcon.Forbidden:
					_pictIconMessage.Image = MyResources.DialogBox_img_Forbidden_png; break;
				case My.DialogBoxIcon.Help:
					_pictIconMessage.Image = MyResources.DialogBox_img_Help_png; break;
				case My.DialogBoxIcon.Information:
					_pictIconMessage.Image = MyResources.DialogBox_img_Information_png; break;
				case My.DialogBoxIcon.Locked:
					_pictIconMessage.Image = MyResources.DialogBox_img_Locked_png; break;
				case My.DialogBoxIcon.Question:
					_pictIconMessage.Image = MyResources.DialogBox_img_Question_png; break;
				case My.DialogBoxIcon.Search:
					_pictIconMessage.Image = MyResources.DialogBox_img_Search_png; break;
				case My.DialogBoxIcon.Waiting:
					_pictIconMessage.Image = MyResources.DialogBox_img_Waiting_png; break;
			}
		}
		
		/// <summary>
		/// Affiche une image dans le PictureBox. Elle doit avoir une taille de 48*48.
		/// </summary>
		public virtual void SetDialogIcon(Image img)
		{
			_lastIcon = DialogBoxIcon.None;
			_pictIconMessage.Image = img;
		}

	}



	// ===========================================================================



	/// <summary>
	/// Hérite de MyFormIcon. Ajoute un _tlpBody de deux lignes, l'une contenant un message se dimensionnant automatiquement, et l'autre laissé vide, sur lequel les héritiers peuvent ajouter des contrôles, ou bien masqué totalement. La première ligne est automatiquement ajusté en hauteur pour contenir le texte. S'il n'y a pas de bouton spécifié lors du premier appel de ShowDialog, alors par défaut le bouton OK est inséré.
	/// </summary>
	public class MyFormMessage : MyFormIcon
	{

		// ---------------------------------------------------------------------------
		// DECLARATIONS


		/// <summary>
		/// Ce TLP a deux lignes, la première contenant le panel du message. Les contrôles sont donc à ajouter dans la deuxième ligne : _tlpBody.Controls.Add(something, 0, 1).
		/// </summary>
		protected TableLayoutPanel _tlpBody;
		
		/// <summary>
		/// Panel contenant le message.
		/// </summary>
		private Panel _pnlDlgMessage;
		
		// Autres variables:
		private Label _lblDlgMessage;
		private TextBox _txtDlgMessage;
		private int _lblMarginX, _lblMarginY, _tlpBodyMarginY;
		private bool _oneLineDlgMessage;


		// ---------------------------------------------------------------------------
		// PROPRIETES
		
		/// <summary>
		/// Obtient ou définit si le message doit être affiché sur une seule ligne. Dans ce cas, AutoEllipsis vaut true. Ne fonctionne que pour le mode Label. False par défaut.
		/// </summary>
		public bool OneLineDialogMessage {
			get { return _oneLineDlgMessage; }
			set { _oneLineDlgMessage = value; _lblDlgMessage.AutoEllipsis = value; ResizeDialogMessage(); } }


		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS


		/// <summary>
		/// Constructeur.
		/// </summary>
		public MyFormMessage(bool showTitleBar=true) : base(showTitleBar)
		{
			// Initialisation des variables:
			_oneLineDlgMessage = false;
			// Initisalition du Label:
			_lblDlgMessage = new Label();
			_lblDlgMessage.Location = new Point(0, 0);
			// Initilisation du TextBox:
			_txtDlgMessage = new TextBox();
			_txtDlgMessage.Dock = DockStyle.Fill;
			_txtDlgMessage.ReadOnly = true;
			_txtDlgMessage.Multiline = true;
			_txtDlgMessage.ScrollBars = ScrollBars.Vertical;
			// Initialisation du Panel:
			_pnlDlgMessage = new Panel();
			_pnlDlgMessage.Dock = DockStyle.Fill;
			_pnlDlgMessage.SizeChanged += new EventHandler(_pnlDlgMessage_SizeChanged);
			// Initialisation du TLP:
			_tlpBody = new TableLayoutPanel();
			_tlpBody.Dock = DockStyle.Fill;
			_tlpBody.RowCount = 2;
			_tlpBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
			_tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			_tlpBody.ColumnCount = 1;
			_tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			_tlpBody.Controls.Add(_pnlDlgMessage, 0, 0);
			_tlpBase.Controls.Add(_tlpBody, 1, 0);
			// Marges du Label et du TLP:
			_lblMarginX = _lblDlgMessage.Padding.Horizontal + _lblDlgMessage.Margin.Horizontal;
			_lblMarginY = _lblDlgMessage.Padding.Vertical + _lblDlgMessage.Margin.Vertical;
			_tlpBodyMarginY = _tlpBody.Padding.Vertical + _tlpBody.Margin.Vertical;
		}


		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES


		/// <summary>
		/// Retaille les Label, TextBox et Panel pour que le message soit affiché en entier, ou bien avec des barres de défilement.
		/// </summary>
		private void ResizeDialogMessage()
		{
		
			// Sort si Minimized (sinon problème lors de la restauration):
			if (WindowState == FormWindowState.Minimized) { return; }
		
			// Annule l'événement:
			_pnlDlgMessage.SizeChanged -= _pnlDlgMessage_SizeChanged;
		
			// Si TextBox, agrandit la ligne du message pour qu'elle prenne tout le TLP:
			if (_pnlDlgMessage.Controls.Contains(_txtDlgMessage))
			{
				_tlpBody.RowStyles[0].SizeType = SizeType.Percent;
				_tlpBody.RowStyles[0].Height = 100F;
				_tlpBody.RowStyles[1].SizeType = SizeType.Percent;
				_tlpBody.RowStyles[1].Height = 0F;
				_pnlDlgMessage.Size = new Size(_tlpBody.ClientSize.Width - 4, _tlpBody.ClientSize.Height - 4);
			}
			
			// Sinon, gère la hauteur du label, en affichant éventuellement des barres de défilement au panel (AutoScroll):
			else if (_pnlDlgMessage.Controls.Contains(_lblDlgMessage))
			{
			
				// Remet la taille du TLP par défaut:
				_tlpBody.RowStyles[0].SizeType = SizeType.Absolute;
				_tlpBody.RowStyles[0].Height = 0F;
				_tlpBody.RowStyles[1].SizeType = SizeType.Percent;
				_tlpBody.RowStyles[1].Height = 100F;
				// Elimine les effets de l'Autoscroll:
				_lblDlgMessage.Location = new Point(0, 0);
				_pnlDlgMessage.AutoScrollOffset = new Point(0, 0);
				_pnlDlgMessage.AutoScrollPosition = new Point(0, 0);
				
				// Largeur par défaut du label:
				_lblDlgMessage.Width = _tlpBody.ClientSize.Width;
				// UseCompatibleTextRendering indipensable pour TextHeightFromWidth:
				_lblDlgMessage.UseCompatibleTextRendering = true;
				
				// Si seulement une ligne, indique la hauteur:
				if (_oneLineDlgMessage) {
					_lblDlgMessage.Height = Functions.TextHeightFromWidth(_lblDlgMessage, "Aj", _lblMarginX, _lblMarginY);
					_tlpBody.RowStyles[0].Height = _lblDlgMessage.Height + _tlpBodyMarginY;
					// TextHeightFromWidth incompatible avec AutoEllipsis:
					_lblDlgMessage.UseCompatibleTextRendering = false;
					_pnlDlgMessage.SizeChanged += _pnlDlgMessage_SizeChanged;
					return; }
				
				// Obtient la hauteur du label:
				int height = Functions.TextHeightFromWidth(_lblDlgMessage, _lblMarginX, _lblMarginY);
				
				// Si hauteur du texte plus grand que le TLP, barre de défilement:
				if (height >= _tlpBody.ClientSize.Height) {
					_pnlDlgMessage.AutoScroll = true;
					_tlpBody.RowStyles[0].SizeType = SizeType.Percent;
					_tlpBody.RowStyles[0].Height = 100F;
					_tlpBody.RowStyles[1].SizeType = SizeType.Percent;
					_tlpBody.RowStyles[1].Height = 0F;
					_pnlDlgMessage.Size = new Size(_tlpBody.ClientSize.Width - 4, _tlpBody.ClientSize.Height - 4);
					_lblDlgMessage.Width = _tlpBody.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 10;
					_lblDlgMessage.Height = Functions.TextHeightFromWidth(_lblDlgMessage, _lblMarginX, _lblMarginY); }
					
				// Sinon, ajuste:
				else {
					_pnlDlgMessage.AutoScroll = false;
					_tlpBody.RowStyles[0].SizeType = SizeType.Absolute;
					_tlpBody.RowStyles[0].Height = height + _tlpBodyMarginY;
					_tlpBody.RowStyles[1].SizeType = SizeType.Percent;
					_tlpBody.RowStyles[1].Height = 100F;
					_pnlDlgMessage.Size = new Size(_tlpBody.Width - 4, height + _tlpBodyMarginY - 4);
					_lblDlgMessage.Height = height; }
			}
			
			// Réinscrit à l'événement:
			_pnlDlgMessage.SizeChanged += _pnlDlgMessage_SizeChanged;
			
		}


		/// <summary>
		/// Définit ou modifie le message affiché. Si null ou vide, la ligne du message est réduite à 0.
		/// </summary>
		public virtual void SetDialogMessage(string msg, bool useTextBox)
		{
			// Enlève les échappements sur \n:
			msg = msg.Replace("\\*", "\n");
			// Réduit à 0 si pas de message, et sort:
			if (String.IsNullOrEmpty(msg)) { _tlpBody.RowStyles[0].Height = 0; return; }
			// Si TextBox, on l'insère dans le Panel:
			if (useTextBox) {
				if (_pnlDlgMessage.Controls.Contains(_lblDlgMessage)) { _pnlDlgMessage.Controls.Remove(_lblDlgMessage); }
				if (!_pnlDlgMessage.Controls.Contains(_txtDlgMessage)) { _pnlDlgMessage.Controls.Add(_txtDlgMessage);  }
				_txtDlgMessage.Text = msg; }
			else {
				if (_pnlDlgMessage.Controls.Contains(_txtDlgMessage)) { _pnlDlgMessage.Controls.Remove(_txtDlgMessage); }
				if (!_pnlDlgMessage.Controls.Contains(_lblDlgMessage)) { _pnlDlgMessage.Controls.Add(_lblDlgMessage); }
				_lblDlgMessage.Text = msg; }
			// Hauteur automatique:
			ResizeDialogMessage();
		}
		
		
		/// <summary>
		/// Définit ou modifie le message affiché. Si null ou vide, la ligne du message est réduite à 0.
		/// </summary>
		public virtual void SetDialogMessage(string msg)
			{ SetDialogMessage(msg, false); }
		
		
		/// <summary>
		/// Insère un contrôle sous le texte, dans la deuxième ligne de _tlpBody. Si ctrl est null, retire les contrôles déjà présent.
		/// </summary>
		public virtual void SetControl(Control ctrl)
		{
			// Retire les contrôles:
			Control prev = _tlpBody.GetControlFromPosition(0, 1);
			if (prev != null) { _tlpBody.Controls.Remove(prev); }
			// Ajoute le nouveau contrôle:
			if (ctrl != null) { ctrl.Dock = DockStyle.Fill; _tlpBody.Controls.Add(ctrl, 0, 1); }
		}
		
		
		/// <summary>
		/// Récupère le contrôle inséré avec SetControl, s'il existe.
		/// </summary>
		public virtual Control GetControl()
			{ return _tlpBody.GetControlFromPosition(0, 1); }
		
		
		/// <summary>
		/// Affiche le form.
		/// </summary>
		public virtual new DialogBoxClickResult ShowDialog()
		{
			if (ButtonsCollections.Length == 0) { AddButtonsCollection(DialogBoxButtons.OK, 0, true); }
			return base.ShowDialog();;
		}


		/// <summary>
		/// Affiche le form.
		/// </summary>
		public virtual new DialogBoxClickResult ShowDialog(IWin32Window owner)
		{
			if (ButtonsCollections.Length == 0) { AddButtonsCollection(DialogBoxButtons.OK, 0, true); }
			return base.ShowDialog(owner);
		}


		/// <summary>
		/// Appelle simplement ResizeDialogMessage.
		/// </summary>
		private void _pnlDlgMessage_SizeChanged(object sender, EventArgs e)
			{ ResizeDialogMessage(); }


	}



	// ===========================================================================


	/// <summary>
	/// Hérite de MyFormMessage. Ajoute à _tlpBody, dans la deuxième ligne, un TLP privé qui contient des lignes que l'on peut ajouter, chaque ligne étant composé d'un Label et/ou d'un contrôle. Si aucun bouton n'est inséré, alors par défaut les boutons OKCancel sont affichés.
	/// </summary>
	public class MyFormMultilines : MyFormMessage
	{

		// ---------------------------------------------------------------------------
		// DECLARATIONS


		protected TableLayoutPanel _tlpMulti;
		protected int _percentWidth;


		// ---------------------------------------------------------------------------
		// PROPRIETES

		
		/// <summary>
		/// Modifie la largeur de la première colonne, en pourcentage.
		/// </summary>
		public int PercentWidth {
			get { return _percentWidth; }
			set { _tlpMulti.ColumnStyles[0].Width = _percentWidth; _tlpMulti.ColumnStyles[1].Width = 100 - _percentWidth; } }


		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS


		/// <summary>
		/// Constructeur.
		/// </summary>
		public MyFormMultilines()
		{
			// Initialisation des variables:
			_percentWidth = 30;
			// Initialisation du TLP:
			_tlpMulti = new TableLayoutPanel();
			_tlpMulti.ColumnCount = 2;
			_tlpMulti.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, _percentWidth));
			_tlpMulti.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 - _percentWidth));
			_tlpMulti.RowCount = 1;
			_tlpMulti.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			SetControl(_tlpMulti);
		}


		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES


		/// <summary>
		/// Ajoute une ligne, avec Label et contrôle.
		/// </summary>
		public void AddLine(string label, Control ctrl, float height)
		{
			int l = _tlpMulti.RowCount - 1;
			_tlpMulti.RowCount++;
			_tlpMulti.RowStyles.Insert(l, new RowStyle(SizeType.Absolute, height));
			Label lbl = new Label();
			lbl.Dock = DockStyle.Fill;
			lbl.Text = label;
			_tlpMulti.Controls.Add(lbl, 0, l);
			ctrl.Dock = DockStyle.Fill;
			_tlpMulti.Controls.Add(ctrl, 1, l);
		}

		/// <summary>
		/// Ajoute une ligne, avec seulement un Label.
		/// </summary>
		public void AddLine(string label, float height)
		{
			int l = _tlpMulti.RowCount - 1;
			_tlpMulti.RowCount++;
			_tlpMulti.RowStyles.Insert(l, new RowStyle(SizeType.Absolute, height));
			Label lbl = new Label();
			lbl.Dock = DockStyle.Fill;
			lbl.Text = label;
			_tlpMulti.SetColumnSpan(lbl, 2);
			_tlpMulti.Controls.Add(lbl, 0, l);
		}

		/// <summary>
		/// Ajoute une ligne, avec seulement un contrôle.
		/// </summary>
		public void AddLine(Control ctrl, float height)
		{
			int l = _tlpMulti.RowCount - 1;
			_tlpMulti.RowCount++;
			_tlpMulti.RowStyles.Insert(l, new RowStyle(SizeType.Absolute, height));
			ctrl.Dock = DockStyle.Fill;
			_tlpMulti.SetColumnSpan(ctrl, 2);
			_tlpMulti.Controls.Add(ctrl, 0, l);
		}

		/// <summary>
		/// Ajoute une ligne, avec Label et contrôle.
		/// </summary>
		public void AddLine(string label, Control ctrl)
			{ AddLine(label, ctrl, 25); }

		/// <summary>
		/// Ajoute une ligne, avec seulement un Label.
		/// </summary>
		public void AddLine(string label)
			{ AddLine(label, 25); }
		
		/// <summary>
		/// Ajoute une ligne, avec seulement un contrôle.
		/// </summary>
		public void AddLine(Control ctrl)
			{ AddLine(ctrl, 25); }

		/// <summary>
		/// Affiche le form.
		/// </summary>
		public virtual new DialogBoxClickResult ShowDialog()
			{ return ShowDialog(null); }

		/// <summary>
		/// Affiche le form.
		/// </summary>
		public virtual new DialogBoxClickResult ShowDialog(IWin32Window owner)
		{
		
			// Ajoute des boutons, s'il n'y en a pas:
			if (ButtonsCollections.Length == 0) { AddButtonsCollection(DialogBoxButtons.OKCancel, 1, true); }
			
			// Vérifie la hauteur de _tlpMulti, et s'il est plus haut que la cellule le contenant, alors
			// augmente la hauteur de la fenêtre. Sinon, la réduit à la hauteur par défaut:
			if (Height != MyForm.FormDefaultSize.Height) { Height = MyForm.FormDefaultSize.Height; }
			// Hauteur réel du TLP:
			int height = 0, l = _tlpMulti.RowStyles.Count - 1;
			for (int i=0; i<l; i++) { height += (int)_tlpMulti.RowStyles[i].Height; }
			// Si hauteur réel est plus grande que hauteur dans _tlpBody, alors on redimensionne le form:
			int diff = height - _tlpMulti.Height;
			if (diff > 0) { Height = MyForm.FormDefaultSize.Height + diff; }
			
			// Affiche le form:
			if (owner == null) { return base.ShowDialog(); }
			else { return base.ShowDialog(owner); }
			
		}

	}



	// ===========================================================================



	/// <summary>
	/// Fournit des méthodes pour afficher cinq types de DialogBox courrants: Message, Question (bouton YesNo), Erreur, Input (avec un contrôle texte) et InputControl (avec n'importe quel contrôle).
	/// </summary>
	public static class DialogBoxes
	{

		// ---------------------------------------------------------------------------
		// DECLARATIONS
		
		private static MyFormMessage _dlgMsg, _dlgQuestion, _dlgQuestionYNC, _dlgInput, _dlgInputCtrl, _dlgError;
		private static TextBox _txtInput;
		private static string _savedTitle, _savedSubtitle;
		private static DialogBoxClickResult _buttonResult;
		
		
		// ---------------------------------------------------------------------------
		// PROPRIETES:
		
		/// <summary>
		/// Obtient ou définit le texte entré par l'utilisateur lors de l'affichage d'un dialogue Input.
		/// </summary>
		public static string InputText {
			get { return _txtInput.Text; }
			set { _txtInput.Text = value; } }
		
		/// <summary>
		/// Obtient le boutton sur lequel l'utilisateur a cliqué en dernier.
		/// </summary>
		public static DialogBoxClickResult ButtonResult { get { return _buttonResult; }  }
		
		/// <summary>
		/// Obtient le dialogue message.
		/// </summary>
		public static MyFormButtons DialogMessage { get { return _dlgMsg; } }
		
		/// <summary>
		/// Obtient le dialogue question.
		/// </summary>
		public static MyFormButtons DialogQuestion { get { return _dlgQuestion; } }
		
		/// <summary>
		/// Obtient le dialogue question YesNoCancel.
		/// </summary>
		public static MyFormButtons DialogQuestionYNC { get { return _dlgQuestionYNC; } }
		
		/// <summary>
		/// Obtient le dialogue input.
		/// </summary>
		public static MyFormButtons DialogInput { get { return _dlgInput; } }
		
		/// <summary>
		/// Obtient le dialogue erreur.
		/// </summary>
		public static MyFormButtons DialogError { get { return _dlgError; } }
		
		/// <summary>
		/// Obtient le dialogue input control.
		/// </summary>
		public static MyFormButtons DialogInputControl { get { return _dlgInputCtrl; } }
		

		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		
		
		/// <summary>
		/// Constructeurs statique.
		/// </summary>
		static DialogBoxes()
		{
			// Initilisation du dialogue message:
			_dlgMsg = new MyFormMessage();
			_dlgMsg.AddButtonsCollection(DialogBoxButtons.OK, 0, true);
			_dlgMsg.FormBorderStyle = FormBorderStyle.FixedSingle;
			// Initialisation du dialogue erreur:
			_dlgError = new MyFormMessage();
			_dlgError.AddButtonsCollection(DialogBoxButtons.OK, 0, true);
			_dlgError.FormBorderStyle = FormBorderStyle.FixedSingle;
			// Initilisation du dialogue question:
			_dlgQuestion = new MyFormMessage();
			_dlgQuestion.AddButtonsCollection(DialogBoxButtons.YesNo, 1, true);
			_dlgQuestion.AddButtonsCollection(DialogBoxButtons.OKCancel, 1, false);
			_dlgQuestion.FormBorderStyle = FormBorderStyle.FixedSingle;
			// Initilisation du dialogue questionYNC:
			_dlgQuestionYNC = new MyFormMessage();
			_dlgQuestionYNC.AddButtonsCollection(DialogBoxButtons.YesNoCancel, 2, true);
			_dlgQuestionYNC.FormBorderStyle = FormBorderStyle.FixedSingle;
			// Initilisation du dialogue input:
			_txtInput = new TextBox();
			_txtInput.Dock = DockStyle.Fill;
			_txtInput.MaxLength = Int32.MaxValue;
			_txtInput.ScrollBars = ScrollBars.Vertical;
			_txtInput.TextChanged += delegate
				{
					_txtInput.Text = _txtInput.Text.Replace("\r\n", "\n").Replace("\n", "\r\n");
				};
			_txtInput.MultilineChanged += delegate
				{
					_txtInput.AcceptsReturn = _txtInput.Multiline;
				};
			_dlgInput = new MyFormMessage();
			_dlgInput.SetControl(_txtInput);
			_dlgInput.AddButtonsCollection(DialogBoxButtons.OKCancel, 1, true);
			_dlgInput.FormBorderStyle = FormBorderStyle.FixedSingle;
			// Initialisation du dialogue input control:
			_dlgInputCtrl = new MyFormMessage();
			_dlgInputCtrl.AddButtonsCollection(DialogBoxButtons.OK, 0, true);
			_dlgInputCtrl.AddButtonsCollection(DialogBoxButtons.OKCancel, 1, false);
			_dlgInputCtrl.AddButtonsCollection(DialogBoxButtons.YesNo, 1, false);
			_dlgInputCtrl.AddButtonsCollection(DialogBoxButtons.Cancel, 0, false);
			_dlgInputCtrl.FormBorderStyle = FormBorderStyle.FixedSingle;
			// Sauvegarde des titres par défaut:
			_savedTitle = _dlgMsg.TitleBox;
			_savedSubtitle = _dlgMsg.SubtitleBox;
			// Par défaut:
			_buttonResult = DialogBoxClickResult.None;
		}

		
		// ---------------------------------------------------------------------------
		// METHODES
		
		private static DialogBoxClickResult ShowDlg(MyFormMessage dlg, string msg, DialogBoxIcon icon, bool useTextBox, string subtitle)
		{
			dlg.SubtitleBox = (subtitle == null ? _savedSubtitle : subtitle);
			dlg.SetDialogMessage(msg, useTextBox);
			dlg.SetDialogIcon(icon);
			return (_buttonResult = dlg.ShowDialog());
		}

		/// <summary>
		/// Affiche une boîte de dialogue de type message, avec un bouton OK. Si subtitle est null, le titre par défaut est remis. Si vide, il est laissé en blanc.
		/// </summary>
		public static void ShowDialogMessage(string msg, DialogBoxIcon icon, bool useTextBox, string subtitle)
			{ ShowDlg(_dlgMsg, msg, icon, useTextBox, subtitle); }
		
		/// <summary>
		/// Affiche une boîte de dialogue de type message, avec un bouton OK, avec un sous-titre par défaut.
		/// </summary>
		public static void ShowDialogMessage(string msg, DialogBoxIcon icon, bool useTextBox)
			{ ShowDialogMessage(msg, icon, useTextBox, null); }
		
		/// <summary>
		/// Affiche une boîte de dialogue de type message, avec un bouton OK, avec un sous-titre par défaut, en mode Label.
		/// </summary>
		public static void ShowDialogMessage(string msg, DialogBoxIcon icon)
			{ ShowDialogMessage(msg, icon, false); }
		
		/// <summary>
		/// Affiche une boîte de dialogue de type message, avec un bouton OK, avec un sous-titre par défaut, en mode Label et avec une icône "information".
		/// </summary>
		public static void ShowDialogMessage(string msg)
			{ ShowDialogMessage(msg, DialogBoxIcon.Information); }

		// ---------------------------------------------------------------------------

		/// <summary>
		/// Affiche une boîte de dialogue de type erreur, avec un bouton OK et une icône style "erreur". Si subtitle est null, le titre par défaut est remis. Si vide, il est laissé en blanc.
		/// </summary>
		public static DialogBoxClickResult ShowDialogError(string msg, bool useTextBox, string subtitle)
			{ return ShowDlg(_dlgError, msg, DialogBoxIcon.Error, useTextBox, subtitle); }
	
		/// <summary>
		/// Affiche une boîte de dialogue de type erreur, avec un bouton OK et une icône style "erreur", et un sous-titre par défaut.
		/// </summary>
		public static DialogBoxClickResult ShowDialogError(string msg, bool useTextBox)
			{ return ShowDialogError(msg, useTextBox, null); }
		
		/// <summary>
		/// Affiche une boîte de dialogue de type erreur, avec un bouton OK et une icône style "erreur", et un sous-titre par défaut, en mode label.
		/// </summary>
		public static DialogBoxClickResult ShowDialogError(string msg)
			{ return ShowDialogError(msg, false); }

		// ---------------------------------------------------------------------------
	
		/// <summary>
		/// Affiche une boîte de dialogue de type question, avec un bouton YesNo ou OKCancel. Si subtitle est null, le titre par défaut est remis. Si vide, il est laissé en blanc.
		/// </summary>
		public static DialogBoxClickResult ShowDialogQuestion(string msg, DialogBoxIcon icon, int defButton, bool useOKCancel, string subtitle)
		{
			_dlgQuestion.ShowButtonsCollection(useOKCancel ? 1 : 0);
			_dlgQuestion.GetButtonsCollection(useOKCancel ? 1 : 0).GetButton(defButton).Select();
			return ShowDlg(_dlgQuestion, msg, icon, false, subtitle);
		}
		
		/// <summary>
		/// Affiche une boîte de dialogue de type question, avec un bouton YesNo ou OKCancel, avec un sous-titre par défaut.
		/// </summary>
		public static DialogBoxClickResult ShowDialogQuestion(string msg, DialogBoxIcon icon, int defButton, bool useOKCancel)
			{ return ShowDialogQuestion(msg, icon, defButton, useOKCancel, null); }
		
		/// <summary>
		/// Affiche une boîte de dialogue de type question, avec un bouton YesNo, avec un sous-titre par défaut.
		/// </summary>
		public static DialogBoxClickResult ShowDialogQuestion(string msg, DialogBoxIcon icon, int defButton)
			{ return ShowDialogQuestion(msg, icon, defButton, false); }
		
		/// <summary>
		/// Affiche une boîte de dialogue de type question, avec un bouton YesNo, avec un sous-titre par défaut, avec le bouton Yes sélectionné.
		/// </summary>
		public static DialogBoxClickResult ShowDialogQuestion(string msg, DialogBoxIcon icon)
			{ return ShowDialogQuestion(msg, icon, 1); }
		
		/// <summary>
		/// Affiche une boîte de dialogue de type question, avec un bouton YesNo, avec un sous-titre par défaut, , avec le bouton Yes sélectionné et avec une icône "information".
		/// </summary>
		public static DialogBoxClickResult ShowDialogQuestion(string msg)
			{ return ShowDialogQuestion(msg, DialogBoxIcon.Question); }

		// ---------------------------------------------------------------------------

		/// <summary>
		/// Même chose que ShowDialogQuestion, mais avec des boutons Yes, No et Cancel.
		/// </summary>
		public static DialogBoxClickResult ShowDialogQuestionYNC(string msg, DialogBoxIcon icon, int defButton, string subtitle)
		{ return ShowDlg(_dlgQuestionYNC, msg, icon, false, subtitle); }
		
		/// <summary>
		/// Même chose que ShowDialogQuestion, mais avec des boutons Yes, No et Cancel.
		/// </summary>
		public static DialogBoxClickResult ShowDialogQuestionYNC(string msg, DialogBoxIcon icon, int defButton)
			{ return ShowDialogQuestionYNC(msg, icon, defButton, null); }
		
		/// <summary>
		/// Même chose que ShowDialogQuestion, mais avec des boutons Yes, No et Cancel.
		/// </summary>
		public static DialogBoxClickResult ShowDialogQuestionYNC(string msg, DialogBoxIcon icon)
			{ return ShowDialogQuestionYNC(msg, icon, 1); }
		
		/// <summary>
		/// Même chose que ShowDialogQuestion, mais avec des boutons Yes, No et Cancel.
		/// </summary>
		public static DialogBoxClickResult ShowDialogQuestionYNC(string msg)
			{ return ShowDialogQuestionYNC(msg, DialogBoxIcon.Question); }

		// ---------------------------------------------------------------------------
	
		/// <summary>
		/// Affiche une boîte de dialogue de type input, avec un bouton OKCancel. Si subtitle est null, le titre par défaut est remis. Si vide, il est laissé en blanc.
		/// </summary>
		public static DialogBoxClickResult ShowDialogInput(string msg, object def, bool isMultiline, DialogBoxIcon icon, string subtitle)
		{
			if (def == null) { def = String.Empty; }
			_txtInput.Text = def.ToString();
			_txtInput.Multiline = isMultiline;
			_txtInput.Select();
			return ShowDlg(_dlgInput, msg, icon, false, subtitle);
		}
		
		/// <summary>
		/// Affiche une boîte de dialogue de type input, avec un bouton OKCancel, avec un sous-titre par défaut.
		/// </summary>
		public static DialogBoxClickResult ShowDialogInput(string msg, object def, bool isMultiline, DialogBoxIcon icon)
			{ return ShowDialogInput(msg, def, isMultiline, icon, null); }
		
		/// <summary>
		/// Affiche une boîte de dialogue de type input, avec un bouton OKCancel, avec un sous-titre par défaut, une icône en forme d'interrogation.
		/// </summary>
		public static DialogBoxClickResult ShowDialogInput(string msg, object def, bool isMultiline)
			{ return ShowDialogInput(msg, def, isMultiline, DialogBoxIcon.Question); }
		
		/// <summary>
		/// Affiche une boîte de dialogue de type input, avec un bouton OKCancel, avec un sous-titre par défaut, une icône en forme d'interrogation, sans multiligne.
		/// </summary>
		public static DialogBoxClickResult ShowDialogInput(string msg, object def)
			{ return ShowDialogInput(msg, def, false); }
		
		/// <summary>
		/// Affiche une boîte de dialogue de type input, avec un bouton OKCancel, avec un sous-titre par défaut, une icône en forme d'interrogation, sans multiligne, sans texte par défaut.
		/// </summary>
		public static DialogBoxClickResult ShowDialogInput(string msg)
			{ return ShowDialogInput(msg, String.Empty); }

		// ---------------------------------------------------------------------------
	
		/// <summary>
		/// Affiche une boîte de dialogue de type input control. Si subtitle est null, le titre par défaut est remis. Si vide, il est laissé en blanc.
		/// </summary>
		public static DialogBoxClickResult ShowDialogInputCtrl(string msg, Control control, DialogBoxButtons buttons, DialogBoxIcon icon,
			int defButton, string subtitle)
		{
			int coll = 0;
			switch (buttons) {
				case DialogBoxButtons.OK: coll = 0; break;
				case DialogBoxButtons.OKCancel: coll = 1; break;
				case DialogBoxButtons.YesNo: coll = 2; break;
				case DialogBoxButtons.Cancel: coll = 3; break; }
			_dlgInputCtrl.ShowButtonsCollection(coll);
			_dlgQuestion.GetButtonsCollection(coll).GetButton(defButton).Select();
			_dlgInputCtrl.SetControl(control);
			DialogBoxClickResult res = ShowDlg(_dlgInputCtrl, msg, icon, false, subtitle);
			_dlgInputCtrl.SetControl(null);
			return res;
		}

		/// <summary>
		/// Affiche une boîte de dialogue de type input control, avec un sous-titre par défaut.
		/// </summary>
		public static DialogBoxClickResult ShowDialogInputCtrl(string msg, Control control, DialogBoxButtons buttons, DialogBoxIcon icon,
			int defButton)
			{ return ShowDialogInputCtrl(msg, control, buttons, icon, defButton, null); }

		/// <summary>
		/// Affiche une boîte de dialogue de type input control, avec un sous-titre par défaut, et un bouton sélectionné par défaut.
		/// </summary>
		public static DialogBoxClickResult ShowDialogInputCtrl(string msg, Control control, DialogBoxButtons buttons, DialogBoxIcon icon)
			{ return ShowDialogInputCtrl(msg, control, buttons, icon, 0); }

		/// <summary>
		/// Affiche une boîte de dialogue de type input control, avec un sous-titre par défaut, et un bouton sélectionné par défaut, une icône de type interrogation.
		/// </summary>
		public static DialogBoxClickResult ShowDialogInputCtrl(string msg, Control control, DialogBoxButtons buttons)
			{ return ShowDialogInputCtrl(msg, control, buttons, DialogBoxIcon.Question); }

		/// <summary>
		/// Affiche une boîte de dialogue de type input control, avec un sous-titre par défaut, et un bouton sélectionné par défaut, une icône de type interrogation, des boutons OKCancel.
		/// </summary>
		public static DialogBoxClickResult ShowDialogInputCtrl(string msg, Control control)
			{ return ShowDialogInputCtrl(msg, control, DialogBoxButtons.OKCancel); }

	}



	// ---------------------------------------------------------------------------
	
	
	
	/// <summary>
	/// Dialogue de progression.
	/// </summary>
	public class DialogBoxProgress : MyFormMessage
	{


		// ---------------------------------------------------------------------------
		// DECLARATIONS


		/// <summary>
		/// L'événement DialogShown se déclenche lorsque le dialog apparaît à l'écran (c'est l'événément Activated du form). Il est donc possible de s'inscrire à cet événement pour effectuer les opérations qui demande une visualisation de leur progression, pendant que la boîte est affichée en mode modal (ShowDialog). L'argument sender envoie l'instance de cet objet même (this), si bien que la procédure qui traite l'événement peut parfaitement s'occuper de l'avancement de la barre de progression, de l'annulation, etc. La propriété DialogShownResult offre un moyen à la méthode de gestion de l'événement de renvoyer un résultat quelconque, résultat qui peut être repris par la procédure qui a appelé ShowDialog, et qui pendant ce temps-là attend toujours...
		/// </summary>
		public event DialogBoxProgressShownEventHandler DialogBoxProgressShown;

		/// <summary>
		/// L'événement Canceled se déclenche lorsque l'utilisateur clique sur le bouton Cancel...
		/// </summary>
		public event DialogBoxProgressCanceledEventHandler Canceled;

		// Autres variables:
		private TextBox _txtLog;
		private Label _lblAction;
		private ProgressBar _pgrBar;
		private bool _logMode, _isCanceled, _allowCancel;
		private ProgressBarStyle _pgrStyle;
		private Button _cmdSave, _cmdOK, _cmdCancel;
		private object _dialogShownResult;
		private TableLayoutPanel _tlpProgress;
		private object[] _eventParameters;
		private string _originalMessage;


		// ---------------------------------------------------------------------------
		// PROPRIETES


		/// <summary>
		/// Objet éventuellement retourné par l'événement DialogShown. Voir l'événement DialogShown.
		/// </summary>
		public object DialogShownResult { get { return _dialogShownResult; } }

		/// <summary>
		/// Obtient s'il faut utiliser le mode journal, i.e. affiche une zone de texte "cumulative" plutôt qu'un label. Par défaut: false.
		/// </summary>
		public bool LogMode
		{
			get { return _logMode; }
			set {
				if (_logMode == value) { return; }
				_tlpProgress.Controls.Remove(_logMode ? (Control)_txtLog : (Control)_lblAction);
				_tlpProgress.Controls.Add(value ? (Control)_txtLog : (Control)_lblAction);
				_logMode = value; }
		}

		/// <summary>
		/// Obtient si l'utilisateur a cliqué sur le bouton Cancel. Permet aussi de réinitialiser la valeur.
		/// </summary>
		public bool IsCanceled {
			get { return _isCanceled; }
			set { _isCanceled = value; } }

		/// <summary>
		/// Obtient ou définit si le bouton Cancel est disponible (enabled). On peut changer la valeur en cours de route.
		/// </summary>
		public bool AllowCancel {
			get { return _allowCancel; }
			set { _allowCancel = value; _cmdCancel.Enabled = value; } }

		/// <summary>
		/// Obtient ou définit le style du ProgressBar.
		/// </summary>
		public ProgressBarStyle ProgressStyle {
			get { return _pgrStyle; }
			set { _pgrStyle = value; } }
			
		/// <summary>
		/// Obtient ou définit la valeur courante du ProgressBar. Cette valeur doit être comprise entre 0 et 1 (donc en pourcentage / 100). Si la valeur est négative, alors la barre est en mode "marquee" (défilement ininterrompue de la barre).
		/// </summary>
		public float ProgressValue {
			get { return (_pgrBar.Style == ProgressBarStyle.Marquee ? -1 : (_pgrBar.Value / 100)); }
			set {
				if (value < 0) { _pgrBar.Style = ProgressBarStyle.Marquee; }
				else { _pgrBar.Style = _pgrStyle; _pgrBar.Value = ((value > 1) ? 1 : (int)(value * 100)); }
				Application.DoEvents(); } }


		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS


		/// <summary>
		/// Constructeur.
		/// </summary>
		public DialogBoxProgress()
		{
		
			// Initialisation des boutons:
			_cmdSave = new Button();
			_cmdSave.Text = MyResources.Common_cmd_Save;
			_cmdSave.Click += new EventHandler(_cmdSave_Click);
			_cmdOK = new Button();
			_cmdOK.Text = MyResources.Common_cmd_OK;
			_cmdOK.Tag = DialogBoxTagButton.AcceptCancel;
			_cmdOK.Click += delegate { ClickResult = DialogBoxClickResult.OK; this.Hide(); };
			_cmdCancel = new Button();
			_cmdCancel.Text = MyResources.Common_cmd_Cancel;
			_cmdCancel.Tag = DialogBoxTagButton.AcceptCancel;
			_cmdCancel.Click += new EventHandler(_cmdCancel_Click);
		
			// Initilisation des contrôles d'affichage:
			_txtLog = new TextBox();
			_txtLog.Dock = DockStyle.Fill;
			_txtLog.Multiline = true;
			_txtLog.ReadOnly = true;
			_txtLog.ScrollBars = ScrollBars.Vertical;
			_lblAction = new Label();
			_lblAction.Dock = DockStyle.Fill;
			_lblAction.Margin = new Padding(3, 14, 3, 0);
			
			// Initialise du PGR:
			_pgrBar = new ProgressBar();
			_pgrBar.Dock = DockStyle.Fill;

			// Initialisation du TLP:
			_tlpProgress = new TableLayoutPanel();
			_tlpProgress.Dock = DockStyle.Fill;
			_tlpProgress.RowCount = 2;
			_tlpProgress.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
			_tlpProgress.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			_tlpProgress.ColumnCount = 1;
			_tlpProgress.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			_tlpProgress.Controls.Add(_pgrBar, 0, 0);
			_tlpProgress.Controls.Add(_lblAction, 0, 1);

			// Initialisation du form:
			ControlBox = false;
			OneLineDialogMessage = true;
			AddButtonsCollection(new ButtonsCollection(0, _cmdCancel), true);
			AddButtonsCollection(DialogBoxButtons.OK, 0, false);
			AddButtonsCollection(new ButtonsCollection(0, _cmdOK, _cmdSave), false);
			SetDialogIcon(DialogBoxIcon.Waiting);
			SetDialogMessage(MyResources.DialogBoxProgress_dialog_DefaultMessage);
			_tlpBody.Controls.Add(_tlpProgress, 0, 1);

			// Valeurs par défaut:
			_logMode = false;
			_allowCancel = false;
			_cmdCancel.Enabled = false;
			_isCanceled = false;
			_pgrStyle = ProgressBarStyle.Blocks;
			_pgrBar.MarqueeAnimationSpeed = 50;
			_pgrBar.Style = _pgrStyle;
			ProgressValue = -1;
			_originalMessage = MyResources.DialogBoxProgress_dialog_DefaultMessage;
			
		}

		/// <summary>
		/// Constructeur.
		/// </summary>
		public DialogBoxProgress(string message, bool logMode) : this()
			{ SetDialogMessage(_originalMessage = message); _logMode = logMode; }

		/// <summary>
		/// Constructeur.
		/// </summary>
		public DialogBoxProgress(string message, bool logMode, bool allowCancel) : this(message, logMode)
			{ _allowCancel = allowCancel; }

		/// <summary>
		/// Constructeur.
		/// </summary>
		public DialogBoxProgress(string message, bool logMode, bool allowCancel, string subtitleBox) : this(message, logMode, allowCancel)
			{ SubtitleBox = subtitleBox; }


		// ---------------------------------------------------------------------------
		// METHODES D'AFFICHAGE

		/// <summary>
		/// Affiche le form sous forme modal et déclenche l'événement DialogBoxProgressShown après avoir remis à zéro toutes les données (Reset).
		/// </summary>
		public DialogBoxClickResult ShowDialog(IWin32Window owner, params object[] parameters)
		{
			// Remet à zéro:
			Reset();
			// S'inscrit à l'événement Activate, puisqu'on ne peut lancer l'événement DialogBoxProgressShown maintenant
			// (sinon, il s'exécute avant l'affichage), ni après l'affichage (car trop tard):
			_eventParameters = parameters;
			this.Activated += new EventHandler(DialogBoxProgress_Activated);
			// Affiche en modal:
			DialogBoxClickResult res;
			if (owner == null) { res = base.ShowDialog(); }
			else { res = base.ShowDialog(owner); }
			return res;
		}

		/// <summary>
		/// Affiche le form sous forme modal et déclenche l'événement DialogBoxProgressShown après avoir remis à zéro toutes les données (Reset).
		/// </summary>
		public override DialogBoxClickResult ShowDialog()
			{ return ShowDialog(null, new object[0]); }

		/// <summary>
		/// Affiche le form sous forme modal et déclenche l'événement DialogBoxProgressShown après avoir remis à zéro toutes les données (Reset).
		/// </summary>
		public override DialogBoxClickResult ShowDialog(IWin32Window owner)
			{ return ShowDialog(owner, new object[0]); }

		/// <summary>
		/// Affiche le form sous forme modal et déclenche l'événement DialogBoxProgressShown après avoir remis à zéro toutes les données (Reset).
		/// </summary>
		public DialogBoxClickResult ShowDialog(params object[] parameters)
			{ return ShowDialog(null, parameters); }

		/// <summary>
		/// Affiche le form sous forme non modal et ne déclenche pas l'événement DialogBoxProgressShown. Remet à zéro.
		/// </summary>
		public new void Show()
			{ Reset(); base.Show(); Application.DoEvents(); }

		/// <summary>
		/// Affiche le form sous forme non modal et ne déclenche pas l'événement DialogBoxProgressShown. Remet à zéro.
		/// </summary>
		public new void Show(IWin32Window owner)
			{ Reset(); base.Show(owner); Application.DoEvents(); }


		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES


		/// <summary>
		/// Remet à zéro toutes les données.
		/// </summary>
		public void Reset()
		{
			_isCanceled = false;
			ProgressValue = -1;
			_txtLog.Text = string.Empty;
			_lblAction.Text = string.Empty;
			ShowButtonsCollection(0);
			SetDialogMessage(_originalMessage);
		}

		/// <summary>
		/// Ajoute une description de l'action. Si le mode journal est activé, ajoute une ligne au journal. Sinon, met à jour la valeur du label.
		/// </summary>
		public void AddActionDescription(string text)
		{
			if (LogMode) {
				_txtLog.SelectionStart = _txtLog.TextLength;
				_txtLog.SelectedText = (_txtLog.TextLength > 0 ? "\r\n" : string.Empty) + text;
				_txtLog.SelectionStart = _txtLog.TextLength; }
			else {
				_lblAction.Text = text; }
			Application.DoEvents();
		}

		/// <summary>
		/// Voir surcharge. (N'est utile qu'en mode journal !)
		/// </summary>
		public void AddActionDescription(string[] lines)
		{
			if (!LogMode) { return; }
			for (int i = 0; i < lines.Length; i++) { AddActionDescription(lines[i]); }
		}

		/// <summary>
		/// Efface le contenu du journal.
		/// </summary>
		public void ClearLog()
			{ _txtLog.Text = string.Empty; }

		/// <summary>
		/// Cette fonction permet de définir la valeur actuelle en une seule fois, l'action en cours (si le mode journal est activé, alors le journal est remplie, sinon c'est le label qui contient la chaîne passée), et de définir si l'utilisateur a la possibilité d'annuler. Retourne si l'utilisateur a cliqué sur Annuler (T/F).
		/// </summary>
		public bool MoveForward(string action, bool allowCancel)
		{
			// Définit une action et examine le mode cancel.
			AddActionDescription(action);
			AllowCancel = allowCancel;
			return IsCanceled;
		}

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public bool MoveForward(float value, string action, bool allowCancel)
		{
			ProgressValue = value;
			return MoveForward(action, allowCancel);
		}

		/// <summary>
		/// Fin de la progression : Le bouton Cancel est remplacé par un OK qui ferme la fenêtre, et si le mode journal est activé, un bouton Save permet de l'enregistrer dans un fichier.
		/// </summary>
		public void EndingProgress()
		{
			// Change le texte du message:
			SetDialogMessage(MyResources.DialogBoxProgress_dialog_DefaultMessageWhenFinished);
			if (ProgressValue < 0) { ProgressValue = 0; }
			// Si mode journal, boutons sauver et OK:
			if (_logMode) { ShowButtonsCollection(2); }
			// Si pas mode journal, juste bouton OK:
			else { ShowButtonsCollection(1); }
			Application.DoEvents();
		}


		// ---------------------------------------------------------------------------
		// EVENEMENTS


		/// <summary>
		/// Annulation: Confirmation, puis mise à jour de _isCanceled, et déclenche l'événement Canceled.
		/// </summary>
		private void _cmdCancel_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show(MyResources.DialogBoxProgress_dialog_ConfirmCancel, My.App.Title,
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
				_isCanceled = true;
				_clickResult = DialogBoxClickResult.Cancel;
				DialogBoxProgressCanceledEventArgs eventArg = new DialogBoxProgressCanceledEventArgs(this, _dialogShownResult);
				if (Canceled != null) { Canceled(this, eventArg); }
				_dialogShownResult = eventArg.Result; }
		}

		/// <summary>
		/// Bouton Save à la fin du mode journal : permet à l'utilisateur de sauver dans un fichier.
		/// </summary>
		private void _cmdSave_Click(object sender, EventArgs e)
		{
			// Demande nom du fichier et enregistre:
			string fileName = My.FilesAndStreams.MySaveFileDialog("log|*.log");
			if (fileName != null) { My.FilesAndStreams.WriteAllLines(fileName, new string[] { _txtLog.Text }); }
		}

		/// <summary>
		/// Lance l'événement DialogBoxProgressShown, et se désinscrit de Activated.
		/// </summary>
		private void DialogBoxProgress_Activated(object sender, EventArgs e)
		{
			// Se désinscrit:
			this.Activated -= DialogBoxProgress_Activated;
			// Lance l'événement:
			DialogBoxProgressShownEventArgs evargs = new DialogBoxProgressShownEventArgs(this, _eventParameters);
			evargs.Result = _dialogShownResult;
			if (DialogBoxProgressShown != null) { DialogBoxProgressShown(this, evargs); }
			// Récupère le résultat:
			_dialogShownResult = evargs.Result;
		}
		
	}



}
