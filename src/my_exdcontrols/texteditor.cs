using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using My.ExdControls;

namespace My
{


	/// <summary>
	/// Affiche sur un Panel (dont la classe hérite) un éditeur RTF, avec les options nécessaires à la mise en forme du texte, et la possibilité d'ouvrir ou d'enregistrer un fichier. Note : Si on change la propriété ReadOnly du contrôle RichTextBox (accessible par la propriété RichTextBo), selon revient à appeler la propriété ReadOnly de cette classe, et vice-versa.
	/// </summary>
	public class TextEditor : Panel
	{




		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS
		
		
		protected ToolStrip _tools;
		protected RichTextBox _rtf;
		protected bool _readOnly;



		#endregion DECLARATIONS












		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES
		
		
		
		/// <summary>
		/// Texte simple de la zone de texte. En fait, la méthode détermine automatiquement s'il s'agit de texte RTF ou de texte simple, et définit la propriété du RichTextBox en conséquence.
		/// </summary>
		public override string Text
		{
			get { return _rtf.Text; }
			set
			{
				if (this.IsRTF(value)) { _rtf.Rtf = value; }
				else { _rtf.Text = value; }
				this.ToolsStateFromSelectionFormat();
			}
		}






		// ---------------------------------------------------------------------------
	
		
		
		
		
		/// <summary>
		/// Texte de la zone de texte, y compris les caratères RTF. En fait, la méthode détermine automatiquement s'il s'agit de texte RTF ou de texte simple, et définit la propriété du RichTextBox en conséquence.
		/// </summary>
		public string Rtf
		{
			get { return _rtf.Rtf; }
			set
			{
				if (this.IsRTF(value)) { _rtf.Rtf = value; }
				else { _rtf.Text = value; }
				this.ToolsStateFromSelectionFormat();
			}
		}





		// ---------------------------------------------------------------------------
	
		
		
		
		
		/// <summary>
		/// Couleur de font de la zone de texte.
		/// </summary>
		public Color TextBoxBackColor
		{
			get { return _rtf.BackColor; }
			set { _rtf.BackColor = value; }
		}





		// ---------------------------------------------------------------------------
		
		
		
		
		/// <summary>
		/// Obtient ou définit si l'utilisateur peut modifier le texte du contrôle, y compris le formatage. Voir commentaire de la classe.
		/// </summary>
		public bool ReadOnly
		{
			get
			{
				return _readOnly;
			}
			set
			{
				_readOnly = value;
				foreach (ToolStripItem i in _tools.Items) { i.Enabled = !value; }
				_tools.Items["save"].Enabled = true;
				_rtf.ReadOnly = value;
			}
		}





		// ---------------------------------------------------------------------------
		
		
		
		
		/// <summary>
		/// Obtient ou définit le RichTextBox affiché.
		/// </summary>
		public RichTextBox RichTextBox { get { return _rtf; } set { _rtf = value; } }
	




		#endregion PROPRIETES








		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS




		/// <summary>
		/// Initialisation des contrôles, etc.
		/// </summary>
		public TextEditor()
		{
		
			// Création des contrôles:
			
			this.Padding = new Padding(2);
			
			_tools = new ToolStrip();
			ToolStripComboBox cbo; ToolStripButton cmd;
			
			
			// Boutons:
			
			cmd = new ToolStripButton(MyResources.TextEditor_img_Open_png);
			cmd.Name = "open";
			_tools.Items.Add(cmd);
			
			cmd = new ToolStripButton(MyResources.TextEditor_img_Save_png);
			cmd.Name = "save";
			_tools.Items.Add(cmd);


			// Polices:
			cbo = new ToolStripComboBox();
			cbo.Name = "font";
			cbo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			cbo.AutoCompleteSource = AutoCompleteSource.ListItems;
			cbo.AutoSize = false;
			cbo.Width = 150;
			foreach (FontFamily f in FontFamily.Families) { cbo.Items.Add(f.Name); }
			cbo.Text = MySettings.TextEditorDefaultFont.Name;
			_tools.Items.Add(cbo);
			
			
			// Taille:
			cbo = new ToolStripComboBox();
			cbo.Name = "fontSize";
			cbo.AutoSize = false;
			cbo.Width = 45;
			for (int i=8; i<36; i+=2) { cbo.Items.Add(i.ToString()); }
			cbo.Text = MySettings.TextEditorDefaultFont.Size.ToString();
			_tools.Items.Add(cbo);
			
			
			// Couleur:
			cbo = new ToolStripComboBox();
			cbo.Name = "fontColor";
			cbo.AutoSize = false;
			cbo.Width = 100;
			PropertyInfo[] properties = (typeof(Color)).GetProperties(BindingFlags.Static | BindingFlags.Public);
			for (int i=0; i<properties.Length; i++) { cbo.Items.Add(properties[i].Name); }
			cbo.Text = "Black";
			_tools.Items.Add(cbo);
			
			
			// Buttons:
			
			cmd = new ToolStripButton(MyResources.TextEditor_img_Bold_png);
			cmd.Name = "bold";
			_tools.Items.Add(cmd);
			
			cmd = new ToolStripButton(MyResources.TextEditor_img_Italic_png);
			cmd.Name = "italic";
			_tools.Items.Add(cmd);
			
			cmd = new ToolStripButton(MyResources.TextEditor_img_Underline_png);
			cmd.Name = "underline";
			_tools.Items.Add(cmd);
			
			
			cmd = new ToolStripButton(MyResources.TextEditor_img_Left_png);
			cmd.Name = "left";
			_tools.Items.Add(cmd);
			
			cmd = new ToolStripButton(MyResources.TextEditor_img_Center_png);
			cmd.Name = "center";
			_tools.Items.Add(cmd);
			
			cmd = new ToolStripButton(MyResources.TextEditor_img_Right_png);
			cmd.Name = "right";
			_tools.Items.Add(cmd);
						

			// RichTextBox:
			_rtf = new RichTextBox();
			_rtf.Dock = DockStyle.Fill;
			_rtf.ScrollBars = RichTextBoxScrollBars.Vertical;
			_rtf.HideSelection = false;
			_rtf.AcceptsTab = true;
			_rtf.ShortcutsEnabled = true;
			_rtf.ShowSelectionMargin = true;
			_rtf.BorderStyle = BorderStyle.None;
			_rtf.Margin = new Padding(0);


			// TLP:
			
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.Dock = DockStyle.Fill;
			tlp.RowCount = 2;
			tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, _tools.Height));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			tlp.ColumnCount = 1;
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			
			tlp.Controls.Add(_tools, 0, 0);
			tlp.Controls.Add(_rtf, 0, 1);
			
			this.Controls.Add(tlp);
			
			
			// Format du texte par défaut:
			
			this.ChangeSelectionFormat(true);
			this.ToolsStateFromSelectionFormat();
		
		
			// Evénements:
			
			// Tools controls:
			foreach (ToolStripItem i in _tools.Items)
			{
				if (i is ToolStripComboBox)
				{
					((ToolStripComboBox)i).KeyDown += new KeyEventHandler(ToolCombo_KeyDown);
					((ToolStripComboBox)i).LostFocus += new EventHandler(ToolCombo_LostFocus);
					((ToolStripComboBox)i).SelectedIndexChanged += new EventHandler(ToolCombo_SelectedIndexChanged);
				}
				else if (i is ToolStripButton)
					{ i.Click += new EventHandler(ToolButton_Click); }
			}
			((ToolStripComboBox)_tools.Items["fontSize"]).KeyPress += new KeyPressEventHandler(ToolComboFontSize_KeyPress);
			
			// RTF:
			_rtf.SelectionChanged += new EventHandler(_rtf_SelectionChanged);
			_rtf.ReadOnlyChanged += delegate { this.ReadOnly = _rtf.ReadOnly; };
			this.Paint += new PaintEventHandler(TextEditor_Paint);
			// Bizarrement, quand la visibilité du panel change, le RTF se réduit... Mais le réinséré dans le contrôle lui fait retrouver sa propriété DockStyle avec effet Fill:
			this.VisibleChanged += delegate { tlp.Controls.Remove(_rtf); tlp.Controls.Add(_rtf, 0, 1); };
			
			
			
			// Par défaut:
			_readOnly = false;
			
			
		}






		#endregion CONSTRUCTEURS










		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES



		/// <summary>
		/// Définit le texte à partir d'un tableau de bits (d'un fichier, par exemple). Encodage: UTF-8. La méthode détermine automatiquement s'il s'agit de texte RTF ou de texte simple.
		/// </summary>
		public virtual void SetTextFromBytes(byte[] data)
		{
			if (data == null) { this.Rtf = null; }
			Encoding enc = Encoding.UTF8;
			string text = enc.GetString(data);
			if (this.IsRTF(text)) { _rtf.Rtf = text; }
			else { _rtf.Text = text; }
			this.ToolsStateFromSelectionFormat();
		}




		/// <summary>
		/// Retourne un tableau de bits correspondant au texte (encodage UTF-8).Si rtf vaut true, c'est le texte rtf qui est codé, sinon c'est le texte simple. Encodage: UTF-8.
		/// </summary>
		public virtual byte[] GetBytesFromText(bool rtf)
		{
			Encoding enc = Encoding.UTF8; 
			if (rtf) { return enc.GetBytes(this.Rtf); }
			else { return enc.GetBytes(this.Text); }
		}



		#endregion METHODES PUBLIQUES
	











		// ---------------------------------------------------------------------------
		// METHODES PROTEGEES
		// ---------------------------------------------------------------------------




		#region METHODES PROTEGEES



		/// <summary>
		/// Modifie le format du texte sélectionné en fonction de l'état des contrôles de mise en forme (texte des combo, cheked des boutons).
		/// </summary>
		protected virtual void ChangeSelectionFormat()
		{
			this.ChangeSelectionFormat(false);
		}
		
		
		protected virtual void ChangeSelectionFormat(bool allRichTextBox)
		{
		
			// Police et taille:
			FontFamily fontFamily = new FontFamily(_tools.Items["font"].Text);
			float fontSize = (float)Convert.ChangeType(_tools.Items["fontSize"].Text, typeof(float));
			FontStyle fontStyle = FontStyle.Regular;
			if (((ToolStripButton)_tools.Items["bold"]).Checked) { fontStyle = fontStyle | FontStyle.Bold; }
			if (((ToolStripButton)_tools.Items["italic"]).Checked) { fontStyle = fontStyle | FontStyle.Italic; }
			if (((ToolStripButton)_tools.Items["underline"]).Checked) { fontStyle = fontStyle | FontStyle.Underline; }
			Font font = new Font(fontFamily, fontSize, fontStyle);
			if (allRichTextBox) { _rtf.Font = font; }
			_rtf.SelectionFont = font;
			
			// Couleur:
			Color fontColor = Color.FromName(_tools.Items["fontColor"].Text);
			_rtf.SelectionColor = fontColor;
			
			// Alignement du texte:
			HorizontalAlignment textAlign = HorizontalAlignment.Left;
			if (((ToolStripButton)_tools.Items["center"]).Checked) { textAlign = HorizontalAlignment.Center; }
			else if (((ToolStripButton)_tools.Items["right"]).Checked) { textAlign = HorizontalAlignment.Right; }
			_rtf.SelectionAlignment = textAlign;
			
			// Sélectionne le RTF:
			_rtf.Select();
			
		}






		// ---------------------------------------------------------------------------
	



		/// <summary>
		/// Modifie l'état des contrôles d'édition (texte des combo, checked des boutons) en fonction de la sélection du RTF.
		/// </summary>
		protected virtual void ToolsStateFromSelectionFormat()
		{
		
			// Désactive les événements de sélection d'index:
			((ToolStripComboBox)_tools.Items["font"]).SelectedIndexChanged -= ToolCombo_SelectedIndexChanged;
			((ToolStripComboBox)_tools.Items["fontSize"]).SelectedIndexChanged -= ToolCombo_SelectedIndexChanged;
			((ToolStripComboBox)_tools.Items["fontColor"]).SelectedIndexChanged -= ToolCombo_SelectedIndexChanged;
			
			// (Si des caractères de différents formats sont sélectionnés, le résultat vaut null: on ne met donc pas à jour):
			
			// Police et taille:
			if (_rtf.SelectionFont != null)
			{
				Font font = _rtf.SelectionFont;
				_tools.Items["font"].Text = font.Name;
				_tools.Items["fontSize"].Text = font.Size.ToString();
				((ToolStripButton)_tools.Items["bold"]).Checked = font.Bold;
				((ToolStripButton)_tools.Items["italic"]).Checked = font.Italic;
				((ToolStripButton)_tools.Items["underline"]).Checked = font.Underline;
			}
			
			// Couleur:
			if (_rtf.SelectionColor.Name != "0")
			{
				_tools.Items["fontColor"].Text = _rtf.SelectionColor.Name;
			}
			
			// Alignement (toujours quelque chose, même si plusieurs alignements sont sélectionnés):
			((ToolStripButton)_tools.Items["left"]).Checked = (_rtf.SelectionAlignment == HorizontalAlignment.Left);
			((ToolStripButton)_tools.Items["center"]).Checked = (_rtf.SelectionAlignment == HorizontalAlignment.Center);
			((ToolStripButton)_tools.Items["right"]).Checked = (_rtf.SelectionAlignment == HorizontalAlignment.Right);
		
			// Réactive les événements de sélection d'index:
			((ToolStripComboBox)_tools.Items["font"]).SelectedIndexChanged += ToolCombo_SelectedIndexChanged;
			((ToolStripComboBox)_tools.Items["fontSize"]).SelectedIndexChanged += ToolCombo_SelectedIndexChanged;
			((ToolStripComboBox)_tools.Items["fontColor"]).SelectedIndexChanged += ToolCombo_SelectedIndexChanged;

		}






		// ---------------------------------------------------------------------------
	


		/// <summary>
		/// Détermine si le texte est du text RTF, i.e. si celui-ci commence par "{\rtf".
		/// </summary>
		protected virtual bool IsRTF(string text)
		{
			return text.StartsWith(@"{\rtf");
		}






		#endregion METHODES PROTEGEES














		// ---------------------------------------------------------------------------
		// GESTIONNAIRES D'EVENEMENTS
		// ---------------------------------------------------------------------------




		#region GESTIONNAIRES D'EVENEMENTS





		/// <summary>
		/// Dessine une bordure autour du panel.
		/// </summary>
		protected virtual void TextEditor_Paint(object sender, PaintEventArgs e)
		{
			Graphics graph = e.Graphics;
			graph.DrawRectangle(new Pen(Color.FromArgb(171, 173, 179), 2F), 0, 0, this.Size.Width, this.Size.Height);
			//graph.DrawRectangle(new Pen(Color.FromArgb(171, 173, 179), 2F), new Rectangle(new Point(0, 0), this.Size));
		}





		// ---------------------------------------------------------------------------
	




		/// <summary>
		/// Si Enter dans un contrôle, sélectionne RTF.
		/// </summary>
		protected virtual void ToolCombo_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter) { _rtf.Select(); }
		}





		// ---------------------------------------------------------------------------
	



		/// <summary>
		/// Dans combo FontSize, élimine les caractère qui ne sont pas des chiffres.
		/// </summary>
		protected void ToolComboFontSize_KeyPress(object sender, KeyPressEventArgs e)
		{
			if ("1234567890".IndexOf(e.KeyChar) == -1) { e.Handled = true; }
		}





		// ---------------------------------------------------------------------------
	



		/// <summary>
		/// Met à jour l'état des contrôles de mise en forme quand sélection dans le RTF change.
		/// </summary>
		protected void _rtf_SelectionChanged(object sender, EventArgs e)
		{
			this.ToolsStateFromSelectionFormat();
		}





		// ---------------------------------------------------------------------------
	



		/// <summary>
		/// Quand sélection change dans Combo, met en forme le texte.
		/// </summary>
		protected void ToolCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.ChangeSelectionFormat();
		}





		// ---------------------------------------------------------------------------
	



		/// <summary>
		/// Quand Combo perd le focus, vérifie que le nom de la police ou de la couleur sont valides, et met en forme.
		/// </summary>
		protected void ToolCombo_LostFocus(object sender, EventArgs e)
		{
		
			// Variables:
			ToolStripComboBox cbo = (ToolStripComboBox)sender;

			// Vérifie que la police existe:
			if (cbo.Name.Equals("font")) { if (cbo.FindString(cbo.Text) == -1) { cbo.Text = _rtf.SelectionFont.Name; } }
			
			// Vérifie que la couleur existe:
			if (cbo.Name.Equals("fontColor")) { if (cbo.FindString(cbo.Text) == -1) { cbo.Text = _rtf.SelectionColor.Name; } }
			
			// Mais à jour le format du texte:
			this.ChangeSelectionFormat();
			
			// Désélectionne le texte:
			cbo.SelectionStart = 0;
			cbo.SelectionLength = 0;
			
		}





		// ---------------------------------------------------------------------------
	



		/// <summary>
		/// Clic sur bouton : Ouvre, enregistre, ou modifie l'état check des boutons (et dans ce cas, met en forme le texte).
		/// </summary>
		protected void ToolButton_Click(object sender, EventArgs e)
		{
		
			ToolStripButton cmd = (ToolStripButton)sender;
			string fileName;
			
			switch (cmd.Name)
			{
			
				// Boutons d'ouverture et d'enregistrement:
				
				case "open":
					fileName = My.FilesAndStreams.MyOpenFileDialog("RTF|*.rtf");
					if (String.IsNullOrEmpty(fileName)) { return; }
					this.SetTextFromBytes(My.FilesAndStreams.ReadBinary(fileName));
					return;
			
				case "save":
					fileName = My.FilesAndStreams.MySaveFileDialog("RTF|*.rtf");
					if (String.IsNullOrEmpty(fileName)) { return; }
					My.FilesAndStreams.WriteBinary(fileName, this.GetBytesFromText(true));
					return;
			
				// Boutons simples:
				
				case "bold":
				case "italic":
				case "underline":
					cmd.Checked = !cmd.Checked;
					break;
				
				// Boutons exclusifs:
				
				case "left":
					cmd.Checked = true;
					((ToolStripButton)_tools.Items["center"]).Checked = false;
					((ToolStripButton)_tools.Items["right"]).Checked = false;
					break;
					
				case "center":
					cmd.Checked = true;
					((ToolStripButton)_tools.Items["left"]).Checked = false;
					((ToolStripButton)_tools.Items["right"]).Checked = false;
					break;
					
				case "right":
					cmd.Checked = true;
					((ToolStripButton)_tools.Items["left"]).Checked = false;
					((ToolStripButton)_tools.Items["center"]).Checked = false;
					break;
					
			}
			
			// Met en forme le texte:
			this.ChangeSelectionFormat();
			
		}




		#endregion GESTIONNAIRES D'EVENEMENTS
	
	
	
	
	
	}
	
	
	
	
}
