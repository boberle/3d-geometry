using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace My
{



	/// <summary>
	/// Fournit des méthodes de géométrie.
	/// </summary>
	public static class MathsGeo
	{





		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------




		#region METHODES
		
		
		
		/// <summary>
		/// Convertit un radian en degré.
		/// </summary>
		public static double RadToDeg(double rad)
		{
			return (180 * rad) / Math.PI;
		}
		
		/// <summary>
		/// Convertit un radian en degré.
		/// </summary>
		public static decimal RadToDeg(decimal rad)
		{
			return (180M * rad) / (decimal)Math.PI;
		}
		
		/// <summary>
		/// Convertit un degré en radian
		/// </summary>
		public static double DegToRad(double deg)
		{
			return (Math.PI * deg) / 180;
		}

		/// <summary>
		/// Convertit un degré en radian
		/// </summary>
		public static decimal DegToRad(decimal deg)
		{
			return ((decimal)Math.PI * deg) / 180;
		}


		// ---------------------------------------------------------------------------

		
		/// <summary>
		/// Retourne Math.Acos converti en degrés.
		/// </summary>
		public static double AcosDeg(double d)
		{
			return RadToDeg(Math.Acos(d));
		}

		/// <summary>
		/// Retourne Math.Asin converti en degrés.
		/// </summary>
		public static double AsinDeg(double d)
		{
			return RadToDeg(Math.Asin(d));
		}

		/// <summary>
		/// Retourne Math.Atan converti en degrés.
		/// </summary>
		public static double AtanDeg(double d)
		{
			return RadToDeg(Math.Atan(d));
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Obtient le rayon des coordonnées polaires à partir des coordonnées cartésiennes.
		/// </summary>
		public static decimal ToPolarR(decimal x, decimal y)
		{
			return (decimal)Math.Sqrt(Math.Pow((double)x, 2) + Math.Pow((double)y, 2));
		}
		
		/// <summary>
		/// Retourne l'angle des coordonnées polaires en radian, compris entre 0 et 2π si minus vaut false, ou entre -π et π si minus vaut true.
		/// </summary>
		public static decimal ToPolarθ(decimal x, decimal y, bool minus)
		{
			// Calcul de r, cos et sin:
			decimal r = ToPolarR(x, y);
			if (r == 0) { return 0; }
			decimal cosθ = x / r;
			decimal sinθ = y / r;
			decimal θ = (decimal)Math.Acos((double)cosθ);
			// Si sin est positif, on retourne de suite:
			if (sinθ >= 0) { return θ; }
			// Sinon, on oppose θ (si minus vaut true) ou on déduit θ par soustraction:
			else if (minus) { return θ * -1; }
			else { return (decimal)(2.0 * Math.PI) - θ; }
		}
		
		/// <summary>
		/// Retourne l'angle des coordonnées polaires en radian, compris entre 0 et 2π.
		/// </summary>
		public static decimal ToPolarθ(decimal x, decimal y)
		{
			return ToPolarθ(x, y, false);
		}
		
		
		/// <summary>
		/// Obtient le rayon des coordonnées polaires à partir des coordonnées cartésiennes.
		/// </summary>
		public static double ToPolarR(double x, double y)
		{
			return Math.Sqrt(x*x + y*y);
		}
		
		/// <summary>
		/// Retourne l'angle des coordonnées polaires en radian, compris entre 0 et 2π si minus vaut false, ou entre -π et π si minus vaut true.
		/// </summary>
		public static double ToPolar(double x, double y, bool minus, out double r)
		{
			r = Math.Sqrt(x*x + y*y);
			if (r == 0) { return 0; }
			double cosθ = x / r;
			double sinθ = y / r;
			double θ = Math.Acos(cosθ);
			// Si sin est positif, on retourne de suite:
			if (sinθ >= 0) { return θ; }
			// Sinon, on oppose θ (si minus vaut true) ou on déduit θ par soustraction:
			else if (minus) { return θ * -1; }
			else { return 2.0 * Math.PI - θ; }
		}
		
		/// <summary>
		/// Retourne l'angle des coordonnées polaires en radian, compris entre 0 et 2π.
		/// </summary>
		public static double ToPolar(double x, double y, out double r)
			{ return ToPolar(x, y, false, out r); }

		/// <summary>
		/// Retourne l'angle des coordonnées polaires en radian, compris entre 0 et 2π si minus vaut false, ou entre -π et π si minus vaut true.
		/// </summary>
		public static double ToPolar(double x, double y, bool minus)
			{ double r; return ToPolar(x, y, minus, out r); }


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne les coordonnées cartésiennes à partir des coordonnées polaires.
		/// </summary>
		public static Coord2D FromPolar(double r, double θ)
		{
			return new Coord2D(r * Math.Cos(θ), r * Math.Sin(θ));
		}


		// ---------------------------------------------------------------------------

	
		/// <summary>
		/// Retourne les coordonnées cartésiennes à partir des coordonnées polaires. Pour que le calcul soit pertinent (sinon, toutefois, il n'est quand même pas absurde), il faut que λ soit limité entre -2π et 2π, et φ entre -π/2 et π/2. Utiliser pour cela NormalizeGeographicAngles.
		/// </summary>
		public static Coord3D FromPolar(double r, double λ, double φ)
		{
			return new Coord3D(r * Math.Cos(λ) * Math.Cos(φ),
				r * Math.Sin(λ) * Math.Cos(φ), r * Math.Sin(φ));
		}


		// ---------------------------------------------------------------------------
	
		/// <summary>
		/// Convertit les coordonnées cartésiennes en coordonnées polaires. λ est limité entre -2π et 2π si minus vaut true, entre 0 et 2π si minus vaut false, et φ entre -π/2 et π/2. Si cos(φ) est nul, alors λ peut être de n'importe quelle valeur : c'est defaultλ qui est pris.
		/// </summary>
		public static void ToGeographic(double x, double y, double z, bool minus, double defaultλ, out double λ, out double φ, out double r)
		{
			r = Math.Sqrt(x*x + y*y + z*z);
			if (r == 0) { λ = defaultλ; φ = 0; return; }
			φ = Math.Asin(z / r);
			double cosφ = Math.Cos(φ);
			if (Maths.Approx(cosφ) == 0) { λ = defaultλ; }
			else { λ = GetAngleFromCosSin(x / (r*cosφ), y / (r*cosφ), minus); }
		}


		/// <summary>
		/// λ est limité entre -2π et 2π, et φ entre -π/2 et π/2. Si φ sort de ses limites, ie. si son cos est négatif, alors les deux angles sont transformés en conséquence.
		/// </summary>
		public static void NormalizeGeographicAngles(ref double λ, ref double φ)
		{
			if (Math.Cos(φ) < 0) { λ -= Math.PI; φ = Math.Acos(Math.Abs(Math.Cos(φ))) * Math.Sign(φ); }
			λ = GetMainAngleMeasure(λ, false); φ = GetMainAngleMeasure(φ, false);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne la valeur de l'angle comprise entre 0 et 2π si minus vaut false, ou entre -π et π si minus vaut true, à partir du cos et du sin.
		/// </summary>
		public static double GetAngleFromCosSin(double cos, double sin, bool minus)
		{
			// Si sin est positif, on retourne de suite:
			if (Maths.Approx(sin) >= 0) { return Math.Acos(Maths.Approx(cos)); }
			// Sinon, on oppose α (si minus vaut true) ou on déduit α par soustraction:
			else if (minus) { return Math.Acos(Maths.Approx(cos)) * -1; }
			else { return 2.0 * Math.PI - Math.Acos(Maths.Approx(cos)); }
		}
	

		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne l'aire du triangle formé par les trois segments dont les longueurs sont fournies, à l'aide des formules métriques.
		/// </summary>
		public static decimal GetArea(decimal a, decimal b, decimal c)
		{
			if (a == 0 || b == 0 || c == 0) { return 0M; }
			return (decimal)( ( Math.Sin((double)GetAngleUsingLengths(a, b, c)) * (double)b * (double)c ) / 2.0 );
		}
		
		/// <summary>
		/// Retourne l'aire du triangle formé par les trois segments dont les longueurs sont fournies, à l'aide des formules métriques.
		/// </summary>
		public static double GetArea(double a, double b, double c)
		{
			if (a == 0 || b == 0 || c == 0) { return 0; }
			return ( Math.Sin(GetAngleUsingLengths(a, b, c)) * b * c ) / 2.0;
		}


		// ---------------------------------------------------------------------------

	
		/// <summary>
		/// Retourne la mesure de l'angle A en radian, en utilisant la formule d'Al-Kashi.
		/// </summary>
		public static decimal GetAngleUsingLengths(decimal a, decimal b, decimal c)
		{
			if (b == 0 || c == 0) { return 0M; }
			decimal t = (a*a - b*b - c*c) / (-2M * b * c);
			if (t > 1 && Maths.Approx(t, 1)) { t = 1; } // Le résultat en decimal peut être supérieur à 1...
			return (decimal)Math.Acos((double)t);
		}
		
		/// <summary>
		/// Retourne la mesure de l'angle A en radian, en utilisant la formule d'Al-Kashi.
		/// </summary>
		public static double GetAngleUsingLengths(double a, double b, double c)
		{
			if (b == 0 || c == 0) { return 0; }
			double t = (a*a - b*b - c*c) / (-2.0 * b * c);
			return Math.Acos(t);
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne la mesure principale de l'angle, en radians. Si onlyPos est vrai, la valeur est comprise entre 0 et 2π, sinon
		/// entre -π et π.
		/// </summary>
		public static double GetMainAngleMeasure(double α, bool onlyPos)
		{
			double π = Math.PI, k;
			if (!onlyPos)
			{
				// Regarde la valeur la plus élevée ksup. Si elle est entière, c'est bon. Sinon, si elle est
				// positive, on récupère la valeur entière directement inférieure. Si elle est négative,
				// il faut récupérer l'entier directement supérieur de la valeur inférieure kinf:
				double ksup = (π - α) / (2.0 * π);
				/*if (ksup == Math.Floor(ksup)) { k = ksup; }
				else if (ksup > 0) { k = Math.Floor(ksup); }
				else { k = Math.Ceiling(-((π + α) / (2.0 * π))); }*/
				if (ksup == Math.Floor(ksup)) { k = ksup; }
				else if (ksup > 0) { k = Math.Floor(ksup); }
				else { k = Math.Floor(ksup); }
				return α + 2.0 * k * π;
			}
			else
			{
				// Même principe que tout à l'heure:
				double kinf = -α / (2.0 * π);//(2.0*π - α) / (2.0 * π);
				if (kinf == Math.Floor(kinf)) { k = kinf; }
				else if (kinf > 0) { k = Math.Ceiling(kinf); }
				else { k = Math.Ceiling(kinf); }
				return α + 2.0 * k * π;
			}
		}
		

		/// <summary>
		/// Retourne la mesure principale de l'angle, en radians. Si onlyPos est vrai, la valeur est comprise entre 0 et 2π, sinon
		/// entre -π et π.
		/// </summary>
		public static decimal GetMainAngleMeasure(decimal α, bool onlyPos)
		{
			decimal π = (decimal)Math.PI, k;
			if (!onlyPos)
			{
				// Regarde la valeur la plus élevée ksup. Si elle est entière, c'est bon. Sinon, si elle est
				// positive, on récupère la valeur entière directement inférieure. Si elle est négative,
				// il faut récupérer l'entier directement supérieur de la valeur inférieure kinf:
				decimal ksup = (π - α) / (2M * π);
				/*if (ksup == Math.Floor(ksup)) { k = ksup; }
				else if (ksup > 0) { k = Math.Floor(ksup); }
				else { k = Math.Ceiling(-((π + α) / (2.0 * π))); }*/
				if (ksup == Math.Floor(ksup)) { k = ksup; }
				else if (ksup > 0) { k = Math.Floor(ksup); }
				else { k = Math.Floor(ksup); }
				return α + 2M * k * π;
			}
			else
			{
				// Même principe que tout à l'heure:
				decimal kinf = -α / (2M * π);//(2.0*π - α) / (2.0 * π);
				if (kinf == Math.Floor(kinf)) { k = kinf; }
				else if (kinf > 0) { k = Math.Ceiling(kinf); }
				else { k = Math.Ceiling(kinf); }
				return α + 2M* k * π;
			}
		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Retourne soit la valeur exacte, soit une approximation ou un encadrement de l'angle α. Par exemple, si α=5.76, alors la chaîne retournée contiendra α = 11π/6 (si onlyPos vaut true) ou α = -π/6 (si onlyPos vaut false). Pour α=5.86, on aura -π/6 inf α inf 0. iMax détermine le dénominateur le plus élevé (doit être supérieur à 2).
		/// </summary>
		public static string GetAngleBounds(double α, int iMax, bool onlyPos, string angleName)
		{
		
			// Obtient la mesure principal de l'angle (ce qui évitera peut-être les problèmes d'arrondi):
			α = GetMainAngleMeasure(α, true);
			
			// Variables:
			double π = Math.PI, refAng; int jMax, mul = 0, div = 0, found = 0;
			double cosα = Math.Cos(α), sinα = Math.Sin(α);
			
			// Obtient la mesure de l'angle entre 0 et π/2 (c'est inversé, e.g. 5π/6 devient π/6):
			double αp = Math.Acos(Math.Abs(cosα));
			
			// Parcours les dénominateurs à partir de 2, puis les numérateur pour une mesure d'angle entre 0 et π/2:
			for (int i=2; i<=iMax; i++)
			{
				jMax = (int)Math.Floor(i / 2.0);
				for (int j=0; j<=jMax; j++)
				{
					refAng = j * π / i;
					// Compare les valeurs, en espérant trouvée une valeur approchée, sinon exacte:
					if (αp == refAng) { found = 1; div = i; mul = j; break; }
					else if (Maths.Approx(αp, refAng)) { found = 2; div = i; mul = j; break; }
					// Si dernier tour de pise, on tâche de trouver une valeur min et max:
					else if (i == iMax && αp < refAng) { found = 3; div = i; mul = j; break; }
					else if (i == iMax && j == jMax) { found = 3; div = i-1; mul = (int)Math.Floor((i-1) / 2.0);; break; }
				}
				// Si trouvé, on sort:
				if (found != 0) { break; }
			}
			
			// Analyse les résultats, et les valeurs réels de mul et div (puisque pour l'heure, l'angle est
			// est compris entre 0 et π/2):
			string result = "Error.", format1 = "{0} = {1}π/{2}", format2 = "{0} ≈ {1}π/{2}", format3 = "{0}π/{1} < {2} < {3}π/{4}";
			if (0 <= sinα && 0 <= cosα && cosα <= 1)
			{
				switch (found) {
					case 1: result = String.Format(format1, angleName, mul, div); break;
					case 2: result = String.Format(format2, angleName, mul, div); break;
					case 3: result = String.Format(format3, mul - 1, div, angleName, mul, div); break; }
			}
			else if (0 <= sinα && -1 <= cosα && cosα <= 0)
			{
				switch (found) {
					case 1: result = String.Format(format1, angleName, div - mul, div); break;
					case 2: result = String.Format(format2, angleName, div - mul, div); break;
					case 3: result = String.Format(format3, div - mul, div, angleName, div - mul + 1, div); break; }
			}
			else if ((0 > sinα && !onlyPos) && 0 <= cosα && cosα <= 1)
			{
				switch (found) {
					case 1: result = String.Format(format1, angleName, -mul, div); break;
					case 2: result = String.Format(format2, angleName, -mul, div); break;
					case 3: result = String.Format(format3, -mul, div, angleName, -(mul - 1), div); break; }
			}
			else if ((0 > sinα && !onlyPos) && -1 <= cosα && cosα <= 0)
			{
				switch (found) {
					case 1: result = String.Format(format1, angleName, -(div - mul), div); break;
					case 2: result = String.Format(format2, angleName, -(div - mul), div); break;
					case 3: result = String.Format(format3, -(div - mul + 1), div, angleName, -(div - mul), div); break; }
			}
			else if (-1 <= cosα && cosα <= 0)
			{
				switch (found) {
					case 1: result = String.Format(format1, angleName, mul + div, div); break;
					case 2: result = String.Format(format2, angleName, mul + div, div); break;
					case 3: result = String.Format(format3, mul - 1 + div, div, angleName, mul + div, div); break; }
			}
			else
			{
				switch (found) {
					case 1: result =  String.Format(format1, angleName, 2 * div - mul, div); break;
					case 2: result =  String.Format(format2, angleName, 2 * div - mul, div); break;
					case 3: result =  String.Format(format3, 2 * div - mul, angleName, div, 2 * div - mul + 1, div); break; }
			}
			
			// Simplifie les écritures:
			result = new Regex(@"0π/(\d)*").Replace(result, "0");
			result = new Regex(@"\b1π").Replace(result, "π");
			result = new Regex(@"/1\b").Replace(result, "");
			return result;
			
		}

		/// <summary>
		/// Voir surcharge. Le nom de l'angle est ici α.
		/// </summary>
		public static string GetAngleBounds(double α, int iMax, bool onlyPos)
		{
			return GetAngleBounds(α, iMax, onlyPos, "α");
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Retourne les trois vecteurs du système de coordonnées orthonormal ayant subi une rotation par ψ, θ et φ.
		/// </summary>
		public static void RotateCoordSystem(double ψ, double θ, double φ, out Coord3D OXp, out Coord3D OYp, out Coord3D OZp)
		{
			// On applique simplement les formules:
			OXp = new Coord3D(); OYp = new Coord3D(); OZp = new Coord3D();
			OXp.X = Math.Cos(ψ) * Math.Cos(φ) - Math.Sin(ψ) * Math.Cos(θ) * Math.Sin(φ);
			OXp.Y = Math.Sin(ψ) * Math.Cos(φ) + Math.Cos(ψ) * Math.Cos(θ) * Math.Sin(φ);
			OXp.Z = Math.Sin(θ) * Math.Sin(φ);
				OYp.X = -Math.Cos(ψ) * Math.Sin(φ) - Math.Sin(ψ) * Math.Cos(θ) * Math.Cos(φ);
				OYp.Y = -Math.Sin(ψ) * Math.Sin(φ) + Math.Cos(ψ) * Math.Cos(θ) * Math.Cos(φ);
				OYp.Z = Math.Sin(θ) * Math.Cos(φ);
			OZp.X = Math.Sin(ψ) * Math.Sin(θ);
			OZp.Y = -Math.Cos(ψ) * Math.Sin(θ);
			OZp.Z = Math.Cos(θ);
		}
	

		#endregion METHODES
		
		
		
		
	}
	
	
}
