using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;


namespace My.ExdControls
{



	/// <summary>
	/// Paramètres pour cette application.
	/// </summary>
	internal static class MySettings
	{


		/// <summary>
		/// Police par défaut pour le TextEditor.
		/// </summary>
		public static Font TextEditorDefaultFont
		{
			get {
				Font def = new Font("Times New Roman", 12F);
				return My.MySettingsWriter.GetFont(true, "TextEditorDefaultFont", def); }
		}

		/// <summary>
		/// Police par défaut pour la console.
		/// </summary>
		public static Font ConsoleDefaultFont
		{
			get {
				Font def = new Font("Courier New", 11F);
				return My.MySettingsWriter.GetFont(true, "ConsoleDefaultFont", def); }
			set {
				My.MySettingsWriter.SetFont(true, "ConsoleDefaultFont", value); }
		}

		/// <summary>
		/// Couleur par défaut de la console.
		/// </summary>
		public static Color ConsoleDefaultColor
		{
			get {
				Color def = Color.Black;
				return My.MySettingsWriter.GetColor(true, "ConsoleDefaultColor", def); }
			set {
				My.MySettingsWriter.SetColor(true, "ConsoleDefaultColor", value); }
		}

		/// <summary>
		/// Couleur de fond par défaut de la console.
		/// </summary>
		public static Color ConsoleDefaultBackColor
		{
			get {
				Color def = Color.White;
				return My.MySettingsWriter.GetColor(true, "ConsoleDefaultBackColor", def); }
			set {
				My.MySettingsWriter.SetColor(true, "ConsoleDefaultBackColor", value); }
		}

		/// <summary>
		/// Couleur de fond "utilisateur" de la fenêtre de zoom.
		/// </summary>
		public static Color DialogBoxPictureZoomDefaultBackColor
		{
			get {
				return My.MySettingsWriter.GetColor(true, "DialogBoxPictureZoomDefaultBackColor", Color.LemonChiffon); }
		}

		/// <summary>
		/// Taille des vignettes du PictureAlbum. Compris entre 16 et 256.
		/// </summary>
		public static int PictureAlbumDefaultThumbnailSize
		{
			get {
				int result = My.MySettingsWriter.GetInt32(true, "PictureAlbumDefaultThumbnailSize", 256);
				if (result < 16) { return 16; }
				else if (result > 256) { return 256; }
				else { return result; } }
		}

		/// <summary>
		/// Couleur de fond du PictureAlbum.
		/// </summary>
		public static Color PictureAlbumDefaultBackColor
		{
			get {
				return My.MySettingsWriter.GetColor(true, "PictureAlbumDefaultBackColor", Color.FromArgb(69, 69, 69)); }
		}

		/// <summary>
		/// Couleur d'avant-plan du PictureAlbum.
		/// </summary>
		public static Color PictureAlbumDefaultForeColor
		{
			get {
				return My.MySettingsWriter.GetColor(true, "PictureAlbumDefaultForeColor", Color.LemonChiffon); }
		}

		/// <summary>
		/// Couleur de la marque d'insertion du PictureAlbum.
		/// </summary>
		public static Color PictureAlbumInsertionMarkColor
		{
			get {
				return My.MySettingsWriter.GetColor(true, "PictureAlbumInsertionMarkColor", Color.DarkBlue); }
		}

		/// <summary>
		/// Indique si par défaut les noms sont affichés dans le PictureAlbum.
		/// </summary>
		public static bool PictureAlbumDefaultShowNames
		{
			get {
				return My.MySettingsWriter.GetBoolean(true, "PictureAlbumDefaultShowNames", true); }
		}

		/// <summary>
		/// Indique s'il faut ignorer au lancement du PictureAlbum la taille d'image enregistrée dans l'album, et préférer celle par défaut.
		/// </summary>
		public static bool PictureAlbumIgnoreSavedPictureSize
		{
			get {
				return My.MySettingsWriter.GetBoolean(true, "PictureAlbumIgnoreSavedPictureSize", false); }
		}

		/// <summary>
		/// Couleur de sélection des noeuds des arbres.
		/// </summary>
		public static Color TreeSelectionColor
		{
			get {
				return My.MySettingsWriter.GetColor(true, "TreeSelectionColor", Color.SteelBlue); }
		}

		/// <summary>
		/// Police par défaut des listes pour la console.
		/// </summary>
		public static Font ConsoleListFont {
			get {
				return My.MySettingsWriter.GetFont(true, "ConsoleListFont", new Font("Courier New", 10)); } }


	}
	
	
	
}
