using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace My
{





	// ---------------------------------------------------------------------------
	// OBJETS DE BASE
	// ---------------------------------------------------------------------------




	#region OBJETS DE BASE

	

	#endregion OBJETS DE BASE
	




	// ---------------------------------------------------------------------------
	// OBJETS DE L'ESPACE
	// ---------------------------------------------------------------------------




	#region OBJETS DE L'ESPACE




	/// <summary>
	/// Polygone définis par différents points.
	/// </summary>
	public class SpPolygon : SpBrushPenObject
	{
	
		protected SpPointObject[] _vertices;
		protected double _perimeter, _area;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Polygon"; } }
		
		/// <summary>
		/// Obtient les sommets du polygone.
		/// </summary>
		public SpPointObject[] Vertices { get { return _vertices; } }
		
		/// <summary>
		/// Obtient le périmètre du polygone.
		/// </summary>
		public double Perimeter { get { return _perimeter; } }
		
		/// <summary>
		/// Obtient l'aire du polygone. Cette aire n'est valable que si le polygone est convexe.
		/// </summary>
		public double Area { get { return _area; } }
		
		/// <summary>
		/// Obtient ou définit les points à dessiner dans le repère du form.
		/// </summary>
		internal PointF[] PointsOnWin { get; set; }
	
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpPolygon(string name) : base(name)
		{
			PointsOnWin = new PointF[0];
		}
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPolygon(string name, params SpPointObject[] vertices) : this(name)
		{
			Alter(vertices);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(params SpPointObject[] vertices)
		{
			_vertices = vertices;
			EndAlterProcess(_vertices);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			if (!CommonCalculateNumericData()) { return; }
			SendCalculationResult();
		}
		
		/// <summary>
		/// Vérifie qu'il y a assez de points, calcule l'aire, le périmètre et l'origine du label. Retourne false si erreur et si l'objet a été mis à indéfini.
		/// </summary>
		protected virtual bool CommonCalculateNumericData()
		{
			// Si moins de trois points, sort:
			if (_vertices.Distinct().ToArray().Length < 3) { SendCalculationResult(true, "Not enough points."); return false; }
			Coord3D[] vertCoords = Array.ConvertAll<SpPointObject,Coord3D>
				(_vertices, delegate(SpPointObject pt) { return pt.Coordinates; });
			// Si les points ne sont pas coplanaires (si poly sur plan, on saute cette vérif), sort:
			if (!(this is SpPolygonOnPlane) && !GeoFunctions.AreCoplanar(vertCoords))
				{ SendCalculationResult(true, "Points are not coplanar."); return false; }
			// Calcule l'aire et le périmètre:
			_perimeter = GeoFunctions.GetPolygonPerimeter(vertCoords);
			_area = GeoFunctions.GetConvexPolygonArea(vertCoords);
			// Origine du label:
			_labelOrigin = GeoFunctions.GetBarycenterCoords(vertCoords);
			return true;
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("through " + My.ArrayFunctions.Join(_vertices, delegate(SpPointObject pt) { return pt.Name; }, ","));
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_vertices}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string peri = FormatText("Perimeter: {0}", _perimeter);
			string area = FormatText("Area: {0} (valid only if polygon is convexe)", _area);
			return base.GetInfos(peri, area, lines);
		}
		
	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Même chose que SpPolygon, mais tous les points sont des SpPointOnPlaneObject.
	/// </summary>
	public class SpPolygonOnPlane : SpPolygon
	{
	
		protected SpPlaneObject _plane;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Polygon on plane"; } }
		
		/// <summary>
		/// Obtient le plan.
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		protected SpPolygonOnPlane(string name) : base(name)
			{ ; }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPolygonOnPlane(string name, params SpPointOnPlaneObject[] vertices) : this(name)
		{
			Alter(vertices);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(params SpPointOnPlaneObject[] vertices)
		{
			_vertices = vertices; _plane = ((SpPointOnPlaneObject)_vertices[0]).Plane;
			EndAlterProcess(_vertices);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets.
			_plane = ((SpPointOnPlaneObject)_vertices[0]).Plane;
			// Vérifie que tous les points appartiennent au même plan, puis appelle la base:
			int l = _vertices.Length;
			for (int i=1; i<l; i++)
			{
				if (((SpPointOnPlane)_vertices[i]).Plane != _plane)
					{ SendCalculationResult(true, String.Format("Point {0} is not on the plane {1}.", _vertices[i].Name, _plane.Name)); return; }
			}
			if (!CommonCalculateNumericData()) { return; }
			SendCalculationResult();
		}
		
	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Rectangle sur un plan.
	/// </summary>
	public class SpRectangleOnPlane : SpPolygonOnPlane
	{
	
		protected SpPointOnPlaneObject _center;
		protected DoubleF _heightDblF, _widthDblF, _alphaDblF;
		protected double _height, _width, _alpha;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Rectangle"; } }
		
		/// <summary>
		/// Obtient le centre.
		/// </summary>
		public SpPointOnPlaneObject Center { get { return _center; } }
		
		/// <summary>
		/// Obtient la largeur.
		/// </summary>
		public double Width { get { return _width; } }
		
		/// <summary>
		/// Obtient la hauteur.
		/// </summary>
		public double Height { get { return _height; } }
		
		/// <summary>
		/// Obtient l'angle de rotation.
		/// </summary>
		public double Alpha { get { return _alpha; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpRectangleOnPlane(string name, SpPointOnPlaneObject center, DoubleF width, DoubleF height, DoubleF alpha) : base(name)
		{
			Alter(center, width, height, alpha);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject center, DoubleF width, DoubleF height, DoubleF alpha)
		{
			_center = center; _plane = _center.Plane; _heightDblF = height; _widthDblF = width; _alphaDblF = alpha;
			// Construit les points:
			if (_vertices == null)
			{
				_vertices = new SpPointOnPlaneObject[4];
				_vertices[0] = new SpPointOnPlaneObject("%pt1", _plane, 0, 0);
				_vertices[1] = new SpPointOnPlaneObject("%pt2", _plane, 0, 0);
				_vertices[2] = new SpPointOnPlaneObject("%pt3", _plane, 0, 0);
				_vertices[3] = new SpPointOnPlaneObject("%pt4", _plane, 0, 0);
			}
			EndAlterProcess(_center, GetObjectsFromFormula(_widthDblF), GetObjectsFromFormula(_heightDblF),
				GetObjectsFromFormula(_alphaDblF), null, _vertices);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets:
			_plane = _center.Plane;
			// Recalcule:
			if (DoubleF.IsThereNan(_width = _widthDblF.Recalculate(), _height = _heightDblF.Recalculate(),
				_alpha = _alphaDblF.Recalculate()))
				{ SendCalculationResult(true, "Width or height not valid."); return; }
			// Dispose les points:
			double halfH = _height / 2, halfW = _width / 2;
			Coord2D center = _center.CoordinatesOnPlane;
			Coord2D[] pts = new Coord2D[4];
			pts[0] = new Coord2D(center.X - halfW, center.Y + halfH);
			pts[1] = new Coord2D(center.X + halfW, center.Y + halfH);
			pts[2] = new Coord2D(center.X + halfW, center.Y - halfH);
			pts[3] = new Coord2D(center.X - halfW, center.Y - halfH);
			if (_alpha != 0) {
				for (int i=0; i<4; i++) {
					pts[i] = GeoFunctions.GetRotatedPtCoords(center, pts[i], _alpha); } }
			for (int i=0; i<4; i++) {
				_vertices[i].RebuildObject(false, _plane);
				((SpPointOnPlaneObject)_vertices[i]).AlterCoords(pts[i]);
				_vertices[i].Recalculate(true); }
			// Appelle la base:
			if (!CommonCalculateNumericData()) { return; }
			SendCalculationResult();
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _widthDblF, oldName, newName);
			ChangeNameInFormula(ref _heightDblF, oldName, newName);
			ChangeNameInFormula(ref _alphaDblF, oldName, newName);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_center, _widthDblF, _heightDblF, _alphaDblF}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string wh = FormatText("Width: {0}; Height: {1}", _width, _height);
			return base.GetInfos(wh, lines);
		}
				
	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Polygone régulier sur un plan, défini par un centre, un sommet et un nombre de côtés (le nombre ne peut être changé par la suite).
	/// </summary>
	public class SpRegularPolygonOnPlane : SpPolygonOnPlane
	{
	
		protected SpPointOnPlaneObject _center, _point1;
		protected int _nb;
		protected bool _invertDir;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Regular polygon"; } }
		
		/// <summary>
		/// Obtient le centre.
		/// </summary>
		public SpPointOnPlaneObject Center { get { return _center; } }
		
		/// <summary>
		/// Obtient le point 1.
		/// </summary>
		public SpPointOnPlaneObject Point1 { get { return _point1; } }
		
		/// <summary>
		/// Obtient le nombre de points.
		/// </summary>
		public double PointsNumber { get { return _nb; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpRegularPolygonOnPlane(string name, SpPointOnPlaneObject center, SpPointOnPlaneObject spt1, bool invertDir, int ptsNb) : base(name)
		{
			_nb = ptsNb;
			Alter(center, spt1, invertDir);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject center, SpPointOnPlaneObject spt1, bool invertDir)
		{
			_center = center; _point1 = spt1; _plane = center.Plane; _invertDir = invertDir;
			// Si Nb est inférieur à 3, on sort:
			if (_nb < 3) { SendCalculationResult(true, "Not enough point. Destroy object and recreate an other one."); return; }
			// Créer les points, si pas déjà fait:
			if (_vertices == null) {
				_vertices = new SpPointOnPlaneObject[_nb];
				for (int i=1; i<_nb; i++) { _vertices[i] = new SpPointOnPlaneObject("%pt" + (i+1).ToString(), _plane, 0, 0); }
			}
			// Le premier sommet est le point 1:
			_vertices[0] = spt1;
			EndAlterProcess(_center, _point1, null, _vertices.Skip(1).ToArray());
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets:
			_plane = _center.Plane;
			// Sort si center et point1 ne sont pas sur le même plan:
			if (_plane != _point1.Plane)
				{ SendCalculationResult(true, "Center and Point1 are not on the same plane."); return; }
			// Variables:
			double angle = Math.PI * 2 / _nb * (_invertDir ? -1 : 1);
			SpPointOnPlaneObject[] pts = Array.ConvertAll<SpPointObject,SpPointOnPlaneObject>(_vertices,
				delegate (SpPointObject o) { return (SpPointOnPlaneObject)o; });
			Coord2D center = _center.CoordinatesOnPlane, pt1 = _point1.CoordinatesOnPlane;
			// Modifie le plan des points systèmes et recalcule:
			for (int i=1; i<_nb; i++) {
				pts[i].RebuildObject(false, _plane);
				pts[i].AlterCoords(GeoFunctions.GetRotatedPtCoords(center, pt1, angle * i));
				pts[i].Recalculate(true); }
			// Procédure commune de calcul:
			if (!CommonCalculateNumericData()) { return; }
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_center, _point1, _invertDir}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string nb = String.Format("Number of vertices: {0}", _nb);
			double r = _center.Coordinates.GetLength(_point1.Coordinates);
			double α = Math.PI * 2 / _nb, β = Math.PI * (_nb - 2) / _nb;
			string radius = FormatText("Radius: {0}", r);
			string edge = FormatText("Edge length: {0}", r * Math.Sin(α / 2) * 2);
			string centerAngle = FormatText("Center angle: {0}, {1}", α, MathsGeo.GetAngleBounds(α, 12, true));
			string sideAngle = FormatText("Side angle: {0}, {1}", β, MathsGeo.GetAngleBounds(β, 12, true));
			return base.GetInfos(nb, radius, edge, centerAngle, sideAngle, lines);
		}
				
	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Sphere définit par un centre et un rayon.
	/// </summary>
	public class SpSphere : SpBrushPenObject
	{
	
		private bool _useBmp;
		private Bitmap _bmpOnWin;
		private My.ChBmpColorValues _bmpValues;
		protected DoubleF _radiusDblF;
		protected SpPointObject _center;
		protected double _radius;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Sphere"; } }
		
		/// <summary>
		/// Centre.
		/// </summary>
		public SpPointObject Center { get { return _center; } }
		
		/// <summary>
		/// Rayon.
		/// </summary>
		public double Radius { get { return _radius; } }
		
		/// <summary>
		/// Indique s'il faut utiliser l'image bmp d'une sphère plutôt qu'un simple disque qui rend peut les 3 dimensions. Entraîne un réaffichage du dessin. True par défaut, sauf si système ou virtuel.
		/// </summary>
		public bool UseBmp {
			get { return _useBmp; }
			set
			{
				_useBmp = value;
				if (value && _bmpOnWin == null) { _bmpOnWin = new Bitmap(Geometry.MyResources.sphere_template200_png); }
				OnRequestDrawing(); }
			}
		
		/// <summary>
		/// Rectangle pour le dessin dans le repère du form.
		/// </summary>
		internal RectangleF RectOnWin { get; set; }
		
		/// <summary>
		/// Obtient l'image à utiliser pour dessiner la sphère.
		/// </summary>
		internal Bitmap BmpOnWin { get { return _bmpOnWin; } }
		
		/// <summary>
		/// Obtient les valeurs de modifications des canaux ARVB de l'image de la sphère.
		/// </summary>
		public ChBmpColorValues BmpValues { get { return _bmpValues; } }

		protected SpSphere(string name) : base(name)
		{
			// Si n'est pas système ou virtuel, affiche par défaut le bitmap:
			if (!IsVirtual && !IsSystem)
			{
				_useBmp = false;
				ChangeBmpColors(new ChBmpColorValues(false, 1, 100, 1, 1, 1));
				_useBmp = true;
			}
			else
				{_useBmp = false; _bmpOnWin = null; }
		}

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpSphere(string name, SpPointObject center, DoubleF radius) : this(name)
		{
			Alter(center, radius);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject center, DoubleF radius)
		{
			_center = center; _radiusDblF = radius;
			EndAlterProcess(_center, GetObjectsFromFormula(_radiusDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule le rayon:
			_radius = _radiusDblF.Recalculate();
			if (_radiusDblF.IsNaN || _radius < 0) { SendCalculationResult(true, "Radius is not valid."); return; }
			// Origine du label:
			_labelOrigin = _center.Coordinates;
			SendCalculationResult();
		}
		
		/// <summary>
		/// Modifie la valeur du rayon, si possible.
		/// </summary>
		public virtual void AlterRadius(double radius)
		{
			_radiusDblF.Value = radius;
		}
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _radiusDblF, oldName, newName);
		}

		/// <summary>
		/// Affiche une boîte de dialogue pour modifier les couleurs de l'image de la sphère, et met UseBmp à true. Entraîne un réaffichage du dessin.
		/// </summary>
		public void ChangeBmpColors()
		{
			My.DialogBoxBmpColors dialog = new My.DialogBoxBmpColors(Geometry.MyResources.sphere_template200_png);
			if (!_bmpValues.IsEmpty) { dialog.TransformValues = _bmpValues; }
			if (dialog.ShowDialog() == DialogBoxClickResult.OK) 
				{ _bmpOnWin = dialog.NewBitmap; _bmpValues = dialog.TransformValues; _useBmp = true; OnRequestDrawing(); }
			dialog.Dispose();
		}
		
		/// <summary>
		/// Change les couleurs de l'image directement, sans afficher une boîte de dialogue et sans modifier UseBmp. Entraîne un réaffichage du dessin si UseBmp vaut true;
		/// </summary>
		public void ChangeBmpColors(ChBmpColorValues bmpValues)
		{
			_bmpValues = bmpValues;
			_bmpOnWin = My.DialogBoxBmpColors.TransformPicture(bmpValues.Light, bmpValues.Alpha, bmpValues.Red, bmpValues.Green, bmpValues.Blue,
				bmpValues.ConvertToGray, Geometry.MyResources.sphere_template200_png);
			if (_useBmp) { OnRequestDrawing(); }
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("({0},{1})", _center, _radius);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_center, _radiusDblF}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string eq = FormatText("Eq.: (x-{0})^2 + (y-{1})^2 + (z-{2})^2 = {3}^2", _center.X, _center.Y, _center.Z, _radius);
			string area = FormatText("Area: 4πr^2 = {0}", 4 * Math.PI * Math.Pow(_radius, 2.0));
			string vol = FormatText("Volume: (4/3)πr^3  = {0}", (4/3) * Math.PI * Math.Pow(_radius, 3.0));
			return base.GetInfos(eq, area, vol, lines);
		}

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Sphère défini par un centre et un point sur le cercle.
	/// </summary>
	public class SpSphereUsingPoint : SpSphere
	{
	
		protected SpPointObject _point;
	
		/// <summary>
		/// Obtient le point définissant le cercle.
		/// </summary>
		public SpPointObject Point { get { return _point; } }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpSphereUsingPoint(string name) : base(name)
		{
			_radiusDblF = new DoubleF();
		}
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpSphereUsingPoint(string name, SpPointObject center, SpPointObject point) : this(name)
		{
			Alter(center, point);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject center, SpPointObject point)
		{
			_center = center; _point = point;
			EndAlterProcess(_center, _point);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			_radiusDblF.Value = _center.Coordinates.GetLength(_point.Coordinates);
			base.CalculateNumericData();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("({0},{1}) through {2}", _center, _radius, _point);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_center, _point}; }

	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Solide dans l'espace.
	/// </summary>
	public class SpSolid : SpBrushPenObject
	{

		// Déclarations:
		protected SpPointObject[] _vertices;
		protected SpPointObject[][] _faces;
		protected SpPointObject _center;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Solid"; } }
	
		/// <summary>
		/// Sommets.
		/// </summary>
		public SpPointObject[] Vertices { get { return _vertices; } }
		
		/// <summary>
		/// Faces.
		/// </summary>
		public SpPointObject[][] Faces { get { return _faces; } }
		
		/// <summary>
		/// Obtient le point d'inertie du solide. Si le centre n'est pas explictement défini, il s'agit de l'isobarycentre de tous les sommets.
		/// </summary>
		public SpPointObject Center { get { return _center; } }
		
		/// <summary>
		/// Faces sur le form.
		/// </summary>
		internal PointF[][] FacesOnWin { get; set; }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpSolid(string name) : base(name)
		{
			FacesOnWin = new PointF[0][];
		}
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpSolid(string name, SpPointObject[] vertices, SpPointObject[][] faces) : this(name)
		{
			_center = new SpPointObject("%center", 0, 0, 0);
			Alter(vertices, faces);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject[] vertices, SpPointObject[][] faces)
		{
			_vertices = vertices; _faces = faces;
			EndAlterProcess(_vertices, null, _center);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Contrôle les faces, puis calcule le centre:
			if (!ControlFaces()) { return; }
			_center.AlterCoords(GeoFunctions.GetBarycenterCoords(
				Array.ConvertAll(_vertices, delegate(SpPointObject o) { return o.Coordinates; })));
			_center.Recalculate(true);
			SendCalculationResult();
		}
		
		/// <summary>
		/// Vérifie que les faces sont correctes, cad coplanaires. Si ce n'est pas le cas, affiche message d'erreur et passe Undefined à true. Le fait aussi si des points des faces ne sont pas dans les sommets.
		/// </summary>
		protected virtual bool ControlFaces()
		{
			// Vérifie qu'il y a le nombre requis de points pour chaque face (au moins trois):
			bool isOk = true;
			if (_vertices.Length < 4) { isOk = false; }
			foreach (SpPointObject[] objs in _faces) { if (objs.Length < 3) { isOk = false; break; } }
			if (!isOk) { SendCalculationResult(true, String.Format("Not enough vertices or faces!")); return false; }
			// Contrôles que les points des faces sont bien des sommets:
			SpPointObject[] allFacePts = My.ArrayFunctions.UnrollArray<SpPointObject>(_faces);
			foreach (SpPointObject o in allFacePts) {
				if (!_vertices.Contains(o)) {
					SendCalculationResult(true, String.Format("Point {0} define a face, but is not a vertex!", o.Name)); return false; } }
			allFacePts = null;
			// Vérifie la coplanarité:
			foreach (SpPointObject[] objs in _faces)
			{
				if (!GeoFunctions.AreCoplanar(Array.ConvertAll<SpPointObject,Coord3D>(objs,
					delegate(SpPointObject o) { return o.Coordinates; })))
					{ SendCalculationResult(true, String.Format("Points {0} are not coplanar.", My.ArrayFunctions.Join(
						Array.ConvertAll<SpPointObject,string>(objs, delegate(SpPointObject o) { return o.Name; }), ","))); return false; }
			}
			return true;
		}
		
		/// <summary>
		/// Retourne une chaîne commune aux points décrivant les coordonnées du point.
		/// </summary>
		public virtual string BaseToString()
		{
			return FormatText("centered at {0}, through {1}", _center,
				My.ArrayFunctions.Join(_vertices, delegate(SpObject o) { return o.Name; }, ","));
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString(BaseToString());
		}
				
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_vertices, _faces}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			StringBuilder sb = new StringBuilder("Faces:\n");
			int l = _faces.Length;
			for (int i=0; i<l; i++)
				{ sb.AppendFormat("   ({0}) {1}{2}", i, My.ArrayFunctions.Join(_faces[i], delegate(SpObject o) { return o.Name; }, ","),
					(i==l-1 ? "" : "\n")); }
			return base.GetInfos(sb.ToString(), lines);
		}
		
	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Solide dans l'espace, dont l'utilisateur peut définir le centre, qui n'est alors pas calculer comme le barycentre des sommets.
	/// </summary>
	public class SpSolidWithCenter : SpSolid
	{

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpSolidWithCenter(string name, SpPointObject center, SpPointObject[] vertices, SpPointObject[][] faces) : base(name)
		{
			Alter(center, vertices, faces);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject center, SpPointObject[] vertices, SpPointObject[][] faces)
		{
			_vertices = vertices; _faces = faces; _center = center;
			EndAlterProcess(_vertices, _center);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Contrôle les faces, puis calcule le centre:
			if (!ControlFaces()) { return; }
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_center, _vertices, _faces}; }
		
	}	


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Solide dans l'espace, avec une rotation. Cet objet n'est pas destiné à être instancié en tant que tel, mais il sert de base au solide qui ont besoin des variables et fonction de rotation selon trois angles.
	/// </summary>
	public class SpRotatedSolid : SpSolid
	{

		// Déclarations:
		protected DoubleF _phiDblF, _thetaDblF, _psiDblF;
		protected double _phi, _theta, _psi;

		/// <summary>
		/// Angle de rotation theta.
		/// </summary>
		public double Theta { get { return _theta; } }
		
		/// <summary>
		/// Angle de rotation phi.
		/// </summary>
		public double Phi { get { return _phi; } }
		
		/// <summary>
		/// Angle de rotation psi.
		/// </summary>
		public double Psi { get { return _psi; } }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpRotatedSolid(string name) : base(name)
			{ ; }
		
		/// <summary>
		/// Applique une rotation dans l'espace, de centre center, avec les trois angles fournis. Recalcule les DblF des trois angles (si erreur, retourne false et affiche message d'erreur tout en mettant l'objet en Indéfini). Ne fait rien s'ils valent 0. Le centre utilisé est center: celui-ci doit donc être calculé avant l'appelle de la méthode.
		/// </summary>
		protected virtual bool RotateVertices(ref Coord3D[] vertices)
		{
			if (DoubleF.IsThereNan(_psi = _psiDblF.Recalculate(), _theta = _thetaDblF.Recalculate(), _phi = _phiDblF.Recalculate()))
				{ SendCalculationResult(true, "Psi, theta or phi not valid."); return false; }
			if (_phi == 0 && _theta == 0 && _psi == 0) { return true; }
			vertices = GeoFunctions.GetEulerRotatedPtCoords(_psi, _theta, _phi, _center.Coordinates, vertices);
			return true;
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _psiDblF, oldName, newName);
			ChangeNameInFormula(ref _thetaDblF, oldName, newName);
			ChangeNameInFormula(ref _phiDblF, oldName, newName);
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0}, rotated by ({1},{2},{3})", BaseToString(), _psi, _theta, _phi);
		}

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string rot = FormatText("Rotation: Psi: {0}; Theta: {1}; Phi: {2}; {3}, {4}, {5}",
				_psi, _theta, _phi, MathsGeo.GetAngleBounds(_psi, 12, false, "ψ"),
				MathsGeo.GetAngleBounds(_theta, 12, false, "θ"), MathsGeo.GetAngleBounds(_phi, 12, false, "φ"));
			return base.GetInfos(rot, lines);
		}
		
	}	


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Cube dans l'espace.
	/// </summary>
	public class SpParallelepiped : SpRotatedSolid
	{

		// Déclarations:
		protected double _width, _height, _length;
		private DoubleF _widthDblF, _heightDblF, _lengthDblF;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Parallelepiped"; } }
	
		/// <summary>
		/// Largeur.
		/// </summary>
		public double Width { get { return _width; } }
	
		/// <summary>
		/// Hauteur.
		/// </summary>
		public double Height { get { return _height; } }
	
		/// <summary>
		/// Longueur.
		/// </summary>
		public double Length { get { return _length; } }
				
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpParallelepiped(string name, SpPointObject center, DoubleF length, DoubleF width, DoubleF heigth,
			DoubleF psi, DoubleF theta, DoubleF phi) : base(name)
		{
			_vertices = new SpPointObject[8];
			_vertices[0] = new SpPointObject("%pt1", 0, 0, 0);
			_vertices[1] = new SpPointObject("%pt2", 0, 0, 0);
			_vertices[2] = new SpPointObject("%pt3", 0, 0, 0);
			_vertices[3] = new SpPointObject("%pt4", 0, 0, 0);
			_vertices[4] = new SpPointObject("%pt5", 0, 0, 0);
			_vertices[5] = new SpPointObject("%pt6", 0, 0, 0);
			_vertices[6] = new SpPointObject("%pt7", 0, 0, 0);
			_vertices[7] = new SpPointObject("%pt8", 0, 0, 0);
			_faces = new SpPointObject[6][];
			_faces[0] = new SpPointObject[]{_vertices[0],_vertices[1],_vertices[2],_vertices[3]};
			_faces[1] = new SpPointObject[]{_vertices[4],_vertices[5],_vertices[6],_vertices[7]};
			_faces[2] = new SpPointObject[]{_vertices[0],_vertices[1],_vertices[5],_vertices[4]};
			_faces[3] = new SpPointObject[]{_vertices[1],_vertices[2],_vertices[6],_vertices[5]};
			_faces[4] = new SpPointObject[]{_vertices[2],_vertices[3],_vertices[7],_vertices[6]};
			_faces[5] = new SpPointObject[]{_vertices[3],_vertices[0],_vertices[4],_vertices[7]};
			Alter(center, length, width, heigth, psi, theta, phi);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject center, DoubleF length, DoubleF width, DoubleF heigth, DoubleF psi, DoubleF theta, DoubleF phi)
		{
			_phiDblF = phi; _thetaDblF = theta; _psiDblF = psi;
			_center = center; _lengthDblF = length; _widthDblF = width; _heightDblF = heigth;
			EndAlterProcess(_center, GetObjectsFromFormula(_lengthDblF), GetObjectsFromFormula(_widthDblF),
				GetObjectsFromFormula(_heightDblF), GetObjectsFromFormula(_thetaDblF),
				GetObjectsFromFormula(_phiDblF), GetObjectsFromFormula(_psiDblF), null, _vertices);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule les longueurs:
			_width = _widthDblF.Recalculate(); _height = _heightDblF.Recalculate(); _length = _lengthDblF.Recalculate();
			if (DoubleF.IsThereNan(_width, _height, _length))
				{ SendCalculationResult(true, "Length, width or height not valid."); return; }
			if (_width < 0 || _height < 0 || _length < 0) { SendCalculationResult(true, "Length, width or height negative."); return; }
			// Calcule la position des points:
			double X = _center.Coordinates.X, Y = _center.Coordinates.Y, Z = _center.Coordinates.Z;
			double halfW = _width / 2, halfH = _height / 2, halfL = _length / 2;
			Coord3D[] vertices = new Coord3D[8];
			vertices[0] = new Coord3D(X-halfL, Y-halfW, Z-halfH);
			vertices[1] = new Coord3D(X-halfL, Y+halfW, Z-halfH);
			vertices[2] = new Coord3D(X+halfL, Y+halfW, Z-halfH);
			vertices[3] = new Coord3D(X+halfL, Y-halfW, Z-halfH);
			vertices[4] = new Coord3D(X-halfL, Y-halfW, Z+halfH);
			vertices[5] = new Coord3D(X-halfL, Y+halfW, Z+halfH);
			vertices[6] = new Coord3D(X+halfL, Y+halfW, Z+halfH);
			vertices[7] = new Coord3D(X+halfL, Y-halfW, Z+halfH);
			// Rotation:
			if (!RotateVertices(ref vertices)) { return; }
			// Recalcule:
			for (int i=0; i<8; i++) { _vertices[i].AlterCoords(vertices[i]); _vertices[i].Recalculate(true); }
			SendCalculationResult();
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			base.RebuildFormulas(oldName, newName);
			ChangeNameInFormula(ref _lengthDblF, oldName, newName);
			ChangeNameInFormula(ref _widthDblF, oldName, newName);
			ChangeNameInFormula(ref _heightDblF, oldName, newName);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_center, _lengthDblF, _widthDblF, _heightDblF, _psiDblF, _thetaDblF, _phiDblF}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string lwh = FormatText("Length:{0}, Width:{1}, Height:{2}", _length, _width, _height);
			string vol = FormatText("Volume: {0}", _length * _width * _height);
			string area = FormatText("Area: {0}", 2.0*_length*_width + 2.0*_width*_height + 2.0*_length*_height);
			string peri = FormatText("Perimeter (edges): {0}", 4.0*_length + 4.0*_width + 4.0*_height);
			return base.GetInfos(lwh, vol, area, peri, lines);
		}
		
	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Tétraèdre régulier dans l'espace.
	/// </summary>
	public class SpRegularTetrahedron : SpRotatedSolid
	{

		// Déclarations:
		protected double _edgeLength;
		private DoubleF _edgeLengthDblF;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Regular tetrahedron"; } }
	
		/// <summary>
		/// Largeur.
		/// </summary>
		public double EdgeLength { get { return _edgeLength; } }

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpRegularTetrahedron(string name, SpPointObject center, DoubleF edgeLength, DoubleF psi, DoubleF theta, DoubleF phi) : base(name)
		{
			_vertices = new SpPointObject[4];
			_vertices[0] = new SpPointObject("%vert", 0, 0, 0);
			_vertices[1] = new SpPointObject("%pt1", 0, 0, 0);
			_vertices[2] = new SpPointObject("%pt2", 0, 0, 0);
			_vertices[3] = new SpPointObject("%pt3", 0, 0, 0);
			_faces = new SpPointObject[4][];
			_faces[0] = new SpPointObject[]{_vertices[0],_vertices[1],_vertices[2]};
			_faces[1] = new SpPointObject[]{_vertices[0],_vertices[1],_vertices[3]};
			_faces[2] = new SpPointObject[]{_vertices[0],_vertices[2],_vertices[3]};
			_faces[3] = new SpPointObject[]{_vertices[1],_vertices[2],_vertices[3]};
			Alter(center, edgeLength, psi, theta, phi);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject center, DoubleF edgeLength, DoubleF psi, DoubleF theta, DoubleF phi)
		{
			_center = center; _edgeLengthDblF = edgeLength; _phiDblF = phi; _thetaDblF = theta; _psiDblF = psi;
			EndAlterProcess(_center, GetObjectsFromFormula(_edgeLengthDblF), GetObjectsFromFormula(_phiDblF),
				GetObjectsFromFormula(_thetaDblF), GetObjectsFromFormula(_psiDblF), null, _vertices);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule les longueurs:
			if (DoubleF.IsThereNan(_edgeLength = _edgeLengthDblF.Recalculate()))
				{ SendCalculationResult(true, "Length is not valid."); return; }
			if (_edgeLength < 0) { SendCalculationResult(true, "Length negative."); return; }
			// Calcule la position des points:
			double X = _center.Coordinates.X, Y = _center.Coordinates.Y, Z = _center.Coordinates.Z;
			double a = _edgeLength;
			Coord3D[] vertices = new Coord3D[4];
			vertices[0] = new Coord3D(X, Y, Z + a * Math.Sqrt(6) / 4);
			vertices[1] = new Coord3D(X + a * Math.Sqrt(3) / 3, Y, Z - a * Math.Sqrt(6) / 12);
			vertices[2] = new Coord3D(X - a * Math.Sqrt(3) / 6, Y + a / 2, Z - a * Math.Sqrt(6) / 12);
			vertices[3] = new Coord3D(X - a * Math.Sqrt(3) / 6, Y - a / 2, Z - a * Math.Sqrt(6) / 12);
			// Rotation:
			if (!RotateVertices(ref vertices)) { return; }
			// Recalcule:
			for (int i=0; i<4; i++) { _vertices[i].AlterCoords(vertices[i]); _vertices[i].Recalculate(true); }
			SendCalculationResult();
		}
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			base.RebuildFormulas(oldName, newName);
			ChangeNameInFormula(ref _edgeLengthDblF, oldName, newName);
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_center, _edgeLengthDblF, _psiDblF, _thetaDblF, _phiDblF}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string height = FormatText("Tetra height: a√6/3 = {2}", _vertices[0].Name, _vertices[1].Name, _edgeLength*Math.Sqrt(6)/3);
			string vol = FormatText("Volume: a^3*√2/12 = {0}", Math.Pow(_edgeLength, 3)*Math.Sqrt(2)/12);
			string area = FormatText("Area: a^2*√3 = {0}", _edgeLength*_edgeLength*Math.Sqrt(3));
			string peri = FormatText("Perimeter (edges): 6a = {0}", 6*_edgeLength);
			return base.GetInfos(height, vol, area, peri, lines);
		}
		
	}
	

	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Tétraèdre régulier posé sur un plan.
	/// </summary>
	public class SpRegularTetrahedronOnPlane : SpSolid
	{

		// Déclarations:
		protected SpPointOnPlaneObject _point1, _point2;
		protected SpPlaneObject _plane;
		protected bool _invertOnPlane, _invertInSpace;
		protected double _edgeLength;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Regular tetrahedron on plane"; } }
	
		/// <summary>
		/// Plan.
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }

		/// <summary>
		/// Point.
		/// </summary>
		public SpPointOnPlaneObject Point1 { get { return _point1; } }

		/// <summary>
		/// Point.
		/// </summary>
		public SpPointOnPlaneObject Point2 { get { return _point2; } }

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpRegularTetrahedronOnPlane(string name, SpPointOnPlaneObject spt1, SpPointOnPlaneObject spt2, bool invertOnPlane,
			bool invertInSpace) : base(name)
		{
			_center = new SpPointObject("%center", 0, 0, 0);
			Alter(spt1, spt2, invertOnPlane, invertInSpace);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject spt1, SpPointOnPlaneObject spt2, bool invertOnPlane, bool invertInSpace)
		{
			_point1 = spt1; _point2 = spt2; _invertOnPlane = invertOnPlane; _invertInSpace = invertInSpace; _plane = _point1.Plane;
			// Si pas encore fait, créer les objets virtuels:
			if (_vertices == null)
			{
				_vertices = new SpPointObject[4];
				_vertices[0] = new SpPointObject("%vert", 0, 0, 0);
				_vertices[1] = _point1;
				_vertices[2] = _point2;
				_vertices[3] = new SpPointOnPlaneObject("%pt3", _plane, 0, 0);
				_faces = new SpPointObject[4][];
				_faces[0] = new SpPointObject[]{_vertices[0],_vertices[1],_vertices[2]};
				_faces[1] = new SpPointObject[]{_vertices[0],_vertices[1],_vertices[3]};
				_faces[2] = new SpPointObject[]{_vertices[0],_vertices[2],_vertices[3]};
				_faces[3] = new SpPointObject[]{_vertices[1],_vertices[2],_vertices[3]};
			}
			// Remplace les points fournit dans les sommets et les faces:
			else
			{
				_vertices[1] = _point1;
				_vertices[2] = _point2;
				_faces[0][1] = _vertices[1]; _faces[0][2] = _vertices[2];
				_faces[1][1] = _vertices[1];
				_faces[2][1] = _vertices[2];
				_faces[3][0] = _vertices[1]; _faces[3][1] = _vertices[2];
				_vertices[3].RebuildObject(false, _plane);
			}
			EndAlterProcess(_plane, _point1, _point2, null, _center, _vertices[0], _vertices[3]);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupére les objets:
			_plane = _point1.Plane;
			_vertices[3].RebuildObject(false, _plane);
			// Sort si les plans ne correspondent pas:
			if (_plane != _point2.Plane) { SendCalculationResult(true, "Points are not on the same plane."); return; }
			// Calcule les coordonnées du troisième point sur le plan:
			Coord2D vAB = _point2.CoordinatesOnPlane - _point1.CoordinatesOnPlane;
			Coord2D u = new Coord2D(-vAB.Y, vAB.X); if (_invertOnPlane) { u *= -1; }
			((SpPointOnPlaneObject)_vertices[3]).AlterCoords(_point1.CoordinatesOnPlane + vAB / 2 + Math.Sqrt(3) / 2 * u);
			_vertices[3].Recalculate(true);
			// Calcule le sommet:
			_edgeLength = _point1.Coordinates.GetLength(_point2.Coordinates);
			Coord3D G = GeoFunctions.GetBarycenterCoords(_vertices[1].Coordinates, _vertices[2].Coordinates, _vertices[3].Coordinates);
			_vertices[0].AlterCoords(G + _plane.NormalVectorCoords * _edgeLength * Math.Sqrt(6) / 3 * (_invertInSpace ? -1 : 1));
			_vertices[0].Recalculate(true);
			// Recalcule le centre:
			_center.AlterCoords(GeoFunctions.GetBarycenterCoords(Array.ConvertAll(_vertices, delegate(SpPointObject o) { return o.Coordinates; })));
			_center.Recalculate(true);
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _point2, _invertOnPlane, _invertInSpace}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string edgeLen = FormatText("Edge length: {0}", _edgeLength);
			string height = FormatText("Tetra height: a√6/3 = {2}", _vertices[0].Name, _vertices[1].Name, _edgeLength*Math.Sqrt(6)/3);
			string vol = FormatText("Volume: a^3*√2/12 = {0}", Math.Pow(_edgeLength, 3)*Math.Sqrt(2)/12);
			string area = FormatText("Area: a^2*√3 = {0}", _edgeLength*_edgeLength*Math.Sqrt(3));
			string peri = FormatText("Perimeter (edges): 6a = {0}", 6*_edgeLength);
			return base.GetInfos(edgeLen, height, vol, area, peri, lines);
		}
		
	}
	

	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Tétraèdre régulier posé sur un plan.
	/// </summary>
	public class SpCubeOnPlane : SpSolid
	{

		// Déclarations:
		protected SpPointOnPlaneObject _point1, _point2;
		protected SpPlaneObject _plane;
		protected bool _invertOnPlane, _invertInSpace;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Cube on plane"; } }
	
		/// <summary>
		/// Plan.
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }

		/// <summary>
		/// Point.
		/// </summary>
		public SpPointOnPlaneObject Point1 { get { return _point1; } }

		/// <summary>
		/// Point.
		/// </summary>
		public SpPointOnPlaneObject Point2 { get { return _point2; } }

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpCubeOnPlane(string name, SpPointOnPlaneObject spt1, SpPointOnPlaneObject spt2, bool invertOnPlane,
			bool invertInSpace) : base(name)
		{
			_center = new SpPointObject("%center", 0, 0, 0);
			Alter(spt1, spt2, invertOnPlane, invertInSpace);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject spt1, SpPointOnPlaneObject spt2, bool invertOnPlane, bool invertInSpace)
		{
			_point1 = spt1; _point2 = spt2; _invertOnPlane = invertOnPlane; _invertInSpace = invertInSpace; _plane = _point1.Plane;
			// Si pas encore fait, créer les objets virtuels:
			if (_vertices == null)
			{
				_vertices = new SpPointObject[8];
				_vertices[0] = _point1;
				_vertices[1] = _point2;
				_vertices[2] = new SpPointOnPlaneObject("%pt3", _plane, 0, 0);
				_vertices[3] = new SpPointOnPlaneObject("%pt4", _plane, 0, 0);
				_vertices[4] = new SpPointObject("%pt5", 0, 0, 0);
				_vertices[5] = new SpPointObject("%pt6", 0, 0, 0);
				_vertices[6] = new SpPointObject("%pt7", 0, 0, 0);
				_vertices[7] = new SpPointObject("%pt8", 0, 0, 0);
				_faces = new SpPointObject[6][];
				_faces[0] = new SpPointObject[]{_vertices[0],_vertices[1],_vertices[2],_vertices[3]};
				_faces[1] = new SpPointObject[]{_vertices[4],_vertices[5],_vertices[6],_vertices[7]};
				_faces[2] = new SpPointObject[]{_vertices[0],_vertices[1],_vertices[5],_vertices[4]};
				_faces[3] = new SpPointObject[]{_vertices[1],_vertices[2],_vertices[6],_vertices[5]};
				_faces[4] = new SpPointObject[]{_vertices[2],_vertices[3],_vertices[7],_vertices[6]};
				_faces[5] = new SpPointObject[]{_vertices[3],_vertices[0],_vertices[4],_vertices[7]};
			}
			// Remplace les points fournit dans les sommets et les faces:
			else
			{
				_vertices[0] = _point1;
				_vertices[1] = _point2;
				_faces[0][0] = _vertices[0];
				_faces[0][1] = _vertices[1];
				_faces[2][0] = _vertices[0];
				_faces[2][1] = _vertices[1];
				_faces[3][0] = _vertices[1];
				_faces[5][1] = _vertices[0];
				_vertices[2].RebuildObject(false, _plane);
				_vertices[3].RebuildObject(false, _plane);
			}
			EndAlterProcess(_plane, _point1, _point2, null, _center, _vertices.Skip(2).ToArray());
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupére les objets:
			_plane = _point1.Plane;
			_vertices[2].RebuildObject(false, _plane);
			_vertices[3].RebuildObject(false, _plane);
			// Sort si les plans ne correspondent pas:
			if (_plane != _point2.Plane) { SendCalculationResult(true, "Points are not on the same plane."); return; }
			// Obtient les vecteurs:
			Coord3D vAC = _plane.To3D(_point1.CoordinatesOnPlane.Y - _point2.CoordinatesOnPlane.Y,
				_point2.CoordinatesOnPlane.X - _point1.CoordinatesOnPlane.X);
			Coord3D vAE = _plane.NormalVectorCoords * ( vAC.GetNorm() / _plane.NormalVectorCoords.GetNorm() );
			if (_invertOnPlane) { vAC *= -1; }
			if (_invertInSpace) { vAE *= -1; }
			// Place les points:
			((SpPointOnPlaneObject)_vertices[2]).AlterCoords(_point2.Coordinates + vAC);
			((SpPointOnPlaneObject)_vertices[3]).AlterCoords(_point1.Coordinates + vAC);
			_vertices[2].Recalculate(true); _vertices[3].Recalculate(true);
			for (int i=4; i<8; i++)
				{ _vertices[i].AlterCoords(_vertices[i-4].Coordinates + vAE); _vertices[i].Recalculate(true); }
			// Recalcule le centre:
			_center.AlterCoords(GeoFunctions.GetBarycenterCoords(Array.ConvertAll(_vertices, delegate(SpPointObject o) { return o.Coordinates; })));
			_center.Recalculate(true);
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _point2, _invertOnPlane, _invertInSpace}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			double len = _point1.Coordinates.GetLength(_point2.Coordinates);
			string edgeLen = FormatText("Edge length: {0}", len);
			string vol = FormatText("Volume: len^3 = {0}", len * len * len);
			string area = FormatText("Area: len^2*6 = {0}", 6 * len * len);
			string peri = FormatText("Perimeter (edges): 12*len = {0}", 12 * len);
			return base.GetInfos(edgeLen, vol, area, peri);
		}
		
	}
	

	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Cube dans l'espace.
	/// </summary>
	public class SpCone : SpBrushPenObject
	{

		// Déclarations:
		protected SpPointObject _vertex;
		protected SpCircle _baseCircle;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Cone"; } }

		/// <summary>
		/// Cercle (ou ellipse) de base.
		/// </summary>
		public SpCircle BaseCircle { get { return _baseCircle; } }
				
		/// <summary>
		/// Centre.
		/// </summary>
		public SpPointObject Vertex { get { return _vertex; } }
		
		/// <summary>
		/// Obtient les faces à dessiner sur le form.
		/// </summary>
		internal PointF[][] FacesOnWin { get; set; }
				
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpCone(string name, SpPointObject vertex, SpCircle baseCircle) : base(name)
		{
			FacesOnWin = new PointF[0][];
			Alter(vertex, baseCircle);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject vertex, SpCircle baseCircle)
		{
			_vertex = vertex; _baseCircle = baseCircle;
			EndAlterProcess(_vertex, _baseCircle);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Pas grand choise à faire... Tout est dans le dessin:
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("based on circle {0}, using vertex {1}", _baseCircle, _vertex);
		}
				
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_vertex, _baseCircle}; }

	}




	#endregion OBJETS DE L'ESPACE


	
}
