using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Text;

namespace My
{





	/// <summary>
	/// Cette classe statique offre des fonctions de manipulations sur les classes, comme le clonage intégral par sérialisation, etc.
	/// </summary>
	public static class ClassManager
	{



		/// <summary>
		/// Clone l'objet par une copie profonde (méthode de la sérialisation). Pour copier une classe, il faut que celle-ci soit notée comme sérialisable, avec l'attribut "[Serializable()]" sur la ligne précédent la définition de la classe.
		/// </summary>
		public static object Clone(object toClone)
		{

			// Définition des variables
			MemoryStream objectStream = new MemoryStream();
			// Sérialise l'objet courant en mémoire
			(new BinaryFormatter()).Serialize(objectStream, toClone);
			// retourne un nouvel objet en le déserialisant
			objectStream.Seek(0, SeekOrigin.Begin);
			return ((new BinaryFormatter()).Deserialize(objectStream));

		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Pour toutes la collection de contrôles, sauf pour les boutons, change la couleur de fond avec la couleur spécifié. Si color vaut Color.Empty, met la couleur par défaut.
		/// </summary>
		public static void SetControlsBackColor(Control[] controls, Color color)
		{
			if (controls == null) { return; }
			// Pour tous les contrôles...
			foreach (Control i in controls)
			{
				// Si ce n'est pas un bouton...
				if (!(i is Button))
				{
					// Si la propriété BackColor existe...
					if (i.GetType().GetProperty("BackColor") != null) { i.BackColor = ((color == Color.Empty) ? Control.DefaultBackColor : color); }
				}
			}
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Obtient les paramètres du constructeur de type. S'il y a plusieurs constructeurs, il faut préciser l'index.
		/// </summary>
		public static Type[] GetCtorParameters(Type type, int index)
		{
			ConstructorInfo[] ctors = type.GetConstructors();
			if (index < 0) { index = 0; }
			if (index >= ctors.Length) { return null; }
			ParameterInfo[] parameters = ctors[index].GetParameters();
			Type[] types = new Type[parameters.Length];
			for (int i=0; i<types.Length; i++) { types[i] = parameters[i].ParameterType; }
			return types;
		}
		
		/// <summary>
		/// Obtient les paramètres du constructeur de type. S'il y a plusieurs constructeurs, il faut préciser l'index.
		/// </summary>
		public static Type[] GetCtorParameters(Type type)
		{
			return GetCtorParameters(type, 0);
		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Retourne les noms des paramètres de methdName. Ce paramètre doit être de la forme "className,methodName", et la méthode doit être dans l'assemblage appelant. Index désigne l'index de la surgcharge.
		/// </summary>
		public static string[] GetParameterNames(string methodName, int index)
		{
			string[] split = methodName.Split(new string[]{","}, StringSplitOptions.None);
			MethodInfo[] meths = Assembly.GetCallingAssembly().GetType(split[0])
				.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			meths = meths.Where(delegate(MethodInfo m) { return (m.Name == split[1]); }).ToArray();
			return Array.ConvertAll<ParameterInfo,string>(meths[index].GetParameters(), delegate(ParameterInfo p) { return p.Name; });
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retourne la syntaxe d'une commande, avec le nom et le type de paramètre. multilines indique si la syntaxe doit être écrite sur plusieurs lignes (un paramètre par ligne) ou non.
		/// </summary>
		public static string GetMethodSyntax(MethodInfo meth, bool multilines)
		{
			StringBuilder sb = new StringBuilder(String.Format("• ({0}) {1}{2}", meth.ReturnType.Name, meth.Name, (multilines ? "\n\t" : ": ")));
			ParameterInfo[] pis = meth.GetParameters(); int l = pis.Length;
			if (l == 0) { sb.Append("(no param)"); }
			for (int i=0; i<l; i++)
				{ sb.AppendFormat("{0}({1}){2}", pis[i].Name, pis[i].ParameterType.Name, (i!=l-1 ? (multilines ? ",\n\t" : ", ") : "")); }
			return sb.ToString();
		}	


	}



	// ===========================================================================
	


	/// <summary>
	/// Fournit une classe d'historique, qui enregistre des lignes.
	/// </summary>
	public class History<T>
	{
	
		public enum AddModeType { AddAtCurPos, AddAtEnd }
		
		private T[] _lines;
		private int _histLen;
		private int _position;
		private int _maxSize;
		
		/// <summary>
		/// Obtient ou définit la façon d'ajouter des données quand la position de l'historique n'est pas à la fin. Si AddAtCurPos, les données sont ajouter à la position actuelle de l'historique, et les données suivantes sont effacées. Si AddAtEnd, les données sont ajoutés à la fin, et aucune autre n'est supprimée.
		/// </summary>
		public AddModeType AddMode { get; set; }
		
		/// <summary>
		/// Obtient la position courrante du curseur dans l'historique.
		/// </summary>
		public int Position { get { return _position; } }
		
		/// <summary>
		/// Obtient le nombre d'éléments enregistrés.
		/// </summary>
		public int Length { get { return _histLen; } }
		
		/// <summary>
		/// Constructeur. maxSize définit la taile totale de l'historique. Lorsqu'on ajoute une ligne et que la taille totale de l'historique est atteinte, les premières lignes entrées sont supprimées. Si la valeur est 0, alors la taille est infinie.
		/// </summary>
		public History(int maxSize)
		{
			_maxSize = maxSize;
			_lines = new T[(_maxSize<1 ? 50 : _maxSize)];
		}
		
		/// <summary>
		/// Ajoute une ligne à l'historique, et retourne cette ligne. resetPosition indique s'il faut remettre la position actuelle de l'historique à la fin.
		/// </summary>
		public T AddLine(T line, bool resetPosition)
		{
			// Remet la position dans les limites du tableau:
			if (_position < 0) { _position = 0; }
			if (_position > _histLen) { _position = _histLen; }
			// Si on doit ajouter des données à la position courante, et que celle-ci n'est pas à la fin,
			// on supprime ce qui vient après:
			if (AddMode == History<T>.AddModeType.AddAtCurPos) {
				for (int i=_position; i<_histLen; i++) { _lines[i] = default(T); }
				_histLen = _position; }
			// Si on doit ajouter éternellement des données:
			if (_histLen >= _lines.Length && _maxSize < 1) {
				Array.Resize(ref _lines, _histLen + 50); }
			// S'il y a une limite, et si on a atteint cette limite, on supprime la première donnée avant d'ajouter la dernière:
			else if (_histLen >= _lines.Length && _maxSize > 0) {
				T[] temp = new T[_maxSize];
				Array.Copy(_lines, 1, temp, 0, _maxSize - 1);
				_lines = temp;
				_histLen = _maxSize - 1; }
			// Ajoute et remet la position du curseur.
			_lines[_histLen++] = line;
			if (resetPosition) { _position = _histLen; }
			return line;
		}
		
		/// <summary>
		/// Ajoute une ligne à l'historique, et retourne cette ligne. Remet à la fin la position actuelle de l'historique.
		/// </summary>
		public T AddLine(T line)
		{
			return AddLine(line, true);
		}
		
		/// <summary>
		/// Avance d'une ligne dans l'historique, et retourne la ligne courante, ou null si on est au bout.
		/// </summary>
		public T Forward()
		{
			if (++_position >= _histLen) { _position = _histLen; return default(T); }
			return _lines[_position];
		}
		
		/// <summary>
		/// Recule d'une ligne dans l'historique, et retourne la ligne courante, ou null si on est au bout.
		/// </summary>
		public T Back()
		{
			if (--_position < 0) { _position = -1; return default(T); }
			return _lines[_position];
		}
		
		/// <summary>
		/// Supprime tout l'historique.
		/// </summary>
		public void Clear()
		{
			_position = _histLen = 0;
			_lines = new T[(_maxSize<1 ? 50 : _maxSize)];
		}
		
		/// <summary>
		/// Sauve l'historique dans un fichier, et retourne true si réussi. Ignore les lignes correspondant au modèle ignore.
		/// </summary>
		public bool SaveInFile(string filename, Regex ignore)
		{
			string[] temp = Array.ConvertAll<T,string>(_lines, delegate(T s) { return (string)Convert.ChangeType(s, typeof(string)); });
			string[] dest = temp.Where(delegate(string s) { if (String.IsNullOrEmpty(s)) return false; else return !ignore.IsMatch(s); }).ToArray();
			return My.FilesAndStreams.WriteAllLines(filename, dest);
		}
	
	
	}



	// ===========================================================================
	
	
	/// <summary>
	/// Fournit un buffer qui enregistre des éléments dans un tableau, puis les retourne en remettant à zéro le buffer. Le constructeur accepte un argument qui définit la taille initiale du tableau, et la taille de ses augmentations régulières.
	/// </summary>
	public class Buffer<T>
	{
	
		private T[] _array;
		private int _counter, _buf;
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public Buffer(int buf)
		{
			_buf = buf; _counter = 0;
			_array = new T[_buf];
		}
		
		/// <summary>
		/// Retourne les éléments enregistrer et remet à zéro.
		/// </summary>
		public T[] Reset()
		{
			T[] result = new T[_counter];
			Array.Copy(_array, result, _counter);
			for (int i=0; i<_counter; i++) { _array[i] = default(T); }
			_array = new T[_buf]; _counter = 0;
			return result;
		}
		
		/// <summary>
		/// Remet à zéro sans retourner les résultats.
		/// </summary>
		public void ResetOnly()
		{
			for (int i=0; i<_counter; i++) { _array[i] = default(T); }
			_array = new T[_buf]; _counter = 0;
		}
		
		/// <summary>
		/// Retourne les éléments en enregistrer sans remettre à zéro.
		/// </summary>
		public T[] GetValues()
		{
			T[] result = new T[_counter];
			Array.Copy(_array, result, _counter);
			return result;
		}
		
		/// <summary>
		/// Enregistre un élément dans le tableau.
		/// </summary>
		public void SetValue(T element)
		{
			if (_counter >= _array.Length) { Array.Resize(ref _array, _counter + _buf); }
			_array[_counter++] = element;
		}
	
	}


	// ===========================================================================
	
	
	/// <summary>
	/// Structure qui fournit des informations HSL pour une couleur.
	/// </summary>
	public struct HSLColor
	{
	
		// ---------------------------------------------------------------------------
		// DECLARATIONS:
		
		private float _h, _s, _l;
		private Color _color;
		private static string _delimiter;
		private static string[] _delimiterArray;
		
		// ---------------------------------------------------------------------------
		// PROPRIETES:

		/// <summary>
		/// Obtient ou définit le séparateur pour les méthodes Parse et TryParse. ":" par défaut.
		/// </summary>
		public static string Delimiter {
			get { return _delimiter; }
			set { _delimiter = value; _delimiterArray = new string[]{_delimiter}; } }
		
		/// <summary>
		/// Obtient ou définit la valeur de teinte. Tout float est accepté. Si dépasse [0;360], la valeur est remise entre ces bornes.
		/// </summary>
		public float H {
			get { return _h; }
			set { _h = value; SetColor(); } }
		
		/// <summary>
		/// Obtient ou définit la valeur de saturation. Tout foat est accepté. Si inférieur à 0, vaut 0; si supérieur à 1, vaut 1.
		/// </summary>
		public float S {
			get { return _s; }
			set { _s = value; SetColor(); } }
		
		/// <summary>
		/// Obtient ou définit la valeur de luminosité. Voir S.
		/// </summary>
		public float L {
			get { return _l; }
			set { _l = value; SetColor(); } }
		
		/// <summary>
		/// Obtient la couleur résultante. Elle est calculée lors de la construction de l'objet, et chaque fois que H, S ou L change.
		/// </summary>
		public Color Color {
			get { return _color; } }
		
		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS:

		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static HSLColor()
			{ Delimiter = ":"; }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public HSLColor(float h, float s, float l) : this()
			{ _h = h; _s = s; _l = l; SetColor(); }

		// ---------------------------------------------------------------------------
		// METHODES:

		/// <summary>
		/// Remet _h, _s et _l dans les limites, et calcule _color.
		/// </summary>
		private void SetColor()
		{
			while (_h < 0) { _h += 360; }
			while (_h > 360) { _h -= 360; }
			if (_s < 0) { _s = 0; }
			if (_s > 1) { _s = 1; }
			if (_l < 0) { _l = 0; }
			if (_l > 1) { _l = 1; }
			int r, g, b;
			My.ColorFunctions.ConvertFromHSLToRGB(_h, _s, _l, out r, out g, out b);
			_color = Color.FromArgb(r, g, b);
		}
		
		/// <summary>
		/// Tente de convertir un texte. FormatException si erreur.
		/// </summary>
		public static HSLColor Parse(string text)
		{
			HSLColor result;
			if (TryParse(text, out result)) { return result; }
			else { throw new FormatException(); }
		}
		
		/// <summary>
		/// Tente de convertir un texte.
		/// </summary>
		public static bool TryParse(string text, out HSLColor hslColor)
		{
			hslColor = new HSLColor();
			string[] split = text.Split(_delimiterArray, StringSplitOptions.RemoveEmptyEntries);
			if (split.Length != 3) { return false; }
			float h, s, l;
			if (!Single.TryParse(split[0], out h) || !Single.TryParse(split[1], out s) || !Single.TryParse(split[2], out l))
				{ return false; }
			hslColor = new HSLColor(h, s, l); return true;
		}

		/// <summary>
		/// Retourne un description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return String.Format("{0}{3}{1}{3}{2}", _h, _s, _l, _delimiter);
		}
		
	}
	


}


