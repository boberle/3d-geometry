using System;
using System.IO;
using System.Threading;

namespace My
{



	/// <summary>
	/// Paramètres pour cet assemblage.
	/// </summary>
	internal static class MySettings
	{


		/// <summary>
		/// Nom de la culture sous la forme "en-US".
		/// </summary>
		public static string CultureName {
			get { return My.MySettingsWriter.GetString(true, "CultureName", Thread.CurrentThread.CurrentCulture.Name); } }


		/// <summary>
		/// Obtient ou définit le répertoire courant. Environment.SpecialFolder.DesktopDirectory par défaut.
		/// </summary>
		public static string CurrentDirectory
		{
			get {
				string res = My.MySettingsWriter.GetString(true, "CurrentDirectory",
					Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
				if (Directory.Exists(res)) { return res; }
				else { return Environment.CurrentDirectory; } }
			set {
				My.MySettingsWriter.SetString(true, "CurrentDirectory", value); }
		}		
		
		
		/// <summary>
		/// Indique s'il faut enregistrer les erreurs dans un journal.
		/// </summary>
		public static bool SaveErrors {
			get { return My.MySettingsWriter.GetBoolean(true, "SaveErrors", false); } }

	}




}
