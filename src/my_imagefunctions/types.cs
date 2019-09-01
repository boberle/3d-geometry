using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace My
{



	// ---------------------------------------------------------------------------
	// STRUCTURES:


	/// <summary>
	/// Structure pour les fonctions de remplacment de couleur.
	/// </summary>
	public struct ColorReplacement
	{
	
		private Color _min, _max, _replaceBy;
		internal byte MinR, MinG, MinB, MaxR, MaxG, MaxB, ReplR, ReplG, ReplB;
	
		public Color MinColor {
			get { return _min; }
			set {
				_min = value; byte A;
				ColorFunctions.GetARGB(_min, out A, out MinR, out MinG, out MinB); } }
		
		public Color MaxColor {
			get { return _max; }
			set {
				_max = value; byte A;
				ColorFunctions.GetARGB(_max, out A, out MaxR, out MaxG, out MaxB); } }
		
		public Color ReplaceByColor {
			get { return _replaceBy; }
			set {
				_replaceBy = value; byte A;
				ColorFunctions.GetARGB(_replaceBy, out A, out ReplR, out ReplG, out ReplB); } }

		public ColorReplacement(Color min, Color max, Color replaceBy) : this()
			{ MinColor = min; MaxColor = max; ReplaceByColor = replaceBy; }
		
	}


	/// <summary>
	/// Structure pour les fonctions de remplacment de couleur.
	/// </summary>
	public struct HSLColorReplacement
	{
	
		private Color _replaceBy;
		private HSLColor _min, _max;
		internal byte ReplR, ReplG, ReplB;
		internal float MinH, MinS, MinL, MaxH, MaxS, MaxL;
	
		public HSLColor MinColor {
			get { return _min; }
			set { _min = value; MinH = _min.H; MinS = _min.S; MinL = _min.L; } }
		
		public HSLColor MaxColor {
			get { return _max; }
			set {
				_max = value; MaxH = _max.H; MaxS = _max.S; MaxL = _max.L; } }
		
		public Color ReplaceByColor {
			get { return _replaceBy; }
			set {
				_replaceBy = value; byte A;
				ColorFunctions.GetARGB(_replaceBy, out A, out ReplR, out ReplG, out ReplB); } }
		
		public HSLColorReplacement(HSLColor min, HSLColor max, Color replaceBy) : this()
			{ MinColor = min; MaxColor = max; ReplaceByColor = replaceBy; }
		
	}

	
	
}
