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
	/// Valeurs des modifications apportées à l'image.
	/// </summary>
	public struct ChBmpColorValues
	{
		public bool ConvertToGray { get; set; }
		public decimal Light { get; set; }
		public byte Alpha { get; set; }
		public decimal Red { get; set; }
		public decimal Green { get; set; }
		public decimal Blue { get; set; }
		public bool IsEmpty { get { return (Light == 0 && Alpha == 0 && Red == 0 && Green == 0 && Blue == 0); } }
		public ChBmpColorValues(bool gray, decimal light, byte alpha, decimal red, decimal green, decimal blue) : this()
			{ ConvertToGray = gray; Light = light; Alpha = alpha; Red = red; Green = green; Blue = blue; }
	}




	/// <summary>
	/// Affiche une boîte de dialogue permettant de régler les canaux A, R, G, et B d'une bitmap, en la convertissant, ou  non, d'abord en gris.
	/// </summary>
	public class DialogBoxBmpColors : MyFormMessage
	{






		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS
		
		
		
		// contrôles:
		protected CheckBox _chkGray;
		protected NumericUpDown _numLight;
		protected NumericUpDown _numAlpha;
		protected NumericUpDown _numRed;
		protected NumericUpDown _numGreen;
		protected NumericUpDown _numBlue;
		protected PictureBox _pict;
		// Variables:
		protected Bitmap _bmp;
		protected Bitmap _newBmp;
		



		#endregion DECLARATIONS







		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES
		
		
		
		/// <summary>
		/// Obtient ou définit les valeurs utilisées par l'utilisateur pour modifier l'image.
		/// </summary>
		public ChBmpColorValues TransformValues
		{
			get
			{
				return new ChBmpColorValues(_chkGray.Checked, _numLight.Value, (byte)_numAlpha.Value,
					_numRed.Value, _numGreen.Value, _numBlue.Value);
			}
			set
			{
				_chkGray.Checked = value.ConvertToGray;
				_numLight.Value = value.Light;
				_numAlpha.Value = value.Alpha;
				_numRed.Value = value.Red;
				_numGreen.Value = value.Green;
				_numBlue.Value = value.Blue;
			}
		}

		/// <summary>
		/// Obtient l'image résultat.
		/// </summary>
		public Bitmap NewBitmap { get { return _newBmp; } }



		#endregion PROPRIETES
	





		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS
		
		
		
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public DialogBoxBmpColors(Bitmap bmp)
		{
		
			// Initialisation du form:
			SubtitleBox = "Change bitmap color";
			SetDialogMessage("Choose options:");
			AddButtonsCollection(DialogBoxButtons.OKCancel, 1, true);
			SetDialogIcon(DialogBoxIcon.Search);
		
			// Initialisation des contrôles et des variables:
			_bmp = bmp;
			_newBmp = new Bitmap(bmp);
			
			_pict = new PictureBox();
			_pict.Dock = DockStyle.Fill;
			_pict.SizeMode = PictureBoxSizeMode.Zoom;
			_pict.Image = _bmp;
			
			_chkGray = new CheckBox();
			_chkGray.Dock = DockStyle.Fill;
			_chkGray.Text = "Convert to gray";
			_chkGray.CheckedChanged += delegate { _numLight.Enabled = _chkGray.Checked; this.TransformPicture(); };
			
			_numLight = new NumericUpDown();
			_numLight.Dock = DockStyle.Fill;
			_numLight.Value = 1M;
			_numLight.Minimum = 0;
			_numLight.Maximum = 10;
			_numLight.Increment = 0.1M;
			_numLight.DecimalPlaces = 3;
			_numLight.ValueChanged += delegate { this.TransformPicture(); };
			_numLight.Enabled = false;
			
			_numAlpha = new NumericUpDown();
			_numAlpha.Dock = DockStyle.Fill;
			_numAlpha.Minimum = 0;
			_numAlpha.Maximum = 255;
			_numAlpha.Value = 255M;
			_numAlpha.Increment = 5M;
			_numAlpha.DecimalPlaces = 0;
			_numAlpha.ValueChanged += delegate { this.TransformPicture(); };
			
			_numRed = new NumericUpDown();
			_numRed.Dock = DockStyle.Fill;
			_numRed.Value = 1M;
			_numRed.Minimum = 0;
			_numRed.Maximum = 10;
			_numRed.Increment = 0.1M;
			_numRed.DecimalPlaces = 3;
			_numRed.ValueChanged += delegate { this.TransformPicture(); };
			
			_numGreen = new NumericUpDown();
			_numGreen.Dock = DockStyle.Fill;
			_numGreen.Value = 1M;
			_numGreen.Minimum = 0;
			_numGreen.Maximum = 10;
			_numGreen.Increment = 0.1M;
			_numGreen.DecimalPlaces = 3;
			_numGreen.ValueChanged += delegate { this.TransformPicture(); };
			
			_numBlue = new NumericUpDown();
			_numBlue.Dock = DockStyle.Fill;
			_numBlue.Value = 1M;
			_numBlue.Minimum = 0;
			_numBlue.Maximum = 10;
			_numBlue.Increment = 0.1M;
			_numBlue.DecimalPlaces = 3;
			_numBlue.ValueChanged += delegate { this.TransformPicture(); };
			
			Label lblLight = new Label();
			lblLight.Dock = DockStyle.Fill;
			lblLight.Text = "Light";
			
			Label lblAlpha = new Label();
			lblAlpha.Dock = DockStyle.Fill;
			lblAlpha.Text = "Alpha";
			
			Label lblRed = new Label();
			lblRed.Dock = DockStyle.Fill;
			lblRed.Text = "Red";
			
			Label lblGreen = new Label();
			lblGreen.Dock = DockStyle.Fill;
			lblGreen.Text = "Green";
			
			Label lblBlue = new Label();
			lblBlue.Dock = DockStyle.Fill;
			lblBlue.Text = "Blue";
			
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.Dock = DockStyle.Fill;
			tlp.RowCount = 6;
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 16));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 16));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 16));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 16));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 16));
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 16));
			tlp.ColumnCount = 3;
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
			
			tlp.SetRowSpan(_pict, 6);
			tlp.Controls.Add(_pict, 0, 0);
			tlp.SetColumnSpan(_chkGray, 2);
			tlp.Controls.Add(_chkGray, 1, 0);
			tlp.Controls.Add(lblLight, 1, 1);
			tlp.Controls.Add(_numLight, 2, 1);
			tlp.Controls.Add(lblAlpha, 1, 2);
			tlp.Controls.Add(_numAlpha, 2, 2);
			tlp.Controls.Add(lblRed, 1, 3);
			tlp.Controls.Add(_numRed, 2, 3);
			tlp.Controls.Add(lblGreen, 1, 4);
			tlp.Controls.Add(_numGreen, 2, 4);
			tlp.Controls.Add(lblBlue, 1, 5);
			tlp.Controls.Add(_numBlue, 2, 5);
			
			_tlpBody.Controls.Add(tlp, 0, 1);
			
		}




		#endregion CONSTRUCTEURS






		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------




		#region METHODES
		
		
		
		/// <summary>
		/// Transforme l'image en multipliant les canaux Alpha, R, G, et B de l'image.
		/// </summary>
		protected void TransformPicture()
		{
			_newBmp = DialogBoxBmpColors.TransformPicture(_numLight.Value, (byte)_numAlpha.Value, _numRed.Value,
				_numGreen.Value, _numBlue.Value, _chkGray.Checked, _bmp);
			_pict.Image = _newBmp;
			_pict.Refresh();
		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Transforme l'image en multipliant les canaux Alpha, R, G, et B de l'image.
		/// </summary>
		public unsafe static Bitmap TransformPicture(decimal light, byte alpha, decimal red, decimal green, decimal blue, bool toGray, Bitmap src)
		{
		
			// Pour toutes les lignes et colonnes de pixels...
			int width = src.Width; int height = src.Height; byte R, G, B;
			Bitmap dest = new Bitmap(width, height, src.PixelFormat);
			dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
			
			float lightF = (float)light, redF = (float)red, greenF = (float)green, blueF = (float)blue;
						
			// Cas d'une image 32 bits: (Voir Leblanc, 2008, p. 399-400)
			if (src.PixelFormat == PixelFormat.Format32bppArgb || src.PixelFormat == PixelFormat.Format32bppPArgb
				|| src.PixelFormat == PixelFormat.Format32bppRgb)
			{
				// Définit un BitmapData:
				BitmapData srcData = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, src.PixelFormat);
				BitmapData destData = dest.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, dest.PixelFormat);
				// Définit un pointeur, et trouve le début de la ligne:
				Pixel32Data* srcP, srcPStart = (Pixel32Data *)srcData.Scan0;
				Pixel32Data* destP, destPStart = (Pixel32Data *)destData.Scan0;
				for (int row=0; row<height; row++)
				{
					// Trouve le début de la ligne:
					srcP = srcPStart + row * width;
					destP = destPStart + row * width;
					for (int col=0; col<width; col++)
					{
						// Obtient les couleurs d'origine:
						R = srcP->Red; G = srcP->Green; B = srcP->Blue;
						// Modifie la valeur alpha:
						destP->Alpha = (srcP->Alpha > alpha ? alpha : srcP->Alpha);
						// S'il faut convertir en gris, obtient la moyenne des couleurs et modifie avec la valeur light:
						if (toGray)
							{ R = G = B = (byte)Math.Min((((R + G + B) / 3) * lightF), Byte.MaxValue); }
						// Pour chaque couleur, multiplie par la valeur donnée:
						destP->Red = (byte)Math.Min((R * redF), Byte.MaxValue);
						destP->Green = (byte)Math.Min((G * greenF), Byte.MaxValue);
						destP->Blue = (byte)Math.Min((B * blueF), Byte.MaxValue);
						srcP++; destP++;
					}
				}
				// Dégage le BitmapData:
				src.UnlockBits(srcData);
				dest.UnlockBits(destData);
			}		

			// Cas d'une image 24 bits: (Voir Leblanc, 2008, p. 399-400)
			else if (src.PixelFormat == PixelFormat.Format24bppRgb)
			{
				// Définit un BitmapData:
				BitmapData srcData = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, src.PixelFormat);
				BitmapData destData = dest.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, dest.PixelFormat);
				// Définit un pointeur, et trouve le début de la ligne:
				Pixel24Data* srcP, srcPStart = (Pixel24Data *)srcData.Scan0;
				Pixel24Data* destP, destPStart = (Pixel24Data *)destData.Scan0;
				for (int row=0; row<height; row++)
				{
					// Trouve le début de la ligne:
					srcP = srcPStart + row * ((width + 1) / 4 * 4);
					destP = destPStart + row * ((width + 1) / 4 * 4);
					for (int col=0; col<width; col++)
					{
						// Obtient les couleurs d'origine:
						R = srcP->Red; G = srcP->Green; B = srcP->Blue;
						// S'il faut convertir en gris, obtient la moyenne des couleurs et modifie avec la valeur light:
						if (toGray)
							{ R = G = B = (byte)Math.Min((((R + G + B) / 3) * lightF), Byte.MaxValue); }
						// Pour chaque couleur, multiplie par la valeur donnée:
						destP->Red = (byte)Math.Min((R * redF), Byte.MaxValue);
						destP->Green = (byte)Math.Min((G * greenF), Byte.MaxValue);
						destP->Blue = (byte)Math.Min((B * blueF), Byte.MaxValue);
						srcP++; destP++;
					}
				}
				// Dégage le BitmapData:
				src.UnlockBits(srcData);
				dest.UnlockBits(destData);
			}
			
			// Cas des autre images: (utilisation du code safe, avec GetPixel et SetPixel)
			else
			{
				byte[] dat;
				for (int i=0; i<width; i++)
				{
					for (int j=0; j<height; j++)
					{
						// Obtient la couleur:
						Color c = src.GetPixel(i, j);
						dat = BitConverter.GetBytes(c.ToArgb());
						R = dat[2]; G = dat[1]; B = dat[0];
						// Convertit en octets:
						// Modifie la valeur alpha:
						if (dat[3] > alpha) dat[3] = alpha;
						// S'il faut convertir en gris, obtient la moyenne des couleurs et modifie avec la valeur light:
						if (toGray)
							{ R = G = B = (byte)Math.Min((((R + G + B) / 3) * lightF), Byte.MaxValue); }
						// Pour chaque couleur, multiplie par la valeur donnée:
						dat[2] = (byte)Math.Min((R * redF), Byte.MaxValue);
						dat[1] = (byte)Math.Min((G * greenF), Byte.MaxValue);
						dat[0] = (byte)Math.Min((B * blueF), Byte.MaxValue);
						// Transforme le pixel de la nouvelle image:
						dest.SetPixel(i, j, Color.FromArgb(BitConverter.ToInt32(dat, 0)));
					}
				}
			}			
			
			return dest;
			
		}


		#endregion METHODES
	
	
	
	}
	
	
	
	
}
