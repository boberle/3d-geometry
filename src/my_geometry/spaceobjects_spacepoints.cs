using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;

namespace My
{






	// ---------------------------------------------------------------------------
	// OBJETS DE BASE
	// ---------------------------------------------------------------------------




	#region OBJETS DE BASE


	/// <summary>
	/// Classe de base pour les points.
	/// </summary>
	public class SpPointObject : SpBrushObject
	{

		// Déclarations:
		protected Coord3D _coords;
		protected double _x, _y, _z;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Point"; } }
	
		/// <summary>
		/// Coordonnées X dans l'espace.
		/// </summary>
		public virtual double X { get { return _x; } }
		
		/// <summary>
		/// Coordonnées Y dans l'espace.
		/// </summary>
		public virtual double Y { get { return _y; } }
		
		/// <summary>
		/// Coordonnées Z dans l'espace.
		/// </summary>
		public virtual double Z { get { return _z; } }
		
		/// <summary>
		/// Obtient une structeur de coordonnées.
		/// </summary>
		public virtual Coord3D Coordinates { get { return _coords; } }

		/// <summary>
		/// Forme du point lorsqu'il est affiché.
		/// </summary>
		public PointShape PointShape { get; internal set; }
		
		/// <summary>
		/// Coordonnée du point dans le plan du form, par rapport au repère du form.
		/// </summary>
		internal PointF PtOnWin { get; set; }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpPointObject(string name) : base(name)
		{
			PointShape = PointShape.Round;
		}
		
		/// <summary>
		/// Constructeur interne.
		/// </summary>
		internal SpPointObject(string name, double x, double y, double z) : this(name)
		{
			Alter(x, y, z);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		internal virtual void Alter(double x, double y, double z)
		{
			_x = x; _y = y; _z = z;
			EndAlterProcess();
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
		/// Calcul la structure de coordonnées. Toujours appeler cette méthode à la fin des calculs, ou bien appeler le CalculateNumData.
		/// </summary>
		protected virtual void CalculateCoordsStruct()
			{ _coords = new Coord3D(_x, _y, _z); }
				
		/// <summary>
		/// Modifie X, Y et Z.
		/// </summary>
		internal virtual void AlterCoords(Coord3D coords)
			{ _x = coords.X; _y = coords.Y; _z = coords.Z; }

		/// <summary>
		/// Modifie X, Y et Z.
		/// </summary>
		internal virtual void AlterXYZ(double x, double y, double z)
			{ _x =x; _y = y; _z = z; }
		
		/// <summary>
		/// Retourne une chaîne commune aux points décrivant les coordonnées du point.
		/// </summary>
		public virtual string BaseToString()
		{
			return FormatText("({0},{1},{2})", _x, _y, _z);
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
			{ return new object[]{_x, _y, _z}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			double r, λ, φ;
			MathsGeo.ToGeographic(_x, _y, _z, true, 0, out λ, out φ, out r);
			string polar = FormatText("Polar: ({0},{1},{2}), {3}, {4}", r, λ, φ, MathsGeo.GetAngleBounds(λ, 12, false, "λ"),
				MathsGeo.GetAngleBounds(φ, 12, false, "φ"));
			return base.GetInfos(polar, lines);
		}
		
	}



	#endregion OBJETS DE BASE
	




	// ---------------------------------------------------------------------------
	// OBJETS DE L'ESPACE
	// ---------------------------------------------------------------------------




	#region OBJETS DE L'ESPACE




	/// <summary>
	/// Point de l'espace, indépendant de tout autre.
	/// </summary>
	public class SpPoint : SpPointObject
	{

		// Déclarations:
		protected DoubleF _xDblF;
		protected DoubleF _yDblF;
		protected DoubleF _zDblF;
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPoint(string name, DoubleF x, DoubleF y, DoubleF z) : base(name)
		{
			Alter(x, y, z);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(DoubleF x, DoubleF y, DoubleF z)
		{
			_xDblF = x; _yDblF = y; _zDblF = z;
			EndAlterProcess(GetObjectsFromFormula(_xDblF), GetObjectsFromFormula(_yDblF),
				GetObjectsFromFormula(_zDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			_x = _xDblF.Recalculate(); _y = _yDblF.Recalculate(); _z = _zDblF.Recalculate();
			if (DoubleF.IsThereNan(_x, _y, _z)) { SendCalculationResult(true, "Coordinates are not valid."); return; }
			CalculateCoordsStruct();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Modifie X.
		/// </summary>
		public virtual void AlterX(double x)
			{ _xDblF.Value = x; }
		
		/// <summary>
		/// Modifie Y.
		/// </summary>
		public virtual void AlterY(double y)
			{ _yDblF.Value = y; }
		
		/// <summary>
		/// Modifie Z.
		/// </summary>
		public virtual void AlterZ(double z)
			{ _zDblF.Value = z; }
		
		/// <summary>
		/// Modifie X, Y et Z.
		/// </summary>
		public new void AlterXYZ(double x, double y, double z)
			{ _xDblF.Value = x; _yDblF.Value = y; _zDblF.Value = z; }
		
		/// <summary>
		/// Modifie X, Y et Z.
		/// </summary>
		internal override void AlterCoords(Coord3D coords)
			{ _xDblF.Value = coords.X; _yDblF.Value = coords.Y; _zDblF.Value = coords.Z; }
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _xDblF, oldName, newName);
			ChangeNameInFormula(ref _yDblF, oldName, newName);
			ChangeNameInFormula(ref _zDblF, oldName, newName);
		}
				
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_xDblF, _yDblF, _zDblF}; }

	}




	// ---------------------------------------------------------------------------


	/// <summary>
	/// Point défini par des coordonnées polaires à 2 angles.
	/// </summary>
	public class SpPointPolar : SpPointObject
	{

		protected DoubleF _lambdaDblF, _radiusDblF, _phiDblF;
		protected double _lambda, _radius, _phi;
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public double Lambda { get { return _lambda; } }
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public double Phi { get { return _phi; } }
		
		/// <summary>
		/// Obtient le rayon.
		/// </summary>
		public double Radius { get { return _radius; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointPolar(string name, DoubleF radius, DoubleF lambda, DoubleF phi) : base(name)
		{
			Alter(radius, lambda, phi);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(DoubleF radius, DoubleF lambda, DoubleF phi)
		{
			_radiusDblF = radius; _lambdaDblF = lambda; _phiDblF = phi;
			EndAlterProcess(GetObjectsFromFormula(_radiusDblF), GetObjectsFromFormula(_lambdaDblF),
				GetObjectsFromFormula(_phiDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule:
			if (DoubleF.IsThereNan(_radius = _radiusDblF.Recalculate(), _lambda = _lambdaDblF.Recalculate(),
				_phi = _phiDblF.Recalculate()))
				{ SendCalculationResult(true, "Coordinates not valid."); return; }
			// Obtient les coordonnées cartésiennes:
			double λ = _lambda, φ = _phi;
			MathsGeo.NormalizeGeographicAngles(ref λ, ref φ);
			_coords = MathsGeo.FromPolar(_radius, λ, φ);
			AlterCoords(_coords);
			SendCalculationResult();
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _radiusDblF, oldName, newName);
			ChangeNameInFormula(ref _lambdaDblF, oldName, newName);
			ChangeNameInFormula(ref _phiDblF, oldName, newName);
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} using ({1},{2},{3})", BaseToString(), _radius, _lambda, _phi);
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_radiusDblF, _lambdaDblF, _phiDblF}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string lambda = String.Format("Lambda: {0}", MathsGeo.GetAngleBounds(_lambda, 12, false, "λ"));
			string phi = String.Format("Phi: {0}", MathsGeo.GetAngleBounds(_phi, 12, false, "φ"));
			return base.GetInfos(lambda, phi, lines);
		}
				
	}
	

	// ---------------------------------------------------------------------------

	
	/// <summary>
	/// Change les coordonnées d'un point (_radius,0,0) dans le repère général en coordonnées dans un repère modifié par les trois angles d'Euler.
	/// </summary>
	public class SpPointPolar3 : SpPointObject
	{

		protected DoubleF _thetaDblF, _psiDblF, _phiDblF, _radiusDblF;
		protected double _theta, _psi, _phi, _radius;
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public double Theta { get { return _theta; } }
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public double Phi { get { return _phi; } }
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public double Psi { get { return _psi; } }
		
		/// <summary>
		/// Obtient le rayon.
		/// </summary>
		public double Radius { get { return _radius; } }

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointPolar3(string name, DoubleF radius, DoubleF psi, DoubleF theta, DoubleF phi) : base(name)
		{
			Alter(radius, psi, theta, phi);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(DoubleF radius, DoubleF psi, DoubleF theta, DoubleF phi)
		{
			_psiDblF = psi; _thetaDblF = theta; _phiDblF = phi; _radiusDblF = radius;
			EndAlterProcess(GetObjectsFromFormula(_psiDblF), GetObjectsFromFormula(_thetaDblF),
				GetObjectsFromFormula(_phiDblF), GetObjectsFromFormula(_radiusDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule:
			_psi = _psiDblF.Recalculate(); _phi = _phiDblF.Recalculate(); _theta = _thetaDblF.Recalculate();
			_radius = _radiusDblF.Recalculate();
			if (DoubleF.IsThereNan(_psi, _phi, _theta, _radius))
				{ SendCalculationResult(true, "Coordinates not valid."); return; }
			// Obtient les coordonnées cartésiennes dans le repère général:
			_coords = GeoFunctions.GetEulerRotatedPtCoords(_psi, _theta, _phi, new Coord3D(), new Coord3D(_radius, 0, 0));
			AlterCoords(_coords);
			SendCalculationResult();
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _psiDblF, oldName, newName);
			ChangeNameInFormula(ref _thetaDblF, oldName, newName);
			ChangeNameInFormula(ref _phiDblF, oldName, newName);
			ChangeNameInFormula(ref _radiusDblF, oldName, newName);
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} using ({1},{2},{3},{4})", BaseToString(), _radius, _psi, _theta, _phi);
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_radiusDblF, _psiDblF, _thetaDblF, _phiDblF}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string psi = String.Format("Psi: {0}", MathsGeo.GetAngleBounds(_psi, 12, false, "ψ"));
			string theta = String.Format("Theta: {0}", MathsGeo.GetAngleBounds(_theta, 12, false, "θ"));
			string phi = String.Format("Phi: {0}", MathsGeo.GetAngleBounds(_phi, 12, false, "φ"));
			return base.GetInfos(psi, theta, phi, lines);
		}
				
	}
	

	// ---------------------------------------------------------------------------

	
	/// <summary>
	/// Change les coordonnées d'un point dans le repère général en coordonnées dans un repère modifié par les trois angles d'Euler.
	/// </summary>
	public class SpPointPolar3UsingCoords : SpPointObject
	{

		protected DoubleF _thetaDblF, _psiDblF, _phiDblF, _xsDblF, _ysDblF, _zsDblF;
		protected double _theta, _psi, _phi, _xs, _ys, _zs;
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public double Theta { get { return _theta; } }
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public double Phi { get { return _phi; } }
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public double Psi { get { return _psi; } }
		
		/// <summary>
		/// Obtient les coordonnées de départ.
		/// </summary>
		public double XStart { get { return _xs; } }

		/// <summary>
		/// Obtient les coordonnées de départ.
		/// </summary>
		public double YStart { get { return _ys; } }

		/// <summary>
		/// Obtient les coordonnées de départ.
		/// </summary>
		public double ZStart { get { return _zs; } }	
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointPolar3UsingCoords(string name, DoubleF psi, DoubleF theta, DoubleF phi, DoubleF xs, DoubleF ys, DoubleF zs) : base(name)
		{
			Alter(psi, theta, phi, xs, ys, zs);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(DoubleF psi, DoubleF theta, DoubleF phi, DoubleF xs, DoubleF ys, DoubleF zs)
		{
			_psiDblF = psi; _thetaDblF = theta; _phiDblF = phi; _xsDblF = xs; _ysDblF = ys; _zsDblF = zs;
			EndAlterProcess(GetObjectsFromFormula(_psiDblF), GetObjectsFromFormula(_thetaDblF),
				GetObjectsFromFormula(_phiDblF), GetObjectsFromFormula(_xsDblF), GetObjectsFromFormula(_ysDblF),
				GetObjectsFromFormula(_zsDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule:
			_psi = _psiDblF.Recalculate(); _phi = _phiDblF.Recalculate(); _theta = _thetaDblF.Recalculate();
			_xs = _xsDblF.Recalculate(); _ys = _ysDblF.Recalculate(); _zs = _zsDblF.Recalculate();
			if (DoubleF.IsThereNan(_psi, _phi, _theta, _xs, _ys, _zs))
				{ SendCalculationResult(true, "Coordinates not valid."); return; }
			// Obtient les coordonnées cartésiennes dans le repère général:
			_coords = GeoFunctions.GetEulerRotatedPtCoords(_psi, _theta, _phi, new Coord3D(), new Coord3D(_xs, _ys, _zs));
			AlterCoords(_coords);
			SendCalculationResult();
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _psiDblF, oldName, newName);
			ChangeNameInFormula(ref _thetaDblF, oldName, newName);
			ChangeNameInFormula(ref _phiDblF, oldName, newName);
			ChangeNameInFormula(ref _xsDblF, oldName, newName);
			ChangeNameInFormula(ref _ysDblF, oldName, newName);
			ChangeNameInFormula(ref _zsDblF, oldName, newName);
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} using ({1},{2},{3}) rotated by ({4},{5},{5})", BaseToString(),
				_xs, _ys, _zs, _psi, _theta, _phi);
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_psiDblF, _thetaDblF, _phiDblF, _xsDblF, _ysDblF, _zsDblF}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string psi = String.Format("Psi: {0}", MathsGeo.GetAngleBounds(_psi, 12, false, "ψ"));
			string theta = String.Format("Theta: {0}", MathsGeo.GetAngleBounds(_theta, 12, false, "θ"));
			string phi = String.Format("Phi: {0}", MathsGeo.GetAngleBounds(_phi, 12, false, "φ"));
			return base.GetInfos(psi, theta, phi, lines);
		}
				
	}
	

	// ---------------------------------------------------------------------------

	
	/// <summary>
	/// Point milieu de deux autres.
	/// </summary>
	public class SpMidpoint : SpPointObject
	{
	
		protected SpPointObject _point1, _point2;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Midpoint"; } }
		
		/// <summary>
		/// Obtient le point 1.
		/// </summary>
		public SpPointObject Point1 { get { return _point1; } }
		
		/// <summary>
		/// Obtient le point 2.
		/// </summary>
		public SpPointObject Point2 { get { return _point2; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpMidpoint(string name, SpPointObject spt1, SpPointObject spt2) : base(name)
		{
			Alter(spt1, spt2);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject spt1, SpPointObject spt2)
		{
			_point1 = spt1; _point2 = spt2;
			EndAlterProcess(_point1, _point2);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			_coords = (_point1.Coordinates + _point2.Coordinates) / 2;
			AlterCoords(_coords);
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} between {1} and {2}", BaseToString(), _point1, _point2);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _point2}; }
		
	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Image d'un point.
	/// </summary>
	public class SpImagePoint : SpPointObject
	{
	
		protected SpPointObject _basePoint;
		protected SpVectorObject _vector;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Image point"; } }
		
		/// <summary>
		/// Point d'origine.
		/// </summary>
		public SpPointObject BasePoint { get { return _basePoint; } }
		
		/// <summary>
		/// Vecteur utilisé.
		/// </summary>
		public SpVectorObject Vector { get { return _vector; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpImagePoint(string name, SpPointObject spt, SpVectorObject vect) : base(name)
		{
			Alter(spt, vect);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject spt, SpVectorObject vect)
		{
			_basePoint = spt; _vector = vect;
			EndAlterProcess(_basePoint, _vector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			_coords = _basePoint.Coordinates + _vector.Coordinates;
			AlterCoords(_coords);
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} of {1} translate by {2}.", BaseToString(), _basePoint, _vector);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_basePoint, _vector}; }
		
	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Forme un barycentre.
	/// </summary>
	public class SpBarycenter : SpPointObject
	{
	
		protected WeightedPoint[] _wPoints;
		protected double _mass;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Barycenter"; } }
		
		/// <summary>
		/// Obtient la liste des points pondérés formant le système.
		/// </summary>
		public WeightedPoint[] WPoints { get { return _wPoints; } }
		
		/// <summary>
		/// Obtient la masse du système.
		/// </summary>
		public double Mass { get { return _mass; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpBarycenter(string name, params WeightedPoint[] wpoints) : base(name)
		{
			Alter(wpoints);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(params WeightedPoint[] wpoints)
		{
			_wPoints = wpoints;
			// Cherche les masterObjects:
			int l = _wPoints.Length;
			SpObject[] masters = new SpObject[l];
			for (int i=0; i<l; i++) { masters[i] = _wPoints[i].Point; }
			for (int i=0; i<l; i++)
				{ masters = masters.Concat(GetObjectsFromFormula(_wPoints[i].Weight)).ToArray(); }
			EndAlterProcess(masters);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Calcule les poids, et la masse, puis sort si la masse est nulle:
			_mass = 0; int l = _wPoints.Length; // Ne pas utiliser une boucle foreach avec les structures.
			for (int i=0; i<l; i++) { _mass += _wPoints[i].Weight.Recalculate(); }
			if (DoubleF.IsThereNan(_mass)) { SendCalculationResult(true, "A weight is not valid."); return; }
			if (_mass == 0) { SendCalculationResult(true, "Mass is null."); return; }
			// Calcule les coordonnées du barycentre directement à partir des coordonnées des points:
			_coords = new Coord3D();
			foreach (WeightedPoint wpt in _wPoints) // foreach et struct: on ne fait que lire
				{ _coords += (wpt.Weight.Value / _mass) * wpt.Point.Coordinates; }
			AlterCoords(_coords);
			SendCalculationResult();
		}
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			int l = _wPoints.Length;
			for (int i=0; i<l; i++) { ChangeNameInFormula(ref _wPoints[i].Weight, oldName, newName); }
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString(FormatText("{0} of {{{{{1}}}}}", BaseToString(), My.ArrayFunctions.Join(_wPoints, ";")));
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_wPoints}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string mass = FormatText("Mass: {0}", _mass);
			return base.GetInfos(mass, lines);
		}

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Un point sur une droite, une demi-droite, un segment.
	/// </summary>
	public class SpPointOnLine : SpPointObject
	{
	
		private DoubleF _tParamDblF;
		protected double _tParam;
		protected SpLine _baseLine;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Point on line"; } }
		
		/// <summary>
		/// Obtient la ligne sur laquelle se déplace le point.
		/// </summary>
		public SpLine BaseLine { get { return _baseLine; } }
		
		/// <summary>
		/// Obtient ou définit le paramètre t.
		/// </summary>
		public double TParam { get { return _tParam; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointOnLine(string name, SpLine line, DoubleF tParam) : base(name)
		{
			Alter(line, tParam);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpLine line, DoubleF tParam)
		{
			_baseLine = line; _tParamDblF = tParam;
			EndAlterProcess(_baseLine, GetObjectsFromFormula(_tParamDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule t:
			_tParam = _tParamDblF.Recalculate();
			if (_tParamDblF.IsNaN) { SendCalculationResult(true, "Parameter t is not valid."); return; }
			// Limite la valeur de t si c'est un segment ou une demi-droite:
			if (_baseLine is SpSegment) { _tParam = (_tParam < 0 ? 0 : (_tParam > 1 ? 1 : _tParam)); }
			if (_baseLine is SpRay) { _tParam = (_tParam < 0 ? 0 : _tParam); }
			// Définit le coordonnées du point par translation:
			_coords = _baseLine.Point1.Coordinates + _tParam * _baseLine.Vector.Coordinates;
			AlterCoords(_coords);
			SendCalculationResult();
		}
		
		/// <summary>
		/// Modifie la valeur du paramètre t, si possible.
		/// </summary>
		public virtual void AlterTParam(double t)
			{ _tParamDblF.Value = t; }
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _tParamDblF, oldName, newName);
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} on {1}", BaseToString(), _baseLine);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_baseLine, _tParamDblF}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string tparam =FormatText("Parameter t = {0}", _tParam);
			return base.GetInfos(tparam, lines);
		}
		
	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Point d'intersection entre deux droites.
	/// </summary>
	public class SpLinesIntersection : SpPointObject
	{
	
		protected SpLine _line1, _line2;
		protected double _t, _tp;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Lines intersection"; } }
		
		/// <summary>
		/// Obtient la droite 1.
		/// </summary>
		public SpLine Line1 { get { return _line1; } }
		
		/// <summary>
		/// Obtient la droite 2.
		/// </summary>
		public SpLine Line2 { get { return _line2; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpLinesIntersection(string name, SpLine line1, SpLine line2) : base(name)
		{
			Alter(line1, line2);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpLine line1, SpLine line2)
		{
			_line1 = line1; _line2 = line2;
			EndAlterProcess(_line1, _line2);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les coordonnées depuis GeoFunctions:
			double? min1 = (_line1 is SpSegment || _line1 is SpRay ? 0 : (double?)null);
			double? max1 = (_line1 is SpSegment ? 1 : (double?)null);
			double? min2 = (_line2 is SpSegment || _line2 is SpRay ? 0 : (double?)null);
			double? max2 = (_line2 is SpSegment ? 1 : (double?)null);
			if (!GeoFunctions.GetLinesInterCoords(_line1.Point1.Coordinates, _line1.Vector.Coordinates,
				_line2.Point1.Coordinates, _line2.Vector.Coordinates, min1, max1, min2, max2, out _coords, out _t, out _tp))
					{ SendCalculationResult(true, "No intersection found."); return; };
			AlterCoords(_coords);
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} between {1} and {2}",	BaseToString(), _line1, _line2);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_line1, _line2}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string tparam = FormatText("Parameter t = {0}, Parameter t' = {1}", _t, _tp);
			return base.GetInfos(tparam, lines);
		}

	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Point sur une sphère.
	/// </summary>
	public class SpPointOnSphere : SpPointObject
	{
	
		private DoubleF _lambdaDblF, _phiDblF;
		protected double _lambda, _phi;
		protected SpSphere _sphere;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Point on sphere"; } }
		
		/// <summary>
		/// Obtient la sphère.
		/// </summary>
		public virtual SpSphere Sphere { get { return _sphere; } }
		
		/// <summary>
		/// Obtient l'angle, en radian.
		/// </summary>
		public virtual double Lambda { get { return _lambda; } }

		/// <summary>
		/// Obtient l'angle, en radian.
		/// </summary>
		public virtual double Phi { get { return _phi; } }

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointOnSphere(string name, SpSphere sphere, DoubleF lambda, DoubleF phi) : base(name)
		{
			Alter(sphere, lambda, phi);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpSphere sphere, DoubleF lambda, DoubleF phi)
		{
			_sphere = sphere; _phiDblF = phi; _lambdaDblF = lambda;
			EndAlterProcess(_sphere, GetObjectsFromFormula(_lambdaDblF), GetObjectsFromFormula(_phiDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule les angles, et les normalise:
			_lambda = _lambdaDblF.Recalculate(); _phi = _phiDblF.Recalculate();
			MathsGeo.NormalizeGeographicAngles(ref _lambda, ref _phi);
			if (DoubleF.IsThereNan(_lambda, _phi)) { SendCalculationResult(true, "Angles not valid."); return; }
			// Trouve les coordonnées cartésienne à partir des coordonnées polaires, pour une sphère de centre (0,0,0)
			// et de rayon _radius, puis translate le point trouvé par le vecteur de (0,0,0) à _center:
			_coords = My.MathsGeo.FromPolar(_sphere.Radius, _lambda, _phi) + _sphere.Center.Coordinates;
			AlterCoords(_coords);
			SendCalculationResult();
		}

		/// <summary>
		/// Modifie les valeurs des angles, si possible.
		/// </summary>
		public virtual void AlterAngles(double lambda, double phi)
		{
			_lambdaDblF.Value = lambda; _phiDblF.Value = phi;
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _lambdaDblF, oldName, newName);
			ChangeNameInFormula(ref _phiDblF, oldName, newName);
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} on sphere {1}", BaseToString(), _sphere);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_sphere, _lambdaDblF, _phiDblF}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string lambdaphi = String.Format("Lambda: {0}, Phi : {1}", MathsGeo.GetAngleBounds(_lambda, 12, false, "λ"),
				MathsGeo.GetAngleBounds(_phi, 12, false, "φ"));
			return base.GetInfos(lambdaphi, lines);
		}		

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Projeté orthogonal d'un point sur une droite.
	/// </summary>
	public class SpOrthoProjPointOnLine : SpPointObject
	{
	
		protected SpPointObject _basePoint;
		protected SpLine _line;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Orthogonal projection on line"; } }
		
		/// <summary>
		/// Obtient le point à projeter.
		/// </summary>
		public SpPointObject BasePoint { get { return _basePoint; } }
		
		/// <summary>
		/// Obtient la droite.
		/// </summary>
		public SpLine Line { get { return _line; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpOrthoProjPointOnLine(string name, SpPointObject basePoint, SpLine line) : base(name)
		{
			Alter(basePoint, line);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject basePoint, SpLine line)
		{
			_basePoint = basePoint; _line = line;
			EndAlterProcess(_basePoint, _line);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Utilise la formule. Si le vecteur directeur de la droite est nul, la droite n'est pas définie,
			// donc on n'arrive jamais ici..
			_coords = GeoFunctions.GetOrthoProjPointOnLineCoords(_basePoint.Coordinates, _line.Point1.Coordinates,
					_line.Vector.Coordinates);
			AlterCoords(_coords);
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} of {1} on {2}", BaseToString(), _basePoint, _line);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_basePoint, _line}; }
		
	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Point d'intersection entre une droite et une sphere.
	/// </summary>
	public class SpLineSphereIntersection : SpPointObject
	{
	
		protected SpLine _line;
		protected SpSphere _sphere;
		protected bool _inter1;
		protected int _solNb;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Line and sphere intersection"; } }
		
		/// <summary>
		/// Obtient la droite.
		/// </summary>
		public SpLine Line { get { return _line; } }
		
		/// <summary>
		/// Obtient la sphère.
		/// </summary>
		public SpSphere Sphere { get { return _sphere; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpLineSphereIntersection(string name, SpLine line, SpSphere sphere, bool inter1) : base(name)
		{
			Alter(line, sphere, inter1);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpLine line, SpSphere sphere, bool inter1)
		{
			_line = line; _sphere = sphere; _inter1 = inter1;
			EndAlterProcess(_line, _sphere);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Cherdche les points d'intersection:
			Coord3D pt1, pt2;
			double? min = (_line is SpSegment || _line is SpRay ? 0 : (double?)null);
			double? max = (_line is SpSegment ? 1 : (double?)null);
			_solNb = GeoFunctions.GetLineSphereInterCoords(_sphere.Center.Coordinates, _sphere.Radius, _line.Point1.Coordinates,
				_line.Vector.Coordinates, min, max, out pt1, out pt2);
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
			return this.MakeToString("{0} between {1} and {2}",	BaseToString(), _line, _sphere);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_line, _sphere, _inter1}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string nb = String.Format("Total number of intersections: {0}", _solNb);
			return base.GetInfos(nb, lines);
		}

	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Point sur une fonction de l'espace.
	/// </summary>
	public class SpPointOnFunction2 : SpPointObject
	{
	
		protected bool _isLimited;
		protected SpFunction2 _function;
		private DoubleF _xDblF, _yDblF;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Point on function2"; } }
		
		/// <summary>
		/// Obtient la fonction
		/// </summary>
		public SpFunction2 Function { get { return _function; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPointOnFunction2(string name, SpFunction2 func, DoubleF x, DoubleF y, bool isLimited) : base(name)
		{
			Alter(func, x, y, isLimited);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpFunction2 func, DoubleF x, DoubleF y, bool isLimited)
		{
			_function = func; _xDblF = x; _yDblF = y; _isLimited = isLimited;
			EndAlterProcess(_function, GetObjectsFromFormula(_xDblF), GetObjectsFromFormula(_yDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule x et y:
			_x = _xDblF.Recalculate(); _y = _yDblF.Recalculate();
			if (DoubleF.IsThereNan(_x, _y)) { SendCalculationResult(true, "Value is not valid."); return; }
			if (_isLimited && _x < _function.MinX) { _x = _function.MinX; }
			if (_isLimited && _x > _function.MaxX) { _x = _function.MaxX; }
			if (_isLimited && _y < _function.MinY) { _y = _function.MinY; }
			if (_isLimited && _y > _function.MaxY) { _y = _function.MaxY; }
			// Obtient la valeur z:
			_z = _function.GetFunctionValue(_x, _y);
			if (DoubleF.IsThereNan(_z)) { SendCalculationResult(true, "Function returns an error."); return; }
			CalculateCoordsStruct();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Modifie la valeur de X.
		/// </summary>
		public virtual void AlterX(double x)
			{ _xDblF.Value = x; }

		/// <summary>
		/// Modifie la valeur de Y.
		/// </summary>
		public virtual void AlterY(double y)
			{ _yDblF.Value = y; }

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _xDblF, oldName, newName);
			ChangeNameInFormula(ref _yDblF, oldName, newName);
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} on {1}",	BaseToString(), _function);
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_function, _xDblF, _yDblF, _isLimited}; }

	}
	
		
	#endregion OBJETS DE L'ESPACE



}
