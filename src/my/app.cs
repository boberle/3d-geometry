using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Drawing;
using System.Diagnostics;

namespace My
{





	/// <summary>
	/// Classe statique qui regroupe les variables, objets, méthodes, etc. propre à toutes les applications. Lorsque le constructeur static est appelé, cherche dans les stettings le nom de la culture, et change s'il y a un setting qui est trouvé. N'affiche pas de message d'erreur si la chaîne des settings n'est pas valide.
	/// </summary>
	public static class App
	{
	
	
		// ---------------------------------------------------------------------------
		// DECLARATIONS
		
		private static string __Title;


		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS


		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static App()
		{
			DefaultEncoding = Encoding.UTF8;
			UserDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
														+ @"\Bruno Oberle\" + GetEntryAssemblyName();
			UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
														+ @"\Bruno Oberle\" + GetEntryAssemblyName();
			SharedFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
														+ @"\Bruno Oberle\Shared";
			Directory.CreateDirectory(SharedFolder);
			Name = GetEntryAssemblyName();
			Title = GetEntryAssemblyName();
			ChangeCulture(MySettings.CultureName);
			SetCurrentDirectory(null);
		}


		// ---------------------------------------------------------------------------
		// PROPRIETES


		/// <summary>
		/// Obtient ou définit le titre de l'application par défaut, ce qui est utilisé notamment par défaut dans les boîtes de dialogue, etc.
		/// </summary>
		public static string Title {
			get { return __Title + (DebugMode ? " (debug version)" : ""); }
			set { __Title = value; } }

		/// <summary>
		/// Obtient ou définit le nom de l'application par défaut, utilisé par exemple pour AppUserPath, etc. Par défaut: nom de l'assemblage d'entrée.
		/// </summary>
		public static string Name { get; set; }

		/// <summary>
		/// Obtient le chemin d'accès de l'exécutable. Il s'agit de Application.StartupPath.
		/// </summary>
		public static string ExePath { get { return System.Windows.Forms.Application.StartupPath; } }

		/// <summary>
		/// Encoding a utilisé par défaut. Par défaut, il s'agit d'UTF-8.
		/// </summary>
		public static Encoding DefaultEncoding { get; set; }

		/// <summary>
		/// Répertoire où l'application peut enregistrer des données systèmes spécifiques à l'utilisateur. Par défaut: ApplicationData\Bruno Oberle\assemblyName.
		/// </summary>
		public static string UserDataFolder { get; set; }

		/// <summary>
		/// Répertoire où l'application peut enregistrer des données personnelles spécifiques à l'utilisateur. Par défaut: My Documents\Bruno Oberle\assemblyName.
		/// </summary>
		public static string UserFolder { get; set; }

		/// <summary>
		/// Répertoire où sont enregistrés des documents communs à tous mes programmes. Par défaut: ApplicationData\Bruno Oberle\Shared.
		/// </summary>
		public static string SharedFolder { get; set; }

		/// <summary>
		/// Icon de l'application. Null par défaut.
		/// </summary>
		public static Icon DefaultIcon { get; set; }

		/// <summary>
		/// Obtient si la compilation est en Debug mode (cad si la constante DEBUG est définie);
		/// </summary>
		public static bool DebugMode
		#if DEBUG
		{ get { return true; } }
		#else
		{ get { return false; } }
		#endif


		// ---------------------------------------------------------------------------
		// MEHTODES


		/// <summary>
		/// Changer la culture sur le thread en cours. Gère les exceptions. L'argument doit être la nouvelle culture, de forme "en-US" ou "fr-FR", "el-GR", etc. Si showException est true, un message d'erreur s'affiche s'il y a une erreur (e.g. si la chaîne de culture n'est pas valide).
		/// </summary>
		public static void ChangeCulture(string culture)
		{
			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
				Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture); }
			catch (Exception exc) { My.ErrorHandler.ShowError(exc); }
		}

		/// <summary>
		/// Nom de l'EntryAssembly.
		/// </summary>
		public static string GetEntryAssemblyName()
			{ return System.Reflection.Assembly.GetEntryAssembly().GetName().Name; }

		/// <summary>
		/// Nom du CallingAssembly.
		/// </summary>
		public static string GetCallingAssemblyName()
			{ return System.Reflection.Assembly.GetCallingAssembly().GetName().Name; }

		/// <summary>
		/// Version de l'EntryAssembly.
		/// </summary>
		public static string GetEntryAssemblyVersion()
			{ return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString(); }

		/// <summary>
		/// Retourne true s'il existe déjà une instance du processus en cours, false sinon.
		/// </summary>
		public static bool AlreadyAnInstance()
			{ Process tmp; return AlreadyAnInstance(out tmp); }

		/// <summary>
		/// Retourne true s'il existe déjà une instance du processus en cours, false sinon.
		/// </summary>
		public static bool AlreadyAnInstance(out Process existingInstance)
		{
			// Obtient le processus courant et la liste des processus:
			existingInstance = null;
			Process currentProcess = Process.GetCurrentProcess();
			Process[] list = Process.GetProcesses();
			// Cherche s'il y a un processus du même nom que le courant, et retourne true et le Process:
			foreach (Process i in list) {
				if ((currentProcess.Id != i.Id) && (currentProcess.ProcessName.Equals(i.ProcessName)))
					{ existingInstance = i; return true; } }
			// Retourne false si rien trouvé:
			return false;
		}

		/// <summary>
		/// Definit le Environment.CurrentDirectory. Si path est null, c'est le chemin stocké dans les paramètres de l'application (s'il existe, sinon, c'est le bureau) qui est appliqué. Sinon, c'est path (s'il est valide) qui est utilisé, et enregistré dans les paramètres d'application.
		/// </summary>
		public static void SetCurrentDirectory(string path)
		{
			if (String.IsNullOrEmpty(path)) {
				Environment.CurrentDirectory = MySettings.CurrentDirectory; }
			else if (Directory.Exists(path)) {
				MySettings.CurrentDirectory = path;
				Environment.CurrentDirectory = MySettings.CurrentDirectory; }	
		}


	}


}
