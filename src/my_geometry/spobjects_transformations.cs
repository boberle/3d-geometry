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
	/// Classe de base pour les transformations.
	/// </summary>
	public class SpTransformationObject : SpObject
	{
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Tranformation"; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		protected SpTransformationObject(string name) : base(name)
		{ ShowName = false; }
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
			{ SendCalculationResult(); }
		
		/// <summary>
		/// Transforme un point, et retourne true si elle a réussit, false, dans le cas contraire. Echec si transformations contient des transformations qui ne sont pas de l'espace, mais du plan.
		/// </summary>
		public static bool TransformPoints(SpTransformationObject[] transformations, ref Coord3D[] pts)
		{
			int l = pts.Length; SpTranslation transl; SpAxialRotation axRot; SpHomothety homoth;
			SpRotation rot; SpAxialSymmetry axSym;
			foreach (SpTransformationObject t in transformations)
			{
				if ((transl = t as SpTranslation) != null)
				{
					Coord3D vec = transl.Vector.Coordinates;
					for (int i=0; i<l; i++) { pts[i] = pts[i] + vec; }
				}
				else if ((axRot = t as SpAxialRotation) != null)
				{
					Coord3D axeVec = axRot.Vector.Coordinates, axeOrigin = axRot.Vector.Point1.Coordinates;
					double α = axRot.Alpha;
					for (int i=0; i<l; i++) { pts[i] = GeoFunctions.GetAxialRotationPtCoords(axeOrigin, axeVec, pts[i], α); }
				}
				else if ((homoth = t as SpHomothety) != null)
				{
					Coord3D center = homoth.Center.Coordinates; double ratio = homoth.Ratio;
					for (int i=0; i<l; i++) { pts[i] = center + ratio * (pts[i] - center); }
				}
				else if ((rot = t as SpRotation) != null)
				{
					pts = GeoFunctions.GetEulerRotatedPtCoords(rot.Psi, rot.Theta, rot.Phi, rot.Center.Coordinates, pts);
				}
				else if ((axSym = t as SpAxialSymmetry) != null)
				{
					Coord3D axisOrigin = axSym.Vector.Point1.Coordinates, axisVec = axSym.Vector.Coordinates;
					for (int i=0; i<l; i++)
						{ pts[i] = pts[i] + 2 * (GeoFunctions.GetOrthoProjPointOnLineCoords(pts[i], axisOrigin, axisVec) - pts[i]); }
				}
				else
				{
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		/// Transforme un point, et retourne true si elle a réussit, false, dans le cas contraire. Echec si transformations contient des transformations qui ne sont pas du plan. De même si la transformation du plan est basée sur un autre plan que celui passé en argument.
		/// </summary>
		public static bool TransformPoints(SpTransformationObject[] transformations, ref Coord2D[] pts, SpPlaneObject plane)
		{
			int l = pts.Length; SpRotationOnPlane planeRot; SpTranslation transl; SpHomothety homoth;
			SpAxialSymmetry axSym;
			foreach (SpTransformationObject t in transformations)
			{
				if ((transl = t as SpTranslation) != null)
				{
					// Vérifie que le vecteur de la translation est coplanaire avec ceux du plan:
					if (!GeoFunctions.AreCoplanar(plane.XVector.Coordinates, plane.YVector.Coordinates, transl.Vector.Coordinates))
						{ return false; }
					Coord2D vec = plane.To2D(transl.Vector.Point2.Coordinates - transl.Vector.Point1.Coordinates);
					for (int i=0; i<l; i++) { pts[i] = pts[i] + vec; }
				}
				else if ((planeRot = t as SpRotationOnPlane) != null)
				{
					if (planeRot.Plane != plane) { return false; }
					Coord2D center = planeRot.Center.CoordinatesOnPlane;
					double α = planeRot.Alpha;
					for (int i=0; i<l; i++) { pts[i] = GeoFunctions.GetRotatedPtCoords(center, pts[i], α); }
				}
				else if ((homoth = t as SpHomothety) != null)
				{
					// Vérifie que le centre de l'homoth est sur le plan:
					Coord2D center = plane.To2D(homoth.Center.Coordinates); double ratio = homoth.Ratio;
					if (center.Empty) { return false; }
					for (int i=0; i<l; i++) { pts[i] = center + ratio * (pts[i] - center); }
				}
				else if ((axSym = t as SpAxialSymmetry) != null)
				{
					// Vérifie que le vecteur de la transformation est coplanaire avec ceux du plan:
					if (!GeoFunctions.AreCoplanar(plane.XVector.Coordinates, plane.YVector.Coordinates, axSym.Vector.Coordinates))
						{ return false; }
					// Vérifie que le vecteur est sur le plan:
					Coord2D axisOrigin = plane.To2D(axSym.Vector.Point1.Coordinates);
					Coord2D axisVec = plane.To2D(axSym.Vector.Point2.Coordinates - axSym.Vector.Point1.Coordinates);
					for (int i=0; i<l; i++)
						{ pts[i] = pts[i] + 2 * (GeoFunctions.GetOrthoProjPointOnLineCoords(pts[i], axisOrigin, axisVec) - pts[i]); }
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Transforme une longueur, par exemple le rayon d'un cercle par le rapport d'une homothétie. Retourne true si réussit. Il n'y a pas d'échec si les transformations ne transforme pas de longueur.
		/// </summary>
		public static bool TransformLength(SpTransformationObject[] transformations, ref double length)
		{
			SpHomothety homoth;
			foreach (SpTransformationObject t in transformations)
			{
				if ((homoth = t as SpHomothety) != null)
				{
					length *= homoth.Ratio;
				}
			}
			return true;
		}
		
		/// <summary>
		/// Obtient une chaîne du genre "using transformations...".
		/// </summary>
		public static string GetUsingTransf(SpTransformationObject[] transformations)
		{
			return String.Format("using transformation(s) {0}", My.ArrayFunctions.Join(
				Array.ConvertAll(transformations, delegate(SpTransformationObject o) { return o.Name; }), ","));
		}
		
		/// <summary>
		/// Obtient la liste des transformations.
		/// </summary>
		public static string GetTrGetInfos(SpTransformationObject[] transformations)
		{
			StringBuilder sb = new StringBuilder("Transformations:");
			int l = transformations.Length;
			for (int i=0; i<l; i++) { sb.AppendFormat("\n   ({0}) {1}", i, transformations[i].ToString()); }
			return sb.ToString();
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[0]; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			StringBuilder sb = new StringBuilder("Transformed objects:"); int c = 0;
			SpObjectsCollection coll = SpObjectsCollection.GetInstance();
			foreach (SpObject o in coll) {
				if (o is ITransformedObject && ((ITransformedObject)o).Transformations.Contains(this))
					{ sb.AppendFormat("\n   ({0}) {1}", c++, o.ToString()); } }
			if (c == 0) { sb.AppendFormat("\n   None"); }
			return base.GetInfos(sb.ToString(), lines);
		}
		
	}
	

	#endregion OBJETS DE BASE
	




	// ---------------------------------------------------------------------------
	// TRANSFORMATIONS
	// ---------------------------------------------------------------------------




	#region TRANSFORMATIONS




	/// <summary>
	/// Translation dans l'espace.
	/// </summary>
	public class SpTranslation : SpTransformationObject
	{
	
		protected SpVectorObject _vector;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Translation"; } }
		
		/// <summary>
		/// Obtient le vecteur.
		/// </summary>
		public SpVectorObject Vector { get { return _vector; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpTranslation(string name, SpVectorObject vector) : base(name)
		{
			Alter(vector);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpVectorObject vector)
		{
			_vector = vector;
			EndAlterProcess(_vector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
			{ SendCalculationResult(); }

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("using {0}", _vector);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_vector}; }
		
	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Rotation de l'espace.
	/// </summary>
	public class SpAxialRotation : SpTransformationObject
	{
	
		protected SpLineObject _baseLineObj;
		protected SpVectorObject _vector;
		protected DoubleF _alphaDblF;
		protected double _alpha;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Axial rotation"; } }
		
		/// <summary>
		/// Obtient la ligne de base (vecteur, droite, etc.).
		/// </summary>
		public SpLineObject BaseLineObject { get { return _baseLineObj; } }
		
		/// <summary>
		/// Obtient le vecteur utilisé.
		/// </summary>
		public SpVectorObject Vector { get { return _vector; } }
	
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public double Alpha { get { return _alpha; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpAxialRotation(string name, SpLineObject lineObj, DoubleF alpha) : base(name)
		{
			Alter(lineObj, alpha);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpLineObject lineObj, DoubleF alpha)
		{
			_baseLineObj = lineObj; _alphaDblF = alpha;
			if (_baseLineObj is SpVectorObject) { _vector = (SpVectorObject)_baseLineObj; }
			else { _vector = ((SpLine)_baseLineObj).Vector; }
			EndAlterProcess(_baseLineObj, GetObjectsFromFormula(_alphaDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets:
			if (_baseLineObj is SpVectorObject) { _vector = (SpVectorObject)_baseLineObj; }
			else { _vector = ((SpLine)_baseLineObj).Vector; }
			// Recalcule l'angle:
			if (DoubleF.IsThereNan(_alpha = _alphaDblF.Recalculate()))
				{ SendCalculationResult(true, "Alpha not valid."); return; }
			SendCalculationResult();
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
			return this.MakeToString("using axis {0}", _baseLineObj);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_baseLineObj, _alphaDblF}; }
		
	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Rotation du plan.
	/// </summary>
	public class SpRotationOnPlane : SpTransformationObject
	{
	
		protected SpPointOnPlaneObject _center;
		protected DoubleF _alphaDblF;
		protected double _alpha;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Rotation on plane"; } }
		
		/// <summary>
		/// Obtient le centre.
		/// </summary>
		public SpPointOnPlaneObject Center { get { return _center; } }
	
		/// <summary>
		/// Obtient l'angle alpha.
		/// </summary>
		public double Alpha { get { return _alpha; } }
		
		/// <summary>
		/// Obtient le plan.
		/// </summary>
		public SpPlaneObject Plane { get { return _center.Plane; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpRotationOnPlane(string name, SpPointOnPlaneObject center, DoubleF alpha) : base(name)
		{
			Alter(center, alpha);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject center, DoubleF alpha)
		{
			_center = center; _alphaDblF = alpha;
			EndAlterProcess(_center, GetObjectsFromFormula(_alphaDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			if (DoubleF.IsThereNan(_alpha = _alphaDblF.Recalculate()))
				{ SendCalculationResult(true, "Alpha is not valid."); return; }
			SendCalculationResult();
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
			return this.MakeToString("({0},{1}) on {2}", _center, _alpha, _center.Plane);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_center, _alphaDblF}; }
				
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
	/// Rotation dans l'espace.
	/// </summary>
	public class SpRotation : SpTransformationObject
	{
	
		protected SpPointObject _center;
		protected DoubleF _psiDblF, _thetaDblF, _phiDblF;
		protected double _psi, _theta, _phi;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Rotation"; } }
		
		/// <summary>
		/// Obtient le centre.
		/// </summary>
		public SpPointObject Center { get { return _center; } }
	
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
		protected SpRotation(string name) : base(name)
			{ ; }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpRotation(string name, SpPointObject center, DoubleF psi, DoubleF theta, DoubleF phi) : this(name)
		{
			Alter(center, psi, theta, phi);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject center, DoubleF psi, DoubleF theta, DoubleF phi)
		{
			_center = center; _thetaDblF = theta; _psiDblF = psi; _phiDblF = phi;
			EndAlterProcess(_center, GetObjectsFromFormula(_psiDblF), GetObjectsFromFormula(_thetaDblF),
				GetObjectsFromFormula(_phiDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			if (DoubleF.IsThereNan(_theta = _thetaDblF.Recalculate(), _phi = _phiDblF.Recalculate(), _psi = _psiDblF.Recalculate()))
				{ SendCalculationResult(true, "Psi, theta or phi not valid."); return; }
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
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("({0},{1},{2},{3})", _center.Name, _psi, _theta, _phi);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_center, _psiDblF, _thetaDblF, _phiDblF}; }
				
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string psi = "Psi: " + MathsGeo.GetAngleBounds(_psi, 12, false, "ψ");
			string theta = "Theta: " + MathsGeo.GetAngleBounds(_theta, 12, false, "θ");
			string phi = "Phi: " + MathsGeo.GetAngleBounds(_phi, 12, false, "φ");
			return base.GetInfos(psi, theta, phi, lines);
		}
		
	}
	
	
	// ---------------------------------------------------------------------------
	
		
	/// <summary>
	/// Rotation d'un solide autour de son centre.
	/// </summary>
	public class SpRotationOfSolid : SpRotation
	{
	
		protected SpSolid _solid;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Rotation of solid"; } }
		
		/// <summary>
		/// Obtient le solid.
		/// </summary>
		public SpSolid Solid { get { return _solid; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpRotationOfSolid(string name, SpSolid solid, DoubleF psi, DoubleF theta, DoubleF phi) : base(name)
		{
			Alter(solid, psi, theta, phi);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpSolid solid, DoubleF psi, DoubleF theta, DoubleF phi)
		{
			_center = solid.Center; _thetaDblF = theta; _psiDblF = psi; _phiDblF = phi; _solid = solid;
			EndAlterProcess(_solid, GetObjectsFromFormula(_psiDblF), GetObjectsFromFormula(_thetaDblF),
				GetObjectsFromFormula(_phiDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets, puis appelle la base:
			_center = _solid.Center;
			base.CalculateNumericData();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_solid, _psiDblF, _thetaDblF, _phiDblF}; }
				
	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Homothétie.
	/// </summary>
	public class SpHomothety : SpTransformationObject
	{
	
		protected SpPointObject _center;
		protected DoubleF _ratioDblF;
		protected double _ratio;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Homothety"; } }
		
		/// <summary>
		/// Obtient le centre.
		/// </summary>
		public SpPointObject Center { get { return _center; } }
		
		/// <summary>
		/// Obtient le rapport.
		/// </summary>
		public double Ratio{ get { return _ratio; } }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpHomothety(string name) : base(name)
			{ ; }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpHomothety(string name, SpPointObject center, DoubleF ratio) : this(name)
		{
			Alter(center, ratio);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject center, DoubleF ratio)
		{
			_center = center; _ratioDblF = ratio;
			EndAlterProcess(_center, GetObjectsFromFormula(_ratioDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule le rapport:
			if (DoubleF.IsThereNan(_ratio = _ratioDblF.Recalculate()))
				{ SendCalculationResult(true, "Ratio not valid."); return; }
			SendCalculationResult();
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _ratioDblF, oldName, newName);
		}
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("({0},{1})", _center.Name, _ratio);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_center, _ratioDblF}; }
		
	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Symétrie axiale.
	/// </summary>
	public class SpAxialSymmetry : SpTransformationObject
	{
	
		protected SpLineObject _baseLineObj;
		protected SpVectorObject _vector;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Axial symmetry"; } }
		
		/// <summary>
		/// Obtient la ligne de base (vecteur, droite, etc.).
		/// </summary>
		public SpLineObject BaseLineObject { get { return _baseLineObj; } }
		
		/// <summary>
		/// Obtient le vecteur utilisé.
		/// </summary>
		public SpVectorObject Vector { get { return _vector; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpAxialSymmetry(string name, SpLineObject lineObj) : base(name)
		{
			Alter(lineObj);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpLineObject lineObj)
		{
			_baseLineObj = lineObj;
			if (_baseLineObj is SpVectorObject) { _vector = (SpVectorObject)_baseLineObj; }
			else { _vector = ((SpLine)_baseLineObj).Vector; }
			EndAlterProcess(_baseLineObj);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets:
			if (_baseLineObj is SpVectorObject) { _vector = (SpVectorObject)_baseLineObj; }
			else { _vector = ((SpLine)_baseLineObj).Vector; }
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("using axis {0}", _baseLineObj);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_baseLineObj}; }
		
	}



	#endregion TRANSFORMATIONS


	
}
