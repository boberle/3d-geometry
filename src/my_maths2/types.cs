using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My
{


	// ---------------------------------------------------------------------------
	// STRUCTURES:


	/// <summary>
	/// Fournit trois champs pour les trois coordonnées de l'espace.
	/// </summary>
	public struct Coord3D
	{
	
		// Champs publiques:
		
		public double X;
		public double Y;
		public double Z;
		public bool Empty;
		
		// Constructeurs:
		
		public Coord3D(bool empty) : this() { Empty = empty; }
		
		public Coord3D(double x, double y, double z) : this()
			{ X = x; Y = y; Z = z; Empty = false; }
		
		// Méthodes:
		
		public override string ToString()
			{ return (Empty ? "Empty" : String.Format("({0},{1},{2})", X, Y, Z)); }
		
		public override bool Equals(object obj)
			{ return (obj is Coord3D && this == (Coord3D)obj); }
			
		public override int GetHashCode()
			{ return (int)(X+Y+Z); }
		
		public Coord3D Square()
			{ return new Coord3D(X*X, Y*Y, Z*Z); }
		
		public Coord3D Sqrt()
			{ return new Coord3D(Math.Sqrt(X), Math.Sqrt(Y), Math.Sqrt(Z)); }
		
		public double AddXYZ()
			{ return X + Y + Z; }
		
		public double GetNorm()
			{ return Math.Sqrt(this.Square().AddXYZ()); }
		
		public double GetLength(Coord3D obj)
			{ return Math.Sqrt((this - obj).Square().AddXYZ()); }
		
		public double Sum()
			{ return X + Y + Z; }
		
		public bool IsNul { get { return (X == 0 && Y == 0 && Z == 0); } }
		
		// Opérateurs:
		
		public static bool operator ==(Coord3D obj1, Coord3D obj2)
			{ return (obj1.X == obj2.X && obj1.Y == obj2.Y && obj1.Z == obj2.Z); }
		
		public static bool operator !=(Coord3D obj1, Coord3D obj2)
			{ return obj1 != obj2; }

		public static Coord3D operator +(Coord3D obj1, Coord3D obj2)
			{ return new Coord3D(obj1.X + obj2.X, obj1.Y + obj2.Y, obj1.Z + obj2.Z); }

		public static Coord3D operator +(double k, Coord3D obj1)
			{ return new Coord3D(obj1.X + k, obj1.Y + k, obj1.Z + k); }

		public static Coord3D operator +(Coord3D obj1, double k)
			{ return new Coord3D(obj1.X + k, obj1.Y + k, obj1.Z + k); }

		public static Coord3D operator -(Coord3D obj1, Coord3D obj2)
			{ return new Coord3D(obj1.X - obj2.X, obj1.Y - obj2.Y, obj1.Z - obj2.Z); }

		public static Coord3D operator *(Coord3D obj1, Coord3D obj2)
			{ return new Coord3D(obj1.X * obj2.X, obj1.Y * obj2.Y, obj1.Z * obj2.Z); }

		public static Coord3D operator *(Coord3D obj1, double k)
			{ return new Coord3D(obj1.X * k, obj1.Y * k, obj1.Z * k); }

		public static Coord3D operator *(double k, Coord3D obj1)
			{ return new Coord3D(k * obj1.X, k * obj1.Y, k * obj1.Z); }

		public static Coord3D operator /(Coord3D obj1, Coord3D obj2)
			{ return new Coord3D(obj1.X / obj2.X, obj1.Y / obj2.Y, obj1.Z / obj2.Z); }

		public static Coord3D operator /(Coord3D obj1, double k)
			{ return new Coord3D(obj1.X / k, obj1.Y / k, obj1.Z / k); }

		public static Coord3D operator /(double k, Coord3D obj1)
			{ return new Coord3D(k / obj1.X, k / obj1.Y, k / obj1.Z); }

	}

	/// <summary>
	/// Fournit deux champs pour les deux coordonnées du plan.
	/// </summary>
	public struct Coord2D
	{
	
		// Champs publiques:
		
		public double X;
		public double Y;
		public bool Empty;
		
		// Constructeurs:
		
		public Coord2D(bool empty) : this() { Empty = empty; }
		
		public Coord2D(double x, double y) : this()
			{ X = x; Y = y;; Empty = false; }
		
		// Méthodes:
		
		public override string ToString()
			{ return (Empty ? "Empty" : String.Format("({0},{1})", X, Y)); }
		
		public override bool Equals(object obj)
			{ return (obj is Coord2D && this == (Coord2D)obj); }
			
		public override int GetHashCode()
			{ return (int)(X+Y); }
		
		public Coord2D Square()
			{ return new Coord2D(X*X, Y*Y); }
		
		public Coord2D Sqrt()
			{ return new Coord2D(Math.Sqrt(X), Math.Sqrt(Y)); }
		
		public double AddXY()
			{ return X + Y; }
		
		public double GetNorm()
			{ return Math.Sqrt(this.Square().AddXY()); }
		
		public double GetLength(Coord2D obj)
			{ return Math.Sqrt((this - obj).Square().AddXY()); }

		public double Sum()
			{ return X + Y; }

		public bool IsNul { get { return (X == 0 && Y == 0); } }
		
		// Opérateurs:
		
		public static bool operator ==(Coord2D obj1, Coord2D obj2)
			{ return (obj1.X == obj2.X && obj1.Y == obj2.Y); }
		
		public static bool operator !=(Coord2D obj1, Coord2D obj2)
			{ return obj1 != obj2; }

		public static Coord2D operator +(Coord2D obj1, Coord2D obj2)
			{ return new Coord2D(obj1.X + obj2.X, obj1.Y + obj2.Y); }

		public static Coord2D operator +(double k, Coord2D obj1)
			{ return new Coord2D(obj1.X + k, obj1.Y + k); }

		public static Coord2D operator +(Coord2D obj1, double k)
			{ return new Coord2D(obj1.X + k, obj1.Y + k); }

		public static Coord2D operator -(Coord2D obj1, Coord2D obj2)
			{ return new Coord2D(obj1.X - obj2.X, obj1.Y - obj2.Y); }

		public static Coord2D operator *(Coord2D obj1, Coord2D obj2)
			{ return new Coord2D(obj1.X * obj2.X, obj1.Y * obj2.Y); }

		public static Coord2D operator *(Coord2D obj1, double k)
			{ return new Coord2D(obj1.X * k, obj1.Y * k); }

		public static Coord2D operator *(double k, Coord2D obj1)
			{ return new Coord2D(k * obj1.X, k * obj1.Y); }

		public static Coord2D operator /(Coord2D obj1, Coord2D obj2)
			{ return new Coord2D(obj1.X / obj2.X, obj1.Y / obj2.Y); }

		public static Coord2D operator /(Coord2D obj1, double k)
			{ return new Coord2D(obj1.X / k, obj1.Y / k); }

		public static Coord2D operator /(double k, Coord2D obj1)
			{ return new Coord2D(k / obj1.X, k / obj1.Y); }

	}


	// ---------------------------------------------------------------------------

	
	/// <summary>
	/// Structure pour une équation à deux inconnues (ou deux variables), de la forme ax + by + c = 0.
	/// </summary>
	public struct Eq2Zero
	{
		
		// Champs publiques:
		
		public double a;
		public double b;
		public double c;
		public bool Empty;
		
		// Constructeurs:
		
		public Eq2Zero(bool empty) : this() { Empty = empty; }
		
		public Eq2Zero(double a, double b, double c) : this()
			{ this.a = a; this.b = b; this.c = c; Empty = false; }
		
		// Méthodes:
		
		public override string ToString()
		{
			if (Empty) { return "Empty"; }
			return String.Format("{0}x {1} {2}y {3} {4} = 0", a, (b<0?"-":"+"), Math.Abs(b), (c<0?"-":"+"), Math.Abs(c));
		}
		
		public string ToString(string format)
		{
			if (Empty) { return "Empty"; }
			return String.Format("{0}x {1} {2}y {3} {4} = 0", a.ToString(format), (b<0?"-":"+"), Math.Abs(b).ToString(format),
				(c<0?"-":"+"), Math.Abs(c).ToString(format));
		}
		
		/// <summary>
		/// Multiplie par -1 si a est négatif. Sinon, ne fait rien.
		/// </summary>
		public void MultiplyMinusOne()
			{ if (a < 0) { a *= -1; b *= -1; c *= -1; } }
		
	}
		

	// ---------------------------------------------------------------------------

	
	/// <summary>
	/// Structure pour une équation à trois inconnues (ou trois variables), de la forme ax + by + cz + d = 0.
	/// </summary>
	public struct Eq3Zero
	{
		
		// Champs publiques:
		
		public double a;
		public double b;
		public double c;
		public double d;
		public bool Empty;
		
		// Constructeurs:
		
		public Eq3Zero(bool empty) : this() { Empty = empty; }
		
		public Eq3Zero(double a, double b, double c, double d) : this()
			{ this.a = a; this.b = b; this.c = c; this.d = d; Empty = false; }
		
		// Méthodes:
		
		public override string ToString()
		{
			if (Empty) { return "Empty"; }
			return String.Format("{0}x {1} {2}y {3} {4}z {5} {6} = 0", a, (b<0?"-":"+"), Math.Abs(b), (c<0?"-":"+"), Math.Abs(c),
				(d<0?"-":"+"), Math.Abs(d));
		}
		
		public string ToString(string format)
		{
			if (Empty) { return "Empty"; }
			return String.Format("{0}x {1} {2}y {3} {4}z {5} {6} = 0", a.ToString(format), (b<0?"-":"+"), Math.Abs(b).ToString(format),
				(c<0?"-":"+"), Math.Abs(c).ToString(format), (d<0?"-":"+"), Math.Abs(d).ToString(format));
		}
		
		/// <summary>
		/// Multiplie par -1 si a est négatif. Sinon, ne fait rien.
		/// </summary>
		public void MultiplyMinusOne()
			{ if (a < 0) { a *= -1; b *= -1; c *= -1; d *= -1; } }
		
	}
		

	// ---------------------------------------------------------------------------
	// ATTRIBUTS:

	
	/// <summary>
	/// Indique si une fonction pour les formules doit être incluses dans le manuel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ExcludeFromManAttribute : Attribute
		{ public ExcludeFromManAttribute() {} }
	
	/// <summary>
	/// Enumération pour les types de fonctions pour formules.
	/// </summary>
	public enum FormulaFuncType
	{
		Statements, Algebra, Geometry, GetObject, Other, GetProperty, General
	}
	
	/// <summary>
	/// Indique les catégories d'une fonction pour formules.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class FormulaFunctionCategoriesAttribute : Attribute
	{
		public string[] Path { get; private set; }
		public FormulaFunctionCategoriesAttribute(params string[] path)
			{ Path = path; }
	}
	
		
		
}
