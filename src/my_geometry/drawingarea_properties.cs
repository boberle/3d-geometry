using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;

namespace My
{




	public partial class DrawingArea
	{


	
	
	
	
		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES
		
		
		
		/// <summary>
		/// Obtient la police pour les listes et autres affichage, inscrite dans les Settings.
		/// </summary>
		public static Font DefaultListFont {
			get { return My.Geometry.MySettings.DefaultListFont; } }


		/// <summary>
		/// Obtient la collection des objets de l'espace.
		/// </summary>
		public SpObjectsCollection SpaceObjects
		{
			get { return _spObjects; }
			private set { _spObjects = value; }
		}
		
		/// <summary>
		/// Affichage ou non du système de coordonnées.
		/// </summary>
		public bool ShowCoordinateSystem
		{
			get { return __ShowCoordinateSystem; }
			set { __ShowCoordinateSystem = value; this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Qualité de dessin.
		/// </summary>
		public bool DrawHighQuality
		{
			get { return __DrawHighQuality; }
			set { __DrawHighQuality = value; this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Affiche ou masque le rectangle de découpe (pour la sauvegarde de l'image).
		/// </summary>
		public bool ShowClipRect
		{
			get { return __ShowClipRect; }
			set { __ShowClipRect = value; this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Définit l'abscisse sur chaque axe au bout de laquelle il ne faut plus afficher l'axe. Si 0, n'affiche pas les axes.
		/// </summary>
		public int ShowAxes
		{
			get { return __MaxAxes; }
			set { __MaxAxes = Math.Abs(value); this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Définit s'il faut afficher les graduations des axes. N'est utile que si MaxAxes est différent de 0.
		/// </summary>
		public bool ShowGraduations
		{
			get { return __ShowGraduations; }
			set { __ShowGraduations = value; this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Définit s'il faut afficher la grille sur le plan X-Y.
		/// </summary>
		public bool ShowXYGrid
		{
			get { return __ShowXYGrid; }
			set { __ShowXYGrid = value; this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Définit s'il faut afficher la grille sur le plan X-Z.
		/// </summary>
		public bool ShowXZGrid
		{
			get { return __ShowXZGrid; }
			set { __ShowXZGrid = value; this.DrawAuto(Assembly.GetCallingAssembly()); }
		}
		
		/// <summary>
		/// Définit s'il faut afficher la grille sur le plan Y-Z.
		/// </summary>
		public bool ShowYZGrid
		{
			get { return __ShowYZGrid; }
			set { __ShowYZGrid = value; this.DrawAuto(Assembly.GetCallingAssembly()); }
		}
		
		/// <summary>
		/// Rectangle de découpe (pour la sauvegarde de l'image).
		/// </summary>
		public Rectangle ClipRect
		{
			get { return __ClipRect; }
			set { __ClipRect = value; this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Epaisseur des axes.
		/// </summary>
		public float AxisWidth
		{ 
			get { return __AxisWidth; } 
			set {
				__AxisWidth = value; MakeCommenPens(); 	this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Epaisseur de la grille.
		/// </summary>
		public float GridWidth
		{ 
			get { return __GridWidth; } 
			set {
				__GridWidth = value; MakeCommenPens(); 	this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Epaisseur du repère.
		/// </summary>
		public float CoordinateSystemWidth
		{ 
			get { return __CoordinateSystemWidth; } 
			set {
				__CoordinateSystemWidth = value; MakeCommenPens(); 	this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Couleur de l'axe X.
		/// </summary>
		public Color XAxisColor
		{ 
			get { return __XAxisColor; } 
			set {
				__XAxisColor = value; MakeCommenPens(); 	this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Couleur de l'axe Y.
		/// </summary>
		public Color YAxisColor
		{ 
			get { return __YAxisColor; } 
			set {
				__YAxisColor = value; MakeCommenPens(); this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Couleur de l'axe Z.
		/// </summary>
		public Color ZAxisColor
		{ 
			get { return __ZAxisColor; } 
			set {
				__ZAxisColor = value; MakeCommenPens(); this.DrawAuto(Assembly.GetCallingAssembly()); }
		}
		
		/// <summary>
		/// Police des graduations.
		/// </summary>
		public Font GraduationsFont
		{
			get { return __GraduationsFont; }
			set { __GraduationsFont = value; MakeCommenPens(); this.DrawAuto(Assembly.GetCallingAssembly()); }
		}
		
		/// <summary>
		/// Point d'origine du plan du form (origine pour les méthodes de C#).
		/// </summary>
		public Point OriginOnWindow
		{
			get { return _OriginOnWin; }
			set { _OriginOnWin = value; this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Angle d'Euler Phi.
		/// </summary>
		public double Phi
		{
			get { return __Phi; }
			set
			{
				__Phi = MathsGeo.GetMainAngleMeasure(value, false);
				CalculateCoordSystemDrawingData();
				CalculateAllObjectsDrawingData(CalcUpdMode.Calc3D);
				DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Angle d'Euler Theta.
		/// </summary>
		public double Theta
		{
			get { return __Theta; }
			set
			{
				__Theta = MathsGeo.GetMainAngleMeasure(value, false);
				CalculateCoordSystemDrawingData();
				CalculateAllObjectsDrawingData(CalcUpdMode.Calc3D);
				DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Angle de rotation.
		/// </summary>
		public double Rotation
		{
			get { return __Rotation; }
			set
			{
				__Rotation = MathsGeo.GetMainAngleMeasure(value, false);
				CalculateCoordSystemDrawingData();
				CalculateAllObjectsDrawingData(CalcUpdMode.Calc3D);
				DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Norme de l'axe X.
		/// </summary>
		public float XNorm
		{
			get { return __XNorm; }
			set
			{
				__XNorm = value;
				CalculateCoordSystemDrawingData();
				CalculateAllObjectsDrawingData(CalcUpdMode.Calc3D);
				DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Norme de l'axe Y.
		/// </summary>
		public float YNorm
		{
			get { return __YNorm; }
			set
			{
				__YNorm = value;
				CalculateCoordSystemDrawingData();
				CalculateAllObjectsDrawingData(CalcUpdMode.Calc3D);
				DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Norme de l'axe Z.
		/// </summary>
		public float ZNorm
		{
			get { return __ZNorm; }
			set
			{
				__ZNorm = value;
				CalculateCoordSystemDrawingData();
				CalculateAllObjectsDrawingData(CalcUpdMode.Calc3D);
				DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Echelle de dessin. Permet de modifier d'un coup l'épaisseur de tous les traits, par exemple. Par défaut, 2.
		/// </summary>
		public float Scale
		{
			get { return __Scale; }
			set
			{
				__Scale = value;
				DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Echelle de dessin pour l'affichage en mode brouillon. Par défaut, 0.5.
		/// </summary>
		public float DraftScale
		{
			get { return __DraftScale; }
			set
			{
				__DraftScale = value;
				DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Zoom. 50 par défaut.
		/// </summary>
		public int Zoom
		{
			get { return __Zoom; }
			set
			{
				// Change le ClipRect:
				if (__Zoom != 0 && value != 0)
				{
					float ratio = (float)value / (float)__Zoom;
					__ClipRect = new Rectangle((int)(__ClipRect.X * ratio), (int)(__ClipRect.Y * ratio),
						(int)(__ClipRect.Width * ratio), (int)(__ClipRect.Height * ratio));
				}
				// Change la valeur:
				__Zoom = value;
				// Redessin:
				CalculateCoordSystemDrawingData();
				CalculateAllObjectsDrawingData(CalcUpdMode.Calc3D);
				DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Obtient ou définit s'il faut redessiner automatiquement après toute modification. True par défaut. Ne recalcule rien et ne redessine pas.
		/// </summary>
		public bool AutoDraw {
			get { return __AutoDraw; }
			set { __AutoDraw = value; } }
		
		/// <summary>
		/// Obtient ou définit s'il faut suspendre les calculs pour le dessin. (Par exemple, lors de nombreuses modification d'un coup.) Si la valeur actuelle est true et qu'elle passe à false, l'ense un recalcul général, mais ne redessine pas.
		/// </summary>
		public bool SuspendCalculation {
			get { return __SuspendCalculation; }
			set { if (__SuspendCalculation && !value) { __SuspendCalculation = value; RecalculateAll(); } __SuspendCalculation = value; } }
		
		/// <summary>
		/// Obtient ou définit s'il faut afficher les messages de recalcul et de dessin.
		/// </summary>
		public bool ShowDrawingMessages {
			get { return __ShowDrawingMessages; }
			set { __ShowDrawingMessages = value; } }



		#endregion PROPRIETES




	}
	
	
	
	
}
