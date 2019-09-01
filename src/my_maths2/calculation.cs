using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My
{





	public static class Calculation
	{





		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES





		/// <summary>
		/// Calcule une racine carrée de n tronquée à precision chiffre après la virgule. Accept n'importe quelle valeur de n et de precision, sans erreur.
		/// </summary>
		public static Number SquareRoot(Number n, int precision)
		{
		
			Number value, newN = new Number(n), odd = new Number(1), substractResult = new Number(), t, tOdd;
			string square = String.Empty, allFigures;
			Number one = new Number(1), two = new Number(2);
			int valCounter, opCounter;
			bool decOK = false;
			
			
			// Récupère la première tranche:
			newN.Trim();
			allFigures = newN.GetFullNumberWithoutDecimalPoint();
			if (newN.DecimalLength % 2 != 0 ) { allFigures += "0"; } // Rend le nombre de décimales pair
			valCounter = (newN.IntegerLength % 2 == 0 ? 2 : 1);
			value = new Number(allFigures.Substring(0, valCounter));
			
			// Boucle:
			for (int i=-1; i<=precision; i++)
			{
			
				// Soustrait tant que cela est possible:
				tOdd = odd; substractResult = new Number(value);
				for (int k=0; true; k++)
				{
					// Tente une soustraction:
					t = Number.Substract(substractResult, tOdd);
					// Si t>0, s'arrête là:
					if (Number.CompareNumbers(t, Number.Zero) < 0) { opCounter = k; break; }
					// Sinon, continue en trouvant un nouveau nombre pair + 2:
					substractResult = t;
					odd = tOdd;
					tOdd = Number.Add(odd, two);
				}
				
					// Note le nombre d'opérations effectuées (opCounter) à la suite de la racine:
					square += opCounter.ToString();
					
					// Si opCounter == 0 et qu'il n'y a plus que des 0 à analyser, abaisse un zéro par tranche restante et sort:
					if (substractResult.IsNul)
					{
						// Si on est au bout de la chaîne, sort directement:
						if ((valCounter + 2 > newN.IntegerLength) && (newN.DecimalLength == 0)) { return new Number(square); }
						// Sinon, compte le nombre de tranches restantes:
						else if (new Number(allFigures.Substring(valCounter)).IsNul)
						{
							// Si on a déjà dépasser la décimale, sort directement:
							if (decOK) { return new Number(square); }
							// Sinon, on compte le nombre de tranches restantes:
							else { square += "".PadRight((newN.IntegerLength - valCounter) / 2, '0'); return new Number(square); }
						}
					}
					
					// Calcule le prochain nombre impair à utiliser:
					if (opCounter == 0) { odd = Number.Substract(odd, two); substractResult = new Number(value); }
					odd = Number.Add(Number.PowOfTen(Number.Add(odd, one), 1), one);
					
					// Récupère la tranche suivante et forme la nouvelle valeur : substractResult accollé à la tranche suivante:
					if (valCounter + 2 > allFigures.Length) { allFigures += "00"; }
					value = new Number(substractResult.IntegerPart + allFigures.Substring(valCounter, 2));
					valCounter += 2;
					
					// Rajoute éventuellement un décimal dans le resultat:
					if ((valCounter > newN.IntegerLength) && (decOK == false)) { square += "."; decOK = true; }
					
					// Réduit le compteur de décimal (i) à 0:
					if (decOK == false) { i = -1; }
			
			}
			
			// Retour:
			return new Number(square);
		
		}





		// ---------------------------------------------------------------------------
	
		
		
		
		
		
		/// <summary>
		/// Calcule une racine carré de n tronquée à precision chiffres après la virgule. Plus rapide que SquareRoot en calculant le même algortihme mais avec le type "long" et non "Number". Si bien que les valeurs de n et de precision sont limitées. En cas d'exception, recalcule automatiquement avec SquareRoot. A n'utiliser, donc, que pour les racines et les précision de taille "raisonnable".
		/// </summary>
		public static Number SquareRootLittle(Number n, int precision)
		{
		
			Number newN = new Number(n);
			string square = String.Empty, allFigures;
			int valCounter, opCounter;
			bool decOK = false;
			
			Number max = new Number(ulong.MaxValue.ToString());
			long l_value, l_odd = 1, l_tOdd, l_substractResult = 0, l_t;
			
			
			// Récupère la première tranche:
			newN.Trim();
			allFigures = newN.GetFullNumberWithoutDecimalPoint();
			if (newN.DecimalLength % 2 != 0 ) { allFigures += "0"; } // Rend le nombre de décimales pair
			valCounter = (newN.IntegerLength % 2 == 0 ? 2 : 1);
			l_value = long.Parse(allFigures.Substring(0, valCounter));
			
			// Boucle:
			for (int i=-1; i<=precision; i++)
			{
			
				// Soustrait tant que cela est possible:
				l_tOdd = l_odd; l_substractResult = l_value;
				for (int k=0; true; k++)
				{
					// Si t>0, s'arrête là:
					if ((l_substractResult - l_tOdd) < 0) { opCounter = k; break; }
					// Tente une soustraction:
					l_t = l_substractResult - l_tOdd;
					// Sinon, continue en trouvant un nouveau nombre pair + 2:
					l_substractResult = l_t;
					l_odd = l_tOdd;
					l_tOdd = l_odd + 2;
				}
				
					// Note le nombre d'opérations effectuées (opCounter) à la suite de la racine:
					square += opCounter.ToString();
					
					// Si opCounter == 0 et qu'il n'y a plus que des 0 à analyser, abaisse un zéro par tranche restante et sort:
					if (l_substractResult == 0)
					{
						// Si on est au bout de la chaîne, sort directement:
						if ((valCounter + 2 > newN.IntegerLength) && (newN.DecimalLength == 0)) { return new Number(square); }
						// Sinon, compte le nombre de tranches restantes:
						else if (new Number(allFigures.Substring(valCounter)).IsNul)
						{
							// Si on a déjà dépasser la décimale, sort directement:
							if (decOK) { return new Number(square); }
							// Sinon, on compte le nombre de tranches restantes:
							else { square += "".PadRight((newN.IntegerLength - valCounter) / 2, '0'); return new Number(square); }
						}
					}
					
					// Calcule le prochain nombre impair à utiliser:
					if (opCounter == 0) { l_odd = l_odd - 2; l_substractResult = l_value; }
					l_odd = ( (l_odd + 1) * 10 ) + 1;
					
					// Récupère la tranche suivante et forme la nouvelle valeur : substractResult accollé à la tranche suivante:
					if (valCounter + 2 > allFigures.Length) { allFigures += "00"; }
					// Si trop grand pour long, recommence avec Number:
					if (Number.CompareNumbers(
						new Number(l_substractResult.ToString() + allFigures.Substring(valCounter, 2)), max) > 0)
						{ return SquareRoot(n, precision); }
					// Sinon, continue:
					l_value = long.Parse(l_substractResult.ToString() + allFigures.Substring(valCounter, 2));
					valCounter += 2;
					
					// Rajoute éventuellement un décimal dans le resultat:
					if ((valCounter > newN.IntegerLength) && (decOK == false)) { square += "."; decOK = true; }
					
					// Réduit le compteur de décimal (i) à 0:
					if (decOK == false) { i = -1; }
			
			}
			
			// Retour:
			return new Number(square);
		
		}






		// ---------------------------------------------------------------------------
		
	
		
		
		
		
		/// <summary>
		/// Retourne un valeur de la racine carrée de n, avec precision décimales. Cette valeur est tronquée, et non arrondie. Si les paramètres de sortie sont des tableaux, alors values[] contient la valeur calculée de la racine, et minValues[] et maxValues[] les valeurs d'encadrement. Si ce ne sont pas des tableaux, alors min et max contiennent le dernier encadrement calculé. Utilise le principe de la dichotomie, donc est en mesure de donner un encadrement exact. Mais il ne faut pas utiliser cette procédure pour calculer simplement une racine : c'est terriblement long !
		/// </summary>
		public static Number SquareRootDichotomy(Number n, int precision, out Number[] values, out Number[] minValues, out Number[] maxValues)
		{
			return SquareRootDichotomy(n, precision, true, out values, out minValues, out maxValues);
		}
		
		
		
		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static Number SquareRootDichotomy(Number n, int precision, out Number min, out Number max)
		{
			Number[] minValues, maxValues, values;
			Number value = SquareRootDichotomy(n, precision, false, out values, out minValues, out maxValues);
			min = minValues[0]; max = maxValues[0];
			return value;
		}
		
		
		
		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static Number SquareRootDichotomy(Number n, int precision)
		{
			Number min, max;
			return SquareRootDichotomy(n, precision, out min, out max);
		}
		
		
		
		/// <summary>
		/// Voir surcharge.
		/// </summary>
		private static Number SquareRootDichotomy(Number n, int precision, bool makeArray, out Number[] values, out Number[] minValues, out Number[] maxValues)
		{
		
			// Variables
			Number min, max, mid, sqrMid, value;
			int compare, c = 0;
			Number newN = new Number(n);
			newN.Trim();
			
			// Tableaux de contrôles éventuellements demandés:
			if (makeArray)
			{
				minValues = new Number[100];
				maxValues = new Number[100];
				values = new Number[100];
			}
			else
			{
				minValues = new Number[1];
				maxValues = new Number[1];
				values = new Number[1];
			}
			
			// Premières valeurs de min et max:
			min = new Number(0);
			max = Number.Ceiling(n);
			Number two = new Number(2);
			mid = new Number(0);

			// Boucle			
			do
			{
			
				// Calcul le milieu de l'intervalle, et le met au carré (pour la précision de la division, il faut savoir que la division par deux peut au pire augmenter le résultat d'une seule décimale par rapport au dividende):
				mid = Number.Divide(Number.Add(min, max), two, mid.DecimalLength + 1);
				sqrMid = Number.Pow(mid, 2);
				
				// Compare le carré et le nombre pour définir la moitié de l'intervalle à utiliser:
				compare = Number.CompareNumbers(sqrMid, n);
				if (compare < 0) { min = mid; }
				else if (compare > 0) { max = mid; }
				else { min = max = new Number(-1); }
				
				// Retaille au besoin les tableaux de contrôles:
				if (c >= minValues.Length)
				{
					Array.Resize(ref minValues, c + 100);
					Array.Resize(ref maxValues, c + 100);
					Array.Resize(ref values, c + 100);
				}
				
				// Calcul une valeur "exacte mais tronquée":
				value = Number.TruncateByInterval(min, max);
				
				// Si value == 0 et que n est entier, c'est que peut-être la racine est entière: on met au carré la valeur entière de mid, et on compare...
				if ((newN.DecimalLength == 0) && (value.IsNul) && (c > 0))
				{
					if (Number.CompareNumbers(Number.Pow(new Number(mid.IntegerPart), 2), n) == 0)
						{ return new Number(mid.IntegerPart); }
				}
				
				// Si n n'est pas un entier, alors la racine peut aussi ne pas avoir une infinité de décimales... On a par exemple √0.01=0.1, √0.001=0.1 ou √1.44=1.2, etc. Quoiqu'il en soit, on remarque que la racine a toujours un nombre de décimales égal à newN.DecimalLength / 2. Mais il faut regarder si les décimales de value ne changent pas.
				if ((newN.DecimalLength != 0) && (newN.DecimalLength % 2 == 0) && (new Number(value.DecimalPart).IsNul) && (c > 0))
				{
					Number newMid = new Number(mid);
					if (mid.DecimalLength >= newN.DecimalLength / 2)
						{ newMid.DecimalPart = mid.DecimalPart.Substring(0, newN.DecimalLength / 2); }
						if (Number.CompareNumbers(Number.Pow(newMid, 2), n) == 0) { newMid.Trim(); return newMid; }
				}
				// De façon plus mathématique, on aurait pu écrire:
				// Number newMid = Number.PowOfTen(mid, t.DecimalLength / 2);
				// newMid = new Number(newMid.IntegerPart);
				// newMid = Number.PowOfTen(newMid, (t.DecimalLength / 2) * -1);


				// Enregistre dans les tableaux de contrôle:
				if (makeArray)
				{
					minValues[c] = min;
					maxValues[c] = max;
					values[c]= value;
				}
				c++;
				
			} while (!(min.Equals(max)) && (value.DecimalLength < precision));

			// Valeur de retour:
			if (makeArray)
			{
				Array.Resize(ref minValues, c);
				Array.Resize(ref maxValues, c);
				Array.Resize(ref values, c);
			}
			else
			{
				minValues[0] = min;
				maxValues[0] = max;
				values[0] = value;
			}
			return value;
		
		}







		// ---------------------------------------------------------------------------
	
	
	
	
	
	
	
		public static Number PiCalculation(int step, int precision, bool makeArray, out Number[] values, out Number[] minValues, out Number[] maxValues)
		{
		
			// Variables et calculs initiaux:
			Number r = new Number(1);
			Number sqrR = Number.Pow(r, 2);
			Number c = new Number(r);
			Number sqrC = Number.Pow(c, 2);
			Number two = new Number(2);
			Number three = new Number(3);
			Number four = new Number(4);
			
			Number c_t, cp, cp_t,edges; int k = 0;
			Number  p = new Number(), pp = new Number(), value = new Number();
			
			// Tableaux de contrôles éventuellements demandés:
			if (makeArray)
			{
				minValues = new Number[100];
				maxValues = new Number[100];
				values = new Number[100];
			}
			else
			{
				minValues = new Number[1];
				maxValues = new Number[1];
				values = new Number[1];
			}

			// Boucle où i correspond à une étape des suites c(n), cp(n), p(n) et pp(n):
			for (int i=1; i<step; i++)
			{

				// Calcul de c(n), la longueur d'un côté du polygone inscrit au demi-cercle:

				// Méthode longue mais d'après les tests elle plus exacte (sans doute pour des problèmes d'arrondi, mais il faudrait vérifier):
				c_t = Number.Substract(sqrR, Number.Divide(sqrC, four, precision));
				c_t = Calculation.SquareRoot(c_t, precision);
				c_t = Number.Add(Number.Pow(Number.Substract(r, c_t), 2), Number.Divide(sqrC, four, precision));
				c = Calculation.SquareRoot(c_t, precision);
				sqrC = Number.Pow(c, 2);
				
				// Méthode courte (factorisée) mais d'après les tests elle produit des erreurs de plus en plus importantes, et finalement aberrante (sans doute pour des problèmes d'arrondi, mais il faudrait vérifier):
				/*c_t = Number.Substract(four, sqrC);
				c_t = Number.SquareRoot(c_t, sqrPrec);
				c_t = Number.Substract(two, c_t);
				c = Number.SquareRoot(c_t, sqrPrec);
				sqrC = Number.Pow(c, 2);*/

				// Calcul de cp(n), la longueur d'un côté du polygone circonscrit au demi-cercle:
				cp_t = Calculation.SquareRoot(Number.Substract(sqrR, Number.Divide(sqrC, four, precision)), precision);
				cp = Number.Divide(Number.Multiply(c, r), cp_t, precision);
				
				// Calcul du nombre de côtés:
				edges = Number.Multiply(three, Number.Pow(two, i));
				
				// Calcul des demi-périmètre p et pp:
				p = Number.Multiply(c, edges);
				pp = Number.Multiply(cp, edges);
				
				// Retaille au besoin les tableaux de contrôles:
				if (k >= minValues.Length)
				{
					Array.Resize(ref minValues, k + 100);
					Array.Resize(ref maxValues, k + 100);
					Array.Resize(ref values, k + 100);
				}
				
				// Calcul une valeur "exacte mais tronquée":
				value = Number.TruncateByInterval(p, pp);
				
				// Enregistre dans les tableaux de contrôle:
				if (makeArray)
				{
					minValues[k] = p;
					maxValues[k] = pp;
					values[k]= value;
					k++;
				}

			}
			
			// Valeur de retour:
			if (makeArray)
			{
				Array.Resize(ref minValues, k);
				Array.Resize(ref maxValues, k);
				Array.Resize(ref values, k);
			}
			else
			{
				minValues[0] = p;
				maxValues[0] = pp;
				values[0] = value;
			}
			return value;

		}
		
		
		
		
		
		public static Number PiCalculation(int step, int precision, out Number min, out Number max)
		{
		
			// Explications:
				// c(n): la longueur d'un côté du polygone inscrit au demi-cercle.
				// c'(n): la longueur d'un côté du polygone circonscrit au demi-cercle.
				// p(n): périmètre du demi-polygone inscrit.
				// p'(n): périmètre du demi-polygone circonscrit.
		
			// Variables:
			Number cp, p, pp, edges;
			Number two = new Number(2);
			Number three = new Number(3);

			// A partir des calculs exacts à l'étape step, calcul une valeur approchée de c(n) à la précision demandée:
			Number t = Calculation.SquareRoot(Number.Add(two, Calculation.SquareRoot(three, precision)), precision);
			for (int i=0; i<step-2; i++)
				{ 	t = Calculation.SquareRoot(Number.Add(two, t), precision); }
			Number c = Calculation.SquareRoot(Number.Substract(two, t), precision);
			
			// Calcul une valeur de c'(n):
			t = Calculation.SquareRoot(Number.Add(two, t), precision);
			cp = Number.Divide(Number.Multiply(c, two), t, precision);
			
			// Calcul du nombre de côtés:
			edges = Number.Multiply(three, Number.Pow(two, step));
			
			// Calcul des demi-périmètre p et pp:
			p = Number.Multiply(c, edges);
			pp = Number.Multiply(cp, edges);
								
			// Valeur de retour:
			min = p;
			max = pp;
			
			// Calcul une valeur "exacte mais tronquée", et retourne:
			return Number.TruncateByInterval(p, pp);

		}







		#endregion METHODES PUBLIQUES
	
	
	
	
	
	
	
	}
	
	
	
	
}
