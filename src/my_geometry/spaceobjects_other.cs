using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace My
{





	// ---------------------------------------------------------------------------
	// OBJETS DE L'ESPACE
	// ---------------------------------------------------------------------------




	#region OBJETS DE L'ESPACE




	/// <summary>
	/// Label de texte à afficher sur le plan du dessin.
	/// </summary>
	public class SpText : SpObject
	{
	
		protected Delegate[] _methods;
		protected string[] _originalText;
		protected string _shortText;
		protected string _finalText;
		protected bool _isAbsolute;
			
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Text"; } }
	
		/// <summary>
		/// Texte.
		/// </summary>
		public string Text { get { return _finalText; } }
		
		/// <summary>
		/// Obtient ou définit si les coordonnées de la propriété CoordLabel sont définies par rapport au centre du form (dans ce cas, le label reste en position absolue par rapport à l'écran) ou si ces coordonnées sont définies par rapport à l'origine (OriginOnWindow) du repère du form (dans ce cas, le label se déplacera dès qu'on déplacera l'origine du repère).
		/// </summary>
		public bool Absolute { get { return _isAbsolute; } }
		
		/// <summary>
		/// Constructeur. Le texte est séparé en champ dans le tableau. Chaque champ est une partie de texte. Si un champ est commence par un guillemet, alors il est considéré comme texte littéral. Sinon, il est considéré comme formule, et calculé comme tel. Tous les champs sont ensuite concaténé.
		/// </summary>
		public SpText(string name, bool absolute, string[] text) : base(name)
		{
			_finalText = String.Empty;
			Alter(absolute, text);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(bool absolute, string[] text)
		{
			_isAbsolute = absolute; _originalText = text;
			SpObject[] masters = MakeDelegates();
			if (masters == null) { return; }
			EndAlterProcess(masters);
		}

		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Forme le texte à partir des délégués des méthodes dynamiques:
			try
			{
				_finalText = String.Empty;
				_shortText = String.Empty;
				foreach (Delegate d in _methods)
					{ _finalText += (d is Func<string> ? ((Func<string>)d).Invoke() : FormatText((double)((Func<double>)d).Invoke())); }
			}
			catch (Exception exc)
				{ My.ErrorHandler.ShowError(exc); SendCalculationResult(true, "Error when executing delegates."); return; }
			// Forme une version raccourcie du texte:
			_shortText = (this.Text.Length>18 ? _finalText.Substring(0, 15) + "..." : _finalText);
			SendCalculationResult();
		}
		
		/// <summary>
		/// Fabrique les délégués de méthodes à partir de _originalText. Retourne les objets maîtres trouvés dans les formules, ou null s'il y a eu erreur.
		/// </summary>
		protected SpObject[] MakeDelegates()
		{
			// Définit le texte, le découpe en champs et calcule les éventuelles formules:
			SpObject[] masters = new SpObject[0];
			int l = _originalText.Length;
			_methods = new Delegate[l];
			try
			{
				for (int i=0; i<l; i++)
				{
					// Continue si texte vide:
					if (String.IsNullOrEmpty(_originalText[i])) { continue; }
					// Si le texte commence par ", alors c'est un texte littéral:
					if (_originalText[i].StartsWith("'")) {
						string t = String.Format("ToStr(“{0}”)", _originalText[i].Remove(0, 1));
						_methods[i] = (Func<string>)Formula.CreateFormulaMethod(t, null, typeof(Func<string>),
							typeof(SpText), typeof(String), FormulaWorkingType.Double, null, null); }
					// Sinon, c'est une formule, et enregistre dans les mastersObjects à retourner:
					else {
						_methods[i] = (Func<double>)Formula.CreateFormulaMethod(_originalText[i], null, typeof(Func<double>),
							typeof(SpText), null, FormulaWorkingType.Double, null, null);
						masters = masters.Concat(GetObjectsFromFormula(_originalText[i])).ToArray(); }
				}
				return masters;
			}
			catch (Exception exc)
				{ My.ErrorHandler.ShowError(exc); SendCalculationResult(true, "Error when making delegates."); return null; }
		}
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			int l = _originalText.Length;
			for (int i=0; i<l; i++)
				{ if (!_originalText[i].StartsWith("\"")) { ChangeNameInFormula(ref _originalText[i], oldName, newName); } }
			MakeDelegates();
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("like \"{0}\"", _shortText);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_isAbsolute, _originalText}; }

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			string text = String.Format("Text: {0}", _finalText);
			return base.GetInfos(text, lines);
		}

	}


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Label de texte à afficher sur le plan du dessin.
	/// </summary>
	public class SpCursor : SpObject
	{
	
		protected DoubleF _valueDblF, _minDblF, _maxDblF;
		protected double _value, _min, _max;
		protected bool _isLimited;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Cursor"; } }
	
		/// <summary>
		/// Value.
		/// </summary>
		public double Value { get { return _value; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpCursor(string name, DoubleF value) : base(name)
		{
			ShowName = false;
			Alter(value);
		}
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpCursor(string name, DoubleF value, DoubleF min, DoubleF max) : base(name)
		{
			ShowName = false;
			Alter(value, min, max);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(DoubleF value)
		{
			_valueDblF = value; _isLimited = false;
			EndAlterProcess(GetObjectsFromFormula(_valueDblF));
		}

		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(DoubleF value, DoubleF min, DoubleF max)
		{
			_valueDblF = value; _minDblF = min; _maxDblF = max; _isLimited = true;
			EndAlterProcess(GetObjectsFromFormula(_valueDblF), GetObjectsFromFormula(_minDblF), GetObjectsFromFormula(_maxDblF));
		}

		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Recalcule simplement la valeur:
			if (DoubleF.IsThereNan(_value = _valueDblF.Recalculate())) { SendCalculationResult(true, "Value is not valid."); return; }
			if (_isLimited)
			{
				if (DoubleF.IsThereNan(_min = _minDblF.Recalculate(), _max = _maxDblF.Recalculate()))
					{ SendCalculationResult(true, "Min or max not valid."); return; }
				if (_value < _min) { _value = _min; }
				if (_value > _max) { _value = _max; }
			}
			SendCalculationResult();
		}
		
		/// <summary>
		/// Modifie si possible la valeur.
		/// </summary>
		public virtual void AlterValue(double val)
			{ _valueDblF.Value = val; }
		
		/// <summary>
		/// Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public override void RebuildFormulas(string oldName, string newName)
		{
			ChangeNameInFormula(ref _valueDblF, oldName, newName);
			ChangeNameInFormula(ref _minDblF, oldName, newName);
			ChangeNameInFormula(ref _maxDblF, oldName, newName);
		}

		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			if (_isLimited) { return MakeToString("using {0} ≤ {1} ≤ {2}", _min, _value, _max); }
			return this.MakeToString("using value {0}", _value);
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
		{
			if (_isLimited) { return new object[]{_valueDblF, _minDblF, _maxDblF}; }
			return new object[]{_valueDblF};
		}

	}	
	


	#endregion OBJETS DE L'ESPACE


	
}
