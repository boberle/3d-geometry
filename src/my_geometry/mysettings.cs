using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace My.Geometry
{



	/// <summary>
	/// Paramètres pour cet assemblage.
	/// </summary>
	internal static class MySettings
	{


		/// <summary>
		/// Chemin d'accès de l'assemblage dynamique. Si vide, l'assemblage est réécrit.
		/// </summary>
		public static string DynamicAssemblyPath
		{
			get {
				string result = My.MySettingsWriter.GetString(false, "DynamicAssemblyPath", String.Empty);
				if (String.IsNullOrEmpty(result) || !File.Exists(result)) { return String.Empty; }
				else { return result; } }
			set {
				My.MySettingsWriter.SetString(false, "DynamicAssemblyPath", value); }
		}

		/// <summary>
		/// Version de SpaceGeometry lors de la dernière création de l'assemblage dynamique. Si la version ne correspond pas, l'assemblage est réécrit.
		/// </summary>
		public static string DynamicAssemblyGeometryVersion
		{
			get {
				return My.MySettingsWriter.GetString(false, "DynamicAssemblyGeometryVersion", String.Empty); }
			set {
				My.MySettingsWriter.SetString(false, "DynamicAssemblyGeometryVersion", value); }
		}

		/// <summary>
		/// DraftScale par défaut.
		/// </summary>
		public static float DefaultDraftScale
		{
			get {
				return (float)My.MySettingsWriter.GetDouble(true, "DefaultDraftScale", 0.7); }
			set {
				My.MySettingsWriter.SetDouble(true, "DefaultDraftScale", value); }
		}

		/// <summary>
		/// Police par défaut des objets.
		/// </summary>
		public static Font DefaultObjectFont {
			get {
				return My.MySettingsWriter.GetFont(true, "DefaultObjectFont", new Font("Arial", 8)); } }

		/// <summary>
		/// Couleur par défaut des objets.
		/// </summary>
		public static Color DefaultObjectColor {
			get {
				return My.MySettingsWriter.GetColor(true, "DefaultObjectColor", Color.Black); } }

		/// <summary>
		/// Couleur par défaut des objets.
		/// </summary>
		public static Color DefaultObjectFillColor {
			get {
				return My.MySettingsWriter.GetColor(true, "DefaultObjectFillColor", Color.FromArgb(125, Color.LightGray)); } }

		/// <summary>
		/// Couleur par défaut des objets.
		/// </summary>
		public static Color DefaultObjectBackColor {
			get {
				return My.MySettingsWriter.GetColor(true, "DefaultObjectBackColor", Color.FromArgb(125, Color.LightGray)); } }

		/// <summary>
		/// Couleur par défaut des objets.
		/// </summary>
		public static Color DefaultObjectEdgeColor {
			get {
				return My.MySettingsWriter.GetColor(true, "DefaultObjectEdgeColor", Color.Black); } }

		/// <summary>
		/// Couleur par défaut des objets sélectionnés.
		/// </summary>
		public static Color SelectedObjectColor {
			get {
				return My.MySettingsWriter.GetColor(true, "SelectedObjectColor", Color.DarkGoldenrod); } }

		/// <summary>
		/// Police par défaut des listes.
		/// </summary>
		public static Font DefaultListFont {
			get {
				return My.MySettingsWriter.GetFont(true, "DefaultListFont", new Font("Courier New", 10)); } }

		/// <summary>
		/// Zoom par défaut.
		/// </summary>
		public static int DefaultZoom {
			get {
				return My.MySettingsWriter.GetInt32(true, "DefaultZoom", 50); } }

		
	}



}
