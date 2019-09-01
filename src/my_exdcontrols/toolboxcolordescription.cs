using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace My
{


	/// <summary>
	/// Affiche une petite boîte de dialogue en ToolBox sizable, affichant les informations de couleurs du pixel sous le pointeur de la souris, n'importe où sur l'écran, à chaque appel de ShowColorDescription.
	/// </summary>
	public class ToolBoxColorDescription : Form
	{
	
		private Label _lblText;
		private Bitmap _bmp;
		private Graphics _graph;
		private Size _oneSize;
		private bool _keyDownShow;
		
		/// <summary>
		/// Obtient ou définit si la valeur de couleur doit être mise à jour lorsque l'utilisateur appuie sur Espace (on peut alors se dispenser d'appeler ShowColorDescription).
		/// </summary>
		public bool KeyDowShow {
			get { return _keyDownShow; }
			set { _keyDownShow = value;
				if (value) { KeyDown += new KeyEventHandler(ToolBoxColorDescription_KeyDown); }
				else { KeyDown -= ToolBoxColorDescription_KeyDown; } } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public ToolBoxColorDescription()
		{
			// Initialisation des variables:
			_bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			_graph = Graphics.FromImage(_bmp);
			_graph.CompositingQuality = CompositingQuality.HighSpeed;
			_graph.InterpolationMode = InterpolationMode.Low;
			_graph.PixelOffsetMode = PixelOffsetMode.None;
			_graph.SmoothingMode = SmoothingMode.None;
			_oneSize = new Size(1, 1);
			// Initialisation des contrôles:
			_lblText = new Label();
			_lblText.Dock = DockStyle.Fill;
			_lblText.Font = new Font(FontFamily.GenericMonospace, 8.5F);
			Controls.Add(_lblText);
			// Initialisation du form:
			FormBorderStyle = FormBorderStyle.SizableToolWindow;
			Width = 170; Height = 75; Text = "Color";
			TopMost = true;
			ShowColorDescription(Color.Black);
			ShowInTaskbar = false;
		}

		/// <summary>
		/// Affiche les informations de couleur passé.
		/// </summary>
		public void ShowColorDescription(Color color)
		{
			_lblText.Text = String.Format("R: {0,3}    H: {3,8:0.0000}\nG: {1,3}    S: {4,8:0.0000}\nB: {2,3}    L: {5,8:0.0000}\n",
				color.R, color.G, color.B, color.GetHue(), color.GetSaturation(), color.GetBrightness());
		}
	
		/// <summary>
		/// Affiche les informations de couleur du pixel situé sous le pointeur de la souris, n'importe où sur l'écran.
		/// </summary>
		public void ShowColorDescription()
		{
			int x = Control.MousePosition.X;
			int y = Control.MousePosition.Y;
			_graph.CopyFromScreen(x, y, 0, 0, _oneSize);
			ShowColorDescription(_bmp.GetPixel(0, 0));
		}

		/// <summary>
		/// Si barre d'espace, affiche les informations.
		/// </summary>
		private void  ToolBoxColorDescription_KeyDown(object sender, KeyEventArgs e)
		{
 			if (e.KeyCode == Keys.Space) { ShowColorDescription(); }
		}
		
	}
	
	
}
