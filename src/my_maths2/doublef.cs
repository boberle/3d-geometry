using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Globalization;

namespace System
{




	/// <summary>
	/// Représente un nombre qui peut être définit par une valeur numérique ou par une formule mathématique.
	/// </summary>
	public struct DoubleF
	{






		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------



		#region DECLARATIONS


		// Propriétés:
		private bool _isFormula;
		
		// Valeurs:
		private string _formula;
		private Func<double> _method;
		private double _value;
		
		// Champ publiques:
		public static readonly DoubleF Zero;
		public static readonly DoubleF Pi;
		public static readonly DoubleF TwoPi;


		#endregion DECLARATIONS






		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES
				
		
		
		/// <summary>
		/// Obtient ou définit les méthodes qu'il est possible d'utiliser dans les formules. Statique.
		/// </summary>
		public static MethodInfo[] UsedMethods { get; set; }


		/// <summary>
		/// Obtient ou définit le type d'appartenance des méthodes dynamiques crées avec les formules. Ce type doit voir l'ensembe des méthodes de usedMethods.
		/// </summary>
		public static Type OwnerType { get; set; }


		/// <summary>
		/// Obtient si la valeur stockée est une formule (true), ou un nombre (false).
		/// </summary>
		public bool IsFormula { get { return _isFormula; } }


		/// <summary>
		/// Obtient la valeur numérique stockée lors du dernier calcul de la formule (ne recalcule pas). S'il n'y a pas de formule, retourne simplement la valeur. Définit la valeur numérique seulement s'il n'y a pas de formule (s'il y en a une, ne change pas la valeur).
		/// </summary>
		public double Value
		{
			get { return _value; }
			set { if (!_isFormula) _value = value; }
		}

		/// <summary>
		/// Obtient le texte de la formule, ou null s'il n'y a pas de formule.
		/// </summary>
		public string Formula { get { return _formula; } }
		
		/// <summary>
		/// Obtient si la valeur n'est pas un nombre par exemple à cause d'une erreur de calcul. En fait, retourne true si Value est NaN ou Infinity.
		/// </summary>
		public bool IsNaN {
			get { return (Double.IsNaN(_value) || Double.IsInfinity(_value)); } }
		



		#endregion PROPRIETES






		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS



		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static DoubleF()
		{
			DoubleF.UsedMethods = new MethodInfo[0];
			DoubleF.OwnerType = typeof(DoubleF);
			Zero = new DoubleF();
			Pi = new DoubleF(Math.PI);
			TwoPi = new DoubleF(Math.PI * 2.0);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Initialise avec s, qui peut être une nombre ou une formule. En cas d'erreur, lève une exception.
		/// </summary>
		public DoubleF(string s) : this()
		{
			Alter(s);
		}

		/// <summary>
		/// Initialise avec n.
		/// </summary>
		public DoubleF(double n) : this()
		{
			Alter(n);
		}

		/// <summary>
		/// Initialise avec n.
		/// </summary>
		public DoubleF(decimal n) : this()
		{
			Alter(n);
		}

		/// <summary>
		/// Initialise avec n.
		/// </summary>
		public DoubleF(int n) : this()
		{
			Alter(n);
		}



		#endregion CONSTRUCTEURS






		// ---------------------------------------------------------------------------
		// SURCHARGE D'OPERATEURS
		// ---------------------------------------------------------------------------




		#region SURCHARGE D'OPERATEURS



		/// <summary>
		/// Surcharge d'opérateur ++. Ajoute 1 au nombre, seulement si ce n'est pas une formule. Retourne la même instance de n.
		/// </summary>
		public static DoubleF operator ++(DoubleF n)
			{ if (!n._isFormula) { n._value++; } return n; }


		/// <summary>
		/// Surcharge d'opérateur --. Enlève 1 au nombre, seulement si ce n'est pas une formule. Retourne la même instance de n.
		/// </summary>
		public static DoubleF operator --(DoubleF n)
			{ if (!n._isFormula) { n._value++; } return n; }


		/// <summary>
		/// Surcharge d'opérateur +. Additionne les deux valeurs, et retourne une nouvelle instance.
		/// </summary>
		public static DoubleF operator +(DoubleF n1, DoubleF n2)
			{ return new DoubleF(n1._value + n2._value); }

		/// <summary>
		/// Surcharge d'opérateur +. Additionne les deux valeurs, et retourne une nouvelle instance.
		/// </summary>
		public static DoubleF operator +(DoubleF n1, double n2)
			{ return new DoubleF(n1._value + n2); }


		/// <summary>
		/// Surcharge d'opérateur -. Soustrait les deux valeurs, et retourne une nouvelle instance.
		/// </summary>
		public static DoubleF operator -(DoubleF n1, DoubleF n2)
			{ return new DoubleF(n1._value - n2._value); }

		/// <summary>
		/// Surcharge d'opérateur -. Soustrait les deux valeurs, et retourne une nouvelle instance.
		/// </summary>
		public static DoubleF operator -(DoubleF n1, double n2)
			{ return new DoubleF(n1._value - n2); }


		/// <summary>
		/// Surcharge d'opérateur *. Multiplie les deux valeurs, et retourne une nouvelle instance.
		/// </summary>
		public static DoubleF operator *(DoubleF n1, DoubleF n2)
			{ return new DoubleF(n1._value * n2._value); }

		/// <summary>
		/// Surcharge d'opérateur *. Multiplie les deux valeurs, et retourne une nouvelle instance.
		/// </summary>
		public static DoubleF operator *(DoubleF n1, double n2)
			{ return new DoubleF(n1._value * n2); }


		/// <summary>
		/// Surcharge d'opérateur /. Divise les deux valeurs, et retourne une nouvelle instance.
		/// </summary>
		public static DoubleF operator /(DoubleF n1, DoubleF n2)
			{ return new DoubleF(n1._value / n2._value); }

		/// <summary>
		/// Surcharge d'opérateur /. Divise les deux valeurs, et retourne une nouvelle instance.
		/// </summary>
		public static DoubleF operator /(DoubleF n1, double n2)
			{ return new DoubleF(n1._value / n2); }

		/// <summary>
		/// Surcharge d'opérateur ==.
		/// </summary>
		public static bool operator ==(DoubleF n1, DoubleF n2)
			{ return n1._value == n2._value; }

		/// <summary>
		/// Surcharge d'opérateur !=.
		/// </summary>
		public static bool operator !=(DoubleF n1, DoubleF n2)
			{ return n1._value != n2._value; }

		/// <summary>
		/// Surcharge de la méthode Equals.
		/// </summary>
		public override bool Equals(object obj)
			{ return (obj is DoubleF && ((DoubleF)obj)._value == _value); }

		/// <summary>
		/// Surcharge de la méthode GetHashCode.
		/// </summary>
		public override int GetHashCode()
			{ return _value.GetHashCode(); }
		

		#endregion SURCHARGE D'OPERATEURS






		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES




		/// <summary>
		/// Tente une conversion d'une chaîne. s peut être une formule ou une valeur numérique.
		/// </summary>
		public static bool TryParse(string s, out DoubleF nb)
		{
			return DoubleF.TryParse(s, out nb, false);
		}
		
		
		/// <summary>
		/// Tente une conversion d'une chaîne. s peut être une formule ou une valeur numérique.
		/// </summary>
		private static bool TryParse(string s, out DoubleF nb, bool showError)
		{
			try { nb = DoubleF.Parse(s); return true; }
			catch (Exception exc) { if (showError) { My.ErrorHandler.ShowError(exc); } nb = new DoubleF(); return false; }
		}
		
		
		/// <summary>
		/// Tente une conversion d'une chaîne. s peut être une formule ou une valeur numérique. Si erreur, lève une exception.
		/// </summary>
		public static DoubleF Parse(string s)
		{
			// Tente une conversion de type, et si ça marche, enregistre le nouveau nombre:
			double n; DoubleF nb = new DoubleF();
			if (Double.TryParse(s, out n))
			{
				nb._value = n;
				nb._isFormula = false;
				nb._formula = null;
				nb._method = null;
				return nb;
			}
			// Sinon, on créer une méthode dynamique:
			else
			{
				// Si la formule commence par un $, on calcule la fonction et on enregistre la valeur de départ
				// mais sans marquer le DoubleF comme une fonction:
				bool isStartValue = s.StartsWith("$");
				if (isStartValue) { s = s.Substring(1); }
				// Créer la méthode et l'exécute. Si on a une FormulaException, ou une SystemException (comme
				// InvalidProgramException, par exemple), on relève l'exception. Si on a une autre execption,
				// on valide la formule, même si le nombre est indéfini:
				try {
					nb._method = (Func<double>)My.Formula.CreateFormulaMethod(s, null, typeof(Func<double>),
						DoubleF.OwnerType, null, My.FormulaWorkingType.Double, DoubleF.UsedMethods, null);
					nb._value = nb._method(); }
				catch (My.FormulaException exc) { throw exc; }
				catch (SystemException exc) { throw exc; }
				catch { ; }
				if (isStartValue) { nb._method = null; }
				nb._isFormula = !isStartValue;
				nb._formula = (isStartValue ? null : s);
				return nb;
			}
		}
		
		
		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Examine tous les DoubleF passés en argument, et retourne true si l'un d'autre eux est NaN ou Infinity, false sinon.
		/// </summary>
		public static bool IsThereNan(params DoubleF[] nbs)
		{
			foreach (DoubleF n in nbs) { if (Double.IsNaN(n._value) || Double.IsInfinity(n._value)) { return true; } }
			return false;
		}
		
		/// <summary>
		/// Examine tous les Double passés en argument, et retourne true si l'un d'autre eux est NaN ou Infinity, false sinon.
		/// </summary>
		public static bool IsThereNan(params double[] nbs)
		{
			foreach (double n in nbs) { if (Double.IsNaN(n) || Double.IsInfinity(n)) { return true; } }
			return false;
		}
		
		
		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// S'il y a une formule, recalcule et retourne le résultat, en l'enregistrant dans Value. Sinon, retourne simplement Value. Si throwExc est vrai, lève une exception en cas d'exception lors de l'exécution de la méthode dynamique. Sinon, affiche l'erreur et met Value à NaN.
		/// </summary>
		public double Recalculate(bool throwExc)
		{
			if (!_isFormula) { return _value; }
			try { _value = _method(); }
			catch (Exception exc)	{
				if (throwExc) { throw exc; } else { My.ErrorHandler.ShowError(exc); }
				_value = Double.NaN; }
			return _value;
		}
		
		/// <summary>
		/// Voir surcharge. throwExc vaut ici false.
		/// </summary>
		public double Recalculate()
			{ return Recalculate(false); }


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Enregistre la valeur et supprime toute formule et fonction. Si s est une valeur numérique, alors Value prend la valeur de s. Sinon, tente d'exécuter la formule. En cas d'erreur, lève une exception. L'objet n'est alors pas modifier.
		/// </summary>
		public double Alter(string s)
		{
			DoubleF n = DoubleF.Parse(s);
			_value = n._value;
			_isFormula = n._isFormula;
			_formula = n._formula;
			_method = n._method;
			return _value;
		}

		/// <summary>
		/// Enregistre la valeur et supprime toute formule et fonction. Si s est une valeur numérique, alors Value prend la valeur de s. Sinon, tente d'exécuter la formule. En cas d'erreur, retourne false. L'objet n'est alors pas modifier.
		/// </summary>
		public bool TryAlter(string s)
		{
			DoubleF n;
			if (!DoubleF.TryParse(s, out n, false)) { return false; }
			_value = n._value;
			_isFormula = n._isFormula;
			_formula = n._formula;
			_method = n._method;
			return true;
		}


		/// <summary>
		/// Enregistre la valeur et supprime toute formule et fonction.
		/// </summary>
		public double Alter(double n)
		{
			_value = n;
			_isFormula = false;
			_formula = null;
			_method = null;
			return _value;
		}


		/// <summary>
		/// Enregistre la valeur et supprime toute formule et fonction.
		/// </summary>
		public double Alter(decimal n)
		{
			_value = (double)n;
			_isFormula = false;
			_formula = null;
			_method = null;
			return _value;
		}


		/// <summary>
		/// Enregistre la valeur et supprime toute formule et fonction.
		/// </summary>
		public double Alter(int n)
		{
			_value = (double)n;
			_isFormula = false;
			_formula = null;
			_method = null;
			return _value;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne une nouvelle instance de l'objet, avec copie profonde.
		/// </summary>
		public DoubleF Copy()
		{
			if (_isFormula) { return new DoubleF(_formula); }
			else { return new DoubleF(_value); }
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne Value sous forme de chaîne de caractères.
		/// </summary>
		public override string ToString()
			{ return _value.ToString(); }
		
		/// <summary>
		/// Applique le format à Value.ToString(). En clair, retourne simplement Value.ToString(format).
		/// </summary>
		public string ToString(string format)
			{ return _value.ToString(format); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne la formule si elle existe, sinon la valeur numérique sous forme de chaîne.
		/// </summary>
		public string GetStrValue()
			{ return (_isFormula ? _formula : _value.ToString()); }

		/// <summary>
		/// Retourne la formule si elle existe, sinon la valeur numérique sous forme de chaîne, avec le format spécifié.
		/// </summary>
		public string GetStrValue(string format)
			{ return (_isFormula ? _formula : _value.ToString(format)); }




		#endregion METHODES PUBLIQUES

	
	
	}
	
	
	
	
}
