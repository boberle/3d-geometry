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



	/// <summary>
	/// Fournit des méthodes pour dessiner sur un contrôle de la géométrie de l'espace.
	/// </summary>
	public partial class DrawingArea
	{






		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS




		/// <summary>
		/// Type de calcul à refaire sur les coordonnées 2D des objets.
		/// </summary>
		protected enum CalcUpdMode
		{
			Calc3D,
			Calc2D
		}		

		
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
		
		// Collection d'objets:
		private SpObjectsCollection _spObjects;
		

		// Variables pour les propriétés:
		// AutoDraw:
		private bool __AutoDraw;
		private bool __SuspendCalculation;
		private bool __ShowDrawingMessages;
		// Angles d'Euler:
		private double __Phi;
		private double __Theta;
		private double __Rotation;
		// Norm et Scale:
		private float __XNorm;
		private float __YNorm;
		private float __ZNorm;
		private float __Scale;
		private float __DraftScale;
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
		// Couleur et taille du repère, des axes et des grilles:
		private Color __XAxisColor;
		private Color __YAxisColor;
		private Color __ZAxisColor;
		private Font __GraduationsFont;
		private float __AxisWidth;
		private float __GridWidth;
		private float __CoordinateSystemWidth;
		
		// Nom de cet assemblage:
		private string _assemblyName;
		
		// Graphics:
		protected bool _resizeGraphics;
		protected int _graphHeight, _graphWidth;
		protected Bitmap _bmp32, _bmp24;
		protected BitmapData _bmp32Data, _bmp24Data;
		protected Graphics _bmpDraftGraph, _bmpHighGraph, _screenGraph;
		protected GraphicsPath _gPath;
		protected Rectangle _drawingRect;
		protected byte[] _rgbValues32, _rgbValues24;
		
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
		
		// Le contrôle pour le dessin:
		Control _controlToDraw;
		


		#endregion DECLARATIONS








		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS ET METHODES STATIQUES
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS ET METHODES STATIQUES



		// Cap de ligne personnalisés:
		public static CustomLineCap CustomArrowCap;
		public static CustomLineCap CustomGraduationCap;
		

		/// <summary>
		/// Classe du repère de l'espace.
		/// </summary>
		private static class _OXYZ
		{
			public static class OPoint
				{ public const int X = 0; public const int Y = 0; public const int Z = 0; }
			public static class XPoint
				{ public const int X = 1; public const int Y = 0; public const int Z = 0; }
			public static class YPoint
				{ public const int X = 0; public const int Y = 1; public const int Z = 0; }
			public static class ZPoint
				{ public const int X = 0; public const int Y = 0; public const int Z = 1; }
		}
		

		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Création des Caps de lignes personnalisés.
		/// </summary>
		static DrawingArea()
		{
		
			// Création du Cap des lignes en forme de flèche (la flèche par défaut est beaucoup trop petite):
			
			/*// J'ignore comment remplit le polygone, donc on le remplit manuellement, en traçant des points à l'intérérieur:
			GraphicsPath gPath = new GraphicsPath();
			gPath.AddLine(new Point(-3,-5), new Point(3,-5)); gPath.CloseFigure();
			gPath.AddLine(new Point(-2,-4), new Point(2,-4)); gPath.CloseFigure();
			gPath.AddLine(new Point(-2,-3), new Point(2,-3)); gPath.CloseFigure();
			gPath.AddLine(new Point(-1,-2), new Point(1,-2)); gPath.CloseFigure();
			gPath.AddLine(new Point(-1,-1), new Point(1,-1)); gPath.CloseFigure();
			// Puis on rajoute les lignes extérieurs du polygone:
			Point[] points = new Point[]{new Point(0,-5), new Point(-3,-5), new Point(0,0), new Point(3,-5), new Point(0,-5)};
			gPath.AddPolygon(points); gPath.CloseFigure();
			DrawingArea.CustomArrowCap = new CustomLineCap(null, gPath);
			gPath.Dispose();*/
			DrawingArea.CustomArrowCap = new AdjustableArrowCap(9, 8, true);
			// Création du cap pour les graduations:
			GraphicsPath gPath = new GraphicsPath();
			gPath.AddLine(new Point(-2,0), new Point(2,0));
			DrawingArea.CustomGraduationCap = new CustomLineCap(null, gPath);
			gPath.Dispose();
			
		}
		


		#endregion CONSTRUCTEURS ET METHODES STATIQUES







		// ---------------------------------------------------------------------------
		// CONSTRUCTEUR
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEUR
		
		
		
		/// <summary>
		/// Constructeur du DrawingArea. Il faut passer en paramètre le contrôle sur lequel on doit dessiner.
		/// </summary>
		public DrawingArea(Control controlToDraw)
		{
		
			// Initialisation des variables:
			_assemblyName = Assembly.GetExecutingAssembly().FullName;
			_resizeGraphics = true;
			
			// Configuration du contrôle:
			_controlToDraw = controlToDraw;
			_graphWidth = _controlToDraw.ClientSize.Width;
			_graphHeight = _controlToDraw.ClientSize.Height;
			_controlToDraw.BackColor = Color.White;
			_controlToDraw.Paint += new PaintEventHandler(_controlToDraw_Paint);
			_controlToDraw.SizeChanged += new EventHandler(_controlToDraw_SizeChanged);
			_controlToDraw.FindForm().ResizeEnd += new EventHandler(DrawingAreaForm_ResizeEnd);
			_controlToDraw.FindForm().ResizeBegin += new EventHandler(DrawingAreaForm_ResizeBegin);
			// Active le double-tampon, ce qui évite l'utilisation manuelle d'une classe BufferedGraphics.
			PropertyInfo prop = _controlToDraw.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance| BindingFlags.NonPublic);
			prop.SetValue(controlToDraw, true, null);
			/* // Vérification de l'activation du double-tampon : il faut que OptimizedDoubleBuffer et AllPaintingInWmPaint valent true:
			string msg = String.Empty; Array st = Enum.GetValues(typeof(ControlStyles));
			MethodInfo mi = _controlToDraw.GetType().GetMethod("GetStyle", BindingFlags.Instance| BindingFlags.NonPublic);
			foreach (ControlStyles i in st) { msg += i.ToString() + " : " + mi.Invoke(_controlToDraw, new object[]{i}).ToString() + "\n"; }
			MessageBox.Show(msg); MessageBox.Show(prop.GetValue(_controlToDraw, null).ToString()); */
			
			// Obtention de la collection, initialisation et distribution des méthodes et propriétés et événements:
			this.SpaceObjects = SpObjectsCollection.GetInstance();
			Formula.GeneralUsedFunctions = GeoMethodsForFormulas.MethodsForFormulas
						.Concat(typeof(GeoFunctions).GetMethods(BindingFlags.Static | BindingFlags.Public)).ToArray();
			_spObjects.RequestDrawing += delegate { if (__AutoDraw) { this.Draw(); } };
			_spObjects.RequestDrawingCalc += delegate(object sender, RequestDrawingCalcEventArgs e)
				{ this.CalculateObjectDrawingData(e.ObjectChanged, CalcUpdMode.Calc3D); };
			
			// Valeurs par défaut;
			SetDefaultProperties();
			
		}
		
		
		/* 
		 * Note sur la possibilité de sélectionner un objet par clic sur le DrawingArea.
		 * -----------------------------------------------------------------------------
		 * Dans le gestionnaire d'événement de MouseClick de _controlToDraw, il faut
		 * reprendre la procédure de dessin, mais plutôt que de dessiner sur le Graphics,
		 * il faut dessiner dans un path, puis tester les méthodes IsOutlineVisible (pour un
		 * objet avec seulement un contour) et IsVisible (pour un objet rempli, où le clic
		 * peut être à l'intérieur de l'objet, comme une face ou un disque). Il faut simplement
		 * songer à la possibilité d'objets superposés, et proposer une boîte de dialogue pour
		 * savoir lequel il faut sélectionner.
		 * Exemple:
		 * private void _controlToDraw_MouseClick(object sender, MouseEventArgs e)
		 * {
		 *		PointF loc = new PointF((e.X - _OriginOnWin.X) / __Scale, (e.Y - _OriginOnWin.Y) / __Scale);
		 *		GraphicsPath p = new GraphicsPath();
		 *		foreach (SpObject o in _spObjects)
		 *		{
		 *			p.Reset();
		 *			// Passe au suivant si l'objet est masqué, ou si l'objet n'est pas défini:
		 *			if (o.Hidden || o.IsUndefined) { continue; }
		 *			if (o is SpPointObject)
		 *			{
		 *				SpPointObject pt = o as SpPointObject;
		 *				p.AddEllipse(pt.PtOnWin.X - 2, pt.PtOnWin.Y - 2, 4, 4);
		 *				if (p.IsVisible(loc)) { ... doing something... }
		 *			}
		*/
		
		
		/// <summary>
		/// Met les propriétés à leur valeur par défaut.
		/// </summary>
		public void SetDefaultProperties()
		{
			this.SuspendCalculation = true;
			this.AutoDraw = false;
			this.ShowDrawingMessages = true;
			this.ShowCoordinateSystem = true;
			this.DrawHighQuality = true;
			this.ShowClipRect = false;
			this.ClipRect = new Rectangle(-10, -10, 20, 20);
			this.Phi = 292.99 * Math.PI / 180;
			this.Theta = 378.77 * Math.PI / 180;
			this.Rotation = 0;
			this.XNorm = 1;
			this.YNorm = 1;
			this.ZNorm = 1;
			this.Scale = 2;
			this.DraftScale = My.Geometry.MySettings.DefaultDraftScale;
			this.Zoom = My.Geometry.MySettings.DefaultZoom;
			this.OriginOnWindow = default(Point);
			this.ShowAxes = 0;
			__XAxisColor = Color.FromArgb(100, Color.Red);
			__YAxisColor = Color.FromArgb(100, Color.Green);
			__ZAxisColor = Color.FromArgb(100, Color.Blue);
			__AxisWidth = 0.6F;
			__GridWidth = 0.25F;
			__CoordinateSystemWidth = 1F;
			__GraduationsFont = new Font("Arial", 8F);
			MakeCommenPens();
			this.ShowGraduations = true;
			this.ShowXYGrid = false;
			this.ShowXZGrid = false;
			this.ShowYZGrid = false;
			__SuspendCalculation = false;
			this.AutoDraw = true;
			GeoMsgSender.ShowInfos = true;
			GeoMsgSender.ShowErrors = true;
		}



		#endregion CONSTRUCTEUR

	





		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES




		/// <summary>
		/// Translate l'origine du repère dans le repère du form. Les valeurs sont en pixels. Relance un calcul 2D.
		/// </summary>
		public void TranslateOrigin(int x, int y)
		{
			_OriginOnWin = new Point(_OriginOnWin.X + x, _OriginOnWin.Y + y * -1);
			CalculateCoordSystemDrawingData();
			CalculateAllObjectsDrawingData(CalcUpdMode.Calc2D);
			SendInfos(String.Format("New origin: ", _OriginOnWin.X, _OriginOnWin.Y));
			DrawAuto(Assembly.GetCallingAssembly());
		}
		
		
		/// <summary>
		/// Centre l'origine du repère au milieu du form. Relance un calcul 2D.
		/// </summary>
		public void CenterOrigin()
		{
			_OriginOnWin = new Point(_graphWidth /2, _graphHeight / 2);
			SendInfos(String.Format("New origin: ({0},{1})", _OriginOnWin.X, _OriginOnWin.Y));
			CalculateCoordSystemDrawingData();
			CalculateAllObjectsDrawingData(CalcUpdMode.Calc2D);
			DrawAuto(Assembly.GetCallingAssembly());
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Créer les points X, Y et Z, et éventuellement O.
		/// </summary>
		public void CreateSystemPoints(bool createO)
		{
			SpPoint spt;
			if (createO)
			{
				spt = (SpPoint)this._spObjects.Add(new SpPoint("O", new DoubleF(), new DoubleF(), new DoubleF()));
				byte alpha = (byte)((__XAxisColor.A + __YAxisColor.A + __ZAxisColor.A) / 3);
				spt.Color = Color.FromArgb(alpha, Color.Black);
			}
			spt = (SpPoint)this._spObjects.Add(new SpPoint("X", new DoubleF(1), new DoubleF(0), new DoubleF(0)));
			spt.Color = __XAxisColor;
			spt = (SpPoint)this._spObjects.Add(new SpPoint("Y", new DoubleF(0), new DoubleF(1), new DoubleF(0)));
			spt.Color = __YAxisColor;
			spt = (SpPoint)this._spObjects.Add(new SpPoint("Z", new DoubleF(0), new DoubleF(0), new DoubleF(1)));
			spt.Color = __ZAxisColor;
			DrawAuto(Assembly.GetCallingAssembly());
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Règle les angles d'Euleur pour avoir des vues du haut, du bas, etc.
		/// </summary>
		public void ChangeView(ViewType v)
		{
			double π = Math.PI;
			switch (v)
			{
				case ViewType.Top:		__Phi = π/2;			__Theta = π/2;			break;
				case ViewType.Bottom:	__Phi = 3*π/2;		__Theta = 3*π/2;		break;
				case ViewType.Front:	__Phi = π/2;			__Theta = 0;				break;
				case ViewType.Back:		__Phi = 3*π/2;		__Theta = 0;				break;
				case ViewType.Left:		__Phi = π;				__Theta = 0;				break;
				case ViewType.Right:	__Phi = 0;				__Theta = 0;				break;
				case ViewType.Def:		__Phi = 292.99*π/180;		__Theta = 378.77*π/180;		break;
			}
			__Rotation = 0;
			RecalculateAll();
			DrawAuto(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Règle les angles d'Euleur pour que le plan passé en argument soit parallèle à l'écran:
		/// </summary>
		public void ChangeView(SpPlaneObject plane)
		{
			// On cherche le projeté orthogonal du point O (origine du repère de l'espace)  sur le plan.
			// Si ce point n'est pas confondu avec O, alors on cherche ses coordonnées polaires, et on les applique au repère:
			if (plane.IsUndefined) { SendError("Plane is undefined."); return; }
			Coord3D proj = GeoFunctions.GetOrthoProjPointOnPlaneCoords(new Coord3D(), plane.CartesianEq, plane.NormalVectorCoords);
			// Si O et O' sont confondus, on recommence avec un plan parallèle, mais n'incluant pas O:
			if (proj.IsNul) {
				Eq3Zero newPlane = plane.CartesianEq;
				newPlane.d++;
				proj = GeoFunctions.GetOrthoProjPointOnPlaneCoords(new Coord3D(), newPlane, plane.NormalVectorCoords); }
			double r, λ, φ;
			MathsGeo.ToGeographic(proj.X, proj.Y, proj.Z, true, 0, out λ, out φ, out r);
			__Phi = -λ; __Theta = φ;
			__Rotation = 0;
			RecalculateAll();
			DrawAuto(Assembly.GetCallingAssembly());
		}


		/// <summary>
		/// Règle les angles d'Euleur pour que le plan passé en argument soit parallèle à l'écran:
		/// </summary>
		public void ChangeView(SpPointObject pt1, SpPointObject pt2, SpPointObject pt3)
		{
			// On cherche le projeté orthogonal du point O (origine du repère de l'espace)  sur le plan.
			// Si ce point n'est pas confondu avec O, alors on cherche ses coordonnées polaires, et on les applique au repère:
			if (pt1.IsUndefined || pt2.IsUndefined || pt3.IsUndefined) { SendError("A point is undefined."); return; }
			Eq3Zero planeEq = GeoFunctions.GetPlaneCartesianEquation(pt1.Coordinates, pt2.Coordinates - pt1.Coordinates,
				pt3.Coordinates - pt1.Coordinates);
			if (planeEq.Empty) { SendError("Points are aligned."); return; }
			Coord3D normalVec = GeoFunctions.GetNormalVectorToPlane(pt2.Coordinates - pt1.Coordinates, pt3.Coordinates - pt1.Coordinates);
			Coord3D proj = GeoFunctions.GetOrthoProjPointOnPlaneCoords(new Coord3D(), planeEq, normalVec);
			// Si O et O' sont confondus, on recommence avec un plan parallèle, mais n'incluant pas O:
			if (proj.IsNul) {
				Eq3Zero newPlane = planeEq;
				newPlane.d++;
				proj = GeoFunctions.GetOrthoProjPointOnPlaneCoords(new Coord3D(), newPlane, normalVec); }
			double r, λ, φ;
			MathsGeo.ToGeographic(proj.X, proj.Y, proj.Z, true, 0, out λ, out φ, out r);
			__Phi = -λ; __Theta = φ;
			__Rotation = 0;
			RecalculateAll();
			DrawAuto(Assembly.GetCallingAssembly());
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Recalcule toutes les données pour l'affichage des objets de l'espace sur le plan du form (mais les calculs 3D propres aux objets ne sont pas recalculés).
		/// </summary>
		public void RecalculateAll()
		{
			CalculateCoordSystemDrawingData();
			CalculateAllObjectsDrawingData(CalcUpdMode.Calc3D);
			DrawAuto(Assembly.GetCallingAssembly());
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne les informations sur les propriétés du DrawingArea.
		/// </summary>
		public string GetInfos()
		{
			StringBuilder sb = new StringBuilder(); string strVal; object val;
			PropertyInfo[] pis = typeof(DrawingArea).GetProperties();
			foreach (PropertyInfo pi in pis)
			{
				if (pi.Name == "SpaceObjects") { continue; }
				val = pi.GetValue(this, null);
				if (val is Color) { strVal = My.ColorFunctions.GetColorDescription((Color)val, ":"); }
				else if (val is Font) { strVal = My.GeneralParser.GetFontDescription((Font)val, ":"); }
				else if (val is Point) { strVal = String.Format("({0},{1})", ((Point)val).X, ((Point)val).Y); }
				else if (val is Rectangle) { strVal = String.Format("{0}*{1} at ({2},{3})", ((Rectangle)val).Width, ((Rectangle)val).Height,
						((Rectangle)val).X, ((Rectangle)val).Y); }
				else { strVal = val.ToString(); }
				sb.AppendFormat("{0}: {1}\n", pi.Name, strVal);
			}
			sb.AppendFormat("Phi: {0}\n", MathsGeo.GetAngleBounds(__Phi, 12, false, "φ"));
			sb.AppendFormat("Theta: {0}\n", MathsGeo.GetAngleBounds(__Theta, 12, false, "θ"));
			sb.AppendFormat("Rotation: {0}\n", MathsGeo.GetAngleBounds(__Rotation, 12, false, "rot"));
			return sb.ToString(0, sb.Length - 1);
		}
		
		
		
		
		#endregion METHODES PUBLIQUES






		// ---------------------------------------------------------------------------
		// METHODES PRIVEES
		// ---------------------------------------------------------------------------




		#region METHODES PRIVEES




		/// <summary>
		/// Envoie un message d'information.
		/// </summary>
		protected void SendInfos(string msg)
			{ GeoMsgSender.SendInfos(this, msg); }


		/// <summary>
		/// Envoie un message d'erreur.
		/// </summary>
		protected void SendError(string msg)
			{ GeoMsgSender.SendError(this, msg); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Refait les pens pour le repère, les axes, la grille.
		/// </summary>
		private void MakeCommenPens()
		{
			// Repère:
			_XCoordSystemPen = new Pen(__XAxisColor, __CoordinateSystemWidth);
			_XCoordSystemPen.CustomEndCap = DrawingArea.CustomArrowCap;
			_YCoordSystemPen = new Pen(__YAxisColor, __CoordinateSystemWidth);
			_YCoordSystemPen.CustomEndCap = DrawingArea.CustomArrowCap;
			_ZCoordSystemPen = new Pen(__ZAxisColor, __CoordinateSystemWidth);
			_ZCoordSystemPen.CustomEndCap = DrawingArea.CustomArrowCap;
			// Axes:
			_XAxisPen = new Pen(__XAxisColor, __AxisWidth);
			_XAxisPen.CustomEndCap = _XAxisPen.CustomStartCap = DrawingArea.CustomGraduationCap;
			_YAxisPen = new Pen(__YAxisColor, __AxisWidth);
			_YAxisPen.CustomEndCap = _YAxisPen.CustomStartCap = DrawingArea.CustomGraduationCap;
			_ZAxisPen = new Pen(__ZAxisColor, __AxisWidth);
			_ZAxisPen.CustomEndCap = _ZAxisPen.CustomStartCap = DrawingArea.CustomGraduationCap;
			// Grille:
			_XGridPen = new Pen(__XAxisColor, __GridWidth);
			_YGridPen = new Pen(__YAxisColor, __GridWidth);
			_ZGridPen = new Pen(__ZAxisColor, __GridWidth);
			// Graduations:
			_XGraduationsBrush = new SolidBrush(__XAxisColor);
			_YGraduationsBrush = new SolidBrush(__YAxisColor);
			_ZGraduationsBrush = new SolidBrush(__ZAxisColor);
		}


		#endregion METHODES PRIVEES






		// ---------------------------------------------------------------------------
		// GESTIONNAIRES D'EVENEMENTS
		// ---------------------------------------------------------------------------




		#region GESTIONNAIRES D'EVENEMENTS



		
		/// <summary>
		/// Redessine le contrôle. Si c'est la première fois, lance Draw. Sinon, utilise _bmp24.
		/// </summary>
		private void _controlToDraw_Paint(object sender, PaintEventArgs e)
		{
			// Si taille a changé, on demande un redessin:
			if (_resizeGraphics) { this.Draw(); }
			// Si mauvaise qualité, applique l'échelle:
			if (!__DrawHighQuality && __DraftScale != 1) { e.Graphics.ScaleTransform(1 / __DraftScale, 1 / __DraftScale); }
			// Dans tous les cas, redessine:	
			e.Graphics.DrawImage(_bmp24, 0, 0);
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Redéfinit la zone cliente et met _resizeGraphics à true.
		/// </summary>
		private void _controlToDraw_SizeChanged(object sender, EventArgs e)
		{
			_resizeGraphics = true;
			_drawingRect = _controlToDraw.ClientRectangle;
			_graphWidth = _drawingRect.Width;
			_graphHeight = _drawingRect.Height;
			if (_controlToDraw.FindForm().Visible) { RecalculateAll(); }
		}


		// ---------------------------------------------------------------------------

		private bool _savedShowInfos;
		
		/// <summary>
		/// Supprime la mise à jour des messages.
		/// </summary>
		private void DrawingAreaForm_ResizeBegin(object sender, EventArgs e)
		{
			_savedShowInfos = GeoMsgSender.ShowInfos;
			GeoMsgSender.ShowInfos = false;
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Relance les calculs 2D du repère et des objets, ainsi que le dessin.
		/// </summary>
		private void DrawingAreaForm_ResizeEnd(object sender, EventArgs e)
		{
			GeoMsgSender.ShowInfos = _savedShowInfos;
		}



		#endregion GESTIONNAIRES D'EVENEMENTS




	}




}
