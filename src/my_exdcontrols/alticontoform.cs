using System;
using System.Windows.Forms;

namespace My
{


	/// <summary>
	/// Gère un NotifyIcon en relation avec un form : Lorsque ce dernier est masqué (visible == false), un NotifyIcon apparaît. Un clique sur le NotifyIcon permet de rendre à nouveau visible le form. De plus, lorsque l'utilisateur réduit la fenêtre en maintenant la touche MAJ appuiyée, le form disparaît de la barre des tâches et un NotifyIcon est affiché.
	/// </summary>
	public class AltIconToForm
	{







		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS



		// Champ privé:
		protected Form _form;
		protected FormWindowState _lastWindowState;
		protected bool _alwaysShow;
		protected ContextMenuStrip _contextMenu;
		
		// Evénements:
		
		/// <summary>
		/// Evénement déclenché lorsque l'utilsateur clique sur le menu contextuel Exit du NotifyIcon.
		/// </summary>
		public event EventHandler AskForExit;
	
	
		#endregion DECLARATIONS







		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES



		/// <summary>
		/// Obtient ou définit le NotifyIcon. Un NotifyIcon est créé par le constructeur. S'il est remplacé, il faut redéfinir le gestionnaire d'événement Click.
		/// </summary>
		public NotifyIcon NotifyIcon { get; set; }





		// ---------------------------------------------------------------------------
	
		
		
		
		
		
		/// <summary>
		/// Obtient ou définit si le NotifyIcon est toujours visible. Si false (par défaut), le NotifyIcon disparaît lorsque la fenêtre est visible.
		/// </summary>
		public bool NotifyIconAlwaysVisible
		{
			get { return _alwaysShow; }
			set { _alwaysShow = value; if (value) { this.NotifyIcon.Visible = true; } }
		}






		// ---------------------------------------------------------------------------
	
		
		
		
		
		
		/// <summary>
		/// Obtient ou définit si l'événement FormClosing avec "UserClosing" comme raison (déclenché par un clic sur la croix, mais aussi par x.Close(), mais pas par Application.Exit()) réduit l'application dans le NotifyIcon plutôt que de fermer l'application. False par défaut.
		/// </summary>
		public bool UserClosingHideWindow { get; set; }






		// ---------------------------------------------------------------------------
	
		
		
		
		
		
		/// <summary>
		/// Obtient ou définit si le menu contextuel Exit est affiché. Si l'utilisateur clique sur ce menu, l'événement AskForExit est déclenché. False par défaut.
		/// </summary>
		public bool ShowExitMenu
		{
			get { return _contextMenu.Items[1].Visible; }
			set { _contextMenu.Items[1].Visible = value; }
		}
		
		
		
		
		
		#endregion PROPRIETES







		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS



		public AltIconToForm(Form form)
		{
		
			// Initialisation des variables:
			
			_form = form;
			
			
			// Initialisation du NotifyIcon:
			
			this.NotifyIcon = new NotifyIcon();
			this.NotifyIcon.Icon = My.App.DefaultIcon;
			this.NotifyIcon.Visible = false;
			this.NotifyIcon.Text = My.App.Title;
			this.NotifyIcon.MouseClick +=
				delegate(object sender, MouseEventArgs e)
				{
					if (e.Button == MouseButtons.Left) { this.ShowForm(); }
				};


			// Menu contextuel:
			
			_contextMenu = new ContextMenuStrip();
			
			ToolStripMenuItem menuItem = new ToolStripMenuItem();
			menuItem.Name = "show";
			menuItem.Font = new System.Drawing.Font(_contextMenu.Font, System.Drawing.FontStyle.Bold);
			menuItem.Text = My.ExdControls.MyResources.AltIconToForm_menu_Show;
			menuItem.Image = this.NotifyIcon.Icon.ToBitmap();
			menuItem.Click += delegate { this.ShowForm(); };
			_contextMenu.Items.Add(menuItem);
			
			menuItem = new ToolStripMenuItem();
			menuItem.Name = "exit";
			menuItem.Text = My.ExdControls.MyResources.AltIconToForm_menu_Exit;
			menuItem.Click += delegate { this.OnAskForExit(); };
			_contextMenu.Items.Add(menuItem);
			
			this.NotifyIcon.ContextMenuStrip = _contextMenu;
			
			
			// Evénements:
			
			_form.VisibleChanged += new EventHandler(_form_VisibleChanged);
			_form.SizeChanged += new EventHandler(_form_SizeChanged);
			_form.FormClosing += new FormClosingEventHandler(_form_FormClosing);
			_form.FormClosed += delegate { this.NotifyIcon.Dispose(); };
			
			
			// Par défaut:
			
			this.NotifyIconAlwaysVisible = false;
			this.UserClosingHideWindow = false;
			this.ShowExitMenu = false;
		
		}




		#endregion CONSTRUCTEURS








		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------




		#region METHODES






		/// <summary>
		/// Déclenche l'événement AskForExit.
		/// </summary>
		protected virtual void OnAskForExit()
		{
			if (AskForExit != null) { AskForExit(this.NotifyIcon,new EventArgs()); }
		}






		// ---------------------------------------------------------------------------
		
		
		
		
		
		/// <summary>
		/// Rend visible le form.
		/// </summary>
		protected virtual void ShowForm()
		{
			_form.Visible = true;
			_form.WindowState = _lastWindowState;
			_form.Activate();
		}

		
		
		
	

		#endregion METHODES
	












		// ---------------------------------------------------------------------------
		// GESTIONNAIRES D'EVENEMENTS
		// ---------------------------------------------------------------------------




		#region GESTIONNAIRES D'EVENEMENTS



		/// <summary>
		/// Si form minimisé et MAJ appuyé, affiche le NotifyIcon. Sinon, retient le WindowState.
		/// </summary>
		protected void _form_SizeChanged(object sender, EventArgs e)
		{
			// Si le form est réduit et que MAJ est enfoncé, le place en NotifyIcon:
			if ((Control.ModifierKeys == Keys.Shift) && (_form.WindowState == FormWindowState.Minimized))
			{
				_form.Visible = false;
				this.NotifyIcon.Visible = true;
			}
			// Sinon, retient le WindowState courant:
			else
			{
				_lastWindowState = _form.WindowState;
			}
		}




		// ---------------------------------------------------------------------------
	
	
	
	
	
		/// <summary>
		/// Modifie la visibilité du NotifyIcon.
		/// </summary>
		protected void _form_VisibleChanged(object sender, EventArgs e)
		{
			if (_form.Visible)
			{
				if (this.NotifyIconAlwaysVisible == false) { this.NotifyIcon.Visible = false; }
			}
			else
			{
				this.NotifyIcon.Visible = true;
			}
		}







		// ---------------------------------------------------------------------------
	
	
	
	
	
	
	
	
	
		protected void _form_FormClosing(object sender, FormClosingEventArgs e)
		{
			if ((this.UserClosingHideWindow) && (e.CloseReason == CloseReason.UserClosing))
			{
				_form.Visible = false;
				this.NotifyIcon.Visible = true;
				e.Cancel = true;
			}
		}

	
	
		#endregion GESTIONNAIRES D'EVENEMENTS
	


	
	
	}
	
}
