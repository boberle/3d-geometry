using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Forms;

namespace My
{


	/// <summary>
	/// Type courant (de travail) pour les formules.
	/// </summary>
	public enum FormulaWorkingType
	{
		Double, Decimal
	}


	/// <summary>
	/// Classe d'exception.
	/// </summary>
	public class FormulaException : Exception
	{
		public FormulaException(string message) : base(message)	{ ; }
		public FormulaException(string message, Exception innerException) : base(message, innerException)	{ ; }
	}

	
	
	// ===========================================================================
	



	/// <summary>
	/// Fournit des méthodes par défaut pour les formules.
	/// </summary>
	public static class MethodsForFormulas
	{
	
		private static MethodInfo[] _methods;

		/// <summary>
		/// Obtient les méthodes de cette classe.
		/// </summary>
		public static MethodInfo[] Methods { get { return _methods; } }
		
		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static MethodsForFormulas()
		{
			_methods = new MethodInfo[25]; int c = 0;
			_methods[c++] = typeof(Math).GetMethod("Sqrt");
			_methods[c++] = typeof(Math).GetMethod("Abs", new Type[]{typeof(Double)});
			_methods[c++] = typeof(Math).GetMethod("Abs", new Type[]{typeof(Decimal)});
			_methods[c++] = typeof(Math).GetMethod("Truncate", new Type[]{typeof(Double)});
			_methods[c++] = typeof(Math).GetMethod("Truncate", new Type[]{typeof(Decimal)});
			_methods[c++] = typeof(Math).GetMethod("Cos");
			_methods[c++] = typeof(Math).GetMethod("Tan");
			_methods[c++] = typeof(Math).GetMethod("Sin");
			_methods[c++] = typeof(Math).GetMethod("Acos");
			_methods[c++] = typeof(Math).GetMethod("Atan");
			_methods[c++] = typeof(Math).GetMethod("Asin");
			Array.Resize(ref _methods, c);
			_methods = _methods.Concat(typeof(MethodsForFormulas).GetMethods(BindingFlags.Static | BindingFlags.Public)).ToArray();
			_methods = _methods.Where(delegate(MethodInfo mi) { return !mi.IsSpecialName; }).ToArray();
		}


		// ---------------------------------------------------------------------------
	

		// CHAINES:
		
		[FormulaFunctionCategories("Internal maths functions","Get object")]
		public static string ToStr(object o)
			{ return o.ToString(); }

		// ACQUISITION D'OBJETS:
		
		[FormulaFunctionCategories("Internal maths functions","Get object")]
		public static DoubleF GetDF(double nb)
			{ return new DoubleF(nb); }
		
		[FormulaFunctionCategories("Internal maths functions","Get object")]
		public static double GetDFVal(DoubleF nb)
			{ return nb.Value; }
		
		[FormulaFunctionCategories("Internal maths functions","Get object")]
		public static DoubleF GetDF(string s)
			{ return new DoubleF(s); }
		
		[FormulaFunctionCategories("Internal maths functions","Get object")]
		public static DecimalF GetDecF(double nb)
			{ return new DecimalF(nb); }
		
		[FormulaFunctionCategories("Internal maths functions","Get object")]
		public static DecimalF GetDecF(string s)
			{ return new DecimalF(s); }
		
		[FormulaFunctionCategories("Internal maths functions","Get object")]
		public static bool GetBool(int value)
			{ return (value != 0); }
		
		[FormulaFunctionCategories("Internal maths functions","Get object")]
		public static Coord3D Coords(double x, double y, double z)
			{ return new Coord3D(x, y, z); }
		
		[FormulaFunctionCategories("Internal maths functions","Get object")]
		public static Coord2D Coords(double x, double y)
			{ return new Coord2D(x, y); }
		
		[FormulaFunctionCategories("Internal maths functions","Get property")]
		public static double CoordsX(Coord3D coords)
			{ return coords.X; }
		
		[FormulaFunctionCategories("Internal maths functions","Get property")]
		public static double CoordsY(Coord3D coords)
			{ return coords.Y; }
		
		[FormulaFunctionCategories("Internal maths functions","Get property")]
		public static double CoordsZ(Coord3D coords)
			{ return coords.Z; }
		
		[FormulaFunctionCategories("Internal maths functions","Get property")]
		public static double CoordsX(Coord2D coords)
			{ return coords.X; }
		
		[FormulaFunctionCategories("Internal maths functions","Get property")]
		public static double CoordsY(Coord2D coords)
			{ return coords.Y; }
			
		// METHODES DE CONDITION:
		
		[FormulaFunctionCategories("Internal maths functions","Statements")]
		public static double If(bool c, double valTrue, double valFalse)
			{ return (c ? valTrue : valFalse); }
		
		[FormulaFunctionCategories("Internal maths functions","Statements")]
		public static decimal If(bool c, decimal valTrue, decimal valFalse)
			{ return (c ? valTrue : valFalse); }
		
		[FormulaFunctionCategories("Internal maths functions","Statements")]
		public static object If(bool c, object valTrue, object valFalse)
			{ return (c ? valTrue : valFalse); }
		
		[FormulaFunctionCategories("Internal maths functions","Statements")]
		public static bool And(params bool[] tests)
			{ foreach (bool b in tests) { if (!b) { return false; } } return true; }
		
		[FormulaFunctionCategories("Internal maths functions","Statements")]
		public static bool Or(params bool[] tests)
			{ bool res = false; foreach (bool b in tests) { res = res || b; } return res; }
		
		[FormulaFunctionCategories("Internal maths functions","Statements")]
		public static double Approx(double x)
			{ return Maths.Approx(x); }
		
		[FormulaFunctionCategories("Internal maths functions","Statements")]
		public static decimal Approx(decimal x)
			{ return Maths.Approx(x); }
		
		// GEOMETRIE:
		
		[FormulaFunctionCategories("Internal maths functions","Geometry")]
		public static double ToDeg(double rad)
			{ return MathsGeo.RadToDeg(rad); }

		[FormulaFunctionCategories("Internal maths functions","Geometry")]
		public static double ToRad(double deg)
			{ return MathsGeo.DegToRad(deg); }
		
		[FormulaFunctionCategories("Internal maths functions","Geometry")]
		public static double Pi()
			{ return Math.PI; }

		// ALGEBRE:
		
		/// <summary>
		/// Résout un trinôme. Si retSupSol vaut true, retourne la solution la plus grande s'il y a deux solutions. Lorsqu'il n'y a qu'une solution, ne la retourne que si retOnlySol vaut true. Sinon, retourne NaN.
		/// </summary>
		[FormulaFunctionCategories("Internal maths functions","Algebra")]
		public static double SolveTrinomial(double a, double b, double c, bool retSupSol, bool retOnlySol)
		{
			double x_1, x_2;
			int solNb = MathsAlg.SolveTrinomial(a, b, c, out x_1, out x_2);
			if (solNb == 0) { return Double.NaN; }
			else if (solNb == 1 && retOnlySol) { return x_1; }
			else if (solNb == 1 && !retOnlySol) { return Double.NaN; }
			else if (solNb == 2 && retSupSol) { return x_2; }
			else { return x_1; }
		}
		
		// ARITHMETIQUE

		[FormulaFunctionCategories("Internal maths functions","Arithmetic")]
		public static double Int(double a)
			{ return Microsoft.VisualBasic.Conversion.Int(a); }
		
		[FormulaFunctionCategories("Internal maths functions","Arithmetic")]
		public static decimal Int(decimal a)
			{ return Microsoft.VisualBasic.Conversion.Int(a); }
		
				
	}


	
	
	// ===========================================================================
	



	/// <summary>
	/// Créer une méthode dynamique à partir d'une formule textuelle.
	/// </summary>
	public static class Formula
	{






		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS



		private enum OpType
		{
			None, Add, Mul, Div, Sub, Pow, Eq, Inf, Sup, InfEq, SupEq, NonEq, Approx
		}

		private enum FormPartType
		{
			None, Method, MethParam, Bracket, Number, Variable, Text, Empty
		}

		private enum CodePartType
		{
			None, Variable, Number, Text, FuncCall, Operator, ParamConversion, ArrCreate, ArrSetIndex, ArrSaveElement
		}

		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Partie d'une formule (parenthèse, fonction, paramètre, nombre, variable, texte, etc.) lors du découpage de la fonction. Les parties sont imbriquées les unes dans les autres via les propriétés Child et Parent.
		/// </summary>
		private class FormPart
		{
		
			// Déclarations:
			private OpType _prevOp;
			private int _opPriority;
			private FormPart[] _children;
		
			/// <summary>
			/// Type de partie.
			/// </summary>
			public FormPartType Type { get; set; }
			
			/// <summary>
			/// Texte de la partie : Nombre, nom de la variable, text, nom de la fonction, etc.
			/// </summary>
			public string Text { get; set; }
			
			/// <summary>
			/// Méthode appelée pour les parties "Function".
			/// </summary>
			public MethodInfo Method { get; set; }
			
			/// <summary>
			/// Type du paramètre pour les parties "Parameter".
			/// </summary>
			public Type ParameterType { get; set; }
			
			/// <summary>
			/// Index du paramètre pour les parties "Parameter".
			/// </summary>
			public int ParameterNb { get; set; }
			
			/// <summary>
			/// Index général du tableau params, pour les paramètres de type "params". C'est l'index général du tableau parmis tous les autres tableau params dans la formule.
			/// </summary>
			public int ParamsArrayIndex { get; set; }
			
			/// <summary>
			/// Index de l'élément (du paramètre) dans un tableau "params".
			/// </summary>
			public int ParamsElementIndex { get; set; } // Index du paramètre dans le tableau params
			
			/// <summary>
			/// Opérateur précédent la partie pour les parties "Number", "Texte", "Function", "Bracket".
			/// </summary>
			public OpType PreviousOperator { get { return _prevOp; } }
			
			/// <summary>
			/// Prioriété de l'opérateur.
			/// </summary>
			public int OperatorPriority { get { return _opPriority; } } 
			
			/// <summary>
			/// Partie parente.
			/// </summary>
			public FormPart Parent { get; set; }
			
			/// <summary>
			/// Liste des sous-parties.
			/// </summary>
			public FormPart[] Children { get { return _children; } }
			
			/// <summary>
			/// Constructeur privé. Initialisation des variables.
			/// </summary>
			private FormPart()
			{
				Type = FormPartType.None; _children = new FormPart[0]; ParameterNb = -1;
				ParamsArrayIndex = -1; ParamsElementIndex = -1;
			}
			
			/// <summary>
			/// Constructeur.
			/// </summary>
			public FormPart(string prevOp, FormPart parent) : this()
			{
				Type = FormPartType.None; SetPreviousOperator(prevOp); Parent = parent;
			}

			/// <summary>
			/// Définit la priorité de l'opérateur en remplissant automatiquement la propriété OperatorPriority.
			/// </summary>
			private void SetPreviousOperator(string s)
			{
				if (s == null) { s = String.Empty; }
				switch (s)
				{
					case "+": _prevOp = OpType.Add; break;
					case "-": _prevOp = OpType.Sub; break;
					case "*": _prevOp = OpType.Mul; break;
					case "/": _prevOp = OpType.Div; break;
					case "^": _prevOp = OpType.Pow; break;
					case "=": _prevOp = OpType.Eq; break;
					case "<": _prevOp = OpType.Inf; break;
					case ">": _prevOp = OpType.Sup; break;
					case "≤": _prevOp = OpType.InfEq; break;
					case "≥": _prevOp = OpType.SupEq; break;
					case "≠": _prevOp = OpType.NonEq; break;
					case "≈": _prevOp = OpType.Approx; break;
					default: _prevOp = OpType.None; break;
				}
				_opPriority = _priorities[_prevOp]; 
			}
			
			/// <summary>
			/// Ajoute une sous-partie.
			/// </summary>
			public void AddChild(FormPart child)
			{
				int c = _children.Length;
				Array.Resize(ref _children, c + 1);
				this.Children[c] = child;
			}
			
			/// <summary>
			/// Ajoute une partie soeur, cad une sous-partie à la partie inscrite dans la propriété Parent.
			/// </summary>
			/// <param name="sibling"></param>
			public void AddSibling(FormPart sibling)
			{
				int c = this.Parent._children.Length;
				Array.Resize(ref this.Parent._children, c + 1);
				this.Parent.Children[c] = sibling;
			}

		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Partie de code MSIL. Chaque partie décrit les caractéristiques du code MSIL à ajouter.
		/// </summary>
		private struct CodePart
		{
		
			/// <summary>
			/// Texte à inscrit, qui peut être un nombre, une variable ou une chaîne.
			/// </summary>
			public string Text { get; set; }
			
			/// <summary>
			/// Type de la partie.
			/// </summary>
			public CodePartType Type { get; set; }
			
			/// <summary>
			/// Type de l'opérateur à inscrire pour les parties "Operator".
			/// </summary>
			public OpType OpType { get; set; }
			
			/// <summary>
			/// Méthode à appeler pour les parties "FunctionCall".
			/// </summary>
			public MethodInfo MethodToCall { get; set; }
			
			/// <summary>
			/// Type vers lequel convertir pour les parties "ParamConversion".
			/// </summary>
			public Type ConvertTo { get; set; }
			
			/// <summary>
			/// Longueur du tableau à créer pour les parties relatives aux tableaux.
			/// </summary>
			public int ArrayLength { get; set; }
			
			/// <summary>
			/// Type du tableau à créer pour les parties relatives aux tableaux.
			/// </summary>
			public Type ArrayElementType { get; set; }
			
			/// <summary>
			/// Index général du tableau, parmi l'ensemble des tableaux à créer, pour les parties relatives aux tableaux.
			/// </summary>
			public int ArrayIndex { get; set; }
			
			/// <summary>
			/// Index de l'élément à inscrire dans le tableau pour les parties relatives aux tableaux.
			/// </summary>
			public int ArrayElementIndex { get; set; }
			
			/// <summary>
			/// Constructeur pour un OPERATOR.
			/// </summary>
			public CodePart(OpType opType) : this()
				{ Type = CodePartType.Operator; OpType = opType; }
			
			/// <summary>
			/// Constructeur pour un FUNCCALL.
			/// </summary>
			public CodePart(MethodInfo methToCall) : this()
				{ Type = CodePartType.FuncCall; MethodToCall = methToCall; }
			
			/// <summary>
			/// Constructeur pour un NUMBER, ou un VARIABLE, ou un TEXT.
			/// </summary>
			public CodePart(string text, CodePartType type) : this()
				{ Type = type; Text = text; }
			
			/// <summary>
			/// Constructeur pour un PARAMCONVERSION.
			/// </summary>
			public CodePart(Type converTo) : this() // Conversion
				{ Type = CodePartType.ParamConversion; ConvertTo = converTo; }
			
			/// <summary>
			/// Constructeur pour un ARRCREATE.
			/// </summary>
			public CodePart(int arrIndex, int arrLength, Type elementType) : this() // Nouvel Array
				{ Type = CodePartType.ArrCreate; ArrayIndex = arrIndex; ArrayLength = arrLength;
					ArrayElementType = elementType; }
			
			/// <summary>
			/// Constructeur pour un ARRSETINDEX.
			/// </summary>
			public CodePart(int arrIndex, int elementIndex) : this() // Set index
				{ Type = CodePartType.ArrSetIndex; ArrayIndex = arrIndex; ArrayElementIndex = elementIndex; }
			
			/// <summary>
			/// Constructeur pour un ARRSAVEELEMENT.
			/// </summary>
			public CodePart(int arrIndex, Type elementType) : this() // Save element in array
				{ Type = CodePartType.ArrSaveElement; ArrayIndex = arrIndex; ArrayElementType = elementType; }
			
			#if DEBUG
			/// <summary>
			/// Retourne la description de l'objet (mode debug uniquement).
			/// </summary>
			public override string ToString()
			{
				string s = Type.ToString() + ": " ;
				switch (Type)
				{
					case CodePartType.Operator: s += OpType.ToString(); break;
					case CodePartType.ParamConversion: s += ConvertTo.ToString(); break;
					case CodePartType.FuncCall: s += MethodToCall.Name; break;
					case CodePartType.ArrCreate: s += "Len: " + ArrayLength.ToString() + " - ArrIndex: " + ArrayIndex.ToString(); break;
					case CodePartType.ArrSetIndex: s += "ElIndex: " + ArrayElementIndex.ToString() + " - ArrIndex: " + ArrayIndex.ToString(); break;
					case CodePartType.ArrSaveElement: s += "Type: " + ArrayElementType.ToString() + " - ArrIndex: " + ArrayIndex.ToString(); break;
					default : s += Text; break;
				}
				return s;
			}
			#endif
			
		}


		// ---------------------------------------------------------------------------


		// Tableau des priorités et des types numériques:
		private static Dictionary<OpType,int> _priorities;
		// Tableau des méthodes mathématiques par défaut:
		private static MethodInfo[] _defaultMeth;
		private static MethodInfo[] _generalUsedFunctions;
		// Méthodes:
		private static MethodInfo _methPowDouble;
		private static MethodInfo _methPowDecimal;
		private static MethodInfo _methAddDecimal;
		private static MethodInfo _methSubtractDecimal;
		private static MethodInfo _methDivideDecimal;
		private static MethodInfo _methMultiplyDecimal;
		private static MethodInfo _methDoubleParse;
		private static MethodInfo _methDecimalParse;
		private static MethodInfo _methChangeType;
		private static MethodInfo _methGetTypeFromHandle;
		private static MethodInfo _methEqDouble;
		private static MethodInfo _methInfDouble;
		private static MethodInfo _methSupDouble;
		private static MethodInfo _methInfEqDouble;
		private static MethodInfo _methSupEqDouble;
		private static MethodInfo _methNonEqDouble;
		private static MethodInfo _methApproxDouble;
		private static MethodInfo _methEqDecimal;
		private static MethodInfo _methInfDecimal;
		private static MethodInfo _methSupDecimal;
		private static MethodInfo _methInfEqDecimal;
		private static MethodInfo _methSupEqDecimal;
		private static MethodInfo _methNonEqDecimal;
		private static MethodInfo _methApproxDecimal;



		#endregion DECLARATIONS








		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES
		
		
		
		/// <summary>
		/// Obtient ou définit les fonctions utilisées que l'utilisateur peut utiliser dans toutes les formules, en plus des fonctions par défaut de cette clase et celles que peuvent passer les méthodes à CreateFormulaMethod (ces dernières sont donc spécifiques à l'appel de CreateFormulaMethod, et ne durent que le temps de l'appel).
		/// </summary>
		public static MethodInfo[] GeneralUsedFunctions {
			get { return _generalUsedFunctions; }
			set { if (value == null) { _generalUsedFunctions = new MethodInfo[0]; } else { _generalUsedFunctions = value; } } }
		
		
		/// <summary>
		/// Obtient les méthodes générales (de GeneralUsedFunctions) et les méthodes mathématiques par défaut.
		/// </summary>
		public static MethodInfo[] GeneralAndDefMethods {
			get { return _defaultMeth.Concat(_generalUsedFunctions).ToArray(); } }
		
		/// <summary>
		/// Obtient les méthodes mathématique par défaut.
		/// </summary>
		public static MethodInfo[] DefaultMethods {
			get { return _defaultMeth; } }
		



		#endregion PROPRIETES
		
		
		
		
		
		
		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS



		/// <summary>
		/// Constructeur d'initialisation des champs statiques.
		/// </summary>
		static Formula()
		{
		
			// Initialisation:
			_generalUsedFunctions = new MethodInfo[0];

			// Priorités:
			_priorities = new Dictionary<OpType,int>(13);
			_priorities.Add(OpType.Eq, 1);
			_priorities.Add(OpType.Inf, 1);
			_priorities.Add(OpType.Sup, 1);
			_priorities.Add(OpType.InfEq, 1);
			_priorities.Add(OpType.SupEq, 1);
			_priorities.Add(OpType.NonEq, 1);
			_priorities.Add(OpType.Approx, 1);
			_priorities.Add(OpType.Add, 2);
			_priorities.Add(OpType.Sub, 2);
			_priorities.Add(OpType.Mul, 3);
			_priorities.Add(OpType.Div, 3);
			_priorities.Add(OpType.Pow, 4);
			_priorities.Add(OpType.None, 0);
			
			// Création des méthodes:
			// Méthode pour la conversion des types:
			_methChangeType = typeof(Convert).GetMethod("ChangeType", new Type[]{typeof(Object),typeof(Type)});
			_methGetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[]{typeof(RuntimeTypeHandle)});
			// Méthode de calcul:
			_methPowDouble = typeof(Math).GetMethod("Pow", new Type[]{typeof(Double),typeof(Double)});
			_methPowDecimal = typeof(Formula).GetMethod("PowDecimal", new Type[]{typeof(Decimal),typeof(Decimal)});
			_methAddDecimal = typeof(Decimal).GetMethod("Add", new Type[]{typeof(Decimal),typeof(Decimal)});
			_methSubtractDecimal = typeof(Decimal).GetMethod("Subtract", new Type[]{typeof(Decimal),typeof(Decimal)});
			_methDivideDecimal = typeof(Decimal).GetMethod("Divide", new Type[]{typeof(Decimal),typeof(Decimal)});
			_methMultiplyDecimal = typeof(Decimal).GetMethod("Multiply", new Type[]{typeof(Decimal),typeof(Decimal)});
			// Méthodes de comparaison:
			_methEqDouble = typeof(Formula).GetMethod("EqDouble", new Type[]{typeof(double), typeof(double)});
			_methInfDouble = typeof(Formula).GetMethod("InfDouble", new Type[]{typeof(double), typeof(double)});
			_methSupDouble = typeof(Formula).GetMethod("SupDouble", new Type[]{typeof(double), typeof(double)});
			_methInfEqDouble = typeof(Formula).GetMethod("InfEqDouble", new Type[]{typeof(double), typeof(double)});
			_methSupEqDouble = typeof(Formula).GetMethod("SupEqDouble", new Type[]{typeof(double), typeof(double)});
			_methNonEqDouble = typeof(Formula).GetMethod("NonEqDouble", new Type[]{typeof(double), typeof(double)});
			_methApproxDouble = typeof(Formula).GetMethod("ApproxDouble", new Type[]{typeof(double), typeof(double)});
			_methEqDecimal = typeof(Formula).GetMethod("EqDecimal", new Type[]{typeof(decimal), typeof(decimal)});
			_methInfDecimal = typeof(Formula).GetMethod("InfDecimal", new Type[]{typeof(decimal), typeof(decimal)});
			_methSupDecimal = typeof(Formula).GetMethod("SupDecimal", new Type[]{typeof(decimal), typeof(decimal)});
			_methInfEqDecimal = typeof(Formula).GetMethod("InfEqDecimal", new Type[]{typeof(decimal), typeof(decimal)});
			_methSupEqDecimal = typeof(Formula).GetMethod("SupEqDecimal", new Type[]{typeof(decimal), typeof(decimal)});
			_methNonEqDecimal = typeof(Formula).GetMethod("NonEqDecimal", new Type[]{typeof(decimal), typeof(decimal)});
			_methApproxDecimal = typeof(Formula).GetMethod("ApproxDecimal", new Type[]{typeof(decimal), typeof(decimal)});
			// Autres méthodes:
			_methDoubleParse = typeof(Double).GetMethod("Parse", new Type[]{typeof(String)});
			_methDecimalParse = typeof(Decimal).GetMethod("Parse", new Type[]{typeof(String)});
			
			// Méthodes mathématiques par défaut:
			_defaultMeth = MethodsForFormulas.Methods;
			
		}
		
		// Méthodes systèmes:
		public static decimal PowDecimal(decimal x, decimal y)
			{ return (decimal)Math.Pow((double)x, (double)y); }

		public static bool EqDouble(double x, double y)
			{ return (x==y); }

		public static bool InfDouble(double x, double y)
			{ return (x<y); }

		public static bool SupDouble(double x, double y)
			{ return (x>y); }

		public static bool InfEqDouble(double x, double y)
			{ return (x<=y); }

		public static bool SupEqDouble(double x, double y)
			{ return (x>=y); }

		public static bool NonEqDouble(double x, double y)
			{ return (x!=y); }

		public static bool ApproxDouble(double x, double y)
			{ return Maths.Approx(x, y); }
		
		public static bool EqDecimal(decimal x, decimal y)
			{ return (x==y); }

		public static bool InfDecimal(decimal x, decimal y)
			{ return (x<y); }

		public static bool SupDecimal(decimal x, decimal y)
			{ return (x>y); }

		public static bool InfEqDecimal(decimal x, decimal y)
			{ return (x<=y); }

		public static bool SupEqDecimal(decimal x, decimal y)
			{ return (x>=y); }

		public static bool NonEqDecimal(decimal x, decimal y)
			{ return (x!=y); }
			
		public static bool ApproxDecimal(decimal x, decimal y)
			{ return Maths.Approx(x, y); }
		



		#endregion CONSTRUCTEURS






		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES
		
		
		
		/// <summary>
		/// Créer une fonction dynamique renvoyée sous forme de délégué à partir d'une formule mathématique passée dans formula. name est le nom de la fonction. variables est un tableau de chaînes qui contient les noms des variables utilisées dans la formule (e.g. "x+2" : x est la variable), et qui correspondent, dans l'ordre, a paramètres de la fonction, cad au tableau paramTypes (il est évident que les index des noms de variables doivent correspondre aux index des types de paramètres). returnType est le type de retour de la fonction. delegType est le type du délégué retourné par la fonction. owner est le type auquel sera attaché la méthode dynamique, et ce type est important pour des questions de visibilité des méthodes appelées dans la formule. workingType est le type courant utilisé, double ou decimal. Quasiment toutes les fonctions utilisables sont écrites en deux exemplaires, l'un pour double, l'autre pour decimal. L'utilisation de double est plus rapide et évite les exceptions (par exemple pour une division par zéro), alors que decimal est plus lent mais plus précis, et lève des exceptions en cas d'erreur. usedMethods permet d'utiliser des méthodes en plus des méthodes par défaut et des méthodes générales définies dans la propriété correspondante de cette classe statique. variables peut être null, et dans ce cas il n'y a pas de variables dans la formule. delegType ne doit pas être null. Owner non plus. returnType peut être null, et c'est alors workingType qui est utilisé. usedMthods peut être null. paramTypes peut être null, et dans ce cas c'est workingType qui est utilisé pour toutes les variables. Pour la syntaxe, voir le fichier Word.
		/// </summary>
		public static Delegate CreateFormulaMethod(string formula, string[] variables, Type delegType, Type owner, Type returnType,
			FormulaWorkingType workingType, MethodInfo[] usedMethods, Type[] paramTypes)
		{
		
			// Variables:
			Type lastWrittenType = null;
			Type workingTypeT = (workingType==FormulaWorkingType.Double ? typeof(Double) : typeof(Decimal));
			if (returnType == null) { returnType = workingTypeT; }
			if (variables == null) { variables = new string[0]; }
			if (usedMethods == null) { usedMethods = new MethodInfo[0]; }
			if (paramTypes == null) {
				paramTypes = new Type[variables.Length];
				for (int i=0; i<variables.Length; i++)
					{ paramTypes[i] = (workingType==FormulaWorkingType.Double ? typeof(Double) : typeof(Decimal)); } }
			
			// Obtient la formule découpée, et appelle la méthode de construction du code:
			CodePart[] codeParts = null; int index = -1;
			try
			{
				FormPart parts = SplitFormula(formula, variables, usedMethods.Concat(_defaultMeth).Concat(_generalUsedFunctions).ToArray(),
					workingType);
				BuildCode(parts, ref codeParts, ref index);
				// Nettoie le tableau pour supprimer les CodePartType.None (le tampon n'est pas raccourcis lors de l'appel de BuildCode):
				codeParts = codeParts.Where(delegate(CodePart p) { return p.Type != CodePartType.None; }).ToArray();
			}
			catch (FormulaException exc) { throw exc; }
			catch (Exception exc) { throw new FormulaException(exc.Message, exc); }
			
			// Affichage des données pour le débogage:
			//string msg = formula + "\n\n";
			//for (int i=0; i<codeParts.Length; i++) { msg += codeParts[i].ToString() + "\n"; }
			//System.Windows.Forms.MessageBox.Show(msg);
			
			// Créer une fonction dynamique et récupère un IL Generator:
			DynamicMethod dynMethod = new DynamicMethod(String.Empty, returnType, paramTypes, owner);
			dynMethod.InitLocals = true;
			ILGenerator il = dynMethod.GetILGenerator();
			
			// Tableau (C#) des tableaux (IL)...
			LocalBuilder[] arrayIndexes = new LocalBuilder[0];

			// Pour toutes les lignes de codes:
			foreach (CodePart p in codeParts)
			{
			
				switch (p.Type)
				{
				
					// Si nombre, inscrit la valeur en fonction de workingType:
					case CodePartType.Number:
						if (workingType == FormulaWorkingType.Double) {
							il.Emit(OpCodes.Ldc_R8, Double.Parse(p.Text)); }
						else {
							il.Emit(OpCodes.Ldstr, p.Text);
							il.EmitCall(OpCodes.Call, _methDecimalParse, null); }
						lastWrittenType = workingTypeT;
						break;
					
					// Si variable, on charge la variable indexée sur la pile, et on la convertit au besoin:
					case CodePartType.Variable:
						int varIndex = Array.IndexOf(variables, p.Text);
						il.Emit(OpCodes.Ldarg_S, varIndex);
						lastWrittenType = paramTypes[varIndex];
						if (paramTypes[varIndex].IsPrimitive || paramTypes[varIndex] == typeof(Decimal))
							{ ConvertTo(il, paramTypes[varIndex], workingTypeT); lastWrittenType = workingTypeT; }
						break;
					
					// Si texte, on charge simplement la chaîne sur la pile:
					case CodePartType.Text:
						il.Emit(OpCodes.Ldstr, p.Text);
						lastWrittenType = typeof(String);
						break;
					
					// Si opérateur, on additionne, multiplie, soustrait et divise directement si on est en Double,
					// sinon on fait appelle à des fonctions spécifique de la classe Decimal (ou Double pour Pow):
					case CodePartType.Operator:
						lastWrittenType = workingTypeT;
						if (workingType == FormulaWorkingType.Double)
						{
							switch (p.OpType)
							{
								case OpType.Add: il.Emit(OpCodes.Add); break;
								case OpType.Mul: il.Emit(OpCodes.Mul); break;
								case OpType.Sub: il.Emit(OpCodes.Sub); break;
								case OpType.Div: il.Emit(OpCodes.Div); break;
								case OpType.Pow: il.EmitCall(OpCodes.Call, _methPowDouble, null); break;
								case OpType.Eq: il.EmitCall(OpCodes.Call, _methEqDouble, null); lastWrittenType = typeof(bool); break;
								case OpType.Inf: il.EmitCall(OpCodes.Call, _methInfDouble, null); lastWrittenType = typeof(bool); break;
								case OpType.Sup: il.EmitCall(OpCodes.Call, _methSupDouble, null); lastWrittenType = typeof(bool); break;
								case OpType.InfEq: il.EmitCall(OpCodes.Call, _methInfEqDouble, null); lastWrittenType = typeof(bool); break;
								case OpType.SupEq: il.EmitCall(OpCodes.Call, _methSupEqDouble, null); lastWrittenType = typeof(bool); break;
								case OpType.NonEq: il.EmitCall(OpCodes.Call, _methNonEqDouble, null); lastWrittenType = typeof(bool); break;
								case OpType.Approx: il.EmitCall(OpCodes.Call, _methApproxDouble, null); lastWrittenType = typeof(bool); break;
							}
						}
						else
						{
							switch (p.OpType)
							{
								case OpType.Add: il.EmitCall(OpCodes.Call, _methAddDecimal, null); break;
								case OpType.Mul: il.EmitCall(OpCodes.Call, _methMultiplyDecimal, null); break;
								case OpType.Sub: il.EmitCall(OpCodes.Call, _methSubtractDecimal, null); break;
								case OpType.Div: il.EmitCall(OpCodes.Call, _methDivideDecimal, null); break;
								case OpType.Pow: il.EmitCall(OpCodes.Call, _methPowDecimal, null); break;
								case OpType.Eq: il.EmitCall(OpCodes.Call, _methEqDecimal, null); lastWrittenType = typeof(bool); break;
								case OpType.Inf: il.EmitCall(OpCodes.Call, _methInfDecimal, null); lastWrittenType = typeof(bool); break;
								case OpType.Sup: il.EmitCall(OpCodes.Call, _methSupDecimal, null); lastWrittenType = typeof(bool); break;
								case OpType.InfEq: il.EmitCall(OpCodes.Call, _methInfEqDecimal, null); lastWrittenType = typeof(bool); break;
								case OpType.SupEq: il.EmitCall(OpCodes.Call, _methSupEqDecimal, null); lastWrittenType = typeof(bool); break;
								case OpType.NonEq: il.EmitCall(OpCodes.Call, _methNonEqDecimal, null); lastWrittenType = typeof(bool); break;
								case OpType.Approx: il.EmitCall(OpCodes.Call, _methApproxDecimal, null); lastWrittenType = typeof(bool); break;
							}
						}
						break;
					
					// Si conversion de paramètres, on convertit:
					case CodePartType.ParamConversion:
						if (p.ConvertTo == typeof(Object)) { il.Emit(OpCodes.Box, lastWrittenType); }
						else if (lastWrittenType == typeof(bool) && p.ConvertTo == typeof(bool)) { ; }
						else { ConvertTo(il, lastWrittenType, p.ConvertTo); }
						break;
					
					// Si appel de fonction, on appelle la fonction, puis on convertit vers le type de travail si la 
					// fonction retourne un primitif ou un Decimal:
					case CodePartType.FuncCall:
						il.EmitCall(OpCodes.Call, p.MethodToCall, null);
						lastWrittenType = p.MethodToCall.ReturnType;
						if (p.MethodToCall.ReturnType.IsPrimitive || p.MethodToCall.ReturnType == typeof(Decimal))
							{ ConvertTo(il, p.MethodToCall.ReturnType, workingTypeT); lastWrittenType = workingTypeT; }
						break;
					
					// Si création d'un tableau, on inscrit le code MSIL correspondant:
					case CodePartType.ArrCreate:
						if (p.ArrayIndex >= arrayIndexes.Length) { Array.Resize(ref arrayIndexes, p.ArrayIndex+1); }
						if (arrayIndexes[p.ArrayIndex] == null) { arrayIndexes[p.ArrayIndex] = il.DeclareLocal(typeof(Array)); }
						il.Emit(OpCodes.Ldc_I4, p.ArrayLength);
						il.Emit(OpCodes.Newarr, p.ArrayElementType);
						il.Emit(OpCodes.Stloc, arrayIndexes[p.ArrayIndex]);
						il.Emit(OpCodes.Ldloc, arrayIndexes[p.ArrayIndex]);
						break;
					
					// Si ajout d'un index de tableau, on inscrit le code MSIL correspondant:
					case CodePartType.ArrSetIndex:
						il.Emit(OpCodes.Ldc_I4, p.ArrayElementIndex);
						break;
					
					// Si enregistrement d'un élément dans un tableau, on inscrit le code MSIL correspondant:
					case CodePartType.ArrSaveElement:
						il.Emit(OpCodes.Stelem, p.ArrayElementType);
						il.Emit(OpCodes.Ldloc, arrayIndexes[p.ArrayIndex]);
						break;
						
				}
				
			}
			
			// On arrive à la fin du calcul: Vérifie que le type de retour est compatible avec le type de la valeur sur la pile:
			bool match = ((returnType.IsPrimitive || returnType == typeof(Decimal))
				&& (lastWrittenType.IsPrimitive || lastWrittenType == typeof(Decimal)));
			match = match || (returnType == lastWrittenType);
			match = match || lastWrittenType.IsSubclassOf(returnType);
			// Si ça ne correspond pas, erreur:
			if (!match) { throw new FormulaException("Type in formula doesn't match return type of delegate."); }
			// Si le type de retour est un primitif ou Decimal, on convertit vers ce type:
			if (returnType.IsPrimitive || returnType == typeof(Decimal)) { ConvertTo(il, workingTypeT, returnType); }
			// Si le type de retour est object
			if (returnType == typeof(Object)) { il.Emit(OpCodes.Box, workingTypeT); }
			// Retourne la valeur sur la pile:
			il.Emit(OpCodes.Ret);
			
			// Retourne un délégué:
			try { return dynMethod.CreateDelegate(delegType); }
			catch (Exception exc) { throw new FormulaException(exc.Message, exc); }
		
		}

		/// <summary>
		/// Tente de créer la méthode, affiche (ou non) les erreurs, et retourne true si dynMethod est valid, false dans le cas contraire.
		/// </summary>
		public static bool TryCreateFormulaMethod(string formula, string[] variables, Type delegType, Type owner, Type returnType,
			FormulaWorkingType workingType, MethodInfo[] usedMethods, Type[] paramTypes, bool showError, out Delegate dynMethod)
		{
			try { dynMethod = CreateFormulaMethod(formula, variables, delegType, owner, returnType, workingType, usedMethods, paramTypes); return true; }
			catch (Exception exc) { if (showError) { My.ErrorHandler.ShowError(exc); } dynMethod = null; return false; }
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Recherche dans les méthodes utilisables dans les formules une méthode dont le nom contient text. Retourne les syntaxes des différentes surcharges. Si le texte est vide ou null, retourne toutes les syntaxes. Exclut de la recherche les méthodes portant l'attribut ExcludeFromManAttribute si useExcludeAttr vaut true.
		/// </summary>
		public static string[] SearchMethod(string text, bool exactMatch, bool useExcludeAttr, bool multiline)
		{
			MethodInfo[] arr = GeneralAndDefMethods;
			if (text == null) { text = String.Empty; }
			text = text.ToLower();
			string[] result = new string[10]; int c = 0;
			foreach (MethodInfo mi in arr)
			{
				if (useExcludeAttr && mi.GetCustomAttributes(typeof(ExcludeFromManAttribute), true).Length != 0) { continue; }
				if (text == ""
							|| (!exactMatch && mi.Name.ToLower().Contains(text))
							|| (exactMatch && mi.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase))) {
					if (c >= result.Length) { Array.Resize(ref result, c + 10); }
					result[c++] = My.ClassManager.GetMethodSyntax(mi, multiline); }
			}
			Array.Resize(ref result, c);
			return result;
		}
		
		/// <summary>
		/// Même chose que surcharge avec useExcludeAttr à true et exactMatch à false et multiline à false.
		/// </summary>
		public static string[] SearchMethod(string text)
			{ return SearchMethod(text, false, true, false); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Enregistre toutes les méthodes au format manuel dans le fichier spécifié. C'est un tableau de TreeNode sérialisé, ce qui permet de disposer les fonctions en arborescence selon l'attribut des méthodes FormulaFunctionsCategoriesAttribute. what indique quels commandes il faut inclure : négatif pour les commandes internes, 0 pour toutes les commandes, positif pour les commandes externes. Exclut de la recherche les méthodes portant l'attribut ExcludeFromManAttribute. Retourne true si réussi, false si échec.
		/// </summary>
		public static bool MakeMan(int what, string filename)
		{
			// Détermine les méthodes à inclure:
			MethodInfo[] mis;
			if (what < 0) { mis = _defaultMeth; }
			else if (what == 0) { mis = GeneralAndDefMethods; }
			else { mis = _generalUsedFunctions; }
			mis = mis.Where(delegate(MethodInfo mi)
				{ return mi.GetCustomAttributes(typeof(ExcludeFromManAttribute), true).Length == 0; }).ToArray();
			// Créer un arbre:
			TreeView tree = new TreeView();
			// Obtient les méthodes et les catégories pour toutes les méthodes:
			string[] defIntPath = new string[]{"Internal maths functions","General"};
			string[] defExtPath = new string[]{"Other functions"};
			object[] customAttr; TreeNode node;
			string[][] cat = Array.ConvertAll(mis,
				delegate(MethodInfo mi)
				{
					customAttr = mi.GetCustomAttributes(typeof(FormulaFunctionCategoriesAttribute), true);
					if (customAttr.Length == 0 && mi.DeclaringType == typeof(Math)) { return defIntPath; }
					else if (customAttr.Length == 0) { return defExtPath; }
					else { return ((FormulaFunctionCategoriesAttribute)customAttr[0]).Path; }
				});
			// Insère les fonctions:
			int l = mis.Length;
			for (int i=0; i<l; i++)
			{
				// Continue si fonction à exclure:
				if (mis[i].GetCustomAttributes(typeof(ExcludeFromManAttribute), true).Length != 0) { continue; }
				// Sinon, insère:
				node = new TreeNode(mis[i].Name);
				// Cherche les synstaxes:
				string tag = String.Empty;
				for (int j=0; j<l; j++) {
					if (mis[i].Name == mis[j].Name && ArrayFunctions.ArrayEquals(cat[i], cat[j]))
						{ tag += ClassManager.GetMethodSyntax(mis[j], true) + "\n"; } }
				if (tag.Length > 1) { tag = tag.Substring(0, tag.Length - 1); }
				node.Tag = tag;
				ControlsFunctions.SetNodeInTreeView(cat[i], node, tree, true);
			}
			// Sérialise l'arbre dans un fichier:
			tree.Sort();
			TreeNode[] nodes = new TreeNode[tree.Nodes.Count];
			for (int i=0; i<nodes.Length; i++) { nodes[i] = tree.Nodes[i]; }
			// Enregistre dans filename:
			return My.FilesAndStreams.SerializeInFile(filename, nodes);
		}
	
	

		#endregion METHODES PUBLIQUES






		// ---------------------------------------------------------------------------
		// METHODES PRIVEES
		// ---------------------------------------------------------------------------




		#region METHODES PRIVEES


		
		
		/// <summary>
		/// Ecrit les codes MSIL pour convertir le dernier élément de la pile du type srcType vers le type destType. Ne doivent être passé que les types primitifs ou Decimal, sinon erreur lors de l'exécution de la fonction dynamique.
		/// </summary>
		private static void ConvertTo(ILGenerator il, Type srcType, Type destType)
		{
		
			// Si les types sont les mêmes, on sort:
			if (srcType == destType) { return; }
			
			// Si les deux types sont primitifs, on convertit avec les méthodes MSIL, et sort:
			if (srcType.IsPrimitive && destType.IsPrimitive)
			{
				if (destType == typeof(Int32)) { il.Emit(OpCodes.Conv_I4); return; }
				else if (destType == typeof(Int64)) { il.Emit(OpCodes.Conv_I8); return; }
				else if (destType == typeof(Single)) { il.Emit(OpCodes.Conv_R4); return; }
				else if (destType == typeof(Double)) { il.Emit(OpCodes.Conv_R8); return; }
			}
			
			// Sinon, ou si types primitifs peu courant, on utilise la méthode ChangeType:
			// Box (place une référence d'objet) sur la pile, car ChangeType a pour premier argument un object:
			il.Emit(OpCodes.Box, srcType);
			// Place un token du type de destination sur la pile:
			il.Emit(OpCodes.Ldtoken, destType);
			// Récupère le handle du type:
			il.EmitCall(OpCodes.Call, _methGetTypeFromHandle, null);
			// Appelle ChangeType sur les deux derniers éléments de la pile:
			il.EmitCall(OpCodes.Call, _methChangeType, null);
			// On a obtenu un object, qu'on va unboxer et dont on va charger la valeur:
			// il.Emit(OpCodes.Unbox, destType);
			// il.Emit(OpCodes.Ldobj, destType); Ou en une seule instruction:
			il.Emit(OpCodes.Unbox_Any, destType);
			
		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Cherche la meilleure surcharge de la méthode demandée. funcPart est une partie de formule correspondant à une fonction. name est le nom de la fonction, methods est le choix des fonctions. workingType est le type numérique désirée (double ou decimal), paramsArrayIndex est augmenté si la fonction a pour dernier paramètre un tableau (de type params, par exemple). Dans ce cas, il doit y avoir au minimum un paramètre (pas de tableau vide, donc). funcPart est directement modifié. Lève une exception si la méthode n'existe pas, où si elle n'a pas les bons paramètres.
		/// </summary>
		private static void GetMethod(ref FormPart funcPart, string name, MethodInfo[] methods, FormulaWorkingType workingType,
			ref int paramsArrayIndex)
		{
		
			// Obtient un tableau correspondant à la méthode nommée:
			MethodInfo[] meths = methods.Where(delegate(MethodInfo mi) { return mi.Name.Equals(name); }).ToArray();
			if (meths.Length == 0) { throw new FormulaException(String.Format("The method \"{0}\" doesn't exist!", name)); }
			
			// Cherche une méthode correspondant aux paramètres de funcPart, parcourt toutes les méthodes name:
			int lenParamsPart = funcPart.Children.Length;
			foreach (MethodInfo mi in meths)
			{
			
				// Flags:
				bool flagIsOk, isParams = false; int paramsArrInd = paramsArrayIndex;
				
				// Construit le tableau des types de params de la méthode en cours d'étude:
				ParameterInfo[] pis = mi.GetParameters();
				int lenParamsMeth = pis.Length;
				// Si pas de paramètre de chaque côté, on retourne la fonction (c'est une fonction sans argument):
				if (lenParamsMeth == 0 && funcPart.Children[0].Children[0].Type == FormPartType.Empty)
					{ funcPart.Method = mi; return; }
				// Construit le tableau:
				Type[] paramsMethTypes = new Type[lenParamsMeth];
				for (int i=0; i<lenParamsMeth; i++) { paramsMethTypes[i] = pis[i].ParameterType; }
				// Si le nombre de paramètres de funcPart est inférieur à celui des paramètres de la méthode en cours,
				// on passe directement à la suite:
				if (lenParamsPart < lenParamsMeth) { continue; } 
				
				// Si le dernier paramètre est un tableau, on le considère comme "params", et on redimmensionne les tableaux
				// avant de compléter celui des types avec le type du tableau:
				if (paramsMethTypes[lenParamsMeth-1].IsArray)
				{
					isParams = true; // Flag qui indique qu'on est dans le cas d'un params
					paramsArrInd = paramsArrayIndex + 1; // Augmente le compteur d'index
					funcPart.ParamsArrayIndex = paramsArrInd; // Inscrit l'index général du tableau parmis tous les tableaux:
					Type arrType = My.ArrayFunctions.GetElementType(paramsMethTypes[lenParamsMeth-1]);
					Array.Resize(ref paramsMethTypes, lenParamsPart);
					for (int i=lenParamsMeth-1; i<lenParamsPart; i++) { paramsMethTypes[i] = arrType; } // Complète le tableau des types
				}
				
				// Maintenant, on parcours tous les FormPart de paramètre (tous les enfants du funcPart), et on vérifie que
				// les types des enfants de funcPart correspondent avec les types donnés par le MethodInfo:
				flagIsOk = true;
				for (int i=0; i<lenParamsPart; i++)
				{
					// Cherche la partie significative du paramètre (celle qui contient un type de données, en évitant ainsi les
					// parenthèses):
					FormPart sigPart = funcPart.Children[i].Children[0]; 
					while (sigPart.Type == FormPartType.Bracket) { sigPart = sigPart.Children[0]; }
					// Si nombre et type primitif, et que le type de travail est Double:
					bool match = (sigPart.Type == FormPartType.Number && paramsMethTypes[i].IsPrimitive
						&& workingType == FormulaWorkingType.Double);
					// Idem avec Decimal:
					match = match || (sigPart.Type == FormPartType.Number && paramsMethTypes[i] == typeof(Decimal)
						&& workingType == FormulaWorkingType.Decimal);
					// Si texte et type String
					match = match || (sigPart.Type == FormPartType.Text && paramsMethTypes[i] == typeof(String));
					// Si variable et type Primitif, et que le type de travail est Double:
					match = match || (sigPart.Type == FormPartType.Variable && paramsMethTypes[i].IsPrimitive
						&& workingType == FormulaWorkingType.Double);
					// Idem avec Decimal:
					match = match || (sigPart.Type == FormPartType.Variable && paramsMethTypes[i] == typeof(Decimal)
						&& workingType == FormulaWorkingType.Decimal);
					// Si méthode et retType et paramsMethTypes[i] sont primitifs tous deux:
					match = match || (sigPart.Type == FormPartType.Method && sigPart.Method.ReturnType.IsPrimitive
						&& paramsMethTypes[i].IsPrimitive);
					// Si méthode et retType et paramsMethTypes[i] sont les mêmes (y compris Decimal) (ou si l'un est enfant de l'autre):
					match = match || (sigPart.Type == FormPartType.Method
						&& (sigPart.Method.ReturnType == paramsMethTypes[i] || sigPart.Method.ReturnType.IsSubclassOf(paramsMethTypes[i])));
					// Si object, tout est accepté:
					match = match || (paramsMethTypes[i] == typeof(Object));
					// Si on a trouvé une correspondance:
					if (match)
					{
						// Complète le type du paramètre:
						funcPart.Children[i].ParameterType = paramsMethTypes[i];
						// Si le dernier paramètre est "params", inscrit pour chaque paramètre les index correspondant:
						if (isParams && i >= lenParamsMeth - 1)
						{
							funcPart.Children[i].ParamsElementIndex = i - lenParamsMeth + 1;
							funcPart.Children[i].ParamsArrayIndex = paramsArrInd;
						}
					}
					// Sinon, sort de la boucle et tente une nouvelle méthode:
					else
						{ flagIsOk = false; break; }
				}
				
				// Si on a trouvé une bonne méthode, on retourne le résultat, sinon, on continue:
				if (flagIsOk)
				{
					funcPart.Method = mi;
					paramsArrayIndex = paramsArrInd;
					return;
				}
				
			}
			
			// Si on arrive là, c'est qu'on a rien trouvé:
			int l = meths.Length; string[] syntaxes = new string[l];
			for (int i=0; i<l; i++) { syntaxes[i] = My.ClassManager.GetMethodSyntax(meths[i], false); }
			throw new FormulaException(String.Format("The method \"{0}\" has not correct parameters. " +
				"Use one of the following syntax:\n{1}", name, My.ArrayFunctions.Join(syntaxes, "\n")));
		
		}
		

		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Découpe la formule en différentes parties, chacune représentant un niveau de parenthèse ou de fonction. Tout part d'un FormPart de type Bracket initial, qui représente l'ensemble de la formule, et qui ne contient rien sinon un tableau d'enfants. Chaque enfant représente un même niveau de parenthèse ou de fonction, et ainsi de suite. De manière général, tous les frères (tous les éléments du tableau Children d'un même FormPart) sont sur le même niveau. A chaque parenthèse, on augmente d'un niveau. Les fonctions représentent un niveau qui ne contient rien sinon des enfants, qui représentent eux-mêmes chacun un argument. Chaque argument est un FormPart qui ne contient rien sinon un tableau d'enfants, représentant les termes de la formule de l'argument, et ces enfants peuvent à leur tour contenir des petits-enfants, représentant chacun un niveau supplémentaire de parenthèses ou de fonction, et ainsi de suite.
		/// </summary>
		private static FormPart SplitFormula(string formula, string[] variables, MethodInfo[] usedMethods, FormulaWorkingType workingType)
		{
		
			// Supprime les guillemets au début et à la fin:
			while ((formula.StartsWith("\"") && formula.EndsWith("\"")) || (formula.StartsWith("“") && formula.EndsWith("”"))
				|| (formula.StartsWith("‘") && formula.EndsWith("’")))
				{ formula = formula.Substring(1, formula.Length - 2); }
			
			// Compte le nombre de parenthèse ouvrante et fermante, et lève une exception si ça ne correspond pas:
			int brCounter = 0, l = formula.Length; string ch;
			for (int i=0; i<l; i++) {
				ch = formula.Substring(i, 1);
				if (ch.Equals("(")) { brCounter++; } else if (ch.Equals(")")) { brCounter--; } }
			if (brCounter != 0 && String.IsNullOrEmpty(formula)) { throw new FormulaException(String.Format("Formula \"{0}\" has a syntax error!", formula)); }
			
			// Méthode anonyme pour déterminer si une partie est un nombre, une variable ou un texte:
			Action<FormPart> setNbVarText =
				delegate(FormPart part)
				{
					if (part.Type == FormPartType.None)
					{
						if (String.IsNullOrEmpty(part.Text)) { part.Type = FormPartType.Empty; return; }
						if (variables.Contains(part.Text)) { part.Type = FormPartType.Variable; return; }
						try {
							if (workingType == FormulaWorkingType.Double) { _methDoubleParse.Invoke(null, new object[]{part.Text}); }
							else { _methDecimalParse.Invoke(null, new object[]{part.Text}); }
							part.Type = FormPartType.Number; }
						catch { part.Type = FormPartType.Text; }
					}
				};
			
			// Partie racine:
			FormPart curPart = new FormPart(null, null);
			curPart.Type = FormPartType.Bracket;
			curPart.AddChild(curPart = new FormPart(null, curPart));
			
			// Parcourt les caractères et enregistre les éléments dans curPart, en créant pour chaque
			// nouvelle partie des enfants:
			int paramNb, paramsArrayIndex = -1; string methName, text;
			for (int i=0; i<l; i++)
			{
			
				ch = formula.Substring(i, 1);
				
				// Si guillemet, on considère l'intérieur des guillemets comme une chaîne:
				if (ch == "\"" || ch == "“" || ch == "‘")
				{
					int endQuote = My.FieldsParser.GetSubfield(formula, i);
					if (endQuote == -1) { throw new FormulaException(String.Format("Formula \"{0}\" has a syntax error!", formula)); }
					curPart.Text = formula.Substring(i + 1, endQuote - i - 1);
					i = endQuote; continue;
				}
				
				// Si espace, on saute:
				if (ch == " ") { continue; }
				
				// Si une parenthèse ouvrante suit une parenthèse fermante, on rajoute * (eg. ")(" => ")*("):
				if (i < l-1 && ch == ")" && formula.Substring(i+1, 1) == "(")
				{
					//formula = formula.Substring(0, i+1) + "*" + formula.Substring(i+1);
					formula = formula.Insert(i + 1, "*");
					l++;
				}
				
				// Si -, et si avant il y a un opérateur, et si après il y a un caractère qui n'est pas un chiffre ou π (le
				// cas de π est traité dans la suite, normalement), alors c'est que le - est là pour *-1. Par ex:
				// 5*-x (où x est une variable) vaut pour 5*-1*x, de même pour -x qui vaut pour -1*x:
				if (ch == "-" &&
					(i == 0 || "+*/^$=≠≈<>≤≥(,".Contains(formula.Substring(i-1, 1)))
					&& (i < l-1 && !")π0123456789".Contains(formula.Substring(i+1, 1))))
				{
					formula = formula.Remove(i, 1).Insert(i, "-1*");
					l += 2;
				}
				
				// Si chiffre suivi d'une lettre ou d'un "(", alors on place un "*". Par ex: 3x devient 3*x, ou 3(1) devient 3*(1):
				if (i < l-1 && "0123456789".Contains(ch) && (Char.IsLetter(formula.Substring(i+1, 1), 0) || formula.Substring(i+1, 1) == "("))
				{
					formula = formula.Insert(i + 1, "*");
					l++;
				}
				
				// Puis on étudie les autres caractères significatifs:
				
				// Si opérateur, clot la partie courante et en commence une nouvelle:
				if (("+*/^$=≠≈<>≤≥".Contains(ch))
					|| ((ch == "-") && (i > 0) && ((")π0123456789".Contains(formula.Substring(i-1, 1)))
							|| variables.Contains(curPart.Text))))
				{
					// Si on est à la fin, exception:
					if (i == l-1) { throw new FormulaException(String.Format("Formula \"{0}\" has a syntax error!", formula)); }
					// Clot la partie courante:
					setNbVarText(curPart);
					// Nouvelle partie, soeur de la courante:
					curPart.AddSibling(curPart = new FormPart(ch, curPart.Parent));
				}
				
				// Si parenthèse ouvrante, examine le texte précédent: S'il existe,
				// c'est une fonction, sinon, c'est une nouvelle parenthèse:
				else if (ch == "(")
				{
					if (String.IsNullOrEmpty(curPart.Text)) // Pas fonction
					{
						// Définit la partie courante comme parenthèse:
						curPart.Type = FormPartType.Bracket;
						// Nouvelle partie, fille de la courante:
						curPart.AddChild(curPart = new FormPart(ch, curPart));
					}
					else // Fonction
					{
						// Définit la partie courante comme méthode:
						curPart.Type = FormPartType.Method;
						methName = curPart.Text;
						// Nouvelle partie (param), fille de la courante:
						curPart.AddChild(curPart = new FormPart(ch, curPart));
						curPart.Type = FormPartType.MethParam;
						curPart.ParameterNb = 0;
						// Nouvelle partie (éléments du paramètre), fille de la courante:
						curPart.AddChild(curPart = new FormPart(ch, curPart));
					}
				}
				
				// Si parenthèse fermante, clot la partie précédente, remonte d'un niveau, et si on clot
				// une fonction, on met à jour les informations de la partie "Function":
				else if (ch == ")")
				{
					// Clot la partie courante, et remonte d'un niveau:
					setNbVarText(curPart);
					text = curPart.Text;
					curPart = curPart.Parent;
					// Si fonction, on est dans un paramètre, donc on remonte encore d'un niveau, et
					// on vérifie qu'il y a bien le nombre de paramètre requis:
					if (curPart.Type == FormPartType.MethParam)
					{
						paramNb = curPart.ParameterNb;
						curPart = curPart.Parent;
						// Obtient l'analyse de la fonction et sa meilleure surcharge en fonction des types de FormPart trouvés:
						GetMethod(ref curPart, curPart.Text, usedMethods, workingType, ref paramsArrayIndex);
					}
				}
				
				// Si virgule, c'est un nouvel argument pour une fonction: Clot la partie courante et en ajoute une nouvelle:
				else if (ch == ",")
				{
					// Clot la partie courante:
					setNbVarText(curPart);
					// Remonte d'un niveau (on est alors au niveau du paramètre):
					curPart = curPart.Parent;
					paramNb = curPart.ParameterNb + 1;
					// Nouvelle partie (param), soeur de la courante:
					curPart.AddSibling(curPart = new FormPart(ch, curPart.Parent));
					curPart.Type = FormPartType.MethParam;
					curPart.ParameterNb = paramNb;
					// Nouvelle partie (éléments du paramètre), fille de la courante:
					curPart.AddChild(curPart = new FormPart(ch, curPart));
				}
				
				// Si π, on inscrit la constante avec la précision souhaité (en double ou decimal):
				else if (ch == "π")
				{
					curPart.Text += "3.1415926535897932384626433833";
				}
				
				// Si rien de tout cela, c'est un texte à écrire (un nombre, une variable, un texte, un nom de fonction):
				else
				{
					curPart.Text += ch;
				}
				
			}
			
			// Sortie: Une fois arrivée ici, le parent du parent de curPart doit être null (ce qui veut dire que curPart.Parent
			// est la partie racine), et alors retourne curPart.Parent. Sinon, exception car il y a une erreur de syntax:
			setNbVarText(curPart);
			if (curPart.Parent.Parent != null) { throw new FormulaException(String.Format("Formula \"{0}\" has a syntax error!", formula)); }
			return curPart.Parent;

		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Ajoute à _codeParts la partie passé en argument, en gérant le redimmensionnement du tableau.
		/// </summary>
		private static void AddCodePart(CodePart part, ref CodePart[] codeParts, ref int index)
		{
			if (codeParts == null) { codeParts = new CodePart[20]; }
			if (index >= codeParts.Length - 1) { Array.Resize(ref codeParts, index + 20); }
			codeParts[++index] = part;
		}
		
		
		/// <summary>
		/// Ajoute à _codeParts tous les opérateurs de ops supérieur ou égaux à priority, dans l'ordre inverse.
		/// </summary>
		private static void AddOperatorsToCodeParts(OpType[] ops, int priority, int c, ref CodePart[] codeParts, ref int index)
		{
			for (int i=c-1; i>=0; i--)
			{
				if ((ops[i] != OpType.None) && (_priorities[ops[i]] >= priority))
					{ AddCodePart(new CodePart(ops[i]), ref codeParts, ref index); ops[i] = OpType.None; }
			}
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Remplit le tableau _codeParts à partir d'un FormPart. La fonction est récursive, et s'appelle à chaque nouveau niveau de FormPart (propriété Children).
		/// </summary>
		private static void BuildCode(FormPart formPart, ref CodePart[] codeParts, ref int index)
		{
		
			// Variables pour les opérateurs restants, à rajouter à la fin:
			OpType[] ops = new OpType[20];
			int c = 0;

			// Parcours le tableau des enfants:
			int l = formPart.Children.Length; FormPart f;
			for (int i=0; i<l; i++)
			{
				f = formPart.Children[i];
				// Vide la pile d'attente des opérateurs de prioriété supérieur ou égale à l'opérateur précédent le terme en cours:
				AddOperatorsToCodeParts(ops, f.OperatorPriority, c, ref codeParts, ref index);
				// Engage une action en fonction du type de la partie en cours:
				switch (f.Type)
				{
					case FormPartType.Method:
						BuildCode(f, ref codeParts, ref index); // Pour tous les paramètres
						AddCodePart(new CodePart(f.Method), ref codeParts, ref index); // Appelle de fonction
						break;
					case FormPartType.MethParam:
						// Si le paramètre est le premier d'un params: construit un tableau:
						if (f.ParamsElementIndex == 0) {
							AddCodePart(new CodePart(f.ParamsArrayIndex,
								f.Parent.Children[f.Parent.Children.Length-1].ParamsElementIndex + 1, f.ParameterType), ref codeParts, ref index); }
						// Si le paramètre est un params, on ajoute un index au tableau:
						if (f.ParamsElementIndex > -1) {
							AddCodePart(new CodePart(f.ParamsArrayIndex, f.ParamsElementIndex), ref codeParts, ref index); }
						// Pour les termes du paramètres
						BuildCode(f, ref codeParts, ref index);
						// Conversion de paramètre s'il y en a un, et si primitif ou Decimal, Object (pour Box):
						if (f.ParameterType != null && (f.ParameterType.IsPrimitive || f.ParameterType == typeof(Decimal) || f.ParameterType == typeof(Object)))
							{ AddCodePart(new CodePart(f.ParameterType), ref codeParts, ref index); }
						// Si le paramètre est un params, on enregistre l'élément du tableau:
						if (f.ParamsElementIndex > -1) {
							AddCodePart(new CodePart(f.ParamsArrayIndex, f.ParameterType), ref codeParts, ref index); }
						break;
					case FormPartType.Number:
						if (f.Type != FormPartType.Empty)
							{ AddCodePart(new CodePart(f.Text, CodePartType.Number), ref codeParts, ref index); }
						break;
					case FormPartType.Variable:
						if (f.Type != FormPartType.Empty)
							{ AddCodePart(new CodePart(f.Text, CodePartType.Variable), ref codeParts, ref index); }
						break;
					case FormPartType.Text:
						// Pour que ce soit un texte, il faut le parent soit un paramètre de fonction String, et qu'il n'y
						// ait pas d'opération (donc qu'il n'y ait pas de frère et soeur), sinon lève exception:
						if (f.Parent.Type != FormPartType.MethParam || f.Parent.Children.Length != 1
								|| (f.Parent.ParameterType != typeof(String) && f.Parent.ParameterType != typeof(Object)))
							{ throw new FormulaException(String.Format("\"{0}\" is not a valid fonction, number or text.", f.Text)); }
						if (f.Type != FormPartType.Empty)
							{ AddCodePart(new CodePart(f.Text, CodePartType.Text), ref codeParts, ref index); }
						break;
					case FormPartType.Bracket:
						BuildCode(f, ref codeParts, ref index); // Pour les termes de la parenthèse
						break;
					case FormPartType.None:
						throw new FormulaException(String.Format("Formula has a syntax error!"));
				}
				// Continue si pas d'opérateur:
				if (f.PreviousOperator == OpType.None) { continue; }
				// Si on est à la fin, ou si l'opérateur suivant est de niveau supérieur ou égal au précédent, on place l'opérateur sur la pile. 
				if ((i == l - 1) || (f.OperatorPriority >= formPart.Children[i+1].OperatorPriority))
					{ AddCodePart(new CodePart(f.PreviousOperator), ref codeParts, ref index); }
				// Sinon, ajoute l'opérateur à la pile d'attente:
				else { ops[c++] = f.PreviousOperator; }
			}
			// A la fin, vide tous les opérateurs:
			AddOperatorsToCodeParts(ops, 0, c, ref codeParts, ref index);

		}
	



		#endregion METHODES PRIVEES
	
	
	
	}
	
	
	
}
