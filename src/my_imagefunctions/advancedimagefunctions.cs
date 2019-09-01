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
	/// Fournit des fonctions d'images avancées.
	/// </summary>
	public unsafe static class AdvancedImageFunctions
	{
	


		private static int[][,] _dblResPosArray; // Contient les positions des pointeurs pour chacune des 4 images
		private static int _dblResPosArrayWidth, _dblResPosArrayHeight;
		private static byte[] _dblResData; // Contient les données de la nouvelle image
		
		/// <summary>
		/// Remplit un tableau de 4 sous-tableau contenant pour chaque coordonnées (x,y) la position du pointeur, en tenant compte d'un offset.
		/// </summary>
		private static void FillAdvancedStillImagePosArray(int width, int height, int offset)
		{
			_dblResPosArray = new int[4][,];
			for (int i=0; i<4; i++)
			{
				_dblResPosArray[i] = new int[width,height];
				int yPos = (i==2 || i==3 ? 1 : 0);
				int xPos = (i==1 || i==3 ? 1 : 0);
				for (int y=0; y<height; y++)
				{
					for (int x=0; x<width; x++)
						{ _dblResPosArray[i][x,y] = (2*y+yPos)*3*(width+offset)*2 + (2*x+xPos)*3; }
				}
			}
			_dblResPosArrayWidth = width;
			_dblResPosArrayHeight = height;
		}
	
		
		public static IntPtr DoubleResolution(int width, int height, int stride, int counter, int avgCoeff, byte* ptrSrc)
		{
			
			// Refait le tableau des positions du pointeur, au besoin:
			int offset = stride - width*3;
			if (_dblResPosArray == null || _dblResPosArrayWidth != width || _dblResPosArrayHeight != height)
				{ FillAdvancedStillImagePosArray(width, height, offset); }
			
			// Si premier passage, créer le tableau:
			if (counter == 0)	{_dblResData = new byte[width*height*2*2*3]; }
			
			// Lors des différentes passages, ajoute l'image en cours:
			if (counter <= 3)
			{
				// Pointeur de la nouvelle image:
				fixed (byte* ptrNew = &_dblResData[0])
				{
					// Pour chaque ligne et chaque colonne:
					for (int y=0; y<height; y++)
					{
						for (int x=0; x<width; x++)
						{
							// Copie les données:
							ptrNew[_dblResPosArray[counter][x,y]] = ptrSrc[0];
							ptrNew[_dblResPosArray[counter][x,y]+1] = ptrSrc[1];
							ptrNew[_dblResPosArray[counter][x,y]+2] = ptrSrc[2];
							// Avance le pointeur source:
							ptrSrc += 3;
						}
						ptrSrc += offset;
					}
				}
			}
			
			// Lors du 4e passage, modifie par moyenne les images pour atténuer les contours:
			if (counter == 3)
			{
				// Si avgCoeff vaut 0, on ne modifie rien, sinon, calcul pour les points 1, 2 et 3
				// les moyennes des pixels alentours (voir aide):
				if (avgCoeff != 0)
				{
					int c = avgCoeff, d = 3 + c;
					int[][,] pos = _dblResPosArray;
					fixed (byte* ptr = &_dblResData[0])
					{
						for (int y=0; y<height-1; y++)
						{
							for (int x=0; x<width-1; x++)
							{
								ptr[pos[2][x,y]] = (byte)((c*ptr[pos[2][x,y]] + ptr[pos[3][x,y]] + ptr[pos[1][x,y+1]] + ptr[pos[0][x,y+1]])/d);
								ptr[pos[2][x,y]+1] = (byte)((c*ptr[pos[2][x,y]+1] + ptr[pos[3][x,y]+1] + ptr[pos[1][x,y+1]+1] + ptr[pos[0][x,y+1]+1])/d);
								ptr[pos[2][x,y]+2] = (byte)((c*ptr[pos[2][x,y]+2] + ptr[pos[3][x,y]+2] + ptr[pos[1][x,y+1]+2] + ptr[pos[0][x,y+1]+2])/d);
							}
						}
						for (int y=0; y<height-1; y++)
						{
							for (int x=0; x<width-1; x++)
							{
								ptr[pos[1][x,y]] = (byte)((c*ptr[pos[1][x,y]] + ptr[pos[0][x+1,y]] + ptr[pos[2][x+1,y]] + ptr[pos[3][x,y]])/d);
								ptr[pos[1][x,y]+1] = (byte)((c*ptr[pos[1][x,y]+1] + ptr[pos[0][x+1,y]+1] + ptr[pos[2][x+1,y]+1] + ptr[pos[3][x,y]+1])/d);
								ptr[pos[1][x,y]+2] = (byte)((c*ptr[pos[1][x,y]+2] + ptr[pos[0][x+1,y]+2] + ptr[pos[2][x+1,y]+2] + ptr[pos[3][x,y]+2])/d);
							}
						}
						for (int y=0; y<height-1; y++)
						{
							for (int x=0; x<width-1; x++)
							{
								ptr[pos[3][x,y]] = (byte)((c*ptr[pos[3][x,y]] + ptr[pos[2][x+1,y]] + ptr[pos[0][x+1,y+1]] + ptr[pos[1][x,y+1]])/d);
								ptr[pos[3][x,y]+1] = (byte)((c*ptr[pos[3][x,y]+1] + ptr[pos[2][x+1,y]+1] + ptr[pos[0][x+1,y+1]+1] + ptr[pos[1][x,y+1]+1])/d);
								ptr[pos[3][x,y]+2] = (byte)((c*ptr[pos[3][x,y]+2] + ptr[pos[2][x+1,y]+2] + ptr[pos[0][x+1,y+1]+2] + ptr[pos[1][x,y+1]+2])/d);
							}
						}
					}
				}
				// Toujours lors du 4e passage, retourne le pointeur final:
				int address = 0;
				fixed (byte* ptr = &_dblResData[0]) { address = (int)ptr; }
				return new IntPtr(address);
			}
		
			// Si pas encore 4e passage, retourne un pointeur vide:
			return IntPtr.Zero;
		
		}
		
		
		public static void DoubleResolution(bool deleteDataArray, bool deletePointerPosArray)
		{
			if (deleteDataArray) { _dblResData = null; }
			if (deletePointerPosArray) { _dblResPosArray = null; _dblResPosArrayWidth = _dblResPosArrayHeight = 0; }
		}
		
		
		
		
		
		public static void DrawRectangleOnPicture(int pictW, int pictH, int stride, Rectangle rect, byte r, byte g, byte b, byte* ptrSrc)
		{
			// Variables:
			int w = rect.Width, h = rect.Height;
			int offset = stride - pictW*3;
			byte* ptr;
			// Ligne du haut:
			ptr = ptrSrc + rect.Y * (3*pictW+offset) + rect.X * 3;
			for (int i=0; i<w; i++, ptr+=3)
				{ ptr[0] = b; ptr[1] = g; ptr[2] = r; }
			// Ligne du bas:
			ptr = ptrSrc + (rect.Y + h - 1) * (3*pictW+offset) + rect.X * 3;
			for (int i=0; i<w; i++, ptr+=3)
				{ ptr[0] = b; ptr[1] = g; ptr[2] = r; }
			// Ligne verticale de gauche:
			int bitsWidth = pictW * 3 + offset;
			ptr = ptrSrc + rect.Y * (3*pictW+offset) + rect.X * 3;
			for (int i=0; i<h; i++, ptr+=bitsWidth)
				{ ptr[0] = b; ptr[1] = g; ptr[2] = r; }
			// Ligne verticale de droite:
			ptr = ptrSrc + rect.Y * (3*pictW+offset) + (rect.X + w - 1) * 3;
			for (int i=0; i<h; i++, ptr+=bitsWidth)
				{ ptr[0] = b; ptr[1] = g; ptr[2] = r; }
		}
	
	
	}
	
	
}
