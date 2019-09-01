using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;

namespace My
{



	/// <summary>
	/// Fournit des fonctions static d'écriture dans des fichiers INI. Les fonctions Set... et Get... stockent des valeurs de types correspondant.
	/// </summary>
	public class IniWriter
	{
	
	
	
	
	
		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS



		[DllImport("kernel32.dll")]
		static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

		[DllImport("kernel32.dll")]
		static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);



		#endregion DECLARATIONS



	
		// ---------------------------------------------------------------------------
		// METHODES D'ECRITURE ET DE LECTURE
		// ---------------------------------------------------------------------------




		#region METHODES D'ECRITURE ET DE LECTURE



		
		/// <summary>
		/// Ecrit une chaîne dans le fichier ini. Vérifie auparavant l'existe du dossier de file, et le crée. Puis crée le fichier s'il n'existe pas.
		/// </summary>
		public static bool SetString(string file, string section, string key, string value)
		{
			// Vérifie que le dossier existe, sinon le créé:
			if (Directory.Exists(Path.GetDirectoryName(file)) == false)
			{
				try { Directory.CreateDirectory(new FileInfo(file).DirectoryName); }
				catch (Exception exc) { My.ErrorHandler.ShowError(exc); }
			}
			return WritePrivateProfileString(section, key, value, file);
		}

		/// <summary>
		/// Lit une chaîne dans le fichier ini. Par défaut, le buffer est de 500 : les chaînes retournée auront au maximum 500 caractères, ce qui peut être changé si bufLength est spécifié.
		/// </summary>
		public static string GetString(string file, string section, string key, string defaultValue)
		{
			return GetString(file, section, key, defaultValue, 500);
		}

		/// <summary>
		/// Voir surcharge. Définit un buffer.
		/// </summary>
		public static string GetString(string file, string section, string key, string defaultValue, int bufLength)
		{
			StringBuilder buf = new StringBuilder(" ".PadLeft(bufLength));
			int total = GetPrivateProfileString(section, key, defaultValue, buf, bufLength, file);
			return buf.ToString().Substring(0, total);
		}


		// ---------------------------------------------------------------------------


		public static bool SetInt32(string file, string section, string key, int value)
		{
			return SetString(file, section, key, value.ToString());
		}

		public static int GetInt32(string file, string section, string key, int defaultValue)
		{
			string res = GetString(file, section, key, defaultValue.ToString());
			int result;
			if (Int32.TryParse(res, out result)) { return result; }
			else { return defaultValue; }
		}




		// ---------------------------------------------------------------------------


		public static bool SetDouble(string file, string section, string key, double value)
		{
			return SetString(file, section, key, value.ToString("R20"));
		}

		public static double GetDouble(string file, string section, string key, double defaultValue)
		{
			string res = GetString(file, section, key, defaultValue.ToString("R20"));
			double result;
			if (Double.TryParse(res, out result)) { return result; }
			else { return defaultValue; }
		}


		// ---------------------------------------------------------------------------


		public static bool SetColor(string file, string section, string key, Color value)
		{
			return SetString(file, section, key, ColorFunctions.GetColorDescription(value, ","));
		}

		public static Color GetColor(string file, string section, string key, Color defaultValue)
		{
			string res = GetString(file, section, key, defaultValue.Name);
			Color color;
			if (!GeneralParser.ColorParser(res, ",", out color)) { return defaultValue; }
			return color;
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Enregistre un tableau. Si value est null, le tableau est effacé. Si value est un tableau de 0 élément, alors la valeur de retour (par Get) sera un tableau de 0 élément.
		/// </summary>
		public static bool SetStringArray(string file, string section, string key, string[] value)
		{
			// Cherche s'il a y déjà un tableau d'inscrit:
			string[] eraser = GetStringArray(file, section, key, null);
			if (eraser != null)
			{
				// Efface ce tableau:
				SetString(file, section, key + "_arr_length", null);
				for (int i=0; i<eraser.Length; i++) { SetString(file, section, key + "_arr" + i.ToString(), null); }
			}
			// Si value est null, on a supprimé le tableau, et maintenant on sort:
			if (value == null) { return true; }
			// Inscrit le nombre d'éléments:
			if (SetString(file, section, key + "_arr_length", value.Length.ToString()) == false) { return false; }
			// Inscrit chaque élément:
			for (int i=0; i<value.Length; i++)
				{ if (SetString(file, section, key + "_arr" + i.ToString(), value[i]) == false) { return false; } }
			// Retour si ok:
			return true;
		}

		/// <summary>
		/// Voir SetStringArray.
		/// </summary>
		public static string[] GetStringArray(string file, string section, string key, string[] defaultValue)
		{
			// Obtient le nombre d'éléments:
			int total = GetInt32(file, section, key + "_arr_length", -1);
			// Valeur par défaut si rien:
			if (total < 0) { return defaultValue; }
			// Cherche chaque élément:
			string[] result = new string[total];
			for (int i=0; i<total; i++)
				{ result[i] = GetString(file, section, key + "_arr" + i.ToString(), String.Empty); }
			return result;
		}


		// ---------------------------------------------------------------------------


		public static bool SetBoolean(string file, string section, string key, bool value)
		{
			return SetInt32(file, section, key, (int)Convert.ChangeType(value, typeof(Int32)));
		}

		public static bool GetBoolean(string file, string section, string key, bool defaultValue)
		{
			int res = GetInt32(file, section, key, (int)Convert.ChangeType(defaultValue, typeof(Int32)));
			if ((res != 0) && (res != 1)) { return defaultValue; }
			else { return (bool)Convert.ChangeType(res, typeof(Boolean)); }
		}


		// ---------------------------------------------------------------------------


		public static bool SetFont(string file, string section, string key, Font value)
		{
			return SetString(file, section, key, GeneralParser.GetFontDescription(value, ","));
		}

		public static Font GetFont(string file, string section, string key, Font defaultValue)
		{
			string res = GetString(file, section, key, defaultValue.Name);
			Font font;
			if (!GeneralParser.FontParser(res, ",", defaultValue.Size, out font)) { return defaultValue; }
			return font;
		}



		#endregion METHODES D'ECRITURE ET DE LECTURE



	}



}
