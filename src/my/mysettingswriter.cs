using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Reflection;

namespace My
{


	/// <summary>
	/// Fournit des fonctions static d'écriture dans des fichiers INI, pour les settings. Deux fichiers sont utilisés : l'un pour les settings propres à l'utilisateur, stockés dans My.App.AppUserDataFolder + @"\settings.ini", l'autre pour les settings propres à l'application, stocké dans settings.ini dans le dossier de l'exe. Il est possible de modifier ces fichiers grâce aux propriétés correspondantes. Les fonctions Set... et Get... stockent des valeurs de types correspondant. Il faut à chaque fois définir si c'est un setting app ou user, avec la clé et la valeur (valeur par défaut pour Get..., retournée si le setting n'est pas défini ou s'il n'est pas valide). Si la section n'est pas passer en paramètre, alors le nom de l'assemblage de la procédure appelante est utilisé (recommandé).
	/// </summary>
	public static class MySettingsWriter
	{




		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS


		private static string _userFile;
		private static string _appFile;


		#endregion DECLARATIONS






		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES



		/// <summary>
		/// Obtient ou définit le fichier settings pour l'utilisateur. My.App.AppUserDataFolder + @"\settings.ini" par défaut. Si le fichier passé n'existe pas, la prop n'est pas modifiée.
		/// </summary>
		public static string UserSettingsFile
		{
			get { return _userFile; }
			set { if (File.Exists(value)) { _userFile = value; } }
		}

		/// <summary>
		/// Obtient ou définit le fichier settings pour l'application. settings.ini dans le dossier de l'exe par défaut. Si le fichier passé n'existe pas, la prop n'est pas modifiée.
		/// </summary>
		public static string AppSettingsFile
		{
			get { return _appFile; }
			set { if (File.Exists(value)) { _appFile = value; } }
		}



		#endregion PROPRIETES






		// ---------------------------------------------------------------------------
		// CONSTRUTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUTEURS



		/// <summary>
		/// Initialise les variables, notamment les fichiers. Les dossiers et les fichiers ne sont pas créés, mais ils le seront au premier appel d'une méthode Set, par la classe InitWriter.
		/// </summary>
		static MySettingsWriter()
		{
			// Initialisation des fichiers, et création des répertoire si n'existent pas:
			_userFile = My.App.UserDataFolder + @"\settings.ini";
			_appFile = My.App.ExePath + @"\settings.ini";
		}




		#endregion CONSTRUTEURS





		// ---------------------------------------------------------------------------
		// METHODES D'ECRITURE ET DE LECTURE
		// ---------------------------------------------------------------------------




		#region METHODES D'ECRITURE ET DE LECTURE



		public static bool SetString(bool user, string key, string value)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return SetString(user, section, key, value);
		}
		
		public static bool SetString(bool user, string section, string key, string value)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.SetString(fileName, section, key, value);
		}


		// ---------------------------------------------------------------------------


		public static string GetString(bool user, string key, string defaultValue)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return GetString(user, section, key, defaultValue);
		}
		
		public static string GetString(bool user, string section, string key, string defaultValue)
		{
			return GetString(user, section, key, defaultValue, 255);
		}


		public static string GetString(bool user, string key, string defaultValue, int bufLength)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return GetString(user, section, key, defaultValue, bufLength);
		}
		
		public static string GetString(bool user, string section, string key, string defaultValue, int bufLength)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.GetString(fileName, section, key, defaultValue, bufLength);
		}


		// ---------------------------------------------------------------------------


		public static bool SetInt32(bool user, string key, int value)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return SetInt32(user, section, key, value);
		}
		
		public static bool SetInt32(bool user, string section, string key, int value)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.SetInt32(fileName, section, key, value);
		}


		// ---------------------------------------------------------------------------


		public static int GetInt32(bool user, string key, int defaultValue)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return GetInt32(user, section, key, defaultValue);
		}
		
		public static int GetInt32(bool user, string section, string key, int defaultValue)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.GetInt32(fileName, section, key, defaultValue);
		}


		// ---------------------------------------------------------------------------


		public static bool SetDouble(bool user, string key, double value)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return SetDouble(user, section, key, value);
		}
		
		public static bool SetDouble(bool user, string section, string key, double value)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.SetDouble(fileName, section, key, value);
		}


		// ---------------------------------------------------------------------------


		public static double GetDouble(bool user, string key, double defaultValue)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return GetDouble(user, section, key, defaultValue);
		}
		
		public static double GetDouble(bool user, string section, string key, double defaultValue)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.GetDouble(fileName, section, key, defaultValue);
		}


		// ---------------------------------------------------------------------------


		public static bool SetColor(bool user, string key, Color value)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return SetColor(user, section, key, value);
		}
		
		public static bool SetColor(bool user, string section, string key, Color value)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.SetColor(fileName, section, key, value);
		}


		// ---------------------------------------------------------------------------


		public static Color GetColor(bool user, string key, Color defaultValue)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return GetColor(user, section, key, defaultValue);
		}
		
		public static Color GetColor(bool user, string section, string key, Color defaultValue)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.GetColor(fileName, section, key, defaultValue);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Enregistre un tableau. Si value est null, le tableau est effacé. Si value est un tableau de 0 élément, alors la valeur de retour (par Get) sera un tableau de 0 élément.
		/// </summary>
		public static bool SetStringArray(bool user, string key, string[] value)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return SetStringArray(user, section, key, value);
		}
		
		public static bool SetStringArray(bool user, string section, string key, string[] value)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.SetStringArray(fileName, section, key, value);
		}


		// ---------------------------------------------------------------------------


		public static string[] GetStringArray(bool user, string key, string[] defaultValue)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return GetStringArray(user, section, key, defaultValue);
		}
		
		public static string[] GetStringArray(bool user, string section, string key, string[] defaultValue)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.GetStringArray(fileName, section, key, defaultValue);
		}


		// ---------------------------------------------------------------------------


		public static bool SetBoolean(bool user, string key, bool value)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return SetBoolean(user, section, key, value);
		}
		
		public static bool SetBoolean(bool user, string section, string key, bool value)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.SetBoolean(fileName, section, key, value);
		}


		// ---------------------------------------------------------------------------


		public static bool GetBoolean(bool user, string key, bool defaultValue)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return GetBoolean(user, section, key, defaultValue);
		}
		
		public static bool GetBoolean(bool user, string section, string key, bool defaultValue)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.GetBoolean(fileName, section, key, defaultValue);
		}


		// ---------------------------------------------------------------------------


		public static bool SetFont(bool user, string key, Font value)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return SetFont(user, section, key, value);
		}
		
		public static bool SetFont(bool user, string section, string key, Font value)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.SetFont(fileName, section, key, value);
		}


		// ---------------------------------------------------------------------------


		public static Font GetFont(bool user, string key, Font defaultValue)
		{
			string section = Assembly.GetCallingAssembly().GetName().Name;
			return GetFont(user, section, key, defaultValue);
		}
		
		public static Font GetFont(bool user, string section, string key, Font defaultValue)
		{
			string fileName = ((user) ? _userFile : _appFile);
			return My.IniWriter.GetFont(fileName, section, key, defaultValue);
		}



		#endregion METHODES D'ECRITURE ET DE LECTURE





		// ---------------------------------------------------------------------------
		// AUTRES METHODES
		// ---------------------------------------------------------------------------




		#region AUTRES METHODES



		/// <summary>
		/// Supprime le fichier settings utilisateur, pour remettre les paramètres par défaut.
		/// </summary>
		public static void DeleteUserFile()
		{
			if (File.Exists(_userFile)) { try { File.Delete(_userFile); } catch { ; } }
		}

		/// <summary>
		/// Supprime le fichier settings app, pour remettre les paramètres par défaut.
		/// </summary>
		public static void DeleteAppFile()
		{
			if (File.Exists(_appFile)) { try { File.Delete(_appFile); } catch { ; } }
		}
		
		
		/*public static void RebuildFiles()
		{
			// Récupère tous les assemblies du programme:
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			assemblies = assemblies.Where(delegate(Assembly a) { return Array.ConvertAll(a.GetTypes(),
				delegate(Type t) { return t.Name; }).Contains("MySettings"); }).ToArray();
			// Obtient un fichier temporaire:
			FileInfo user = new FileInfo(Path.GetTempFileName());
			FileInfo app = new FileInfo(Path.GetTempFileName());
			// Pour chaque assemblage...
			foreach (Assembly a in assemblies)
			{
				// Récupère le fichier décrivant les settings par défaut, et les inclut aux fichiers:
				Type resources = a.GetTypes().First(delegate(Type t) { return t.Name == "MyResources"; });
				PropertyInfo pi;
				if (resources != null && (pi = resources.GetProperty("UserSettings")) != null)
				{
					// Ecrit dans le fichier temporaire:
					File.AppendAllText(user, pi.GetValue(null, null), Encoding.UTF8);
					// Met à jour les informations:
					
				}
			}
		}*/



		#endregion AUTRES METHODES



	}
	
	
}
