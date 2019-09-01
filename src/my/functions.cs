using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace My
{




	/// <summary>
	/// Fournit des fonctions personnalisées à portée générale.
	/// </summary>
	public static class Functions
	{



		private static Random rand_ForRandomText = new Random();

		/// <summary>
		/// Génère un string aléatoire contenant une série de lettres majuscules (A-Z) (ou grec polytonique).
		/// </summary>
		public static string RandomText(int min, int max, bool greek)
		{
			int length = rand_ForRandomText.Next(min, max + 1);
			StringBuilder s = new StringBuilder();
			if (greek) { for (int i = 0; i <= length; i++) { s.Append((char)rand_ForRandomText.Next(7936, 8190)); } }
			else { for (int i = 0; i <= length; i++) { s.Append((char)rand_ForRandomText.Next(65, 90)); } }
			return s.ToString();
		}


		/// <summary>
		/// Génère un string aléatoire contenant une série de lettres majuscules (A-Z) (ou grec polytonique).
		/// </summary>
		public static string RandomText(int min, int max)
		{
			return RandomText(min, max, false);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Cette fonction raccourcis un chemin d'accès pour qu'il correspondent au nombre de caractères maximum spécifié. Si c'est trop long, insère au début des points de suspension (e.g. "...dir\dir\file.ext"). En fait, on peut passer n'importe quelle chaînes de caractères, pas forcément un chemin d'accès.
		/// </summary>
		public static string ShortPath(string fileName, int maxChar)
		{
		
			// Sort si déjà ok, ou si vide:
			if (String.IsNullOrEmpty(fileName)) { return null; }
			if (fileName.Length <= maxChar) { return fileName; }
			
			// Supprime le début et retour avec des points de suspension:
			return fileName.Substring(fileName.Length - maxChar - 3, maxChar - 3) + "...";
			
		}


		// ---------------------------------------------------------------------------


		private static string[] _fontNames;
		
		/// <summary>
		/// Retourne un tableau avec les noms des polices disponibles.
		/// </summary>
		public static string[] GetFontNames()
		{
			// Si le tableau est déjà rempli, le retourne directement:
			if (_fontNames != null) { return _fontNames; }
			// Sinon, le remplit puis le retourne:
			string[] names = new string[FontFamily.Families.Length];
			int c = 0;
			foreach (FontFamily f in FontFamily.Families) { names[c++] = f.Name; }
			_fontNames = names;
			return names;
		}


		// ---------------------------------------------------------------------------
	
	
		private static ImageFormat[] _imageFormats;
		private static string[] _imageFormatsExtensions;
		
		/// <summary>
		/// Retourne tous les types d'images de ImageFormat, et les extensions associées. includeMemory indique s'il faut inclure MemoryBmp.
		/// </summary>
		public static ImageFormat[] GetImageFormats(bool includeMemory, out string[] extensions)
		{
			if (_imageFormats != null) { extensions = _imageFormatsExtensions; return _imageFormats; }
			extensions = new string[20]; ImageFormat[] formats = new ImageFormat[20]; int c = 0;
			PropertyInfo[] props = typeof(ImageFormat).GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach (PropertyInfo pi in props) {
				if (c >= formats.Length) { Array.Resize(ref extensions, c + 20); Array.Resize(ref formats, c + 20); }
				if (((ImageFormat)pi.GetValue(null, null)).Equals(ImageFormat.MemoryBmp) && !includeMemory) { continue; }
				formats[c] = (ImageFormat)pi.GetValue(null, null);
				extensions[c] = My.GeneralParser.GetExtensionFromImageFormat(formats[c]);
				c++; }
			Array.Resize(ref extensions, c); Array.Resize(ref formats, c);
			_imageFormats = formats; _imageFormatsExtensions = extensions;
			return formats;
		}
		
	
		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Délégué à exécuter par ExecuteAndGC.
		/// </summary>
		public delegate void ExecuteAndGCMethod1(object[] args);
		
		/// <summary>
		/// Lance un délégué avec le paramètres args, puis force un GC.Collect(...). Tout cela parce que dans certaine application quelques peut complexe, et surtout en mode Realease, le GC ne s'exécute pas sinon...
		/// </summary>
		public static void ExecuteAndGC(ExecuteAndGCMethod1 deleg, object[] args)
		{
			deleg.Invoke(args);
			System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Batch;
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			GC.WaitForPendingFinalizers();
			System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Interactive;
		}

		/// <summary>
		/// Délégué à exécuter par ExecuteAndGC.
		/// </summary>
		public delegate void ExecuteAndGCMethod();
		
		/// <summary>
		/// Lance un délégué avec le paramètres args, puis force un GC.Collect(...). Tout cela parce que dans certaine application quelques peut complexe, et surtout en mode Realease, le GC ne s'exécute pas sinon...
		/// </summary>
		public static void ExecuteAndGC(ExecuteAndGCMethod deleg)
		{
			deleg.Invoke();
			System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Batch;
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			GC.WaitForPendingFinalizers();
			System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Interactive;
		}


		// ---------------------------------------------------------------------------


		private static object _clipboardData;

		/// <summary>
		/// Sauve le contenu du presse papier (si save vaut true), ou restaure une précédente sauvegarde (si save vaut false).
		/// </summary>
		public static void SaveRestoreClipboard(bool save)
		{
			try
			{
				// Si sauvegarde mais pas de données, sort:
				if (save && Clipboard.GetDataObject().GetFormats() == null) { return; }
				// Si sauvegarde et des données, enregistre la valeur:
				else if (save) { _clipboardData = Clipboard.GetData(Clipboard.GetDataObject().GetFormats()[0]); }
				// Si restauration et pas de données, sort:
				else if (!save && _clipboardData == null) { Clipboard.Clear(); }
				// Sinon (restauration et données valides), insère dans le presse papier:
				else { Clipboard.SetDataObject(_clipboardData, true); _clipboardData = null; }
			}
			catch { ; }
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne la hauteur d'un texte en considérant la largeur du contrôle. La hauteur retourné est augmentée de marginY. marginX est pris en considération pour la largeur du contrôle. Pour une case à cocher, par exemple, il faut rajouter +15 à marginX, pour tenir compte de la case.
		/// </summary>
		public static int TextHeightFromWidth(Control control, string text, int marginX, int marginY)
		{
			return (int)Math.Ceiling(control.CreateGraphics().MeasureString(text, control.Font,
				control.ClientSize.Width - marginX).Height) + marginY;
		}

		/// <summary>
		/// Même chose que la surcharge, mais le texte utilisé est celui du contrôle. 
		/// </summary>
		public static int TextHeightFromWidth(Control control, int marginX, int marginY)
			{ return TextHeightFromWidth(control, control.Text, marginX, marginY); }
		
		/// <summary>
		/// Même chose que la surcharge, mais le texte utilisé est celui du contrôle et les marges sont calculés. 
		/// </summary>
		public static int TextHeightFromWidth(Control control)
		{
			return TextHeightFromWidth(control, control.Text, control.Margin.Horizontal+control.Padding.Horizontal,
				control.Margin.Vertical+control.Padding.Vertical);
		}
		

	}



	// ===========================================================================



	/// <summary>
	/// Fournit des méthodes pour les tableaux.
	/// </summary>
	public static class ArrayFunctions
	{
	
		// ---------------------------------------------------------------------------
		// SOUS-CLASSES ET DECLARATIONS:
			
		
		/// <summary>
		/// Cette classe est un comparateur d'égalité insensible à la casse, utilisable dans des méthodes telles que Distinct ou Contains des tableaux, par exemple.
		/// </summary>
		public class IgnoreCaseComparer<T> : IEqualityComparer<T>
		{
			private CaseInsensitiveComparer _comp;
			public IgnoreCaseComparer()
				{ _comp = CaseInsensitiveComparer.Default; }
			public bool Equals(T a, T b)
				{ return _comp.Compare(a, b) == 0; }
			public int GetHashCode(T obj)
				{ return obj.ToString().ToLower().GetHashCode(); }
		}

		// DECLARATIONS:
		private static IgnoreCaseComparer<string> _stringIgnoreCaseComparer;


		// ---------------------------------------------------------------------------
		// PROPRIETE:
		
		
		/// <summary>
		/// Obtient un comparateur String insensible à la casse.
		/// </summary>
		public static IgnoreCaseComparer<string> StringIgnoreCaseComparer { get { return _stringIgnoreCaseComparer; } }
		

		// ---------------------------------------------------------------------------
		// PROPRIETE:
		
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		static ArrayFunctions()
		{
			_stringIgnoreCaseComparer = new IgnoreCaseComparer<string>();
		}
		

		// ---------------------------------------------------------------------------
		// METHODES:
		

		/// <summary>
		/// Retourne le type des éléments de arr. Eg. si on passe un type string[] pour arr, on obtient string. Pour string[][], on obtient string[].
		/// </summary>
		public static Type GetElementType(Type arr)
		{
			// Il peut y avoir plusieurs fois [] (tableau déchiqueté), donc il ne faut en supprimer qu'un seul:
			string s = arr.AssemblyQualifiedName;
			int indexOf; if ((indexOf = s.IndexOf("[]")) == -1) { return arr; }
			return Type.GetType(arr.AssemblyQualifiedName.Remove(indexOf, 2));
		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Inverse un tableau bidimensionnel de base 0. Ce sont les lignes qui sont inversées, et non les colonnes !
		/// </summary>
		public static void ReverseBidimArray<T>(T[,] arr)
		{
			int cLength = arr.GetLength(1);
			int end = arr.GetLength(0) - 1;
			for (int start=0; start<end; start++)
			{
				for (int c=0; c<cLength; c++)
				{
					T tmp = arr[start,c]; arr[start,c] = arr[end,c]; arr[end,c] = tmp;
				}
				end--;
			}
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Tente de convertir une chaîne en tableau de Int32. Par exemple, "1/2/3" sera converti en int[3] si le séparateur est "/". Si zeroForNoNumeric vaut false, la fonction s'arrête et retourne null dès qu'une sous-chaîne n'est pas de type numérique. Si true, la cellule du tableau de retour contiendra 0 à la place de cette sous-chaîne non numérique (e.g. "1/a/2" retournera {1, 0, 2} plutôt que de retourner null).
		/// </summary>
		public static int[] SplitStringToIntArray(string text, string separator, bool zeroForNoNumeric)
		{
			string[] arr = text.Split(new string[] { separator }, StringSplitOptions.None);
			int[] arrInt = new int[arr.Length];
			int nb;
			for (int i = 0; i < arr.Length; i++)
			{
				if (Int32.TryParse(arr[i], out nb)) { arrInt[i] = nb; }
				else { if (zeroForNoNumeric) { return null; } else { arrInt[i] = 0; } }
			}
			return arrInt;

		}

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static int[] SplitStringToIntArray(string text, string separator)
			{ return SplitStringToIntArray(text, separator, true); }


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Cherche dans un tableau si l'argument value existe, en ignorant la casse.
		/// </summary>
		public static bool ArrayContainsIgnoreCase(string[] array, string value)
		{
			/*bool found = false;
			foreach (object i in array)
				{ if (i.ToString().Equals(value, StringComparison.CurrentCultureIgnoreCase)) { found = true; break; } }
			return found;*/
			return array.Contains(value, _stringIgnoreCaseComparer);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Compare deux tableaux avec pour chaque élément la méthode Equals du type. Autrement dit, les deux tableaux sont identiques si les valeurs de chaque élément sont égales pour les types de bases, ou si les références sont égales pour les types qui ne sont pas de base. Si les deux tableaux sont null, retourne true. Si l'un seulement des tableaux est null, retourne false.
		/// </summary>
		public static bool ArrayEquals<T>(T[] arr1, T[] arr2)
		{
			// Si null:
			if ((arr1 == null) && (arr2 == null)) { return true; }
			if ((arr1 == null) || (arr2 == null)) { return false; }
			// Compare les longueurs:
			if (arr1.Length != arr2.Length) { return false; }
			// Compare chaque élément:
			int length = arr1.Length;
			for (int i=0; i<length; i++) { if (arr1[i].Equals(arr2[i]) == false) { return false; } }
			return true;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Retaille un tableau bidimensionnel selon la première dimension. Si array est null, ne fait rien, si newSize est inférieur à 0, ne fait rien.
		/// </summary>
		public static void ResizeBidimArray<T>(ref T[,] array, int newSize)
		{
			if ((array == null) || (newSize < 0)) { return; }
			int iLength = newSize;
			int jLength = array.GetLength(1);
			T[,] result = new T[iLength, jLength];
			for (int i=0; i<iLength; i++)
			{
				for (int j=0; j<jLength; j++) {
					if (i < array.GetLength(0)) { result[i,j] = array[i,j]; }
					else { result[i,j] = default(T); } }
			}
			array = result;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Concatène deux tableaux bidimensionnels selon la première dimension. La longueur de deuxième dimension des deux tableaux doit être égales, sinon lève une exception.
		/// </summary>
		public static T[,] ConcatBidimArray<T>(T[,] array1, T[,] array2)
		{
			// Exception si les arguments n'ont pas le même nombre de secondes dimensions:
			if (array1.GetLength(1) != array2.GetLength(1))
				{ throw new ArgumentException("Both arrays don't have the same length for dimension 2"); }
			int iLength1 = array1.GetLength(0);
			int iLength2 = array2.GetLength(0);
			int jLength = array1.GetLength(1);
			T[,] result = new T[iLength1+iLength2,jLength];
			for (int i=0; i<iLength1; i++) { for (int j=0; j<jLength; j++) { result[i,j] = array1[i,j]; } }
			for (int i=0; i<iLength2; i++) { for (int j=0; j<jLength; j++) { result[iLength1+i,j] = array2[i,j]; } }
			return result;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Joint un tableau de type T[] en une seule chaîne string avec un séparateur string.
		/// </summary>
		public static string Join<T>(T[] sourceArray, string separator)
		{
			// Si source null, sort:
			if (sourceArray == null) { return null; }
			// Parcourt tous les éléments, et les assemble:
			string result = string.Empty;
			for (int i = 0; i < sourceArray.Length; i++) { result += sourceArray[i].ToString() + ((i < sourceArray.Length - 1) ? separator : string.Empty); }
			return result;
		}


		/// <summary>
		/// Joint un tableau de type IList en une seule chaîne string avec un séparateur string.
		/// </summary>
		public static string Join<T>(IList<T> sourceArray, string separator)
		{
			// Si source null, sort:
			if (sourceArray == null) { return null; }
			// Parcourt tous les éléments, et les assemble:
			string result = string.Empty;
			for (int i = 0; i < sourceArray.Count; i++) { result += sourceArray[i].ToString() + ((i < sourceArray.Count - 1) ? separator : string.Empty); }
			return result;
		}


		/// <summary>
		/// Joint un objet de type IDictionary contenant des paires clé/valeur de type string. Chaque clé/valeur est réuni par keyValueSepartor, chaque élément du dictionnaire est réuni par elementsSeparator.
		/// </summary>
		public static string Join<T, V>(IDictionary<T, V> sourceArray, string keyValueSepartor, string elementsSeparator)
		{
			// Si source null, sort:
			if (sourceArray == null) { return null; }
			// Parcourt tous les éléments, et les assemble:
			string result = string.Empty; int i = 0;
			foreach (T j in sourceArray.Keys) {
				result += j.ToString() + keyValueSepartor + sourceArray[j].ToString() + ((i < sourceArray.Count - 1) ? elementsSeparator : string.Empty);
				i++; }
			return result;
		}


		/// <summary>
		/// Retourne le contenu d'un tableau sous forme de chaîne, avec le séparateur spécifié. Il faut ici passer une fonction qui renverra un string. Par exemple : "int[] arr = new int[] {1,2,3,4,5,6,7,8,9}; MessageBox.Show(Join((int))(arr, delegate(int nb) { return nb.ToString(); }, "\n"));".
		/// </summary>
		public static string Join<T>(T[] arr, Func<T, string> f, string separator)
		{
			if (arr == null) { return null; }
			string result = string.Empty;
			foreach (T i in arr) { result += f(i) + separator; }
			return result.ToString().Substring(0, result.Length - (result.Length>0 ? separator.Length : 0));
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Trie un tableau unidimensionnel IComparable de base 0.
		/// </summary>
		public static void QuickSort(IComparable[] arr)
			{ QuickSort(arr, 0, arr.Length - 1); }

		/// <summary>
		/// Méthode de tri privée pour QuickSort.
		/// </summary>
		private static void QuickSort(IComparable[] arr, int start, int end)
		{
			int i = start; int j = end;
			IComparable middle = arr[(i+j)/2];
			while (i <= j)
			{
				while (arr[i].CompareTo(middle) < 0) i++;
				while (middle.CompareTo(arr[j]) < 0) j--;
				if (i <= j) 
				{
					IComparable tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
					i++; j--;
				}
			}
			if (start < j) QuickSort(arr, start, j);
			if (i < end) QuickSort(arr, i, end);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Trie un tableau unidimensionnel int de base 0.
		/// </summary>
		public static void QuickSort(int[] arr)
			{ QuickSort(arr, 0, arr.Length - 1); }

		/// <summary>
		/// Méthode de tri privée pour QuickSort.
		/// </summary>
		private static void QuickSort(int[] arr, int start, int end)
		{
			int i = start; int j = end;
			int middle = arr[(i+j)/2];
			while (i <= j)
			{
				while (arr[i].CompareTo(middle) < 0) i++;
				while (middle.CompareTo(arr[j]) < 0) j--;
				if (i <= j) 
				{
					int tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
					i++; j--;
				}
			}
			if (start < j) QuickSort(arr, start, j);
			if (i < end) QuickSort(arr, i, end);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Trie un tableau bidimensionnel IComparable de base 0, sur les éléments de la première dimension.
		/// </summary>
		public static void QuickSort(IComparable[,] arr)
			{ QuickSort(arr, 0, arr.GetLength(0) - 1); }

		/// <summary>
		/// Méthode de tri privée pour QuickSort.
		/// </summary>
		private static void QuickSort(IComparable[,] arr, int start, int end)
		{
			int i = start; int j = end;
			IComparable middle = arr[(i+j)/2,0];
			int cLength = arr.GetLength(1);
			while (i <= j)
			{
				while (arr[i,0].CompareTo(middle) < 0) i++;
				while (middle.CompareTo(arr[j,0]) < 0) j--;
				if (i <= j) 
				{
					for (int c=0; c<cLength; c++)
					{
						IComparable tmp = arr[i,c]; arr[i,c] = arr[j,c]; arr[j,c] = tmp;
					}
					i++; j--;
				}
			}
			if (start < j) QuickSort(arr, start, j);
			if (i < end) QuickSort(arr, i, end);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Trie un tableau bidimensionnel object de base 0, sur les éléments de la première dimension. Ces éléments de première dimension doivent être de type IComparable, sinon une exception est levée.
		/// </summary>
		public static void QuickSort(object[,] arr)
			{ QuickSort(arr, 0, arr.GetLength(0) - 1); }

		/// <summary>
		/// Méthode de tri privée pour QuickSort.
		/// </summary>
		private static void QuickSort(object[,] arr, int start, int end)
		{
			int i = start; int j = end;
			IComparable middle = (IComparable)arr[(i+j)/2,0];
			int cLength = arr.GetLength(1);
			while (i <= j)
			{
				while (((IComparable)arr[i,0]).CompareTo(middle) < 0) i++;
				while (middle.CompareTo(((IComparable)arr[j,0])) < 0) j--;
				if (i <= j) 
				{
					for (int c=0; c<cLength; c++)
					{
						object tmp = arr[i,c]; arr[i,c] = arr[j,c]; arr[j,c] = tmp;
					}
					i++; j--;
				}
			}
			if (start < j) QuickSort(arr, start, j);
			if (i < end) QuickSort(arr, i, end);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Trie un tableau unidimensionnel string de base 0, selon la longueur de chaîne des éléments.
		/// </summary>
		public static void QuickSortByStringLength(string[] arr)
			{ QuickSortByStringLength(arr, 0, arr.Length - 1); }

		/// <summary>
		/// Méthode de tri privée pour QuickSortByStringLength.
		/// </summary>
		private static void QuickSortByStringLength(string[] arr, int start, int end)
		{
			int i = start; int j = end;
			int middle = arr[(i+j)/2].Length;
			while (i <= j)
			{
				while (arr[i].Length.CompareTo(middle) < 0) i++;
				while (middle.CompareTo(arr[j].Length) < 0) j--;
				if (i <= j) 
				{
					string tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
					i++; j--;
				}
			}
			if (start < j) QuickSortByStringLength(arr, start, j);
			if (i < end) QuickSortByStringLength(arr, i, end);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Trie un tableau bidimensionnel string de base 0, selon la longueur de chaîne des éléments de la première dimension.
		/// </summary>
		public static void QuickSortByStringLength(string[,] arr)
			{ QuickSortByStringLength(arr, 0, arr.GetLength(0) - 1); }

		/// <summary>
		/// Méthode de tri privée pour QuickSortByStringLength.
		/// </summary>
		private static void QuickSortByStringLength(string[,] arr, int start, int end)
		{
			int i = start; int j = end;
			int middle = arr[(i+j)/2,0].Length;
			int cLength = arr.GetLength(1);
			while (i <= j)
			{
				while (arr[i,0].Length.CompareTo(middle) < 0) i++;
				while (middle.CompareTo(arr[j,0].Length) < 0) j--;
				if (i <= j) 
				{
					for (int c=0; c<cLength; c++)
					{
						string tmp = arr[i,c]; arr[i,c] = arr[j,c]; arr[j,c] = tmp;
					}
					i++; j--;
				}
			}
			if (start < j) QuickSortByStringLength(arr, start, j);
			if (i < end) QuickSortByStringLength(arr, i, end);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Remplace dans une chaîne tous les éléments de la première colonne de list par ceux de la deuxième colonne.
		/// E.g.: string[,]list={{"a", "b"},{"c","d"}}; Debug.WriteLine(MultiReplace(list, "abcd")); donnera "bbdd";.
		/// Le problème, c'est que si des éléments du tableau (dans la 1ère colonne) ont un début commun, il peut y avoir confusion.
		/// Par exemple, avec un tableau contenant {{"ab", "12"},{"abc","123"}}, on obtiendra (avec une chaîne "abcd") "12cd" et non "123d".
		/// Pour corriger le problème, il faut trier le tableau en sens inverse de la longueur des éléments de la 1ère colonne (les éléments de la 1ère colonne les plus longs ayant alors l'index le plus faible), et l'exemple complet devient alors:
		/// string[,]list={{"ab", "12"},{"abc","123"}};
		/// My.Functions.QuickSortByStringLength(list);
		/// My.Functions.ReverseBidimArray(list);
		/// Debug.WriteLine(My.ArrayFunctions.MultiReplace(list, "abcd"));
		/// Ce qui donnera : "123d", et non plus "12cd";
		/// On peut aussi utiliser des caractères délimiteurs : {{"!ab!", "12"},{"!abc!","123"}} ne pose pas de problème.
		/// </summary>
		public static string MultiReplace(string[,] list, string text)
		{
			StringBuilder sb = new StringBuilder(text);
			int lenght = list.GetLength(0);
			for (int i = 0; i < lenght; i++) { sb.Replace(list[i, 0], list[i, 1]); }
			return sb.ToString();
		}


		// ---------------------------------------------------------------------------
		

		/// <summary>
		/// Découpe un tableau en deux autres, avec null comme séparateur (s'il y a plusieurs séparateur, seul le premier est utilisé). S'il n'y a pas de séparateur, arr1 est entry, et arr2 est un tableau à 0 élément. Ne fonctionne que pour les types nullable (évidemment).
		/// </summary>
		public static void SplitTwoArrays<T>(T[] entry, out T[] arr1, out T[] arr2)
		{
			// Variables:
			int start1 = 0, start2 = 0, len1 = 0, len2 = 0, l = entry.Length; bool split = false;
			// Pour chaque objet de entry, on regarde si on trouve un separator, et on incrémente
			// les compteurs en fonction.
			for (int i=0; i<l; i++) {
				if (entry[i] == null) { split = true; start2 = i + 1; len2 = l - start2; break; }
				len1++; }
			if (!split)
				{ arr1 = entry; arr2 = new T[0]; return; }
			else
			{
				arr1 = new T[len1]; arr2 = new T[len2];
				Array.Copy(entry, start1, arr1, 0, len1);
				Array.Copy(entry, start2, arr2, 0, len2);
			}
		}


		// ---------------------------------------------------------------------------
		

		/// <summary>
		/// Déroule des éléments ou des tableaux contenu dans elements en un seul tableau. Si T est objects, elements peut contenir des objects ou des tableaux à une dimension.
		/// </summary>
		public static T[] UnrollArray<T>(Array elements)
		{
			int l = elements.Length, c = 0;
			T[] arr = new T[10], temp;
			for (int i=0; i<l; i++)
			{
				if (elements.GetValue(i) is Array) {
					temp = UnrollArray<T>((Array)elements.GetValue(i));
					if (c + temp.Length >= arr.Length) { Array.Resize(ref arr, arr.Length + temp.Length + 10); }
					Array.Copy(temp, 0, arr, c, temp.Length);
					c += temp.Length; }
				else {
					if (c >= arr.Length) { Array.Resize(ref arr, c + 10); }
					arr[c++] = (T)elements.GetValue(i); }
			}
			Array.Resize(ref arr, c);
			return arr;
		}
		
		
		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Exécute un délégué sur chacun des objets du tableau passé en argument, y compris les sous-tableaux et les tableaux déchiquetés. Chaque élément est remplacé par la valeur de retour du délégué.
		/// </summary>
		public static Array RecursiveArrayBrowsing(Array array, Func<object,object> action)
		{
			int l = array.Length;
			for (int i=0; i<l; i++)
			{
				if (array.GetValue(i) is Array)
					{ array.SetValue(RecursiveArrayBrowsing((Array)array.GetValue(i), action), i); }
				else
					{ array.SetValue(action(array.GetValue(i)), i); }
			}
			return array;
		}

		/// <summary>
		/// Exécute un délégué sur chacun des objets du tableau passé en argument, y compris les sous-tableaux et les tableaux déchiquetés.
		/// </summary>
		public static void RecursiveArrayBrowsing(Array array, Action<object> action)
		{
			int l = array.Length;
			for (int i=0; i<l; i++)
			{
				if (array.GetValue(i) is Array)
					{ RecursiveArrayBrowsing((Array)array.GetValue(i), action); }
				else
					{ action(array.GetValue(i)); }
			}
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Affiche tous les composants d'un tableau, à la manière d'un tableau déchiqueté, en parcourant tous les sous-tableaux (s'ils existes). Retourne les lignes.
		/// </summary>
		public static string[] ShowJaggedArray(Array array, int tab)
		{
			object[] jagArr = ShowJaggedArray(array, 0, tab);
			string[] res = new string[20]; int c = 0;
			Action<object> action = delegate(object o)
				{
					if (c >= res.Length) { Array.Resize(ref res, c + 20); }
					res[c++] = o.ToString();
				};
			RecursiveArrayBrowsing(jagArr, action);
			Array.Resize(ref res, c);
			return res;
		}
		
		/// <summary>
		/// Méthode privée récursive pour ShowJaggedArray.
		/// </summary>
		private static object[] ShowJaggedArray(Array array, int index, int tab)
		{
			int l = array.Length, c = 0; object[] res = new object[10];
			for (int i=0; i<l; i++)
			{
				if (array.GetValue(i) is Array) {
					if (c >= res.Length) { Array.Resize(ref res, c + 10); }
					res[c++] = String.Format("{0}[{1}] => (Array: {2})", "".PadLeft(tab*index, ' '), i, ((Array)array.GetValue(i)).Length);
					if (c >= res.Length) { Array.Resize(ref res, c + 10); }
					res[c++] = ShowJaggedArray((Array)array.GetValue(i), index + 1, tab); }
				else {
					if (c >= res.Length) { Array.Resize(ref res, c + 10); }
					res[c++] = String.Format("{0}[{1}] => {2}", "".PadLeft(tab*index, ' '), i, array.GetValue(i).ToString()); }
			}
			Array.Resize(ref res, c);
			return res;
		}
	
		/// <summary>
		/// Affiche tous les composants d'un dictionnaire, où la valeur (type object) est un autre dictionnaire de même type, et ainsi de suite de façon récursive.
		/// </summary>
		public static string[] ShowJaggedArray(SortedDictionary<string,object> elements, int tab)
		{
			object[] jagArr = ShowJaggedArray(elements, 0, tab);
			string[] res = new string[20]; int c = 0;
			Action<object> action = delegate(object o)
				{
					if (c >= res.Length) { Array.Resize(ref res, c + 20); }
					res[c++] = o.ToString();
				};
			RecursiveArrayBrowsing(jagArr, action);
			Array.Resize(ref res, c);
			return res;
		}
		
		/// <summary>
		/// Méthode privée récursive pour ShowJaggedArray.
		/// </summary>
		private static object[] ShowJaggedArray(SortedDictionary<string,object> elements, int index, int tab)
		{
			int l = elements.Count, c = 0; object[] res = new object[10];
			for (int i=0; i<l; i++)
			{
				KeyValuePair<string,object> pair = elements.ElementAt(i);
				if (((SortedDictionary<string,object>)pair.Value).Count > 0) {
					if (c >= res.Length) { Array.Resize(ref res, c + 10); }
					res[c++] = String.Format("{0}[{1}] => {2}", "".PadLeft(tab*index, ' '), i, pair.Key);
					if (c >= res.Length) { Array.Resize(ref res, c + 10); }
					res[c++] = ShowJaggedArray((SortedDictionary<string,object>)pair.Value, index + 1, tab); }
				else {
					if (c >= res.Length) { Array.Resize(ref res, c + 10); }
					res[c++] = String.Format("{0}[{1}] => {2}", "".PadLeft(tab*index, ' '), i, pair.Key); }
			}
			Array.Resize(ref res, c);
			return res;
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Fait une liste de type imbriqué, en cherchant tous les types enfants de currentType. Retourne un dictionnaire où la clé est le nom du en cours, et la valeur un dictionnaire de même type, contenant en clé le sous-type courant, et un valeur un autre dictionnaire, et ainsi de suite de façon récursive. On peut ensuite l'affiché avec ShowJaggedArray.
		/// </summary>
		public static SortedDictionary<string,object> MakeTypesList(ref Type[] types, Type currentType)
		{
			SortedDictionary<string,object> dic = new SortedDictionary<string,object>();
			dic.Add(currentType.Name, MakeTypesListPrivate(ref types, currentType));
			return dic;
		}
		
		/// <summary>
		/// Méthode privée pour MakeTypesList
		/// </summary>
		private static SortedDictionary<string,object> MakeTypesListPrivate(ref Type[] types, Type currentType)
		{
			int l = types.Length; SortedDictionary<string,object> dic = new SortedDictionary<string,object>();
			for (int i=0; i<l; i++)
			{
				if (types[i].BaseType == currentType)
				{
					dic.Add(types[i].Name, MakeTypesListPrivate(ref types, types[i]));
				}
			}
			return dic;
		}
		

	}



	// ===========================================================================
	
	

	/// <summary>
	/// Fournit des méthodes pour parser un texte en champs.
	/// </summary>
	public static class FieldsParser
	{


	
		/// <summary>
		/// Parse un texte en différents champs, en utilisant le TextFieldParser de VB. Les contraintes à respecter, comme les échappements, sont donc ceux de cette classe. Si text est vide ou null, retourne un tableau vide (0 élément).
		/// </summary>
		public static string[] ParseTextUsingVB(string text, string delimiter)
		{
			// Sort si pas de texte:
			if (String.IsNullOrEmpty(text)) { return new string[0]; }
			// Utilise le TextFieldParser de VB, mais avec un MemoryStream et non un fichier.
			MemoryStream ms = new MemoryStream();
			byte[] data = Encoding.UTF8.GetBytes(text);
			ms.Write(data, 0, data.Length); ms.Position = 0;
			TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(ms, Encoding.UTF8);
			parser.TextFieldType = FieldType.Delimited;
			parser.SetDelimiters(delimiter);
			parser.TrimWhiteSpace = true;
			try {	return parser.ReadFields(); }
			catch { return null; }
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Parse un texte pour retourner un tableau de champs String. Retourne null en cas d'erreur. text est le texte à parser, delimiter le délimiteur de champ et trim indique s'il faut faut supprimer les espaces avant et après chaque champ. Ce parser utilise les des guillemets ouvrants et fermants : “ et ” (les guillemets ‘ et ’ peuvent aussi être utilisés, mais cela n'est qu'une aide pour l'utilisateur, puisqu'ils sont remplacés par des guillemets double avant traitement). Le principe est celui des parenthèses: chaque nouveau guillemet ouvrant représente une chaîne dans laquelle les guillemets ne sont plus analysés. Autrement dit, lors de l'analyse présente, seuls les guillemets les plus extérieurs servent à caractériser des champs, et les guillemets internes sont ignorés. Les guillemets doivent toujours ouvrir et fermer un champ: ils ne peuvent se trouver au milieu d'un champ (sauf s'ils sont à l'intérieu d'autre guillemets). Un champ qui ne contient pas de délimiteur ou pas d'autre guillemets peut ne pas être entouré de guillemets. Les guillemets normaux " peuvent être utilisés pour échapper l'ensemble d'un champ. Dans ce cas, il ne peut y avoir d'autres guillemets normaux à l'intérieur du champ (mais il peut y avoir des guillemets ouvrants et fermants), et il ne peuvent pas être échappés. Si ce sont des guillemets ouvrants et fermants qui ouvrent et ferment un champ, alors il peut y avoir autant de guillements normaux à l'intérieur du champ : ils sont considérés comme tout autre caractère. Il n'est pas possible d'échapper les guillemets ouvrants, fermants, ou normaux. Si le texte est vide, retourne un tableau à 0 élément. Exception si text est null.
		/// </summary>
		public static string[] ParseText(string text, string delimiter, bool trim)
		{
		
			string[] result = new string[10]; string ch, ch2, chs = String.Empty;
			int len = text.Length, c = 0, qCounter = 0; bool normalQ;
			
			// Sort si null ou empty:
			if (String.IsNullOrEmpty(text)) { return new string[0]; }
			
			// Les guillemets simples sont considérés comme égaux aux guillemets doubles:
			text = text.Replace("‘", "“").Replace("’", "”");
			
			// Pour tous les caractères du texte:
			c = 0;
			for (int i=0; i<len; i++)
			{
				// Obtient le caractère:
				ch = text.Substring(i, 1); normalQ = false;
				// Si c'est un délimiteur, enregistre chs dans le champ courant et passe au suivant:
				if (ch == delimiter)
				{
					if (c >= result.Length) { Array.Resize(ref result, c + 10); }
					result[c++] = chs;
					chs = String.Empty;
				}
				// Si guillemet ouvrant, on parcours le texte jusqu'à la fin du champ:
				else if (ch == "“" || ((normalQ = (ch == "\"")) && chs == String.Empty))
				{
					// Sort si chs n'est pas vide (car ce serait un guillemet de début de champ à l'intérieur d'un champ):
					if (chs != String.Empty) { return null; }
					qCounter = 0;
					// Parcours le texte, jusqu'à trouver le guillemet fermant correspondant (fermeture du champ):
					for (int j=i; j<len; j++)
					{
						ch2 = text.Substring(j, 1);
						if (!normalQ) { if (ch2 == "“") { qCounter++; } else if (ch2 == "”") { qCounter--; } }
						else { if (ch2 == "\"" && i == j) { qCounter++; } else if (ch2 == "\"") { qCounter--; } }
						// Si qCounter est négatif, il y a un problème dans l'ordre d'apparition des guillemets:
						if (qCounter < 0) { return null; }
						else if (qCounter == 0)
						{
							// Si le caractère suivant n'est pas délimiteur, ou si on n'est pas à la fin, on sort (car ce
							// serait un guillemet de fin de champ à l'intérieur d'un champ):
							if (j < len-1 && text.Substring(j+1, 1) != delimiter) { return null; }
							// Inscrit le texte dans chs:
							chs = text.Substring(i + 1, j - i - 1);
							i = j;
							break;
						}
					}
					// Si qCounter est différent de 0, c'est qu'il y a un problème:
					if (qCounter != 0) { return null; }
				}
				// Si guillemet fermant, c'est qu'il y a une erreur:
				else if (ch == "”")
				{
					return null;
				}
				// Sinon, enregistre le caractère et continue:
				else
				{
					chs += ch;
				}
			}
			// A la fin, on enregistre le dernier champs:
			if (c >= result.Length) { Array.Resize(ref result, c + 10); }
			result[c++] = chs;
			
			// Retaille le tableau et sort:
			Array.Resize(ref result, c);
			if (trim) { for (int i=0; i<c; i++) { result[i] = result[i].Trim(); } }
			return result;
			
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Retourne l'index du guillemet fermant correspond au guillemet ouvrant signalé par startIndex, en ignorant les guillemets de niveaux plus élevés (les guillemets internes). Il est logique que startIndex désigne l'index de guillemets ouvrants. Si ce n'est pas le cas, startIndex est retourné. Si startIndex pointe sur un guillemet normal ", alors la valeur retourné est celle du prochain guillemet normal. Retourne -1 en cas d'erreur. Pour retrouver le texte compris entre les guillemets, cad le champ, la formule est text.Substring(startIndex + 1, endIndex - startIndex - 1), où endIndex est la valeur retournée par la fonction.
		/// </summary>
		public static int GetSubfield(string text, int startIndex)
		{
			// Si guillemets normaux :
			if (text.Substring(startIndex, 1) == "\"") { return text.IndexOf("\"", startIndex+1); }
			// Les guillemets simples sont considérés comme égaux aux guillemets doubles:
			text = text.Replace("‘", "“").Replace("’", "”");
			// Parcours le texte, jusqu'à trouver le guillemet fermant correspondant (fermeture du champ):
			int qCounter = 0; int len = text.Length; string ch;
			for (int i=startIndex; i<len; i++)
			{
				ch = text.Substring(i, 1);
				if (ch == "“") { qCounter++; } else if (ch == "”") { qCounter--; }
				// Si qCounter est négatif, il y a un problème dans l'ordre d'apparition des guillemets:
				if (qCounter < 0) { return -1; }
				else if (qCounter == 0) { return i; }
			}
			// Retourne -1 en cas d'erreur:
			return -1;
		}
		

		/// <summary>
		/// Voir GetSubfield. Cette surcharge vérifie que startIndex et endIndex (la valeur retournée) correspondent bien à un début et à une fin de champ. Si ce n'est pas le cas, retourne -1.
		/// </summary>
		public static int GetSubfield(string text, int startIndex, string delimiter)
		{
			// Obtient l'index de fin:
			int endIndex = GetSubfield(text, startIndex);
			if (endIndex == -1) { return -1; }
			// Vérifie que ce sont bien des délimiteurs de champs:
			if ((startIndex != 0 && text.Substring(startIndex-1, 1) != delimiter)
				|| (endIndex != text.Length-1 && text.Substring(endIndex+1, 1) != delimiter)) { return -1; }
			return endIndex;
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Echappe un champ: s'il contient un délimiteur, des guillemets ouvrants ou fermants, ou s'il commence ou termine par des guillemets normaux, ils est entouré de guillemets ouvrants et fermants. Sinon, le texte n'est pas échappé.
		/// </summary>
		public static string EscapeField(string field, string delimiter)
		{
			// S'il y a des guillemets ou un délimiteur, ou le champ commence ou termine par un guillemet normal, on échappe l'ensemble:
			if (field.Contains("‘") || field.Contains("’") || field.Contains("“") || field.Contains("”") || field.Contains(delimiter)
				|| field.StartsWith("\"") || field.EndsWith("\""))
				{ field = "“" + field + "”"; }
			return field;
		}
		
		
		/// <summary>
		/// Echappe l'ensemble des champs (voir EscapeField) et retourne une champ où les champs sont séparés par délimiter.
		/// </summary>
		public static string EscapeFields(string delimiter, params object[] fields)
		{
			int len = fields.Length; StringBuilder sb = new StringBuilder();
			for (int i=0; i<len; i++) { sb.Append(EscapeField(fields[i].ToString(), delimiter) + (i!=len-1 ? delimiter : String.Empty)); }
			return sb.ToString();
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Echappe tout un tableau. On obtient quelque chose du genre “X,O,Y,Z”,““X,O,Z”,“O,Y,Z”,“X,Y,Z”,“X,Y,O”” avec T[][], par exemple.
		/// </summary>
		public static string EscapeArray<T>(Array array, string delimiter, Func<T,string> converter, bool escapeEachElement)
		{
			int l = array.Length; StringBuilder sb = new StringBuilder(); string temp;
			for (int i=0; i<l; i++)
			{
				if (array.GetValue(i) is Array) {
					temp = EscapeArray((Array)array.GetValue(i), delimiter, converter, escapeEachElement);
					sb.Append(My.FieldsParser.EscapeField(temp, delimiter)); }
				else if (escapeEachElement) {
					sb.Append(My.FieldsParser.EscapeField(converter((T)array.GetValue(i)), delimiter)); }
				else {
					sb.Append(converter((T)array.GetValue(i))); }
				sb.Append(i!=l-1 ? delimiter : "");
			}
			return sb.ToString();
		}
	
	}



	// ===========================================================================
	
	
	
	public static class GeneralParser
	{
	

	
		/// <summary>
		/// Parse un texte pour obtenir une couleur. En cas d'erreur retourne false, et color est alors la couleur Black. Le texte peut être un nombre (int), le nom d'une couleur (propriété de Color), un nom suivi d'un nombre (alpha), trois nombres (RGB) ou quatre nombres (ARGB). Si "Empty", retourne une couleur Empty. Si le texte commence par TSL, on attend trois champs séparés par delimiter, sous la forme "TSL0:0:0", où le premier 0 représente la teinte (0 à 360), le deuxième la saturation (0 à 1) et le troisième la luminosité (0 à 1). Si getHSL vaut true, alors le tableau hsl est rempli, soit avec les valeurs TSL donné par l'utilisateur, soit avec les valeurs TSL de la classe Color (si la couleur n'a pas été définie avec des données TSL par l'utilisateur). Si getHSL vaut false, le tableau hsl vaut null.
		/// </summary>
		public static bool ColorParser(string text, string delimiter, bool getHSL, out Color color, out float[] hsl)
		{
		
			// Valeur par défaut:
			color = Color.Black; hsl = null; string[] split;
			
			// Sort si Empty:
			if (text.Equals("Empty", StringComparison.CurrentCultureIgnoreCase)) { color = Color.Empty; return true; }
			
			// Si TSL:
			if (text.StartsWith("TSL"))
			{
				HSLColor testHSL;
				if (HSLColor.TryParse(text.Substring(3), out testHSL)) {
					hsl = new float[]{testHSL.H, testHSL.S, testHSL.L};
					color = testHSL.Color; return true; }
				else { return false; }
			}
			
			// Split en un tableau:
			split = text.Split(new string[]{delimiter}, StringSplitOptions.RemoveEmptyEntries);
			// Si plus de 4 éléments, sort:
			if (split.Length > 4 || split.Length == 0) { return false; }
			// Si 3 ou 4 éléments, convertit en byte:
			if (split.Length == 3 || split.Length == 4)
			{
				byte[] argb = new byte[split.Length]; byte bytTest;
				for (int k=0; k<split.Length; k++) {
					if (!Byte.TryParse(split[k], out bytTest)) { return false; }
					argb[k] = bytTest; }
				if (split.Length == 3) { color = Color.FromArgb(argb[0], argb[1], argb[2]); }
				else { color = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]); }
			}
			// Si 2 élément, le premier doit être le nom d'une couleur, et le second un byte (alpha), et si
			// un seul élément, ce peut être un int ou un nom:
			else
			{
				int intTest;
				if (split.Length == 1 && Int32.TryParse(split[0], out intTest))
					{ color = Color.FromArgb(intTest); }
				else
				{
					// La méthode FromName fournit une couleur non Empty, même si on entre n'importe quoi... D'où
					// le tour de passe-passe suivant:
					Color c = Color.FromName(split[0]); Byte bytTest;
					if (typeof(Color).GetProperty(c.Name) == null) { return false; }
					if (split.Length == 2) {
						if (!Byte.TryParse(split[1], out bytTest)) { return false; }
						color = Color.FromArgb(bytTest, c); }
					else { color = c; }
				}
			}
			if (getHSL) { hsl = new float[]{color.GetHue(), color.GetSaturation(), color.GetBrightness()}; }
			return true;
			
		}
		
		/// <summary>
		/// Voir surcharge. getHSL vaut ici false.
		/// </summary>
		public static bool ColorParser(string text, string delimiter, out Color color)
			{ float[] tmp; return ColorParser(text, delimiter, false, out color, out tmp); }
		
		
		// ---------------------------------------------------------------------------

	
		private static string[] _fontNames;
		/// <summary>
		/// Parse un texte vers un Font. text peut être "name", "name:size", ou "name:size:FontStyle". Si FontStyle n'est pas reconnue, le Font est tout de même retourné avec un style regulier.
		/// </summary>
		public static bool FontParser(string text, string delimiter, float preferedSize, out Font font)
		{
			// Valeur par défaut:
			font = new Font("Arial", preferedSize);
			// Split en un tableau:
			string[] split = text.Split(new string[]{delimiter}, StringSplitOptions.RemoveEmptyEntries);
			// Si plus de 3 éléments, sort:
			if (split.Length > 3 || split.Length == 0) { return false; }
			// Cherche le tableau de toutes les polices et tente de trouver le nom de la police:
			if (_fontNames == null) { _fontNames = Functions.GetFontNames(); }
			string name = null;
			foreach (string s in _fontNames) { if (s.StartsWith(split[0], StringComparison.CurrentCultureIgnoreCase)) { name = s; break; } }
			if (name == null) { return false; }
			// Sort si un seul élément:
			if (split.Length == 1) { font = new Font(name, preferedSize); return true; }
			// Taille:
			float size;
			if (!Single.TryParse(split[1], out size)) { return false; }
			// Sort si deux éléments:
			if (split.Length == 2) { font = new Font(name, size); return true; }
			// Style:
			FontStyle style = FontStyle.Regular; FontStyle[] arr = (FontStyle[])Enum.GetValues(typeof(FontStyle));
			foreach (FontStyle fs in arr) { if (fs.ToString().StartsWith(split[2], StringComparison.CurrentCultureIgnoreCase)) { style = fs; break; } }
			font = new Font(name, size, style); return true;
		}
		
		/// <summary>
		/// Retourne le Font sous forme de chaîne, à la façon de FontParser.
		/// </summary>
		public static string GetFontDescription(Font font, string delimiter)
			{ return String.Format("{0}{1}{2}{3}", font.Name, delimiter, font.Size, (font.Style!=FontStyle.Regular ? delimiter + font.Style.ToString() : "")); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne un ImageFormat à partir d'une chaîne, qui peut être soit une valeur de l'énumération ImageFormat, soit une extension.
		/// </summary>
		public static bool ImageFormatParser(string s, out ImageFormat format)
		{
			s = s.ToLower();
			if (s.StartsWith(".") && s.Length > 1) { s = s.Substring(1); }
			switch (s) {
				case "bmp": format = ImageFormat.Bmp; return true;
				case "emf": format = ImageFormat.Emf; return true;
				case "exif": format = ImageFormat.Exif; return true;
				case "gif": format = ImageFormat.Gif; return true;
				case "icon": format = ImageFormat.Icon; return true;
				case "jpeg": format = ImageFormat.Jpeg; return true;
				case "jpg": format = ImageFormat.Jpeg; return true;
				case "memorybmp": format = ImageFormat.MemoryBmp; return true;
				case "png": format = ImageFormat.Png; return true;
				case "tiff": format = ImageFormat.Tiff; return true;
				case "wmf": format = ImageFormat.Wmf; return true; }
			format = null; return false;
		}

		/// <summary>
		/// Retourne l'extension à partir de ImageFormat.
		/// </summary>
		public static string GetExtensionFromImageFormat(ImageFormat format)
		{
			if (format == ImageFormat.Bmp) return "bmp";
			else if (format.Equals(ImageFormat.Emf)) return "emf";
			else if (format.Equals(ImageFormat.Exif)) return "exif";
			else if (format.Equals(ImageFormat.Gif)) return "gif";
			else if (format.Equals(ImageFormat.Icon)) return "ico";
			else if (format.Equals(ImageFormat.Jpeg)) return "jpg";
			else if (format.Equals(ImageFormat.MemoryBmp)) return "bmp";
			else if (format.Equals(ImageFormat.Png)) return "png";
			else if (format.Equals(ImageFormat.Tiff)) return "tiff";
			else if (format.Equals(ImageFormat.Wmf)) return "wmf";
			else { return String.Empty; }
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Retourne une chaîne décrivant le format ImageFormat passé en argument.
		/// </summary>
		public static string GetImageFormatDescription(ImageFormat format)
		{
			PropertyInfo[] props = typeof(ImageFormat).GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach (PropertyInfo pi in props) {
				if (((ImageFormat)pi.GetValue(null, null)).Guid.Equals(format.Guid)) { return pi.Name; } }
			return null;
		}


	}



	// ===========================================================================
	
	
	
	/// <summary>
	/// Founit des fonctions pour la gestion des couleurs.
	/// </summary>
	public static class ColorFunctions
	{
	
		// Déclarations:
		private static Color[] _colorsList, _originalOrderColorList;
		private static int[] _intColorsList;

		/// <summary>
		/// Obtient la liste des couleurs de la classe Color.
		/// </summary>
		public static Color[] ColorList { get { return _originalOrderColorList; } }

		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static ColorFunctions()
		{
			// Cherche toutes les propriétés static de la classe Color:
			PropertyInfo[] properties = (typeof(Color)).GetProperties(BindingFlags.Static | BindingFlags.Public);
			int nb = properties.Length;
			_colorsList = new Color[nb]; _originalOrderColorList = new Color[nb]; _intColorsList = new int[nb];
			for (int i=0; i<nb; i++) {
				_colorsList[i] = (Color)properties[i].GetValue(null, null);
				_originalOrderColorList[i] = _colorsList[i];
				_intColorsList[i] = _colorsList[i].ToArgb(); }
			Array.Sort(_intColorsList, _colorsList);
		}
		
		/// <summary>
		/// Retourne un Color de référence à partir d'une couleur, même si cette dernière a un canal alpha. En effet, la méthode supprime le canal alpha pour pouvoir comparer avec les Couleur de référence (qui ont un canal alpha à 255). Si la couleur n'existe pas, retourne Empty.
		/// </summary>
		public static Color GetRefColor(Color color)
		{
			// Si c'est une couleur transparente, retourne directement:
			if (Color.Transparent.ToArgb() == color.ToArgb()) { return Color.Transparent; }
			// Obtient une couleur avec le canal alpha à 255, puis obtient une valeur Int et compare aux couleurs connues:
			int val = Color.FromArgb(255, color).ToArgb(), index;
			if ((index = Array.BinarySearch(_intColorsList, Color.FromArgb(255, color).ToArgb())) < 0) { return Color.Empty; }
			return _colorsList[index];
		}
		
		/// <summary>
		/// Retourne une chaîne décrivant la couleur. Si la couleur est une couleur de référence, retourne "name". Si la couleur est une couleur de référence avec un canal alpha, retourne "name,alpha". Si la couleur n'est pas une couleur de référence, retourne "A,R,G,B". La virgule peut être remplacé par le paramètre separator. Si la structure est Empty, retourne "Empty".
		/// </summary>
		public static string GetColorDescription(Color color, string separator)
		{
			if (color.IsEmpty) { return "Empty"; }
			Color c = GetRefColor(color);
			if (c.IsEmpty) { return String.Format("{0}{1}{2}{3}{4}{5}{6}", color.A.ToString(), separator, color.R.ToString(), separator,
				color.G.ToString(), separator, color.B.ToString()); }
			else if (color.A == 255 || color == Color.Transparent) { return c.Name; }
			else { return String.Format("{0}{1}{2}", c.Name, separator, color.A.ToString()); }
		}
		
		/// <summary>
		/// Obtient les valeurs ARGB;
		/// </summary>
		public static void GetARGB(Color col, out byte A, out byte R, out byte G, out byte B)
		{
			byte[] dat = BitConverter.GetBytes(col.ToArgb());
			A = dat[3];
			R = dat[2];
			G = dat[1];
			B = dat[0];
		}

		
		/// <summary>
		/// Convertit de HSL vers RGB. H compris entre 0 et 360, S et L entre 0 et 1, sinon retourne false.
		/// </summary>
		public static bool ConvertFromHSLToRGB(float H, float S, float L, out int R, out int G, out int B)
		{
		
			// Sort si pas dans les normes ou des S = 0:
			R = G = B = 0;
			if (H < 0 || H > 360 || S < 0 || S > 1 || L < 0 || L > 1) { return false; }
			if (S == 0) { R = G = B = (int)(L * 255F); return true; }
			
			// Applique la formule (Wikipedia US):
			float q = 0, p = 0, h = 0;
			Func<float,float> func = delegate(float t)
				{
					if (t < 0) { t += 1; }
					if (t > 1) { t -= 1; }
					if (t * 6F < 1F) { return p + ( (q - p) * 6F  * t); }
					if (t < 0.5F) { return q; }
					if (t * 3F < 2F) { return p + ( (q - p) * 6F  * (2F/3F - t) ); }
					return p;
				};
			if (L < 0.5) { q = L * (1 + S); } else { q = L + S  - L * S; }	
			p = 2 * L - q;	
			h = H / 360F;	// Conversion en pourcentage
			R = (int)Math.Round(255 * func(h + (1F/3F)), 0);
			G = (int)Math.Round(255 * func(h), 0);
			B = (int)Math.Round(255 * func(h - (1F/3F)), 0);
			return true;
		}

	
	}



	// ===========================================================================
	
	
	
	/// <summary>
	/// Fournit des méthodes de mise en forme de texte et de nombre.
	/// </summary>
	public static class FormatFunctions
	{
	
		
		/// <summary>
		/// Contient les préfixes métriques de Y (1E24) à y (1E-24).
		/// </summary>
		public enum MetricSystemPrefixes
		{
			y = -8,
			z = -7,
			a = -6,
			f = -5,
			p = -4,
			n = -3,
			µ = -2,
			m = -1,
			none = 0,
			k = 1,
			M = 2,
			G = 3,
			T = 4,
			P = 5,
			E = 6,
			Z = 7,
			Y = 8,
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Retourne une chaîne contenant le nombre nb avec le nombre maximal de décimales decimalPlaces, en suivant les patterns intPatten (partie entière) et decPattern (partie décimale) utilisant la culture ci. Si le nombre ne contient pas de décimal, retourne simplement la partie entière. S'il y a moins de décimales que decimalPlaces, retourne simplement le nombre requis de décimal, sans ajouter de 0. S'il y a plus de décimales, la dernière renvoyée est arrondie. Si ci vaut null, la culture de l'application est utilisée.
		/// </summary>
		public static string NumberToString(double nb, int decimalPlaces, string intPattern, string decPattern, CultureInfo ci)
		{
			if (ci == null) { ci = Application.CurrentCulture; }
			if (nb - Math.Truncate(nb) == 0) { return nb.ToString(intPattern, ci); }
			else { return Math.Round(nb, decimalPlaces).ToString(decPattern, ci); }
		}
				
		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static string NumberToString(double nb, int decimalPlaces, CultureInfo ci)
			{ return NumberToString(nb, decimalPlaces, "#,0", "#,0.###############", ci); }

		/// <summary>
		/// Voir surcharge.
		/// </summary>		
		public static string NumberToString(double nb, int decimalPlaces)
			{ return NumberToString(nb, decimalPlaces, "#,0", "#,0.###############", null); }
		
		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static string NumberToString(double nb, int decimalPlaces, string intPattern, string decPattern)
			{ return NumberToString(nb, decimalPlaces, intPattern, decPattern, null); }


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Retourne une chaîne contenant le nombre val mis en forme avec un préfixe métrique. unit indique l'unité à utiliser et format le format. Si val=12340, unit="g" et format=0, retourne "12.34kg". Si format=1, retourne "12kg34". La mise en forme du nombre s'effectue avec NumberToString, et la définition du préfixe avec GetMetricSystemPrefix. Si ci est null, alors la culture de l'application est utilisée.
		/// </summary>
		public static string GetMetricSystemPrefix(decimal val, string unit, int format, CultureInfo ci)
		{
			decimal newVal; MetricSystemPrefixes pref;
			if (unit == null) { unit = ""; }
			GetMetricSystemPrefix(val, out newVal, out pref);
			string p = (pref == MetricSystemPrefixes.none ? "" : pref.ToString());
			string decSeparator = Application.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			string s = NumberToString((double)newVal, 3, ci);
			if (format == 0 || !s.Contains(decSeparator)) { return String.Format("{0}{1}", s, p + unit); }
			else { return s.Replace(decSeparator, p + unit); }
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Retourne dans newVal un nombre arrondi qui est la transformation de val adaptée au préfixe métrique pref. La partie entière a généralement au maximum 3 chiffre, sauf si val est un nombre plus grand que le plus grand préfixe de MetricSystemPrefixes. La partie décimale est toujours arrondie à 3 décimales (ou à 0, 1 ou 2 si 3 ne sont pas nécessaires). Si val est plus petit que le plus petit préfixe de MetricSystemPrefixes - 1 préfixe, alors retourne 0. (Par exemple, si le plus petit préfixe est µ et que val=123E-9, retourne 0.123, mais si val=123E-12, retourne 0.) 
		/// </summary>
		public static void GetMetricSystemPrefix(decimal val, out decimal newVal, out MetricSystemPrefixes pref)
		{
			int index = 0; decimal d = val;
			
			
			// Si la partie entière vaut 0, on descend dans les sous-multiples:
			if (val != 0 && Math.Truncate(val) == 0)
			{
				// Procédure pour supprimer la partie entière:
				Func<decimal,decimal> delIntegerPart = delegate(decimal r) { return r - Math.Truncate(r); };
				int intPart=0;
				// Boucle:
				while (true)
				{
					// Trois premiers chiffres après la virgule:
					intPart = (int)Math.Truncate(delIntegerPart(d)*1000);
					// Mise à jour de d:
					d *= 1000;
					// Décrémente le compteur:
					index--;
					// Si intPart n'est pas nul, alors il faut s'arrêté et sortir:
					if (intPart != 0 || index == -8)
					{
						newVal = (decimal)intPart + Decimal.Round(delIntegerPart(d), 3);
						pref = (MetricSystemPrefixes)index;
						return;
					}
					// Sinon, on continue:
				}
			}
			
			// Sinon, monte dans les multiplies:
			else
			{			
				// Récupère les 3 premiers chiffres après la virgule:
				int decPart = (int)Math.Truncate((val - Math.Truncate(val)) * 1000);
				// Boucle:
				while (true)
				{
					// S'il n'y a plus de chiffres significatifs avant les trois derniers chiffres avant la virgule,
					// sort en retournant le résultat:
					if (Math.Truncate(d/1000) == 0 || index == 8)
					{
						newVal = Math.Truncate(d) + (decimal)decPart / 1000;
						pref = (MetricSystemPrefixes)index;
						return;
					}
					// Sinon, continue en incrémentant l'index des préfixes:
					else
					{
						// Trois derniers chiffres avant la virgule:
						decPart = (int)Math.Truncate(((d/1000) - Math.Truncate(d/1000)) * 1000);
						// Met à jour d:
						d = d / 1000;
						// Incrémente:
						index++;
					}
				}
			}
		
		}
		
		
	}	
	

}
