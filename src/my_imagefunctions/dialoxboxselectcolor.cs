using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace My
{



	/// <summary>
	/// Affiche une boîte de dialogue permettant à l'utilisateur de choisir un Color.
	/// </summary>
	public class DialogBoxSelectColor : MyFormIcon
	{






		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS
		
		
		// contrôles:
		protected ListView _list;
		protected NumericUpDown _numAlpha;
		// variables:
		protected Color _selectedColor;
		protected int _size;
		



		#endregion DECLARATIONS







		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES
		
		
		/// <summary>
		/// Obtient la couleur choisie.
		/// </summary>
		public Color SelectedColor { get { return _selectedColor; } }

		/// <summary>
		/// Obtient ou définit le côté des carrés de couleur.
		/// </summary>
		public int SideSize { get { return _size; } set { _size = value; } }



		#endregion PROPRIETES
	





		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS
		
		
		
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public DialogBoxSelectColor()
		{
		
			// Initialisation du form:
			SubtitleBox = "Select color";
			AddButtonsCollection(DialogBoxButtons.OKCancel, 1, true);
			SetDialogIcon(DialogBoxIcon.Search);
			WindowState = FormWindowState.Maximized;
		
			// Initialisation des variables:
			_size = 100;
			_selectedColor = Color.Black;
			
			// Création d'un LV:
			_list = new ListView();
			_list.View = View.LargeIcon;
			_list.Dock = DockStyle.Fill;
			_list.MultiSelect = false;
			_list.HideSelection = false;
			// Définition de l'ImageList:
			_list.LargeImageList = new ImageList();
			_list.LargeImageList.ImageSize = new Size(_size, _size);
			_list.LargeImageList.ColorDepth = ColorDepth.Depth32Bit;
			// Parcours toutes les couleurs:
			int c = 0; Bitmap bmp; Graphics g; ListViewItem item;
			foreach (Color color in ColorFunctions.ColorList)
			{
				// Créer un bmp, un Graphics et peint une couleur unie sur le Graphics associé au bmp:
				bmp = new Bitmap(_size, _size);
				g = Graphics.FromImage(bmp);
				g.Clear(color);
				// Ajoute le bmp à l'ImageList, et le nom au LV:
				_list.LargeImageList.Images.Add(bmp);
				item = new ListViewItem(color.Name, c++);
				item.Tag = color;
				_list.Items.Add(item);
				if (color == Color.Black) { item.Selected = true; }
			}
			
			// NumericUpDown:
			Label lblAlpha = new Label();
			lblAlpha.Dock = DockStyle.Fill;
			lblAlpha.Text = "Alpha: ";
			lblAlpha.TextAlign = ContentAlignment.MiddleCenter;
			_numAlpha = new NumericUpDown();
			_numAlpha.Dock = DockStyle.Fill;
			_numAlpha.Minimum = 0;
			_numAlpha.Maximum = 255;
			_numAlpha.Value = 255M;
			_numAlpha.DecimalPlaces = 0;
			
			// Label:
			Label lblMessage = new Label();
			lblMessage.Dock = DockStyle.Fill;
			lblMessage.Text = "Select a color, and click OK:";
			lblAlpha.TextAlign = ContentAlignment.MiddleLeft;
			
			// TLP:
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.Dock = DockStyle.Fill;
			tlp.RowCount = 2;
			tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			tlp.ColumnCount = 3;
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
			
			tlp.Controls.Add(lblMessage, 0, 0);
			tlp.Controls.Add(lblAlpha, 1, 0);
			tlp.Controls.Add(_numAlpha, 2, 0);
			tlp.SetColumnSpan(_list, 3);
			tlp.Controls.Add(_list, 0, 1);
			
			_tlpBase.Controls.Add(tlp, 1, 0);

			// Evénements:
			_list.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(_list_ItemSelectionChanged);
			_list.DoubleClick += delegate { ClickResult = DialogBoxClickResult.OK; this.Hide(); };
			_numAlpha.ValueChanged += new EventHandler(_numAlpha_ValueChanged);
			
			// Sélection:
			_list.Select();
			
		}



		#endregion CONSTRUCTEURS





		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------




		#region METHODES



		/// <summary>
		/// Gestionnaire d'événement. Change la couleur sélectionnée.
		/// </summary>
		private void _numAlpha_ValueChanged(object sender, EventArgs e)
		{
			_selectedColor = Color.FromArgb((int)_numAlpha.Value, _selectedColor);
		}

		/// <summary>
		/// Gestionnaire d'événement. Change la couleur sélectionnée.
		/// </summary>
		private void _list_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (e.IsSelected)
			{
				// Si Transparent, retourne directement la couleur:
				if ((Color)e.Item.Tag == Color.Transparent) { _selectedColor = Color.Transparent; }
				// Sinon, transforme avec le canal Alpha:
				else { _selectedColor = Color.FromArgb((int)_numAlpha.Value, (Color)e.Item.Tag); }
			}
		}



		#endregion METHODES
	

	
	}
	
	
	
	
}
