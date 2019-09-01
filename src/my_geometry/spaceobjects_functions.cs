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
	// FONCTIONS DU PLAN
	// ---------------------------------------------------------------------------




	#region FONCTIONS DU PLAN



	/// <summary>
	/// Classe de base pour les fonctions à une, deux et trois inconnues (x, y et z). Contient des méthodes de préparation pour les trois types de fonctions. Il faut remplir _unknowNb;
	/// </summary>
	public class SpFunctionObject : SpPenBrushObject
	{
	
		protected int _unknowNb;
		protected double _resX, _resY, _resZ, _minX, _maxX, _minY, _maxY, _minZ, _maxZ;
		protected DoubleF _minXDblF, _maxXDblF, _minYDblF, _maxYDblF, _minZDblF, _maxZDblF;
		protected float _tensionOnWin;
		protected Coord3D[] _ptsOnWinCoords;
		protected string _formula;
		protected Func<double,double> _method1;
		protected Func<double,double,double> _method2;
		protected Func<double,double,double,double> _method3;

		/// <summary>
		/// Réécriture de la propriété pour demander le recalcule des données d'affichage (puisque la couleur détermine s'il faut afficher des lignes ou des points, ou des polygones, il faut demander un calcul avant le redessin).
		/// </summary>
		public override Color Color {
			get { return base.Color; }
			internal set { base.Color = value; OnRequestDrawingCalc(this); } }
		
		/// <summary>
		/// Réécriture de la propriété pour demander le recalcule des données d'affichage (puisque la couleur détermine s'il faut afficher des lignes ou des points, ou des polygones, il faut demander un calcul avant le redessin).
		/// </summary>
		public override Color BackColor {
			get { return base.BackColor; }
			set { base.BackColor = value; OnRequestDrawingCalc(this); } }

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Function"; } }
		
		/// <summary>
		/// Obtient la valeur minimum définie.
		/// </summary>
		public double MinX { get { return _minX; } }
		
		/// <summary>
		/// Obtient la valeur maximum définie.
		/// </summary>
		public double MaxX { get { return _maxX; } }
		
		/// <summary>
		/// Obtient la valeur minimum définie.
		/// </summary>
		public double MinY { get { return _minY; } }
		
		/// <summary>
		/// Obtient la valeur maximum définie.
		/// </summary>
		public double MaxY { get { return _maxY; } }
		
		/// <summary>
		/// Obtient la valeur minimum définie.
		/// </summary>
		public double MinZ { get { return _minZ; } }
		
		/// <summary>
		/// Obtient la valeur maximum définie.
		/// </summary>
		public double MaxZ { get { return _maxZ; } }
		
		/// <summary>
		/// Obtient la tension pour le dessin des courbes (paramètre "tension" dans les g.DrawCruve, où g est un Graphics).
		/// </summary>
		public float TensionOnWin { get { return _tensionOnWin; } }
		
		/// <summary>
		/// Obtient les points calculés de la courbe représentative de la fonction.
		/// </summary>
		internal Coord3D[] PointsOnWinCoords { get { return _ptsOnWinCoords; } }
		
		/// <summary>
		/// Obtient ou définit les points dans le repère 2D du form.
		/// </summary>
		internal PointF[] PointsOnWin { get; set; }
		
		/// <summary>
		/// Obtient ou définit les différentes lignes à tracer.
		/// </summary>
		internal PointF[][] LinesOnWin { get; set; }
		
		/// <summary>
		/// Obtient ou définit les différents polygones à tracer.
		/// </summary>
		internal PointF[][] PolygonsOnWin { get; set; }
		
		/// <summary>
		/// Constructeur protégé.
		/// </summary>
		protected SpFunctionObject(string name) : base(name)
		{
			_ptsOnWinCoords = new Coord3D[0];
			LinesOnWin = new PointF[0][];
			PolygonsOnWin = new PointF[0][];
			_unknowNb = 4;
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées. Cette méthode recalcule l'ensemble des min et max, et appelle CalculatePointsOnWinCoords et SendCalculationResult.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule min et max:
			if (_unknowNb >= 1) {
				_minX = _minXDblF.Recalculate(); _maxX = _maxXDblF.Recalculate();
				if (DoubleF.IsThereNan(_minX, _maxX)) { SendCalculationResult(true, "Min or max not valid."); return; } }
			if (_unknowNb >= 2) {
				_minY = _minYDblF.Recalculate(); _maxY = _maxYDblF.Recalculate();
				if (DoubleF.IsThereNan(_minY, _maxY)) { SendCalculationResult(true, "Min or max not valid."); return; } }
			if (_unknowNb >= 3) {
				_minZ = _minZDblF.Recalculate(); _maxZ = _maxZDblF.Recalculate();
				if (DoubleF.IsThereNan(_minZ, _maxZ)) { SendCalculationResult(true, "Min or max not valid."); return; } }
			// Calcule les points pour l'affichage de la fonction:
			CalculatePointsOnWinCoords();
			SendCalculationResult();
		}

		/// <summary>
		/// Méthode à overrider.
		/// </summary>
		protected virtual void CalculatePointsOnWinCoords()
			{ ; }
		
		/// <summary>
		/// Change la formule, pour les trois types de fonctions. Inutile de l'overrider, donc. Retourne true si réussie.
		/// </summary>
		public virtual bool AlterFormula(string formula)
		{
			// Tente de construire une formule, puis fait un test. Ce test permet de déterminer si un SpObjectNotFound est levé. Si
			// c'est le cas, retourne false.
			_formula = formula; double test;
			try
			{
				switch (_unknowNb)
				{
					case 1:
						_method1 = (Func<double,double>)Formula.CreateFormulaMethod(formula, new string[]{"x"}, typeof(Func<double,double>),
							typeof(SpFunctionObject), null, FormulaWorkingType.Double, null, null);
							test = _method1(0);
						break;
					case 2:
						_method2 = (Func<double,double,double>)Formula.CreateFormulaMethod(formula, new string[]{"x","y"}, typeof(Func<double,double,double>),
							typeof(SpFunctionObject), null, FormulaWorkingType.Double, null, null);
						test = _method2(0, 0);
						break;
					case 3:
						_method3 = (Func<double,double,double,double>)Formula.CreateFormulaMethod(formula, new string[]{"x","y","z"},
							typeof(Func<double,double,double,double>), typeof(SpFunctionObject), null, FormulaWorkingType.Double, null, null);
						test = _method3(0, 0, 0);
						break;
					default:
						return false;
				}
				return true;
			}
			catch (Exception exc)
			{
				My.ErrorHandler.ShowError(exc); _ptsOnWinCoords = new Coord3D[0];
				return false;
			}
		}
				
		/// <summary>
		/// Modifie les valeurs Min et Max.
		/// </summary>
		public void AlterMinMax(double minX, double maxX)
			{ _minXDblF.Value = minX; _maxXDblF.Value = maxX; }
		
		/// <summary>
		/// Retourne la valeur de la fonction pour une valeur donnée. Retourne NaN si échec.
		/// </summary>
		public double GetFunctionValue(double x)
		{
			double result;
			try { if (DoubleF.IsThereNan(result = _method1(x))) { return Double.NaN; } else { return result; } }
			catch { return Double.NaN; }
		}
		
		/// <summary>
		/// Retourne la valeur de la fonction pour une valeur donnée. Retourne Single.MaxValue si échec.
		/// </summary>
		public double GetFunctionValue(double x, double y)
		{
			double result;
			try { if (DoubleF.IsThereNan(result = _method2(x, y))) { return Double.NaN; } else { return result; } }
			catch { return Double.NaN; }
		}
		
		/// <summary>
		/// Retourne la valeur de la fonction pour une valeur donnée. Retourne Single.MaxValue si échec.
		/// </summary>
		public double GetFunctionValue(double x, double y, double z)
		{
			double result;
			try { if (DoubleF.IsThereNan(result = _method3(x, y, z))) { return Double.NaN; } else { return result; } }
			catch { return Double.NaN; }
		}

		/// <summary>
		/// Retourne un tableau contenant l'ensemble des résultats valides calculés entre min et max.
		/// </summary>
		public string[] GetArrayValues(decimal minX, decimal maxX, decimal resX)
		{
			// Sort si pas la bonne fonction:
			if (!(this is SpFunction1OnPlane)) { return new string[]{"Function must be a function with 1 variable"}; }
			int c = 0; double y;
			string[] result = new string[100];
			result[c++] = "+--------------------+--------------------+";
			result[c++] = "|         x          |          y         |";
			result[c++] = "+--------------------+--------------------+";
			for (decimal x=minX; x<=maxX; x+=resX)
			{
				y = GetFunctionValue((double)x);
				if (c >= result.Length) { Array.Resize(ref result, c + 100); }
				if (DoubleF.IsThereNan(y)) { result[c++] = String.Format("|{0,20}|{1,20}|", x.ToString("N10"), "ERROR"); }
				else { result[c++] = String.Format("|{0,20}|{1,20}|", x.ToString("N10"), y.ToString("N10")); }
			}
			Array.Resize(ref result, c+1);
			result[result.Length-1] = "+--------------------+--------------------+";
			return result;
		}

		/// <summary>
		/// Retourne un tableau contenant l'ensemble des résultats valides calculés entre min et max.
		/// </summary>
		public string[] GetArrayValues(decimal minX, decimal maxX, decimal resX, decimal minY, decimal maxY, decimal resY)
		{
			// Sort si pas la bonne fonction:
			if (!(this is SpFunction2)) { return new string[]{"Function must be a function with 2 variables"}; }
			int c = 0; double z;
			string[] result = new string[100];
			result[c++] = "+--------------------+--------------------+--------------------+";
			result[c++] = "|         x          |          y         |          z         |";
			result[c++] = "+--------------------+--------------------+--------------------+";
			for (decimal x=minX; x<=maxX; x+=resX)
			{
				for (decimal y=minY; y<=maxY; y+=resY)
				{
					z = GetFunctionValue((double)x, (double)y);
					if (c >= result.Length) { Array.Resize(ref result, c + 100); }
					if (DoubleF.IsThereNan(z)) { result[c++] = String.Format("|{0,20}|{1,20}|{2,20}|", x.ToString("N10"), y.ToString("N10"), "ERROR"); }
					else { result[c++] = String.Format("|{0,20}|{1,20}|{1,20}|", x.ToString("N10"), y.ToString("N10"), z.ToString("N10")); }
				}
			}
			Array.Resize(ref result, c+1);
			result[result.Length-1] = "+--------------------+--------------------+--------------------+";
			return result;
		}

		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			if (_unknowNb >= 1) {
				ChangeNameInFormula(ref _minXDblF, oldName, newName);
				ChangeNameInFormula(ref _maxXDblF, oldName, newName); }
			if (_unknowNb >= 2) {
				ChangeNameInFormula(ref _minYDblF, oldName, newName);
				ChangeNameInFormula(ref _maxYDblF, oldName, newName); }
			if (_unknowNb >= 3) {
				ChangeNameInFormula(ref _minZDblF, oldName, newName);
				ChangeNameInFormula(ref _maxZDblF, oldName, newName); }
			ChangeNameInFormula(ref _formula, oldName, newName);
			AlterFormula(_formula);
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("defined by {0}", _formula);
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
			string minmaxX, minmaxY, minmaxZ, nb;
			nb = FormatText("Number of points displayed: {0}", _ptsOnWinCoords.Length);
			minmaxX = FormatText("MinX = {0}; MaxX = {1}; ResX = {2}", _minX, _maxX, _resX);
			minmaxY = FormatText("MinY = {0}; MaxY = {1}; ResY = {2}", _minY, _maxY, _resY);
			minmaxZ = FormatText("MinY = {0}; MaxY = {1}; ResY = {2}", _minY, _maxY, _resY);
			if (this is SpFunction1OnPlane) { return base.GetInfos(minmaxX, nb, lines); }
			else if (this is SpFunction2 || this is SpFunction2OnPlane) { return base.GetInfos(minmaxX, minmaxY, nb, lines); }
			else { return base.GetInfos(minmaxX, minmaxY, minmaxZ, nb, lines); }
		}

	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Trace une fonction dans un plan 2D de l'espace.
	/// </summary>
	public class SpFunction1OnPlane : SpFunctionObject
	{
	
		protected SpPlaneObject _plane;
		protected bool _errorOccurred;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Function 1 on plane"; } }
		
		/// <summary>
		/// Obtient s'il y a des structures Empty dans PointsOnWinCoords après le calcul du tableau.
		/// </summary>
		internal bool ErrorOccurred { get { return _errorOccurred; } }
		
		/// <summary>
		/// Obtient le plan utilisé.
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpFunction1OnPlane(string name, SpPlaneObject plane, DoubleF minX, DoubleF maxX, double res, float tension, string formula) : base(name)
		{
			_unknowNb = 1; base.BackColor = Color.FromArgb(0, BackColor); base.Color = Color.FromArgb(255, Color);
			Alter(plane, minX, maxX, res, tension, formula);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPlaneObject plane, DoubleF minX, DoubleF maxX, double resX, float tension, string formula)
		{
			_minXDblF = minX; _maxXDblF = maxX; _resX = resX; _tensionOnWin = tension; _plane = plane;
			if (!AlterFormula(formula)) { SendCalculationResult(true, null); return; }
			EndAlterProcess(_plane, GetObjectsFromFormula(_minXDblF), GetObjectsFromFormula(_maxXDblF),
				GetObjectsFromFormula(_formula));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			base.CalculateNumericData();
		}

		/// <summary>
		/// Calcule une série de points sur le tracé de la fonction, si l'objet n'est pas virtuel, système ou caché. Créé le tableau PointsOnWinCoords, qui contient les points calculés, dans l'ordre des abscisses. Lorsqu'il y a erreur, l'entrée du tableau correspondante contient une strucuture Empty, et c'est généralemnet le signe qu'il faut tracer des lignes séparées.
		/// </summary>
		protected override void CalculatePointsOnWinCoords()
		{
		
			// Sort si on ne doit pas calculer:
			if (IsUndefined || IsVirtual || IsSystem || Hidden) { _ptsOnWinCoords = new Coord3D[0]; return; }
						
			// Calcule les points pour l'affichage de la fonction:
			_errorOccurred = false;
			_ptsOnWinCoords = new Coord3D[100]; int c = 0; double y;
			double max = _maxX + _resX;
			
			// Pour toutes les valeurs de x:
			for (double x=_minX; x<max; x+=_resX)
			{
				// Si on arrive à la fin, met la dernière valeur sur la ligne (sinon, il y a un blanc),
				// tout en veillant à ce que les deux dernières valeurs ne soit pas trop proche (sinon,
				// le dessin DrawCurve dépasse de la ligne):
				if (x > _maxX) {
					if (_maxX-x+_resX < 0.05) { break; }
					x = _maxX; }
				// Détermine une valeur exacte, car il est important ici d'avoir des valeurs exactes
				// (gestion des erreurs, etc.)
				x = (double)(decimal)x;
				// Tente un calcul:
				if (c >= _ptsOnWinCoords.Length) { Array.Resize(ref _ptsOnWinCoords, c + 100); }
				try
				{
					y = _method1(x); // Calcule y
					// Si erreur, on enregistre une structure vide, sinon on calcul les coordonées du nouveau point:
					if (!DoubleF.IsThereNan(y)) { _ptsOnWinCoords[c++] = _plane.To3D(x, y); }
					else if (c > 0) { _ptsOnWinCoords[c++] = new Coord3D(true); _errorOccurred = true; }
				}
				// Si une autre erreur, on enregistre une structure vide
				catch { if (c > 0) { _ptsOnWinCoords[c++] = new Coord3D(true); _errorOccurred = true; } }
			}
			Array.Resize(ref _ptsOnWinCoords, c);
			
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_plane, _minXDblF, _maxXDblF, _resX, _tensionOnWin, _formula}; }

	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Trace une fonction a deux variables dans l'espace. Formule du type [z=]x+y.
	/// </summary>
	public class SpFunction2 : SpFunctionObject
	{
	
		protected int _xLines, _yLines;
		protected bool _errorOccurred;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Function2"; } }
		
		/// <summary>
		/// Obtient s'il y a des structures Empty dans PointsOnWinCoords après le calcul du tableau.
		/// </summary>
		internal bool ErrorOccurred { get { return _errorOccurred; } }
		
		/// <summary>
		/// Obtient le nombre de lignes X calculées.
		/// </summary>
		internal int XLines { get { return _xLines; } }
		
		/// <summary>
		/// Obtient le nombre de lignes Y calculées.
		/// </summary>
		internal int YLines { get { return _yLines; } }
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpFunction2(string name, DoubleF minX, DoubleF maxX, DoubleF minY, DoubleF maxY, double resX, double resY,
			float tension, string formula) : base(name)
		{
			_unknowNb = 2; base.BackColor = Color.FromArgb(100, BackColor); base.Color = Color.FromArgb(100, Color);
			Alter(minX, maxX, minY, maxY, resX, resY, tension, formula);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(DoubleF minX, DoubleF maxX, DoubleF minY, DoubleF maxY, double resX, double resY,
			float tension, string formula)
		{
			_minXDblF = minX; _maxXDblF = maxX; _minYDblF = minY; _maxYDblF = maxY;
			_resX = resX; _resY = resY; _tensionOnWin = tension;
			if (!AlterFormula(formula)) { SendCalculationResult(true, null); return; }
			EndAlterProcess(GetObjectsFromFormula(_minXDblF), GetObjectsFromFormula(_maxXDblF),
				GetObjectsFromFormula(_minYDblF), GetObjectsFromFormula(_maxYDblF),
				GetObjectsFromFormula(_formula));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			base.CalculateNumericData();
		}

		/// <summary>
		/// Calcul une série de points sur le tracé de la fonction, si l'objet n'est pas virtuel, système ou caché.
		/// </summary>
		protected override void CalculatePointsOnWinCoords()
		{
		
			// Sort si on ne doit pas calculer:
			if (IsUndefined || IsVirtual || IsSystem || Hidden) { _ptsOnWinCoords = new Coord3D[0]; return; }
			
			// Calcule les points pour l'affichage de la fonction:
			_errorOccurred = false;
			_ptsOnWinCoords = new Coord3D[100]; int c = 0; double z;
			double maxX = _maxX + _resX, maxY = _maxY + _resY;
			_xLines = _yLines = 0;
			
			// Pour toutes les valeurs de x:
			for (double x=_minX; x<maxX; x+=_resX)
			{
				// Si on arrive à la fin, met la dernière valeur sur la ligne (sinon, il y a un blanc),
				// tout en veillant à ce que les deux dernières valeurs ne soit pas trop proche (sinon,
				// le dessin DrawCurve dépasse de la ligne):
				if (x > _maxX) { x = _maxX; }
				_xLines++;
				// Détermine une valeur exacte, car il est important ici d'avoir des valeurs exactes
				// (gestion des erreurs, etc.)
				x = (double)(decimal)x;
				// Pour toutes les valeurs de y:
				for (double y=_minY; y<maxY; y+=_resY)
				{
					if (y > _maxY) { y = _maxY; }
					if (x == _minX) { _yLines++; }
					y = (double)(decimal)y;
					// Tente un calcul:
					if (c >= _ptsOnWinCoords.Length) { Array.Resize(ref _ptsOnWinCoords, c + 100); }
					try
					{
						z = _method2(x, y); // Calcule z
						// Si erreur, indique qu'il y a eu des erreurs, sinon on calcul les coordonées du nouveau point:
						if (!DoubleF.IsThereNan(z)) { _ptsOnWinCoords[c++] = new Coord3D(x, y, z); }
						else { _errorOccurred = true; }
					}
					// Si erreur, indique qu'il y a eu des erreurs:
					catch { if (c > 0) { _errorOccurred = true; } }
				}
				
			}
			Array.Resize(ref _ptsOnWinCoords, c);
			
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_minXDblF, _maxXDblF, _minYDblF, _maxYDblF, _resX, _resY, _tensionOnWin, _formula}; }
		
	}




	// ---------------------------------------------------------------------------
	
	
	/// <summary>
	/// Trace une fonction du plan. Il y a deux variable. L'équation est du type x+y=0.
	/// </summary>
	public class SpFunction2OnPlane : SpFunctionObject
	{
	
		protected SpPlaneObject _plane;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Function2 on plane"; } }
		
		/// <summary>
		/// Obtient le plan utilisé.
		/// </summary>
		public SpPlaneObject Plane { get { return _plane; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpFunction2OnPlane(string name, SpPlaneObject plane, DoubleF minX, DoubleF maxX, DoubleF minY, DoubleF maxY, double resX, double resY,
			float tension, string formula) : base(name)
		{
			_unknowNb = 2; base.BackColor = Color.FromArgb(0, BackColor); base.Color = Color.FromArgb(255, Color);
			Alter(plane, minX, maxX, minY, maxY, resX, resY, tension, formula);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPlaneObject plane, DoubleF minX, DoubleF maxX, DoubleF minY, DoubleF maxY, double resX, double resY,
			float tension, string formula)
		{
			_minXDblF = minX; _maxXDblF = maxX; _minYDblF = minY; _maxYDblF = maxY;
			_resX = resX; _resY = resY; _plane = plane; _tensionOnWin = tension;
			if (!AlterFormula(formula)) { SendCalculationResult(true, null); return; }
			EndAlterProcess(_plane, GetObjectsFromFormula(_minXDblF), GetObjectsFromFormula(_maxXDblF),
				GetObjectsFromFormula(_minYDblF), GetObjectsFromFormula(_maxYDblF),
				GetObjectsFromFormula(_formula));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			base.CalculateNumericData();
		}

		/// <summary>
		/// Calcule une série de points sur le tracé de la fonction, si l'objet n'est pas virtuel, système ou caché. Créé le tableau PointsOnWinCoords, qui contient les points calculés.
		/// </summary>
		protected override void CalculatePointsOnWinCoords()
		{
		
			// Sort si on ne doit pas calculer:
			if (IsUndefined || IsVirtual || IsSystem || Hidden) { _ptsOnWinCoords = new Coord3D[0]; return; }
			
			// Calcule les points pour l'affichage de la fonction:
			_ptsOnWinCoords = new Coord3D[100]; int c = 0; double f, lastF = 0;
			double maxX = _maxX + _resX, maxY = _maxY + _resY;
			
			// Pour toutes les valeurs de x:
			for (double x=_minX; x<maxX; x+=_resX)
			{
				// Si on arrive à la fin, met la dernière valeur sur la ligne (sinon, il y a un blanc),
				// tout en veillant à ce que les deux dernières valeurs ne soit pas trop proche (sinon,
				// le dessin DrawCurve dépasse de la ligne):
				if (x > _maxX) { x = _maxX; }
				// Pour toutes les valeurs de y:
				for (double y=_minY; y<maxY; y+=_resY)
				{
					if (y > _maxY) { y = _maxY; }
					// Tente un calcul:
					try
					{
						f = _method2(x, y); // Calcule f
						// Si erreur, on ne fait rien, sinon on calcul les coordonées du nouveau point:
						if (DoubleF.IsThereNan(f)) { continue; }
						// Si première fois, enregistre la valeur et continue:
						else if (y == _minY && f != 0) { lastF = f; continue; }
						// Sinon, compare f à lastF, et si pas de même signe, c'est qu'il y a une valeur en 0 au milieu:
						else if (f == 0 || f * lastF < 0) {
							if (c >= _ptsOnWinCoords.Length) { Array.Resize(ref _ptsOnWinCoords, c + 100); }
							_ptsOnWinCoords[c++] = _plane.To3D(x, (f==0 ? y : ((y + y - _resY) / 2))); }
						lastF = f;
					}
					// Si erreur, on passe:
					catch { ; }
				}
				
			}
			Array.Resize(ref _ptsOnWinCoords, c);
			
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_plane, _minXDblF, _maxXDblF, _minYDblF, _maxYDblF, _resX, _resY, _tensionOnWin, _formula}; }

	}


	// ---------------------------------------------------------------------------
	
	/// <summary>
	/// Trace une fonction dans l'espace. Il y a trois variables. L'équation est du type x+y+z=0.
	/// </summary>
	public class SpFunction3 : SpFunctionObject
	{
	
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Function3"; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpFunction3(string name, DoubleF minX, DoubleF maxX, DoubleF minY, DoubleF maxY, DoubleF minZ, DoubleF maxZ,
			double resX, double resY, double resZ, float tension, string formula) : base(name)
		{
			_unknowNb = 3; base.BackColor = Color.FromArgb(255, BackColor); base.Color = Color.FromArgb(0, Color);
			Alter( minX, maxX, minY, maxY, minZ, maxZ, resX, resY, resZ, tension, formula);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(DoubleF minX, DoubleF maxX, DoubleF minY, DoubleF maxY, DoubleF minZ, DoubleF maxZ,
			double resX, double resY, double resZ, float tension, string formula)
		{
			_minXDblF = minX; _maxXDblF = maxX; _minYDblF = minY; _maxYDblF = maxY; _minZDblF = minZ; _maxZDblF = maxZ;
			_resX = resX; _resY = resY; _resZ = resZ; _tensionOnWin = tension;
			if (!AlterFormula(formula)) { SendCalculationResult(true, null); return; }
			EndAlterProcess(GetObjectsFromFormula(_minXDblF), GetObjectsFromFormula(_maxXDblF),
				GetObjectsFromFormula(_minYDblF), GetObjectsFromFormula(_maxYDblF),
				GetObjectsFromFormula(_minZDblF), GetObjectsFromFormula(_maxZDblF),
				GetObjectsFromFormula(_formula));
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			base.CalculateNumericData();
		}

		/// <summary>
		/// Calcule une série de points sur le tracé de la fonction, si l'objet n'est pas virtuel, système ou caché. Retourne false si une erreur InvalidProgramException a été levé par l'exécution de la méthode dynamique, true sinon. Créé le tableau PointsOnWinCoords, qui contient les points calculés.
		/// </summary>
		protected override void CalculatePointsOnWinCoords()
		{
		
			// Sort si on ne doit pas calculer:
			if (IsUndefined || IsVirtual || IsSystem || Hidden) { _ptsOnWinCoords = new Coord3D[0]; return; }
			
			// Calcule les points pour l'affichage de la fonction:
			_ptsOnWinCoords = new Coord3D[100]; int c = 0; double f, lastF = 0;
			double maxX = _maxX + _resX, maxY = _maxY + _resY, maxZ = _maxZ + _resZ;
			
			// Pour toutes les valeurs de x:
			for (double x=_minX; x<maxX; x+=_resX)
			{
				// Si on arrive à la fin, met la dernière valeur sur la ligne (sinon, il y a un blanc),
				// tout en veillant à ce que les deux dernières valeurs ne soit pas trop proche (sinon,
				// le dessin DrawCurve dépasse de la ligne):
				if (x > _maxX) { x = _maxX; }
				// Pour toutes les valeurs de y:
				for (double y=_minY; y<maxY; y+=_resY)
				{
					if (y > _maxY) { y = _maxY; }
					// Pour toute les valeurs de z:
					for (double z=_minZ; z<maxZ; z+=_resZ)
					{
						// Tente un calcul:
						try
						{
							f = _method3(x, y, z); // Calcule f
							// Si erreur, on ne fait rien, sinon on calcul les coordonées du nouveau point:
							if (DoubleF.IsThereNan(f)) { continue; }
							// Si première fois, enregistre la valeur et continue:
							else if (z == _minZ && f != 0) { lastF = f; continue; }
							// Sinon, compare f à lastF, et si pas de même signe, c'est qu'il y a une valeur en 0 au milieu:
							else if (f == 0 || f * lastF < 0) {
								if (c >= _ptsOnWinCoords.Length) { Array.Resize(ref _ptsOnWinCoords, c + 100); }
								_ptsOnWinCoords[c++] = new Coord3D(x, (f==0 ? y : ((y + y - _resY) / 2)),
									(f==0 ? z : ((z + z - _resZ) / 2))); }
							lastF = f;
						}
						// Si erreur, on passe:
						catch { ; }
					}
				}
				
			}
			Array.Resize(ref _ptsOnWinCoords, c);
			
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_minXDblF, _maxXDblF, _minYDblF, _maxYDblF, _minZDblF, _maxZDblF, _resX, _resY, _resZ, _tensionOnWin, _formula}; }

	}
	
	
	
	#endregion FONCTIONS DU PLAN


	
}
