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
	// OBJETS DE BASE 2D ET 3D
	// ---------------------------------------------------------------------------




	#region OBJETS DE BASE 2D ET 3D




	/// <summary>
	/// Classe de base pour les objets liant deux points par un trait.
	/// </summary>
	public class SpLineObject : SpPenObject
	{
	
		protected SpPointObject _point1, _point2;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Line"; } }
		
		/// <summary>
		/// Obtient le point 1.
		/// </summary>
		public SpPointObject Point1 { get { return _point1; } }
		
		/// <summary>
		/// Obtient le point 2.
		/// </summary>
		public SpPointObject Point2 { get { return _point2; } }		
		
		/// <summary>
		/// Coordonnées dans le repère du form du point extrême du segment représentant la droite dans la zone visible du form, situé sur le bord du form pour faire croire que la droite "sort" du form.
		/// </summary>
		internal PointF PtOnWin1 { get; set; }
		
		/// <summary>
		/// Coordonnées dans le repère du form du point extrême du segment représentant la droite dans la zone visible du form, situé sur le bord du form pour faire croire que la droite "sort" du form.
		/// </summary>
		internal PointF PtOnWin2 { get; set; }

		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpLineObject(string name) : base(name)
			{ _labelOriginParam = 0.5; }
		
		protected override void CalculateNumericData()
			{ SendCalculationResult(); }

		/// <summary>
		/// Calcule l'origine du label. Le paramètre est le coefficient multiplicateur du vecteur formé par les deux points.
		/// </summary>
		protected override void CalculateLabelOrigin()
		{
			_labelOrigin = _point1.Coordinates + _labelOriginParam * (_point2.Coordinates - _point1.Coordinates);
		}

		/// <summary>
		/// Retourne une chaîne commune aux points décrivant les coordonnées du point.
		/// </summary>
		public virtual string BaseToString()
		{
			return FormatText("through {0} and {1}", _point1, _point2);
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString(BaseToString());
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[0]; }
		
	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Classe de base pour les vecteurs.
	/// </summary>
	public class SpVectorObject : SpLineObject
	{
	
		protected double _norm, _x, _y, _z;
		protected Coord3D _coords;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Vector"; } }
		
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
		/// Obtient une structure de coordonnées.
		/// </summary>
		public Coord3D Coordinates { get { return _coords; } }
		
		/// <summary>
		/// Obtient la norme du vecteur.
		/// </summary>
		public double Norm { get { return _norm; } }
		
		/// <summary>
		/// Obtient si le vecteur est nul.
		/// </summary>
		public bool IsNul { get { return _coords.IsNul; } }
		
		/// <summary>
		/// Constructeur protégé. Modifie le cap du Pen.
		/// </summary>
		protected SpVectorObject(string name) : base(name)
		{
			_pen.CustomEndCap = DrawingArea.CustomArrowCap;
		}
	
		/// <summary>
		/// Constructeur. Le vecteur est défini par un point (pour le dessin) et des coordonnées.
		/// </summary>
		internal SpVectorObject(string name, SpPointObject spt1, double x, double y, double z) : this(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, x, y, z);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		internal void Alter(SpPointObject spt1, double x, double y, double z)
		{
			_point1 = spt1; _x = x; _y = y; _z = z;
			EndAlterProcess(_point1, null, _point2);
		}

		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Calcule les coordonnées du point 2:
			CalculateCoordsStruct();
			_point2.AlterCoords(_point1.Coordinates + _coords);
			_point2.Recalculate(true);
			// Calcule la norme:
			_norm = _coords.GetNorm();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Calcul la structure de coordonnées. Toujours appeler cette méthode à la fin des calculs, ou bien appeler le CalculateNumData.
		/// </summary>
		protected void CalculateCoordsStruct()
			{ _coords = new Coord3D(_x, _y, _z); }
				
		/// <summary>
		/// Modifie X, Y et Z.
		/// </summary>
		internal virtual void AlterCoords(Coord3D coords)
			{ _x = coords.X; _y = coords.Y; _z = coords.Z; }

		/// <summary>
		/// Retourne une chaîne commune aux points décrivant les coordonnées du point.
		/// </summary>
		public override string BaseToString()
		{
			return FormatText("({0},{1},{2}) from {3} to {4}", _x, _y, _z, _point1, _point2);
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString(BaseToString());
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _x, _y, _z}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet:
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string norm = FormatText("Norm: {0}", _norm);
			return base.GetInfos(norm, lines);
		}
		
	}
	
	

	#endregion OBJETS DE BASE 2D ET 3D
	



	// ---------------------------------------------------------------------------
	// OBJETS DE L'ESPACE
	// ---------------------------------------------------------------------------




	#region OBJETS DE L'ESPACE




	/// <summary>
	/// Vecteur défini par un point et des coordonnées.
	/// </summary>
	public class SpVectorUsingCoords : SpVectorObject
	{
	
		private DoubleF _xDblF, _yDblF, _zDblF;
		
		/// <summary>
		/// Constructeur. Le vecteur est défini par un point (pour le dessin) et des coordonnées.
		/// </summary>
		public SpVectorUsingCoords(string name, SpPointObject spt1, DoubleF x, DoubleF y, DoubleF z) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, x, y, z);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpPointObject spt1, DoubleF x, DoubleF y, DoubleF z)
		{
			_point1 = spt1; _xDblF = x; _yDblF = y; _zDblF = z;
			EndAlterProcess(_point1, GetObjectsFromFormula(_xDblF), GetObjectsFromFormula(_yDblF),
				GetObjectsFromFormula(_zDblF), null, _point2);
		}

		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule les coordonnées:
			_x = _xDblF.Recalculate(); _y = _yDblF.Recalculate(); _z = _zDblF.Recalculate();
			if (DoubleF.IsThereNan(_x, _y, _z)) { SendCalculationResult(true, "Coordinates are not valid."); return; }
			CalculateCoordsStruct();
			// Calcule les coordonnées du deuxième point:
			_point2.AlterCoords(_point1.Coordinates + _coords);
			_point2.Recalculate(true);
			// Calcul la norme:
			_norm = _coords.GetNorm();
			SendCalculationResult();
		}
		
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
			{ return new object[]{_point1, _xDblF, _yDblF, _zDblF}; }
		
	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Vecteur défini par deux points (dans l'ordre).
	/// </summary>
	public class SpVectorUsingPoints : SpVectorObject
	{
	
		/// <summary>
		/// Constructeur. Le vecteur est défini par deux points.
		/// </summary>
		public SpVectorUsingPoints(string name, SpPointObject spt1, SpPointObject spt2) : base(name)
		{
			Alter(spt1, spt2);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpPointObject spt1, SpPointObject spt2)
		{
			_point1 = spt1; _point2 = spt2;
			EndAlterProcess(_point1, _point2);
		}

		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Calcule les coordonnées:
			_coords = _point2.Coordinates - _point1.Coordinates;
			AlterCoords(_coords);
			// Calcule la norme:
			_norm = _coords.GetNorm();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _point2}; }
		
	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Vecteur défini par deux points (dans l'ordre) et une origine.
	/// </summary>
	public class SpVectorUsingPointsAndOrigin : SpVectorObject
	{
	
		protected SpPointObject _basePt1, _basePt2;
		
		/// <summary>
		/// Obtient le premier point du vecteur de base.
		/// </summary>
		public SpPointObject BasePoint1 { get { return _basePt1; } }
		
		/// <summary>
		/// Obtient le deuxième point du vecteur de base.
		/// </summary>
		public SpPointObject BasePoint2 { get { return _basePt2; } }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpVectorUsingPointsAndOrigin(string name) : base(name)
			{ ; }
	
		/// <summary>
		/// Constructeur. Le vecteur est défini par deux points.
		/// </summary>
		public SpVectorUsingPointsAndOrigin(string name, SpPointObject spt1, SpPointObject basePt1, SpPointObject basePt2) : this(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, basePt1, basePt2);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpPointObject spt1, SpPointObject basePt1, SpPointObject basePt2)
		{
			_point1 = spt1; _basePt1 = basePt1; _basePt2 = basePt2;
			EndAlterProcess(_point1, _basePt1, _basePt2, null, _point2);
		}

		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Calcule les coordonnées du point 2:
			_coords = _basePt2.Coordinates - _basePt1.Coordinates;
			AlterCoords(_coords);
			_point2.AlterCoords(_point1.Coordinates + _coords);
			_point2.Recalculate(true);
			// Calcule la norme:
			_norm = _coords.GetNorm();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} based on {1} and {2}", BaseToString(), _basePt1, _basePt2);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _basePt1, _basePt2}; }
		
	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Vecteur défini par deux points (dans l'ordre), et un coefficient.
	/// </summary>
	public class SpVectorUsingPointsAndCoeff : SpVectorUsingPointsAndOrigin
	{
	
		private DoubleF _coeffDblF;
		protected double _coeff;
		
		/// <summary>
		/// Obtient le coefficient multiplicateur du vecteur.
		/// </summary>
		public double Coefficient { get { return _coeff; } }
		
		/// <summary>
		/// Constructeur. Le vecteur est défini par deux points.
		/// </summary>
		public SpVectorUsingPointsAndCoeff(string name, DoubleF coeff, SpPointObject basePt1, SpPointObject basePt2) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(coeff, basePt1, basePt2);
		}

		/// <summary>
		/// Constructeur. Le vecteur est défini par deux points.
		/// </summary>
		public SpVectorUsingPointsAndCoeff(string name, SpPointObject spt1, DoubleF coeff, SpPointObject basePt1, SpPointObject basePt2) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, coeff, basePt1, basePt2);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(DoubleF coeff, SpPointObject basePt1, SpPointObject basePt2)
		{
			_coeffDblF = coeff; _point1 = _basePt1 = basePt1; _basePt2 = basePt2;
			EndAlterProcess(_basePt1, _basePt2, GetObjectsFromFormula(_coeffDblF), null, _point2);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpPointObject spt1, DoubleF coeff, SpPointObject basePt1, SpPointObject basePt2)
		{
			_coeffDblF = coeff; _point1 = spt1; _basePt1 = basePt1; _basePt2 = basePt2;
			EndAlterProcess(_point1, _basePt1, _basePt2, GetObjectsFromFormula(_coeffDblF), null, _point2);
		}

		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule les coordonnées:
			_coeff = _coeffDblF.Recalculate();
			if (_coeffDblF.IsNaN) { SendCalculationResult(true, "Coefficient is not valid."); return; }
			_coords = _coeff * (_basePt2.Coordinates - _basePt1.Coordinates);
			AlterCoords(_coords);
			// Calcule les coordonnées du point 2:
			_point2.AlterCoords(_point1.Coordinates + _coords);
			_point2.Recalculate(true);
			// Calcule la norme:
			_norm = _coords.GetNorm();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
			{ ChangeNameInFormula(ref _coeffDblF, oldName, newName); }

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} based on {1} and {2}, using coeff {3}", BaseToString(), _basePt1, _basePt2, _coeff);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
		{
			if (_point1 == _basePt1) {
				return new object[]{_coeff, _basePt1, _basePt2}; }
			else {
				return new object[]{_point1, _coeff, _basePt1, _basePt2}; }
		}
		
	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Vecteur défini par un réel multiplicateur et un autre vecteur.
	/// </summary>
	public class SpVectorUsingMultiply : SpVectorObject
	{
	
		private DoubleF _coeffDblF;
		protected double _coeff;
		protected SpVectorObject _baseVector;
		
		/// <summary>
		/// Obtient le vecteur de base.
		/// </summary>
		public SpVectorObject BaseVector { get { return _baseVector; } }
		
		/// <summary>
		/// Obtient le coefficient multiplicateur du vecteur.
		/// </summary>
		public double Coefficient { get { return _coeff; } }
	
		/// <summary>
		/// Constructeur. Le vecteur est défini par un réel et un autre vecteur.
		/// </summary>
		public SpVectorUsingMultiply(string name, SpPointObject spt1, DoubleF coeff, SpVectorObject vec) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, coeff, vec);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpPointObject spt1, DoubleF coeff, SpVectorObject vec)
		{
			_coeffDblF = coeff; _point1 = spt1; _baseVector = vec;
			EndAlterProcess(_point1, _baseVector, GetObjectsFromFormula(_coeffDblF), null, _point2);
		}

		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule les coordonnées:
			_coeff = _coeffDblF.Recalculate();
			if (_coeffDblF.IsNaN) { SendCalculationResult(true, "Coefficient is not valid."); return; }
			_coords = _coeff * _baseVector.Coordinates;
			AlterCoords(_coords);
			// Recalcule le point 2:
			_point2.AlterCoords(_point1.Coordinates + _coords);
			_point2.Recalculate(true);
			// Calcule la norme:
			_norm = _coords.GetNorm();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
			{ ChangeNameInFormula(ref _coeffDblF, oldName, newName); }

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} based on {1}, using coeff {2}", BaseToString(), _baseVector, _coeff);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _coeffDblF, _baseVector}; }
		
	}
	

	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Vecteur défini par une somme de vecteurs.
	/// </summary>
	public class SpVectorUsingSum : SpVectorObject
	{
	
		protected SpVectorObject[] _baseVectors;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Vector sum"; } }
		
		/// <summary>
		/// Obtient les vecteurs de base.
		/// </summary>
		public SpVectorObject[] BaseVectors { get { return _baseVectors; } }
		
		/// <summary>
		/// Constructeur. Le vecteur est défini par une somme de vecteurs.
		/// </summary>
		public SpVectorUsingSum(string name, SpPointObject spt1, params SpVectorObject[] baseVectors) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, baseVectors);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpPointObject spt1, params SpVectorObject[] baseVectors)
		{
			_point1 = spt1; _baseVectors = baseVectors;
			EndAlterProcess(_point1, _baseVectors, null, _point2);
		}

		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule les coordonnées:
			_coords = GeoFunctions.GetVectorsSumCoords(_baseVectors);
			AlterCoords(_coords);
			// Recalcule le point 2:
			_point2.AlterCoords(_point1.Coordinates + _coords);
			_point2.Recalculate(true);
			// Calcule la norme:
			_norm = _coords.GetNorm();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} sum of vectors {1}", BaseToString(),
				My.ArrayFunctions.Join(_baseVectors, delegate(SpVectorObject v) { return v.Name; }, ","));
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _baseVectors}; }
		
	}

	
	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Vecteur qui est orthogonal et de même norme qu'un autre vecteur.
	/// </summary>
	public class SpOrthonormalVector : SpVectorObject
	{
	
		protected bool _invertDir;
		protected SpVectorObject _baseVector;
		protected SpPointObject _planePoint;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Orthonormal vector"; } }
		
		/// <summary>
		/// Vecteur de référence.
		/// </summary>
		public SpVectorObject BaseVector { get { return _baseVector; } }
		
		/// <summary>
		/// Point définissant le plan avec le vecteur de référence.
		/// </summary>
		public SpPointObject PlanePoint { get { return _planePoint; } }
		
		/// <summary>
		/// Indique si le sens est inversé.
		/// </summary>
		public bool InvertDirection { get { return _invertDir; } }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpOrthonormalVector(string name) : base(name)
			{ ; }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpOrthonormalVector(string name, SpPointObject spt1, SpVectorObject baseVec, SpPointObject planePt, bool invertDir) : this(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, baseVec, planePt, invertDir);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpPointObject spt1, SpVectorObject baseVec, SpPointObject planePt, bool invertDir)
		{
			_baseVector = baseVec; _planePoint = planePt; _invertDir = invertDir; _point1 = spt1;
			EndAlterProcess(_point1, _baseVector, _planePoint, null, _point2);
		}

		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Si le vecteur de base est nul, on sort:
			if (_baseVector.IsNul) { SendCalculationResult(true, FormatText("Base vector {0} is null.", _baseVector)); return; }
			// Récupère les coordonnées:
			_coords = GeoFunctions.GetOrthonormalVectorCoords(_baseVector.Coordinates, _point1.Coordinates,
				_planePoint.Coordinates, _invertDir);
			AlterCoords(_coords);
			// Sort si le vecteur est indéfini:
			if (_coords.Empty) { SendCalculationResult(true, FormatText("Points {0}, {1} and {2} are aligned.", _baseVector.Point1,
						_baseVector.Point2, _planePoint)); return; }
			CalculateCoordsStruct();
			// Définit les nouvelles coordonnées du point 2:
			_point2.AlterCoords(_point1.Coordinates + _coords);
			_point2.Recalculate(true);
			// Calcule la norme:
			_norm = _coords.GetNorm();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} based on {1}, using {2}", BaseToString(), _baseVector, _planePoint);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _baseVector, _planePoint, _invertDir}; }
		
	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Vecteur normal v à une droite de vecteur directeur u, et défini par le projeté orthogonal d'un point sur la droite à ce point. Ce point ne doit pas être aligné avec les points définissant u. Le vecteur v est défini par le projeté orthogonal A' de A sur la droite dirigé par u, et passant par les points définissant u, et par le point A. D'où v = A'A.
	/// </summary>
	public class SpNormalVectorToLineStartingAtLine : SpVectorObject
	{
	
		private SpLine _baseLine;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Normal vector to line"; } }
		
		/// <summary>
		/// Obtient la droite.
		/// </summary>
		public SpLine BaseLine { get { return _baseLine; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpNormalVectorToLineStartingAtLine(string name, SpLine line, SpPointObject spt2) : base(name)
		{
			Alter(line, spt2);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpLine line, SpPointObject spt2)
		{
			_baseLine = line; _point2 = spt2;
			if (_point1 == null) { _point1 = new SpOrthoProjPointOnLine("%pt1", _point2, _baseLine); _point1.Recalculate(true); }
			else { _point1.RebuildObject(true, _point2, _baseLine); }
			EndAlterProcess(_baseLine, _point2, null, _point1);
		}
				
		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Obtient les coordonnées du vecteur:
			_coords = _point2.Coordinates - _point1.Coordinates;
			AlterCoords(_coords);
			// Calcule la norme:
			_norm = _coords.GetNorm();
			// Si le vecteur est nul (cas, notamment, si le _point2 est sur la droite), il est indéfini, puisqu'un vecteur normal ne peut être nul:
			if (_coords.IsNul) {
				SendCalculationResult(true, FormatText("Points {0}, {1}, {2} are aligned. Normal vector can't be null.",
					_point2, _baseLine.Point1, _baseLine.Point2)); return; }
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} to {1}", BaseToString(), _baseLine);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_baseLine, _point2}; }
		
	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Vecteur normal à une droite, de même norme que le vecteur directeur de la droite.
	/// </summary>
	public class SpNormalVectorToLine : SpOrthonormalVector
	{
	
		private SpLine _baseLine;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Normal vector to line"; } }
		
		/// <summary>
		/// Obtient la droite.
		/// </summary>
		public SpLine BaseLine { get { return _baseLine; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpNormalVectorToLine(string name, SpPointObject spt1, SpLine line, SpPointObject planePt, bool invertDir) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, line, planePt, invertDir);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpPointObject spt1, SpLine line, SpPointObject planePt, bool invertDir)
		{
			_baseLine = line; _point1 = spt1; _planePoint = planePt; _invertDir = invertDir;
			EndAlterProcess(_baseLine, _point1, _planePoint, null, _point2);
		}
				
		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets:
			_baseVector = _baseLine.Vector;
			// Appel la base:
			base.CalculateNumericData();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} to {1}, using {2}", BaseToString(), _baseLine, _planePoint);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _baseLine, _planePoint, _invertDir}; }
		
	}

		
	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Vecteur normal à un plan.
	/// </summary>
	public class SpNormalVectorToPlane : SpVectorObject
	{
	
		protected SpPlaneObject _plane;
		protected bool _invertDir;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Normal vector to plane"; } }
		
		/// <summary>
		/// Obtient la droite.
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpNormalVectorToPlane(string name, SpPointObject spt1, SpPlaneObject plane, bool invertDir) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, plane, invertDir);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpPointObject spt1, SpPlaneObject plane, bool invertDir)
		{
			_plane = plane; _point1 = spt1; _invertDir = invertDir;
			EndAlterProcess(_plane, _point1, null, _point2);
		}
				
		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Translate simplement le point 2 à partir du point 1, et recopie les coordonnées:
			AlterCoords(_coords = _plane.NormalVectorCoords * (_invertDir ? -1 : 1));
			_point2.AlterCoords(_point1.Coordinates + _coords);
			_point2.Recalculate(true);
			// Calcule la norme:
			_norm = _coords.GetNorm();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} to {1}",	BaseToString(), _plane);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _plane, _invertDir}; }
		
	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Vecteur normal à un plan défini par le projeté orthogonal d'un point sur le plan, et par le point.
	/// </summary>
	public class SpNormalVectorToPlaneStartingAtPlane : SpVectorObject
	{
	
		protected SpPlaneObject _plane;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Normal vector to plane"; } }
		
		/// <summary>
		/// Obtient la droite.
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpNormalVectorToPlaneStartingAtPlane(string name, SpPlaneObject plane, SpPointObject spt2) : base(name)
		{
			Alter(plane, spt2);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public void Alter(SpPlaneObject plane, SpPointObject spt2)
		{
			_plane = plane; _point2 = spt2;
			if (_point1 == null) { _point1 = new SpOrthoProjPointOnPlane("%pt1", _point2, _plane); _point1.Recalculate(true); }
			else { _point1.RebuildObject(true, _point2, _plane); }
			EndAlterProcess(_plane, _point2, null, _point1);
		}
				
		/// <summary>
		/// Recalcule les données numériques.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Obtient les coordonnées du vecteur:
			_coords = _point2.Coordinates - _point1.Coordinates;
			AlterCoords(_coords);
			// Calcule la norme:
			_norm = _coords.GetNorm();
			// Si le vecteur est nul (cas, notamment, si le _point2 est sur le plan), il est indéfini, puisqu'un vecteur normal ne peut être nul:
			if (_coords.IsNul) {
				SendCalculationResult(true, FormatText("Point {0} is on plane {1}. Normal vector can't be null.", _point2, _plane)); return; }
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} to {1}", BaseToString(), _plane);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_plane, _point2}; }
		
	}
		
		
	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Droite passant par deux points de l'espace.
	/// </summary>
	public class SpLine : SpLineObject
	{
	
		protected SpVectorUsingPoints _dirVector;
		protected Eq2Zero _eqOnPlane;
	
		/// <summary>
		/// Obtient le vecteur directeur de la droite.
		/// </summary>
		public SpVectorObject Vector { get { return _dirVector; } }
		
		/// <summary>
		/// Obtient l'équation de la droite sur le plan. Si les points ne sont pas sur le même plane, ou ne sont pas des SpPointOnPlane, structure Empty.
		/// </summary>
		public Eq2Zero CartesianEq { get { return _eqOnPlane; } }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpLine(string name) : base(name)
			{ ; }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpLine(string name, SpPointObject spt1, SpPointObject spt2) : this(name)
		{
			Alter(spt1, spt2);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject spt1, SpPointObject spt2)
		{
			_point1 = spt1; _point2 = spt2;
			if (_dirVector == null) { _dirVector = new SpVectorUsingPoints("%dirVec", _point1, _point2); _dirVector.Recalculate(false); }
			else { _dirVector.RebuildObject(true, _point1, _point2); }
			EndAlterProcess(_point1, _point2, null, _dirVector);
		}

		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			if (!TestDefined(_point1, _point2)) { return; }
			EndCalculationProcess();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Vérifie que les points passés ne sont pas confondus. Retourne true si non confondus, au false si confondus. Affiche message d'erreur et met IsUndifined à true.
		/// </summary>
		protected bool TestDefined(SpPointObject spt1, SpPointObject spt2)
		{
			if (spt1.Coordinates == spt2.Coordinates)
				{ SendCalculationResult(true, FormatText("Points {0} and {1} have same coordinates.", spt1, spt2)); return false; }
			return true;
		}
		
		/// <summary>
		/// Calcule l'équation sur le plan, si les points sont des points du même plan.
		/// </summary>
		protected void EndCalculationProcess()
		{
			// Calcule l'équation sur le plan, au besoin:
			if (_point1 is SpPointOnPlane && _point2 is SpPointOnPlane)
			{
				Coord2D vec = ((SpPointOnPlane)_point2).CoordinatesOnPlane - ((SpPointOnPlane)_point1).CoordinatesOnPlane;
				_eqOnPlane.Empty = false;
				_eqOnPlane.a = vec.Y;
				_eqOnPlane.b = -vec.X;
				_eqOnPlane.c = vec.X * ((SpPointOnPlane)_point1).CoordinatesOnPlane.Y
					- vec.Y * ((SpPointOnPlane)_point1).CoordinatesOnPlane.X;
				_eqOnPlane.MultiplyMinusOne();
			}
			else { _eqOnPlane.Empty = true; }
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("through {0} and {1}, directed by {2},", _point1, _point2, _dirVector);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _point2}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet, pour les droites, les demi-droites et les segments (affichage de la longueur).
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string eq = FormatText("Eq.: {{x = {0} + {1}t\n     {{y = {2} + {3}t, t ∈ {4}\n     {{z = {5} + {6}t",
				_point1.X, _dirVector.X, _point1.Y, _dirVector.Y, (this is SpRay ? "[0;+∞]" :
				(this is SpSegment ? "[0;1]" : "ℝ")), _point1.Z, _dirVector.Z);
			if (_eqOnPlane.Empty) { return base.GetInfos(eq, lines); }
			string planeEq = String.Format("Eq. on plane: {0}", _eqOnPlane.ToString(_decPlacesFormat));
			return base.GetInfos(eq, planeEq, lines);
		}

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Droite définie par un vecteur directeur.
	/// </summary>
	public class SpLineUsingVector : SpLine
	{
	
		protected SpVectorObject _baseVector;
		
		/// <summary>
		/// Obtient le vecteur directeur de référence.
		/// </summary>
		public SpVectorObject BaseVector { get { return _baseVector; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpLineUsingVector(string name, SpPointObject spt1, SpVectorObject vec) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, vec);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject spt1, SpVectorObject vec)
		{
			_baseVector = vec; _point1 = spt1;
			if (_dirVector == null) { _dirVector = new SpVectorUsingPoints("%dirVec", _point1, _point2); _dirVector.Recalculate(false); }
			else { _dirVector.RebuildObject(false, _point1); }
			EndAlterProcess(_point1, _baseVector, null, _point2, _dirVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			if (!TestDefined(_baseVector.Point1, _baseVector.Point2)) { return; }
			// Calcule les coordonnées du point 2:
			_point2.AlterCoords(_point1.Coordinates + _baseVector.Coordinates);
			_point2.Recalculate(true);
			EndCalculationProcess();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _baseVector}; }

	}


	// ---------------------------------------------------------------------------
		
	
	/// <summary>
	/// Droite parallèle à une autre.
	/// </summary>
	public class SpParallelLine : SpLine
	{
	
		protected SpLine _baseLine;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Parallel line"; } }
		
		/// <summary>
		/// Obtient la droite de référence.
		/// </summary>
		public SpLine BaseLine { get { return _baseLine; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpParallelLine(string name, SpPointObject spt1, SpLine baseLine) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, baseLine);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject spt1, SpLine baseLine)
		{
			_point1 = spt1; _baseLine = baseLine; 
			if (_dirVector == null) { _dirVector = new SpVectorUsingPoints("%dirVec", _point1, _point2); _dirVector.Recalculate(false); }
			else { _dirVector.RebuildObject(false, _point1); }
			EndAlterProcess(_point1, _baseLine, null, _point2, _dirVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Calcule les coordonnées du point 2:
			_point2.AlterCoords(_point1.Coordinates + _baseLine.Vector.Coordinates);
			_point2.Recalculate(true);
			if (!TestDefined(_point1, _point2)) { return; }
			EndCalculationProcess();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} to {1}", BaseToString(), _baseLine);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _baseLine}; }

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Droite perpenduculaire à une autre.
	/// </summary>
	public class SpPerpendicularLine : SpLine
	{
	
		protected SpLine _baseLine;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Perpendicular line"; } }
		
		/// <summary>
		/// Obtient la droite de référence.
		/// </summary>
		public SpLine BaseLine { get { return _baseLine; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPerpendicularLine(string name, SpPointObject spt2, SpLine baseLine) : base(name)
		{
			Alter(spt2, baseLine);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject spt2, SpLine baseLine)
		{
			_point2 = spt2; _baseLine = baseLine; 
			if (_point1 == null) { _point1 = new SpOrthoProjPointOnLine("%pt1", _point2, _baseLine); _point1.Recalculate(true); }
			else { _point1.RebuildObject(true, _point2, _baseLine); }
			if (_dirVector == null) { _dirVector = new SpVectorUsingPoints("%dirVec", _point1, _point2); _dirVector.Recalculate(true); }
			else { _dirVector.RebuildObject(true, null, _point2); }
			EndAlterProcess(_point2, _baseLine, null, _point1, _dirVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Rien à faire sinon vérifier que les points ne sont pas confondus:
			if (!TestDefined(_point1, _point2)) { return; }
			EndCalculationProcess();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} to {1}", BaseToString(), _baseLine);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point2, _baseLine}; }

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Droite perpenduculaire à un plan.
	/// </summary>
	public class SpPerpendicularLineToPlane : SpLine
	{
	
		protected SpPlaneObject _basePlane;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Perpendicular line to plane"; } }
		
		/// <summary>
		/// Obtient la droite de référence.
		/// </summary>
		public SpPlaneObject Plane { get { return _basePlane; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPerpendicularLineToPlane(string name, SpPointObject spt1, SpPlaneObject basePlane) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(spt1, basePlane);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject spt1, SpPlaneObject basePlane)
		{
			_point1 = spt1; _basePlane = basePlane;
			if (_dirVector == null) { _dirVector = new SpVectorUsingPoints("%dirVec", _point1, _point2); _dirVector.Recalculate(false); }
			else { _dirVector.RebuildObject(false, _point1); }
			EndAlterProcess(_point1, _basePlane, null, _point2, _dirVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Translate simplement le point 2 à partir du point 1 (inutile de vérifier que les points sont distincts:
			// si le vecteur normal au plan est nul, c'est que le plan n'est pas défini):
			_point2.AlterCoords(_point1.Coordinates + _basePlane.NormalVectorCoords);
			_point2.Recalculate(true);
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} to {1}", BaseToString(), _basePlane);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_point1, _basePlane}; }

	}

	
	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Segment entre deux points de l'espace.
	/// </summary>
	public class SpSegment : SpLine
	{
	
		protected double _length;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Segment"; } }
		
		/// <summary>
		/// Obtient la longueur du segment dans l'espace.
		/// </summary>
		public double Length { get { return _length; } }
	
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpSegment(string name) : base(name)
			{ ; }
	
		/// <summary>
		/// Constructeur. 
		/// </summary>
		public SpSegment(string name, SpPointObject spt1, SpPointObject spt2) : this(name)
		{
			Alter(spt1, spt2);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public override void Alter(SpPointObject spt1, SpPointObject spt2)
			{ base.Alter(spt1, spt2); }
				
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			base.CalculateNumericData();
			// Calcule la longueur du segment:
			_length = _point1.Coordinates.GetLength(_point2.Coordinates);
		}

		/// <summary>
		/// Retourne une description détaillée de l'objet:
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string length = FormatText("Length: {0}", _length);
			return base.GetInfos(length, lines);
		}
		
	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Demi-droite à partir d'un point de l'espace et passant par un autre point.
	/// </summary>
	public class SpRay : SpLine
	{
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Ray"; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpRay(string name, SpPointObject spt1, SpPointObject spt2) : base(name)
		{
			Alter(spt1, spt2);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public override void Alter(SpPointObject spt1, SpPointObject spt2)
			{ base.Alter(spt1, spt2); }
				
	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Bissectrice d'un segment.
	/// </summary>
	public class SpAngleBissector : SpLine
	{
	
		protected SpAngle _angle;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Angle bissector"; } }
		
		/// <summary>
		/// Obtient l'angle.
		/// </summary>
		public SpAngle Angle { get { return _angle; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpAngleBissector(string name, SpAngle angle) : base(name)
		{
			Alter(angle);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpAngle angle)
		{
			_angle = angle; _point1 = _angle.Vertex; _point2 = _angle.PointForBissector;
			if (_dirVector == null) { _dirVector = new SpVectorUsingPoints("%dirVec", _point1, _point2); _dirVector.Recalculate(false); }
			EndAlterProcess(_angle, null, _dirVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les points 1 et 2:
			_point1 = _angle.Vertex; _point2 = _angle.PointForBissector;
			// Test si la droite existe:
			if (!TestDefined(_point1, _point2)) { return; }
			// Met à jour de _dirVector:
			_dirVector.RebuildObject(true, _point1, _point2);
			EndCalculationProcess();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} of {1}", BaseToString(), _angle);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_angle}; }

	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Intersection de deux plans.
	/// </summary>
	public class SpPlanesIntersection : SpLine
	{
	
		protected SpPlaneObject _plane1, _plane2;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Planes intersection"; } }
		
		/// <summary>
		/// Obtient un plan.
		/// </summary>
		public SpPlaneObject Plane1 { get { return _plane1; } }
	
		/// <summary>
		/// Obtient un plan.
		/// </summary>
		public SpPlaneObject Plane2 { get { return _plane2; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPlanesIntersection(string name, SpPlaneObject plane1, SpPlaneObject plane2) : base(name)
		{
			_point1 = new SpPointObject("%pt1", 0, 0, 0);
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(plane1, plane2);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPlaneObject plane1, SpPlaneObject plane2)
		{
			_plane1 = plane1; _plane2 = plane2;
			if (_dirVector == null) { _dirVector = new SpVectorUsingPoints("%dirVec", _point1, _point2); _dirVector.Recalculate(false); }
			EndAlterProcess(_plane1, _plane2, null, _point1, _point2, _dirVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Obtient les coordonnées du vecteur directeur, ainsi que le point 1 (puis translate le point 2):
			Coord3D pt1; Coord3D vec;
			if (!GeoFunctions.GetPlanesInterCoords(_plane1.CartesianEq, _plane2.CartesianEq, out vec, out pt1))
				{ SendCalculationResult(true, "Intersection not found"); return; }
			_point1.AlterCoords(pt1); _point1.Recalculate(true);
			_point2.AlterCoords(pt1 + vec); _point2.Recalculate(true);
			EndCalculationProcess();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} between {1} and {2}", BaseToString(), _plane1, _plane2);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_plane1, _plane2}; }

	}
		
	
	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Intersection d'un plan et d'un polygone.
	/// </summary>
	public class SpPlanePolygonIntersection : SpSegment
	{
	
		protected SpPlaneObject _plane;
		protected SpPolygon _polygon;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Plane and polygon intersection"; } }
		
		/// <summary>
		/// Obtient un plan.
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }
	
		/// <summary>
		/// Obtient un polygone.
		/// </summary>
		public SpPolygon Polygon { get { return _polygon; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPlanePolygonIntersection(string name, SpPlaneObject plane, SpPolygon polygon) : base(name)
		{
			_point1 = new SpPointObject("%pt1", 0, 0, 0);
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(plane, polygon);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPlaneObject plane, SpPolygon polygon)
		{
			_plane = plane; _polygon = polygon;
			if (_dirVector == null) { _dirVector = new SpVectorUsingPoints("%dirVec", _point1, _point2); _dirVector.Recalculate(false); }
			EndAlterProcess(_plane, _polygon, null, _point1, _point2, _dirVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Obtient la droite d'intersection entre le plan et le plan du polygon, si elle existe:
			Coord3D lineVec, lineOrigin;
			Coord3D[] vertices = Array.ConvertAll(_polygon.Vertices, delegate(SpPointObject o) { return o.Coordinates; });
			vertices = vertices.Distinct().ToArray();
			if (vertices.Length < 3) { SendCalculationResult(true, "Not enough points in polygon."); return; }
			if (!GeoFunctions.GetPlanesInterCoords(_plane.CartesianEq, GeoFunctions.GetPlaneCartesianEquation(vertices[0],
				vertices[1] - vertices[0], vertices[2] - vertices[0]), out lineVec, out lineOrigin))
				{ SendCalculationResult(true, "Intersection not found or polygon not correctly defined."); return; }
			// Cherche une intersection entre chacun des côtés du polygone, et la droite:
			int l = vertices.Length, ip, c = 0; Coord3D inter; double t, tp;
			for (int i=0; i<l; i++)
			{
				ip = (i==l-1 ? 0 : i+1);
				if (GeoFunctions.GetLinesInterCoords(vertices[i], vertices[ip] - vertices[i], lineOrigin, lineVec, 0, 1, null, null,
					out inter, out t, out tp))
				{
					if (c++ == 0) { _point1.AlterCoords(inter); _point1.Recalculate(true); }
					else { _point2.AlterCoords(inter); _point2.Recalculate(true); }
					if (c == 2) { break; }
				}
			}
			// Si pas deux intersections, indéfini (il peut y avoir une seule intersection, mais comme il faut un segment,
			// il faut deux points !):
			if (c != 2) { SendCalculationResult(true, "No intersection found."); return; }
			// Calcule la longueur du segment:
			_length = _point1.Coordinates.GetLength(_point2.Coordinates);
			EndCalculationProcess();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} between {1} and {2}", BaseToString(), _plane, _polygon);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_plane, _polygon}; }

	}
		
	
	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Tangente à un cercle.
	/// </summary>
	public class SpCircleTangent : SpLine
	{
	
		protected SpCircle _circle;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Circle tangent"; } }
		
		/// <summary>
		/// Obtient le cercle.
		/// </summary>
		public SpCircle Circle { get { return _circle; } }

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpCircleTangent(string name, SpCircle circle, SpPointOnPlaneObject ptOnCircle) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(circle, ptOnCircle);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpCircle circle, SpPointOnPlaneObject ptOnCircle)
		{
			_circle = circle; _point1 = ptOnCircle;
			if (_dirVector == null) { _dirVector = new SpVectorUsingPoints("%dirVec", _point1, _point2); _dirVector.Recalculate(false); }
			else { _dirVector.RebuildObject(false, _point1); }
			EndAlterProcess(_circle, _point1, null, _point2, _dirVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule le vecteur, le point 2:
			Coord2D vec = ((SpPointOnPlaneObject)_point1).CoordinatesOnPlane - _circle.Center.CoordinatesOnPlane;
			Coord3D vec3D = _circle.Plane.To3D(-vec.Y, vec.X);
			_point2.AlterCoords(_point1.Coordinates + vec3D);
			_point2.Recalculate(true);
			// Test si la droite existe:
			if (!TestDefined(_point1, _point2)) { return; }
			EndCalculationProcess();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} of {1} at {2}", BaseToString(), _circle, _point1);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_circle, _point1}; }

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Tangente à une fonction du plan.
	/// </summary>
	public class SpFunction1OnPlaneTangent : SpLine
	{
	
		protected SpFunction1OnPlane _function;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Function tangent"; } }
		
		/// <summary>
		/// Obtient la fonction.
		/// </summary>
		public SpFunction1OnPlane Function { get { return _function; } }

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpFunction1OnPlaneTangent(string name, SpFunction1OnPlane function, SpPointOnPlaneObject ptOnFunc) : base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0);
			Alter(function, ptOnFunc);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpFunction1OnPlane function, SpPointOnPlaneObject ptOnFunc)
		{
			_function = function; _point1 = ptOnFunc;
			if (_dirVector == null) { _dirVector = new SpVectorUsingPoints("%dirVec", _point1, _point2); _dirVector.Recalculate(false); }
			else { _dirVector.RebuildObject(false, _point1); }
			EndAlterProcess(_function, _point1, null, _point2, _dirVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Calcule deux points sur la fonction:
			SpPointOnPlaneObject pt1 = (SpPointOnPlaneObject)_point1;
			Coord2D A = new Coord2D(pt1.XOnPlane - 0.05, _function.GetFunctionValue(pt1.XOnPlane - 0.05));
			Coord2D B = new Coord2D(pt1.XOnPlane + 0.05, _function.GetFunctionValue(pt1.XOnPlane + 0.05));
			if (DoubleF.IsThereNan(A.Y, B.Y))
				{ SendCalculationResult(true, "Function return an error."); return; }
			// ... puis le vecteur définis par ces deux points:
			Coord3D dirVec = _function.Plane.To3D(B) - _function.Plane.To3D(A);
			// Translate le point sur la fonction, et on a une approximation de la tangente:
			_point2.AlterCoords(_point1.Coordinates + dirVec);
			_point2.Recalculate(true);
			// Test si la droite existe:
			if (!TestDefined(_point1, _point2)) { return; }
			EndCalculationProcess();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} of {1} at {2}", BaseToString(), _function, _point1);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_function, _point1}; }

	}
	
	

	#endregion OBJETS DE L'ESPACE


	
}
