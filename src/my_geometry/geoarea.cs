using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Drawing.Imaging;

namespace My
{







	public partial class GeoArea
	{
	
	
	
	
	


		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS




		// Contrôles:
		private My.ExdListView _extListView;
		
		// Repère de l'espace:
		//UNDONE:private SpPoint _O;
		//UNDONE:private SpPoint _X;
		//UNDONE:private SpPoint _Y;
		//UNDONE:private SpPoint _Z;
		
		// Points sur le plan du form:
		private Point _XPointOnWin;
		private Point _YPointOnWin;
		private Point _ZPointOnWin;
		
		// Valeurs min et max de X et Y dans le repère du form:
		private int _XWin_min;
		private int _XWin_max;
		private int _YWin_min;
		private int _YWin_max;
		private int _XCenterForm;
		private int _YCenterForm;
		
		// Point d'origine du plan du form (origine pour les méthodes de C#):
		private Point _OriginOnWin;
		
		// Tableau de tous les objets, dans l'ordre de création:
		private SpObject[] _allObjects;
		// Tableau de tous les objets, dans l'ordre d'affichage. Contient les index de _allObjetcs:
		private int[] _orderedObjects;
		// Liste des objets sélectionnés:
		//UNDONE:private int[] _selectedIndexes;
		

		// Variables pour les propriétés (à n'utiliser qu'en lecture dans le code):
		// AutoDraw:
		private bool __AutoDraw;
		// Angles d'Euler:
		private double __Phi;
		private double __Theta;
		// Norm et Scale:
		private float __XNorm;
		private float __YNorm;
		private float __ZNorm;
		private int __XScale;
		private int __YScale;
		private int __Zoom;
		// Affichage:
		private bool __ShowCoordinateSystem;
		private bool __DrawHighQuality;
		private Rectangle __ClipRect;
		private bool __ShowClipRect;
		private int __MaxAxes;
		private bool __ShowGraduations;
		private bool __ShowXYGrid;
		private bool __ShowXZGrid;
		private bool __ShowYZGrid;
		// Couleur du repère, des axes et des grilles:
		private Color __XAxisColor;
		private Color __YAxisColor;
		private Color __ZAxisColor;
		
		// Nom de cet assemblage:
		private string _assemblyName;
		
		// Cap de ligne personnalisés:
		private CustomLineCap _customArrowCap;
		private CustomLineCap _customGraduationCap;
		
		// Graphics et BMP pour l'événément Paint (ce qui évite de recalculer à chaque fois que Paint est appelé):
		private Graphics _graphics;
		private Bitmap _bmpForPaint;
		
		// Objets Pen et Brush:
		private Pen _XCoordSystemPen;
		private Pen _YCoordSystemPen;
		private Pen _ZCoordSystemPen;
		private Pen _XAxisPen;
		private Pen _YAxisPen;
		private Pen _ZAxisPen;
		private Pen _XGridPen;
		private Pen _YGridPen;
		private Pen _ZGridPen;
		private Brush _XGraduationsBrush;
		private Brush _YGraduationsBrush;
		private Brush _ZGraduationsBrush;
		private Font _graduationsFont;
		//UNDONE:private Pen[] _objPens;
		//UNDONE:private Brush[] _objBrushes;
		//UNDONE:private SolidBrush[] _objLabelBrushes;


		/// <summary>
		/// Classe d'événement pour l'affichage des informations et des erreurs. Action fournit la description de l'information et IsError indique s'il s'agit d'une erreur (toujours false si Infos est déclenché, toujours true si Error est déclenché).
		/// </summary>
		public class InfosEventArgs : EventArgs
		{
			public string Action { get; set; }
			public bool IsError { get; set; }
			public InfosEventArgs(string action, bool isError) { Action = action; IsError = isError; }
		}
		
		
		/// <summary>
		/// Classe d'événément qui fournit toutes les données pour que les objets calculent leur position sur le form:
		/// </summary>
		public class CalculationDataEnventArgs : EventArgs
		{
			// Points sur le plan du form:
			private Point _XPointOnWin;
			private Point _YPointOnWin;
			private Point _ZPointOnWin;
			// Valeurs min et max de X et Y dans le repère du form:
			private int _XWin_min;
			private int _XWin_max;
			private int _YWin_min;
			private int _YWin_max;
			private int _XCenterForm;
			private int _YCenterForm;
			// Point d'origine du plan du form (origine pour les méthodes de C#):
			private Point _OriginOnWin;
			public CalculationDataEnventArgs() { ; }
		}
		
		
		// Délégués d'évenements:
		public delegate void InfosEventHandler(object sender, InfosEventArgs e);
		public delegate void CalculationDataEventHandler(object sender, CalculationDataEnventArgs e);
		
		/// <summary>
		/// Se déclenche lorsqu'une information est fournit par le programme.
		/// </summary>
		public event InfosEventHandler Infos;
		
		/// <summary>
		/// Se déclenche lorsqu'une erreur est rencontrée.
		/// </summary>
		public event InfosEventHandler Error;
		
		




		#endregion DECLARATIONS










		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS



		#endregion CONSTRUCTEURS








		// ---------------------------------------------------------------------------
		// METHODES STATIQUES
		// ---------------------------------------------------------------------------




		#region METHODES STATIQUES



		#endregion METHODES STATIQUES













		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES



		#endregion METHODES PUBLIQUES











		// ---------------------------------------------------------------------------
		// METHODES PRIVEES
		// ---------------------------------------------------------------------------




		#region METHODES PRIVEES



		#endregion METHODES PRIVEES
	





	}





}