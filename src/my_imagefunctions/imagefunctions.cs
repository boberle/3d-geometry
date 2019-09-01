using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace My
{



	/// <summary>
	/// Format des 4 octets d'un pixels.
	/// </summary>
	internal struct Pixel32Data
	{
		public byte Blue;
		public byte Green;
		public byte Red;
		public byte Alpha;
	}

	/// <summary>
	/// Format des 3 octets d'un pixels.
	/// </summary>
	internal struct Pixel24Data
	{
		public byte Blue;
		public byte Green;
		public byte Red;
	}




	// ===========================================================================
	
	
	

	/// <summary>
	/// Fournit des fonctions pour les modifications des images.
	/// </summary>
	public static class ImageFunctions
	{
	

		/*/// <summary>
		/// J'ignore comment changer la résolution d'une image sans rééchantillonnage (i.e. simplement changer la taille physique sans toucher à la taille en pixels) par une méthode de C#. Cette méthode permet donc de le faire, sans aucun problème, et rapidement.
		/// </summary>
		public static Bitmap ChangeBitmapResolution(Bitmap src, float xDpi, float yDpi)
		{
			
			// Créer une nouvelle image à partir de l'image d'origine, de même taille (en pixels), en changeant
			//la résolution, puis en copiant, pixel par pixel:
			int width = src.Size.Width, height = src.Size.Height;
			Bitmap dest = new Bitmap(width, height, src.PixelFormat);
			dest.SetResolution(xDpi, yDpi);
			
			// Cas d'une image 32 bits:
			if (Bitmap.GetPixelFormatSize(src.PixelFormat) == 32)
			{
				// Définit un BitmapData:
				BitmapData srcData = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, src.PixelFormat);
				BitmapData destData = dest.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, dest.PixelFormat);
        int bytesNb  = srcData.Stride * height;
				byte[] rgbValues = new byte[bytesNb];
				// Copie de la mémoire vers le tableau:
        System.Runtime.InteropServices.Marshal.Copy(srcData.Scan0, rgbValues, 0, bytesNb);
				// Puis directement du tableau vers la mémoire:
        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, destData.Scan0, bytesNb);
				// Dégage le BitmapData:
				src.UnlockBits(srcData);
				dest.UnlockBits(destData);
			}		
			
			// Cas d'une image 24 bits:
			else if (Bitmap.GetPixelFormatSize(src.PixelFormat) == 24)
			{
				// Définit un BitmapData:
				BitmapData srcData = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, src.PixelFormat);
				BitmapData destData = dest.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, dest.PixelFormat);
        int bytesNb  = srcData.Stride * height;
				byte[] rgbValues = new byte[bytesNb];
				// Copie de la mémoire vers le tableau:
        System.Runtime.InteropServices.Marshal.Copy(srcData.Scan0, rgbValues, 0, bytesNb);
				// Puis directement du tableau vers la mémoire:
        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, destData.Scan0, bytesNb);
				// Dégage le BitmapData:
				src.UnlockBits(srcData);
				dest.UnlockBits(destData);
			}
			
			// Cas des autre images: (utilisation du code safe, avec GetPixel et SetPixel)
			else
			{
				for (int x=0; x<src.Width; x++) {
					for (int y=0; y<src.Height; y++) { dest.SetPixel(x, y, src.GetPixel(x, y)); } }
			}
			
			return dest;

		}*/


		// ---------------------------------------------------------------------------
		
		// Champs pour le remplacement des couleurs:
		private static int[,,] _replRangesMatrix;
		private static float[,,] _colorHSLValuesH, _colorHSLValuesS, _colorHSLValuesL;
		private static byte[] _replRangesR, _replRangesG, _replRangesB;
		
		/// <summary>
		/// Créer un tableau à trois dimensions, chacune de 0 à 255, pour chaque couleur, contenant les index permettant de retouver la valeur de chaque composante dans les tableaux _replRangesR/G/B. Remplit aussi ces derniers tableaux, en rajoutant une ligne, à la fin, désignant defColor. Cette méthode doit être appelée avant l'appel des méthodes de remplacement de plages de couleurs comme ReplaceColorRanges.
		/// </summary>
		public static void MakeReplaceColorRangesMatrix(ColorReplacement[] repl, Color defColor)
		{
			int l = repl.Length; bool defIsEmpty = defColor.IsEmpty;
			_replRangesR = new byte[l+1]; _replRangesG = new byte[l+1]; _replRangesB = new byte[l+1];
			for (int i=0; i<l; i++) {
				_replRangesR[i] = repl[i].ReplaceByColor.R;
				_replRangesG[i] = repl[i].ReplaceByColor.G;
				_replRangesB[i] = repl[i].ReplaceByColor.B; }
			_replRangesR[l] = defColor.R; _replRangesG[l] = defColor.G; _replRangesB[l] = defColor.B;		
		
			_replRangesMatrix = new int[256,256,256];
			bool found;
			for (int R=0; R<256; R++)
			{
				for (int G=0; G<256; G++)
				{
					for (int B=0; B<256; B++)
					{
						found = false;
						for (int i=0; i<l; i++)
						{
							if (B >= repl[i].MinB && B <= repl[i].MaxB && G >= repl[i].MinG && G <= repl[i].MaxG && R >= repl[i].MinR && R <= repl[i].MaxR)
								{ _replRangesMatrix[R,G,B] = i; found = true; break; }
						}
						if (!found) { _replRangesMatrix[R,G,B] = (defIsEmpty ? -1 : l); }
					}
				}
			}
		}
		
		
		/// <summary>
		/// Créer un tableau à trois dimensions, chacune de 0 à 255, pour chaque couleur, contenant les index permettant de retouver la valeur de chaque composante dans les tableaux _replRangesR/G/B. Remplit aussi ces derniers tableaux, en rajoutant une ligne, à la fin, désignant defColor. Cette méthode doit être appelée avant l'appel des méthodes de remplacement de plages de couleurs comme ReplaceColorRanges.
		/// </summary>
		public static void MakeReplaceColorRangesMatrixUsingHSL(HSLColorReplacement[] repl, Color defColor)
		{
			int l = repl.Length; bool defIsEmpty = defColor.IsEmpty;
			_replRangesR = new byte[l+1]; _replRangesG = new byte[l+1]; _replRangesB = new byte[l+1];
			float[] minH = new float[l], minS = new float[l], minL = new float[l];
			float[] maxH = new float[l], maxS = new float[l], maxL = new float[l];
			for (int i=0; i<l; i++) {
				_replRangesR[i] = repl[i].ReplaceByColor.R;
				_replRangesG[i] = repl[i].ReplaceByColor.G;
				_replRangesB[i] = repl[i].ReplaceByColor.B;
				minH[i] = Math.Min(repl[i].MinH, repl[i].MaxH); maxH[i] = Math.Max(repl[i].MinH, repl[i].MaxH);
				minS[i] = Math.Min(repl[i].MinS, repl[i].MaxS); maxS[i] = Math.Max(repl[i].MinS, repl[i].MaxS);
				minL[i] = Math.Min(repl[i].MinL, repl[i].MaxL); maxL[i] = Math.Max(repl[i].MinL, repl[i].MaxL); }
			_replRangesR[l] = defColor.R; _replRangesG[l] = defColor.G; _replRangesB[l] = defColor.B;
			
			// Si pas déjà fait, créer le tableau avec les valeurs HSL:
			if (_colorHSLValuesH == null)
			{
				Color test;
				_colorHSLValuesH = new float[256,256,256];
				_colorHSLValuesS = new float[256,256,256];
				_colorHSLValuesL = new float[256,256,256];
				for (int R=0; R<256; R++)
				{
					for (int G=0; G<256; G++)
					{
						for (int B=0; B<256; B++)
						{
							test = Color.FromArgb(R, G, B);
							_colorHSLValuesH[R,G,B] = test.GetHue();
							_colorHSLValuesS[R,G,B] = test.GetSaturation();
							_colorHSLValuesL[R,G,B] = test.GetBrightness();
						}
					}
				}
			}
			
			// Crée la matrice:
			_replRangesMatrix = new int[256,256,256];
			bool found; float H, S, L;
			for (int R=0; R<256; R++)
			{
				for (int G=0; G<256; G++)
				{
					for (int B=0; B<256; B++)
					{
						found = false;
						for (int i=0; i<l; i++)
						{
							H = _colorHSLValuesH[R,G,B]; S = _colorHSLValuesS[R,G,B]; L = _colorHSLValuesL[R,G,B];
							if (H >= minH[i] && H <= maxH[i] && S >= minS[i] && S <= maxS[i] && L >= minL[i] && L <= maxL[i])
								{ _replRangesMatrix[R,G,B] = i; found = true; break; }
						}
						if (!found) { _replRangesMatrix[R,G,B] = (defIsEmpty ? -1 : l); }
					}
				}
			}
			
		}
		
		
		/// <summary>
		/// Remplace une plage de couleur par une couleur unique. Les plages de couleurs sont définies par MakeReplaceColorRangesMatrix, il faut donc appeler cette méthode avant tout. L'image d'entrée doit être une image 24bits, retourne null sinon. L'image de sortie est aussi une image 24bits.
		/// </summary>
		public static Bitmap ReplaceColorRanges(Bitmap src)
		{
		
			// Sort si l'image n'est pas du bon type:
			if (Bitmap.GetPixelFormatSize(src.PixelFormat) != 24) { return null; }
			
			// Nouvelle image:
			int width = src.Size.Width, height = src.Size.Height;
			Bitmap dest = new Bitmap(width, height, src.PixelFormat);
			dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
			
			// Bloque les images:
			BitmapData srcData = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, src.PixelFormat);
			BitmapData destData = dest.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
			
			// Code unsafe:
			unsafe
			{
				int index, offset = width % 4;
				// Pointeurs au débuts:
				byte* srcPix = (byte*)(void*)srcData.Scan0;
				byte* destPix = (byte*)(void*)destData.Scan0;
				// Pour chaque ligne:
				for (int y=0; y<height; y++)
				{
					// Pour chaque colonne:
					for (int x=0; x<width; x++)
					{
						// Récupère l'index de la couleur de remplacement dans le tableau:
						index = _replRangesMatrix[srcPix[2], srcPix[1], srcPix[0]];
						// Si index positif, récupère les valeurs de la couleurs:
						if (index != -1) { destPix[0] = _replRangesB[index]; destPix[1] = _replRangesG[index]; destPix[2] = _replRangesR[index]; }
						// Sinon, recopie le pixel d'origine:
						else { destPix[0] = srcPix[0]; destPix[1] = srcPix[1]; destPix[2] = srcPix[2]; }
						// Avance les pixels:
						srcPix += 3; destPix += 3;
					}
					// Offset des images 24bits:
					srcPix += offset; destPix += offset;
				}
			}
      
			// Dégage le BitmapData:
			src.UnlockBits(srcData);
			dest.UnlockBits(destData);
			// Retour:
			return dest;

		}


		/// <summary>
		/// Même chose que ReplaceColorRanges, mais l'image de sortie est une image indexée (sur les couleurs fournit à la méthode MakeReplaceColorRangesMatrix) 8bits.
		/// </summary>
		public static Bitmap ReplaceColorRanges8bitsIndexed(Bitmap src)
		{
		
			// Sort si l'image n'est pas du bon type:
			if (Bitmap.GetPixelFormatSize(src.PixelFormat) != 24) { return null; }
			
			// Nouvelle image:
			int width = src.Size.Width, height = src.Size.Height;
			Bitmap dest = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
			dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
			
			ColorPalette palette = dest.Palette; int l = _replRangesR.Length, m = palette.Entries.Length;
			int defColor = l - 1;
			for (int i=0; i<l; i++) { palette.Entries[i] = Color.FromArgb(_replRangesR[i], _replRangesG[i], _replRangesB[i]); }
			for (int i=l; i<m; i++) { palette.Entries[i] = Color.Black; }
			dest.Palette = palette;
			
			// Bloque les images:
			BitmapData srcData = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, src.PixelFormat);
			BitmapData destData = dest.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
			
			// Code unsafe:
			unsafe
			{
				int index, offset24bits = width % 4, offset8bits = destData.Stride - width;
				// Pointeurs au débuts:
				byte* srcPix = (byte*)(void*)srcData.Scan0;
				byte* destPix = (byte*)(void*)destData.Scan0;
				// Pour chaque ligne:
				for (int y=0; y<height; y++)
				{
					// Pour chaque colonne:
					for (int x=0; x<width; x++)
					{
						// Récupère l'index de la couleur de remplacement dans le tableau:
						index = _replRangesMatrix[srcPix[2], srcPix[1], srcPix[0]];
						// Indique l'index dans le pixel en cours:
						destPix[0] = (byte)(index == -1 ? defColor : index);
						// Avance les pixels:
						srcPix += 3; destPix++;
					}
					// Offset des images 8bits et 24bits:
					srcPix += offset24bits; destPix += offset8bits;
				}
			}
      
			// Dégage le BitmapData:
			src.UnlockBits(srcData);
			dest.UnlockBits(destData);
			// Retour:
			return dest;

		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Converti l'image passée en image 1bit indexé. Converti l'image en noir et blanc par simplement moyenne ((R+G+B)/3), et si le résultat est supérieur à 128, le pixel est "allumé". Pour l'heure, ne sont prise en charge que les images 24bits. Sinon, retourne null.
		/// </summary>
		public static Bitmap ConvertTo1bppIndexed(Bitmap src)
		{
		
			// Sort si l'image n'est pas du bon type:
			if (Bitmap.GetPixelFormatSize(src.PixelFormat) != 24) { return null; }
			
			// Nouvelle image:
			int width = src.Size.Width, height = src.Size.Height;
			Bitmap dest = new Bitmap(width, height, PixelFormat.Format1bppIndexed);
			dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
			
			// Bloque les images:
			BitmapData srcData = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, src.PixelFormat);
			BitmapData destData = dest.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
			
			// Offset pour l'image 1bit:
			int offset1bit = destData.Stride - ((int)Math.Ceiling(width / 8F));
			int offset24bits = width % 4;
			// Ensemble de 8 pixels dans l'image 1bit:
			int eightPix = 0;
			// Flag indiquant si width est divisible par 8 (si ce n'est pas le cas, il faut rajouter en plus le dernier pixel de la dernière
			// colonne de chaque ligne:
			bool addLastCol = (width % 8 != 0);
			// Note: Pour les offset, soit on fait "offset1bit = destData.Stride - (width / 8);" ET "if (addLastCol) { offset1bit--; }"
			// soit, on fait "offset1bit = destData.Stride - (Math.Ceiling(width / 8F));" et rien d'autre.
				
			// Code unsafe:
			unsafe
			{
				// Pointeurs au débuts:
				byte* pix24 = (byte*)(void*)srcData.Scan0;
				byte* pix1 = (byte*)(void*)destData.Scan0;
				// Pour chaque ligne:
				for (int y=0; y<height; y++)
				{
					// Pour chaque colonne:
					for (int x=0; x<width; x++)
					{
						// Si la couleur convertie en gris est supérieur à 128, alors blanc (on allume le pixel), sinon noir:
						if ((pix24[0] + pix24[1] + pix24[2]) / 3 > 128) { eightPix = eightPix | (1 << (7 - (x % 8))); }
						pix24 += 3;
						// Quand on a remplit 8 bits, on transfère pix dans l'image, on avance le pointeur, et on remet à zéro:
						if ((x+1) % 8 == 0) { pix1[0] = (byte)eightPix; pix1++; eightPix = 0; }
					}
					// Si width n'est pas divisible par 8, alors le dernier pixel n'a pas été inscrit auparavent:
					if (addLastCol) { pix1[0] = (byte)eightPix; pix1++; eightPix = 0; }
					// A la fin de la ligne courrante, ajoute l'offset au pointeur 1bit, et au pointeur 24bits:
					pix1 += offset1bit; pix24 += offset24bits;
				}
			}
			
			// Dégage le BitmapData:
			src.UnlockBits(srcData);
			dest.UnlockBits(destData);
			// Retour:
			return dest;
			
		}
		
		
		/// <summary>
		/// Converti une image en image 24bits. L'image src doit être en 32bits, sinon retourne null.
		/// </summary>
		public static Bitmap ConvertTo24bpp(Bitmap src)
		{
		
			// Sort si l'image n'est pas du bon type:
			if (Bitmap.GetPixelFormatSize(src.PixelFormat) != 32) { return null; }

			// Nouvelle image:
			int width = src.Size.Width, height = src.Size.Height;
			Bitmap dest = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
			
			// Bloque les images:
			BitmapData srcData = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, src.PixelFormat);
			BitmapData destData = dest.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			
			// Calcule l'offset pour l'image 24bits (puisqu'il y a des bits à la fin des lignes qui ne sont pas utilisés):
			int offset24 = width % 4;
			// Code unsafe:
			unsafe
			{
				// Pointeurs au débuts:
				byte* pix32 = (byte*)(void*)srcData.Scan0;
				byte* pix24 = (byte*)(void*)destData.Scan0;
				// Pour chaque ligne:
				for (int y=0; y<height; y++)
				{
					// Pour chaque colonne:
					for (int x=0; x<width; x++)
					{
						// Recopie les pixels:
						pix24[0] = pix32[0];
						pix24[1] = pix32[1];
						pix24[2] = pix32[2];
						// Avance le pointeur 24bits de 3 octets, le 32bits de 4:
						pix24 += 3; pix32 += 4;
					}
					// A la fin de la ligne courrante, ajoute l'offset au pointeur 24bits:
					pix24 += offset24;
				}
			}
			// Débloque les images:
			dest.UnlockBits(destData);
			src.UnlockBits(srcData);
			// Retour:
			return dest;

		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Tourne et coupe une image pour supprimer un cache autour de l'image, ce cache étant définie par une couleur entre minColor et maxColor. Retourne donc l'image au milieu du masque. Image 24bits en entrée (sinon retourne null) et en sortie. Voir le fichier annexe pour le détail des opérations et des conditions d'utilisation.
		/// </summary>
		public unsafe static Bitmap ClipAndRotateUsingMask(Bitmap src, Color minColor, Color maxColor)
		{
		
			// Sort si l'image n'est pas du bon type:
			if (Bitmap.GetPixelFormatSize(src.PixelFormat) != 24) { return null; }
			
			// Variables de composantes de couleurs et de tailles:
			byte minR = minColor.R, minG = minColor.G, minB = minColor.B, maxR = maxColor.R, maxG = maxColor.G, maxB = maxColor.B;
			int width = src.Size.Width, height = src.Size.Height;
			
			// Bloque l'image source, définit un pointeur et un offset:
			BitmapData srcData = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, src.PixelFormat);
			byte* srcPix;
			int offset = width % 4;
			// Récupère les deux premiers points verticaux A et B, qui donneront la mesure de l'angle:
			Point Apt = new Point(-1,-1), Bpt = new Point(-1,-1);
			int hLine1 = height / 3, hLine2 = 2 * height / 3;
			for (int i=0; i<2; i++)
			{
				// Début des lignes 1 ou 2:
				srcPix = (byte*)(void*)srcData.Scan0 + (i==0 ? hLine1 : hLine2) * (width * 3 + offset);
				// Parcours la ligne à la recherche d'un pixel dont la couleur n'est pas celle du cache:
				for (int x=0; x<width; x++)
				{
					if (srcPix[0] < minB || srcPix[0] > maxB || srcPix[1] < minG || srcPix[1] > maxG || srcPix[2] < minR || srcPix[2] > maxR) {
						if (i==0) { Apt = new Point(x, hLine1); } else { Bpt = new Point(x, hLine2); }
						break; }
					srcPix+=3;
				}
			}
			// Dégage le BitmapData:
			src.UnlockBits(srcData);
			
			// Nouvelle image dans laquelle sera recopié l'image retournée:
			Bitmap dest = new Bitmap(width, height, src.PixelFormat);
			dest.SetResolution(src.HorizontalResolution, src.VerticalResolution);
			// Créer un Grpahics pour appliquer, éventuellement, une transformation (rotation), puis recopier l'image:
			Graphics g = Graphics.FromImage(dest);
			if (Apt.X > 0 && Bpt.X > 0)
			{
				// Utilise la méthode des vecteurs, en examinant l'ordre des abscisses des points pour trouver l'angle de rotation:
				float xHG = Bpt.X - Apt.X, yHG = Bpt.Y - Apt.Y;
				float cosAngle = yHG / (float)Math.Sqrt(xHG*xHG + yHG*yHG);
				if (cosAngle > 1) { cosAngle = 1; }; if (cosAngle < -1) { cosAngle = -1; }
				float angle = (float)(Math.Acos(cosAngle) * 180 / Math.PI);
				if (Bpt.X < Apt.X) { angle *= -1; }
				g.RotateTransform(angle); }
			// Dessine l'image:
			g.DrawImageUnscaled(src, 0, 0);
			g.Dispose();
			
			// Bloque l'image créer, afin de pouvoir trouver les limites du rectangle de dessin (à l'intérieur du cache):
			BitmapData destData = dest.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, dest.PixelFormat);
			byte* destPix;
			
			// Cadre l'image en cherchant les points d'intersection E, F, G et H, entre les bords intérieurs du cache et les lignes médianes
			// verticales et horizontales de l'image:
			Point Ept = new Point(-1,-1), Fpt = new Point(-1,-1), Gpt = new Point(-1,-1), Hpt = new Point(-1,-1);
			int vLine = width / 2, hLine = height / 2, twoHalfLine = (width - vLine) * 3 + offset + vLine * 3;
			// Point E:
			destPix = (byte*)(void*)destData.Scan0 + vLine * 3;
			for (int y=0; y<height; y++)
			{
				if (destPix[0] < minB || destPix[0] > maxB || destPix[1] < minG || destPix[1] > maxG || destPix[2] < minR || destPix[2] > maxR)
					{ Ept = new Point(vLine, y); break; }
				destPix += twoHalfLine;
			}
			// Point F:
			destPix = (byte*)(void*)destData.Scan0 + vLine * 3 + (height - 1) * (width * 3 + offset);
			for (int y=height-1; y>=0; y--)
			{
				if (destPix[0] < minB || destPix[0] > maxB || destPix[1] < minG || destPix[1] > maxG || destPix[2] < minR || destPix[2] > maxR)
					{ Fpt = new Point(vLine, y); break; }
				destPix -= twoHalfLine;
			}
			// Point G:
			destPix = (byte*)(void*)destData.Scan0 + (hLine - 1) * (width * 3 + offset);
			for (int x=0; x<width; x++)
			{
				if (destPix[0] < minB || destPix[0] > maxB || destPix[1] < minG || destPix[1] > maxG || destPix[2] < minR || destPix[2] > maxR)
					{ Gpt = new Point(x, hLine); break; }
				destPix += 3;
			}
			// Point H:
			destPix = (byte*)(void*)destData.Scan0 + width * 3 + (hLine - 1) * (width * 3 + offset);
			for (int x=width-1; x>=0; x--)
			{
				if (destPix[0] < minB || destPix[0] > maxB || destPix[1] < minG || destPix[1] > maxG || destPix[2] < minR || destPix[2] > maxR)
					{ Hpt = new Point(x, hLine); break; }
				destPix -= 3;
			}
			// Dégage le BitmapData:
			dest.UnlockBits(destData);
			
			// Si tous les points ont été trouvés et si les dimensions du rectangle sont positives, dessine une nouvelle image dest2, puis
			// la retourne:
			Bitmap dest2; int rectHeight, rectWidth;
			if (Ept.X != 1 && Fpt.X != 1 && Gpt.X != 1 && Hpt.X != 1 && (rectHeight = Fpt.Y - Ept.Y + 1) > 0 && (rectWidth = Hpt.X - Gpt.X + 1) > 0)
			{
				dest2 = new Bitmap(rectWidth, rectHeight, src.PixelFormat);
				dest2.SetResolution(src.HorizontalResolution, src.VerticalResolution);
				g = Graphics.FromImage(dest2);
				g.DrawImage(dest, 0, 0, new Rectangle(Gpt.X, Ept.Y, rectWidth, rectHeight), GraphicsUnit.Pixel);
				g.Dispose(); dest.Dispose();
				return dest2;
			}
			// Sinon, retourne simplement l'image tournée précédemment:
			return dest;
			
		}


	}
	
	
	
	
}
