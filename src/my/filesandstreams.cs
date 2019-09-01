using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace My
{


	
	

	// ---------------------------------------------------------------------------
	// TYPES
	// ---------------------------------------------------------------------------


	#region TYPES


	/// <summary>
	/// Enumération pour GetFilesAndDirectories.
	/// </summary>
	public enum GetFileSystemMethod
	{
		OnlyFiles,
		OnlyFolders,
		FilesAndFolders,
		FoldersAndFiles
	}


	/// <summary>
	/// Enumération pour GetFilesAndDirectories.
	/// </summary>
	public enum GetFileSystemSortMethod
	{
		DeepFirst,
		DeepFirstMixed,
		DeepLast,
		DeepLastMixed,
		None
	}
	
	
	#endregion TYPES
	
	




	/// <summary>
	/// Fournit des fonctions pour accélérer l'écriture textuelle ou binaire dans les fichiers, ou entre les fichiers et la mémoire, ainsi que des fonctions pour l'affichage des boîtes de dialogues de fichiers ou de dossiers, avec les options courrantes déjà réglées, etc.
	/// </summary>
	public static class FilesAndStreams
	{



		// ---------------------------------------------------------------------------
		// LECTURES ET ECRITURES
		// ---------------------------------------------------------------------------



		#region LECTURES ET ECRITURES



		/// <summary>
		/// Lit un fichier binaire. Gère les exceptions.
		/// </summary>
		public static byte[] ReadBinary(string fileName)
		{
			FileStream fs = null; BinaryReader br = null;
			try {
				/* // Définit un stream, et un BinReader, puis transfert le fichier dans un tableau Byte, et le retourne.
				if (!System.IO.File.Exists(fileName)) { throw new FileNotFoundException(); }
				fs = new FileStream(fileName, FileMode.Open);
				br = new BinaryReader(fs);
				byte[] binData = br.ReadBytes((int)(new System.IO.FileInfo(fileName).Length));
				return binData; */
				return File.ReadAllBytes(fileName); }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc, MyResources.FilesAndStreams_errorMsg_UnableToReadFile + " " + fileName);
				return null; }
			finally {
				if (br != null) br.Close(); if (fs != null) fs.Close(); if (fs != null) fs.Dispose(); }
		}


		/// <summary>
		/// Ecrit un fichier binaire. Gère les exceptions.
		/// </summary>
		public static bool WriteBinary(string fileName, byte[] binData)
		{
			FileStream fs = null; BinaryWriter bw = null;
			try {
				/* // Définit un stream, et un BinWriter, puis transfert le tableau Byte dans un fichier, et retour true.
				fs = new FileStream(fileName, FileMode.OpenOrCreate);
				bw = new BinaryWriter(fs); bw.Write(binData); bw.Flush();
				return true; */
				File.WriteAllBytes(fileName, binData); return true; }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc, MyResources.FilesAndStreams_errorMsg_UnableToWriteFile + " " + fileName);
				return false; }
			finally {
				if (bw != null) bw.Close(); if (fs != null) fs.Close(); if (fs != null) fs.Dispose(); }
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Lit un fichier binaire, et retourne son contenu dans un MemoryStream. Retourne null si erreur.
		/// </summary>
		public static MemoryStream ReadBinaryToMemory(string filename)
		{
			try { return new MemoryStream(ReadBinary(filename)); }
			catch (Exception exc) { My.ErrorHandler.ShowError(exc); return null; }
		}


		/// <summary>
		/// Ecrit un MemoryStream dans un fichier.
		/// </summary>
		public static bool WriteMemoryToFile(string fileName, MemoryStream ms)
		{
			try { return WriteBinary(fileName, ms.GetBuffer()); }
			catch (Exception exc) { My.ErrorHandler.ShowError(exc); return false; }
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne toutes les lignes d'un fichier texte. Affiche les erreurs au besoin et retourne null si erreur.
		/// </summary>
		public static string[] ReadAllLines(string fileName)
		{
			try {
				/* if (!System.IO.File.Exists(fileName)) { throw new FileNotFoundException();}
				StreamReader sr = new StreamReader(fileName, true);
				string line;
				List<string> allLines = new List<string>();
				while ((line = sr.ReadLine()) != null) { allLines.Add(line); }
				return allLines.ToArray<string>(); */
				return File.ReadAllLines(fileName, My.App.DefaultEncoding); }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc, MyResources.FilesAndStreams_errorMsg_UnableToReadFile + " " + fileName);
				return null; }
			/* finally {
				if (sr != null) sr.Close(); if (sr != null) sr.Dispose(); } */
		}


		/// <summary>
		/// Ecrit toutes les lignes passées dans un tableau dans un fichier. Gère les exceptions.
		/// </summary>
		public static bool WriteAllLines(string fileName, string[] lines)
		{
			try {
				/* StreamWriter sw = new StreamWriter(fileName, false, My.App.AppDefaultEncoding);
				foreach (string i in lines) { sw.WriteLine(i); }
				return true; */
				File.WriteAllLines(fileName, lines, My.App.DefaultEncoding);
				return true; }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc, MyResources.FilesAndStreams_errorMsg_UnableToWriteFile + " " + fileName);
				return false; }
			/* finally {
				if (sw != null) sw.Close(); if (sw != null) sw.Dispose(); } */
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Ajoute une ligne ou plusieurs lignes à la fin d'un fichier, qui est créé s'il n'existe pas. Gère les exceptions.
		/// </summary>
		public static bool AppendTextFile(string fileName, string[] lines)
		{
			StreamWriter sw = null;
			try {
				sw = new StreamWriter(fileName, true, My.App.DefaultEncoding);
				foreach (string i in lines) { sw.WriteLine(i); }
				return true; }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc, MyResources.FilesAndStreams_errorMsg_UnableToWriteFile + " " + fileName);
				return false; }
			finally {
				if (sw != null) sw.Close(); if (sw != null) sw.Dispose(); }
		}
		
		
		/// <summary>
		/// Ajoute une ligne ou plusieurs lignes à la fin d'un fichier, qui est créé s'il n'existe pas. Gère les exceptions.
		/// </summary>
		public static bool AppendTextFile(string fileName, string line)
			{ return AppendTextFile(fileName, new string[] { line }); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Sérialise un objet dans un fichier. L'objet doit évidemment pouvoir être sérialisable. Gère les exceptions.
		/// </summary>
		public static bool SerializeInFile(string fileName, object objectToSerialize)
		{
			if (objectToSerialize == null) { return false; }
			BinaryFormatter bf; FileStream fs = null; 
			try {
				bf = new BinaryFormatter();
				fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
				bf.Serialize(fs, objectToSerialize);
				return true; }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc, MyResources.FilesAndStreams_errorMsg_UnableToWriteFile + " " + fileName);
				return false; }
			finally {
				if (fs != null) { fs.Close(); fs.Dispose(); } }
		}


		/// <summary>
		/// Désérialise un objet depuis un fichier. Retourne null si erreur. Gère les exceptions.
		/// </summary>
		public static bool DeserializeFromFile(string fileName, out object objectToDeserialize)
		{
			BinaryFormatter bf; FileStream fs = null; 
			try {
				bf = new BinaryFormatter();
				fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				objectToDeserialize = bf.Deserialize(fs);
				return true; }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc, MyResources.FilesAndStreams_errorMsg_UnableToReadFile + " " + fileName);
				objectToDeserialize = null;
				return false; }
			finally {
				if (fs != null) { fs.Close(); fs.Dispose(); } }
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne un tableau byte[] correspondant à l'objet sérialisé.
		/// </summary>
		public static byte[] SerializeToBytes(object objectToSerialize)
		{
			if (objectToSerialize == null) { return null; }
			BinaryFormatter bf; MemoryStream ms = null; 
			try {
				bf = new BinaryFormatter();
				ms = new MemoryStream();
				bf.Serialize(ms, objectToSerialize);
				return ms.GetBuffer(); }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc, MyResources.FilesAndStreams_errorMsg_UnableToSerialize);
				return null; }
			finally {
				if (ms != null) { ms.Close(); ms.Dispose(); } }
		}


		/// <summary>
		/// Retourne dans le paramètre de sortie l'objet désérialisé correspondant au paramètre d'entrée byte[]. Retourne true si réussi.
		/// </summary>
		public static bool DeserializeFromBytes(byte[] data, out object objectToDeserialize)
		{
			BinaryFormatter bf; MemoryStream ms = null; 
			try {
				bf = new BinaryFormatter();
				ms = new MemoryStream(data);
				objectToDeserialize = bf.Deserialize(ms);
				return true; }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc, MyResources.FilesAndStreams_errorMsg_UnableToDeserialize);
				objectToDeserialize = null;
				return false; }
			finally {
				if (ms != null) { ms.Close(); ms.Dispose(); } }
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Remplit les valeurs des propriétés et des champs de obj trouvés dans info, et correspondant aux noms passés dans names (séparés par des virgules), ou remplit tous les champs et toutes les propriétés de obj trouvés dans info si names est null. Par défaut, flags contient Instance, Public et NonPublic.
		/// </summary>
		public static void FillPropsAndFieldsFromSerialInfo(SerializationInfo info, string names, object obj, BindingFlags flags)
		{
			// Split du tableau:
			string[] arr = null;
			if (!String.IsNullOrEmpty(names))
				{ arr = names.Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries); }
			// Propriété:
			PropertyInfo prop; FieldInfo field; Type type = obj.GetType();
			foreach (SerializationEntry e in info)
			{
				if ((prop = type.GetProperty(e.Name)) != null
						&& (arr == null || arr.Contains(prop.Name))) { prop.SetValue(obj, e.Value, null); }
				else if ((field = type.GetField(e.Name, flags)) != null
						&& (arr == null || arr.Contains(field.Name))) { field.SetValue(obj, e.Value); }
			}
		}

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static void FillPropsAndFieldsFromSerialInfo(SerializationInfo info, string names, object obj)
			{ FillPropsAndFieldsFromSerialInfo(info, names, obj, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); }


		/// <summary>
		/// Ajoute dans info les valeurs des propriétés de l'objet correspondant aux noms passés par names (séparés par des virgules), ou toutes les propriétés si names est null. Par défaut, flags contient Instance, Public et NonPublic. ne sont enregistré que les propriétés qui peuvent être lues (évidemment), et celle qui peuvent être écrite (sinon, problème lors de la désérialisation).
		/// </summary>
		public static void MakeSerialInfoFromProperties(SerializationInfo info, string names, object obj, BindingFlags flags)
		{
			string[] arr = null;
			if (!String.IsNullOrEmpty(names))
				{ arr = names.Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries); }
			PropertyInfo[] props = obj.GetType().GetProperties(flags);
			foreach (PropertyInfo prop in props)
			{
				if ((arr == null || arr.Contains(prop.Name)) && prop.CanRead && prop.CanWrite)
					{	info.AddValue(prop.Name, prop.GetValue(obj, null)); }
			}
		}
		
		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static void MakeSerialInfoFromProperties(SerializationInfo info, string names, object obj)
		{
			MakeSerialInfoFromProperties(info, names, obj, BindingFlags.Instance | BindingFlags.Public
				| BindingFlags.NonPublic);
		}
		
		
		/// <summary>
		/// Ajoute dans info les valeurs des champs de l'objet correspondant aux noms passés par names (séparés par des virgules), ou tous les champs si names est null. Par défaut, flags contient Instance, Public et NonPublic (il vaut donc mieux indiqués des names si on ne veut pas sérialiser tous les champs !).
		/// </summary>
		public static void MakeSerialInfoFromFields(SerializationInfo info, string names, object obj, BindingFlags flags)
		{
			string[] arr = null;
			if (!String.IsNullOrEmpty(names))
				{ arr = names.Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries); }
			FieldInfo[] fields = obj.GetType().GetFields(flags);
			foreach (FieldInfo field in fields)
				{ if ((arr == null || arr.Contains(field.Name)) && !field.IsInitOnly)
					{	info.AddValue(field.Name, field.GetValue(obj)); } }
		}

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static void MakeSerialInfoFromFields(SerializationInfo info, string names, object obj)
			{ MakeSerialInfoFromFields(info, names, obj, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); }



		#endregion LECTURES ET ECRITURES





		// ---------------------------------------------------------------------------
		// BOITES DE DIALOGUE
		// ---------------------------------------------------------------------------



		#region BOITES DE DIALOGUE



		private static string _saveSavedFileDialogPath;
		private static string _savedFolderDialogPath;
		private static string _savedOpenFileDialogPath;


		/// <summary>
		/// Affiche la boîte de dialogue d'ouverture/sauvegarde de fichier/dossier, et retourne le fichier/dossier sélectionné par l'utilisateur, ou null si l'utilisateur a annulé. Filtre de forme "All|*.*|Images|*.jpg". "|All|*.*" est ajouté automatiquement. Si filter est simplement une extension (commençant par un point), alors le filtre est formé automatiquement (si ".jpg" est passé, le filtre devient automatiquement ".jpg|*.jpg"). Dossier par défaut paramétrable dans les settings (Desktop par défaut).
		/// </summary>
		public static string MyOpenFileDialog(string filter, string fileName, string initDirectory)
		{
			// Obtient boîte avec les propriétés les plus courantes:
			OpenFileDialog openDialog = GetMyOpenFileDialog(filter, fileName, initDirectory);
			// Affiche et retourne la réponse:
			if (openDialog.ShowDialog() != DialogResult.Cancel)
			{
				// Retient le nom du dossier:
				_savedOpenFileDialogPath = new FileInfo(openDialog.FileName).DirectoryName;
				// Retour:
				return openDialog.FileName;
			}
			return null;
		}

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static string MyOpenFileDialog(string filter)
			{ return MyOpenFileDialog(filter, null, null); }

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static string MyOpenFileDialog(string filter, string fileName)
			{ return MyOpenFileDialog(filter, fileName, null); }

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static string MyOpenFileDialog()
			{ return MyOpenFileDialog(null, null, null); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne une boîte de dialogue d'ouverte/sauvegarde de fichier/dossier, avec les propriétés les plus courantes déjà définies, et qu'il est possible de modifier par la suite. Les paramètres sont les mêmes que pour MyOpenFileDialog.
		/// </summary>
		public static OpenFileDialog GetMyOpenFileDialog(string filter, string fileName, string initDirectory)
		{
			// Indique les propriétés les plus courantes, par défaut:
			OpenFileDialog openDialog = new OpenFileDialog();
			openDialog.AddExtension = true;
			openDialog.DereferenceLinks = true;
			if ((filter != null) && (filter.IndexOf('|')) < 0 && (filter.IndexOf('.') == 0)) { filter += "|*" + filter; }
			openDialog.Filter = ((filter != null) ? filter + "|All|*.*" : "All|*.*");
			if (!String.IsNullOrEmpty(fileName)) {
				if (String.IsNullOrEmpty(Path.GetDirectoryName(fileName))) { openDialog.FileName = fileName; }
				else { openDialog.FileName = Path.GetFileName(fileName); initDirectory = Path.GetDirectoryName(fileName); } }
			if ((initDirectory != null)) { openDialog.InitialDirectory = initDirectory; }
			else if (_savedOpenFileDialogPath != null) { openDialog.InitialDirectory = _savedOpenFileDialogPath; }
			else { openDialog.InitialDirectory = MySettings.CurrentDirectory; }
			openDialog.RestoreDirectory = true;
			openDialog.ValidateNames = true;
			openDialog.CheckFileExists = true;
			openDialog.Multiselect = false;
			openDialog.ShowReadOnly = false;
			return openDialog;
		}

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static OpenFileDialog GetMyOpenFileDialog(string filter)
			{ return GetMyOpenFileDialog(filter, null, null); }

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static OpenFileDialog GetMyOpenFileDialog(string filter, string fileName)
			{ return GetMyOpenFileDialog(filter, fileName, null); }

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static OpenFileDialog GetMyOpenFileDialog()
			{ return GetMyOpenFileDialog(null, null, null); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Voir MyOpenFileDialog et GetMyOpenFileDialog.
		/// </summary>
		public static string MySaveFileDialog(string filter, string fileName, string initDirectory)
		{
			// Obtient boîte avec les propriétés les plus courantes:
			SaveFileDialog saveDialog = GetMySaveFileDialog(filter, fileName, initDirectory);
			// Affiche et retourne la réponse:
			if (saveDialog.ShowDialog() != DialogResult.Cancel)
			{
				// Retient le nom du dossier:
				_saveSavedFileDialogPath = new FileInfo(saveDialog.FileName).DirectoryName;
				// Retour:
				return saveDialog.FileName;
			}
			return null;
		}

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static string MySaveFileDialog(string filter)
			{ return MySaveFileDialog(filter, null, null); }

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static string MySaveFileDialog(string filter, string fileName)
			{ return MySaveFileDialog(filter, fileName, null); }

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static string MySaveFileDialog()
			{ return MySaveFileDialog(null, null, null); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Voir MyOpenFileDialog et GetMyOpenFileDialog.
		/// </summary>
		public static SaveFileDialog GetMySaveFileDialog(string filter, string fileName, string initDirectory)
		{
			// Indique les propriétés les plus courantes, par défaut:
			SaveFileDialog saveDialog = new SaveFileDialog();
			saveDialog.AddExtension = true;
			saveDialog.DereferenceLinks = true;
			if ((filter != null) && (filter.IndexOf('|')) < 0 && (filter.IndexOf('.') == 0)) { filter += "|*" + filter; }
			saveDialog.Filter = ((filter != null) ? filter + "|All|*.*" : "All|*.*");
			if (!String.IsNullOrEmpty(fileName)) {
				if (String.IsNullOrEmpty(Path.GetDirectoryName(fileName))) { saveDialog.FileName = fileName; }
				else { saveDialog.FileName = Path.GetFileName(fileName); initDirectory = Path.GetDirectoryName(fileName); } }
			if ((initDirectory != null)) { saveDialog.InitialDirectory = initDirectory; }
			else if (_saveSavedFileDialogPath != null) { saveDialog.InitialDirectory = _saveSavedFileDialogPath; }
			else { saveDialog.InitialDirectory = MySettings.CurrentDirectory; }
			saveDialog.RestoreDirectory = true;
			saveDialog.ValidateNames = true;
			saveDialog.OverwritePrompt = true;
			return saveDialog;
		}

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static SaveFileDialog GetMySaveFileDialog(string filter)
			{ return GetMySaveFileDialog(filter, null, null); }

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static SaveFileDialog GetMySaveFileDialog(string filter, string fileName)
			{ return GetMySaveFileDialog(filter, fileName, null); }

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static SaveFileDialog GetMySaveFileDialog()
			{ return GetMySaveFileDialog(null, null, null); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Voir MyOpenFileDialog et GetMyOpenFileDialog.
		/// </summary>
		public static string MyFolderDialog(string description, string initDirectory)
		{
			// Indique les propriétés les plus courantes, par défaut:
			FolderBrowserDialog folderDialog = new FolderBrowserDialog();
			if (description != null) { folderDialog.Description = description; }
			// folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
			if (!String.IsNullOrEmpty(initDirectory)) { folderDialog.SelectedPath = initDirectory; }
			else if (_savedFolderDialogPath != null) { folderDialog.SelectedPath = _savedFolderDialogPath; }
			else { folderDialog.SelectedPath = MySettings.CurrentDirectory; }
			folderDialog.ShowNewFolderButton = true;
			if (folderDialog.ShowDialog() != DialogResult.Cancel)
			{
				// Retient le nom du dossier:
				_savedFolderDialogPath = folderDialog.SelectedPath;
				// Retour:
				return folderDialog.SelectedPath;
			}
			return null;
		}

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static string MyFolderDialog(string description)
			{ return MyFolderDialog(description, null); }

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public static string MyFolderDialog()
			{ return MyFolderDialog(null, null); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Sauve un Bitmap en affiche une boîte. Le bitmap peut être converti par l'utilisateur en plusieurs formats, mais defFormat (ou le format d'origine si defFormat est null) est proposé par défaut. Retourne le nom du fichier choisi par l'utilisateur (ou null si annulation ou erreur). Il est possible de passer un tableau d'images : toutes les images sont alors enregistrés. Le nom du fichier est le nom choisi par l'utilisateur, mais sont rajouté des numéros juste avant l'extension sous la forme 00.
		/// </summary>
		public static string SaveBitmap(Bitmap[] pictures, string defaultFileName, ImageFormat defaultFormat)
		{
		
			// Sort si null ou si pas d'image:
			if (pictures == null || pictures.Length == 0) { return null; }
			
			// Récupère la première image (référent), et cherche les types de fichiers:
			Bitmap pict = pictures[0];
			string[] extensions;
			ImageFormat[] formats = My.Functions.GetImageFormats(false, out extensions);

			// Filtres pour le dialogue, et cherche le format par défaut dans la liste des filtres:
			if (defaultFormat == null) { defaultFormat = pict.RawFormat; }
			string filters = String.Empty;
			int index = 0, l = extensions.Length;
			for (int i=0; i<l; i++) {
				filters += extensions[i] + "|*." + extensions[i] + "|";
				if (defaultFormat == formats[i]) { index = i; } }
			filters = filters.Substring(0, filters.Length - 1);
			
			// Affiche la boîte de dialogue:
			if (String.IsNullOrEmpty(defaultFileName)) { defaultFileName = "picture"; }
			else { defaultFileName = Path.Combine(Path.GetDirectoryName(defaultFileName), Path.GetFileNameWithoutExtension(defaultFileName)); }
			defaultFileName += "." + extensions[index];
			SaveFileDialog dialog = GetMySaveFileDialog(filters, defaultFileName);
			dialog.FilterIndex = index + 1;
			if (dialog.ShowDialog() == DialogResult.Cancel) { return null; }
			string filename = dialog.FileName;
			
			// Cherche le format d'image choisi par l'utilisateur:
			ImageFormat saveFormat;
			if (!My.GeneralParser.ImageFormatParser(Path.GetExtension(filename), out saveFormat))
				{ My.ErrorHandler.ShowError(new FormatException()); return null; }

			// Enregistre une seule image:
			try
			{
				if (pictures.Length == 1) {
					pict.Save(filename, saveFormat); }
				else {
					string dir = Path.GetDirectoryName(filename), name = Path.GetFileNameWithoutExtension(filename), ext = Path.GetExtension(filename);
					for (int i=0; i<pictures.Length; i++) {
						pictures[i].Save(Path.Combine(dir, String.Format("{0} {1:00}{2}", name, i, ext)), saveFormat); } }
			}
			catch (Exception exc)
			{
				My.ErrorHandler.ShowError(exc, "Process stopped. Enable to continue saving.");
			}
			
			return filename;
			
		}
		
		/// <summary>
		/// Affiche un dialogue permettant de changer le type de bitmap et de le sauver. Voir surcharge.
		/// </summary>
		public static string SaveBitmap(Bitmap[] pictures, string defaultFileName)
		{ return SaveBitmap(pictures, defaultFileName, null); }
		
		/// <summary>
		/// Affiche un dialogue permettant de changer le type de bitmap et de le sauver. Voir surcharge.
		/// </summary>
		public static string SaveBitmap(Bitmap picture)
			{ return SaveBitmap(new Bitmap[]{picture}, null); }
		
		/// <summary>
		/// Affiche un dialogue permettant de changer le type de bitmap et de le sauver. Voir surcharge.
		/// </summary>
		public static string SaveBitmap(Bitmap picture, string defaultFileName, ImageFormat defaultFormat)
			{ return SaveBitmap(new Bitmap[]{picture}, defaultFileName, defaultFormat); }
		
		/// <summary>
		/// Affiche un dialogue permettant de changer le type de bitmap et de le sauver. Voir surcharge.
		/// </summary>
		public static string SaveBitmap(Bitmap picture, string defaultFileName)
			{ return SaveBitmap(new Bitmap[]{picture}, defaultFileName); }
		
		/// <summary>
		/// Affiche un dialogue permettant de changer le type de bitmap et de le sauver. Voir surcharge.
		/// </summary>
		public static string SaveBitmap(Bitmap[] pictures)
			{ return SaveBitmap(pictures, null); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Sauve un bitmap dans un type donné. Si le type est différent, l'extension en remplacé par celle du nouveau type. Retourne true si pas d'exception, false sinon. Affiche le message d'exception si showError.
		/// </summary>
		public static bool SaveBitmap(Image bmp, ImageFormat format, string filename, bool showError)
		{
			// Détermine l'extension à utiliser:
			string ext;
			if (format.Guid == ImageFormat.Jpeg.Guid) { ext = ".jpg"; }
			else if (format.Guid == ImageFormat.Icon.Guid) { ext = ".ico"; }
			else { ext = "." + format.ToString().ToLower(); }
			// Remplace l'extension:
			string[] types = new string[]{"Bmp","Emf","Exif","Gif","Ico","Jpeg","Jpg","Png","Tiff","Wmf"};
			foreach (string s in types) {
				if (filename.EndsWith("." + s, StringComparison.CurrentCultureIgnoreCase))
					{ filename = filename.Substring(0, filename.Length - s.Length - 1) + ext; break; } }
			filename = filename.ToLower();
			// Sauve:
			try { bmp.Save(filename, format); return true; }
			catch (Exception exc) { if (showError) { My.ErrorHandler.ShowError(exc); } return false; }
		}



		#endregion BOITES DE DIALOGUE




		// ---------------------------------------------------------------------------
		// FONCTIONS DIVERSES
		// ---------------------------------------------------------------------------



		#region FONCTIONS DIVERSES



		/// <summary>
		/// Compare les clés MD5 de deux fichiers. Si l'un des deux fichiers n'existe pas, retourne false. En cas d'erreur (e.g. impossibilité de lire un fichier), retourne false. Gère les exceptions.
		/// </summary>
		public static bool CompareMD5(FileInfo f1, FileInfo f2)
		{
			try {
				// Sort si fichier n'existe pas:
				if ((f1.Exists == false) || (f2.Exists == false)) { return false; }
				// Clé pour fichier 1:
				FileStream fs = new FileStream(f1.FullName, FileMode.Open, FileAccess.Read);
				System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
				byte[] key = md5.ComputeHash(fs);
				// Clé pour fichier 2:
				fs.Close(); fs.Dispose();
				fs = new FileStream(f2.FullName, FileMode.Open, FileAccess.Read);
				byte[] key2 = md5.ComputeHash(fs);
				// Libération:
				fs.Close(); fs.Dispose(); md5.Clear();
				// Compare:
				return My.ArrayFunctions.ArrayEquals(key, key2); }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc);
				return false; }
		}


		/// <summary>
		/// Retourne la clé MD5 d'un fichier, ou null si n'existe pas. Gère les exceptions et retourne null en cas d'erreur.
		/// </summary>
		public static string GetMD5(FileInfo file)
		{
			try {
				// Sort si fichier n'existe pas:
				if (file.Exists == false) { return null; }
				// Clé pour fichier:
				FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
				System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
				byte[] key = md5.ComputeHash(fs);
				fs.Close(); fs.Dispose(); md5.Clear();
				// Transforme en texte:
				StringBuilder sb = new StringBuilder();
				for (int i=0; i<key.Length; i++) { sb.Append(key[i].ToString("x2")); }
				return sb.ToString(); }
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc);
				return null; }
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne un tableau contenant tous les fichiers et/ou dossiers, et tous les petits-enfants, d'un dossier. L'argument method définit si seulement les fichiers doivent être inclus, ou les dossier, ou les deux (et dans ce cas qui vient d'abord). Le tri peut se faire en mettant d'abord les éléments les plus profonds dans la hiérarchie d'abord avec DeepFirst (dans ce cas, fichiers et dossiers restent séparés comme indiqué dans l'argument method) et DeepFirstMixed (fichiers et dossiers sont mélangés), ou bien, au contraire, en mettant les éléments les moins profonds au début avec DeepLast et DeepLastMixed. Le tri s'effectue en fait sur la base d'un tri alphabétique sans respect de la casse. Retourne null en cas d'erreur.
		/// </summary>
		public static FileSystemInfo[] GetFilesAndDirectories(DirectoryInfo folder, GetFileSystemMethod method, GetFileSystemSortMethod sorting)
		{
			try
			{
				// Variables:
				FileSystemInfo[] files = new FileSystemInfo[0];
				FileSystemInfo[] folders = new FileSystemInfo[0];
				FileSystemInfo[] mixed = new FileSystemInfo[0];
				// Cherche les dossier et les fichiers:
				if (method != GetFileSystemMethod.OnlyFolders) { files =  folder.GetFiles("*", SearchOption.AllDirectories); }
				if (method != GetFileSystemMethod.OnlyFiles) { folders = folder.GetDirectories("*", SearchOption.AllDirectories); }
				// Tri avant concaténation:
				if (sorting == GetFileSystemSortMethod.DeepFirst) {
					Array.Sort(files, FileSystemInfoComparerDeepFirst);
					Array.Sort(folders, FileSystemInfoComparerDeepFirst); }
				else if (sorting == GetFileSystemSortMethod.DeepLast) {
					Array.Sort(files, FileSystemInfoComparerDeepLast);
					Array.Sort(folders, FileSystemInfoComparerDeepLast); }
				// Concaténation:
				switch (method) {
					case GetFileSystemMethod.FilesAndFolders: mixed = files.Concat(folders).ToArray(); break;
					case GetFileSystemMethod.FoldersAndFiles: mixed = folders.Concat(files).ToArray(); break;
					case GetFileSystemMethod.OnlyFiles: mixed = files; break;
					case GetFileSystemMethod.OnlyFolders: mixed = folders; break; }
				// Tri après concaténation:
				if (sorting == GetFileSystemSortMethod.DeepFirstMixed) { Array.Sort(mixed, FileSystemInfoComparerDeepFirst); }
				else if (sorting == GetFileSystemSortMethod.DeepLastMixed) { Array.Sort(mixed, FileSystemInfoComparerDeepLast); }
				// Retour:
				return mixed;
			}
			catch (Exception exc) {
				My.ErrorHandler.ShowError(exc);
				return null; }
		}

		/// <summary>
		/// Méthode de tri pour GetFilesAndDirectories.
		/// </summary>
		private static int FileSystemInfoComparerDeepFirst(FileSystemInfo f1, FileSystemInfo f2)
			{ return f1.FullName.ToLower().CompareTo(f2.FullName.ToLower()) * -1; }

		/// <summary>
		/// Méthode de tri pour GetFilesAndDirectories.
		/// </summary>
		private static int FileSystemInfoComparerDeepLast(FileSystemInfo f1, FileSystemInfo f2)
			{ return f1.FullName.ToLower().CompareTo(f2.FullName.ToLower()); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne un DirectoryInfo si path est un dossier, un FileInfo si path est un fichier, et null si path n'existe pas.
		/// </summary>
		public static FileSystemInfo IsFileOrFolder(string path)
		{
			if (Directory.Exists(path)) { return new DirectoryInfo(path); }
			if (File.Exists(path)) { return new FileInfo(path); }
			return null;
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Supprime un fichier ou un dossier, mais pour éviter des erreurs au cas où des fichiers seraient en lecture seule, supprime d'abord tous les contenus du dossier, fichier par fichier et dossier par dossier, en veillant à modifier les attributs.
		/// </summary>
		public static bool DeleteFileOrFolder(FileSystemInfo f)
		{
			// Sort si n'existe pas:
			if ((File.Exists(f.FullName) == false) && Directory.Exists(f.FullName) == false) { return false; }
			try
			{
				// Si dossier, supprime d'abord tous les contenus:
				if (f is DirectoryInfo)
				{
					FileSystemInfo[] files = GetFilesAndDirectories((DirectoryInfo)f,
								GetFileSystemMethod.FilesAndFolders, GetFileSystemSortMethod.DeepFirstMixed);
					foreach (FileSystemInfo i in files) { i.Attributes = FileAttributes.Normal; i.Delete(); }
				}
				// Si dossier ou fichier, supprime l'item passé:
				f.Attributes = FileAttributes.Normal;
				f.Delete();
				return true;
			}
			catch (Exception exc) { My.ErrorHandler.ShowError(exc); return false; }
		}



		#endregion FONCTIONS DIVERSES


	}



}
