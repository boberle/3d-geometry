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


	/// <summary>
	/// Créer un point dans un repère 2D.
	/// </summary>
	public class SpPointOnPlaneObject : SpPointObject
	{

		// Déclarations:
		protected double _xp;
		protected double _yp;
		protected SpPlaneObject _plane;
		protected Coord2D _coordsp;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Point on plane"; } }
		
		/// <summary>
		/// Coordonnées X du plan.
		/// </summary>
		public virtual double XOnPlane { get { return _xp; } }
		
		/// <summary>
		/// Coordonnées Y du plan.
		/// </summary>
		public virtual double YOnPlane { get { return _yp; } }
		
		/// <summary>
		/// Obtient le plan
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }
		
		/// <summary>
		/// Obtient les coordonnées 2D.
		/// </summary>
		public Coord2D CoordinatesOnPlane { get { return _coordsp; } }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpPointOnPlaneObject(string name) : base(name)
		 { ; }
		
		/// <summary>
		/// Constructeur interne.
		/// </summary>
		internal SpPointOnPlaneObject(string name, SpPlaneObject plane, double xp, double yp) : this(name)
		{
			Alter(plane, xp, yp);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		internal virtual void Alter(SpPlaneObject plane, double xp, double yp)
		{
			_xp = xp; _yp = yp; _plane = plane;
			EndAlterProcess(_plane);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			CalculateCoordsStruct();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Modifie X, Y et Z. Recalcule les coordonnées 2D correspondantes. Ne recalcule pas les structures.
		/// </summary>
		internal override void AlterCoords(Coord3D coords)
		{
			_x = coords.X; _y = coords.Y; _z = coords.Z;
			Calculate2DCoordsFrom3D();
		}

		/// <summary>
		/// Modifie X, Y. Recalcules les coordonnées 3D correspondantes. Ne recalcule pas les structures.
		/// </summary>
		internal virtual void AlterCoords(Coord2D coordsp)
		{
			_xp = coordsp.X; _yp = coordsp.Y;
			Calculate3DCoordsFrom2D();
		}

		/// <summary>
		/// Modifie X, Y et Z, et à partir de ces données, XOnPlane et YOnPlane.
		/// </summary>
		internal override void AlterXYZ(double x, double y, double z)
		{
			_x =x; _y = y; _z = z;
			Calculate2DCoordsFrom3D();
		}

		/// <summary>
		/// Modifie XOnPlane et YOnPlane, et, à partir de ces données, X, Y et Z.
		/// </summary>
		internal virtual void AlterXYOnPlane(double xp, double yp)
		{
			_xp = xp; _yp = yp;
			Calculate3DCoordsFrom2D();
		}

		/// <summary>
		/// Calcul la structure de coordonnées. Toujours appeler cette méthode à la fin des calculs, ou bien appeler le CalculateNumData.
		/// </summary>
		protected override void CalculateCoordsStruct()
		{
			base.CalculateCoordsStruct();
			_coordsp = new Coord2D(_xp, _yp);
		}
		
		/// <summary>
		/// Recalcule les coordonnées 2D à partir des coordonnées 3D actuelles. Ne recalcule pas les structures.
		/// </summary>
		protected void Calculate2DCoordsFrom3D()
		{
			Coord2D coordsp = _plane.To2D(_x, _y, _z);
			_xp = coordsp.X; _yp = coordsp.Y;
		}
		
		/// <summary>
		/// Recalcule les coordonnées 3D à partir des coordonnées 2D actuelles. Ne recalcule pas les structures.
		/// </summary>
		protected void Calculate3DCoordsFrom2D()
		{
			Coord3D coords = _plane.To3D(_xp, _yp);
			_x = coords.X; _y = coords.Y; _z = coords.Z;
		}
		
		/// <summary>
		/// Retourne une chaîne commune aux points décrivant les coordonnées du point.
		/// </summary>
		public override string BaseToString()
		{
			return FormatText("({0},{1},{2}), ({3},{4}) on {5},", _x, _y, _z, _xp, _yp, _plane);
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
			{ return new object[]{_plane, _xp, _yp}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			double r, θ;
			θ = MathsGeo.ToPolar(_xp, _yp, true, out r);
			string polar2D = FormatText("Polar on plane {0}: ({1},{2}), {3}", _plane, r, θ, MathsGeo.GetAngleBounds(θ, 12, false, "θ"));
			return base.GetInfos(polar2D, lines);
		}
		
	}


	#endregion OBJETS DE BASE








	// ---------------------------------------------------------------------------
	// OBJECTS DU PLAN
	// ---------------------------------------------------------------------------




	#region OBJECTS DU PLAN


	/// <summary>
	/// Point du plan, indépendant de tout autre.
	/// </summary>
	public class SpPointOnPlane : SpPointOnPlaneObject
	{

		// Déclarations:
		private DoubleF _xpDblF;
		private DoubleF _ypDblF;
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointOnPlane(string name, SpPlaneObject plane, DoubleF xp, DoubleF yp) : base(name)
		{
			Alter(plane, xp, yp);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPlaneObject plane, DoubleF xp, DoubleF yp)
		{
			_plane = plane; _xpDblF = xp; _ypDblF = yp;
			EndAlterProcess(_plane, GetObjectsFromFormula(_xpDblF), GetObjectsFromFormula(_ypDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			_xp = _xpDblF.Recalculate(); _yp = _ypDblF.Recalculate();
			if (DoubleF.IsThereNan(_xp, _yp)) { SendCalculationResult(true, "Coordinates are not valid."); return; }
			Calculate3DCoordsFrom2D();
			CalculateCoordsStruct();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Modifie X.
		/// </summary>
		public virtual void AlterXOnPlane(double xp)
			{ _xpDblF.Value = xp; }
		
		/// <summary>
		/// Modifie Y.
		/// </summary>
		public virtual void AlterYOnPlane(double yp)
			{ _ypDblF.Value = yp; }
		
		/// <summary>
		/// Modifie X, Y.
		/// </summary>
		internal override void AlterXYOnPlane(double xp, double yp)
			{ _xpDblF.Value = xp; _ypDblF.Value = yp; }
		
		/// <summary>
		/// Modifie les coordonnées 2D, sans rien recalculer d'autre.
		/// </summary>
		internal override void AlterCoords(Coord2D coordsp)
			{ _xpDblF.Value = coordsp.X; _ypDblF.Value = coordsp.Y; }
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _xpDblF, oldName, newName);
			ChangeNameInFormula(ref _ypDblF, oldName, newName);
		}
				
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_plane, _xpDblF, _ypDblF}; }

	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Point défini par des coordonnées polaires.
	/// </summary>
	public class SpPointOnPlanePolar : SpPointOnPlaneObject
	{

		protected DoubleF _thetaDblF, _radiusDblF;
		protected double _theta, _radius;
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public double Theta { get { return _theta; } }
		
		/// <summary>
		/// Obtient le rayon.
		/// </summary>
		public double Radius { get { return _radius; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointOnPlanePolar(string name, SpPlaneObject plane, DoubleF radius, DoubleF theta) : base(name)
		{
			Alter(plane, radius, theta);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPlaneObject plane, DoubleF radius, DoubleF theta)
		{
			_plane = plane; _radiusDblF = radius; _thetaDblF = theta;
			EndAlterProcess(_plane, GetObjectsFromFormula(_radiusDblF), GetObjectsFromFormula(_thetaDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule:
			if (DoubleF.IsThereNan(_radius = _radiusDblF.Recalculate(), _theta = _thetaDblF.Recalculate()))
				{ SendCalculationResult(true, "Coordinates not valid."); return; }
			// Obtient les coordonnées cartésiennes:
			AlterCoords(MathsGeo.FromPolar(_radius, _theta));
			CalculateCoordsStruct();
			SendCalculationResult();
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _radiusDblF, oldName, newName);
			ChangeNameInFormula(ref _thetaDblF, oldName, newName);
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} using ({1},{2})", BaseToString(), _radius, _theta);
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_plane, _radiusDblF, _thetaDblF}; }
		
	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Tente de convertir un point de l'espace en point sur plan.
	/// </summary>
	public class SpPointOnPlaneFromSpace : SpPointOnPlaneObject
	{

		// Déclarations:
		private SpPointObject _base;
		
		/// <summary>
		/// Obtient le point de base.
		/// </summary>
		public SpPointObject BasePoint { get { return _base; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointOnPlaneFromSpace(string name, SpPlaneObject plane, SpPointObject basePt) : base(name)
		{
			Alter(plane, basePt);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPlaneObject plane, SpPointObject basePt)
		{
			_plane = plane; _base = basePt;
			EndAlterProcess(_plane, _base);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			Coord2D test = _plane.To2D(_base.Coordinates);
			if (test.Empty)
				{ SendCalculationResult(true, String.Format("Point {0} is not on plane {1}", _base.Name, _plane.Name)); return; }
			AlterCoords(test);
			CalculateCoordsStruct();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_plane, _base}; }

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Projeté orthogonal d'un point sur un plan.
	/// </summary>
	public class SpOrthoProjPointOnPlane : SpPointOnPlaneObject
	{
	
		protected SpPointObject _basePoint;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Orthogonal projection on plane"; } }
		
		/// <summary>
		/// Obtient le point à projeter.
		/// </summary>
		public SpPointObject BasePoint { get { return _basePoint; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpOrthoProjPointOnPlane(string name, SpPointObject basePoint, SpPlaneObject plane) : base(name)
		{
			Alter(basePoint, plane);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject basePoint, SpPlaneObject plane)
		{
			_basePoint = basePoint; _plane = plane;
			EndAlterProcess(_basePoint, _plane);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Utilise la formule:
			_coords = GeoFunctions.GetOrthoProjPointOnPlaneCoords(_basePoint.Coordinates, _plane.CartesianEq, _plane.NormalVectorCoords);
			if (_coords.Empty) { SendCalculationResult(true, "Normal vector is null"); return; }
			AlterCoords(_coords);
			CalculateCoordsStruct();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} of {1}", BaseToString(), _basePoint);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_basePoint, _plane}; }
		
	}
	
	
	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Point sur une fonction 2D.
	/// </summary>
	public class SpPointOnFunction1OnPlane : SpPointOnPlaneObject
	{
	
		protected bool _isLimited;
		protected SpFunction1OnPlane _function;
		private DoubleF _xpDblF;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Point on function1"; } }
		
		/// <summary>
		/// Obtient la fonction
		/// </summary>
		public SpFunction1OnPlane Function { get { return _function; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointOnFunction1OnPlane(string name, SpFunction1OnPlane func, DoubleF xp, bool isLimited) : base(name)
		{
			Alter(func, xp, isLimited);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpFunction1OnPlane func, DoubleF xp, bool isLimited)
		{
			_function = func; _xpDblF = xp; _isLimited = isLimited;
			EndAlterProcess(_function, GetObjectsFromFormula(_xpDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objest:
			_plane = _function.Plane;
			// Recalcule l'abscisse:
			_xp = _xpDblF.Recalculate();
			if (_xpDblF.IsNaN) { SendCalculationResult(true, "Value is not valid."); return; }
			if (_isLimited && _xp < _function.MinX) { _xp = _function.MinX; }
			if (_isLimited && _xp > _function.MaxX) { _xp = _function.MaxX; }
			// Obtient la valeur y:
			_yp = _function.GetFunctionValue(_xp);
			if (DoubleF.IsThereNan(_yp)) { SendCalculationResult(true, "Function returns an error."); return; }
			Calculate3DCoordsFrom2D();
			CalculateCoordsStruct();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Modifie la valeur de XOnPlane.
		/// </summary>
		public virtual void AlterXOnPlane(double xp)
			{ _xpDblF.Value = xp; }

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _xpDblF, oldName, newName);
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} on {1}", BaseToString(), _function);
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_function, _xpDblF, _isLimited}; }

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Point sur un cercle ou une ellipse.
	/// </summary>
	public class SpPointOnCircle : SpPointOnPlaneObject
	{
	
		protected SpCircle _circle;
		protected DoubleF _alphaDblF;
		protected double _alpha;
		protected bool _isLimited;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Point on circle"; } }
		
		/// <summary>
		/// Obtient le cercle
		/// </summary>
		public virtual SpCircle Circle { get { return _circle; } }
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public virtual double Alpha { get { return _alpha; } }

		/// <summary>
		/// Obtient si le déplacement du point est limité par Min et Max du cercle.
		/// </summary>
		public virtual bool IsLimited { get { return _isLimited; } }

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointOnCircle(string name, SpCircle circle, DoubleF alpha, bool isLimited) : base(name)
		{
			Alter(circle, alpha, isLimited);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpCircle circle, DoubleF alpha, bool isLimited)
		{
			_alphaDblF = alpha; _circle = circle; _isLimited = isLimited;
			EndAlterProcess(_circle, GetObjectsFromFormula(_alphaDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets:
			_plane = _circle.Plane;
			// Recalcule l'angle alpha:
			_alpha = _alphaDblF.Recalculate();
			if (_alphaDblF.IsNaN) { SendCalculationResult(true, "Alpha is not valid."); return; }
			if (_isLimited && _alpha > _circle.Max) { _alpha = _circle.Max; }
			if (_isLimited && _alpha < _circle.Min) { _alpha = _circle.Min; }
			// Si le cercle est défini par un centre et un point, on utilise une rotation à partir du repère 2D, ce qui permet de 
			// définir le point qui défini le cercle comme confondu avec ce point this si α = 0:
			if (_circle is SpCircleUsingPoint)
			{
				AlterCoords(GeoFunctions.GetRotatedPtCoords(_circle.Center.CoordinatesOnPlane,
					((SpCircleUsingPoint)_circle).Point.CoordinatesOnPlane, _alpha));
			}
			// Sinon, définit les coordonnées à partir d'une simple rotation;. Si le cercle est une ellipse,
			// modifie simplement width et height en conséquence:
			else
			{
				double width, height;
				if (_circle is SpEllipse) { width = ((SpEllipse)_circle).EllipseWidth; height = ((SpEllipse)_circle).EllipseHeight; }
				else { width = height = _circle.Radius; }
				AlterCoords(GeoFunctions.GetRotatedPointCoordsCreatingPlane(_circle.Center.CoordinatesOnPlane,
					new Coord2D(width, 0), new Coord2D(0, height), _alpha));
			}
			CalculateCoordsStruct();
			SendCalculationResult();
		}

		/// <summary>
		/// Modifie les valeurs des angles, si possible.
		/// </summary>
		public virtual void AlterApha(double alpha)
		{
			_alphaDblF.Value = alpha;
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _alphaDblF, oldName, newName);
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} on {1}", BaseToString(), _circle);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_circle, _alphaDblF, _isLimited}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string alpha = "Alpha: " + MathsGeo.GetAngleBounds(_alpha, 12, false, "α");
			return base.GetInfos(alpha, lines);
		}		

	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Point d'intersection entre une droite et un cercle.
	/// </summary>
	public class SpLineCircleIntersection : SpPointOnPlaneObject
	{
	
		protected SpLine _line;
		protected SpCircle _circle;
		protected bool _inter1;
		protected int _solNb;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Line and circle intersection"; } }
		
		/// <summary>
		/// Obtient la droite.
		/// </summary>
		public SpLine Line { get { return _line; } }
		
		/// <summary>
		/// Obtient le cercle.
		/// </summary>
		public SpCircle Circle { get { return _circle; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpLineCircleIntersection(string name, SpLine line, SpCircle circle, bool inter1) : base(name)
		{
			Alter(line, circle, inter1);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpLine line, SpCircle circle, bool inter1)
		{
			_line = line; _circle = circle; _inter1 = inter1;
			EndAlterProcess(_line, _circle);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets:
			_plane = _circle.Plane;
			// Cherdche les points d'intersection:
			Coord3D pt1, pt2;
			double? min = (_line is SpSegment || _line is SpRay ? 0 : (double?)null);
			double? max = (_line is SpSegment ? 1 : (double?)null);
			_solNb = GeoFunctions.GetLineCircleInterCoords(_circle.Center.Coordinates, _circle.Radius, _line.Point1.Coordinates,
				_line.Vector.Coordinates, _plane.Origin.Coordinates, _plane.XVector.Coordinates, _plane.YVector.Coordinates,
				min, max, out pt1, out pt2);
			// Si pas d'intersection, indéfini; si une intersection, pas de pb; si 2 intersections, choisit:
			if (_solNb == 0) { SendCalculationResult(true, "No intersection found."); return; }
			else if (_solNb == 1) { AlterCoords(pt1); }
			else if (_solNb == 2) { AlterCoords(_inter1 ? pt1 : pt2); }
			CalculateCoordsStruct();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} between {1} and {2}",	BaseToString(), _line, _circle);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_line, _circle, _inter1}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string nb = FormatText("Total number of intersections: {0}", _solNb);
			return base.GetInfos(nb, lines);
		}

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Point d'intersection entre une droite et un cercle.
	/// </summary>
	public class SpPlaneLineIntersection : SpPointOnPlaneObject
	{
	
		protected SpLine _line;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Plane and line intersection"; } }
		
		/// <summary>
		/// Obtient la droite.
		/// </summary>
		public SpLine Line { get { return _line; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPlaneLineIntersection(string name, SpPlaneObject plane, SpLine line) : base(name)
		{
			Alter(plane, line);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPlaneObject plane, SpLine line)
		{
			_plane = plane; _line = line;
			EndAlterProcess(_line, _plane);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les coordonnées 2D de l'intersection:
			double? min = (_line is SpSegment || _line is SpRay ? 0 : (double?)null);
			double? max = (_line is SpSegment ? 1 : (double?)null);
			if (!GeoFunctions.GetPlaneLineInterCoords(_plane.Origin.Coordinates, _plane.XVector.Coordinates, _plane.YVector.Coordinates,
				_line.Point1.Coordinates, _line.Vector.Coordinates, min, max, out _coordsp))
				{ SendCalculationResult(true, "No intersection found."); return; }
			AlterCoords(_coordsp);
			CalculateCoordsStruct();
			CalculateCoordsStruct();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} between {1} and {2}",	BaseToString(), _plane, _line);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_plane, _line}; }

	}

	

	#endregion OBJECTS DU PLAN
	


	
}
