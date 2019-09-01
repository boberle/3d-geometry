using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;



namespace My
{





	/// <summary>
	/// Fournit des fonctions pour la gestion des erreurs.
	/// </summary>
	public static class ErrorHandler
	{
	
		// ---------------------------------------------------------------------------
		// SOUS CLASSES
		
		
		/// <summary>
		/// Dialogue d'affichage d'une erreur.
		/// </summary>
		private class DialogBoxError : MyFormMessage
		{
		
			private Button _cmdOK, _cmdMore;
			private TextBox _txtMsg;
			private string _shortMsg, _msg;
			private bool _isShort;
			
			/// <summary>
			/// Constructeur.
			/// </summary>
			public DialogBoxError()
			{
				// Initialisation du form et des contrôles:
				_cmdOK = new Button();
				_cmdOK.Text = MyResources.Common_cmd_OK;
				_cmdOK.Click += delegate { this.Hide(); };
				_cmdMore = new Button();
				_cmdMore.Text = MyResources.Common_cmd_More;
				_cmdMore.Click += new EventHandler(_cmdMore_Click);
				_txtMsg = new TextBox();
				_txtMsg.Multiline = true;
				_txtMsg.ReadOnly = true;
				SubtitleBox = MyResources.ErrorHandler_dialog_Subtitle;
				AddButtonsCollection(new ButtonsCollection(0, _cmdOK, _cmdMore), true);
				SetDialogIcon(DialogBoxIcon.Error);
				SetDialogMessage(MyResources.ErrorHandler_dialog_ErrorMessage);
				SetControl(_txtMsg);
			}

			/// <summary>
			/// Affichage du texte.
			/// </summary>
			private void _cmdMore_Click(object sender, EventArgs e)
			{
				_isShort = !_isShort;
				if (_isShort) { _txtMsg.Text = _shortMsg; _cmdMore.Text = MyResources.Common_cmd_More; }
				else { _txtMsg.Text = _msg; _cmdMore.Text = MyResources.Common_cmd_Less; }
			}

			/// <summary>
			/// Affiche un message d'erreur.
			/// </summary>
			public void ShowDialog(string shortMsg, string msg)
			{
				_shortMsg = shortMsg; _msg = msg; _isShort = true; _cmdMore.Text = MyResources.Common_cmd_More;
				_txtMsg.Text = shortMsg;
				base.ShowDialog();
			}
			
		}


		// ---------------------------------------------------------------------------
		// DECLARATIONS


		/// <summary>
		/// Paramètres d'événement pour ErrorOccurred.
		/// </summary>
		public class ErrorOccurredEventArgs : EventArgs
		{
			public Exception Exception { get; set; }
			public string ShortMessage { get; set; }
			public string Message { get; set; }
			public ErrorOccurredEventArgs(string shortMsg, string msg, Exception exc)
			{
				Exception = exc;
				ShortMessage = shortMsg;
				Message = msg;
			}
		}
		
		/// <summary>
		/// Délégué pour l'événement ErrorOccurred.
		/// </summary>
		public delegate void ErrorOccurredEventHandler(object sender, ErrorOccurredEventArgs e);
		
		/// <summary>
		/// Evénement se déclenchant quand une exception est levée, si ShowErrorMessage est faux.
		/// </summary>
		public static event ErrorOccurredEventHandler ErrorOccurred;

		// Autres variables:
		private static bool _showError, _writeInLog;
		private static string _logPath;
		private static DialogBoxError _dlgError;


		// ---------------------------------------------------------------------------
		// PROPRIETES


		/// <summary>
		/// Obtient ou définit si les messages sont affichés dans une boîte de dialogue, ou si un événement ErrorOccurred est déclenché.
		/// </summary>
		public static bool ShowErrorMsg {
			get { return _showError; }
			set { _showError = value; } }


		/// <summary>
		/// Retourne le chemin d'accès du journal.
		/// </summary>
		public static string LogPath { get { return _logPath; } }


		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS


		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static ErrorHandler()
		{
			_showError = true;
			_logPath = Path.Combine(App.SharedFolder, "err.log");
			_writeInLog = MySettings.SaveErrors;
			_dlgError = new DialogBoxError();
		}


		// ---------------------------------------------------------------------------
		// METHODES
		
		
		/// <summary>
		/// Déclenche l'événement.
		/// </summary>
		private static void OnErrorOccurred(string shortMsg, string msg, Exception exc)
		{
			if (ErrorOccurred != null) { ErrorOccurred(typeof(ErrorHandler), new ErrorOccurredEventArgs(shortMsg, msg, exc)); }
		}

		/// <summary>
		/// Affiche un message d'erreur ou déclenche l'événement ErrorOccurred.
		/// </summary>
		public static void ShowError(Exception exc, string info)
		{
			// Prépare le message:
			StringBuilder shortMsg = new StringBuilder(exc.Message);
			if (!String.IsNullOrEmpty(info)) { shortMsg.AppendFormat(" [{0}]", info); }
			StringBuilder msg = new StringBuilder(shortMsg.ToString());
			msg.AppendFormat(" ({0})", exc.GetType().Name);
			if (exc.Source != null || exc.TargetSite != null) {
				msg.AppendFormat("\n   In {0}.{1}", (exc.Source == null ? "Unknown" : exc.Source),
					(exc.TargetSite == null ? "Unknown" : exc.TargetSite.Name)); }
			if (exc.StackTrace != null) { msg.AppendFormat("\n{0}", exc.StackTrace); }
			if (exc.InnerException != null)
			{
				msg.AppendFormat("\nINNER EXCEPTION: {0}", exc.InnerException.Message);
				msg.AppendFormat(" ({0})", exc.InnerException.GetType().Name);
				if (exc.InnerException.Source != null || exc.InnerException.TargetSite != null) {
					msg.AppendFormat("\n   In {0}.{1}", (exc.InnerException.Source == null ? "Unknown" : exc.InnerException.Source),
						(exc.InnerException.TargetSite == null ? "Unknown" : exc.InnerException.TargetSite.Name)); }
				if (exc.InnerException.StackTrace != null) { msg.AppendFormat("\n{0}", exc.InnerException.StackTrace); }
			}

			// Ecrit dans le journal:
			if (_writeInLog) {
				string log = String.Format("\n{0}: {1}\n{2}\n----------", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), App.Title, msg);
				try { File.AppendAllText(_logPath, log, App.DefaultEncoding); } catch { ; } }
			// Affiche le message ou déclenche l'événement:
			if (_showError) {
				string log = String.Format("{0}\n{1}", MyResources.ErrorHandler_dialog_ErrorMessage, msg);
				_dlgError.ShowDialog(shortMsg.ToString().Replace("\n", "\r\n\r\n"), msg.ToString().Replace("\n", "\r\n\r\n")); }
			else {
				OnErrorOccurred(shortMsg.ToString(), msg.ToString(), exc); }
		}
		
		/// <summary>
		/// Affiche un message d'erreur ou déclenche l'événement ErrorOccurred, et écrit dans le journal de l'application.
		/// </summary>
		public static void ShowError(Exception exc)
			{ ShowError(exc, null); }

	}


}
