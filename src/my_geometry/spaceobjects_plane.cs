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
	/// Objet de base pour les plans munis d'un repère.
	/// </summary>
	public class SpPlaneObject : SpPenObject
	{
	
		protected Coord3D _OOp, _OpI, _OpJ, _normalVec;
		protected SpVectorObject _xVector, _yVector;
		protected SpPointObject _origin;
		protected Eq3Zero _eq;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Plane"; } }
		
		/// <summary>
		/// Obtient le vecteur des abscisses.
		/// </summary>
		public SpVectorObject XVector { get { return _xVector; } }
		
		/// <summary>
		/// Obtient le vecteur des ordonnées.
		/// </summary>
		public SpVectorObject YVector { get { return _yVector; } }
		
		/// <summary>
		/// Obtient le point d'origine du repère.
		/// </summary>
		public SpPointObject Origin { get { return _origin; } }
		
		/// <summary>
		/// Obtient l'équation cartésienne du plan.
		/// </summary>
		public Eq3Zero CartesianEq { get { return _eq; } }
		
		/// <summary>
		/// Obtient les coordonnées du vecteur normal.
		/// </summary>
		public Coord3D NormalVectorCoords { get { return _normalVec; } }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpPlaneObject(string name) : base(name)
		{
			_pen.CustomEndCap = DrawingArea.CustomArrowCap;
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
			{ ; }
		
		/// <summary>
		/// Calcule l'équation cartésienne du plan, ainsi que les variables _OOp, _OpI, _OpJ. Toujours appeler cette méthode à la fin du calcul.
		/// </summary>
		protected virtual void EndCalculationProcess()
		{
			// Coordonnées des vecteurs de base pour le calcul des To2D et To3D:
			_OOp = _origin.Coordinates;
			_OpI = _xVector.Coordinates;
			_OpJ = _yVector.Coordinates;
			// Equation cartésienne:
			_eq.a = _OpJ.Z * _OpI.Y - _OpI.Z * _OpJ.Y;
			_eq.b = _OpI.Z * _OpJ.X - _OpJ.Z * _OpI.X;
			_eq.c = _OpI.X * _OpJ.Y - _OpJ.X * _OpI.Y;
			_eq.d = - ( _eq.a * _origin.X + _eq.b * _origin.Y + _eq.c * _origin.Z );
			// Coordonnées du vecteur normal:
			_normalVec.X = _eq.a;
			_normalVec.Y = _eq.b;
			_normalVec.Z = _eq.c;
		}
		
		/// <summary>
		/// Calcule l'origine du label. Le paramètre est le coefficient multiplicateur du vecteur formé par les deux points.
		/// </summary>
		protected override void CalculateLabelOrigin()
			{ _labelOrigin = _origin.Coordinates; }
		
		/// <summary>
		/// Obtient les coordonnées 3D du repère général à partir des coordonnées 2D de ce repère.
		/// </summary>
		public Coord3D To3D(double x, double y)
			{ return _OOp + x * _OpI + y * _OpJ; }
		
		/// <summary>
		/// Obtient les coordonnées 3D du repère général à partir des coordonnées 2D de ce repère.
		/// </summary>
		public Coord3D To3D(Coord2D pt)
			{ return To3D(pt.X, pt.Y); }
		
		/// <summary>
		/// Obtient les coordonnées 2D de ce repère à partir des coordonnées 3D du repère général. Retourne une structeur IsEmpty si le point n'appartient pas au plan.
		/// </summary>
		public Coord2D To2D(double x, double y, double z)
		{
			// Calcul les coordonnées du vecteur O'M:
			Coord3D OpM = new Coord3D(x, y, z) - Origin.Coordinates;
			// Tente de résoudre le système de trois équations à deux inconnues:
			double[,] system = new double[3,3];
			system[0,0] = _OpI.X; system[0,1] = _OpJ.X; system[0,2] = OpM.X;
			system[1,0] = _OpI.Y; system[1,1] = _OpJ.Y; system[1,2] = OpM.Y;
			system[2,0] = _OpI.Z; system[2,1] = _OpJ.Z; system[2,2] = OpM.Z;
			double[] sol = My.MathsAlg.SolveSimul(system);
			if (sol == null) { return new Coord2D(true); }
			return new Coord2D(sol[0], sol[1]);
		}

		/// <summary>
		/// Obtient les coordonnées 2D de ce repère à partir des coordonnées 3D du repère général. Retourne une struture IsEmpty si le point n'appartient pas au plan.
		/// </summary>
		public Coord2D To2D(Coord3D pt)
			{ return To2D(pt.X, pt.Y, pt.Z); }
		
		/// <summary>
		/// Retourne une chaîne commune aux points décrivant les coordonnées du point.
		/// </summary>
		public virtual string BaseToString()
		{
			return FormatText("({0},{1},{2})", _origin, _xVector, _yVector);
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
			{ return new object[0]; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet, pour les droites, les demi-droites et les segments (affichage de la longueur).
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string eq = FormatText("Eq.: {{x = {0} + {1}t + {2}t'\n     {{y = {3} + {4}t + {5}t', (t,t') ∈ ℝ×ℝ\n     {{z = {6} + {7}t + {8}t'",
				_OOp.X, _OpI.X, _OpJ.X, _OOp.Y, _OpI.Y, _OpJ.Y, _OOp.Z, _OpI.Z, _OpJ.Z);
			string cartEq = String.Format("Cartesian eq.: {0}", _eq.ToString(_decPlacesFormat));
			string normalVec = FormatText("Normal vector: ({0},{1},{2})", _normalVec.X, _normalVec.Y, _normalVec.Z);
			return base.GetInfos(eq, cartEq, normalVec, lines);
		}

	}



	#endregion OBJETS DE BASE
	





	// ---------------------------------------------------------------------------
	// CLASSES D'OBJETS DU PLAN
	// ---------------------------------------------------------------------------




	#region CLASSES D'OBJETS DU PLAN



	/// <summary>
	/// Définit un repère défini par une origine et deux vecteurs non colinéaires et non nuls.
	/// </summary>
	public class SpPlane : SpPlaneObject
	{
	
		protected SpVectorObject _baseXVector, _baseYVector;
		
		/// <summary>
		/// Obtient le vecteur de base.
		/// </summary>
		public SpVectorObject BaseXVector { get { return _baseXVector; } }
		
		/// <summary>
		/// Obtient le vecteur de base.
		/// </summary>
		public SpVectorObject BaseYVector { get { return _baseYVector; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPlane(string name, SpPointObject origin, SpVectorObject xVec, SpVectorObject yVec) : base(name)
		{
			Alter(origin, xVec, yVec);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject origin, SpVectorObject xVec, SpVectorObject yVec)
		{
			_origin = origin; _baseXVector = xVec; _baseYVector = yVec;
			if (_xVector == null) { _xVector = new SpVectorObject("%xVec", _origin, 0, 0, 0); }
			else { _xVector.RebuildObject(false, _origin); }
			if (_yVector == null) { _yVector = new SpVectorObject("%yVec", _origin, 0, 0, 0); }
			else { _yVector.RebuildObject(false, _origin); }
			EndAlterProcess(_origin, _baseXVector, _baseYVector, null, _xVector, _yVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Sort si les vecteurs sont colinéaires ou si l'un est nul:
			if (GeoFunctions.AreCollinear(_baseXVector, _baseYVector) || _baseXVector.IsNul || _baseYVector.IsNul)
				{ SendCalculationResult(true, "Vectors are null or collinear."); return; }
			// Reproduit les coordonnées des vecteurs de base sur les vecteurs du repère:
			_xVector.AlterCoords(_baseXVector.Coordinates); _xVector.Recalculate(true);
			_yVector.AlterCoords(_baseYVector.Coordinates); _yVector.Recalculate(true);
			EndCalculationProcess();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} based on {1} and {2}", BaseToString(), _baseXVector, _baseYVector);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_origin, _baseXVector, _baseYVector}; }

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Définit un repère défini par trois points
	/// </summary>
	public class SpPlaneUsingPoints : SpPlaneObject
	{
	
		protected SpPointObject _iPoint, _jPoint;
	
		/// <summary>
		/// Obtient le vecteur de base.
		/// </summary>
		public SpPointObject BaseIPoint { get { return _iPoint; } }
		
		/// <summary>
		/// Obtient le vecteur de base.
		/// </summary>
		public SpPointObject BaseJPoint { get { return _jPoint; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPlaneUsingPoints(string name, SpPointObject origin, SpPointObject iPoint, SpPointObject jPoint) : base(name)
		{
			Alter(origin, iPoint, jPoint);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject origin, SpPointObject iPoint, SpPointObject jPoint)
		{
			_origin = origin; _iPoint = iPoint; _jPoint = jPoint;
			if (_xVector == null) { _xVector = new SpVectorUsingPoints("%xVec", _origin, _iPoint); _xVector.Recalculate(true); }
			else { _xVector.RebuildObject(true, _origin, _iPoint); }
			if (_yVector == null) { _yVector = new SpVectorUsingPoints("%yVec", _origin, _jPoint); _yVector.Recalculate(true); }
			else { _yVector.RebuildObject(true, _origin, _jPoint); }
			EndAlterProcess(_origin, _iPoint, _jPoint, null, _xVector, _yVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Sort si les vecteurs sont colinéaires ou si l'un est nul:
			if (GeoFunctions.AreCollinear(_xVector, _yVector) || _xVector.IsNul || _yVector.IsNul)
				{ SendCalculationResult(true, "Vectors are null or collinear."); return; }
			EndCalculationProcess();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} based on {1} and {2}", BaseToString(), _iPoint, _jPoint);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_origin, _iPoint, _jPoint}; }

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Définit un repère orthonormal défini par une origin, un vecteur et un point du plan.
	/// </summary>
	public class SpOrthonormalPlane : SpPlaneObject
	{
	
		protected SpPointObject _planePoint;
		protected SpVectorObject _baseXVector;
		protected bool _invertDir;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Orthonormal plane"; } }
		
		/// <summary>
		/// Obtient le vecteur de base.
		/// </summary>
		public SpVectorObject BaseXVector { get { return _baseXVector; } }
		
		/// <summary>
		/// Obtient le troisième point qui définit le plan.
		/// </summary>
		public SpPointObject PlanePoint { get { return _origin; } }
		
		/// <summary>
		/// Indique s'il faut inverser la direction de l'axe des ordonnées.
		/// </summary>
		public bool InvertDirection { get { return _invertDir; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpOrthonormalPlane(string name, SpPointObject origin, SpVectorObject xVec, SpPointObject planePt, bool invertDir) : base(name)
		{
			Alter(origin, xVec, planePt, invertDir);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject origin, SpVectorObject xVec, SpPointObject planePt, bool invertDir)
		{
			_origin = origin; _baseXVector = xVec; _planePoint = planePt; _invertDir = invertDir;
			if (_xVector == null) { _xVector = new SpVectorObject("%xVec", _origin, 0, 0, 0); }
			else { _xVector.RebuildObject(false, _origin); }
			if (_yVector == null) { _yVector = new SpOrthonormalVector("%yVec", _origin, _xVector, _planePoint, _invertDir); }
			else { _yVector.RebuildObject(false, _origin, null, _planePoint, invertDir); }
			EndAlterProcess(_origin, _baseXVector, _planePoint, null, _xVector, _yVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Sort si _baseXVector est nul:
			if (_baseXVector.IsNul) { SendCalculationResult(true, "X vector is null."); return; }
			// Recopie les coordonnées du vecteur de base;
			_xVector.AlterCoords(_baseXVector.Coordinates); _xVector.Recalculate(true);
			// Sort si _yVector est indéfini (si, par exemple, on ne peut former de plan avec origin, planePt et xVec):
			if (_yVector.IsUndefined) { SendCalculationResult(true, null); return; }
			EndCalculationProcess();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} to {1} through {2}", BaseToString(), _baseXVector, _planePoint);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_origin, _baseXVector, _planePoint, _invertDir}; }

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Définit un repère défini par une origine et deux vecteurs non colinéaires et non nuls.
	/// </summary>
	public class SpParallelPlane : SpPlaneObject
	{
	
		protected SpPlaneObject _basePlane;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Parallel plane"; } }
		
		/// <summary>
		/// Obtient le plan de base.
		/// </summary>
		public SpPlaneObject BasePlane { get { return _basePlane; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpParallelPlane(string name, SpPointObject origin, SpPlaneObject plane) : base(name)
		{
			Alter(origin, plane);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject origin, SpPlaneObject plane)
		{
			_origin = origin; _basePlane = plane;
			if (_xVector == null) { _xVector = new SpVectorObject("%xVec", _origin, 0, 0, 0); }
			else { _xVector.RebuildObject(false, _origin); }
			if (_yVector == null) { _yVector = new SpVectorObject("%yVec", _origin, 0, 0, 0); }
			else { _yVector.RebuildObject(false, _origin); }
			EndAlterProcess(_origin, _basePlane, null, _xVector, _yVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Reproduit les coordonnées des vecteurs de base sur les vecteurs du repère:
			_xVector.AlterCoords(_basePlane.XVector.Coordinates); _xVector.Recalculate(true);
			_yVector.AlterCoords(_basePlane.YVector.Coordinates); _yVector.Recalculate(true);
			EndCalculationProcess();
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
			{ return new object[]{_origin, _basePlane}; }

	}


	// ---------------------------------------------------------------------------
	
		
	/// <summary>
	/// Définit un repère perpendiculaire à un autre plan, à partir de deux points, dont l'un est l'origine du nouveau repère et l'autre définissant le point I du nouveau repère.
	/// </summary>
	public class SpOrthogonalPlaneToPlane : SpPlaneObject
	{
	
		protected SpPlaneObject _basePlane;
		protected SpPointObject _planePt;
		protected bool _invertDir;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Orthogonal plane to plane"; } }
		
		/// <summary>
		/// Obtient le plan de base.
		/// </summary>
		public SpPlaneObject BasePlane { get { return _basePlane; } }
		
		/// <summary>
		/// Obtient le point I.
		/// </summary>
		public SpPointObject PlanePoint { get { return _planePt; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpOrthogonalPlaneToPlane(string name, SpPlaneObject plane, SpPointObject origin, SpPointObject planePt,
			bool invertDir) : base(name)
		{
			Alter(plane, origin, planePt, invertDir);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPlaneObject plane, SpPointObject origin, SpPointObject planePt, bool invertDir)
		{
			_origin = origin; _basePlane = plane; _planePt = planePt; _invertDir = invertDir;
			if (_xVector == null) { _xVector = new SpVectorUsingPoints("%xVec", _origin, _planePt); }
			else { _xVector.RebuildObject(false, _origin, _planePt); }
			if (_yVector == null) { _yVector = new SpVectorObject("%yVec", _origin, 0, 0, 0); }
			else { _yVector.RebuildObject(false, _origin); }
			EndAlterProcess(_origin, _basePlane, _planePt, null, _xVector, _yVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Obtient le vecteur orthonormal en se servant du point d'origine translaté par le vecteur normal au plan
			// pour définir un troisième point sur le plan:
			Coord3D xVec = _planePt.Coordinates - _origin.Coordinates;
			Coord3D thirdPt = _origin.Coordinates + _basePlane.NormalVectorCoords;
			Coord3D yVec = GeoFunctions.GetOrthonormalVectorCoords(xVec, _origin.Coordinates, thirdPt, _invertDir);
			if (yVec.Empty) { SendCalculationResult(true, "Vector is collinear to normal vector of plane."); return; }
			_xVector.AlterCoords(xVec); _xVector.Recalculate(true);
			_yVector.AlterCoords(yVec); _yVector.Recalculate(true);
			EndCalculationProcess();
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
			{ return new object[]{_origin, _basePlane, _planePt, _invertDir}; }

	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Définit un repère perpendiculaire à un vecteur (normal au plan, donc), à partir d'une origine donnée. Les vecteurs du repère sont orthogonaux et de même normes que le vecteur normal.
	/// </summary>
	public class SpOrthogonalPlaneToVector : SpPlaneObject
	{
	
		protected SpVectorObject _vector;
		protected DoubleF _phiDblF;
		protected double _phi;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Orthogonal plane to vector"; } }
		
		/// <summary>
		/// Obtient le vecteur.
		/// </summary>
		public SpVectorObject Vector { get { return _vector; } }
		
		/// <summary>
		/// Obtient l'angle de rotation.
		/// </summary>
		public double Alpha { get { return _phi; } }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpOrthogonalPlaneToVector(string name) : base(name)
			{ ; }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpOrthogonalPlaneToVector(string name, SpPointObject origin, SpVectorObject vector, DoubleF phi) : this(name)
		{
			Alter(origin, vector, phi);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject origin, SpVectorObject vector, DoubleF phi)
		{
			_origin = origin;_vector = vector; _phiDblF = phi;
			if (_xVector == null) { _xVector = new SpVectorObject("%xVec", _origin, 0, 0, 0); }
			else { _xVector.RebuildObject(false, _origin); }
			if (_yVector == null) { _yVector = new SpVectorObject("%yVec", _origin, 0, 0, 0); }
			else { _yVector.RebuildObject(false,  _origin); }
			EndAlterProcess(_origin, _vector, GetObjectsFromFormula(_phiDblF), null, _xVector, _yVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule phi:
			if (DoubleF.IsThereNan(_phi = _phiDblF.Recalculate()))
				{ SendCalculationResult(true, "Phi is not valid."); return; }
			// Sort si le vecteur est nul:
			if (_vector.IsNul) { SendCalculationResult(true, "Vector is null."); return; }
			// On considère que _vector est le vecteur O'Z'. Ce qui permet de trouver les
			// deux angles d'Euler ψ et θ:
			Coord3D OpZp = _vector.Coordinates;
			double OpZpNorm = OpZp.GetNorm(), k = 1 / OpZp.GetNorm(); OpZp *= k;
			double θ = Math.Acos(Maths.Approx(OpZp.Z)), ψ;
			if (θ == 0) { ψ = 0; }
			else { ψ = MathsGeo.GetAngleFromCosSin(-OpZp.Y / Math.Sin(θ), OpZp.X / Math.Sin(θ), false); }
			// Puis on peut récupère les coordonnées des vecteurs O'X' et O'Y':
			Coord3D OpXp, OpYp;
			MathsGeo.RotateCoordSystem(ψ, θ, _phi, out OpXp, out OpYp, out OpZp);
			_xVector.AlterCoords(OpXp * OpZpNorm); _xVector.Recalculate(true);
			_yVector.AlterCoords(OpYp * OpZpNorm); _yVector.Recalculate(true);
			EndCalculationProcess();
			SendCalculationResult();
		}

		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _phiDblF, oldName, newName);
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} to {1}", BaseToString(), _vector);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_origin, _vector, _phiDblF}; }

	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Même chose que SpOrthogonalPlaneToVector, mais avec une ligne.
	/// </summary>
	public class SpOrthogonalPlaneToLine : SpOrthogonalPlaneToVector
	{
	
		protected SpLine _line;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Orthogonal plane to line"; } }
		
		/// <summary>
		/// Obtient la ligne.
		/// </summary>
		public SpLine Line { get { return _line; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpOrthogonalPlaneToLine(string name, SpPointObject origin, SpLine line, DoubleF alpha) : base(name)
		{
			Alter(origin, line, alpha);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject origin, SpLine line, DoubleF alpha)
		{
			_origin = origin; _line = line; _vector = _line.Vector; _phiDblF = alpha;
			if (_xVector == null) { _xVector = new SpVectorObject("%xVec", _origin, 0, 0, 0); }
			else { _xVector.RebuildObject(false, _origin); }
			if (_yVector == null) { _yVector = new SpVectorObject("%yVec", _origin, 0, 0, 0); }
			else { _yVector.RebuildObject(false, _origin); }
			EndAlterProcess(_origin, _line, GetObjectsFromFormula(_phiDblF), null, _xVector, _yVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets et appelle la base:
			_vector = _line.Vector;
			base.CalculateNumericData();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} to {1}", BaseToString(), _line);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_origin, _line, _phiDblF}; }

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Définit un repère perpendiculaire à une droite en utilisant un point qui n'est pas sur la droite. Le vecteur OI est définit par le projeté orthogonal du point sur la droite et par le point. Le repère est orthogonormal.
	/// </summary>
	public class SpOrthogonalPlaneToLineUsingPoints : SpPlaneObject
	{
	
		protected SpLine _line;
		protected SpPointObject _planePt;
		protected bool _invertDir;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Orthogonal plane to line"; } }
		
		/// <summary>
		/// Obtient la droite.
		/// </summary>
		public SpLine Line { get { return _line; } }
		
		/// <summary>
		/// Obtient le point.
		/// </summary>
		public SpPointObject PlanePoint { get { return _planePt; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpOrthogonalPlaneToLineUsingPoints(string name, SpPointObject origin, SpLine line, SpPointObject planePt,
			bool inverDir) : base(name)
		{
			Alter(origin, line, planePt, inverDir);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject origin, SpLine line, SpPointObject planePt, bool invertDir)
		{
			_origin = origin; _line = line; _planePt = planePt; _invertDir = invertDir;
			if (_xVector == null) { _xVector = new SpVectorObject("%xVec", _origin, 0, 0, 0); }
			else { _xVector.RebuildObject(false, _origin); }
			if (_yVector == null) { _yVector = new SpVectorObject("%yVec", _origin, 0, 0, 0); }
			else { _yVector.RebuildObject(false, _origin); }
			EndAlterProcess(_origin, _line, _planePt, null, _xVector, _yVector);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Obtient le projeté orthogonal du point sur la droite, puis le vecteur X:
			Coord3D proj = GeoFunctions.GetOrthoProjPointOnLineCoords(_planePt.Coordinates, _line.Point1.Coordinates, _line.Vector.Coordinates);
			Coord3D xVec = _planePt.Coordinates - proj;
			// Si le vecteur est nul, alors indéfini:
			if (xVec.IsNul) { SendCalculationResult(true, "Plane point is on line."); return; }
			// Obtient le vecteur orthonormal et définit les vecteurs du repère:
			Coord3D yVec = GeoFunctions.GetAxialRotationPtCoords(_line.Point1.Coordinates,
				_line.Vector.Coordinates, _planePt.Coordinates, Math.PI / 2 * (_invertDir?1:-1)) - proj;
			_xVector.AlterCoords(xVec); _xVector.Recalculate(true);
			_yVector.AlterCoords(yVec); _yVector.Recalculate(true);
			EndCalculationProcess();
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} to {1}, through {2}", BaseToString(), _line, _planePt);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_origin, _line, _planePt, _invertDir}; }

	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Angle entre trois points. L'angle est toujours inférieur à 180° (IsOpposite permet de créer un angle supérieur à 180°). Si l'angle est égal à 0 ou 180°, il est défini, mais l'arc le symbolisant n'est pas dessiné (car trois points alignés ne définissent pas un plan).
	/// </summary>
	public class SpAngle : SpPenBrushObject
	{
	
		protected double _radValue, _sizeRadius;
		protected SpPointObject _vertex, _point1, _point2, _pointForBissector;
		protected Coord3D[] _ptsOnWinCoords;
		protected bool _isOpposite, _isOriented;

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Angle"; } }
		
		/// <summary>
		/// Valeur de l'angle.
		/// </summary>
		public double RadValue { get { return _radValue; } }
		
		/// <summary>
		/// Obtient le sommet de l'angle.
		/// </summary>
		public SpPointObject Vertex { get { return _vertex; } }
		
		/// <summary>
		/// Obtient le point 1.
		/// </summary>
		public SpPointObject Point1 { get { return _point1; } }
		
		/// <summary>
		/// Obtient le point 2.
		/// </summary>
		public SpPointObject Point2 { get { return _point2; } }
		
		/// <summary>
		/// Obtient un point qui se trouve sur la bissectrice de l'angle. C'est aussi le point d'origine du label.
		/// </summary>
		public SpPointObject PointForBissector { get { return _pointForBissector; } }
		
		/// <summary>
		/// Obtient si l'angle est orienté.
		/// </summary>
		public bool IsOriented { get { return _isOriented; } }
		
		/// <summary>
		/// Obtient les coordonnées des points à dessiner dans le repère du form.
		/// </summary>
		internal Coord3D[] PointsOnWinCoords { get { return _ptsOnWinCoords; } }
	
		/// <summary>
		/// Obtient ou définit les points à dessiner dans le repère du form.
		/// </summary>
		internal PointF[] PointsOnWin { get; set; }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		protected SpAngle(string name) : base(name)
		{
			_ptsOnWinCoords = new Coord3D[0];
			_pointForBissector = new SpPointObject("%bissectorPt", 0, 0, 0);
		}

		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpAngle(string name, SpPointObject spt1, SpPointObject vertex, SpPointObject spt2) : this(name)
		{
			Alter(spt1, vertex, spt2);
		}
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpAngle(string name, double size, bool isOpposite, bool isOriented, SpPointObject spt1, SpPointObject vertex,
			SpPointObject spt2) : this(name)
		{
			Alter(size, isOpposite, isOriented, spt1, vertex, spt2);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject spt1, SpPointObject vertex, SpPointObject spt2)
		{
			Alter(0.4, false, false, spt1, vertex, spt2);
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(double size, bool isOpposite, bool isOriented, SpPointObject spt1, SpPointObject vertex, SpPointObject spt2)
		{
			_isOpposite = isOpposite; _sizeRadius = size; _vertex = vertex; _point1 = spt1; _point2 = spt2; _isOriented = isOriented;
			EndAlterProcess(_vertex, _point1, _point2, null, _pointForBissector);
		}

		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Sort si même coordonnées:
			if (_vertex.Coordinates == _point1.Coordinates || _vertex.Coordinates == _point2.Coordinates)
				{ SendCalculationResult(true, "Points have same coordinates."); return; }
			// Calcule l'angle:
			_radValue = GeoFunctions.GetAngleMeasure(_point1.Coordinates - _vertex.Coordinates,
				_point2.Coordinates - _vertex.Coordinates, true, true);
			if (_isOpposite) { _radValue = Math.PI * 2.0 - _radValue; }
			
			// Si l'angle vaut 0 ou 180°, on ne peut pas l'afficher:
			if (_radValue == 0 || _radValue == Math.PI) {
				_ptsOnWinCoords = new Coord3D[0];
				_pointForBissector.AlterXYZ(_vertex.X, _vertex.Y, _vertex.Z);
				_pointForBissector.Recalculate(true);
				_labelOrigin = _pointForBissector.Coordinates; }
			// Sinon, on calcule des points pour dessiner l'arc de cercle:
			else { CalculatePointsOnWinCoords(); }
			
			SendCalculationResult();
		}
		
		/// <summary>
		/// Calcule une série de points sur l'arc de cercle, ou des points pour former un carré, si l'objet n'est pas virtuel, système ou caché.
		/// </summary>
		protected void CalculatePointsOnWinCoords()
		{
			// Sort si on ne doit pas calculer:
			if (IsUndefined || IsVirtual || IsSystem || Hidden) { _ptsOnWinCoords = new Coord3D[0]; return; }
			// Cherche le vecteur correspondant à k(vec1), en fonction de la taille souhaitée:
			Coord3D xVec = _point1.Coordinates - _vertex.Coordinates;
			xVec = (_sizeRadius / xVec.GetNorm()) * xVec;
			
			// Si 90°, on trace un parallélogramme:
			if (Maths.Approx(_radValue, Math.PI / 2) || (_radValue < 0 && Maths.Approx(_radValue, -Math.PI / 2)))
			{
				Coord3D yVec = _point2.Coordinates - _vertex.Coordinates;
				yVec = (_sizeRadius / yVec.GetNorm()) * yVec;
				_ptsOnWinCoords = new Coord3D[3];
				GeoFunctions.GetParallelogramCoords(_vertex.Coordinates, 1, xVec, 1, yVec, out _ptsOnWinCoords[0], out _ptsOnWinCoords[2],
					out _ptsOnWinCoords[1]);
				// Supprime la flèche:
				_pen.StartCap = _pen.EndCap = LineCap.NoAnchor;
				// Point de la bissectrice et origine du label:
				_pointForBissector.AlterCoords(_ptsOnWinCoords[1]);
				_pointForBissector.Recalculate(true);
				_labelOrigin = _ptsOnWinCoords[1];
			}
			// Sinon:
			else
			{
				// Met éventuellement une flèche:
				if (_isOriented && this is SpFixedAngle && _radValue < 0)
					{ _pen.CustomStartCap = DrawingArea.CustomArrowCap; _pen.EndCap = LineCap.NoAnchor; }
				else if (_isOriented)
					{ _pen.StartCap = LineCap.NoAnchor; _pen.CustomEndCap = DrawingArea.CustomArrowCap; }
				else
					{ _pen.StartCap = _pen.EndCap = LineCap.NoAnchor; }
				// Cherche le vecteur yVec pour créer un repère orthonormal:
				Coord3D yVec;
				if (this is SpAngleOnPlane) {
					SpPlaneObject plane = ((SpAngleOnPlane)this).Plane;
					Coord2D xVec2D = plane.To2D(xVec);
					yVec = plane.To3D(-xVec2D.Y, xVec2D.X);
					yVec = (_sizeRadius / yVec.GetNorm()) * yVec; }
				else if (this is SpFixedAngle) {
					yVec = GeoFunctions.GetOrthonormalVectorCoords(xVec, _vertex.Coordinates, ((SpFixedAngle)this).PlanePoint.Coordinates,
					false); }
				else {
					yVec = GeoFunctions.GetOrthonormalVectorCoords(xVec, _vertex.Coordinates, _point2.Coordinates,
					false); }
				// Calcule la série de points:
				_ptsOnWinCoords = GeoFunctions.GetRotatedPointsCoordsCreatingPlane(xVec, yVec, _vertex.Coordinates,
					Math.Min(_radValue,	0), Math.Max(_radValue,	0), 0.1);
				// Point de la bissectrice et origine du label:
				_pointForBissector.AlterCoords(GeoFunctions.GetRotatedPointCoordsCreatingPlane(_vertex.Coordinates, xVec, yVec, _radValue / 2));
				_pointForBissector.Recalculate(true);
				_labelOrigin = _pointForBissector.Coordinates;
			}
		}

		/// <summary>
		/// Retourne une chaîne commune aux points décrivant les coordonnées du point.
		/// </summary>
		public virtual string BaseToString()
		{
			return FormatText("({0},{1},{2}) of {3} rad", _point1, _vertex, _point2, _radValue);
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
			{ return new object[]{_sizeRadius, _isOpposite, _isOriented, _point1, _vertex, _point2}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string radVal = FormatText("Value: {0} rad, {1}, {2}", _radValue, MathsGeo.GetAngleBounds(_radValue, 12, false, Name),
				MathsGeo.GetAngleBounds(_radValue, 12, true, Name));
			string defVal = FormatText("Value: {0}°", MathsGeo.RadToDeg(_radValue));
			return base.GetInfos(radVal, defVal, lines);
		}
		
	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// L'angle est défini par une valeur donnée et par deux points. Définit simplement le Point2 dans l'objet de base, ce point étant l'image de Point1 (donné par l'utilisateur) par une rotation dont le centre est le sommet de l'angle, et la valeur la valeur de l'angle.
	/// </summary>
	public class SpFixedAngle : SpAngle
	{
	
		protected DoubleF _fixedValueDblF;
		protected SpPointObject _planePoint;
		
		/// <summary>
		/// Retourne le point qui fait plan.
		/// </summary>
		public SpPointObject PlanePoint { get { return _planePoint; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpFixedAngle(string name, double size, bool isOriented, SpPointObject spt1, SpPointObject vertex, SpPointObject planePt,
			DoubleF value)
			: base(name)
		{
			_point2 = new SpPointObject("%pt2", 0, 0, 0); _isOpposite = false;
			Alter(size, isOriented, spt1, vertex, planePt, value);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(double size, bool isOriented, SpPointObject spt1, SpPointObject vertex, SpPointObject planePt, DoubleF value)
		{
			_sizeRadius = size; _vertex = vertex; _point1 = spt1; _fixedValueDblF = value; _planePoint = planePt;
			_isOriented = isOriented;
			EndAlterProcess(_vertex, _point1, planePt, GetObjectsFromFormula(_fixedValueDblF), null, _point2, _pointForBissector);
		}

		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			if (_vertex.Coordinates == _point1.Coordinates || _vertex.Coordinates == _planePoint.Coordinates)
				{ SendCalculationResult(true, "Points have same coordinates."); return; }
			// Recalcule le DoubleF, puis définit le Point2 comme image de Point1 par une rotation:
			_radValue = _fixedValueDblF.Recalculate();
			if (_fixedValueDblF.IsNaN) { SendCalculationResult(true, "Value is not valid."); return; }
			Coord3D pt2 = GeoFunctions.GetRotatedPointCoordsCreatingPlane(_radValue, _vertex.Coordinates, _point1.Coordinates, _planePoint.Coordinates);
			if (pt2.Empty) { SendCalculationResult(true, String.Format("Points {0}, {1} and {2} are aligned",
				_point1.Name, _vertex.Name, _planePoint.Name)); return; }
			_point2.AlterCoords(pt2);
			_point2.Recalculate(true);
			// Calcule les points à afficher:
			CalculatePointsOnWinCoords();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _fixedValueDblF, oldName, newName);
		}
				
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_sizeRadius, _isOriented, _point1, _vertex, _planePoint, _fixedValueDblF}; }
		
	}
	

	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// L'angle est défini sur un plan, ce qui permet d'avoir des angles supérieurs à π.
	/// </summary>
	public class SpAngleOnPlane : SpAngle
	{
	
		protected SpPlaneObject _plane;
		protected bool _onlyPos;
		
		/// <summary>
		/// Retourne le plan.
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpAngleOnPlane(string name) : base(name)
			{ ; }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpAngleOnPlane(string name, double size, bool isOriented, SpPointOnPlaneObject spt1,
			SpPointOnPlaneObject vertex, SpPointOnPlaneObject spt2, bool onlyPos) : this(name)
		{
			_isOpposite = false;
			Alter(size, isOriented, spt1, vertex, spt2, onlyPos);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(double size, bool isOriented, SpPointOnPlaneObject spt1,
			SpPointOnPlaneObject vertex, SpPointOnPlaneObject spt2, bool onlyPos)
		{
			_sizeRadius = size; _vertex = vertex; _point1 = spt1; _point2 = spt2; _isOriented = isOriented;
			_plane = ((SpPointOnPlaneObject)_vertex).Plane; _onlyPos = onlyPos;
			EndAlterProcess(_vertex, _point1, _point2, null, _pointForBissector);
		}

		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Objets récupérés:
			SpPointOnPlaneObject vertex = (SpPointOnPlaneObject)_vertex;
			SpPointOnPlaneObject pt1 = (SpPointOnPlaneObject)_point1;
			SpPointOnPlaneObject pt2 = (SpPointOnPlaneObject)_point2;
			_plane = vertex.Plane;
			// Test du même coordonnées:
			if (_vertex.Coordinates == _point1.Coordinates || _vertex.Coordinates == _point2.Coordinates)
				{ SendCalculationResult(true, "Points have same coordinates."); return; }
			// Indéfini si les points ne sont pas sur le même plan:
			if (pt1.Plane != _plane || pt2.Plane != _plane)
				{ SendCalculationResult(true, "Points are not on the same plane!"); return; }
			// Calcule l'angle: Définit un nouveau repère orthonormal 2D basé sur vertex/pt1, puis obtient les
			// coordonnées polaires de pt2 dans ce repère. Cette méthode permet d'avoir tous les angles de 0 à 2π:
			Coord2D xVec = pt1.CoordinatesOnPlane - vertex.CoordinatesOnPlane;
			Coord2D yVec = new Coord2D(-xVec.Y, xVec.X);
			Coord2D pt2p = GeoFunctions.ChangeCoordinatesSystem(vertex.CoordinatesOnPlane, xVec, yVec, pt2.CoordinatesOnPlane);
			if (pt2p.Empty) { SendCalculationResult(true, "An error occured."); return; }
			_radValue = MathsGeo.ToPolar(pt2p.X, pt2p.Y, false);
			if (!_onlyPos && _radValue > Math.PI) { _radValue -= 2 * Math.PI; }
			// Calcule des points pour dessiner l'arc de cercle:
			CalculatePointsOnWinCoords();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_sizeRadius, _isOriented, _point1, _vertex, _point2, _onlyPos}; }
		
	}
	

	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// L'angle est défini sur un plan et avec une valeur définie.
	/// </summary>
	public class SpFixedAngleOnPlane : SpAngleOnPlane
	{
	
		protected DoubleF _fixedValueDblF;
				
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpFixedAngleOnPlane(string name, double size, bool isOriented, SpPointOnPlaneObject spt1,
			SpPointOnPlaneObject vertex, DoubleF value)
			: base(name)
		{
			Alter(size, isOriented, spt1, vertex, value);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(double size, bool isOriented, SpPointOnPlaneObject spt1, SpPointOnPlaneObject vertex,
			DoubleF value)
		{
			_fixedValueDblF = value; _sizeRadius = size; _vertex = vertex; _point1 = spt1; _isOriented = isOriented; _isOpposite = false;
			_plane = ((SpPointOnPlaneObject)_vertex).Plane;
			if (_point2 == null) { _point2 = new SpPointOnPlaneObject("%pt2", _plane, 0, 0); }
			else { _point2.RebuildObject(false, _plane); }
			EndAlterProcess(_vertex, _point1, _plane, GetObjectsFromFormula(_fixedValueDblF), null, _point2, _pointForBissector);
		}

		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Objets récupérés:
			SpPointOnPlaneObject vertex = (SpPointOnPlaneObject)_vertex;
			SpPointOnPlaneObject pt1 = (SpPointOnPlaneObject)_point1;
			SpPointOnPlaneObject pt2 = (SpPointOnPlaneObject)_point2;
			_plane = vertex.Plane;
			// Test du même coordonnées:
			if (_vertex.Coordinates == _point1.Coordinates)
				{ SendCalculationResult(true, "Points have same coordinates."); return; }
			// Indéfini si les points ne sont pas sur le même plan:
			if (pt1.Plane != _plane)
				{ SendCalculationResult(true, "Points are not on the same plane!"); return; }
			// Recalcule le DoubleF, puis définit le Point2 comme image de Point1 par une rotation:
			_radValue = _fixedValueDblF.Recalculate();
			if (_fixedValueDblF.IsNaN) { SendCalculationResult(true, "Value is not valid."); return; }
			pt2.AlterCoords(GeoFunctions.GetRotatedPtCoords(vertex.CoordinatesOnPlane, pt1.CoordinatesOnPlane,
					_plane.Origin.Coordinates, _plane.XVector.Coordinates, _plane.YVector.Coordinates, _radValue));
			_point2.Recalculate(true);
			// Calcule des points pour dessiner l'arc de cercle:
			CalculatePointsOnWinCoords();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_sizeRadius, _isOriented, _point1, _vertex, _fixedValueDblF}; }
		
	}
	

	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Cercle.
	/// </summary>
	public class SpCircle : SpPenBrushObject
	{
	
		protected DoubleF _radiusDblF, _minDblF, _maxDblF;
		protected SpPointOnPlaneObject _center;
		protected double _radius, _min, _max;
		protected SpPlaneObject _plane;
		protected Coord3D[] _ptsOnWinCoords;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Circle"; } }
		
		/// <summary>
		/// Centre.
		/// </summary>
		public SpPointOnPlaneObject Center { get { return _center; } }
		
		/// <summary>
		/// Rayon, défini que le cercle est été construit par un rayon ou par un centre et un point.
		/// </summary>
		public double Radius { get { return _radius; } }
		
		/// <summary>
		/// Angle minimum.
		/// </summary>
		public double Min { get { return _min; } }
		
		/// <summary>
		/// Angle maximum.
		/// </summary>
		public double Max { get { return _max; } }
		
		/// <summary>
		/// Plan utilisé.
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }
		
		/// <summary>
		/// Indique si le cercle est complet, ie. s'il va de 0 à 2π.
		/// </summary>
		public bool IsComplete { get { return (_minDblF == DoubleF.Zero && _maxDblF == DoubleF.TwoPi); } }
		
		/// <summary>
		/// Obtient les coordonnées des points à dessiner dans le repère du form.
		/// </summary>
		internal Coord3D[] PointsOnWinCoords { get { return _ptsOnWinCoords; } }
	
		/// <summary>
		/// Obtient ou définit les points à dessiner dans le repère du form.
		/// </summary>
		internal PointF[] PointsOnWin { get; set; }

		/// <summary>
		/// Obtient ou définit les points des polygones à dessiner dans le repère du form.
		/// </summary>
		internal PointF[][] PolygonsOnWin { get; set; }

		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpCircle(string name) : base(name)
		{
			_ptsOnWinCoords = new Coord3D[0];
		}
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpCircle(string name, SpPointOnPlaneObject center, DoubleF radius) : this(name)
		{
			Alter(center, radius);
		}
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpCircle(string name, SpPointOnPlaneObject center, DoubleF radius, DoubleF min, DoubleF max)
			: this(name)
		{
			Alter(center, radius, min, max);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject center, DoubleF radius)
		{
			_center = center; _radiusDblF = radius;
			_minDblF = DoubleF.Zero; _maxDblF = DoubleF.TwoPi;
			EndAlterProcess(_center, GetObjectsFromFormula(_radiusDblF));
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject center, DoubleF radius, DoubleF min, DoubleF max)
		{
			_center = center; _radiusDblF = radius; _minDblF = min; _maxDblF = max;
			EndAlterProcess(_center, GetObjectsFromFormula(_radiusDblF),
				GetObjectsFromFormula(_minDblF), GetObjectsFromFormula(_maxDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets:
			_plane = _center.Plane;
			// Recalcule les DoubleF:
			_radius = _radiusDblF.Recalculate(); _min = _minDblF.Recalculate(); _max = _maxDblF.Recalculate();
			if (DoubleF.IsThereNan(_radius, _min, _max) || _radius < 0)
				{ SendCalculationResult(true, "Radius, min or max is not valid."); return; }
			// Ne calcule rien si on ne doit pas afficher quelque chose:
			if (IsUndefined || IsVirtual || IsSystem || Hidden)
				{ _ptsOnWinCoords = new Coord3D[0]; }
			else
			{
				// Le point d'origine est l'image du centre par la translation de vecteur r(O'I'), où r est le rayon et (O'I')
				// le vecteur directeur des abscisses du plan:
				Coord3D vec1 = _radius * _plane.XVector.Coordinates;
				Coord3D vec2 = _radius * _plane.YVector.Coordinates;
				_ptsOnWinCoords = GeoFunctions.GetRotatedPointsCoordsCreatingPlane(vec1, vec2, _center.Coordinates, _min, _max, 0.1);
				// Origine du label:
				_labelOrigin = GeoFunctions.GetRotatedPointCoordsCreatingPlane(_center.Coordinates, vec1, vec2, Math.Max(_labelOriginParam, _min));
			}
			SendCalculationResult();
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _radiusDblF, oldName, newName);
			ChangeNameInFormula(ref _minDblF, oldName, newName);
			ChangeNameInFormula(ref _maxDblF, oldName, newName);
		}
				
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("({0},{1}) on {2}", _center, _radius, _plane);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
		{
			if (IsComplete) {
				return new object[]{_center, _radiusDblF}; }
			else {
				return new object[]{_center, _radiusDblF, _minDblF, _maxDblF}; }
		}

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string eq = FormatText("Eq.: (x-{0})^2 + (y-{1})^2 = {2}^2", _center.XOnPlane, _center.YOnPlane, _radius);
			string min = FormatText("Min = {0} ; {1}", _min, MathsGeo.GetAngleBounds(_min, 12, false, "min"));
			string max = FormatText("Max = {0} ; {1}", _max, MathsGeo.GetAngleBounds(_max, 12, false, "max"));
			string peri = FormatText("Perimeter: 2πr(max-min)/2π = {0}", (_max - _min) * _radius);
			string totalPeri = FormatText("Total perimeter: 2πr = {0}", 2 * Math.PI * _radius);
			string area = FormatText("Area: πr^2 = {0}", Math.PI * _radius*_radius);
			return base.GetInfos(eq, min, max, peri, totalPeri, area, lines);
		}

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Cercle défini par un centre et un point sur le cercle.
	/// </summary>
	public class SpCircleUsingPoint : SpCircle
	{
	
		protected SpPointOnPlaneObject _point;
	
		/// <summary>
		/// Obtient le point définissant le cercle.
		/// </summary>
		public SpPointOnPlaneObject Point { get { return _point; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpCircleUsingPoint(string name, SpPointOnPlaneObject center, SpPointOnPlaneObject point) : base(name)
		{
			Alter(center, point);
		}
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpCircleUsingPoint(string name, SpPointOnPlaneObject center, SpPointOnPlaneObject point, DoubleF min,
			DoubleF max) : base(name)
		{
			Alter(center, point, min, max);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject center, SpPointOnPlaneObject point)
		{
			_center = center; _point = point;
			_minDblF = DoubleF.Zero; _maxDblF = DoubleF.TwoPi;
			EndAlterProcess(_center, _point);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject center, SpPointOnPlaneObject point, DoubleF min, DoubleF max)
		{
			_center = center; _point = point;_minDblF = min; _maxDblF = max;
			EndAlterProcess(_center, _point,
				GetObjectsFromFormula(_minDblF), GetObjectsFromFormula(_maxDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets:
			_plane = _center.Plane;
			// Vérifie que le centre et le point sont sur le même plan (le même objet), et calcule le rayon:
			if (_center.Plane != _point.Plane)
				{ SendCalculationResult(true, "Center and point are not on same plane."); return; }
			// Recalcule center, min et max:
			_radius = _center.Coordinates.GetLength(_point.Coordinates);
			_min = _minDblF.Recalculate(); _max = _maxDblF.Recalculate();
			if (DoubleF.IsThereNan(_radius, _min, _max))
				{ SendCalculationResult(true, "Radius, min or max is not valid."); return; }			
			// Ne calcule rien si on ne doit pas afficher quelque chose:
			if (IsUndefined || IsVirtual || IsSystem || Hidden)
				{ _ptsOnWinCoords = new Coord3D[0]; }
			else
			{
				// On pourrait effectuer une rotation du point sur le cercle (qui a une rotation de 0 rad) tout au long du cercle,
				// mais c'est plus long. La méthode suivante est plus rapide:
				// Calcule le point du vecteur entre le centre et le point sur le cercle, puis obtient le vecteur orthonormal:
				Coord3D vec1 = _point.Coordinates - _center.Coordinates;
				Coord2D vec12D = _plane.To2D(vec1);
				Coord3D vec2 = _plane.To3D(-vec12D.Y, vec12D.X);
				//Coord3D vec2 = _plane.To3D(GeoFunctions.GetRotPtCoordsUsingCoordSys(_center.CoordinatesOnPlane,
					//_point.CoordinatesOnPlane, Math.PI / 2)) - _center.Coordinates;
				_ptsOnWinCoords = GeoFunctions.GetRotatedPointsCoordsCreatingPlane(vec1, vec2, _center.Coordinates, _min, _max, 0.1);
				// Origine du label:
				_labelOrigin = GeoFunctions.GetRotatedPointCoordsCreatingPlane(_center.Coordinates, vec1, vec2, Math.Max(_labelOriginParam, _min));
			}
			SendCalculationResult();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("({0},{1}) on {2}, through", _center, _radius, _plane, _point);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
		{
			if (_minDblF == DoubleF.Zero && _maxDblF == DoubleF.TwoPi) {
				return new object[]{_center, _point}; }
			else {
				return new object[]{_center, _point, _minDblF, _maxDblF}; } 
		}

	}


	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Ellipse.
	/// </summary>
	public class SpEllipse : SpCircle
	{
	
		protected DoubleF _widthDblF, _heightDblF, _alphaDblF;
		protected double _width, _height, _alpha;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Ellipse"; } }
		
		/// <summary>
		/// Largeur.
		/// </summary>
		public double EllipseWidth { get { return _width; } }
		
		/// <summary>
		/// Hauteur.
		/// </summary>
		public double EllipseHeight { get { return _height; } }
		
		/// <summary>
		/// Angle de rotation alpha.
		/// </summary>
		public double Alpha { get { return _alpha; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpEllipse(string name, SpPointOnPlaneObject center, DoubleF width, DoubleF height) : base(name)
		{
			Alter(center, width, height);
		}
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpEllipse(string name, SpPointOnPlaneObject center, DoubleF width, DoubleF height, DoubleF min,
			DoubleF max, DoubleF alpha) : base(name)
		{
			Alter(center, width, height, min, max, alpha);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject center, DoubleF width, DoubleF height)
		{
			_center = center; _widthDblF = width; _heightDblF = height; _plane = _center.Plane;
			_minDblF = DoubleF.Zero; _maxDblF = DoubleF.TwoPi; _alphaDblF = new DoubleF();
			EndAlterProcess(_center, GetObjectsFromFormula(_widthDblF), GetObjectsFromFormula(_heightDblF));
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject center, DoubleF width, DoubleF height, DoubleF min, DoubleF max,
			DoubleF alpha)
		{
			_center = center; _widthDblF = width; _heightDblF = height; _minDblF = min; _maxDblF = max;
			_alphaDblF = alpha;
			EndAlterProcess(_center, GetObjectsFromFormula(_widthDblF), GetObjectsFromFormula(_heightDblF),
				GetObjectsFromFormula(_minDblF), GetObjectsFromFormula(_maxDblF), GetObjectsFromFormula(_alphaDblF));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objets:
			_plane = _center.Plane;
			// Recalcule les DoubleF:
			_width = _widthDblF.Recalculate(); _height = _heightDblF.Recalculate(); _min = _minDblF.Recalculate(); _max = _maxDblF.Recalculate();
			_alpha = _alphaDblF.Recalculate();
			if (DoubleF.IsThereNan(_width, _height, _min, _max, _alpha) || _width < 0 || _height < 0)
				{ SendCalculationResult(true, "Width, height, min, max, or alpha is not valid."); return; }
			// Ne calcule rien si on ne doit pas afficher quelque chose:
			if (IsUndefined || IsVirtual || IsSystem || Hidden)
				{ _ptsOnWinCoords = new Coord3D[0]; }
			else
			{
				// Le point d'origine est l'image du centre par la translation de vecteur r(O'I'), où r est le rayon et (O'I')
				// le vecteur directeur des abscisses du plan:
				Coord2D vec1 = new Coord2D(_width, 0);
				Coord2D vec2 = new Coord2D(0, _height);
				Coord2D[] coords = GeoFunctions.GetRotatedPointsCoordsCreatingPlane(vec1, vec2, _center.CoordinatesOnPlane, _min, _max, 0.1);
				int l = coords.Length; _ptsOnWinCoords = new Coord3D[l];
				Coord2D center = _center.CoordinatesOnPlane;
				for (int i=0; i<l; i++) {
					if (_alpha != 0) { coords[i] = GeoFunctions.GetRotatedPtCoords(center, coords[i], _alpha); }
					_ptsOnWinCoords[i] = _plane.To3D(coords[i]); }
				// Origine du label:
				Coord2D lbl = GeoFunctions.GetRotatedPointCoordsCreatingPlane(center, vec1, vec2, Math.Max(_labelOriginParam, _min));
				if (_alpha != 0) { lbl = GeoFunctions.GetRotatedPtCoords(center, lbl, _alpha); }
				_labelOrigin = _plane.To3D(lbl);
			}
			SendCalculationResult();
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _widthDblF, oldName, newName);
			ChangeNameInFormula(ref _heightDblF, oldName, newName);
			ChangeNameInFormula(ref _minDblF, oldName, newName);
			ChangeNameInFormula(ref _maxDblF, oldName, newName);
			ChangeNameInFormula(ref _alphaDblF, oldName, newName);
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("({0},{1},{2}) on {3}", _center, _width, _height, _plane);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
		{
			if (IsComplete && _alpha == 0) {
				return new object[]{_center, _widthDblF, _heightDblF}; }
			else {
				return new object[]{_center, _widthDblF, _heightDblF, _minDblF, _maxDblF, _alphaDblF}; }
		}

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string min = FormatText("Min = {0} ; {1}", _min, MathsGeo.GetAngleBounds(_min, 12, false, "min"));
			string max = FormatText("Max = {0} ; {1}", _max, MathsGeo.GetAngleBounds(_max, 12, false, "min"));
			string area = FormatText("Area: π(width/2)(height/2) = {0}", Math.PI * _width * _height / 4);
			string alpha = String.Format("Alpha: {0}", MathsGeo.GetAngleBounds(_alpha, 12, false, "α"));
			return base.GetInfos(min, max, area, alpha, lines);
		}

	}


	// ---------------------------------------------------------------------------

	
	/// <summary>
	/// Intersection d'un plan et d'une sphère.
	/// </summary>
	public class SpPlaneSphereIntersection : SpCircle
	{
	
		protected SpSphere _sphere;
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Plane and sphere intersection"; } }
		
		/// <summary>
		/// Sphere.
		/// </summary>
		public SpSphere Sphere { get { return _sphere; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPlaneSphereIntersection(string name, SpPlaneObject plane, SpSphere sphere) : base(name)
		{
			Alter(plane, sphere);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPlaneObject plane, SpSphere sphere)
		{
			_plane = plane; _sphere = sphere; _minDblF = DoubleF.Zero; _maxDblF = DoubleF.TwoPi;
			if (_center == null) { _center = new SpPointOnPlaneObject("%center", _plane, 0, 0); }
			else { _center.RebuildObject(false, _plane); }
			EndAlterProcess(_plane, _sphere, null, _center);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Obtient le centre et le rayon:
			Coord2D center; double radius;
			if (!GeoFunctions.GetPlaneSphereInterCoords(_plane.Origin.Coordinates, _plane.XVector.Coordinates, _plane.YVector.Coordinates,
				_sphere.Center.Coordinates, _sphere.Radius, out center, out radius))
				{ SendCalculationResult(true, "Intersection not found."); return; }
			_radiusDblF.Value = radius;
			_center.AlterCoords(center);
			_center.Recalculate(true);
			// Appel la base pour le dessin:
			base.CalculateNumericData();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("({0},{1}) between {2} and {3}", _center, _radius, _plane, _sphere);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_plane, _sphere}; }

	}
	
		
	
	#endregion CLASSES D'OBJETS DU PLAN


	
}
