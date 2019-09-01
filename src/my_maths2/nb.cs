/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My
{




	[Serializable()]
	public class Nb
	{





		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS
		
		
		const long max = 999999999;
		
		
		protected long[] _intPart;
		protected long[] _decPart;
		
		protected bool _isNul;
		



		#endregion DECLARATIONS










		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES
		
		
		
		
		
		
		public bool IsNul { get { return _isNul; } }
		
		public bool IsNegative { get; set; }
		
		
		public long[] IntPart { get { return _intPart; } }
		public long[] DecPart { get { return _decPart; } }
		



		#endregion PROPRIETES












		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS
		
		
		
		
		
		public Nb()
		{
			// Initialisation des variables:
			_isNul = true;
			this.IsNegative = false;
			_intPart = new long[1];
			_decPart = new long[0];
		}
		
		
		
		public Nb(long n)
		{
			_isNul = (n==0 ? true : false);
			this.IsNegative = (n<0 ? true : false);
			
			
			string sn = ReverseString(n.ToString());
			_intPart = new long[(int)Math.Floor(sn.Length / 9.0) + 1];
			int c = 0; string tmp = sn;
			while (sn.Length > 0)
			{
				tmp = sn.Substring(0, (sn.Length-9<=0 ? sn.Length : 9));
				sn = sn.Substring(tmp.Length);
				_intPart[c++] = Int64.Parse(ReverseString(tmp));
			}
			Array.Resize(ref _intPart, c);
			_decPart = new long[0];
		}
		
		public static string ReverseString(string s)
		{
			string result = String.Empty;
			for (int i=s.Length-1; i>=0; i--) { result += s.Substring(i, 1); }
			return result;
		}
		


		public Nb(double n)
		{
			_isNul = (n==0 ? true : false);
			this.IsNegative = (n<0 ? true : false);
			_intPart = new long[]{(long)Math.Floor(Math.Abs(n))};
			n = Math.Abs(Math.Abs(n) - Math.Floor(Math.Abs(n)));
			n = n * Math.Pow(10, n.ToString().Length - 2);
			_decPart = new long[]{(long)n};			
		}
		
		
		public Nb(long[] intPart, long[] decPart, bool isNegative)
		{
			if ((intPart == null) || (intPart.Length == 0)) { intPart = new long[1]; }
			if (decPart == null) { decPart = new long[0]; }
			_intPart = intPart;
			_decPart = decPart;
			this.IsNegative = isNegative;
			this.UpdateProperties();
		}
		



		#endregion CONSTRUCTEURS
















		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES





		public override string ToString()
		{
			string[] arrDec = new string[_decPart.Length]; int c = 0;
			foreach (long l in _decPart) { arrDec[c++] = l.ToString("000000000"); }
			string[] arrInt = new string[_intPart.Length]; c = 0;
			foreach (long l in _intPart) { arrInt[c++] = l.ToString("000000000"); }
			arrInt = arrInt.Reverse().ToArray();
			string result = My.ArrayFunctions.Join(arrInt.Concat(arrDec).ToArray(), String.Empty);
			result = result.Substring(0, _intPart.Length * 9) + "." + result.Substring(_intPart.Length * 9);
			result = result.Trim('0');
			if (result.EndsWith(".")) { result = result.Substring(0, result.Length - 1); }
			return result;
		}
		
		
		
		
		
		
		/// <summary>
		/// Additionne des nombres.
		/// </summary>
		public static Nb Add(params Nb[] nbs)
		{
		
			// S'il a des nombres négatifs...
			
			
			
			
			
			
			// Récupère les plus grands index de tableux:
			int maxDec = 0, maxInt = 0;
			foreach (Nb nb in nbs) { if (nb.IntPart.Length > maxInt) { maxInt = nb.IntPart.Length; } }
			foreach (Nb nb in nbs) { if (nb.DecPart.Length > maxDec) { maxDec = nb.DecPart.Length; } }
			
			// Additionne chaque partie, avec la retenue, et tout:
			long carry = 0, res;
			long[] resultDec = new long[maxDec], resultInt = new long[maxInt + 1];
			int c = 0;
			for (int i=maxDec-1; i>=0; i--)
			{
				res = carry;
				foreach (Nb nb in nbs) { if (nb.DecPart.Length > i) { res += nb.DecPart[i]; } }
				if (res <= max) { resultDec[c] = res; carry = 0; }
				else { resultInt[c] = (long)(res - Math.Floor((double)res / 1000000000) * 1000000000); resultDec[c] = res - resultInt[c]; }
				c++;
			}
			for (int i=0; i<maxInt; i++)
			{
				res = carry;
				foreach (Nb nb in nbs) { if (nb.IntPart.Length > i) { res += nb.IntPart[i]; } }
				if (res <= max) { resultInt[c] = res; carry = 0; }
				else { resultInt[c] = (long)(res - Math.Floor((double)res / 1000000000) * 1000000000); carry = (res - resultInt[c]) / 1000000000; }
				c++;
			}
			if (carry == 0) { Array.Resize(ref resultInt, c); }
			else { resultInt[c] = carry; }
			resultDec = resultDec.Where(delegate(long val) { return val != 0; }).ToArray();
			resultInt = resultInt.Where(delegate(long val) { return val != 0; }).ToArray();
			
			// Redéfinit un nouveau nombre:
			return new Nb(resultInt, resultDec, false);

		}
		
		
		
		



		#endregion METHODES PUBLIQUES











		// ---------------------------------------------------------------------------
		// METHODES PRIVEES
		// ---------------------------------------------------------------------------




		#region METHODES PRIVEES
		
		
		
		
		
		
		protected void UpdateProperties()
		{
		
		
			// isNul:
			_isNul = true;
			foreach (long l in _intPart) { if (l != 0) { _isNul = false; break; } }
			foreach (long l in _decPart) { if (l != 0) { _isNul = false; break; } }
		
		
		}



		#endregion METHODES PRIVEES
	
	
	
	
	
	
	}
	
	
	
	
}
*/