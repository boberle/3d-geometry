using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using My.ExdControls;

namespace My
{




	/// <summary>
	/// Affiche un navigateur web avec des contrôles de base (Home, Back, Forward, Home, URL), dans un TableLayoutPanel (la classe hérite de TableLayoutPanel), directement insérable. Le composant principal est un ExdWebBrowser, qui hérite de WebBrowser. Dans cet ExdWebBrowser, les prorpriétés DocumentText et DocumentStream ont été réécrites afin d'inclure dans l'historique du BasicWebNavigator les textes et stream originaux, et de revenir à eux par l'historique ou le bouton Home. Il faut donc définir les propriétés de navigation par la propriété WebBrowser, et les propriétés de disposition directement par les propriétés du BasicWebNavigator héritées de TableLayoutPanel.
	/// </summary>
	public class BasicWebNavigator : TableLayoutPanel
	{









		// ---------------------------------------------------------------------------
		// SOUS-CLASSE
		// ---------------------------------------------------------------------------




		#region SOUS-CLASSE




		/// <summary>
		/// Cette classe ajoute simplement deux événements à WebBrowser : NewDocumentTex et NewDocumentStream sont déclenchés lorsque les propriétés DocumentText et DocumentStream sont modifiés.
		/// </summary>
		public class ExdWebBrowser : WebBrowser
		{
		
			public class NewDocumentTextEventArgs : EventArgs
			{
				public string Text { get; set; }
				public NewDocumentTextEventArgs(string text) { this.Text = text; }
			}
			public class NewDocumentStreamEventArgs : EventArgs
			{
				public Stream Stream { get; set; }
				public NewDocumentStreamEventArgs(Stream stream) { this.Stream = stream; }
			}
		
			public delegate void NewDocumentTextEventHandler(object sender, NewDocumentTextEventArgs e);
			public delegate void NewDocumentStreamEventHandler(object sender, NewDocumentStreamEventArgs e);
			public event NewDocumentTextEventHandler NewDocumentText;		
			public event NewDocumentStreamEventHandler NewDocumentStream;
			
			public ExdWebBrowser() : base() { ; }
		
			public new string DocumentText
			{
				get { return base.DocumentText; }
				set { base.DocumentText = value; if (NewDocumentText != null) { NewDocumentText(this, new NewDocumentTextEventArgs(value)); } }
			}
			public new Stream DocumentStream
			{
				get { return base.DocumentStream; }
				set { base.DocumentStream = value; if (NewDocumentStream != null) { NewDocumentStream(this, new NewDocumentStreamEventArgs(value)); } }
			}
			
		}
		
		
		
		

		// ---------------------------------------------------------------------------
			
			
		
		
		
		
		/// <summary>
		/// Cette classe implémente des propriétés pour la gestion interne de l'historique du BasicWebNavigator.
		/// </summary>
		public class WebHistory
		{
			public string DisplayText { get; set; }
			public string DocumentText { get; set; }
			public Stream DocumentStream { get; set; }
			public Uri URL { get; set; }
			
			public WebHistory() { this.DisplayText = String.Empty; this.DocumentText = null; this.DocumentStream = null; this.URL = null; }
			public WebHistory(string display, string documentText) : this() { this.DisplayText = display; this.DocumentText = documentText; }
			public WebHistory(string display, Stream documentStream) : this() { this.DisplayText = display; this.DocumentStream = documentStream; }
			public WebHistory(string display, Uri url) : this() { this.DisplayText = display; this.URL = url; }
		}
	
		
		




		#endregion SOUS-CLASSE
	












		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS



		private ToolStrip _tools;
		private ToolStripButton _cmdRefresh;
		private ToolStripButton _cmdHome;
		private ToolStripButton _cmdGoBack;
		private ToolStripButton _cmdGoForward;
		private ToolStripButton _cmdGoTo;
		private ToolStripComboBox _cboURL;
		
		public ExdWebBrowser WebBrowser { get; set; }
		
		private WebHistory[] _history;
	
	
		#endregion DECLARATIONS








		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS



		
		public BasicWebNavigator()
		{
		
			// Initialisation:
			this.WebBrowser = new ExdWebBrowser();
			
			// Création des contrôles:
			_tools = new ToolStrip();
			_tools.Dock = DockStyle.Fill;
			
			_cmdHome = new ToolStripButton();
			_cmdHome.Image = MyResources.Navigator_img_Home_png_little;
			_cmdGoBack = new ToolStripButton();
			_cmdGoBack.Image = MyResources.Navigator_img_Back_png_little;
			_cmdGoForward = new ToolStripButton();
			_cmdGoForward.Image = MyResources.Navigator_img_Forward_png_little;
			_cmdRefresh = new ToolStripButton();
			_cmdRefresh.Image = MyResources.Navigator_img_Refresh_png_little;
			_cboURL = new ToolStripComboBox();
			_cboURL.AutoSize = false;
			_cboURL.AutoToolTip = true;
			_cmdGoTo = new ToolStripButton();
			_cmdGoTo.Image = MyResources.Navigator_img_Go_png_little;

			_cmdGoBack.Size = new Size(32, 32);
			
			_tools.Items.Add(_cmdHome);
			_tools.Items.Add(_cmdGoBack);
			_tools.Items.Add(_cmdGoForward);
			_tools.Items.Add(_cmdRefresh);
			_tools.Items.Add(_cboURL);
			_tools.Items.Add(_cmdGoTo);
			

			

			// TLP:
			this.RowCount = 2;
			this.RowStyles.Add(new RowStyle(SizeType.Absolute, _tools.Height));
			this.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			this.ColumnCount = 1;
			this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			
			this.Controls.Add(_tools, 0, 0);
			
			
			// WebBrowser:
			this.WebBrowser.Dock = DockStyle.Fill;
			this.WebBrowser.Margin = new Padding(0);
			this.Controls.Add(this.WebBrowser, 0, 1);


			// Evénements:
			_cmdHome.Click += new EventHandler(_cmdHome_Click);
			_cmdGoBack.Click += new EventHandler(_cmdGoBack_Click);
			_cmdGoForward.Click += new EventHandler(_cmdGoForward_Click);
			_cmdRefresh.Click += new EventHandler(_cmdRefresh_Click);
			_cboURL.Validated += new EventHandler(_cboURL_Validated);
			_cboURL.KeyDown += new KeyEventHandler(_cboURL_KeyDown);
			_cboURL.SelectedIndexChanged += new EventHandler(_cboURL_SelectedIndexChanged);
			_cmdGoTo.Click += new EventHandler(_cmdGoTo_Click);
			this.WebBrowser.CanGoBackChanged += new EventHandler(WebBrowser_CanGoBackChanged);
			this.WebBrowser.CanGoForwardChanged += new EventHandler(WebBrowser_CanGoForwardChanged);
			this.WebBrowser.Navigated += new WebBrowserNavigatedEventHandler(WebBrowser_Navigated);
			_tools.SizeChanged += new EventHandler(_tools_SizeChanged);
			this.WebBrowser.NewDocumentText += new ExdWebBrowser.NewDocumentTextEventHandler(WebBrowser_NewDocumentText);
			this.WebBrowser.NewDocumentStream += new ExdWebBrowser.NewDocumentStreamEventHandler(WebBrowser_NewDocumentStream);

		}


		#endregion CONSTRUCTEURS







		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------




		#region METHODES






		/// <summary>
		/// Ajoute un élément à l'historique, c'est-à-dire au ComboBox qui sert de barre d'adresse.
		/// </summary>
		protected virtual void MakeHistory(WebHistory addedItem)
		{
		
			try
			{
			
				// Sort si aucun texte à afficher:
				if (String.IsNullOrEmpty(addedItem.DisplayText)) { return; }
				
				// Sort si l'url est déjà inscrite dans l'élement précédent:
				if ((_history != null) && (String.IsNullOrEmpty(_history[0].DisplayText) == false))
				{
					if (_history[0].DisplayText == addedItem.DisplayText) { return; }
				}
				
				// Sort si about:blank:
				if (addedItem.DisplayText.Equals("about:blank")) { return; }
		
				// Retaille le tableau:
				if (_history == null) { _history = new WebHistory[1]; }
				else {_history = new WebHistory[1].Concat(_history).ToArray<WebHistory>();
				foreach (WebHistory i in _history) { if ((i != null) && (i.DocumentStream != null)) { i.DocumentStream.Seek(0, SeekOrigin.Begin); }} }
				// Ajoute l'élément au tableau:
				_history[0] = addedItem;
				// Reforme la liste:
				_cboURL.SelectedIndexChanged -= _cboURL_SelectedIndexChanged;
				_cboURL.Items.Clear();
				foreach (WebHistory i in _history) { _cboURL.Items.Add(i.DisplayText); }
				_cboURL.SelectedIndex = 0;
				_cboURL.SelectedIndexChanged += new EventHandler(_cboURL_SelectedIndexChanged);
				
			}
			
			catch { ; }
		
		}






		// ---------------------------------------------------------------------------
	
		
		
		/// <summary>
		/// Dispose sur le WebBrowser et sur soi-même.
		/// </summary>
		public new void Dispose()
		{
			this.WebBrowser.Dispose();
			base.Dispose();
		}
		




		#endregion METHODES
	










		
		// ---------------------------------------------------------------------------
		// GESTIONNAIRES D'EVENEMENTS
		// ---------------------------------------------------------------------------




		#region GESTIONNAIRES D'EVENEMENTS





		void _cboURL_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter) { _cmdGoTo_Click(_cmdGoTo, new EventArgs()); }
		}




		void _cmdGoTo_Click(object sender, EventArgs e)
		{
		
			// Sort si rien:
			if (String.IsNullOrEmpty(_cboURL.Text)) { return; }
			
			try
			{
			
				// Si pas de sélection, va à l'url écrite par le texte:
				if ((_cboURL.SelectedIndex == -1) && (String.IsNullOrEmpty(_cboURL.Text) == false)) { this.WebBrowser.Url = new Uri(_cboURL.Text); return; }
			
				// Cherche l'historique courant:
				WebHistory hist = _history[_cboURL.SelectedIndex];
				
				// Charge le document en fonction des informations de l'historique:
				if (hist.DocumentText != null)
				{
					this.WebBrowser.DocumentText = hist.DocumentText;
				}
				else if (hist.DocumentStream != null)
				{
					//this.WebBrowser.NewDocumentStream -= WebBrowser_NewDocumentStream;
					this.WebBrowser.DocumentStream = hist.DocumentStream;
					//this.WebBrowser.NewDocumentStream += new ExdWebBrowser.NewDocumentStreamEventHandler(WebBrowser_NewDocumentStream);
				}
				else if (hist.URL != null) { this.WebBrowser.Url = hist.URL; }

			}

			catch { ; }

		}


		void _cboURL_SelectedIndexChanged(object sender, EventArgs e)
		{
			_cmdGoTo_Click(_cmdGoTo, new EventArgs());
		}



		void WebBrowser_NewDocumentStream(object sender, ExdWebBrowser.NewDocumentStreamEventArgs e)
		{
			MakeHistory(new WebHistory("Html document", e.Stream));
			_cboURL.SelectedIndex = 0;
		}



		void WebBrowser_NewDocumentText(object sender, ExdWebBrowser.NewDocumentTextEventArgs e)
		{
			MakeHistory(new WebHistory("Html document", e.Text));
			_cboURL.SelectedIndex = 0;
		}




		void _tools_SizeChanged(object sender, EventArgs e)
		{
			_cboURL.Width = _tools.ClientSize.Width - (_cmdHome.Width * 6);
		}




		void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			if (WebBrowser.Url != null) { MakeHistory(new WebHistory(this.WebBrowser.Url.AbsoluteUri, this.WebBrowser.Url)); }
		}




		void WebBrowser_CanGoForwardChanged(object sender, EventArgs e)
		{
			if (WebBrowser.CanGoForward) { _cmdGoForward.Enabled = true; }
			else { _cmdGoForward.Enabled = false; }
		}




		void WebBrowser_CanGoBackChanged(object sender, EventArgs e)
		{
			if (WebBrowser.CanGoBack) { _cmdGoBack.Enabled = true; }
			else { _cmdGoBack.Enabled = false; }
		}




		void _cmdGoBack_Click(object sender, EventArgs e)
		{
			if (this.WebBrowser.CanGoBack) { this.WebBrowser.GoBack(); }
		}




		void _cmdHome_Click(object sender, EventArgs e)
		{
			if (_cboURL.Items.Count > 0) { _cboURL.SelectedIndex = _cboURL.Items.Count - 1; }
		}




		void _cmdGoForward_Click(object sender, EventArgs e)
		{
			if (this.WebBrowser.CanGoForward) { this.WebBrowser.GoForward(); }
		}




		void _cmdRefresh_Click(object sender, EventArgs e)
		{
			this.WebBrowser.Refresh();
		}




		void _cboURL_Validated(object sender, EventArgs e)
		{
			_cmdGoTo_Click(_cmdGoTo, new EventArgs());
		}



		#endregion GESTIONNAIRES D'EVENEMENTS





	}
	
	
	
	
	
}
