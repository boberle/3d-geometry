using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My
{



	/// <summary>
	/// Fournit des méthodes statiques pour l'envoie de message.
	/// </summary>
	public static class GeoMsgSender
	{

		private static bool _showInfos, _showErrors;
		private static StringBuilder _sb;
		private static string _errorPrefix;
		private static bool _locked;

		// Délégués d'évenements:
		public delegate void InfosEventHandler(object sender, InfosEventArgs e);
		
		/// <summary>
		/// Se déclenche lorsqu'une information est fournit par le programme.
		/// </summary>
		public static event InfosEventHandler Infos;
		
		/// <summary>
		/// Se déclenche lorsqu'une erreur est rencontrée.
		/// </summary>
		public static event InfosEventHandler Error;
		

		/// <summary>
		/// Obtient ou définit s'il faut afficher les informations.
		/// </summary>
		public static bool ShowInfos {
			get { return _showInfos; }
			set { _showInfos = value; } }
		
		/// <summary>
		/// Obtient ou définit s'il faut afficher les erreurs.
		/// </summary>
		public static bool ShowErrors {
			get { return _showErrors; }
			set { _showErrors = value; } }
		
		/// <summary>
		/// Obtient ou définit le préfixe pour les erreurs.
		/// </summary>
		public static string ErrorPrefix {
			get { return _errorPrefix; }
			set { _errorPrefix = value; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		static GeoMsgSender()
		{
			_showInfos = true;
			_errorPrefix = "ERROR: ";
			_sb = new StringBuilder();
			_locked = false;
		}
	
		/// <summary>
		/// Sauve un message d'information.
		/// </summary>
		public static void SendInfos(object sender, string message)
		{
			if (!_showInfos) { return; }
			else if (_locked) { _sb.Append("\n" + message); }
			else if (Infos != null) { Infos(sender, new InfosEventArgs(message, false)); }
		}
		
		/// <summary>
		/// Sauve un message d'information.
		/// </summary>
		public static void SendError(object sender, string message)
		{
			if (!_showErrors) { return; }
			if (_locked) { _sb.Append("\n" + _errorPrefix + message); }
			else if (Error != null) { Error(sender, new InfosEventArgs(message, false)); }
		}
		
		/// <summary>
		/// Bloque le déclenchement des événements et stocke les messages. Ceux-ci sont disponibles via la méthode Reset.
		/// </summary>
		public static void LockEvents()
		{
			_locked = true;
		}
		
		/// <summary>
		/// Retourne les messages en attente dans le buffer, et le remet à zéro.
		/// </summary>
		public static string Reset(bool unlock)
		{
			string result = (_sb.Length > 0 ? _sb.Remove(0, 1) : _sb).ToString();
			_sb = new StringBuilder();
			if (unlock) { _locked = false; }
			return result;
		}


	}
	
	
	
	
}
