using System;
using System.Drawing;
using System.Windows.Forms;

namespace My
{






	/// <summary>
	/// Form de base, qui doit servir de base à tous les autres form, des fenêtres d'application aux boîtes de dialogues. Cette classe est censée être héritée de multiple fois, de sorte que chaque enfant apporte un nouvel élément de construction...
	/// </summary>
	public abstract class MyForm : Form
	{





		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS


		/// <summary>
		/// Constante pour les messages de window:
		/// </summary>
		private const int SC_CLOSE = 0xF060;
		
		/// <summary>
		/// Taille par défaut des boutons.
		/// </summary>
		protected static Size _buttonDefaultSize;
		
		/// <summary>
		/// Taille par défaut des fenêtres.
		/// </summary>
		protected static Size _formDefaultSize;
		
		/// <summary>
		/// TLP sur lequel doivent être insérer les autres contrôles, tandis que _tlpForm, bien que protected, ne doit pas être utilisé, sauf utilisation spéciale (comme la suppression de la barre blanche).
		/// </summary>
		protected TableLayoutPanel _tlpBase;

		/// <summary>
		/// Obtient ou définit si l'utilisateur peut fermer le form via le bouton X, le menu Alt+Space ou bien Alt+F4. False par défaut.
		/// </summary>
		protected bool _enableUserClosing;

		/// <summary>
		/// TLP plaqué directement sur le form, contenant la barre des titres, dans la première ligne, la seconde contenant le _tlpBase.
		/// </summary>
		protected TableLayoutPanel _tlpForm;
		
		// Autres déclarations:
		private Panel _pnlTitleBar;
		private Label _lblSubtitle;
		private Label _lblTitle;
		private PictureBox _pictTitleBar;
		private static Pen _penDarkLine;
		private static Pen _penLightLine;



		#endregion DECLARATIONS





		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------



		
		#region PROPRIETES


		/// <summary>
		/// Obtient une taille par défaut des boutons.
		/// </summary>
		public static Size ButtonDefaultSize {
			get { return _buttonDefaultSize; } }


		/// <summary>
		/// Obtient une taille par défaut des fenêtres.
		/// </summary>
		public static Size FormDefaultSize {
			get { return _formDefaultSize; } }


		/// <summary>
		/// Obtient ou définit le titre de la fenêtre. Par défaut, il s'agit de la valeur de My.App.DefaultAppTitle.
		/// </summary>
		public string TitleForm {
			get { return this.Text; }
			set { if (value != null) { this.Text = value; } } }


		/// <summary>
		/// Obtient ou définit le titre de la boîte de dialogue, affiché dans la barre blanche. Par défaut, il s'agit de la valeur de My.App.DefaultAppTitle.
		/// </summary>
		public string TitleBox {
			get { return _lblTitle.Text; }
			set { if (value != null) { _lblTitle.Text = value; } } }


		/// <summary>
		/// Obtient ou définit le sous-titre de la boîte (en maigre). Par défaut vide.
		/// </summary>
		public string SubtitleBox {
			get { return _lblSubtitle.Text; }
			set { if (value != null) { _lblSubtitle.Text = value; } } }


		#endregion PROPRIETES






		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------



		#region CONSTRUCTEURS


		
		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static MyForm()
		{
			// Initialisation des variables:
			_buttonDefaultSize = new Size(75, 22);
			_formDefaultSize = new Size(504, 386);
			_penDarkLine = new Pen(SystemColors.ControlDark); // = Color.FromArgb(172, 168, 153)
			_penLightLine = new Pen(SystemColors.ControlLightLight);
		}

        /// <summary>
        /// Constructeur.
        /// </summary>
        public MyForm(bool showTitleBar=true)
        {

            // Initialise les composants:
            //InitializeComponent();

            // Initialise les variables:
            _enableUserClosing = false;

            // Initilisation du picture en haut à droite:
            _pictTitleBar = new PictureBox();
            _pictTitleBar.InitialImage = null;
            _pictTitleBar.Location = new Point(385, -4);
            _pictTitleBar.Margin = new Padding(0);
            _pictTitleBar.Size = new Size(111, 60);
            _pictTitleBar.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictTitleBar.TabStop = false;

            // Initialisation du label de titre:
            _lblTitle = new Label();
            _lblTitle.AutoSize = true;
            _lblTitle.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
            _lblTitle.Location = new Point(10, 9);
            _lblTitle.Size = new Size(94, 13);
            _lblTitle.BackColor = Color.Transparent;
            _lblTitle.Text = "Title of the Box";

            // Initialisation du label de sous-titre:
            _lblSubtitle = new Label();
            _lblSubtitle.AutoSize = true;
            _lblSubtitle.Location = new Point(25, 31);
            _lblSubtitle.Size = new Size(92, 13);
            _lblSubtitle.Text = "Subtitle of the box";

            // Initialisation du panel des titres:
            _pnlTitleBar = new Panel();
            _pnlTitleBar.BackColor = Color.White;
            _pnlTitleBar.Controls.Add(_lblTitle);
            _pnlTitleBar.Controls.Add(_lblSubtitle);
            _pnlTitleBar.Controls.Add(_pictTitleBar);
            _pnlTitleBar.Dock = DockStyle.Fill;
            _pnlTitleBar.Location = new Point(0, 0);
            _pnlTitleBar.Margin = new Padding(0);

            // Initilisation de _tlpBase:
            _tlpBase = new TableLayoutPanel();
            _tlpBase.Dock = DockStyle.Fill;
            _tlpBase.RowCount = 1;
            _tlpBase.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _tlpBase.ColumnCount = 1;
            _tlpBase.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _tlpBase.Padding = new Padding(5, 15, 5, 5);

            // Initialisation du _tlpForm:
            _tlpForm = new TableLayoutPanel();
            _tlpForm.BackColor = SystemColors.Control;
            _tlpForm.Dock = DockStyle.Fill;
            _tlpForm.Location = new Point(0, 0);
            _tlpForm.RowCount = 2;
            if (showTitleBar) {
                _tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            }
            else {
                _tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
            }
            _tlpForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			_tlpForm.ColumnCount = 1;
			_tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            if (showTitleBar) {
                _tlpForm.Controls.Add(_pnlTitleBar, 0, 0);
            }
			_tlpForm.Controls.Add(_tlpBase, 0, 1);

			// Propriété du form:
			this.StartPosition = FormStartPosition.CenterScreen;
			this.AutoScroll = false;
			this.Height = _formDefaultSize.Height;
			this.Width = _formDefaultSize.Width;
			this.Icon = App.DefaultIcon;
			this.Controls.Add(_tlpForm);

			// Image du titre:
			_pictTitleBar.Image = MyResources.frmBaseForm_img_TitleBar_jpg;
			
			// Titre de la fenêtre par défaut, et celui de la box:
			if (My.App.Title != null) {
				this.Text = My.App.Title;
				_lblTitle.Text = My.App.Title;
				_lblSubtitle.Text = ""; } //String.Format("by Bruno Oberle - {0}", My.App.GetEntryAssemblyVersion()); }

			// Quand la taille change, modifie la position des contrôles de la barre des titres:
			this.SizeChanged += delegate {
				_pictTitleBar.Left = _pnlTitleBar.Right - _pictTitleBar.Width;
				_pictTitleBar.Top = _pnlTitleBar.Bottom - _pictTitleBar.Height; };
			
			// Evénément Paint pour tracer les barres:
			_tlpForm.Paint += delegate(object sender, PaintEventArgs e) {
				DrawShadedLine(e.Graphics, new PointF(_tlpForm.Left, _tlpForm.RowStyles[0].Height),
					new PointF(_tlpForm.Right, _tlpForm.RowStyles[0].Height)); };
			
		}



		#endregion CONSTRUCTEURS





		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------



		#region METHODES



		/// <summary>
		/// Dessine un trait d'ombre entre deux points. Ce trait est constitué de deux traits, un clair et juste en-dessous un trait gris sombre pour simuler l'ombre.
		/// </summary>
		public static void DrawShadedLine(Graphics graph, PointF startPt, PointF endPt)
		{
			graph.DrawLine(_penDarkLine, startPt, endPt);
			startPt.Y++; endPt.Y++;
			graph.DrawLine(_penLightLine, startPt, endPt);
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Annule la fermeture si l'utilisateur veut fermer la fenêtre par le bouton X, Alt+F4, etc., et si la propriété EnableUserClosing vaut false.
		/// </summary>
		protected override void WndProc(ref Message m)
		{
			// Sort si le message est un message de fermeture engagée par l'utilisateur.
			if ((_enableUserClosing == false) && (m.WParam.ToInt32() == SC_CLOSE)) { return; }
			// Sinon, laisser traiter le message.
			base.WndProc(ref m);
		}



		#endregion METHODES


	
	}
	
	
	
	
}
