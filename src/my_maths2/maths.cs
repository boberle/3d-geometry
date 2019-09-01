using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace My
{




	public static partial class Maths
	{





		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES
		
		
		
		/// <summary>
		/// Retourne les fonctions de géométrie.
		/// </summary>
		public static MethodInfo[] GeometryFunctions { get; private set; }
		
		/// <summary>
		/// Retourne les fonctions d'algèbre.
		/// </summary>
		public static MethodInfo[] AlgebraFunctions { get; private set; }
		
		/// <summary>
		/// Obtient ou définit la precision pour les méthodes Approx. 0.0000005 par défaut.
		/// </summary>
		public static double Precision {
			get { return _precDouble; }
			set { _precDecimal = (decimal)value; My.Maths2.MySettings.ApproxInterval = _precDouble = value; } }
		
		/// <summary>
		/// Pi, bien plus précis que Math.Pi en double. Le dernier chiffre est 2, arrondi ici à 3.
		/// </summary>
		public const decimal PI = 3.1415926535897932384626433833M;



		#endregion PROPRIETES





		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS
		
		
		
		// DECLARATIONS:
		private static decimal _precDecimal;
		private static double _precDouble;


		/// <summary>
		/// Initialise les propriétés statiques. Statiques.
		/// </summary>
		static Maths()
		{
			Precision = My.Maths2.MySettings.ApproxInterval;
			GeometryFunctions = typeof(MathsGeo).GetMethods();
			AlgebraFunctions = typeof(MathsAlg).GetMethods();
		}
		


		#endregion CONSTRUCTEURS





		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------




		#region METHODES


		
		/// <summary>
		/// Compare deux nombres, en valeur approchée.
		/// </summary>
		public static bool Approx(decimal a, decimal b)
		{
			return (b > a - _precDecimal && b < a + _precDecimal);
		}

		/// <summary>
		/// Compare deux nombres, en valeur approchée.
		/// </summary>
		public static bool Approx(double a, double b)
		{
			return (b > a - _precDouble && b < a + _precDouble);
		}

		/// <summary>
		/// Pour un nombre décimal, transforme une écriture du type a + 0.0000000123 (ou a est relatif) en a.
		/// </summary>
		public static double Approx(double a)
		{
			double trunc = Math.Truncate(a);
			if (Approx(a - trunc, 0)) { return trunc; }
			int sign = Math.Sign(a);
			if (Approx(a - trunc, 1 * sign)) { return trunc + 1 * sign; }
			return a;
		}

		/// <summary>
		/// Pour un nombre décimal, transforme une écriture du type a + 0.0000000123 (ou a est relatif) en a.
		/// </summary>
		public static decimal Approx(decimal a)
		{
			decimal trunc = Math.Truncate(a);
			if (Approx(a - trunc, 0)) { return trunc; }
			int sign = Math.Sign(a);
			if (Approx(a - trunc, 1 * sign)) { return trunc + 1 * sign; }
			return a;
		}


		// ---------------------------------------------------------------------------
	


		#endregion METHODES
	
	
	
	
	
	
	
	
	}
	
	
	
	
}
