using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My
{



	/// <summary>
	/// Fournit des méthodes d'algèbre.
	/// </summary>
	public static class MathsAlg
	{





		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------




		#region METHODES
		
		
		
		
		/// <summary>
		/// Résout un système d'équations linéaire. system représente le système: Chaque ligne est de la forme ax+bx+cx=d, ou a, b, c et d sont les coefficients inscrit dans le tableau. La fonction retourne un tableau contenant, dans l'ordre, les valeurs des variable x, y, z, etc. (et explainSol vaut alors 0). Si le système a une infinité de solutions, ou pas de solution, alors la fonction retourne null, et explainSol contient 1 (infinité de solutions) ou -1 (résolution impossible). On utilise ici la méthode par combinaisons (voir Fractale 2nde 1994 p. 267 et TP). On peut avoir plus d'équations que d'inconnues (mais pas l'inverse).
		/// </summary>
		public static double[] SolveSimul(double[,] system, out int explainSol)
		{
		
			// Nombre d'équations et d'inconnues:
			int colNb = system.GetLength(1);
			int rowNb = system.GetLength(0);
			int eqNb = colNb - 1;
			explainSol = 0;
			
			// Le principe ne fonctionne pas si les premiers coefficients des premières équations sont nuls, puisqu'on se retrouve à la fin
			// avec des infinités de valeurs possibles, alors que ce n'est pas exact. Donc, on trie le tableau de telle sorte que les coefficients
			// nuls se retrouvent plutôt à la fin:
			system = OrderForSolveSimul(system);
			
			// Tableau pour le nouveau système (une colonne de plus, puisqu'il y a le nombre après le signe égal):
			double[,] subSystem = new double[rowNb-1,colNb-1];
			// firstCoeff est le coefficient de a (la première inconnue):
			double firstCoeff = system[0,0], iCoeff;
			
			// Pour chaque équation sauf la première, on multiplie l'éq et la première par le premier coefficient de
			// l'autre, puis on soustrait la première à l'éq courrante, afin d'éléminer la première inconnue.
			// On obtient alors un système où la première inconnue n'est plus là.
			for (int i=1; i<rowNb; i++)
			{
				// iCoeff est le premier coefficient de l'éq courrante:
				iCoeff = system[i,0];
				// On multiplie la première éq et l'équation courrante par le premier coefficient de l'autre éq, et on soustrait:
				for (int j=1; j<colNb; j++)
				{
					subSystem[i-1,j-1] = system[0,j] * iCoeff - system[i,j] * firstCoeff;
				}
			}
			
			// S'appelle récursivement pour résoudre le nouveau système, s'il a plus d'une inconnue:
			double[] resSubSys = (eqNb>1 ? SolveSimul(subSystem, out explainSol) : new double[0]);
			// Si résultat null, alors retourne null:
			if (resSubSys == null) { return null; }
			
			// Pour chaque équation (il peut y avoir plus d'une équation, puisqu'on peut résoudre des systèmes de plus d'éq que d'inc):
			double lastSol = double.MaxValue, curSol;
			for (int i=0; i<rowNb; i++)
			{
				// Chaque ligne du tableau qu'on reçoit correspond à la valeur d'une inconnue du système envoyé, dans l'ordre
				// d'apparition. Donc on remplace les inconnues par les valeurs dans l'équation en cours de system:
				double interCalc = 0;
				for (int j=1; j<colNb-1; j++) { interCalc += system[i,j] * resSubSys[j-1]; }
				// Reste alors la première inconnue de system et le résultat de l'équation (après le signe égal, dernière colonne
				// du tableau). On peut alors trouver la solution de l'équation à une inconnue:
				double sum = system[i,colNb-1] - interCalc;
				
				// Si on a une équation du type 0x=a avec a non nul, l'équation n'a pas de solution:
				if (Maths.Approx(system[i,0], 0) && !Maths.Approx(sum, 0)) { explainSol = -1; return null; }
				
				// Si on a une équation du type 0x=0, alors x peut être tout réel. Il faut donc vérifier avec les autres équations
				// du système, pour tenter de déterminer une valeur unique qui vérifie toutes les équations, et on passe à la suite:
				if (Maths.Approx(system[i,0], 0) && Maths.Approx(sum, 0)) { continue; }
				
				// Sinon, on on résout, et on compare avec la valeur précédente:
				curSol = sum / system[i,0]; 
				// Si les deux valeurs ne corresondent pas, c'est qu'il n'y a pas de solution:
				if (lastSol != double.MaxValue && !Maths.Approx(curSol, lastSol)) { explainSol = -1; return null; }
				// Si elles correspondent, on continue pour voir les autres équations:
				lastSol = curSol;
			}
			
			// Si on a MaxValue, c'est que toutes les équations ont donné une infinité de possibilités, et on sort:
			if (lastSol == double.MaxValue) { explainSol = 1; return null; }
			
			// A la fin, on retourne la valeur:
			return (new double[]{lastSol}).Concat(resSubSys).ToArray();
			
		}
		
		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static double[] SolveSimul(double[,] system)
		{
			int temp; return SolveSimul(system, out temp);
		}


		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static decimal[] SolveSimul(decimal[,] system, out int explainSol)
		{
		
			// Nombre d'équations et d'inconnues:
			int colNb = system.GetLength(1);
			int rowNb = system.GetLength(0);
			int eqNb = colNb - 1;
			explainSol = 0;
			
			// Le principe ne fonctionne pas si les premiers coefficients des premières équations sont nuls, puisqu'on se retrouve à la fin
			// avec des infinités de valeurs possibles, alors que ce n'est pas exact. Donc, on trie le tableau de telle sorte que les coefficients
			// nuls se retrouvent plutôt à la fin:
			system = OrderForSolveSimul(system);
			
			// Tableau pour le nouveau système (une colonne de plus, puisqu'il y a le nombre après le signe égal):
			decimal[,] subSystem = new decimal[rowNb-1,colNb-1];
			// firstCoeff est le coefficient de a (la première inconnue):
			decimal firstCoeff = system[0,0], iCoeff;
			
			// Pour chaque équation sauf la première, on multiplie l'éq et la première par le premier coefficient de
			// l'autre, puis on soustrait la première à l'éq courrante, afin d'éléminer la première inconnue.
			// On obtient alors un système où la première inconnue n'est plus là.
			for (int i=1; i<rowNb; i++)
			{
				// iCoeff est le premier coefficient de l'éq courrante:
				iCoeff = system[i,0];
				// On multiplie la première éq et l'équation courrante par le premier coefficient de l'autre éq, et on soustrait:
				for (int j=1; j<colNb; j++)
				{
					subSystem[i-1,j-1] = system[0,j] * iCoeff - system[i,j] * firstCoeff;
				}
			}
			
			// S'appelle récursivement pour résoudre le nouveau système, s'il a plus d'une inconnue:
			decimal[] resSubSys = (eqNb>1 ? SolveSimul(subSystem, out explainSol) : new decimal[0]);
			// Si résultat null, alors retourne null:
			if (resSubSys == null) { return null; }
			
			// Pour chaque équation (il peut y avoir plus d'une équation, puisqu'on peut résoudre des systèmes de plus d'éq que d'inc):
			decimal lastSol = decimal.MaxValue, curSol;
			for (int i=0; i<rowNb; i++)
			{
				// Chaque ligne du tableau qu'on reçoit correspond à la valeur d'une inconnue du système envoyé, dans l'ordre
				// d'apparition. Donc on remplace les inconnues par les valeurs dans l'équation en cours de system:
				decimal interCalc = 0;
				for (int j=1; j<colNb-1; j++) { interCalc += system[i,j] * resSubSys[j-1]; }
				// Reste alors la première inconnue de system et le résultat de l'équation (après le signe égal, dernière colonne
				// du tableau). On peut alors trouver la solution de l'équation à une inconnue:
				decimal sum = system[i,colNb-1] - interCalc;
				
				// Si on a une équation du type 0x=a avec a non nul, l'équation n'a pas de solution:
				if (Maths.Approx(system[i,0], 0) && !Maths.Approx(sum, 0)) { explainSol = -1; return null; }
				
				// Si on a une équation du type 0x=0, alors x peut être tout réel. Il faut donc vérifier avec les autres équations
				// du système, pour tenter de déterminer une valeur unique qui vérifie toutes les équations, et on passe à la suite:
				if (Maths.Approx(system[i,0], 0) && Maths.Approx(sum, 0)) { continue; }
				
				// Sinon, on on résout, et on compare avec la valeur précédente:
				curSol = sum / system[i,0]; 
				// Si les deux valeurs ne corresondent pas, c'est qu'il n'y a pas de solution:
				if (lastSol != decimal.MaxValue && !Maths.Approx(curSol, lastSol)) { explainSol = -1; return null; }
				// Si elles correspondent, on continue pour voir les autres équations:
				lastSol = curSol;
			}
			
			// Si on a MaxValue, c'est que toutes les équations ont donné une infinité de possibilités, et on sort:
			if (lastSol == decimal.MaxValue) { explainSol = 1; return null; }
			
			// A la fin, on retourne la valeur:
			return (new decimal[]{lastSol}).Concat(resSubSys).ToArray();
			
		}
		
		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static decimal[] SolveSimul(decimal[,] system)
		{
			int temp; return SolveSimul(system, out temp);
		}


		/// <summary>
		/// Retourne un tableau trié de façon à ce que les premières valeurs égales à 0 de chaque lignes se retrouve sur les dernières lignes.
		/// Chaque valeur est d'abord convertie en 0 (si la valeur est nulle) ou 1 (si la valeur n'est pas nulle), de façon à pouvoir écrire
		/// chaque ligne sous la forme de chaîne : 0110, 0010, 1100. Puis un tri sur chaîne est effectué, puis inversé, de façon à obtenir,
		/// dans l'exemple: 1100, 0110, 0010.
		/// </summary>
		private static T[,] OrderForSolveSimul<T>(T[,] array)
		{
			// Variables:
			int length = array.GetLength(0);
			int colNb = array.GetLength(1);
			int[] indexes = new int[length];
			string[] bin = new string[length];
			// Forme les chaînes sous forme de 0 et 1, pour chaque ligne:
			for (int i=0; i<length; i++)
			{
				indexes[i] = i; bin[i] = String.Empty;
				for (int j=0; j<colNb; j++) { bin[i] += (array[i,j].Equals(default(T)) ? "0" : "1"); }
			}
			// Tri les tableaux et inverse le tri:
			Array.Sort(bin, indexes);
			Array.Reverse(indexes);
			// Construit un nouveau tableau en se servant du tableau indexes, qui contient les index de array trié dans l'ordre de bin:
			T[,] result = new T[length,colNb];
			for (int i=0; i<length; i++) 	{ for (int j=0; j<colNb; j++) { result[i,j] = array[indexes[i],j]; } }
			return result;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Résout le nombre de racines du trinôme ax^2 + bx +cv. Les paramètres de sorti contiennent les solutions.
		/// </summary>
		public static int SolveTrinomial(decimal a, decimal b, decimal c, out decimal x_1, out decimal x_2)
		{
			x_1 = x_2 = 0;
			decimal Δ = (b * b) - 4M * a * c;
			if (Δ < 0) { return 0; }
			else if (Δ == 0) {
				x_1 = (b * -1) / (2 * a); return 1; }
			else {
				x_1 = ( (b * -1) + (decimal)Math.Sqrt((double)Δ) ) / (2 * a);
				x_2 = ( (b * -1) - (decimal)Math.Sqrt((double)Δ) ) / (2 * a);
				return 2; }
		}

		/// <summary>
		/// Résout le nombre de racines du trinôme ax^2 + bx +cv. Les paramètres de sorti contiennent les solutions.
		/// </summary>
		public static int SolveTrinomial(double a, double b, double c, out double x_1, out double x_2)
		{
			x_1 = x_2 = 0;
			double Δ = (b * b) - 4 * a * c;
			if (Δ < 0) { return 0; }
			else if (Δ == 0) {
				x_1 = (b * -1) / (2 * a); return 1; }
			else {
				x_1 = ( (b * -1) + Math.Sqrt(Δ) ) / (2 * a);
				x_2 = ( (b * -1) - Math.Sqrt(Δ) ) / (2 * a);
				return 2; }
		}



		#endregion METHODES
		
		
		
		
	}

	
}
