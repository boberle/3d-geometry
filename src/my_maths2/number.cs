using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My
{




	/// <summary>
	/// Classe de nombre, conservé sous forme de chaîne, et donnant des calculs exacts (mais lents), avec n'importe quel nombre de chiffres significatifs.
	/// </summary>
	[Serializable()]
	public class Number
	{







		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS
		
		
		
		protected string _decimalPart;
		protected string _integerPart;
		protected bool _isNegative;
		



		#endregion DECLARATIONS











		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES
		
		
		
		
		public bool NaN { get; set; }
		
		
		/// <summary>
		/// Obtient ou définit la partie décimale du nombre. Si le texte passé n'est pas que constitué que de chiffres, NaN à true et partie décimal à "".
		/// </summary>
		public string DecimalPart
		{
			get { return _decimalPart; }
			set
			{
				bool ok = Number.IsNumber(value, false, false);
				if (ok == false) { this.NaN = true; _decimalPart = String.Empty; }
				else { _decimalPart = value; }
			}
		}
		
		
		
		
		
		
		
		
		/// <summary>
		/// Obtient ou définit la partie entière du nombre. Si le texte passé n'est pas que constitué que de chiffres, NaN à true et partie entière à "0".
		/// </summary>
		public string IntegerPart
		{
			get { return _integerPart; }
			set
			{
				bool ok = Number.IsNumber(value, false, false);
				if (ok == false) { this.NaN = true; _integerPart = "0"; }
				else { _integerPart = value; }
			}
		}
		
		
		
		
		/// <summary>
		/// Obtient ou définit si le nombre est négatif.
		/// </summary>
		public bool IsNegative { get { return _isNegative; } set { _isNegative = value; } }
		
		
		
		
		/// <summary>
		/// Obtient la valeur absolue du nombre entier, avec partie entière et éventuellement décimale.
		/// </summary>
		public string Abs
			{ get { return _integerPart + (_decimalPart.Length > 0 ? "." + _decimalPart : String.Empty); } }
			
			
			
		/// <summary>
		/// Obtient le nombre de chiffres (significatifs ou non) de la partie entière.
		/// </summary>
		public int IntegerLength { get { return _integerPart.Length; } }

		/// <summary>
		/// Obtient le nombre de chiffres (significatifs ou non) de la partie décimale .
		/// </summary>
		public int DecimalLength { get { return _decimalPart.Length; } }
		
		
		
		
		
		/// <summary>
		/// Retourne true si le nombre est nul (égal à 0).
		/// </summary>
		public bool IsNul
		{
			get
			{
				Number test = new Number(this);
				test.Trim();
				return ((test.IntegerPart.Equals("0")) && (test.DecimalLength == 0));
			}
		}
		



		
		
		
		/// <summary>
		/// Retourne un zéro.
		/// </summary>
		public static Number Zero
		{
			get { return new Number(); }
		}







		#endregion PROPRIETES
	















		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS
		
		
		
		
		/// <summary>
		/// Constructeur simple : initialisation des variables.
		/// </summary>
		public Number()
		{
		
			// Valeurs par défaut:
			_decimalPart = String.Empty;
			_integerPart = "0";
			this.NaN = false;
		
		}
		
		
		
		
		/// <summary>
		/// Constructeur appelant la méthode Parse.
		/// </summary>
		public Number(string number) : this()
		{
			Number n = Parse(number);
			_isNegative = n.IsNegative;
			_integerPart = n.IntegerPart;
			_decimalPart = n.DecimalPart;
		}




		/// <summary>
		/// Constructeur construisant un nouveau nombre à partir de deux chaînes.
		/// </summary>
		public Number(string integerPart, string decimalPart, bool isNegative) : this()
		{
		
			bool notOk = false;
			
			if (Number.IsNumber(integerPart, false, false))
			{
				if (String.IsNullOrEmpty(integerPart)) { _integerPart = "0"; }
				else { _integerPart = integerPart; }
			}
			else
			{
				notOk = true;
			}
			
			if (Number.IsNumber(decimalPart, false, false))
			{
				if (String.IsNullOrEmpty(decimalPart)) { _decimalPart = String.Empty; }
				else { _decimalPart = decimalPart; }
			}
			else
			{
				notOk = true;
			}
			
			_isNegative = isNegative;
			
			if (notOk) { this.NaN = true; throw new NotFiniteNumberException(); }
				
		}
		
		
		
		
		
		/// <summary>
		/// Ce constructeur clone le nombre passé en argument.
		/// </summary>
		public Number(Number number) : this()
		{
			_isNegative = number.IsNegative;
			_integerPart = number.IntegerPart;
			_decimalPart = number.DecimalPart;
			this.NaN = number.NaN;
		}
		
		
		
		
		
		
		/// <summary>
		/// Ce constructeur clone accepte un int.
		/// </summary>
		public Number(int nb) : this(nb.ToString()) { }
		
		
		
		
		



		#endregion CONSTRUCTEURS












		// ---------------------------------------------------------------------------
		// METHODES D'INSTANCE PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES D'INSTANCE PUBLIQUES







		/// <summary>
		/// Supprime les zéros superflus avant et après le nombre. Retourne FullNumber.
		/// </summary>
		public string Trim()
		{
			_integerPart = _integerPart.TrimStart(new char[]{'0'});
			if (String.IsNullOrEmpty(_integerPart)) { _integerPart = "0"; }
			_decimalPart = _decimalPart.TrimEnd(new char[]{'0'});
			return this.ToString();
		}
		
		
		
		
		
		
		
		

		// ---------------------------------------------------------------------------
		
		
		
		
		/// <summary>
		/// Rajoute des zéros avant le nombre pour que la partie entière ait totalLength chiffres. Retourne IntegerPart.
		/// </summary>
		public string PadIntegerPart(int totalLength)
		{
			_integerPart = _integerPart.PadLeft(totalLength, '0');
			return _integerPart;
		} 
	
		


		/// <summary>
		/// Rajoute des zéros après le nombre pour que la partie décimale ait totalLength chiffres. Retourne DecimalPart.
		/// </summary>
		public string PadDecimalPart(int totalLength)
		{
			_decimalPart = _decimalPart.PadRight(totalLength, '0');
			return _decimalPart;
		}








		// ---------------------------------------------------------------------------
		
		
		
		
		
		/// <summary>
		/// Clone le nombre.
		/// </summary>
		public object Clone()
		{
			return new Number(this);
		}








		// ---------------------------------------------------------------------------
		
		
		
		
		
		
		/// <summary>
		/// Compare deux nombres. Si this supérieur à n, retourne 1 ; si this inférieur à n, retourne -1 ; si this == n, retourne 0.
		/// </summary>
		public int CompareTo(Number n)
		{
			return Number.CompareNumbers(this, n);
		}









		// ---------------------------------------------------------------------------
		
		
		
		
		/// <summary>
		/// Retourne tous les chiffres de la partie entière dans un tableau d'entiers.
		/// </summary>
		public int[] GetIntegerPartArray()
		{
			int length = _integerPart.Length;
			int[] result = new int[length];
			for (int i=0; i<length; i++) { result[i] = Int32.Parse(_integerPart.Substring(i, 1)); }
			return result;
		}



		
		/// <summary>
		/// Retourne tous les chiffres de la partie décimale dans un tableau d'entiers. Tableau vide si rien.
		/// </summary>
		public int[] GetDecimalPartArray()
		{
			int length = _decimalPart.Length;
			int[] result = new int[length];
			for (int i=0; i<length; i++) { result[i] = Int32.Parse(_decimalPart.Substring(i, 1)); }
			return result;
		}
		
		
		
		
		/// <summary>
		/// Retourne tous les chiffres du nombre (parties entière et décimale) dans un tableau d'entiers.
		/// </summary>
		public int[] GetIntegerAndDecimalPartsArray()
		{
			return this.GetIntegerPartArray().Concat(this.GetDecimalPartArray()).ToArray();
		}







		// ---------------------------------------------------------------------------
		
		
		
		
		/// <summary>
		/// Retourne le nombre (parties entières et décimal) sans le séparateur décimal.
		/// </summary>
		public string GetFullNumberWithoutDecimalPoint()
		{
			return _integerPart + _decimalPart;
		}







		// ---------------------------------------------------------------------------



		/// <summary>
		/// Retourne le nombre sous forme de chaîne, avec signe si négatif et point décimal.
		/// </summary>
		public override string ToString()
		{
			return (_isNegative ? "-" : String.Empty)
				+  _integerPart
				+ (_decimalPart.Length > 0 ? "." + _decimalPart : String.Empty);
		}
	
	
	
	
	
		// ---------------------------------------------------------------------------






		/// <summary>
		/// Convertit en Int32.
		/// </summary>
		public int ToInt()
		{
			return Int32.Parse((_isNegative ? "-" : String.Empty) + _integerPart);
		}







		// ---------------------------------------------------------------------------
		
		
		
		
		
		public bool Equals(Number n)
		{
			
			// Clone les nombres et les aligne, puis les élague:
			Number newThis = new Number(this);
			Number newN = new Number(n);
			AlignNumbers(ref newThis, ref newN);
			newThis.Trim(); newN.Trim();
			
			// Compare les caractéristiques:
			if (newN.IsNegative != newThis.IsNegative) { return false; }
			if (newN.NaN != newThis.NaN) { return false; }
			if (!newN.IntegerPart.Equals(newThis.IntegerPart)) { return false; }
			if (!newN.DecimalPart.Equals(newThis.DecimalPart)) { return false; }

			// Retour si ok:
			return true;
			
		}
	
	
	
	
	
		#endregion METHODES D'INSTANCE PUBLIQUES















		// ---------------------------------------------------------------------------
		// METHODES STATIQUES
		// ---------------------------------------------------------------------------




		#region METHODES STATIQUES







		/// <summary>
		/// Vérifie si l'argument est constitué de chiffres seulement, et éventuellement d'un point ou d'un signe.
		/// </summary>
		public static bool IsNumber(string test, bool allowPoint, bool allowSign)
		{
		
			bool ok = true;
			for (int i=0; i<test.Length; i++)
			{
				if (!"0123456789".Contains(test.Substring(i, 1)))
				{
					if (((test.Substring(i, 1) == ".") && (allowPoint)) || (((test.Substring(i, 1) == "-") && (allowSign) && (i == 0))))
					{
						// OK
					}
					else
					{
						ok = false; break;
					}
				}
			}
			
			return ok;

		}






		// ---------------------------------------------------------------------------
		
		
		
		
		public static Number Parse(string s)
		{
		
			// Si la chaîne est vide ou nul, mais le nombre à zéro:
			if (String.IsNullOrEmpty(s)) { return new Number(); }
		
			// Contrôle si la chaîne est correcte:
			bool ok = Number.IsNumber(s, true, true);
			
			// Si non, lève exception:
			if (ok == false) { throw new NotFiniteNumberException(s); }
			
			// Si oui, retourne une instance:
			Number n = new Number();
			if (s.Substring(0, 1).Equals("-")) { n.IsNegative = true; s = s.Substring(1); }
			string[] arr = s.Split(new string[]{"."}, StringSplitOptions.None);
			n.IntegerPart = (String.IsNullOrEmpty(arr[0]) ? "0" : arr[0]);
			if (arr.Length > 1) { n.DecimalPart = arr[1]; }
			
			return n;
		
		}







		// ---------------------------------------------------------------------------
		
		
		
		
		
		
		/// <summary>
		/// Test Parse();
		/// </summary>
		public static bool TryParse(string s, out Number nb)
		{
		
			try
			{
				nb = Parse(s);
				return true;
			}
			catch
			{
				nb = null;
				return false;
			}
		
		}









		// ---------------------------------------------------------------------------
		
		
		
		
		
		/// <summary>
		/// "Egalise" deux nombres, c'est-à-dire rajoute des zéros avant les entiers et des zéros derrière les décimales pour avoir des nombres de mêmes nombre de chiffres et avec une virgule au même endroit dans les deux nombres. Ex: "21.51" et "4589" donneront "0021.51" et "4589.00";
		/// </summary>
		public static void AlignNumbers(ref Number n1, ref Number n2)
		{
		
			// Padding:
			n1.PadIntegerPart(Math.Max(n1.IntegerLength, n2.IntegerLength));
			n2.PadIntegerPart(Math.Max(n1.IntegerLength, n2.IntegerLength));
			n1.PadDecimalPart(Math.Max(n1.DecimalLength, n2.DecimalLength));
			n2.PadDecimalPart(Math.Max(n1.DecimalLength, n2.DecimalLength));

		}



		/// <summary>
		/// Voir EquilazeNumber.
		/// </summary>
		public static void AlignNumbers(params Number[] nbs)
		{
		
			// Cherche la plus grande largeur pour les deux parties:
			int maxInt = 0; int maxDec = 0;
			for (int i=0; i<nbs.Length; i++)
			{
				if (nbs[i].IntegerLength > maxInt) { maxInt = nbs[i].IntegerLength; }
				if (nbs[i].DecimalLength > maxDec) { maxDec = nbs[i].DecimalLength; }
			}
			
			// Padding:
			for (int i=0; i<nbs.Length; i++)
			{
				nbs[i].PadIntegerPart(maxInt);
				nbs[i].PadDecimalPart(maxDec);
			}

		}




		// ---------------------------------------------------------------------------
		
		
		
		
		
		/// <summary>
		/// Compare deux nombres. Si n1 supérieur à n2, retourne 1 ; si n1 inférieur à n2, retourne -1 ; si n1 == n2, retourne 0.
		/// </summary>
		public static int CompareNumbers(Number n1, Number n2)
		{
		
			// Si le nombre ont des signes diffrérents, ou s'ils sont tous deux nuls:
			if (n1.IsNul && n2.IsNul) { return 0; }
			if (n1.IsNegative && !n2.IsNegative) { return -1; }
			if (!n1.IsNegative && n2.IsNegative) { return 1; }

			// Obtient des chaînes formattées:
			Number newN1 = new Number(n1);
			Number newN2 = new Number(n2);
			newN1.Trim();
			newN2.Trim();
			Number.AlignNumbers(ref newN1, ref newN2);
			
			// Compare, en fonction du signe:
			if (n1.IsNegative) { return newN2.ToString().CompareTo(newN1.ToString()); }
			else { return newN1.ToString().CompareTo(newN2.ToString()); }
		
		}









		// ---------------------------------------------------------------------------
		
		
		
		
		
		/// <summary>
		/// Additionne des nombres.
		/// </summary>
		public static Number Add(params Number[] nbs)
		{
		
			// S'il a des nombres négatifs...
			bool negative = false;
			for (int i=0; i<nbs.Length; i++) { if (nbs[i].IsNegative) { negative = true; break; } }
			if (negative)
			{
				// Additionne tous les nombres positifs et toutes les valeurs absolues des négatifs, puis fait une soustraction:
				Number totalPos = new Number(), totalNeg = new Number(), tempNeg;
				for (int i=0; i<nbs.Length; i++)
				{
					if (nbs[i].IsNegative)
					{
						tempNeg = new Number(nbs[i]);
						tempNeg.IsNegative = false;
						totalNeg = Number.Add(totalNeg, tempNeg);
					}
					else
					{
						totalPos = Number.Add(totalPos, nbs[i]);
					}
				}
				return Number.Substract(totalPos, totalNeg);
			}
		
			// Clone et aligne les nombres:
			Number[] numbers = new Number[nbs.Length];
			for (int i=0; i<nbs.Length; i++) { numbers[i] = new Number(nbs[i]); }
			Number.AlignNumbers(numbers);
			
			// Récupère les tableaux d'entiers, sans le séparateur décimal:
			int[][] arr = new int[numbers.Length][];
			for (int i=0; i<arr.Length; i++) { arr[i] = numbers[i].GetIntegerAndDecimalPartsArray(); }
			
			// Additionne chaque "colonne" et crée une nouvelle chaîne:
			string result = String.Empty; string temp; int total = 0; int carry = 0;
			for (int i=arr[0].Length-1; i>=0; i--)
			{
				total = carry;
				for (int j=0; j<arr.Length; j++) { total += arr[j][i]; }
				temp = total.ToString();
				result = temp.Substring(temp.Length - 1, 1) + result; // Inscrit le dernier chiffre
				// Rajoute la dernière retenue si c'est le dernier calcul:
				if (i == 0) { result = temp.Substring(0, temp.Length - 1) + result; }
				carry = 0; // Retenue à 0
				if (temp.Length > 1) { carry = Int32.Parse(temp.Substring(0, temp.Length - 1)); } // Retenue
			}
			
			// Définit un nouveau nombre, en redéfinissant la partie décimal:
			return new Number(Number.AddDecimalPoint(result, numbers[0].DecimalLength));

		}









		// ---------------------------------------------------------------------------
		
		
		
		/// <summary>
		/// Mais le nombre à une puissance donnée.
		/// </summary>
		public static Number Pow(Number n, int pow)
		{
			if (pow == 0) { return new Number("1"); }
			Number result = new Number(n);
			for (int i=1; i<pow; i++) { result = new Number(Number.Multiply(result, n)); }
			return result;
		}
		
		
		
		
		/// <summary>
		/// Multiplie deux nombres.
		/// </summary>
		public static Number Multiply(Number n1, Number n2)
		{
		
			// Compte les décimales:
			int dec = n1.DecimalLength + n2.DecimalLength;
			
			// Obtient des tableaux de int contenant chaque chiffre des nombres:
			int[] N1 = n1.GetIntegerAndDecimalPartsArray();
			int[] N2 = n2.GetIntegerAndDecimalPartsArray();
			
			// "Pose" la multiplication: Pour tous les chiffres de n1, multiplie par chaque chiffre de n2, avec retenue, etc.:
			string temp; int carry = 0;
			string[] inter = new string[N2.Length];
			int c = 0;
			// Pour tous les chiffres de n2 (à l'envers):
			for (int i=N2.Length-1; i>=0; i--)
			{
				// Initialisation:
				inter[c] = String.Empty;
				carry = 0;
				// Pour tous les chiffres de n1 (à l'envers):
				for (int j=N1.Length-1; j>=0; j--)
				{
					temp = (N2[i] * N1[j] + carry).ToString();
					inter[c] = temp.Substring(temp.Length - 1, 1) + inter[c]; // Inscrit le dernier chiffre
					// Rajoute la dernière retenue si c'est le dernier calcul:
					if (j == 0) { inter[c] = temp.Substring(0, temp.Length - 1) + inter[c]; }
					carry = 0; // Retenue à 0
					if (temp.Length > 1) { carry = Int32.Parse(temp.Substring(0, temp.Length - 1)); } // Retenue
				}
				// Augmente le compteur:
				c++;
			}
			
			// Décalage de chaque ligne des résultats intermédiaires
			for (int i=1; i<N2.Length; i++)
				{ inter[i] += "".PadLeft(i, '0'); }
			
			// Créer des Numbers avec tous ces nombres intermédiaires, puis les additionnes:
			Number[] nbs = new Number[inter.Length];
			for (int i=0; i<nbs.Length; i++) { nbs[i] = new Number(inter[i]); }
			string result = Number.Add(nbs).ToString();
			
			// Ajoute le point décimal, créer un nombre et retourne:
			return new Number(Number.AddDecimalPoint(result, dec));			
			
		}







		// ---------------------------------------------------------------------------
		
		
		
		/// <summary>
		/// Ajoute un point décimal à la chaîne s pour laisser decimalNumbers décimal. Si la chaîne est plus courte que decimalNumbers, des 0 sont ajoutés devant la chaîne.
		/// </summary>
		public static string AddDecimalPoint(string s, int decimalNumbers)
		{
		
			// Ajoute le point décimal (met la chaîne à l'envers):
			s = ReverseString(s);
			s = s.PadRight(decimalNumbers + 1, '0');
			s = s.Substring(0, decimalNumbers) + "." + s.Substring(decimalNumbers);
			s = ReverseString(s);
			s = s.Trim(new char[]{'0'});
			if (s.StartsWith(".")) { s = "0" + s; }
			return s;

		}








		// ---------------------------------------------------------------------------
		
		
		
		
		/// <summary>
		/// Soustrait à un premier nombre les suivants.
		/// </summary>
		public static Number Substract(params Number[] nbs)
		{
		
			// Somme les nombres sauf le premier:
			Number[] sumArr = new Number[nbs.Length-1];
			for (int i=1; i<nbs.Length; i++) { sumArr[i-1] = nbs[i]; }
			Number sum = Number.Add(sumArr);
			
			// Compare les nombres, afin de savoir lequel est le plus grand:
			int comparison = Number.CompareNumbers(nbs[0], sum);
			// Sort si les deux nombres sont égaux:
			if (comparison == 0) { return new Number(); }
			// Détermine le signe de la différence, et lequel des deux nombres doit passer en premier:
			bool isNegative = false; Number firstNb = null, secondNb = null;
			if (comparison > 0) { isNegative = false; firstNb = new Number(nbs[0]); secondNb = sum; }
			if (comparison < 0) { isNegative = true; firstNb = sum; secondNb = new Number(nbs[0]); }
		
			// Aligne les nombres:
			Number.AlignNumbers(ref firstNb, ref secondNb);
			
			// Récupère les tableaux d'entiers sans le séparateur décimal pour les deux nombres:
			int[] firstArr = firstNb.GetIntegerAndDecimalPartsArray();
			int[] secondArr = secondNb.GetIntegerAndDecimalPartsArray();
			
			// Soustrait dans chaque "colonne" et crée une nouvelle chaîne:
			string result = String.Empty; string temp; int total, first; int carry = 0;
			for (int i=firstArr.Length-1; i>=0; i--)
			{
				// Calcul le total à soustraire (deuxième nombre et retenue):
				total = carry + secondArr[i];
				// Si first est inférieur à total, augmente first:
				first = firstArr[i];
				while (first < total) { first += 10; } 
				// Inscrit le chiffre calculé par différence dans le texte:
				result = (first - total).ToString() + result;
				// Obtient les retenues:
				temp = first.ToString();
				//if (i == 0) { result = sFirst.Substring(0, sFirst.Length - 1) + result; }
				carry = 0; // Retenue à 0
				if (temp.Length > 1) { carry = Int32.Parse(temp.Substring(0, temp.Length - 1)); } // Retenue
			}
			
			// Définit un nouveau nombre, en redéfinissant la partie décimal:
			Number difference = new Number(Number.AddDecimalPoint(result, firstNb.DecimalLength));
			difference.IsNegative = isNegative;
			return difference;
		
		
		}
	
	
	








		// ---------------------------------------------------------------------------
		
		
		
		
		/// <summary>
		/// Divise n par div. Le troisième argument est le nombre de décimal après la virgule (au minimum 0, par défaut). Le quotient est tronqué, et non arrondi. Test pour la division : 14789/67 = 220,7313432835 8208955223 8805970149 2537313432 8358208955 2238805970 1492537313 4328358208 9552238805 9701492537... (Le résultat vient d'internet, donc de l'"extérieur".)
		/// </summary>
		public static Number Divide(Number n, Number div, int precision, out Number remainder)
		{
		
			// Exception si division par zéro:
			if (div.IsNul) { throw new DivideByZeroException(); }
		
			// Clone les nombres:
			Number newN = new Number(n);
			Number newDiv = new Number(div);
			
			// Si n ou div ont une partie décimale, multiplie n et div par 10^decLength:
			newN.Trim(); newDiv.Trim();
			int decLength = Math.Max(newN.DecimalLength, newDiv.DecimalLength);
			if (decLength > 0)
			{
				newN = Number.PowOfTen(newN, decLength);
				newDiv = Number.PowOfTen(newDiv, decLength);
			}
			
			// Appelle un surcharge pour avoir un quotient entier:
			Number result;
			result = Divide(newN, newDiv, out remainder);
			
			// Retourne ce résultat si on ne veut pas de décimale, ou si le reste est 0:
			if ((precision == 0) || (remainder.IsNul)) { return result; }
			
			// Sinon, on entre dans une boucle:
			Number a, localRemainder; string decQuot = String.Empty; int i;
			while (true)
			{
				i = 0;
				do
				{
					// Multiplie le reste par 10:
					remainder = Number.PowOfTen(remainder, 1);
					a = Number.HowManyTimes(remainder, newDiv, out localRemainder);
					if (i > 0) { decQuot += "0"; }
					i++;
				} while (Number.CompareNumbers(a, Number.Zero) <= 0);
				
				// Calcul le nouveau reste pour l'étape suivante:
				remainder = localRemainder;
				
				// a représente le quotient tronqué (sans l'éventuel reste). C'est donc une partie de notre quotient:
				decQuot += a.ToString();
				
				// Sort si on a atteint la décimale souhaitée pour la précision, ou s'il n'y a plus de reste:
				if ((precision <= decQuot.Length) || (remainder.IsNul))
				{
					// Divise le reste:
					remainder = Number.PowOfTen(remainder, decQuot.Length * -1);
					// Découpe à precision la partie décimal (avec l'ajout de zéro avant a, elle peut être plus longue):
					if (decQuot.Length > precision) { decQuot = decQuot.Substring(0, precision); }
					result.DecimalPart = decQuot; // Rajoute la partie décimale:
					result.Trim(); // Découpe
					return result;
				}
				
			}

		}
		
		
		
		
		/// <summary>
		/// Réalise une division euclidienne sans chercher de décimales, et de façon nettement plus rapide que la méthode de HowManyTimes. Ne prend pas en compte les décimales dans le dividende ou le diviseur : il faut donc multiplier les nombres décimaux avant l'appel de cette méthode.
		/// </summary>
		private static Number Divide(Number n, Number div, out Number remainder)
		{
		
			// Si n < div, retourne directement 0, et n est reste:
			if (CompareNumbers(n, div) < 0)
			{
				remainder = new Number(n);
				return new Number();
			}
		
			// Obtient la chaîne de n:
			string s = n.GetFullNumberWithoutDecimalPoint();
			
			// Boucle qui parcours la chaîne s, en divisant pas à pas:
			Number a, result; string quotient = String.Empty; int c, f;
			while (true)
			{
			
				// Cherche un nombre constitué des premiers chiffres de s pour pouvoir le diviser:
				c = 0;
				do
				{
					// Longueur de la chaîne à prendre
					c++; 
					// Récupère un nombre constitué des c premiers chiffres de s et tente une division simple:
					a = Number.HowManyTimes(new Number(s.Substring(0, c)), div, out remainder);
				} while(Number.CompareNumbers(a, Number.Zero) <= 0);
				
				// Calcul le reste, et le remplace dans la chaîne s:
				f = s.Length - c;
				s = remainder.ToString() + s.Substring(c);
				
				// a représente le quotient tronqué (sans l'éventuel reste). C'est donc une partie de notre quotient:
				quotient += a.IntegerPart;

				// Si s est nul, rajoute les éventuels 0 restant (on les "abaisse"):
				remainder = new Number(s);
				//if (remainder.IsNul) { quotient += "".PadLeft(s.Length - 1, '0'); }
				
				// Si ce qu'il reste dans s est inférieur à div, sort car c'est la fin de la division euclidienne:
				if (Number.CompareNumbers(remainder, div) < 0)
				{
					// On "abaisse" éventuellement les derniers chiffres en mettant des 0 au quotient:
					quotient += "".PadLeft(f, '0');
					result = new Number(quotient);
					result.Trim();
					return result;
				}
				
			}
			
		}
		
		
		
		
		
		/// <summary>
		/// Tente de trouver un nombre entier i tel que (div * i) soit inférieur ou égal à n, en essayant les valeurs de i pour i=1, i=2, i=3... Selon les nombres (par exemple un petit div et un grand n) la méthode peut être très, très, très longue. A réserver, donc, pour les petits nombres.
		/// </summary>
		private static Number HowManyTimes(Number n, Number div, out Number remainder)
		{
			
			// Grossièrement, on tente de trouver un nombre entier i tel que div * i <= n.
			Number current_i; Number product, lastProduct = new Number(0);
			for (int i=1; true; i++)
			{
				current_i = new Number(i);
				// Si div * i > n, alors sort en retournant la dernière valeur de i (et 0 si c'est raté depuis le début...):
				product = Number.Multiply(current_i, div);
				if (Number.CompareNumbers(product, n) > 0)
				{
					remainder = Number.Substract(n, lastProduct);
					return new Number(i-1);
				}
				lastProduct = (Number)product.Clone();
			}
		
		}



		/// <summary>
		/// Voir Divide.
		/// </summary>
		public static Number Divide(Number n, Number div, int precision)
		{
			Number remainder;
			return Divide(n, div, precision, out remainder);
		}

		/// <summary>
		/// Voir Divide.
		/// </summary>
		//private static Number Divide(Number n, Number div)
		//{
			//Number remainder;
			//return Divide(n, div, out remainder);
		//}









		// ---------------------------------------------------------------------------
		
		
		
		
		
		/// <summary>
		/// Multiplie par une puissance de 10, positive ou négative, de façon plus rapide qu'avec Pow() ou Multiply().
		/// </summary>
		public static Number PowOfTen(Number n, int pow)
		{
		
			// Si puissance de 0, retourne une copie du nombre:
			if (pow == 0) { return new Number(n); }
		
			// Recupère les parties entière et décimale de n:
			string i = n.IntegerPart;
			string d = n.DecimalPart;
			
			// Si pow est positif:
			if (pow > 0)
			{
			 for (int c=0; c<pow; c++)
			 {
				if (String.IsNullOrEmpty(d) == false) { i += d.Substring(0, 1); d = d.Substring(1); }
				else { i += 0; }
			 } 
			}
			
			// Si pow est négatif:
			if (pow < 0)
			{
			 for (int c=0; c>pow; c--)
			 {
				if (String.IsNullOrEmpty(i) == false) { d = i.Substring(i.Length - 1) + d; i = i.Substring(0, i.Length - 1); }
				else { d = "0" + d; }
			 }
			 if (String.IsNullOrEmpty(i)) { i = "0"; }
			}
			
			// Créer un nouveau nombre avec les nouvelles valeurs:
			Number result = new Number(i + "." + d);
			result.Trim();
			return result;
		
		}






		// ---------------------------------------------------------------------------
		
		
		
		
		
		/// <summary>
		/// Retourne l'entier supérieur.
		/// </summary>
		public static Number Ceiling(Number n)
		{
			Number result = new Number(n.IntegerPart);
			return Number.Add(result, new Number(1));
		}








		// ---------------------------------------------------------------------------
		
		
		
		
		public static Number TruncateByInterval(Number min, Number max)
		{
		
			// Clone les nombres et les aligne:
			Number newMin = new Number(min);
			Number newMax = new Number(max);
			AlignNumbers(ref newMin, ref newMax);
			
			// Vérifie que les parties entières sont identiques, sinon retourne 0:
			if (!newMin.IntegerPart.Equals(newMax.IntegerPart)) { return new Number(0); }
			
			// Parcours la partie décimale pour noter les décimales communes à min et à max, et sort à la première différence:
			int l = newMin.DecimalLength;
			string sMin = newMin.DecimalPart;
			string sMax = newMax.DecimalPart;
			for (int i=0; i<l; i++)
			{
				if (!sMin.Substring(i, 1).Equals(sMax.Substring(i, 1)))
					{ newMin.DecimalPart = newMin.DecimalPart.Substring(0, i); break; }
			}
			
			// Retour:
			return newMin;
			
		
		}
	

	





		#endregion METHODES STATIQUES










		// ---------------------------------------------------------------------------
		// METHODES PRIVEES
		// ---------------------------------------------------------------------------




		#region METHODES PRIVEES




		/// <summary>
		/// Inverse une chaîne de caractères.
		/// </summary>
		private static string ReverseString(string s)
		{
			if (s == null) { return null; }
			StringBuilder result = new StringBuilder();
			for (int i=s.Length - 1; i>=0; i--)
				{ result = result.Append(s.Substring(i, 1)); }
			return result.ToString();
		}





		#endregion METHODES PRIVEES
	
	
	
	
	
	
	}




}
