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
	[Serializable()]
	public class DecimalF : ISerializable
	{






		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------



		#region DECLARATIONS


		// Propriétés:
		protected bool _isFormula;
		
		// Valeurs:
		protected string _formula;
		protected _methodDelegate _method;
		
		// Délégué pour la fonction:
		protected delegate decimal _methodDelegate();
		
		/// <summary>
		/// Ce champ est publique, mais il ne doit être utisé que pour la lecture, et seulement si le temps d'accès est important. Sinon, il faut utiliser la propriété Value.
		/// </summary>
		public decimal _value;


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
		/// Obtient la valeur numérique stockée lors du dernier calcul de la formule (ne recalcule pas). S'il n'y a pas de formule, retourne simplement la valeur. Définit la valeur numérique seulement s'il n'y a pas de formule (auquel cas, ne change pas la valeur).
		/// </summary>
		public decimal Value
		{
			get { return _value; }
			set { if (!_isFormula) _value = value; }
		}


		/// <summary>
		/// Obtient le texte de la formule, ou null s'il n'y a pas de formule.
		/// </summary>
		public string Formula { get { return _formula; } }


		/// <summary>
		/// Obtient si la formule a une erreur. Si c'est le cas, le valeur par défaut est 0, et l'objet se comporte comme s'il était définit par une valeur et non par une formule, sauf que la propriété Formula retourne tout de même la formule.
		/// </summary>
		public bool HasError { get; set; }
		
		
		/// <summary>
		/// Obtient si la valeur n'est pas un nombre, à la suite d'une erreur de calcul dans la formule (mais pas d'une erreur dans la syntaxe de la formule, mais bien un calcul impossible, par exemple une division par zéro). De fait, dans ce cas, Value est égal à Decimal.MaxValue.
		/// </summary>
		public bool IsNaN { get { return (_value == Decimal.MaxValue); } }
		



		#endregion PROPRIETES






		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS



		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static DecimalF()
		{
			DecimalF.UsedMethods = new MethodInfo[0];
			DecimalF.OwnerType = typeof(DecimalF);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Initialise à 0 et sans formule.
		/// </summary>
		public DecimalF()
		{
			_isFormula = false;
			_method = null;
			_formula = null;
			_value = 0;
		}


		/// <summary>
		/// Initialise avec s, suivant la méthode SetValue.
		/// </summary>
		public DecimalF(string s) : this()
		{
			this.SetValue(s);
		}


		/// <summary>
		/// Initialise avec n, suivant la méthode SetValue.
		/// </summary>
		public DecimalF(double n) : this()
		{
			this.SetValue(n);
		}


		/// <summary>
		/// Initialise avec n, suivant la méthode SetValue.
		/// </summary>
		public DecimalF(decimal n) : this()
		{
			this.SetValue(n);
		}


		/// <summary>
		/// Initialise avec n, suivant la méthode SetValue.
		/// </summary>
		public DecimalF(int n) : this()
		{
			this.SetValue(n);
		}


		/// <summary>
		/// Constructeur pour la désérialisation. Ne pas oublier d'appeler (par ": this()") le constructeur normal, sinon les contrôles ne sont pas définis !
		/// </summary>
		protected DecimalF(SerializationInfo info, StreamingContext context) : this()
		{
			// Récupère les propriétés:
			this.HasError = info.GetBoolean("HasError");
			_formula = info.GetString("_formula");
			_isFormula = info.GetBoolean("_isFormula");
			_value = info.GetDecimal("_value");
			// Si formule et si pas d'erreur, on recréer le délégué:
			if (_isFormula && !this.HasError) { this.SetValue(_formula); }
		}




		#endregion CONSTRUCTEURS






		// ---------------------------------------------------------------------------
		// SURCHARGE D'OPERATEURS
		// ---------------------------------------------------------------------------




		#region SURCHARGE D'OPERATEURS



		/// <summary>
		/// Surcharge d'opérateur ++. Ajoute 1 au nombre, seulement si ce n'est pas une formule. Retourne la même instance de n.
		/// </summary>
		public static DecimalF operator ++(DecimalF n)
			{ if (!n._isFormula) { n._value++; } return n; }


		/// <summary>
		/// Surcharge d'opérateur --. Enlève 1 au nombre, seulement si ce n'est pas une formule. Retourne la même instance de n.
		/// </summary>
		public static DecimalF operator --(DecimalF n)
			{ if (!n._isFormula) { n._value++; } return n; }


		/// <summary>
		/// Surcharge d'opérateur +. Ajoute n2 à n1, seulement si n1 et n2 ne sont pas des formules, et retourne l'instance modifiée de n1.
		/// </summary>
		public static DecimalF operator +(DecimalF n1, DecimalF n2)
		{
			if (!n1._isFormula && !n2._isFormula) { n1._value += n2._value; }
			return n1;
		}

		public static DecimalF operator +(DecimalF n1, int n2)
		{
			if (!n1._isFormula) { n1._value += n2; }
			return n1;
		}

		public static DecimalF operator +(DecimalF n1, float n2)
		{
			if (!n1._isFormula) { n1._value += (decimal)n2; }
			return n1;
		}

		public static DecimalF operator +(DecimalF n1, double n2)
		{
			if (!n1._isFormula) { n1._value += (decimal)n2; }
			return n1;
		}

		public static DecimalF operator +(DecimalF n1, decimal n2)
		{
			if (!n1._isFormula) { n1._value += n2; }
			return n1;
		}


		/// <summary>
		/// Surcharge d'opérateur -. Soustrait n2 à n1, seulement si n1 et n2 ne sont pas des formules, et retourne l'instance modifiée de n1.
		/// </summary>
		public static DecimalF operator -(DecimalF n1, DecimalF n2)
		{
			if (!n1._isFormula && !n2._isFormula) { n1._value -= n2._value; }
			return n1;
		}

		public static DecimalF operator -(DecimalF n1, int n2)
		{
			if (!n1._isFormula) { n1._value -= n2; }
			return n1;
		}

		public static DecimalF operator -(DecimalF n1, float n2)
		{
			if (!n1._isFormula) { n1._value -= (decimal)n2; }
			return n1;
		}

		public static DecimalF operator -(DecimalF n1, double n2)
		{
			if (!n1._isFormula) { n1._value -= (decimal)n2; }
			return n1;
		}

		public static DecimalF operator -(DecimalF n1, decimal n2)
		{
			if (!n1._isFormula) { n1._value -= n2; }
			return n1;
		}


		/// <summary>
		/// Surcharge d'opérateur *. Multiplie n2 à n1, seulement si n1 et n2 ne sont pas des formules, et retourne l'instance modifiée de n1.
		/// </summary>
		public static DecimalF operator *(DecimalF n1, DecimalF n2)
		{
			if (!n1._isFormula && !n2._isFormula) { n1._value = n1._value * n2.Value; }
			return n1;
		}

		public static DecimalF operator *(DecimalF n1, int n2)
		{
			if (!n1._isFormula) { n1._value = n1._value * n2; }
			return n1;
		}

		public static DecimalF operator *(DecimalF n1, decimal n2)
		{
			if (!n1._isFormula) { n1._value = n1._value * n2; }
			return n1;
		}


		/// <summary>
		/// Surcharge d'opérateur /. Divise n1 par n1, seulement si n1 et n2 ne sont pas des formules, et retourne l'instance modifiée de n1.
		/// </summary>
		public static DecimalF operator /(DecimalF n1, DecimalF n2)
		{
			if (!n1._isFormula && !n2._isFormula) { n1._value = n1._value / n2._value; }
			return n1;
		}

		public static DecimalF operator /(DecimalF n1, int n2)
		{
			if (!n1._isFormula) { n1.Value = n1._value / n2; }
			return n1;
		}

		public static DecimalF operator /(DecimalF n1, decimal n2)
		{
			if (!n1._isFormula) { n1.Value = n1._value / n2; }
			return n1;
		}



		#endregion SURCHARGE D'OPERATEURS






		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES




		/// <summary>
		/// Tente une conversion d'une chaîne en NbF. s peut être une formule ou une valeur numérique.
		/// </summary>
		public static bool TryParse(string s, out DecimalF nb)
		{
			return DecimalF.TryParse(s, out nb, false);
		}
		
		
		protected static bool TryParse(string s, out DecimalF nb, bool showError)
		{
			// Tente une conversion de type, et si ça marche, enregistre le nouveau nombre:
			decimal n; nb = new DecimalF();
			if (Decimal.TryParse(s, out n))
			{
				nb._value = n;
				nb._isFormula = false;
				nb._formula = null;
				nb._method = null;
				nb.HasError = false;
				return true;
			}
			// Si échec, créer une fonction et un délégué, et l'exécute:
			else
			{
				try
				{
					nb._method = null;/*(_methodDelegate)My.Formula.CreateFormulaMethod("formula",
											s,
											new string[0],
											typeof(Decimal),
											typeof(_methodDelegate),
											DecimalF.OwnerType,
											typeof(Decimal),
											new Type[0],
											DecimalF.UsedMethods,
											My.Formula.ErrorAction.MaxValue);*/
					nb._value = nb._method();
					nb._isFormula = true;
					nb._formula = s;
					nb.HasError = false;
					return true;
				}
				catch (Exception exc)
				{
					My.ErrorHandler.ShowError(exc);
					nb._value = 0;
					nb._isFormula = false;
					nb._formula = s;
					nb.HasError = true;
					nb._value = Decimal.MaxValue;
					return false;
				}
			}
		}
		
		
		/// <summary>
		/// Tente une conversion d'une chaîne en NbF. s peut être une formule ou une valeur numérique. Si erreur, lève une exception.
		/// </summary>
		public static DecimalF Parse(string s)
		{
			DecimalF nb;
			if (DecimalF.TryParse(s, out nb, false)) { return nb; }
			else { throw new FormatException("Input string was not in a correct format."); }
		}
		
		
		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Examine tous les DecimalF passés en argument, et retourne true si l'un d'autre eux est Nan, false sinon.
		/// </summary>
		public static bool IsThereNan(params DecimalF[] nbs)
		{
			foreach (DecimalF n in nbs) { if (n.Value == Decimal.MaxValue) { return true; } }
			return false;
		}
		
		
		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// S'il y a une formule, recalcule et retourne le résultat, en l'enregistrant dans Value. Sinon, retourne simplement Value.
		/// </summary>
		public decimal Recalculate()
		{
			if (_isFormula && !this.HasError)
			{
				try { _value = _method(); }
				catch (Exception exc) {
					My.ErrorHandler.ShowError(exc); _value = Decimal.MaxValue; }
			}
			return _value;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Définit la valeur. Si s est numérique, alors la la valeur est celle de s. Sinon, interprète s comme une formule, et l'exécute et retourne la valeur.
		/// </summary>
		public decimal SetValue(string s)
		{
			DecimalF n;
			DecimalF.TryParse(s, out n);
			_value = n._value;
			_isFormula = n._isFormula;
			_formula = n._formula;
			_method = n._method;
			this.HasError = n.HasError;
			return _value;
		}


		/// <summary>
		/// Enregistre la valeur et supprime toute formule et fonction.
		/// </summary>
		public decimal SetValue(double n)
		{
			_value = (decimal)Convert.ChangeType(n, typeof(Decimal));
			_isFormula = false;
			_formula = null;
			_method = null;
			this.HasError = false;
			return _value;
		}


		/// <summary>
		/// Enregistre la valeur et supprime toute formule et fonction.
		/// </summary>
		public decimal SetValue(decimal n)
		{
			_value = (decimal)Convert.ChangeType(n, typeof(Decimal));
			_isFormula = false;
			_formula = null;
			_method = null;
			this.HasError = false;
			return _value;
		}


		/// <summary>
		/// Enregistre la valeur et supprime toute formule et fonction.
		/// </summary>
		public decimal SetValue(int n)
		{
			_value = (decimal)Convert.ChangeType(n, typeof(Decimal));
			_isFormula = false;
			_formula = null;
			_method = null;
			this.HasError = false;
			return _value;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne une nouvelle instance de l'objet:
		/// </summary>
		public DecimalF Copy()
		{
			DecimalF n = new DecimalF();
			if (_isFormula) { n.SetValue(_formula); }
			else { n.SetValue(_value); }
			return n;
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Méthode pour sérialisation.
		/// </summary>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// Exception si null:
			if (info == null) throw new System.ArgumentNullException("SerializationInfo missing.");
			// Pour toutes les propriétés du type, les ajoute à la sérialisation:
			info.AddValue("_isFormula", _isFormula);
			info.AddValue("_value", _value);
			info.AddValue("_formula", _formula);
			info.AddValue("HasError", this.HasError);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne Value sous forme de chaîne de caractères:
		/// </summary>
		public override string ToString()
		{
			return (this.IsNaN || this.HasError ? "NaN" : _value.ToString());
		}
		
		/// <summary>
		/// Si format est "f", retourne la formule si elle existe sinon la valeur numérique sous forme de chaîne. Sinon, applique le format à la Value, ce qui signifie que tout format acceptable par la méthode ToString de Decimal est acceptable pour cette méthode.
		/// </summary>
		public string ToString(string format)
		{
			if (format.Equals("f")) { return (_isFormula ? _formula : this.ToString()); }
			else { return _value.ToString(format); }
		}



		#endregion METHODES PUBLIQUES






		// ---------------------------------------------------------------------------
		// METHODES PRIVEES
		// ---------------------------------------------------------------------------




		#region METHODES PRIVEES



		#endregion METHODES PRIVEES
	
	
	
	
	
	
	
	}
	
	
	
	
}
