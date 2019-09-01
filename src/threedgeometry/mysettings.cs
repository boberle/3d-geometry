using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;


namespace ThreeDGeometry
{



	/// <summary>
	/// Paramètres pour cette application.
	/// </summary>
	internal static class MySettings
	{


		/// <summary>
		/// Fichier chargé par défaut lors de l'ouverture du programme.
		/// </summary>
		public static string DefaultFile
		{
			get {
				string dir = My.MySettingsWriter.GetString(true, "DefaultFile", String.Empty);
				if (!String.IsNullOrEmpty(dir) && Directory.Exists(dir)) { return dir; }
				return null; }
		}	

		/// <summary>
		/// Hauteur, en pourcentage sur 100, de la zone de dessin.
		/// </summary>
		public static float DrawingAreaPercentHeight
		{
			get {
				return (float)Math.Min(My.MySettingsWriter.GetDouble(true, "DrawingAreaPercentHeight", 0.8), 1); }
			set {
				My.MySettingsWriter.SetDouble(true, "DrawingAreaPercentHeight", value); }
		}
		
		/// <summary>
		/// Indique si les commandes doivent être affichées dans le console lors du chargement d'un fichier ou de l'utilisation des commandes Cancel/Restore.
		/// </summary>
		public static bool WriteCommandsWhenLoading {
			get {
				return My.MySettingsWriter.GetBoolean(true, "WriteCommandsWhenLoading", true); } }
	
	
	}
	
	
	
}
