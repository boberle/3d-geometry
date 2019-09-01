using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

namespace My
{




	/// <summary>
	/// Fournit des méthodes simples d'acquisition d'objets pour les formules.
	/// </summary>
	public static class MethodsForFormulas
	{

		/// <summary>
		/// Obtient les méthodes de cette classe.
		/// </summary>
		public static MethodInfo[] Methods { get; private set; }
		
		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static MethodsForFormulas()
		{
			Methods = typeof(MethodsForFormulas).GetMethods(BindingFlags.Static | BindingFlags.Public);
		}

		public static string GetText(string text)
			{ return text; }

		public static DecimalF GetDF(double nb)
			{ return new DecimalF(nb); }
		
		public static DecimalF GetDF(string s)
			{ return new DecimalF(s); }
		
		public static bool GetBool(int value)
			{ return (value != 0); }

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
			None, Add, Mul, Div, Sub, Pow
		}



		private enum FormPartType
		{
			None, Method, MethParam, Bracket, Number, Variable, Text, Empty
		}


		private enum CodePartType
		{
			None, Variable, Number, Text, FuncCall, Operator, ParameterConversion, ArrCreate, ArrSetIndex, ArrSaveElement
		}


		private enum CalcMeth
		{
			Pow, AddDec, SubDec, DivDec, MulDec
		}
		
		
		private enum VerifyType
		{
			NotZero, NotNegative
		}
		
		
		public enum ErrorAction
		{
			None, Throw, MaxValue
		}


		// ---------------------------------------------------------------------------
	

		private class FormPart
		{
		
			// Déclarations:
			OpType _prevOp; int _opPriority; FormPart[] _children;
		
			// Propriétés:
			public FormPartType Type { get; set; }
			public string Text { get; set; }
			public MethodInfo Method { get; set; }
			public Type ParameterType { get; set; }
			public int ParameterNb { get; set; }
			public int ParamsArrayIndex { get; set; } // Si le paramètre est un paramètre de params, indique l'index du tableau (s'il y a plusieurs tableaux)
			public int ParamsElementIndex { get; set; } // Index du paramètre dans le tableau params
			public OpType PreviousOperator { get { return _prevOp; } }
			public int OperatorPriority { get { return _opPriority; } } 
			public FormPart Parent { get; set; }
			public FormPart[] Children { get { return _children; } }
			
			// Constructeurs:
			private FormPart()
				{ Type = FormPartType.None; _children = new FormPart[0]; ParameterNb = -1;
				ParamsArrayIndex = -1; ParamsElementIndex = -1; }
			public FormPart(string prevOp, FormPart parent) : this()
				{ Type = FormPartType.None; SetPreviousOperator(prevOp); Parent = parent; }

			// Définit l'opérateur et la priorité:
			public void SetPreviousOperator(string s)
			{
				if (s == null) { s = String.Empty; }
				switch (s)
				{
					case "+": _prevOp = OpType.Add; break;
					case "-": _prevOp = OpType.Sub; break;
					case "*": _prevOp = OpType.Mul; break;
					case "/": _prevOp = OpType.Div; break;
					case "^": _prevOp = OpType.Pow; break;
					default: _prevOp = OpType.None; break;
				}
				_opPriority = _priorities[_prevOp]; 
			}
			
			// Ajoute un enfant:
			public void AddChild(FormPart child)
			{
				int c = _children.Length;
				Array.Resize(ref _children, c + 1);
				this.Children[c] = child;
			}
			
			// Ajoute un frère:
			public void AddSibling(FormPart sibling)
			{
				int c = this.Parent._children.Length;
				Array.Resize(ref this.Parent._children, c + 1);
				this.Parent.Children[c] = sibling;
			}

		}


		// ---------------------------------------------------------------------------


		private struct CodePart
		{
		
			// Propriétés:
			public string Text { get; set; }
			public CodePartType Type { get; set; }
			public OpType OpType { get; set; }
			public MethodInfo MethodToCall { get; set; }
			public Type ConvertTo { get; set; }
			public int ArrayLength { get; set; }
			public int ArrayElementIndex { get; set; }
			public Type ArrayElementType { get; set; }
			public int ArrayIndex { get; set; }
			
			// Constructeurs:
			public CodePart(OpType opType) : this() // Opérateur
				{ Type = CodePartType.Operator; OpType = opType; }
			public CodePart(MethodInfo methToCall) : this() // FuncCall
				{ Type = CodePartType.FuncCall; MethodToCall = methToCall; }
			public CodePart(string text, CodePartType type) : this() // Number et variable, ou text
				{ Type = type; Text = text; }
			public CodePart(Type converTo) : this() // Conversion
				{ Type = CodePartType.ParameterConversion; ConvertTo = converTo; }
			public CodePart(int arrIndex, int arrLength, Type elementType) : this() // Nouvel Array
				{ Type = CodePartType.ArrCreate; ArrayIndex = arrIndex; ArrayLength = arrLength;
					ArrayElementType = elementType; }
			public CodePart(int arrIndex, int elementIndex) : this() // Set index
				{ Type = CodePartType.ArrSetIndex; ArrayIndex = arrIndex; ArrayElementIndex = elementIndex; }
			public CodePart(int arrIndex, Type elementType) : this() // Save element in array
				{ Type = CodePartType.ArrSaveElement; ArrayIndex = arrIndex; ArrayElementType = elementType; }
			
			#if DEBUG
			public override string ToString()
			{
				string s = Type.ToString() + ": " ;
				switch (Type)
				{
					case CodePartType.Operator: s += OpType.ToString(); break;
					case CodePartType.ParameterConversion: s += ConvertTo.ToString(); break;
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
		private static Type[] _numTypes;
		// Tableau des codes:
		private static CodePart[] _codeParts;
		private static int _index;
		// Tableau des méthodes mathématiques par défaut:
		private static MethodInfo[] _defaultMeth;
		private static MethodInfo[] _generalUsedFunctions;
		// Constructeur Decimal(int[] bits) du type decimal pour le chargement en MSIL de constante numérique en Decimal:
		private static ConstructorInfo __DecimalCtor;



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
			set { if (value == null) { _generalUsedFunctions = new MethodInfo[0]; }
				else { _generalUsedFunctions = value; } } }
		



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
			// Priorités:
			_priorities = new Dictionary<OpType,int>(6);
			_priorities.Add(OpType.Add, 1);
			_priorities.Add(OpType.Sub, 1);
			_priorities.Add(OpType.Mul, 2);
			_priorities.Add(OpType.Div, 2);
			_priorities.Add(OpType.Pow, 3);
			_priorities.Add(OpType.None, 0);
			// Types numériques:
			_numTypes = new Type[]{typeof(Byte),typeof(SByte),typeof(Int16),typeof(UInt16),typeof(Int32),typeof(UInt32),
			typeof(Int64),typeof(UInt64),typeof(Single),typeof(Double),typeof(Decimal)};
			// Tableau des codes:
			_codeParts = null;
			_index = -1;
			// Méthode pour la conversion des types:
			__methChangeType_ = typeof(Convert).GetMethod("ChangeType", new Type[]{typeof(Object),typeof(Type)});
			__methGetTypeFromHandle_ = typeof(Type).GetMethod("GetTypeFromHandle", new Type[]{typeof(RuntimeTypeHandle)});
			__locTypeConv_ = new Dictionary<Type,LocalBuilder>();
			// Constructeur Decimal(int[] bits) de Decimal:
			__DecimalCtor = typeof(Decimal).GetConstructor(new Type[]{typeof(int[])});
			// Méthode de calcul:
			__methPowDouble_ = typeof(Math).GetMethod("Pow", new Type[]{typeof(Double),typeof(Double)});
			__methAddDecimal_ = typeof(Decimal).GetMethod("Add", new Type[]{typeof(Decimal),typeof(Decimal)});
			__methSubtractDecimal_ = typeof(Decimal).GetMethod("Subtract", new Type[]{typeof(Decimal),typeof(Decimal)});
			__methDivideDecimal_ = typeof(Decimal).GetMethod("Divide", new Type[]{typeof(Decimal),typeof(Decimal)});
			__methMultiplyDecimal_ = typeof(Decimal).GetMethod("Multiply", new Type[]{typeof(Decimal),typeof(Decimal)});
			// Méthodes mathématiques par défaut (il est important de mettre les types généraux de paramètres des surcharges en premiers):
			_defaultMeth = new MethodInfo[8]; int c = 0;
			_defaultMeth[c++] = typeof(Math).GetMethod("Sqrt");
			_defaultMeth[c++] = typeof(Math).GetMethod("Abs", new Type[]{typeof(Double)});
			_defaultMeth[c++] = typeof(Math).GetMethod("Abs", new Type[]{typeof(Decimal)});
			_defaultMeth[c++] = typeof(Math).GetMethod("Abs", new Type[]{typeof(Single)});
			_defaultMeth[c++] = typeof(Math).GetMethod("Abs", new Type[]{typeof(Int32)});
			_defaultMeth[c++] = typeof(Math).GetMethod("Cos");
			_defaultMeth[c++] = typeof(Math).GetMethod("Tan");
			_defaultMeth[c++] = typeof(Math).GetMethod("Sin");
			// Méthodes d'acquisiation:
			_defaultMeth = _defaultMeth.Concat(MethodsForFormulas.Methods).ToArray();
			// Méthodes mathématiques personnelles:
			_defaultMeth = _defaultMeth.Concat(Maths.AlgebraFunctions).ToArray();
			_defaultMeth = _defaultMeth.Concat(Maths.GeometryFunctions).ToArray();
			// Initialisation:
			_generalUsedFunctions = new MethodInfo[0];
		}


		#endregion CONSTRUCTEURS






		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES
		
		
		
		/// <summary>
		/// Créer une fonction dynamique renvoyée sous forme de délégué à partir d'une formule mathématique passée dans formula. name est le nom de la fonction. variables est un tableau de chaînes qui contient les noms des variables utilisées dans la formule (e.g. "x+2" : x est la variable), et qui correspondent, dans l'ordre, a paramètres de la fonction, cad au tableau paramTypes (il est évident que les index des noms de variables doivent correspondre aux index des types de paramètres). returnType est le type de retour de la fonction. delegType est le type du délégué retourné par la fonction. owner est le type auquel sera attaché la méthode dynamique, et ce type est important pour des questions de visibilité des méthodes appelées dans la formule. curType est le type vers lequel sont converti les constantes qu'on peut trouver dans la formule (e.g. si curType est de type Int32, alors dans "3.12*x" 3.12 sera converti en Int32...). parmaTypes est un tableau de types contenant les types des paramètres de la fonction dynamique. usedMethods est un tableau de MethodInfo contenant les fonctions qui peuvent être appelés dans la formule, auxquelles seront automatiquement rajoutées des fonctions mathématiques par défaut (voir le code du constructeur pour la liste des fonctions par défaut, et l'aide de GetMethod pour les spécifités des arguments de ces méthodes). Si la formule contient un nom de fonction qui n'est pas dans usedMethods, une exception est générée. Ces méthodes doivent être de type publique et statique, et doivent être visible par owner.
		/// Syntaxe de la formule: Tout les termes et facteur doivent être séparés par un opérateur ("3*x" est accepté, mais pas "3x"). La seule exception sont les facteurs de deux parenthèse, du type "(a)(b)" qui est interprété comme "(a)*(b)". Mais une expression comme "3(a)" n'est pas acceptée. Le lettre "π" (pi) est remplacée par la valeur, avec la précision imposée par curType (si curType est Int32, donc, on aura π = 3...).
		/// Gestion des erreurs: Si errorAction vaut None, alors il n'y a pas de gestion des erreurs. En cas d'opération interdite (division par zéro, racine d'un réel négatif, etc.), certains types comme Double ne lèveront pas d'exception mais seront Infinite ou NaN, alors que d'autres lèveront une exception. Si errorAction vaut Throw, alors dans tous les cas une exception sera levée en cas d'opération interdite. Si errorAction vaut MaxValue, alors la valeur Maximum de returnType issue de returnType.MaxValue sera retournée en cas d'opération interdite, mais aucune exception ne sera levée, ce qui permet d'économiser beaucoup de temps si l'on fait beaucou de calculs, car une comparaison sur une valeur est bien plus rapide que le traitement d'un catch quand il y a une erreur (quand il n'y a pas d'erreur, le try/catch ne prend pas beaucoup plus de temps). Pour l'heure, les opérations interdites testés sont la division par zéro et l'utilisation de Sqrt (de Math.Sqrt) avec un paramètre négatif.
		/// </summary>
		public static Delegate CreateFormulaMethod(string name, string formula, string[] variables, Type returnType, Type delegType, Type owner, Type curType, Type[] paramTypes, MethodInfo[] usedMethods, ErrorAction errorAction)
		{
		
			// Vérifie que curType est un type numérique:
			if (!_numTypes.Contains(curType))
				{ throw new Exception("The current type must be a numeric type."); }
			
			// Variables:
			bool returnTypeIsNum = _numTypes.Contains(returnType);
			if (variables == null) { variables = new string[0]; }
			if (paramTypes == null) { paramTypes = new Type[0]; }
			if (usedMethods == null) { usedMethods = new MethodInfo[0]; }
			
			// Si returnType n'est pas numérique, la gestion des erreurs est sur None:
			if (!returnTypeIsNum) { errorAction = ErrorAction.None; }

			// Reset:
			_codeParts = null;
			_index = -1;
			__locTypeConv_ = new Dictionary<Type,LocalBuilder>();
								
			// Obtient la formule découpée, et appelle la méthode de construction du code:
			FormPart parts = SplitFormula(formula, variables, usedMethods.Concat(_defaultMeth).Concat(_generalUsedFunctions).ToArray(), curType);
			BuildCode(parts);
			// Retaille le tableau:
			_codeParts = _codeParts.Where(delegate(CodePart p) { return p.Type != CodePartType.None; }).ToArray();
			
			// Affichage des données pour le débogage:
			//string msg = formula + "\n\n";
			//for (int i=0; i<_codeParts.Length; i++) { msg += _codeParts[i].ToString() + "\n"; }
			//System.Windows.Forms.MessageBox.Show(msg);
			
			// Créer une fonction dynamique et récupère un IL Generator:
			DynamicMethod dynMethod = new DynamicMethod(name, returnType, paramTypes, owner);
			dynMethod.InitLocals = true;
			ILGenerator il = dynMethod.GetILGenerator();
			
			// Une fois le tableau _codeParts construit, il ne reste plus qu'à le convertir en MSIL:
			int paramIndex; bool primitive = curType.IsPrimitive;
			
			// Tableau (C#) des tableaux (IL)...
			LocalBuilder[] arrayIndexes = new LocalBuilder[0];

			// Définit une variable d'erreur, et y inscrit 0:
			LocalBuilder ilErrLoc = null;
			if (errorAction != ErrorAction.None) {
				ilErrLoc = il.DeclareLocal(returnType);
				WriteConstantNumber(il, "0", returnType);
				il.Emit(OpCodes.Stloc, ilErrLoc); }
			
			
			// Pour toutes les lignes de codes:
			foreach (CodePart p in _codeParts)
			{
				switch (p.Type)
				{
					case CodePartType.Number:
						WriteConstantNumber(il, p.Text, curType);
						break;
					case CodePartType.Variable:
						paramIndex = Array.IndexOf(variables, p.Text);
						il.Emit(OpCodes.Ldarg_S, paramIndex);
						if (_numTypes.Contains(paramTypes[paramIndex])) { ConvertTo(il, paramTypes[paramIndex], curType); }
						break;
					case CodePartType.Text:
						il.Emit(OpCodes.Ldstr, p.Text);
						break;
					case CodePartType.Operator:
						// Si Type primitif (ce qui exclut Decimal), additionne directement, sinon appelle les fonctions de Decimal.
						// L'opérateur ^ ne fonctionne que pour les entiers, donc appelle Math.Pow même pour les types primitifs:
						switch (p.OpType)
						{
							case OpType.Add:
								if (primitive) { il.Emit(OpCodes.Add); }
								else { CalculationMethod(il, CalcMeth.AddDec, curType); }
								break;
							case OpType.Mul:
								if (primitive) { il.Emit(OpCodes.Mul); }
								else { CalculationMethod(il, CalcMeth.MulDec, curType); }
								break;
							case OpType.Sub:
								if (primitive) { il.Emit(OpCodes.Sub); }
								else { CalculationMethod(il, CalcMeth.SubDec, curType); }
								break;
							case OpType.Div: // Vérifie d'abord que ce n'est pas zéro.
								if (errorAction != ErrorAction.None) { NotZero(il, ilErrLoc, curType, returnType); }
								if (primitive) { il.Emit(OpCodes.Div); }
								else { CalculationMethod(il, CalcMeth.DivDec, curType); }
								break;
							case OpType.Pow:
								CalculationMethod(il, CalcMeth.Pow, curType);
								break;
						}
						break;
					case CodePartType.ParameterConversion:
						ConvertTo(il, curType, p.ConvertTo);
						break;
					case CodePartType.FuncCall:
								// Si la méthode est Sqrt, vérifie que le nombre n'est pas négatif:
								if (errorAction != ErrorAction.None && p.MethodToCall.Name == "Sqrt")
									{ NotNegative(il, ilErrLoc, p.MethodToCall.GetParameters()[0].ParameterType, returnType); }
						il.EmitCall(OpCodes.Call, p.MethodToCall, null);
						// Converti dans le type des autres nombres:
						if (_numTypes.Contains(p.MethodToCall.ReturnType)) { ConvertTo(il, p.MethodToCall.ReturnType, curType); }
						break;
					case CodePartType.ArrCreate:
						if (p.ArrayIndex >= arrayIndexes.Length) { Array.Resize(ref arrayIndexes, p.ArrayIndex+1); }
						if (arrayIndexes[p.ArrayIndex] == null) { arrayIndexes[p.ArrayIndex] = il.DeclareLocal(typeof(Array)); }
						il.Emit(OpCodes.Ldc_I4, p.ArrayLength);
						il.Emit(OpCodes.Newarr, p.ArrayElementType);
						il.Emit(OpCodes.Stloc, arrayIndexes[p.ArrayIndex]);
						il.Emit(OpCodes.Ldloc, arrayIndexes[p.ArrayIndex]);
						break;
					case CodePartType.ArrSetIndex:
						il.Emit(OpCodes.Ldc_I4, p.ArrayElementIndex);
						break;
					case CodePartType.ArrSaveElement:
						il.Emit(OpCodes.Stelem, p.ArrayElementType);
						il.Emit(OpCodes.Ldloc, arrayIndexes[p.ArrayIndex]);
						break;
				}
			}
			
			// On arrive à la fin du calcul: Reste à vérifier si la variable d'erreur contient 0 (alors pas d'erreur), ou
			// bien maxValue (alors il n'y a pas eu erreur):
			if (errorAction != ErrorAction.None)
			{
				// Etiquettes pour le bloc conditionnel:
				Label ilTRUE = il.DefineLabel();
				Label ilFALSE = il.DefineLabel();
				Label ilENDIF = il.DefineLabel();
				// Place la valeur de la variable sur la pile, ainsi que 0, et compare:
				il.Emit(OpCodes.Ldloc, ilErrLoc);
				if (!returnType.IsPrimitive) { il.Emit(OpCodes.Box, returnType); } // Box pour la fonction Equals
				WriteConstantNumber(il, "0", returnType);
				if (!returnType.IsPrimitive) { il.Emit(OpCodes.Box, returnType); } // Box pour la fonction Equals
				if (returnType.IsPrimitive) { il.Emit(OpCodes.Ceq); }
				else
					{ il.EmitCall(OpCodes.Call, typeof(Object).GetMethod("Equals", new Type[]{typeof(object),typeof(object)}), null); }
				il.Emit(OpCodes.Brtrue_S, ilTRUE);
				il.Emit(OpCodes.Br_S, ilFALSE);
				// Si la variable d'erreur est 0, converti la valeur courante dans le type du retour:
				il.MarkLabel(ilTRUE);
				ConvertTo(il, curType, returnType);
				il.Emit(OpCodes.Br_S, ilENDIF);
				// S'il y a eu erreur, ou bien place la variable d'erreur sur la pile, ou bien lève une exception:
				il.MarkLabel(ilFALSE);
				if (errorAction == ErrorAction.MaxValue) {
					il.Emit(OpCodes.Pop);
					il.Emit(OpCodes.Ldloc, ilErrLoc); }
				else {
					il.Emit(OpCodes.Newobj, typeof(ArithmeticException).GetConstructor(new Type[0]));
					il.Emit(OpCodes.Throw); }
				il.Emit(OpCodes.Br_S, ilENDIF);
				// Fin du if, et sort:
				il.MarkLabel(ilENDIF);
			}
			else
			{
				if (returnTypeIsNum) { ConvertTo(il, curType, returnType); }
			}
			
			// Retourne la valeur sur la pile:
			il.Emit(OpCodes.Ret);
			
			// Retourne un délégué:
			return dynMethod.CreateDelegate(delegType);
		
		}


		// ---------------------------------------------------------------------------
		
		
		// Methodes pour la conversion de type:
		private static MethodInfo __methChangeType_;
		private static MethodInfo __methGetTypeFromHandle_;
		// Dictionnaire des variables pour la conversion de type:
		private static Dictionary<Type,LocalBuilder> __locTypeConv_;

		
		/// <summary>
		/// Ecrit les codes MSIL pour convertir le dernier élément de la pile du type srcType vers le type destType.
		/// </summary>
		private static void ConvertTo(ILGenerator il, Type srcType, Type destType)
		{
			if (srcType == destType) { return; }
			// Box (place une référence d'objet) sur la pile, car ChangeType a pour premier argument un object:
			il.Emit(OpCodes.Box, srcType);
			// Place un token du type de destination sur la pile:
			il.Emit(OpCodes.Ldtoken, destType);
			// Récupère le handle du type:
			il.EmitCall(OpCodes.Call, __methGetTypeFromHandle_, null);
			// Appelle ChangeType sur les deux derniers éléments de la pile:
			il.EmitCall(OpCodes.Call, __methChangeType_, null);
			// On a obtenu un object, qu'on va unboxer et dont on va charger la valeur:
			// il.Emit(OpCodes.Unbox, destType);
			// il.Emit(OpCodes.Ldobj, destType); Ou en une seule instruction:
			il.Emit(OpCodes.Unbox_Any, destType);
		}


		/// <summary>
		/// Ecrit les codes MSIL pour convertir les deux derniers éléments de la pile du type srcType vers le type destType.
		/// </summary>
		private static void ConvertLastTwoValuesTo(ILGenerator il, Type srcType, Type destType)
		{
			if (srcType == destType) { return; }
			// Déclare une variable pour sortir la dernière valeur de la pile:
			LocalBuilder loc = DeclareTempLocal(il, srcType);
			il.Emit(OpCodes.Stloc, loc);
			// Convertit la valeur qui reste sur la pile (la précédente, donc):
			ConvertTo(il, srcType, destType);
			// Remet la valeur de la variable sur la pile, et la convertit à son tour:
			il.Emit(OpCodes.Ldloc, loc);
			ConvertTo(il, srcType, destType);
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Vérifie que la dernière valeur sur la pile n'est pas 0, quelque soit son type (numérique). Si elle vaut zéro, alors la variable ilErrLoc est rempli avec la MaxValue du returnType, et la dernière valeur sur la pile est remplacée par 1 pour que le calcul puisse continuer. En effet, on ne peut directement sortir, car la pile doit être vide (à l'exception de la valeur de retour)...
		/// </summary>
		private static void NotZero(ILGenerator il, LocalBuilder ilErrLoc, Type curType, Type returnType)
		{
			// Labels pour les conditions:
			Label ilTRUE = il.DefineLabel();
			Label ilFALSE = il.DefineLabel();
			Label ilENDIF = il.DefineLabel();
			// Sauve la valeur de la pile (à analyser) dans une variable:
			LocalBuilder ilSavedValue = DeclareTempLocal(il, curType);
			il.Emit(OpCodes.Stloc, ilSavedValue);
			il.Emit(OpCodes.Ldloc, ilSavedValue);
			if (!curType.IsPrimitive) { il.Emit(OpCodes.Box, curType); } // Box pour la fonction Equals
			// Compare à zéro:
			WriteConstantNumber(il, "0", curType);
			if (!curType.IsPrimitive) { il.Emit(OpCodes.Box, curType); } // Box pour la fonction Equals
			if (curType.IsPrimitive) { il.Emit(OpCodes.Ceq); }
			else
				{ il.EmitCall(OpCodes.Call, typeof(Object).GetMethod("Equals", new Type[]{typeof(object),typeof(object)}), null); }
			il.Emit(OpCodes.Brtrue_S, ilTRUE);
			il.Emit(OpCodes.Br_S, ilFALSE);
			// Si erreur, charge MaxValue dans la variable d'erreur, puis remplace le 0 comparé par un 1:
			il.MarkLabel(ilTRUE);
			WriteConstantNumber(il, null, returnType);
			il.Emit(OpCodes.Stloc, ilErrLoc);
			WriteConstantNumber(il, "1", curType);
			il.Emit(OpCodes.Br_S, ilENDIF);
			// Sinon, continue en réinscrivant l'élément comparé:
			il.MarkLabel(ilFALSE);
			il.Emit(OpCodes.Ldloc, ilSavedValue);
			il.Emit(OpCodes.Br_S, ilENDIF);
			// Fin du if:
			il.MarkLabel(ilENDIF);
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Vérifie que la dernière valeur sur la pile n'est pas strictement négative, quelque soit son type (numérique). Si elle strictement négative, alors la variable ilErrLoc est rempli avec la MaxValue du returnType, et la dernière valeur sur la pile est remplacée par 0 pour que le calcul puisse continuer. En effet, on ne peut directement sortir, car la pile doit être vide (à l'exception de la valeur de retour)...
		/// </summary>
		private static void NotNegative(ILGenerator il, LocalBuilder ilErrLoc, Type curType, Type returnType)
		{
			// Labels pour les conditions:
			Label ilTRUE = il.DefineLabel();
			Label ilFALSE = il.DefineLabel();
			Label ilENDIF = il.DefineLabel();
			// Sauve la valeur de la pile (à analyser) dans une variable:
			LocalBuilder ilSavedValue = DeclareTempLocal(il, curType);
			il.Emit(OpCodes.Stloc, ilSavedValue);
			il.Emit(OpCodes.Ldloc, ilSavedValue);
			// Compare à zéro:
			WriteConstantNumber(il, "0", curType);
			if (curType.IsPrimitive) { il.Emit(OpCodes.Clt); }
			else
			{
				il.EmitCall(OpCodes.Call, curType.GetMethod("Compare", new Type[]{curType,curType}), null);
				il.Emit(OpCodes.Ldc_I4, 0);
				il.Emit(OpCodes.Clt);
			}
			il.Emit(OpCodes.Brtrue_S, ilTRUE);
			il.Emit(OpCodes.Br_S, ilFALSE);
			// Si erreur, charge MaxValue dans la variable d'erreur, puis remplace le 0 comparé par un 1:
			il.MarkLabel(ilTRUE);
			WriteConstantNumber(il, null, returnType);
			il.Emit(OpCodes.Stloc, ilErrLoc);
			WriteConstantNumber(il, "0", curType);
			il.Emit(OpCodes.Br_S, ilENDIF);
			// Sinon, continue en réinscrivant l'élément comparé:
			il.MarkLabel(ilFALSE);
			il.Emit(OpCodes.Ldloc, ilSavedValue);
			il.Emit(OpCodes.Br_S, ilENDIF);
			// Fin du if:
			il.MarkLabel(ilENDIF);
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Ecrit dans il le MSIL correspondant à la valeur numérique parser dans le type correspondant. Si nb est null, alors écrit la MaxValue pour le type correspondant:
		/// </summary>
		private static void WriteConstantNumber(ILGenerator il, string nb, Type type)
		{
			// Ecrit les valeurs:
			if (type == typeof(Int32))
				{ il.Emit(OpCodes.Ldc_I4, (nb==null ? Int32.MaxValue : Int32.Parse(nb))); }
			else if (type == typeof(Int64))
				{ il.Emit(OpCodes.Ldc_I8, (nb==null ? Int64.MaxValue : Int64.Parse(nb))); }
			else if (type == typeof(Single))
				{ il.Emit(OpCodes.Ldc_R4, (nb==null ? Single.MaxValue : Single.Parse(nb))); }
			else if (type == typeof(Double))
				{ il.Emit(OpCodes.Ldc_R8, (nb==null ? Double.MaxValue : Double.Parse(nb))); }
			/*// On récupère d'abord le tableau d'entiers int[] avec Decimal.GetBits, et on inscrit chaque
			// entier sur la pile dans un tableau, puis on appelle le constructeur
			// Decimal(int[] bits) qui a été défini plus haut dans la variable __DecimalCtor.
			// --- Mais ça ne semble pas marcher pour, par exemple, Decimal.MaxValue... ---
			else if (type == typeof(Decimal))
			{
				// Déclare une variable int[] pour l'écriture d'un nombre décimal:
				LocalBuilder loc = null; int[] decimalBits;
				if (type == typeof(Decimal))
				{
					if (__locTypeConv_.ContainsKey(typeof(int[]))) { loc = __locTypeConv_[typeof(int[])]; }
					else { loc = il.DeclareLocal(typeof(int[])); __locTypeConv_.Add(typeof(int[]), loc); }
				}
				decimalBits = Decimal.GetBits((nb==null ? Decimal.MaxValue : Decimal.Parse(nb))); // Le tableau de bits
				//il.DeclareLocal(typeof(int[])); // Déclare une variable pour le tableau (on l'a fait précédemment)
				il.Emit(OpCodes.Ldc_I4, 4); // Charge le nombre d'éléments du tableau sur la pile
				il.Emit(OpCodes.Newarr, typeof(Int32)); // Créer un nouveau tableau
				il.Emit(OpCodes.Stloc, loc); // Stocke la référence au tableau dans la variable
				for (int i=0; i<4; i++) // Pour chaque bit
				{
					il.Emit(OpCodes.Ldloc, loc); // Charge le tableau (la variable) sur la pile
					il.Emit(OpCodes.Ldc_I4, i); // Charge l'index à modifier sur la pile
					il.Emit(OpCodes.Ldc_I4, decimalBits[i]); // La valeur à stocker dans le tableau
					il.Emit(OpCodes.Stelem_I4); // Enregistre dans le tableau (la référence au tableau à alors
				}														// disparue, c'est pourquoi il faut le rappeler)
				il.Emit(OpCodes.Ldloc, loc); // Rappelle la variable du tableau sur la pile
				il.Emit(OpCodes.Newobj, __DecimalCtor); // Appelle le constructeur de Decimal(int[])
			}*/
			else
			{
				if (nb == null) { nb = type.GetField("MaxValue").GetValue(null).ToString(); }
				// On tente d'abord de parser, pour ne pas noter une valeur qui ne pourrait pas être parser
				// dans la méthode dynamique (ce qui provoque une erreur irrécupérable):
				//object temp = type.GetMethod("Parse", new Type[]{typeof(String)}).Invoke(null, new object[]{nb});
				// Si pas d'exception (auquel cas la procédure s'arrête), on continue:
				il.Emit(OpCodes.Ldstr, nb);
				il.EmitCall(OpCodes.Call, type.GetMethod("Parse", new Type[]{typeof(String)}), null);
			}
		}
	

		// ---------------------------------------------------------------------------
		

		// Méthode de calcul:
		private static MethodInfo __methPowDouble_;
		private static MethodInfo __methAddDecimal_;
		private static MethodInfo __methSubtractDecimal_;
		private static MethodInfo __methDivideDecimal_;
		private static MethodInfo __methMultiplyDecimal_;
		
		
		/// <summary>
		/// Utilise les méthodes de calcul correspondant à l'énumération meth. Les arguments sont d'abord convertis depuis curType vers le type nécessaire à l'appelle des fonctions, puis le résultat est converti à son tour vers curType.
		/// </summary>
		private static void CalculationMethod(ILGenerator il, CalcMeth meth, Type curType)
		{
			switch (meth)
			{
				case CalcMeth.Pow:
					ConvertLastTwoValuesTo(il, curType, typeof(Double));
					il.EmitCall(OpCodes.Call, __methPowDouble_, null);
					ConvertTo(il, typeof(Double), curType);
					break;
				case CalcMeth.AddDec:
					ConvertLastTwoValuesTo(il, curType, typeof(Decimal));
					il.EmitCall(OpCodes.Call, __methAddDecimal_, null);
					ConvertTo(il, typeof(Decimal), curType);
					break;
				case CalcMeth.SubDec:
					ConvertLastTwoValuesTo(il, curType, typeof(Decimal));
					il.EmitCall(OpCodes.Call, __methSubtractDecimal_, null);
					ConvertTo(il, typeof(Decimal), curType);
					break;
				case CalcMeth.MulDec:
					ConvertLastTwoValuesTo(il, curType, typeof(Decimal));
					il.EmitCall(OpCodes.Call, __methMultiplyDecimal_, null);
					ConvertTo(il, typeof(Decimal), curType);
					break;
				case CalcMeth.DivDec:
					ConvertLastTwoValuesTo(il, curType, typeof(Decimal));
					il.EmitCall(OpCodes.Call, __methDivideDecimal_, null);
					ConvertTo(il, typeof(Decimal), curType);
					break;
			}
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Gère le dictinnaire __locTypeConv_ qui contient des variables temporaires et réutilisables. Cette procédure cherche une variable de type type dans le dictinnaire, et si elle en trouve une, elle l'a renvoie pour utilisation. Sinon, elle en stocke une nouvelle dans le dictionnaire en la déclarant dans il, puis retourne sa référence.
		/// </summary>
		private static LocalBuilder DeclareTempLocal(ILGenerator il, Type type)
		{
			if (!__locTypeConv_.ContainsKey(type)) { __locTypeConv_.Add(type, il.DeclareLocal(type)); }	
			return __locTypeConv_[type];	
		}



		#endregion METHODES PUBLIQUES






		// ---------------------------------------------------------------------------
		// METHODES PRIVEES
		// ---------------------------------------------------------------------------




		#region METHODES PRIVEES



		/// <summary>
		/// Obtient le type de paramètre de meth à l'index spécifié (numéro du paramètre).
		/// </summary>
		/*private static Type GetParamType(MethodInfo meth, int index, out bool isParams)
		{
			ParameterInfo[] pars = meth.GetParameters();
			isParams = false;
			// Si le dernier élément du tableau est un tableau, on considère que c'est un params:
			if (pars[pars.Length-1].ParameterType.IsArray)
			{
				// Si on est au dernier élément où si on l'a dépassé, on indique le type de tableau,
				// sinon on continue normalement:
				if (index >= pars.Length-1)
				{
					isParams = true;
					return Type.GetType(pars[pars.Length-1].ParameterType.AssemblyQualifiedName.Replace("[]", ""));
				}
			}
			// Sinon on continue normalement:
			if (index >= pars.Length)
				{ throw new Exception(String.Format("The method \"{0}\" must have {1} parameter(s).", meth.Name, pars.Length)); }
			return pars[index].ParameterType;
		}*/


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Obtient le MethodInfo correspondant à la méthode de methods qui a pour nom name. prefType indique le type préféré pour le choix des surcharges. La procédure cherche la méthode qui a le plus de paramètres correspondant à prefType. S'il n'y a rien qui correspond, elle prend le premier élément du tableau correspondant au nom passé. C'est pourquoi il est important de mettre dans le tableau les surcharges aux types de paramètres les plus généraux en premier (e.g. une surcharge avec un paramètre double avant une surcharge avec un paramètre int). Lève une exception si aucune méthode ne s'appelle name.
		/// </summary>
		/*private static MethodInfo GetMethod(string name, MethodInfo[] methods, Type prefType)
		{
			// Obtient un tableau correspondant aux méthodes nommées:
			MethodInfo[] meths = methods.Where(delegate(MethodInfo mi) { return mi.Name.Equals(name); }).ToArray();
			if (meths.Length == 0) { throw new Exception(String.Format("The method \"{0}\" doesn't exist!", name)); }
			// Cherche la méthode qui a le plus de paramètres correspondant à prefType. S'il n'y a rien qui correspond,
			// prend le premier élément du tableau correspondant au nom passé. C'est pourquoi il est important de 
			// mettre dans le tableau les fonctions aux types de paramètres les plus généraux en premier.
			MethodInfo meth = meths[0];; int c = 0, last = 0;
			foreach (MethodInfo mi in meths)
			{
				c = 0; if (mi.GetParameters() == null) { continue; }
				foreach (ParameterInfo p in mi.GetParameters()) { if (p.ParameterType == prefType) { c++; } }
				if (c > last) { meth = mi; last = c; }
			}
			return meth;
		}*/
		
		
		
		private static FormPart GetMethod(string name, MethodInfo[] methods, FormPart funcPart, Type curType, ref int paramsArrayIndex)
		{
			// Obtient un tableau correspondant à la méthode nommée:
			MethodInfo[] meths = methods.Where(delegate(MethodInfo mi) { return mi.Name.Equals(name); }).ToArray();
			if (meths.Length == 0) { throw new Exception(String.Format("The method \"{0}\" doesn't exist!", name)); }
			// Cherche une méthode correspondant aux paramètres de funcPart:
			int lenParamsPart = funcPart.Children.Length;
			foreach (MethodInfo mi in meths) // Parcourt toutes les méthodes name
			{
				// Flags:
				bool flagIsOk, isParams = false; int paramsArrInd = paramsArrayIndex;
				// Construit le tableau des types de params de la méthode en cours d'étude:
				ParameterInfo[] pis = mi.GetParameters();
				int lenParamsMeth = pis.Length;
				// Si pas de paramètre de chaque côté, on retourne la fonction:
				if (lenParamsMeth == 0 && funcPart.Children[0].Children[0].Type == FormPartType.Empty)
					{ funcPart.Method = mi; return funcPart; }
				Type[] paramTypes = new Type[lenParamsMeth];
				for (int i=0; i<lenParamsMeth; i++) { paramTypes[i] = pis[i].ParameterType; }
				// Si le nombre de paramètres de funcPart est inférieur à celui des paramètres de la méthode en cours,
				// on passe directement à la suite:
				if (lenParamsPart < lenParamsMeth) { continue; } 
				// Si le dernier paramètre est un tableau, on le considère comme "params", et on redimmensionne les tableaux
				// avant de compléter celui des types avec le type du tableau:
				if (paramTypes[lenParamsMeth-1].IsArray)
				{
					isParams = true;
					paramsArrInd = paramsArrayIndex + 1; // Augmente le compteur d'index
					funcPart.ParamsArrayIndex = paramsArrInd;
					Type arrType = Type.GetType(paramTypes[lenParamsMeth-1].AssemblyQualifiedName.Replace("[]", ""));
					Array.Resize(ref paramTypes, lenParamsPart);
					for (int i=lenParamsMeth-1; i<lenParamsPart; i++) { paramTypes[i] = arrType; }
				}
				flagIsOk = true;
				for (int i=0; i<lenParamsPart; i++) // Pour tous les FormPart d'arguments
				{
					// Si le type du param de la méthode correspond avec le type du FormPart, alors inscrit le type dans le
					// FormPart de paramètre, et passe aux paramètres suivants:
					FormPart sigPart = funcPart.Children[i].Children[0]; // Cherche la partie significative du paramètre (celle qui contient 
					while (sigPart.Type == FormPartType.Bracket) { sigPart = sigPart.Children[0]; } // ...un type de données)
					// Si nombre et type numérique:
					bool match = (sigPart.Type == FormPartType.Number && _numTypes.Contains(paramTypes[i]));
					// Si texte et type String
					match = match || (sigPart.Type == FormPartType.Text && paramTypes[i] == typeof(String));
					// Si variable et curType et paramTypes[i] sont numériques ou bien paramTypes[i] et curType sont les mêmes:
					match = match || (sigPart.Type == FormPartType.Variable 
						&& (_numTypes.Contains(curType) && _numTypes.Contains(paramTypes[i])
							|| ((curType.IsSubclassOf(paramTypes[i])) || curType == paramTypes[i])));
					// Si méthode et retType et paramTypes[i] sont numériques ou bien paramTypes[i] et retType sont les mêmes:
					match = match || (sigPart.Type == FormPartType.Method 
						&& (_numTypes.Contains(sigPart.Method.ReturnType) && _numTypes.Contains(paramTypes[i])
							|| ((sigPart.Method.ReturnType.IsSubclassOf(paramTypes[i])) || sigPart.Method.ReturnType == paramTypes[i])));
					// Si object, tout est accepté:
					if (paramTypes[i] == typeof(Object)) { match = true; }
					// Si on a trouvé une correspondance:
					if (match)
					{
						// Complète le type du paramètre:
						funcPart.Children[i].ParameterType = paramTypes[i];
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
					return funcPart;
				}
			}
			// Si on arrive là, c'est qu'on a rien trouvé:
			throw new Exception(String.Format("The method \"{0}\" has not correct parameters.", name));
		
		}
		

		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Découpe la formule en différentes parties, chacune représentant un niveau de parenthèse ou de fonction. Tout part d'un FormPart de type Bracket initial, qui représente l'ensemble de la formule, et qui ne contient rien sinon un tableau d'enfants. Chaque enfant représente un même niveau de parenthèse ou de fonction, et ainsi de suite. De manière général, tous les frères (tous les éléments du tableau Children d'un même FormPart) sont sur le même niveau. A chaque parenthèse, on augmente d'un niveau. Les fonctions représentent un niveau qui ne contient rien sinon des enfants, qui représentent eux-mêmes chacun un argument. Chaque argument est un FormPart qui ne contient rien sinon un tableau d'enfants, représentant les termes de la formule de l'argument, et ces enfants peuvent à leur tour contenir des petits-enfants, représentant chacun un niveau supplémentaire de parenthèses ou de fonction, et ainsi de suite.
		/// </summary>
		private static FormPart SplitFormula(string formula, string[] variables, MethodInfo[] usedMethods, Type curType)
		{
		
			// Compte le nombre de parenthèse ouvrante et fermante, et lève une exception si ça ne correspond pas:
			int brCounter = 0, l = formula.Length; string ch;
			for (int i=0; i<l; i++)
			{
				ch = formula.Substring(i, 1);
				if (ch.Equals("(")) { brCounter++; } if (ch.Equals(")")) { brCounter--; }
			}
			if (brCounter != 0) { throw new Exception(String.Format("Formula \"{0}\" has a syntax error!", formula)); }
			
			// Fonction anonyme pour déterminer si une partie est un nombre, une variable ou un texte:
			MethodInfo methParse = curType.GetMethod("Parse", new Type[]{typeof(String)});
			Action<FormPart> setIsVar =
				delegate(FormPart part)
				{
					if (part.Type == FormPartType.None)
					{
						if (String.IsNullOrEmpty(part.Text)) { part.Type = FormPartType.Empty; return; }
						if (variables.Contains(part.Text)) { part.Type = FormPartType.Variable; return; }
						try { methParse.Invoke(null, new object[]{part.Text}); part.Type = FormPartType.Number; }
						catch { part.Type = FormPartType.Text; }
					}
				};
			
			// Partie racine:
			FormPart curPart = new FormPart(null, null);
			curPart.Type = FormPartType.Bracket;
			curPart.AddChild(curPart = new FormPart(null, curPart));
			
			// Parcourt les caractères et enregistre les éléments dans curPart, en créant pour chaque
			// nouvelle partie des enfants:
			int paramNb; string methName, text;
			int paramsArrayIndex = -1;
			for (int i=0; i<l; i++)
			{
				ch = formula.Substring(i, 1);
				// Si guillemet, on considère l'intérieur des guillemets comme une chaîne:
				if (ch == "\"")
				{
					int endQuote = formula.IndexOf("\"", i + 1);
					while (endQuote != -1 && (endQuote != l-1 && formula.Substring(endQuote+1, 1) == "\""))
						{ endQuote = formula.IndexOf("\"", endQuote + 2); } // Boucle pour les guillemets à l'intérieur des guillemets
					if (endQuote == -1)
						{ throw new Exception(String.Format("Formula \"{0}\" has a syntax error!", formula)); }
					// Enregistre le texte en supprimant l'échappement des caractères:
					curPart.Text = formula.Substring(i + 1, endQuote - i - 1).Replace("\"\"", "\"");
					i = endQuote; continue;
				}
				// Si espace, on saute:
				if (ch == " ") { continue; }
				// Si une parenthèse ouvrante suit une parenthèse fermante, on rajoute * (eg. ")("=")*("):
				if (i < l-1 && ch == ")" && formula.Substring(i+1, 1) == "(")
				{
					formula = formula.Substring(0, i+1) + "*" + formula.Substring(i+1);
					l++;
				}
				// Puis on étudie les autres caractères significatifs:
				if (("+*/^$".Contains(ch))
					|| ((ch == "-") && (i > 0) && ((")0123456789".Contains(formula.Substring(i-1, 1)))
							|| variables.Contains(curPart.Text))))
				{
					// Clot la partie courante:
					setIsVar(curPart);
					// Nouvelle partie, soeur de la courante:
					curPart.AddSibling(curPart = new FormPart(ch, curPart.Parent));
				}
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
				else if (ch == ")")
				{
					// Clot la partie courante, et remonte d'un niveau:
					setIsVar(curPart);
					text = curPart.Text;
					curPart = curPart.Parent;
					// Si fonction, on est dans un paramètre, donc on remonte encore d'un niveau, et
					// on vérifie qu'il y a bien le nombre de paramètre requis:
					if (curPart.Type == FormPartType.MethParam)
					{
						paramNb = curPart.ParameterNb;
						curPart = curPart.Parent;
						// Obtient l'analyse de la fonction et sa meilleure surcharge en fonction des types de FormPart trouvés:
						curPart = GetMethod(curPart.Text, usedMethods, curPart, curType, ref paramsArrayIndex);
					}
				}
				else if (ch == ",")
				{
					// Clot la partie courante:
					setIsVar(curPart);
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
				else if (ch == "π")
				{
					curPart.Text = Convert.ChangeType(Math.PI, curType).ToString();
				}
				else
				{
					curPart.Text += ch;
				}
			}
			
			// Sortie: Une fois arrivée ici, la parent de curPart doit être null. Sinon, exception:
			setIsVar(curPart);
			if (curPart.Parent.Parent != null) { throw new Exception(String.Format("Formula \"{0}\" has a syntax error!", formula)); }
			return curPart.Parent;

		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Ajoute à _codeParts la partie passé en argument, en gérant le redimmensionnement du tableau.
		/// </summary>
		private static void AddCodePart(CodePart part)
		{
			if (_codeParts == null) { _codeParts = new CodePart[20]; }
			if (_index >= _codeParts.Length - 1) { Array.Resize(ref _codeParts, _index + 20); }
			_codeParts[++_index] = part;
		}
		
		
		/// <summary>
		/// Ajoute à _codeParts tous les opérateurs de ops supérieur ou égaux à priority, dans l'ordre inverse.
		/// </summary>
		private static void AddOperatorsToCodeParts(OpType[] ops, int priority, int c)
		{
			for (int i=c-1; i>=0; i--)
			{
				if ((ops[i] != OpType.None) && (_priorities[ops[i]] >= priority))
					{ AddCodePart(new CodePart(ops[i])); ops[i] = OpType.None; }
			}
		}


		// ---------------------------------------------------------------------------
	
		
		
		/// <summary>
		/// Remplit le tableau _codeParts à partir d'un FormPart. La fonction est récursive, et s'appelle à chaque nouveau niveau de FormPart (propriété Children).
		/// </summary>
		private static void BuildCode(FormPart formPart)
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
				AddOperatorsToCodeParts(ops, f.OperatorPriority, c);
				// Engage une action en fonction du type de la partie en cours:
				switch (f.Type)
				{
					case FormPartType.Method:
						BuildCode(f); // Pour tous les paramètres
						AddCodePart(new CodePart(f.Method)); // Appelle de fonction
						break;
					case FormPartType.MethParam:
						// Si le paramètre est le premier params: construit un tableau:
						if (f.ParamsElementIndex == 0) {
							AddCodePart(new CodePart(f.ParamsArrayIndex,
								f.Parent.Children[f.Parent.Children.Length-1].ParamsElementIndex + 1, f.ParameterType)); }
						// Si le paramètre est un params, on ajoute un index au tableau:
						if (f.ParamsElementIndex > -1) {
							AddCodePart(new CodePart(f.ParamsArrayIndex, f.ParamsElementIndex)); }
						// Pour les termes du paramètres
						BuildCode(f);
						// Conversion de paramètre s'il y en a un, et si numérique:
						if (f.ParameterType != null && _numTypes.Contains(f.ParameterType)) 
							{ AddCodePart(new CodePart(f.ParameterType)); }
						// Si le paramètre est un params, on enregistre l'élément du tableau:
						if (f.ParamsElementIndex > -1) {
							AddCodePart(new CodePart(f.ParamsArrayIndex, f.ParameterType)); }
						break;
					case FormPartType.Number:
						if (f.Type != FormPartType.Empty)
							{ AddCodePart(new CodePart(f.Text, CodePartType.Number)); }
						break;
					case FormPartType.Variable:
						if (f.Type != FormPartType.Empty)
							{ AddCodePart(new CodePart(f.Text, CodePartType.Variable)); }
						break;
					case FormPartType.Text:
						// Pour que ce soit un texte, il faut le parent soit un paramètre de fonction String, et qu'il n'y
						// ait pas d'opération (donc qu'il n'y ait pas de frère et soeur), sinon lève exception:
						if (f.Parent.Type != FormPartType.MethParam || f.Parent.Children.Length != 1
								|| (f.Parent.ParameterType != typeof(String) && f.Parent.ParameterType != typeof(Object)))
							{ throw new Exception(String.Format("\"{0}\" is not a valid fonction, number or text.", f.Text)); }
						if (f.Type != FormPartType.Empty)
							{ AddCodePart(new CodePart(f.Text, CodePartType.Text)); }
						break;
					case FormPartType.Bracket:
						BuildCode(f); // Pour les termes de la parenthèse
						break;
					case FormPartType.None:
						throw new Exception(String.Format("Formula has a syntax error!"));
				}
				// Continue si pas d'opérateur:
				if (f.PreviousOperator == OpType.None) { continue; }
				// Si on est à la fin, ou si l'opérateur suivant est de niveau supérieur ou égal au précédent, on place l'opérateur sur la pile. 
				if ((i == l - 1) || (f.OperatorPriority >= formPart.Children[i+1].OperatorPriority))
					{ AddCodePart(new CodePart(f.PreviousOperator)); }
				// Sinon, ajoute l'opérateur à la pile d'attente:
				else { ops[c++] = f.PreviousOperator; }
			}
			// A la fin, vide tous les opérateurs:
			AddOperatorsToCodeParts(ops, 0, c);

		}
	



		#endregion METHODES PRIVEES
	
	
	
	}
	
	
	
}
