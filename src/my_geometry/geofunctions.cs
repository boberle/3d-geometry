using System;
using System.Linq;

namespace My
{



	/// <summary>
	/// Fournit des fonctions de calcul.
	/// </summary>
	public static class GeoFunctions
	{




		// ---------------------------------------------------------------------------
		// METHODES D'INTERROGATION
		// ---------------------------------------------------------------------------




		#region METHODES D'INTERROGATION



		/// <summary>
		/// Retourne true si les vecteurs passés sont colinéaires.
		/// </summary>
		public static bool AreCollinear(SpVectorObject vec1, SpVectorObject vec2)
		{
			return AreCollinear(vec1.Coordinates, vec2.Coordinates);
		}

		/// <summary>
		/// Retourne true si les vecteurs passés sont colinéaires.
		/// </summary>
		public static bool AreCollinear(Coord3D vec1, Coord3D vec2)
		{
			// Avec le test de colinéarité xy'-x'y=0 ET xz'-x'z=0 ET yz'-y'z=0, on a:
			return (vec1.X * vec2.Y - vec2.X * vec1.Y == 0
				&& vec1.X * vec2.Z - vec2.X * vec1.Z == 0
				&& vec1.Z * vec2.Y - vec1.Y * vec2.Z == 0);
			/* // Ne connaissant pas le test de colinéarité dans l'espace, je regarde s'il existe un nombre
			// k tel que vec1=k*vec2, en divisant les coordonnées, puisque k=x(vec1)/x(vec2), etc.
			// Si l'un des vecteurs est nuls, alors ils sont colinéaires:
			if ((vec1.X.Value == 0 && vec1.Y.Value == 0 && vec1.Z.Value == 0)
				|| (vec2.X.Value == 0 && vec2.Y.Value == 0 && vec2.Z.Value == 0)) { return true; }			
			decimal k1 = Decimal.MaxValue, k2 = Decimal.MaxValue, k3 = Decimal.MaxValue;
			// Soit x'=kx. Si x=0 et x'=0, alors k peut être de n'importe quel valeur (MaxValue): on ne fait rien.
			// Si x=0 et que x'!=0, alors c'est impossible: les vecteurs ne sont pas colinéaire, et on sort de suite.
			if (vec2.X.Value == 0 && vec1.X.Value == 0) { ; }
			else if (vec2.X.Value == 0 && vec1.X.Value != 0) { return false; }
			else { k1 = vec1.X.Value / vec2.X.Value; }
				if (vec2.Y.Value == 0 && vec1.Y.Value == 0) { ; }
				else if (vec2.Y.Value == 0 && vec1.Y.Value != 0) { return false; }
				else { k2 = vec1.Y.Value / vec2.Y.Value; }
			if (vec2.Z.Value == 0 && vec1.Z.Value == 0) { ; }
			else if (vec2.Z.Value == 0 && vec1.Z.Value != 0) { return false; }
			else { k3 = vec1.Z.Value / vec2.Z.Value; }
			// On sort en comparant les valeurs, ou les MaxValue:
			return (k1 == Decimal.MaxValue || k2 == Decimal.MaxValue || k1 == k2)
				&& (k1 == Decimal.MaxValue || k3 == Decimal.MaxValue || k1 == k3)
				&& (k2 == Decimal.MaxValue || k3 == Decimal.MaxValue || k2 == k3);*/
		}

		/// <summary>
		/// Retourne true si les points sont alignés.
		/// </summary>
		public static bool AreAligned(SpPointObject spt1, SpPointObject spt2, SpPointObject spt3)
		{
			return AreCollinear(spt2.Coordinates - spt1.Coordinates, spt3.Coordinates - spt1.Coordinates);
		}

		/// <summary>
		/// Retourne true si les droites sont parallèles (on cherche la colinéarité des vecteurs).
		/// </summary>
		public static bool AreParallel(SpLine line1, SpLine line2)
		{
			return AreCollinear(line1.Vector.Coordinates, line2.Vector.Coordinates);
		}

		/// <summary>
		/// Retourne true si les plans sont parallèles (on regarde si les vecteurs normaux sont colinéaires.
		/// </summary>
		public static bool AreParallel(Eq3Zero plane1, Eq3Zero plane2)
		{
			// Avec le test de colinéarité xy'-x'y=0 ET xz'-x'z=0 ET yz'-y'z=0, on a:
			return (plane1.a * plane2.b - plane2.a * plane1.b == 0
				&& plane1.a * plane2.c - plane2.a * plane1.c == 0
				&& plane1.c * plane2.b - plane1.b * plane2.c == 0);
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne true si les points sont coplanaires, false sinon.
		/// </summary>
		public static bool AreCoplanar(params Coord3D[] pts)
		{
			// Elimine les points confondus:
			Coord3D[] newPts = pts.Distinct().ToArray();
			// S'il y a moins de 4 pts, ils sont forcément coplanaires:
			int length = newPts.Length;
			if (length < 4) { return true; }
			// Il faut vérifier que les points sont bien tous coplanaires. On sait que 3 vecteurs AB, AC et AD sont
			// coplanaires si, et seulement si, il existe un triplet (a,b,c) tel que aAB+bAC+cAD=0 différent de (0,0,0).
			// Autrement dit, on résout le système d'équation avec les coordonnées (donc on a un système à trois inconnues
			// et trois équations), et si la seule solution est (0,0,0), alors les points ne sont pas coplanaires.
			// On prend à chaque fois les deux mêmes premiers vecteurs, et on compare avec chacun des autres:
			Coord3D firstVec = newPts[1] - newPts[0];
			Coord3D secondVec = newPts[2] - newPts[0];
			Coord3D thirdVec;
			for (int i=3; i<length; i++)
			{
				thirdVec = newPts[i] - newPts[0];
				if (!AreCoplanar(firstVec, secondVec, thirdVec)) { return false; }
			}
			return true;
		}
		
		
		/// <summary>
		/// Retourne true si les vecteurs sont coplanaires, false sinon.
		/// </summary>
		public static bool AreCoplanar(SpVectorObject vec1, SpVectorObject vec2, SpVectorObject vec3)
		{
			return AreCoplanar(vec1.Coordinates, vec2.Coordinates, vec3.Coordinates);
		}


		/// <summary>
		/// Retourne true si les vecteurs sont coplanaires, false sinon.
		/// </summary>
		public static bool AreCoplanar(Coord3D vec1, Coord3D vec2, Coord3D vec3)
		{
			// Il faut vérifier que les vecteurs sont bien tous coplanaires. On sait que 3 vecteurs AB, AC et AD sont
			// coplanaires si, et seulement si, il existe un triplet (a,b,c) tel que aAB+bAC+cAD=0 différent de (0,0,0).
			// Autrement dit, on résout le système d'équation avec les coordonnées (donc on a un système à trois inconnues
			// et trois équations), et si la seule solution est (0,0,0), alors les vecteurs ne sont pas coplanaires.
			double[,] system = new double[3,4];
			system[0,0] = vec1.X; system[0,1] = vec2.X; system[0,2] = vec3.X; system[0,3] = 0;
			system[1,0] = vec1.Y; system[1,1] = vec2.Y; system[1,2] = vec3.Y; system[1,3] = 0;
			system[2,0] = vec1.Z; system[2,1] = vec2.Z; system[2,2] = vec3.Z; system[2,3] = 0;
			// Si le système a une résolution unique, les points ne sont pas coplanaires:
			if (MathsAlg.SolveSimul(system) != null) { return false; }
			return true;
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne true si des points ou vecteur du tableau ont même coordonnées.
		/// </summary>
		public static bool HaveSameCoords(params Coord3D[] pts)
		{
			/*int l = pts.Length, start = 0;
			for (int i=start; i<l; i++) {
				for (int j=0; j<l; j++) { if (i != j && pts[i] == pts[j]) { return true; } }
				start = i; }
			return false;*/
			// Plus simplement, on regarde si Distinct retourne le même nombre d'éléments:
			return (pts.Length != pts.Distinct().ToArray().Length);
		}

		/// <summary>
		/// Retourne true si des points du tableau ont même coordonnées.
		/// </summary>
		public static bool HaveSameCoords(params SpPointObject[] pts)
		{
			return HaveSameCoords(Array.ConvertAll<SpPointObject,Coord3D>(pts, delegate(SpPointObject o) { return o.Coordinates; }));
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne true si les vecteurs sont orthogonaux.
		/// </summary>
		public static bool AreOrthogonal(SpVectorObject vec1, SpVectorObject vec2)
		{
			return AreOrthogonal(vec1.Coordinates, vec2.Coordinates);
		}

		/// <summary>
		/// Retourne true si les vecteurs sont orthogonaux.
		/// </summary>
		public static bool AreOrthogonal(Coord3D vec1, Coord3D vec2)
		{
			return (vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z == 0);
		}


		// ---------------------------------------------------------------------------
		

		/// <summary>
		/// Indique si le point dont on passe les coordonnées 3D sont sur le plan spécifié. Le plan est défini par deux vecteurs (vec1 pour les abscisses et vec2 pour les odonnées) et une origine.
		/// </summary>
		public static bool IsPointOnPlane(Coord3D pt, Coord3D origin, Coord3D vec1, Coord3D vec2)
		{
			return !GeoFunctions.Get2DFrom3DCoords(pt, origin, vec1, vec2).Empty;
		}
		

		#endregion METHODES D'INTERROGATION




		// ---------------------------------------------------------------------------
		// METHODES DE CALCUL
		// ---------------------------------------------------------------------------




		#region METHODES DE CALCUL



		/// <summary>
		/// Retourne les coordonnées d'un vecteur somme.
		/// </summary>
		public static Coord3D GetVectorsSumCoords(params Coord3D[] vectors)
		{
			Coord3D coords = new Coord3D(0, 0, 0);
			foreach (Coord3D c in vectors) { coords += c; }
			return coords;
		}

		/// <summary>
		/// Retourne les coordonnées d'un vecteur somme.
		/// </summary>
		public static Coord3D GetVectorsSumCoords(params SpVectorObject[] vectors)
		{
			return GetVectorsSumCoords(Array.ConvertAll<SpVectorObject,Coord3D>(vectors,
				delegate(SpVectorObject o) { return o.Coordinates; }));
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne les points d'un parallélogramme défini par un point de départ et deux vecteurs auxquels on peut affecter des coefficients.
		/// </summary>
		public static void GetParallelogramCoords(Coord3D startPt, double coeff1, Coord3D vec1, double coeff2,
			Coord3D vec2, out Coord3D pt2, out Coord3D pt3, out Coord3D pt4)
		{
			pt2 = new Coord3D(); pt3 = new Coord3D(); pt4 = new Coord3D();
			pt2 = startPt + coeff1 * vec1;
			pt3 = startPt + coeff2 * vec2;
			pt4 = startPt + coeff1 * vec1 + coeff2 * vec2;
		}
		
		
		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne les coordonnées du projeté orthogonal d'un point sur une droite, ou une structure Empty si le vecteur directeur de la droite est nul.
		/// </summary>
		public static Coord3D GetOrthoProjPointOnLineCoords(Coord3D basePt, Coord3D lineOrigin, Coord3D lineVec)
		{
			if (lineVec.IsNul) { return new Coord3D(true); }
			double α = lineVec.X, β = lineVec.Y, γ = lineVec.Z;
			double xD = basePt.X, yD = basePt.Y, zD = basePt.Z;
			double xA = lineOrigin.X, yA = lineOrigin.Y, zA = lineOrigin.Z;
			double k = ( α * (xD - xA) + β * (yD - yA) + γ * (zD - zA) ) / ( α*α + β*β + γ*γ );
			return lineOrigin + k * lineVec;
		}
		
		/// <summary>
		/// Retourne les coordonnées du projeté orthogonal d'un point sur une droite, ou une structure Empty si le vecteur directeur de la droite est nul.
		/// </summary>
		public static Coord2D GetOrthoProjPointOnLineCoords(Coord2D basePt, Coord2D lineOrigin, Coord2D lineVec)
		{
			if (lineVec.IsNul) { return new Coord2D(true); }
			double α = lineVec.X, β = lineVec.Y;
			double xD = basePt.X, yD = basePt.Y;
			double xA = lineOrigin.X, yA = lineOrigin.Y;
			double k = ( α * (xD - xA) + β * (yD - yA) ) / ( α*α + β*β );
			return lineOrigin + k * lineVec;
		}
		
		
		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Vecteur v orthogonal à un vecteur u. Il faut définir un troisième point A, pour avoir un plan. Ce point ne doit pas être aligné avec les points définissant u. Le vecteur v est défini par le projeté orthogonal A' de A sur la droite dirigé par u, et passant par les points définissant u, et par le point A. D'où v = A'A.
		/// </summary>
		public static Coord3D GetOrthogonalVectorCoords(Coord3D basePt, Coord3D lineOrigin, Coord3D lineVec)
		{
			// Calcule les coordonnées du projeté orthogonal sur la droite dirigée par le vecteur,
			// puis calcule les coordonnées du vecteur:
			Coord3D projPt = GeoFunctions.GetOrthoProjPointOnLineCoords(basePt, lineOrigin, lineVec);
			return basePt - projPt;
		}
		
		
		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne les coordonnées du vecteur définit par le projeté orthogonal du point sur la droite, et par le point lui-même. Le vecteur retourné peut être nul sans erreur, sauf si lineVec est nul: alors retourne une structure Empty.
		/// </summary>
		public static Coord3D GetNormalVectorToLineCoords(Coord3D pt, Coord3D lineOrigin, Coord3D lineVec)
		{
			if (lineVec.IsNul) { return new Coord3D(true); }
			return pt - GeoFunctions.GetOrthoProjPointOnLineCoords(pt, lineOrigin, lineVec);
		}
		
		
		// ---------------------------------------------------------------------------
		

		/// <summary>
		/// Obtient un vecteur orthogonal et de même norme que vec, sur le plan défini par vec, pt1 et pt2. Si le vecteur défini par pt1 et pt2 est colinéaire à vec, aucun plan n'est défini et retourne Empty. De même si vec est nul, ou si pt1 et pt2 sont confondus. Si invertDir est vrai, la direction du vecteur normal est inversée.
		/// </summary>
		public static Coord3D GetOrthonormalVectorCoords(Coord3D vec, Coord3D pt1, Coord3D pt2, bool invertDir)
		{
			if (vec.IsNul || pt1 == pt2) { return new Coord3D(true); }
			Coord3D vec2 = pt2 - pt1; double vec_3DNorm = vec.GetNorm(), vec2_3DNorm = vec2.GetNorm();
			double α = Math.Acos( (vec * vec2).Sum() / (vec_3DNorm * vec2_3DNorm) ), sinα, vec2_2DNorm;
			if (Maths.Approx(sinα = Math.Sin(α), 0)) { return new Coord3D(true); }
			vec2_2DNorm = vec2_3DNorm / vec_3DNorm;
			return ( vec2 - Math.Cos(α) * vec * vec2_2DNorm ) / (sinα * vec2_2DNorm) * (invertDir ? -1 : 1);
		}
		
		
		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne les coordonnées du barycentre. Les tableaux de points et poids doivent bien sûr correspondre. Retourne une structure vide si la masse du système est nulle.
		/// </summary>
		public static Coord3D GetBarycenterCoords(Coord3D[] points, double[] weights, out double mass)
		{
			// Calcule la masse et sort si nulle:
			if ((mass = weights.Sum()) == 0) { return new Coord3D(true); }
			// Additionne les coordonnées de vecteurs:
			Coord3D result = new Coord3D(false); int l = points.Length;
			for (int i=0; i<l; i++) { result += (weights[i] / mass) * points[i]; }
			return result;
		}
		
		
		/// <summary>
		/// Retourne les coordonnées de l'isobarycentre. La masse est égale au nombre de points...
		/// </summary>
		public static Coord3D GetBarycenterCoords(params Coord3D[] points)
		{
			// Sort si pas de points:
			double mass = points.Length;
			if (mass == 0) { return new Coord3D(true); }
			// Additionne les coordonnées de vecteurs:
			Coord3D result = new Coord3D();
			for (int i=0; i<mass; i++) { result += (1 / mass) * points[i]; }
			return result;
		}
		

		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne le périmètre du polygone définit par les points. Ceux-ci sont supposés coplanaires. Le repère utilisé est supposé coplainaires. Si pts a une longueur de 0, une exception est levée.
		/// </summary>
		public static double GetPolygonPerimeter(params Coord3D[] pts)
		{
			double peri = 0; int l = pts.Length;
			for (int i=0; i<l-1; i++) { peri += pts[i].GetLength(pts[i+1]); }
			peri += pts[l-1].GetLength(pts[0]);
			return peri;
		}

		/// <summary>
		/// Retourne le périmètre du polygone définit par les points. Ceux-ci sont supposés coplanaires. Le repère utilisé est supposé orthonormal. Si pts a une longueur de 0, une exception est levée.
		/// </summary>
		public static double GetPolygonPerimeter(params Coord2D[] pts)
		{
			double peri = 0; int l = pts.Length;
			for (int i=0; i<l-1; i++) { peri += pts[i].GetLength(pts[i+1]); }
			peri += pts[l-1].GetLength(pts[0]);
			return peri;
		}

		/// <summary>
		/// Retourne l'aire du polygone défini par les points. Ceux-ci sont supposés coplanaires. Le repère utilisé est supposé orthonormal. Si le polygone n'est pas convexe, le résultat sera calculé, mais faux.
		/// </summary>
		public static double GetConvexPolygonArea(params Coord3D[] pts)
		{
			// Découpe le polygone en triangles, et additionne les aires de ceux-ci:
			double area = 0, a = 0, b = 0, c = 0; int l = pts.Length;
			for (int i=1; i<l-1; i++)
			{
				a = (i==1 ? pts[0].GetLength(pts[i]) : b);
				b = pts[0].GetLength(pts[i+1]);
				c = pts[i].GetLength(pts[i+1]);
				// Si l'une des valeurs vaut 0, c'est que l'aire est nulle: passe à la suite:
				if (a == 0 || b == 0 || c == 0) { continue; }
				area += My.MathsGeo.GetArea(a, b, c);
			}
			return area;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne la mesure d'un angle défini par deux vecteurs. Si value est vrai, retourne la valeur de l'angle (en degrés ou en radians, selon rad), sinon retourne le cosinus de l'angle. L'angle ainsi calculé est toujours inférieur ou égal à 180°. Une exception est levée si l'un des vecteurs au moins est nul.
		/// </summary>
		public static double GetAngleMeasure(Coord3D vertex, Coord3D spt1, Coord3D spt2, bool value, bool rad)
		{
			return GetAngleMeasure(spt1 - vertex, spt2 - vertex, value, rad);
		}

		/// <summary>
		/// Retourne la mesure d'un angle défini par deux vecteurs. Si value est vrai, retourne la valeur de l'angle (en degrés ou en radians, selon rad), sinon retourne le cosinus de l'angle. L'angle ainsi calculé est toujours inférieur ou égal à 180°. Une exception est levée si l'un des vecteurs au moins est nul.
		/// </summary>
		public static double GetAngleMeasure(Coord3D vec1, Coord3D vec2, bool value, bool rad)
		{
			double cosA = Maths.Approx( ( vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z ) / ( vec1.GetNorm() * vec2.GetNorm() ) );
			if (!value) { return cosA; }
			if (value && rad) { return Math.Acos(cosA); }
			return MathsGeo.RadToDeg(Math.Acos(cosA));
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne les coordonnées de l'image d'un point par une rotation, en créant un nouveau plan à partir des points fournis. La valeur de l'angle est en radian.
		/// </summary>
		public static Coord3D GetRotatedPointCoordsCreatingPlane(double radValue, Coord3D center, Coord3D basePt,
			Coord3D planePt)
		{
			Coord3D OpAvec = basePt - center; // Vecteur du centre vers le basePt.
			Coord3D OpApvec = GetOrthonormalVectorCoords(OpAvec, center, planePt, false); // Vecteur orthonormal.
			if (OpApvec.Empty) { return new Coord3D(true); }
			return GetRotatedPointCoordsCreatingPlane(center, OpAvec, OpApvec, radValue);
		}

		/// <summary>
		/// Retourne les coordonnées de l'image d'un point par une rotation, en créant un nouveau plan à partir des points fournis. La valeur de l'angle est en radian. vec1 est le vecteur défini par le centre et le point d'origine. vec1 et vec2 sont deux vecteurs orthogonaux et de même norme.
		/// </summary>
		public static Coord3D GetRotatedPointCoordsCreatingPlane(Coord3D center, Coord3D vec1, Coord3D vec2,
			double radValue)
		{
			return center + Math.Cos(radValue) * vec1 + Math.Sin(radValue) * vec2;
		}
		
		/// <summary>
		/// Retourne les coordonnées de l'image d'un point par une rotation, en créant un nouveau plan à partir des points fournis. La valeur de l'angle est en radian.
		/// </summary>
		public static Coord2D GetRotatedPointCoordsCreatingPlane(double radValue, Coord2D center, Coord2D basePt)
		{
			Coord2D OpAvec = basePt - center;; // Vecteur du centre vers le basePt.
			Coord2D OpApvec = new Coord2D(OpAvec.Y*-1, OpAvec.X); // Vecteur orthonormal.
			return GetRotatedPointCoordsCreatingPlane(center, OpAvec, OpApvec, radValue);
		}

		/// <summary>
		/// Retourne les coordonnées de l'image d'un point par une rotation, en créant un nouveau plan à partir des points fournis. La valeur de l'angle est en radian. vec1 est le vecteur défini par le centre et le point d'origine.
		/// </summary>
		public static Coord2D GetRotatedPointCoordsCreatingPlane(Coord2D center, Coord2D vec1, double radValue)
		{
			Coord2D OpApvec = new Coord2D(vec1.Y*-1, vec1.X); // Vecteur orthonormal.
			return GetRotatedPointCoordsCreatingPlane(center, vec1, OpApvec, radValue);
		}

		/// <summary>
		/// Retourne les coordonnées de l'image d'un point par une rotation, en créant un nouveau plan à partir des points fournis. La valeur de l'angle est en radian. vec1 est le vecteur défini par le centre et le point d'origine. vec1 et vec2 sont deux vecteurs orthogonaux et de même norme.
		/// </summary>
		public static Coord2D GetRotatedPointCoordsCreatingPlane(Coord2D center, Coord2D vec1, Coord2D vec2,
			double radValue)
		{
			return center + Math.Cos(radValue) * vec1 + Math.Sin(radValue) * vec2;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Calcule une série de points sur un arc de cercle (entre min et max, en radian). Les points sont des rotations du point center translaté par xVec, autour de center. xVec et yVec doivent définir un repère orthonormal. step est l'incrément d'angle.
		/// </summary>
		public static Coord3D[] GetRotatedPointsCoordsCreatingPlane(Coord3D xVec, Coord3D yVec, Coord3D center,
			double min, double max, double step)
		{
			// Calcule une série de point sur l'arc de cercle:
			int c = 0; double forMax=max+step;
			Coord3D[] result = new Coord3D[50];
			for (double i=min; i<forMax; i+=step)
			{
				// Si on arrive à la fin, met la dernière valeur sur la ligne (sinon, il y a un blanc),
				// tout en veillant à ce que les deux dernières valeurs ne soit pas trop proche (sinon,
				// le dessin DrawCurve dépasse de la ligne):
				if (i > max) {
					if (forMax-i+step < 0.05) { break; }
					i = max; }
				if (c >= result.Length) { Array.Resize(ref result, c + 30); }
				result[c++] = center + Math.Cos(i) * xVec + Math.Sin(i) * yVec;
			}
			Array.Resize(ref result, c);
			return result;
		}

		/// <summary>
		/// Calcule une série de points sur un arc de cercle (entre min et max, en radian). Les points sont des rotations du point center translaté par xVec, autour de center. xVec et yVec doivent définir un repère orthonormal. step est l'incrément d'angle.
		/// </summary>
		public static Coord2D[] GetRotatedPointsCoordsCreatingPlane(Coord2D xVec, Coord2D yVec, Coord2D center,
			double min, double max, double step)
		{
			// Calcule une série de point sur l'arc de cercle:
			int c = 0; double forMax=max+step;
			Coord2D[] result = new Coord2D[50];
			for (double i=min; i<forMax; i+=step)
			{
				// Si on arrive à la fin, met la dernière valeur sur la ligne (sinon, il y a un blanc),
				// tout en veillant à ce que les deux dernières valeurs ne soit pas trop proche (sinon,
				// le dessin DrawCurve dépasse de la ligne):
				if (i > max) {
					if (forMax-i+step < 0.05) { break; }
					i = max; }
				if (c >= result.Length) { Array.Resize(ref result, c + 30); }
				result[c++] = center + Math.Cos(i) * xVec + Math.Sin(i) * yVec;
			}
			Array.Resize(ref result, c);
			return result;
		}


	
		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne les coordonnées de l'image d'un point par une rotation. La valeur de l'angle est en radian. Les coordonnées retournées sont les coordonnées dans un repère orthonormal du plan.
		/// </summary>
		public static Coord2D GetRotatedPtCoords(Coord2D rotCenter, Coord2D basePt, double radValue)
		{
			double r; double β = MathsGeo.ToPolar(basePt.X - rotCenter.X, basePt.Y - rotCenter.Y, out r);
			Coord2D result = new Coord2D();
			result.X = rotCenter.X + Math.Cos(radValue + β) * r;
			result.Y = rotCenter.Y + Math.Sin(radValue + β) * r;
			return result;
		}

		/// <summary>
		/// Retourne les coordonnées de l'image d'un point par une rotation. La valeur de l'angle est en radian. xVec et yVec sont les deux vecteurs orthonormaux du repère 2D. xVec et yVec sont les deux vecteurs orthonormaux du repère 2D données par leurs coordonnées du repère de l'espace. sysCenter est l'origine du repère 2D. Les coordonnées retournées sont les coordonnées dans le repère de l'espace.
		/// </summary>
		public static Coord3D GetRotatedPtCoords(Coord2D rotCenter, Coord2D basePt, Coord3D sysCenter,
			Coord3D xVec, Coord3D yVec, double radValue)
			{ return Get3DFrom2DCoords(GetRotatedPtCoords(rotCenter, basePt, radValue), sysCenter, xVec, yVec); }


		/// <summary>
		/// Calcule une série de points sur un arc de cercle (entre min et max, en radian). La valeur de l'angle est en radian. xVec et yVec sont les deux vecteurs orthonormaux du repère 2D. xVec et yVec sont les deux vecteurs orthonormaux du repère 2D données par leurs coordonnées du repère de l'espace. sysCenter est l'origine du repère 2D. Les coordonnées retournées sont les coordonnées dans le repère de l'espace.
		/// </summary>
		public static Coord3D[] GetRotatedPtsCoords(Coord2D rotCenter, Coord2D basePt,
			Coord3D sysCenter, Coord3D xVec, Coord3D yVec, double min, double max, double step)
		{
			// Calcule une série de point sur l'arc de cercle:
			int c = 0; double forMax=max+step;
			Coord3D[] result = new Coord3D[50];
			for (double i=min; i<forMax; i+=step)
			{
				// Si on arrive à la fin, met la dernière valeur sur la ligne (sinon, il y a un blanc),
				// tout en veillant à ce que les deux dernières valeurs ne soit pas trop proche (sinon,
				// le dessin DrawCurve dépasse de la ligne):
				if (i > max) {
					if (forMax-i+step < 0.05) { break; }
					i = max; }
				if (c >= result.Length) { Array.Resize(ref result, c + 30); }
				result[c++] = Get3DFrom2DCoords(GetRotatedPtCoords(rotCenter, basePt, i), sysCenter, xVec, yVec);
			}
			Array.Resize(ref result, c);
			return result;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Obtient les coordonnées de l'image de basePt par une rotation d'angle radValue autour de l'axe définie par lineOrigin et lineVec. Retourne Empty si lineVec est null.
		/// </summary>
		public static Coord3D GetAxialRotationPtCoords(Coord3D lineOrigin, Coord3D lineVec, Coord3D basePt, double radValue)
		{
			// Obtient le projeté orthogonal du point sur l'axe:
			Coord3D proj = GetOrthoProjPointOnLineCoords(basePt, lineOrigin, lineVec);
			// Sort si erreur, ou retourne le point si le basePt est sur la droite:
			if (proj.Empty || proj == basePt) { return proj; }
			// Obtient le vecteur normal au plan formé par le vecteur de la droite et par celui formé du point et du projeté):
			Coord3D newVec = basePt - proj; // il faut y aller dans cet ordre, sinon la rotation est décaler de π rad.
			Coord3D normal = GetNormalVectorToPlane(lineVec, newVec);
			// On applique la formule:
			return proj + Math.Cos(radValue) * newVec + Math.Sin(radValue) * (newVec.GetNorm() / normal.GetNorm()) * normal;
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Retourne un point ayant subi une rotation selon trois angles, autour d'un point.
		/// </summary>
		public static Coord3D GetEulerRotatedPtCoords(double ψ, double θ, double φ, Coord3D center, Coord3D pt)
		{
			Coord3D OXp, OYp, OZp; MathsGeo.RotateCoordSystem(ψ, θ, φ, out OXp, out OYp, out OZp);
			Coord3D OAp = pt - center;
			return ( OAp.X * OXp + OAp.Y * OYp + OAp.Z * OZp ) + center;
		}

		/// <summary>
		/// Retourne un point ayant subi une rotation selon trois angles, autour d'un point.
		/// </summary>
		public static Coord3D[] GetEulerRotatedPtCoords(double ψ, double θ, double φ, Coord3D center, params Coord3D[] pts)
		{
			Coord3D OXp, OYp, OZp; MathsGeo.RotateCoordSystem(ψ, θ, φ, out OXp, out OYp, out OZp);
			int l = pts.Length;
			Coord3D[] result = new Coord3D[l]; Coord3D OAp;
			for (int i=0; i<l; i++) {
				OAp = pts[i] - center;
				result[i] = ( OAp.X * OXp + OAp.Y * OYp + OAp.Z * OZp ) + center; }
			return result;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne le nombre d'intersections trouvées (0, 1 ou 2). Les paramètres de sorties contiennent les coordonnées des points d'intersection.
		/// </summary>
		public static int GetLineSphereInterCoords(Coord3D center, double radius, Coord3D lineOrigin, Coord3D lineVec,
			out Coord3D pt1, out Coord3D pt2)
		{
			// Varibles:
			pt1 = new Coord3D(true); pt2 = new Coord3D(true);
			double x_1 = 0, x_2 = 0, y_1 = 0, y_2 = 0, z_1 = 0, z_2 = 0, e, f, g; int solNb;
			double a = center.X, b = center.Y, c = center.Z;
			double α = lineVec.X, β = lineVec.Y, γ = lineVec.Z;
			double x_0 = lineOrigin.X, y_0 = lineOrigin.Y, z_0 = lineOrigin.Z;
			double α2 = α * α, β2 = β * β, γ2 = γ * γ;
			double a2 = a * a, b2 = b * b, c2 = c * c;
			double r = radius, r2 = r * r;
			// Formule:
			if (α == 0 && β == 0 &&  γ == 0)
			{
				return 0;
			}
			else if (α == 0 && β == 0)
			{
				x_1 = x_2 = x_0; y_1 = y_2 = y_0;
				e = 1;
				f = -2 * c;
				g = x_0 * (x_0 - 2 * a) + y_0 * (y_0 - 2 * b) + a2 + b2 + c2 - r2;
				solNb = MathsAlg.SolveTrinomial(e, f, g, out z_1, out z_2);
			}
			else if (α == 0 && γ == 0)
			{
				x_1 = x_2 = x_0; z_1 = z_2 = z_0;
				e = 1;
				f = -2 * b;
				g = x_0 * (x_0 - 2 * a) + z_0 * (z_0 - 2 * c) + a2 + b2 + c2 - r2;
				solNb = MathsAlg.SolveTrinomial(e, f, g, out y_1, out y_2);
			}
			else if (α == 0)
			{
				x_1 = x_2 = x_0;
				e = β2 + γ2;
				f = 2 * ( β * (γ*z_0 - (β*b + γ*c)) - γ2*y_0 );
				g = β2 * ( a2 + b2 + c2 - r2 + x_0*x_0 + z_0*z_0 - 2 * (a*x_0 + c*z_0) )
					+ 2 * β * γ * y_0 * (c - z_0)
					+ γ2 * y_0*y_0;
				solNb = MathsAlg.SolveTrinomial(e, f, g, out y_1, out y_2);
				if (solNb == 1 || solNb == 2) {
					z_1 = z_0 + γ * (y_1 - y_0) / β; }
				if (solNb == 2) {
					z_2 = z_0 + γ * (y_2 - y_0) / β; }
			}
			else
			{
				e = α2 + β2 + γ2;
				f = 2 * ( α * (β*y_0 + γ*z_0 - (α*a + β*b + γ*c)) - x_0 * (β2 + γ2) );
				g = α2 * (a2 + b2 + c2 - r2 + y_0 * (y_0 - 2*b) + z_0 * (z_0 - 2*c))
					+ 2 * α * x_0 * (β * (b - y_0) + γ *  (c - z_0))
					+ x_0 * x_0 *(β2 + γ2);
				solNb = MathsAlg.SolveTrinomial(e, f, g, out x_1, out x_2);
				if (solNb == 1 || solNb == 2) {
					y_1 = y_0 + β * (x_1 - x_0) / α;
					z_1 = z_0 + γ * (x_1 - x_0) / α; }
				if (solNb == 2) {
					y_2 = y_0 + β * (x_2 - x_0) / α;
					z_2 = z_0 + γ * (x_2 - x_0) / α; }
			}
			// On sort selon le nombre de solutions:
			if (solNb == 0) { return 0; }
			pt1 = new Coord3D(x_1, y_1, z_1);
			if (solNb == 1) { return 1; }
			pt2 = new Coord3D(x_2, y_2, z_2);
			return 2;
		}
		
		/// <summary>
		/// Retourne le nombre d'intersections trouvées (0, 1 ou 2). Les paramètres de sorties contiennent les coordonnées des points d'intersection.
		/// </summary>
		public static int GetLineSphereInterCoords(Coord3D center, double radius, Coord3D lineOrigin, Coord3D lineVec,
			double? min, double? max, out Coord3D pt1, out Coord3D pt2)
		{
			pt1 = new Coord3D(true); pt2 = new Coord3D(true);
			Coord3D test1, test2; double t; int result = 0;
			int solNb = GetLineSphereInterCoords(center, radius, lineOrigin, lineVec, out test1, out test2);
			// Si pas de solution, sort:
			if (solNb == 0) { return 0; }
			// Si une solution, on regarde le premier point:
			t = GetPointOnLineTParam(test1, lineOrigin, lineVec);
			if ((min == null || t >= 0) && (max == null) || t <= max) { pt1 = test1; result = 1; }
			if (solNb == 1) { return result; }
			// Si deux solutions, on regarde le second point:
			t = GetPointOnLineTParam(test2, lineOrigin, lineVec);
			if ((min == null || t >= 0) && (max == null) || t <= max)
			{
				if (result == 0) { pt1 = test2; result = 1; }
				else { pt2 = test2; result = 2; }
			}
			return result;
		}
		
		
		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne s'il y a une intersection entre deux lignes (droite, segment, demi-droite). Les min et max indique les limites du paramètre t (null et null pour une droite, 0 et 1 pour un segment, 0 et null pour une demi-droite) pour chacune des lignes. Retourne false si les droites sont parallèles.
		/// </summary>
		public static bool GetLinesInterCoords(Coord3D lineOrigin1, Coord3D lineVec1, Coord3D lineOrigin2, Coord3D lineVec2,
			double? min1, double? max1, double? min2, double? max2, out Coord3D interPt, out double t, out double tp)
		{
			// Sort si parallèle:
			t = 0; tp = 0;
			if (AreCollinear(lineVec1, lineVec2)) { interPt = new Coord3D(true); return false; }
			// Dans les équations paramétriques des deux droites, on cherche les paramètres t et t' en résolvant un système
			// de trois équations à deux inconnues: αt-α't'=x_0'-x_0, et ainsi pour β et γ:
			double[,] system = new double[3,3];
			system[0,0] = lineVec1.X; system[0,1] = -lineVec2.X; system[0,2] = lineOrigin2.X - lineOrigin1.X;
			system[1,0] = lineVec1.Y; system[1,1] = -lineVec2.Y; system[1,2] = lineOrigin2.Y - lineOrigin1.Y;
			system[2,0] = lineVec1.Z; system[2,1] = -lineVec2.Z; system[2,2] = lineOrigin2.Z - lineOrigin1.Z;
			double[] sol = My.MathsAlg.SolveSimul(system);
			// On a t=sol[0] et t'=sol[1]. Si t et t' ne sont pas compris entre min et max, on sort:
			if (sol == null || (sol[0] < min1 || sol[0] > max1) || (sol[1] < min2 || sol[1] > max2))
					{ interPt = new Coord3D(true); return false; }
			// Il ne reste plus qu'à choisir le système paramétrie de line1, e.g., et de trouver les coordonnées:
			t = sol[0]; tp = sol[1];
			interPt = lineOrigin1 + t * lineVec1;
			return true;
		}
		
		
		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne s'il y a une intersection entre une ligne (droite, segment, demi-droite) et un plan. Les min et max indique les limites du paramètre t (null et null pour une droite, 0 et 1 pour un segment, 0 et null pour une demi-droite) pour la ligne. Retourne false si pas d'intersection.
		/// </summary>
		public static bool GetPlaneLineInterCoords(Coord3D planeOrigin, Coord3D xVec, Coord3D yVec, Coord3D lineOrigin, Coord3D lineVec,
			double? min, double? max, out Coord2D interPt)
		{
			// On remplit le système pour trouver x, y et le paramètre de la droite:
			double[,] system = new double[3,4];
			system[0,0] = xVec.X; system[0,1] = yVec.X; system[0,2] = -lineVec.X; system[0,3] = lineOrigin.X - planeOrigin.X;
			system[1,0] = xVec.Y; system[1,1] = yVec.Y; system[1,2] = -lineVec.Y; system[1,3] = lineOrigin.Y - planeOrigin.Y;
			system[2,0] = xVec.Z; system[2,1] = yVec.Z; system[2,2] = -lineVec.Z; system[2,3] = lineOrigin.Z - planeOrigin.Z;
			double[] sol = My.MathsAlg.SolveSimul(system);
			// On a x=sol[0], y=sol[1], t=sol[2]:
			if (sol == null || sol[2] < min || sol[2] > max) { interPt = new Coord2D(true); return false; }
			interPt = new Coord2D(sol[0], sol[1]);
			return true;
		}
		
		
		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne le nombre d'intersections trouvées (0, 1 ou 2). Les paramètres de sorties contiennent les coordonnées des points d'intersection. Le cercle étant sur un plan, ce dernier est définit par origin, vec1 (abscisses) et vec2 (ordonnées). Les coordonnées du centre du cercle et des points d'intersection sont données dans l'espace.
		/// </summary>
		public static int GetLineCircleInterCoords(Coord3D center, double radius, Coord3D lineOrigin, Coord3D lineVec,
			Coord3D origin, Coord3D vec1, Coord3D vec2, out Coord3D pt1, out Coord3D pt2)
		{
			// Sortie par défaut:
			pt1 = new Coord3D(true); pt2 = new Coord3D(true); int result = 0;
			// Un cercle et l'intersection entre un plan et une sphère. Donc on cherche les intersections de la droite
			// avec la sphère qu'on peut définir avec le centre et le rayon du cecle, et on regarde si l'un ou les points
			// trouvés sont sur le plan du cercle.
			// Cherdche les points d'intersection:
			Coord3D test1, test2;
			int solNb = GeoFunctions.GetLineSphereInterCoords(center, radius, lineOrigin, lineVec, out test1, out test2);
			// Si pas de solution, sort:
			if (solNb == 0) { return 0; }
			// Si une solution, on regarde le premier point (on regarde si les vecteurs sont coplanaires):
			if (GeoFunctions.AreCoplanar(vec1, vec2, test1 - origin)) { pt1 = test1; result = 1; }
			if (solNb == 1) { return result; }
			// Si deux solutions, on regarde le second point:
			if (GeoFunctions.AreCoplanar(vec1, vec2, test2 - origin))
			{
				if (result == 0) { pt1 = test2; result = 1; }
				else { pt2 = test2; result = 2; }
			}
			return result;
		}

		/// <summary>
		/// Retourne le nombre d'intersections trouvées (0, 1 ou 2). Les paramètres de sorties contiennent les coordonnées des points d'intersection. Le cercle étant sur un plan, ce dernier est définit par origin, vec1 (abscisses) et vec2 (ordonnées). Les coordonnées du centre du cercle et des points d'intersection sont données dans l'espace.
		/// </summary>
		public static int GetLineCircleInterCoords(Coord3D center, double radius, Coord3D lineOrigin, Coord3D lineVec,
			Coord3D origin, Coord3D vec1, Coord3D vec2, double? min, double? max,
			out Coord3D pt1, out Coord3D pt2)
		{
			pt1 = new Coord3D(true); pt2 = new Coord3D(true);
			Coord3D test1, test2; double t; int result = 0;
			int solNb = GetLineCircleInterCoords(center, radius, lineOrigin, lineVec, origin, vec1, vec2, out test1, out test2);
			// Si pas de solution, sort:
			if (solNb == 0) { return 0; }
			// Si une solution, on regarde le premier point:
			t = GetPointOnLineTParam(test1, lineOrigin, lineVec);
			if ((min == null || t >= 0) && (max == null) || t <= max) { pt1 = test1; result = 1; }
			if (solNb == 1) { return result; }
			// Si deux solutions, on regarde le second point:
			t = GetPointOnLineTParam(test2, lineOrigin, lineVec);
			if ((min == null || t >= 0) && (max == null) || t <= max)
			{
				if (result == 0) { pt1 = test2; result = 1; }
				else { pt2 = test2; result = 2; }
			}
			return result;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne le paramètre t dans un système d'équations paramétriques de droite. Si le vecteur directeur est nul, retourne 0.
		/// </summary>
		public static double GetPointOnLineTParam(Coord3D pt, Coord3D lineOrigin, Coord3D lineVec)
		{
			if (lineVec.X != 0) { return (pt.X - lineOrigin.X) / lineVec.X; }
			else if (lineVec.Y != 0) { return (pt.Y - lineOrigin.Y) / lineVec.Y; }
			else if (lineVec.Z != 0) { return (pt.Z - lineOrigin.Z) / lineVec.Z; }
			return 0;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Obtient les coordonnées 2D d'un point à partir de ses coordonnées 3D dans le repère général de l'espace. Le plan est défini par deux vecteurs (vec1 pour les abscisses et vec2 pour les odonnées) et une origine. Retourne une structeur IsEmpty si le point n'appartient pas au plan.
		/// </summary>
		public static Coord2D Get2DFrom3DCoords(Coord3D pt, Coord3D origin, Coord3D vec1, Coord3D vec2)
		{
			// Calcul les coordonnées du vecteur O'M (de l'origine du repère 2D au pt considéré):
			Coord3D OpM = pt - origin;
			// Tente de résoudre le système de trois équations à deux inconnues:
			double[,] system = new double[3,3];
			system[0,0] = vec1.X; system[0,1] = vec2.X; system[0,2] = OpM.X;
			system[1,0] = vec1.Y; system[1,1] = vec2.Y; system[1,2] = OpM.Y;
			system[2,0] = vec1.Z; system[2,1] = vec2.Z; system[2,2] = OpM.Z;
			double[] sol = My.MathsAlg.SolveSimul(system);
			if (sol == null) { return new Coord2D(true); }
			return new Coord2D(sol[0], sol[1]);
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Obtient les coordonnées 3D du repère général à partir des coordonnées 2D de ce repère. Le plan est défini par deux vecteurs (vec1 pour les abscisses et vec2 pour les odonnées) et une origine.
		/// </summary>
		public static Coord3D Get3DFrom2DCoords(Coord2D pt, Coord3D origin, Coord3D vec1, Coord3D vec2)
		{
			return origin + pt.X * vec1 + pt.Y * vec2;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Le vecteur normal d'un plan définis par deux vecteurs (ils doivent être non colinéaires et non nuls, sinon le résultat retourné est aberrant).
		/// </summary>
		public static Coord3D GetNormalVectorToPlane(Coord3D vec1, Coord3D vec2)
		{
			return new Coord3D(vec2.Z * vec1.Y - vec1.Z * vec2.Y,
				vec1.Z * vec2.X - vec2.Z * vec1.X,
				vec1.X * vec2.Y - vec2.X * vec1.Y);
		}
		
		
		// ---------------------------------------------------------------------------
		
	
		/// <summary>
		/// Retourne les coordonnées du projeté orthogonal d'un point sur un plan. Retourne Empty si le vecteur normal est nul.
		/// </summary>
		public static Coord3D GetOrthoProjPointOnPlaneCoords(Coord3D basePt, Eq3Zero plane, Coord3D normalVec)
		{
			if (normalVec.IsNul) { return new Coord3D(true); }
			double t = - ( plane.a * basePt.X + plane.b * basePt.Y + plane.c * basePt.Z + plane.d ) /
				( plane.a*plane.a + plane.b*plane.b + plane.c*plane.c );
			return basePt + t * normalVec;
		}
		
		
		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne en paramètre de sortie les coordonnées d'un vecteur directeur de l'intersection de deux plans, ainsi que un point de la droite. Si les plans sont parallèles ou confondus, retourne false.
		/// </summary>
		public static bool GetPlanesInterCoords(Eq3Zero p1, Eq3Zero p2, out Coord3D lineVec, out Coord3D linePt)
		{
			linePt = new Coord3D(); lineVec = new Coord3D();
			if (AreParallel(p1, p2)) { return false; }
			// On applique la formule pour trouver α, β et γ:
			lineVec.X = p2.b * p1.c - p1.b * p2.c;
			lineVec.Y = p2.c * p1.a - p1.c * p2.a;
			lineVec.Z = p2.a * p1.b - p1.a * p2.b;
			// Trouve un point sur la droite, c'est-à-dire un point coupant l'un des trois plans du repère de l'espace:
			if (lineVec.X != 0) {
				linePt.X = 0;
				linePt.Y = (p2.c * p1.d - p1.c * p2.d) / lineVec.X;
				linePt.Z = (p2.d * p1.b - p1.d * p2.b) / lineVec.X; }
			else if (lineVec.Y != 0) {
				linePt.X = (p2.d * p1.c - p1.d * p2.c) / lineVec.Y;;
				linePt.Y = 0;
				linePt.Z = (p2.a * p1.d - p1.a * p2.d) / lineVec.Y; }
			else if (lineVec.Z != 0) {
				linePt.X = (p2.b * p1.d - p1.b * p2.d) / lineVec.Z;;
				linePt.Y = (p2.d * p1.a - p1.d * p2.a) / lineVec.Z;
				linePt.Z = 0; }
			return true;
		}
		
		
		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne en paramètre de sortie les coordonnées 2D du centre et le rayon du cercle formant l'intersection d'un plan et d'une sphère. Retourne false si l'intersection n'existe pas. Le plan est défini par un point d'origine et deux vecteurs. La sphère par son centre et son rayon. Les vecteurs définissant le plan ne doivent pas être nuls (sinon une exception est levée), et ils doivent être orthonormaux (sinon, le résultat sera aberrant).
		/// </summary>
		public static bool GetPlaneSphereInterCoords(Coord3D origin, Coord3D xVec, Coord3D yVec, Coord3D center, double radius,
			out Coord2D cirCenter, out double cirRadius)
		{
			double a = xVec.X*xVec.X + xVec.Y*xVec.Y + xVec.Z*xVec.Z;
			double b = origin.X - center.X + xVec.Z * yVec.Y / 2 - yVec.Z * xVec.Y / 2;
			double c = origin.Y - center.Y + xVec.X * yVec.Z / 2 - yVec.X * xVec.Z / 2;
			double d = origin.Z - center.Z + xVec.Y * yVec.X / 2 - yVec.Y * xVec.X / 2;
			cirCenter = new Coord2D();
			cirCenter.X = -(xVec.X * b + xVec.Y * c + xVec.Z * d) / a;
			cirCenter.Y = -(yVec.X * b + yVec.Y * c + yVec.Z * d) / a;
			// Calcule r²:
			cirRadius = ( ( Math.Pow(origin.X - center.X, 2) + Math.Pow(origin.Y - center.Y, 2) + Math.Pow(origin.Z - center.Z, 2)
				- Math.Pow(radius, 2) ) / a - Math.Pow(cirCenter.X, 2) - Math.Pow(cirCenter.Y, 2) ) * -1;
			// Si carré négatif, on sort:
			if (cirRadius < 0) { return false; }
			cirRadius = Math.Sqrt(cirRadius);
			return true;
		}
		
		
		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne l'équation cartésienne d'un plan défini par un point et deux vecteurs. Retourne Empty si le plan n'est pas défini (vec1 et vec2 colinéaires).
		/// </summary>
		public static Eq3Zero GetPlaneCartesianEquation(Coord3D origin, Coord3D xVec, Coord3D yVec)
		{
			Eq3Zero eq = new Eq3Zero();
			eq.a = yVec.Z * xVec.Y - xVec.Z * yVec.Y;
			eq.b = xVec.Z * yVec.X - yVec.Z * xVec.X;
			eq.c = xVec.X * yVec.Y - yVec.X * xVec.Y;
			eq.d = - ( eq.a * origin.X + eq.b * origin.Y + eq.c * origin.Z );
			if (eq.a == 0 && eq.b == 0 && eq.c == 0) { return new Eq3Zero(true); }
			return eq;
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Passe d'un repère 2D à un autre, du repère général au repère secondaire (les deux sur le même plan, bien entendu). O est l'origine du repère général (toujours (0,0)), Op est l'origine du repère secondaire, Ip et Jp les points I et J du repère secondaire, et A est le point dont il faut changer les coordonnées. Retourne Empty en cas d'erreur.
		/// </summary>
		public static Coord2D ChangeCoordinatesSystem(Coord2D OOp, Coord2D OpIp, Coord2D OpJp, Coord2D OA)
		{
			double[,] system = new double[2,3];
			system[0,0] = OpIp.X; system[0,1] = OpJp.X; system[0,2] = OA.X - OOp.X;
			system[1,0] = OpIp.Y; system[1,1] = OpJp.Y; system[1,2] = OA.Y - OOp.Y;
			double[] sol = MathsAlg.SolveSimul(system);
			if (sol == null) { return new Coord2D(true); }
			return new Coord2D(sol[0], sol[1]);
		}


		
		
		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne le tableau passé en argument, mais en ayant enlevé les points confondus.
		/// </summary>
		/*public static Coord3D[] RemoveCoincidentPoints(params Coord3D[] pts)
		{
			int l = pts.Length, c = 0; bool found;
			Coord3D[] result = new Coord3D[l];
			for (int i=0; i<l; i++)
			{
				found = false;
				for (int j=0; j<i; j++) { if (pts[i] == pts[j]) { found = true; break; } }
				if (!found) { result[c++] = pts[i]; }
			}
			Array.Resize(ref result, c);
			return result;
		} */

	

		#endregion METHODES DE CALCUL




		// ---------------------------------------------------------------------------
		// METHODES POUR LES FORMULES
		// ---------------------------------------------------------------------------




		#region METHODES POUR LES FORMULES



		/// <summary>
		/// Retourne les coordonnées d'un vecteur à partir de deux points.
		/// </summary>
		/*public static Coord3D GetVectorCoords(Coord3D pt1, Coord3D pt2)
		{
			return new Coord3D(pt2.X - pt1.X, pt2.Y - pt1.Y, pt2.Z - pt1.Z);
		}

		/// <summary>
		/// Retourne les coordonnées d'un vecteur à partir de deux points.
		/// </summary>
		public static Coord2D GetVectorCoords(Coord2D pt1, Coord2D pt2)
		{
			return new Coord2D(pt2.X - pt1.X, pt2.Y - pt1.Y);
		}

		/// <summary>
		/// Retourne les coordonnées d'un vecteur à partir de deux points, et d'un rapport.
		/// </summary>
		public static Coord3D GetVectorCoords(double k, Coord3D pt1, Coord3D pt2)
		{
			return new Coord3D((pt2.X - pt1.X) * k, (pt2.Y - pt1.Y) * k, (pt2.Z - pt1.Z) * k);
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne les coordonnées d'un vecteur multiplié par un réel.
		/// </summary>
		public static Coord3D GetMulVectorCoords(double k, Coord3D vec)
		{
			return new Coord3D(vec.X * k, vec.Y * k, vec.Z * k);
		}


		// ---------------------------------------------------------------------------
		

		/// <summary>
		/// Retourne les coordonnées d'un point image du point pt1 par la translation de vecteur vec.
		/// </summary>
		public static Coord3D GetTranslatedPointCoords(Coord3D pt1, Coord3D vec)
		{
			return new Coord3D(pt1.X + vec.X, pt1.Y + vec.Y, pt1.Z + vec.Z);
		}

		/// <summary>
		/// Retourne les coordonnées d'un point image du point pt1 par la translation de vecteur vec.
		/// </summary>
		public static Coord2D GetTranslatedPointCoords(Coord2D pt1, Coord2D vec)
		{
			return new Coord2D(pt1.X + vec.X, pt1.Y + vec.Y);
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne la norme du vecteur.
		/// </summary>
		public static double GetVectorNorm(Coord3D vec)
		{
			return Math.Sqrt(vec.X*vec.X + vec.Y*vec.Y + vec.Z*vec.Z);
		}
		
		/// <summary>
		/// Retourne la norme du vecteur définit par deux points.
		/// </summary>
		public static double GetVectorNorm(Coord3D pt1, Coord3D pt2)
		{
			return GetVectorNorm(GetVectorCoords(pt1, pt2));
		}
		
		/// <summary>
		/// Retourne la norme du vecteur.
		/// </summary>
		public static double GetVectorNorm(Coord2D vec)
		{
			return Math.Sqrt(vec.X*vec.X + vec.Y*vec.Y);
		}
		
		/// <summary>
		/// Retourne la norme du vecteur définit par deux points.
		/// </summary>
		public static double GetVectorNorm(Coord2D pt1, Coord2D pt2)
		{
			return GetVectorNorm(GetVectorCoords(pt1, pt2));
		}
		
		
		// ---------------------------------------------------------------------------
		

		/// <summary>
		/// Retourne la longueur du segment. Le repère utilisé est supposé orthonormal, si bien qu'on passe simplement par la norme du vecteur défini par les deux points.
		/// </summary>
		public static double GetSegmentLength(Coord3D pt1, Coord3D pt2)
		{
			return GetVectorNorm(pt1, pt2);
		}

		/// <summary>
		/// Retourne la longueur du segment. Le repère utilisé est supposé orthonormal, si bien qu'on passe simplement par la norme du vecteur défini par les deux points.
		/// </summary>
		public static double GetSegmentLength(Coord2D pt1, Coord2D pt2)
		{
			return GetVectorNorm(pt1, pt2);
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne les coordonnées du milieu de deux points.
		/// </summary>
		public static Coord3D GetMidpointCoords(Coord3D pt1, Coord3D pt2)
		{
			return (pt1 + pt2) / 2;
		}*/




		#endregion METHODES POUR LES FORMULES


	}



}
