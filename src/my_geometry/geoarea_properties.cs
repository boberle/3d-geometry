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
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES




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
		/// Couleur de l'axe X.
		/// </summary>
		public Color XAxisColor
		{ 
			get { return __XAxisColor; } 
			set { __XAxisColor = value; this.MakeCoordSystemPens(); this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Couleur de l'axe Y.
		/// </summary>
		public Color YAxisColor
		{ 
			get { return __YAxisColor; } 
			set { __YAxisColor = value; this.MakeCoordSystemPens(); this.DrawAuto(Assembly.GetCallingAssembly()); }
		}

		/// <summary>
		/// Couleur de l'axe Z.
		/// </summary>
		public Color ZAxisColor
		{ 
			get { return __ZAxisColor; } 
			set { __ZAxisColor = value; this.MakeCoordSystemPens(); this.DrawAuto(Assembly.GetCallingAssembly()); }
		}
		
		
		/// <summary>
		/// Point d'origine du plan du form (origine pour les méthodes de C#). Lecture publique, définition interne.
		/// </summary>
		public Point OriginOnWindow
		{
			get { return _OriginOnWindow; }
			internal set { _OriginOnWindow = value; }
		}

		/// <summary>
		/// Angle d'Euler Phi.
		/// </summary>
		public double Phi
		{
			get { return __Phi; }
			set
			{
				__Phi = value;
				this.OnInfos("Phi = " + value.ToString());
				this.DrawAuto(Assembly.GetCallingAssembly());
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
				__Theta = value;
				this.OnInfos("Theta = " + value.ToString());
				this.DrawAuto(Assembly.GetCallingAssembly());
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
				this.OnInfos("XNorm = " + value.ToString());
				this.DrawAuto(Assembly.GetCallingAssembly());
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
				this.OnInfos("YNorm = " + value.ToString());
				this.DrawAuto(Assembly.GetCallingAssembly());
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
				this.OnInfos("ZNorm = " + value.ToString());
				this.DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Deformation sur l'axe X du form par l'objet Graphics. Permet de modifier l'épaisseur de tous les traits, par exemple, et sans déformation si XScale = YScale.
		/// </summary>
		public int XScale
		{
			get { return __XScale; }
			set
			{
				__XScale = value;
				this.OnInfos("XScale = " + value.ToString());
				this.DrawAuto(Assembly.GetCallingAssembly());
			}
		}

		/// <summary>
		/// Deformation sur l'axe Y du form par l'objet Graphics. Permet de modifier l'épaisseur de tous les traits, par exemple, et sans déformation si XScale = YScale.
		/// </summary>
		public int YScale
		{
			get { return __YScale; }
			set
			{
				__YScale = value;
				this.OnInfos("YScale = " + value.ToString());
				this.DrawAuto(Assembly.GetCallingAssembly());
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
				__Zoom = value;
				this.OnInfos("Zoom = " + value.ToString());
				this.DrawAuto(Assembly.GetCallingAssembly());
			}
		}
		
		/// <summary>
		/// Obtient ou définit s'il faut redessiner automatiquement après toute modification. True par défaut.
		/// </summary>
		public bool AutoDraw { get { return __AutoDraw; } set { __AutoDraw = value; this.DrawAuto(Assembly.GetCallingAssembly()); } }



		#endregion PROPRIETES
	






	}





}