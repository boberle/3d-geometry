using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace My
{





		// ---------------------------------------------------------------------------
		// ENUM
		// ---------------------------------------------------------------------------




		#region ENUM




		/// <summary>
		/// Forme d'un point.
		/// </summary>
		public enum PointShape
		{
			Round,
			Square
		}


		/// <summary>
		/// Type de Brush pour les objets de l'espace.
		/// </summary>
		public enum BrushStyle
		{
			Solid,
			Hatch
		}
		
		/// <summary>
		/// Flags pour les SpObjects
		/// </summary>
		public enum ObjectFlags
		{
			None = 0,
			Explicit = 1,
			Virtual = 2,
			System = 4,
			Extracted = 8,
			Formula = 16,
			Created = 32,
			Altering = 64,
			Deleting = 128,
			Deleted = 256,
			Selected = 512,
			Undefined = 1024,
			DontRecalc = 2048
		}


		#endregion ENUM






	// ---------------------------------------------------------------------------
	// OBJECTS DE BASE: SPOBJECT, SPPENOBJECT ET SPBRUSHOBJECT
	// ---------------------------------------------------------------------------




	#region OBJECTS DE BASE: SPOBJECT, SPPENOBJECT ET SPBRUSHOBJECT
	
	
	
	/// <summary>
	/// Sous-classes, événements, délégués, propriétés et méthodes statiques.
	/// </summary>
	public abstract partial class SpObject
	{
	
		// DECLARATIONS:
		private static int _virtsysObjCounter;
		private static int _defNamesCounter;
		private static Regex _namesReg;
		private static byte __DecimalPlaces;
		protected static string _decPlacesFormat;
		private static My.Buffer<string> _delObjInfos;
		private static Func<string,string> _getFreeName;
		protected static Color _defColor, _defFillColor, _defBackColor, _defEdgeColor, _selColor;
		protected static Font _defFont;
		protected static bool _showCalcRes;
		
		
		// ---------------------------------------------------------------------------
		// DELEGUES:
		
		/// <summary>
		/// Délégué destiné à être renseigné par une collection pour offrir un nom libre.
		/// </summary>
		internal static Func<string,string> GetFreeName {
			get { return _getFreeName; }
			set { _getFreeName = value; } }


		// ---------------------------------------------------------------------------
		// EVENEMENTS:
		
		/// <summary>
		/// Classe de paramètres d'événement pour l'événement ObjectAltered. BigMasterObj est le premier objet a être modifié, de l'extérieur, dans la chaîne de tous les esclaves qui seront ensuite modifié. En d'autres termes, c'est le maître par qui le changement arrive...
		/// </summary>
		protected class ObjectAlteredEventArgs : EventArgs
		{
			public SpObject BigMasterObj { get; set; }
			public ObjectAlteredEventArgs(SpObject bigMasterObj) { BigMasterObj = bigMasterObj; }
		}
		
		/// <summary>
		/// Classe de paramètres d'événement pour l'événement IsUndefinedChanged.
		/// </summary>
		protected class IsUndefinedChangedEventArgs : EventArgs
		{
			public bool Value { get; set; }
			public IsUndefinedChangedEventArgs(bool value) { Value = value; }
		}
				
		/// <summary>
		/// Classe de paramètres d'événement pour l'événement NameChanged.
		/// </summary>
		internal class NameChangedEventArgs : EventArgs
		{
			public string OldName { get; set; }
			public string NewName { get; set; }
			public NameChangedEventArgs(string oldName, string newName)
				{ OldName = oldName; NewName = newName; }
		}

		/// <summary>
		/// Classe de paramètres d'événement pour l'événement ObjectExtracted.
		/// </summary>
		internal class ObjectExtractedEventArgs : EventArgs
		{
			public SpObject ObjectExtracted { get; set; }
			public ObjectExtractedEventArgs(SpObject obj) { ObjectExtracted = obj; }
		}

		/// <summary>
		/// Classe de paramètres d'événement pour l'événement ObjectChanged.
		/// </summary>
		internal class ObjectChangedEventArgs : EventArgs
		{
			public SpObject ObjectChanged { get; set; }
			public ObjectChangedEventArgs(SpObject obj) { ObjectChanged = obj; }
		}

		/// <summary>
		/// Evénement de suppression de l'objet.
		/// </summary>
		protected event EventHandler ObjectDeleted;
		
		/// <summary>
		/// Evénement de changement de couleur (pour la mise à jour des Brush et Pen);
		/// </summary>
		protected event EventHandler ColorChanged;
		
		/// <summary>
		/// Evénement de changement de l'état de sélection.
		/// </summary>
		protected event EventHandler SelectionChanged;
		
		/// <summary>
		/// Délégué d'événement.
		/// </summary>
		protected delegate void ObjectAlteredEventHandler(object sender, ObjectAlteredEventArgs e);
		
		/// <summary>
		/// Evénement se déclenchant lorsqu'une donnée numérique de l'objet change. Cet événement est protégé pour tous les SpObjetcs, et il doit servir à indiquer aux escalves que leurs maîtres ont changé. 
		/// </summary>
		protected event ObjectAlteredEventHandler ObjectAltered;
		
		/// <summary>
		/// Evénement se déclenchant lorsqu'une donnée numérique de l'objet change, mais avant ObjectAltered. Il permet de préparer la procédure avant que l'événement ObjectAltered se déclenche.
		/// </summary>
		protected event ObjectAlteredEventHandler PreObjectAltered;
		
		/// <summary>
		/// Délégué d'événement.
		/// </summary>
		protected delegate void IsUndefinedChangedEventHandler(object sender, IsUndefinedChangedEventArgs e);
		
		/// <summary>
		/// Evénement se déclenchant lorsque la valeur de la propriété IsUndefined change.
		/// </summary>
		protected event IsUndefinedChangedEventHandler IsUndefinedChanged;
		
		/// <summary>
		/// Evénement qui se déclenche lorsqu'un objet maître change lui-même d'objets maîtres.
		/// </summary>
		protected event EventHandler MastersListChanged;
		
		
		// ---------------------------------------------------------------------------
		// METHODES ET PROPRIETES STATIQUES:

		/// <summary>
		/// Evénement se déclenchant lorsqu'une donnée numérique de l'objet change, pour demander un recalcul des données d'affichage par le DrawingArea qui reçoit (peut-être) l'événement.
		/// </summary>
		internal event RequestDrawingCalcEventHandler RequestDrawingCalc;
		
		/// <summary>
		/// Se déclenche lorsqu'un ou des éléments ont changé. Cet événement ne se déclenche qu'une fois, lorsque le BigMaster de Num3DDateChangedEventArgs a fini de modifier tous les esclaves.
		/// </summary>
		internal event EventHandler RequestDrawing;

		/// <summary>
		/// Délégué d'événement.
		/// </summary>
		internal delegate void NameChangedEventHandler(object sender, NameChangedEventArgs e);
		
		/// <summary>
		/// Evénement se déclenchant lorsque la valeur de la propriété Name change (uniquement pour les objets explicites.
		/// </summary>
		internal static event NameChangedEventHandler NameChanged;

		/// <summary>
		/// Délégué d'événement.
		/// </summary>
		internal delegate void ObjectExtractedEventHandler(object sender, ObjectExtractedEventArgs e);
		
		/// <summary>
		/// Evénement se déclenchant lorsque qu'un objet système est extrait (et seulement s'il est extrait, pas s'il y a seulement une tentative d'extraction).
		/// </summary>
		internal static event ObjectExtractedEventHandler ObjectExtracted;

		/// <summary>
		/// Délégué d'événement.
		/// </summary>
		internal delegate void ObjectChangedEventHandler(object sender, ObjectChangedEventArgs e);
		
		/// <summary>
		/// Evénement se déclenchant lorsque qu'un objet exécute une méthode Alter (véritable, cad pas une méthode Alter avec suffixe).
		/// </summary>
		internal static event ObjectChangedEventHandler ObjectChanged;

		/// <summary>
		/// Obtient ou définit le nombre de décimal à afficher dans les nombres. Statique.
		/// </summary>
		public static byte DecimalPlaces {
			get { return __DecimalPlaces; }
			set { __DecimalPlaces = value; _decPlacesFormat = "0." + "".PadLeft(__DecimalPlaces, '#'); } }
		
		/// <summary>
		/// Obtient ou définit s'il faut affiche quand un objet a été modifié ou recalculé. Statique.
		/// </summary>
		public static bool ShowCalculationResult {
			get { return _showCalcRes; }
			set { _showCalcRes = value; } }
		
		/// <summary>
		/// Obtient l'expression régulière utilisé pour valider les noms.
		/// </summary>
		public static Regex ValidNameRegex { get { return _namesReg; } }

		/// <summary>
		/// Constructeur statique.
		/// </summary>
		static SpObject()
		{
			_virtsysObjCounter = 0; _defNamesCounter = 0;
			_delObjInfos = new Buffer<string>(15);
			_showCalcRes = true;
			//_namesReg = new Regex(@"([\w]*|[']*)");
			_namesReg = new Regex(@"([a-zA-Zα-ωΑ-Ω0-9_' \[\]\(\)]*)");
			DecimalPlaces = 4;
			_selColor = My.Geometry.MySettings.SelectedObjectColor;
			_defColor = My.Geometry.MySettings.DefaultObjectColor;
			_defFillColor = My.Geometry.MySettings.DefaultObjectFillColor;
			_defBackColor = My.Geometry.MySettings.DefaultObjectBackColor;
			_defEdgeColor = My.Geometry.MySettings.DefaultObjectEdgeColor;
			_defFont = My.Geometry.MySettings.DefaultObjectFont;
		}

		/// <summary>
		/// Retourne la liste de tous les maîtres de l'objet passé en argument, et de tous les maîtres des maîtres, etc.
		/// </summary>
		public static SpObject[] GetAllMasters(SpObject obj)
		{
			SpObject[] result = (SpObject[])obj.MasterObjects.Clone();
			foreach (SpObject o in obj.MasterObjects) { result = result.Concat(GetAllMasters(o)).ToArray();	}
			return result.Distinct().ToArray();
		}
		
		/// <summary>
		/// Retourne la liste de tous les owneds de l'objet passé en argument, et de tous les owneds des owneds, etc.
		/// </summary>
		public static SpObject[] GetAllOwneds(SpObject obj)
		{
			SpObject[] result = (SpObject[])obj.OwnedObjects.Clone();
			foreach (SpObject o in obj.OwnedObjects) { result = result.Concat(GetAllOwneds(o)).ToArray();	}
			return result;
		}
		
		/// <summary>
		/// Retourne un nom valide, débarassé des signes non alphanumérique (caractères latins, grecs, etc.) sauf underscore ou apostrophe, et non vide.
		/// </summary>
		private static string GetValidName(string s, SpObject obj)
		{
			MatchCollection mc = _namesReg.Matches(s);
			string result = String.Empty;
			foreach (Match m in mc) { result += m.Value; }
			if (result.EndsWith("_")) { result = result.Substring(0, result.Length - 1); }
			if (String.IsNullOrEmpty(result)) { result = String.Format("{0}_{1}",
				SpObjectCtorInfosCollection.GetInstance().GetShortNameOf(obj.GetType()), (_defNamesCounter++).ToString()); }
			return result;
		}
		
		/// <summary>
		/// Remplace dans toutes les formules un nom par un autre. Attention! Cette méthode ne doit être utilisée que pour un renommage spécial de l'ensemble des fomules, et ne doit pas être utilisé pour changer le nom d'un objet (au quel cas, il faut modifier la propriété Name de l'objet...). Ne redessine pas le DrawingArea.
		/// </summary>
		internal static void ChangeName(string oldName, string newName)
		{
			if (NameChanged != null) { NameChanged(typeof(SpObject), new NameChangedEventArgs(oldName, newName)); }
		}

		/// <summary>
		/// Le destructeur ne peut pas envoyer de message à afficher sur un window, par exemple, puisque le GC opère sur un autre thread. Cette méthode enregistre donc les messages des objets supprimés envoyés par les destructeurs. Pour l'utiliser, il suffit d'appeler cette méthode après la suppression d'un ou d'une série d'objets pour avoir les informations concernant les objets supprimés.
		/// </summary>
		public static string[] DeletedObjectsInfos()
			{ return _delObjInfos.Reset(); }
		
		/// <summary>
		/// Enregistre dans le buffer les messages en provenance des destructeurs.
		/// </summary>
		private static void DeletedObjectsInfos(string text)
			{ _delObjInfos.SetValue(text); }
		
		/// <summary>
		/// Formate un nombre pour qu'il s'affiche avec le nombre de décimales requis.
		/// </summary>
		public static string FormatText(double n)
		{
			return n.ToString(_decPlacesFormat);
		}
		
		/// <summary>
		/// Formate une chaîne en utilisant String.Format(s, args), mais en ayant converti auparavant les valeurs de args pour les afficher avec le nombre de décimales requis, ou pour afficher le nom de l'objet s'il s'agit d'un SpObject).
		/// </summary>
		public static string FormatText(string s, params object[] args)
		{
			int l = args.Length;
			for (int i=0; i<l; i++)
			{
				if (args[i].GetType() == typeof(Double)) { args[i] = ((double)args[i]).ToString(_decPlacesFormat); }
				else if (args[i].GetType() == typeof(Single)) { args[i] = ((Single)args[i]).ToString(_decPlacesFormat); }
				else if (args[i].GetType() == typeof(Decimal)) { args[i] = ((Decimal)args[i]).ToString(_decPlacesFormat); }
				else if (args[i].GetType() == typeof(DoubleF)) { args[i] = ((DoubleF)args[i]).ToString(_decPlacesFormat); }
				else if (args[i] is SpObject) { args[i] = ((SpObject)args[i]).Name; }
			}
			return String.Format(s, args);
		}


		// ---------------------------------------------------------------------------
		// METHODES POUR LA RECUPERATION DES OBJETS OU LE CHANGEMENT DE NOMS DANS UNE FORMULE:


		/// <summary>
		/// Retourne un tableau de SpObject contenant les objets appelés par des fonctions par leur nom dans une formule passé en argument.
		/// </summary>
		protected static SpObject[] GetObjectsFromFormula(string formula)
		{
			SpObjectsCollection coll = SpObjectsCollection.GetInstance();
			// Obtient le tableau des noms de tous les objets de la collection, construit le pattern et cherche un a un les
			// noms d'objets, avant de les convertir:
			string names = String.Format("({0})", My.ArrayFunctions.Join(coll.GetAllNames(), "|"));
			Regex reg = new Regex(String.Format(@"(\(|,)({0})(\)|,)", names));
			// On doit passer par une boucle et non par un MatchCollection, sinon dans une expression comme "func(A,B)",
			// le B passe à la trappe:
			Match m; int index = 0, c = 0; string[] result = new string[5]; string temp;
			while ((m = reg.Match(formula, index)).Success) {
				if (c >= result.Length) { Array.Resize(ref result, c + 5); }
				temp = m.Value;
				result[c++] = temp.Substring(1, temp.Length - 2);
				index = m.Index + m.Length - 1; }
			Array.Resize(ref result, c);
			return coll.GetObjets(result.Distinct().ToArray(), true);
		}

		/// <summary>
		/// Voir surcharge. La formule est contenue dans un DoubleF. S'il n'y a pas de formule, retourne un tableau vide.
		/// </summary>
		protected static SpObject[] GetObjectsFromFormula(DoubleF nb)
		{
			if (nb.IsFormula) { return GetObjectsFromFormula(nb.Formula); }
			else { return new SpObject[0]; }
		}

		/// <summary>
		/// Retourne une nouvelle formule, ayant changé oldName par newName.
		/// </summary>
		protected static void ChangeNameInFormula(ref string formula, string oldName, string newName)
		{
			// Construit le pattern et pour chaque occurence de oldName remplace par newName:
			Regex reg = new Regex(String.Format(@"(\(|,)({0})(\)|,)", oldName));
			// On doit passer par une boucle et non par un MatchCollection, sinon dans une expression comme "func(A,B)",
			// le B passe à la trappe:
			Match m; int index = 0;
			while ((m = reg.Match(formula, index)).Success) {
				formula = formula.Replace(m.Value, m.Value.Replace(oldName, newName));
				if ((index = m.Index + m.Length - 1) >= formula.Length) { break; }; }
		}

		/// <summary>
		/// Voir surcharge. Modifie directement le DoubleF. S'il n'y a pas de formule, ne fait rien.
		/// </summary>
		protected static void ChangeNameInFormula(ref DoubleF nb, string oldName, string newName)
		{
			if (nb.IsFormula) {
				string newFormula = nb.Formula;
				ChangeNameInFormula(ref newFormula, oldName, newName);
				if (newFormula != nb.Formula) { nb.Alter(newFormula); } }
		}

	}


	// ===========================================================================
	


	/// <summary>
	/// Classe de base pour les objets de l'espace, qui y sont tous rattachés. Pour la suppression d'objet: Tous les objets crées qui dépendent d'autres objets doivent s'inscrire à l'événement ObjectDeleted des objets dont ils dépendent. Lorsque la propriété Deleted prend la valeur true, l'événement est décleché, et les objets dépendant se marque aussi comme à supprimer, et ainsi de suite pour les dépendances des dépendances. La seule chose à faire, donc, quand on crée une nouvelle classe, c'est de s'inscrire à l'événement ObjectDeleted des objets dont on dépend. Le même principe est appliqué pour NumericDataChanged, et les autres événements.
	/// </summary>
	public abstract partial class SpObject
	{
	
		// DECLARATIONS:
		// Propriétés système:
		private string __Name;
		private SolidBrush __LabelBrush;
		// Propriétés d'affichage:
		private Color __Color;
		protected Coord3D _labelOrigin;
		protected double _labelOriginParam;
		// Variables:
		private ObjectFlags _flags;
		private SpObject[] _masterObjects;
		private SpObject[] _allMasterObjects;
		private SpObject[] _ownedObjects;
		private string __CtorFormula;
		private bool _hasCircularRef;
		private SpObject _owner;
		private string _systemName;
		private int _masterAlteredCounter;


		// ---------------------------------------------------------------------------
		// PROPRIETES SYTEME:
		
		/// <summary>
		/// Nom de l'objet. Si le nom de l'objet est $, il est marqué comme virtuel, si le nom commence par %, il est marqué comme système, sinon il est marqué comme explicite. Un objet ne peut pas changer de type.
		/// </summary>
		public string Name
		{
			get
			{
				return __Name;
			}
			set
			{
				// Au début le nom est null, et c'est seulement quand le nom est null qu'on peut définir le type d'objet
				// (explicite, virtuel o u système), puisqu'il ne peut plus changer de type après:
				bool firstTime = (__Name == null);
				// Retient le nom, en vu du changement de nom:
				string oldName = __Name;
				// Analyse le nom:
				if (value.StartsWith("%")) {
					__Name = String.Format("{0}[{1}]", value, _virtsysObjCounter++);
					if (firstTime) { _flags = _flags | ObjectFlags.System; _systemName = value; } }
				else if (value.StartsWith("$")) {
					__Name = String.Format("$[{0}]", _virtsysObjCounter++); _systemName = __Name; 
					if (firstTime) { _flags = _flags | ObjectFlags.Virtual; } }
				else {
					string test = GetValidName(value, this);
					if (_getFreeName != null) { test = _getFreeName(test); }
					__Name = test;
					if (firstTime) { _flags = _flags | ObjectFlags.Explicit; }
					if (!IsSystem && !IsVirtual) { _systemName = test; } }
				// Si l'objet a déjà été créé, c'est qu'il y a changement de nom (seult pour les objets Explicit, les
				// autre n'étant pas utilisés dans les formules, sauf pour les objest Extracted), puis demande un nouveau dessin:
				if (_flags == (_flags | ObjectFlags.Created) && !IsVirtual && (!IsSystem || (IsSystem && IsExtracted)) && NameChanged != null)
				{
					NameChanged(this, new NameChangedEventArgs(oldName, __Name));
					SendInfos(String.Format("\"{0}\" renamed to \"{1}\"", oldName, __Name));
					OnRequestDrawing();
				}
			}
		}
		
		/// <summary>
		/// Obtient le "nom système". Pour les objets explicites et virtuels, il s'agit de Name et pour les objets systèmes (qui peuvent changer de nom s'ils sont extraits), il s'agit du nom système original, sans le numéro de série.
		/// </summary>
		public string SystemName { get { return _systemName; } }
		
		/// <summary>
		/// Obtient le "nom constructeur". Il s'agit de Name si l'objet n'a pas été créer par une formule, ou de la formule de création sinon.
		/// </summary>
		public string CtorName { get { return (_flags==(_flags|ObjectFlags.Formula) ? __CtorFormula : Name); } }
		
		/// <summary>
		/// Obtient si l'objet n'est pas défini, cad ne peut être affiché par suite d'une erreur dans les calculs ou dans la définition.
		/// </summary>
		public bool IsUndefined { get { return (_flags == (_flags | ObjectFlags.Undefined)); } }
		
		/// <summary>
		/// Obtient si l'objet est un objet système.
		/// </summary>
		public bool IsSystem { get { return (_flags == (_flags | ObjectFlags.System)); } }
		
		/// <summary>
		/// Obtient si l'objet est un objet virtuel.
		/// </summary>
		public bool IsVirtual { get { return (_flags == (_flags | ObjectFlags.Virtual)); } }
		
		/// <summary>
		/// Obtient si l'objet est un objet extrait.
		/// </summary>
		public bool IsExtracted { get { return (_flags == (_flags | ObjectFlags.Extracted)); } }
		
		/// <summary>
		/// Obtient si l'objet est un objet extrait.
		/// </summary>
		public SpObject Owner { get { return _owner; } private set { _owner = value; } }
		
		/// <summary>
		/// Obtient si l'objet est un objet définit par une formule (tous les objets virtuels ne sont pas définis par une formule, seulement owned de premier niveau d'un objet explicite. Les virtuels qui sont des owned de niveau supérieur sont "inclus" dans la formule du premier.
		/// </summary>
		public bool IsDefinedByFormula { get { return (_flags == (_flags | ObjectFlags.Formula)); } }
		
		/// <summary>
		/// Obtient ou définit la formule de création de l'objet. L'objet est marqué comme créer par une formule, et la propriété CtorName retourne la formule plutôt que le nom.
		/// </summary>
		public string CtorFormula {
			get { return __CtorFormula; }
			set { __CtorFormula = value; _flags = _flags | ObjectFlags.Formula; } }
		
		/// <summary>
		/// Marque l'objet comme Deleted et déclenche l'événement ObjectDeleted pour que les objets dépendant qui se sont inscrits aux événements des objets dont ils dépendent puissent se marquer eux aussi comme Deleted.
		/// </summary>
		public bool Deleted
		{
			get { return (_flags == (_flags | ObjectFlags.Deleted)); }
			internal set
			{
				// Si on est déjà passé par là, inutile de recommencer:
				if (!value || _flags == ObjectFlags.Deleted || _flags == ObjectFlags.Deleting) { return; }
				// Marque comme Deleting, pour éviter les appels récurrents:
				_flags = _flags | ObjectFlags.Deleting;
				// Déclenche l'événement pour supprimer les esclaves:
				if (ObjectDeleted != null) { ObjectDeleted(this, new EventArgs()); }
				// Si Extracted et que l'owner n'est pas supprimé ou en cours de suppression, on réintègre l'objet en tant qu'objet
				// système normal (non Extracted) sans le supprimer (il aura quand même eu le temps de supprimer ses esclaves), et on enlève
				// des masters et des owneds les masters et owneds marqués comme Deleted (mais pas les autres, qui sont indépendants ou 
				// qui sont indispensables au fonctionnement de l'objet système:
				if (IsExtracted && _owner._flags != (_owner._flags|ObjectFlags.Deleting) && _owner._flags != (_owner._flags|ObjectFlags.Deleted))
				{
					string oldName = Name;
					_flags = _flags ^ ObjectFlags.Extracted;
					_flags = _flags ^ ObjectFlags.Deleting;
					Name = _systemName;
					ChangeMastersAndOwneds(_masterObjects.Where(delegate(SpObject o) { return !o.Deleted; }).ToArray(),
						_ownedObjects.Where(delegate(SpObject o) { return !o.Deleted; }).ToArray());
					SendInfos(String.Format("{0} is not deleted (its owner {1} already exists), but no more extracted. Its new name is {2}.",
						oldName, _owner.Name, Name));
					return;
				}
				// Sinon, on supprime les maîtres et les owneds, les références aux événéments externes, puis se marque comme supprimer:
				else
				{
					// Très important, sinon il reste la référence des objets dans les masters, et l'objet n'est pas supprimé!
					ChangeMastersAndOwneds(null, null);
					_ownedObjects = new SpObject[0]; _masterObjects = new SpObject[0]; _allMasterObjects = new SpObject[0];
					// Supprime les références aux événements:
					//this.SelectionChanged -= SpObject_SelectionChanged; // Inutile puisque événement interne.
					//this.ColorChanged -= SpObject_ColorChanged; // Inutile puisque événement interne.
					SpObject.NameChanged -= SpObject_NameChanged;
					// Se marque comme supprimé et affiche un message:
					_flags = ObjectFlags.Deleted;
					SendInfos(String.Format("{0} marked as deleted.", this.ToString()));
				}
			}
		}
		
		/// <summary>
		/// Obtient si l'objet a subit une erreur de référence circulaire.
		/// </summary>
		public bool HasCircularRef { get { return _hasCircularRef; } }
		
		/// <summary>
		/// Obtient le tableau des objets maîtres.
		/// </summary>
		public SpObject[] MasterObjects { get { return _masterObjects; } }

		/// <summary>
		/// Obtient la liste de tous les objets maîtres de l'objet, y compris les maîtres des maîtres, etc.
		/// </summary>
		internal SpObject[] AllMastersObjects { get { return _allMasterObjects; } }

		/// <summary>
		/// Obtient la liste des objets virtuels ou système appartenant à cet objet-ci.
		/// </summary>
		public SpObject[] OwnedObjects { get { return _ownedObjects; } }


		// ---------------------------------------------------------------------------
		// PROPRIETES D'AFFICHAGE:
		
		/// <summary>
		/// Coordonnées 2D du point d'origine du label, et qui peut varier selon les objets (le point lui-même pour un point, le centre d'un segment, etc.).
		/// </summary>
		internal PointF LabelOriginOnWin { get; set; }
		
		/// <summary>
		/// Coordonnées 2D du label calculer à partir de OriginLabel.
		/// </summary>
		public Point LabelCoordsOnWin { get; set; }
		
		/// <summary>
		/// Obtient les coordonnées 3D de l'origine du label. Pour certains objets, obtient une structure vide, comme pour les points (l'origine du label étant le point lui-même).
		/// </summary>
		internal Coord3D LabelOrigin { get { return _labelOrigin; } }
		
		/// <summary>
		/// Obtient ou définit le paramètre qui définit l'origine du label. Ce paramètre change de signification selon les objets. Pour les droites et vecteurs, par exemple, il représente le coefficient multiplicateur du vecteur directeur qui sert à la translation du point de base de la droite. Pour les cercle, il s'agit d'un angle, etc.
		/// </summary>
		public double LabelOriginParam {
			get { return _labelOriginParam; }
			set { _labelOriginParam = value; CalculateLabelOrigin(); } }
		
		/// <summary>
		/// Police du label.
		/// </summary>
		public Font LabelFont { get; internal set; }
		
		/// <summary>
		/// Retourne le Brush pour le label. Lecture seule.
		/// </summary>
		public SolidBrush LabelBrush { get { return __LabelBrush; } }
		
		/// <summary>
		/// Couleur de l'objet et de son label. Déclenche l'événement ColorChanged.
		/// </summary>
		public virtual Color Color {
			get { return __Color; }
			internal set { __Color = value; if (ColorChanged != null) { ColorChanged(this, new EventArgs()); } } }
				
		/// <summary>
		/// Obtient ou définit si l'objet est caché ou affiché. Indépendant de ShowName, cad qu'un label peut être visible même si l'objet, lui, ne l'est pas.
		/// </summary>
		public bool Hidden { get; set; }
		
		/// <summary>
		/// Obtient ou définit si les labels de nom sont affichés. Indépendant de Hidden, cad qu'un label peut être visible même si l'objet, lui, ne l'est pas.
		/// </summary>
		public bool ShowName { get; set; }
		
		/// <summary>
		/// Obtient ou définit si l'objet est sélectionné. Déclenche SelectionChanged.
		/// </summary>
		public bool Selected
		{
			get { return (_flags == (_flags | ObjectFlags.Selected)); }
			internal set {
				if (value != Selected) { _flags = _flags ^ ObjectFlags.Selected; }
				if (SelectionChanged != null) { SelectionChanged(this, new EventArgs()); } }
		}
		
	
		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS ET METHODES:
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		protected SpObject(string name)
		{
			// Initialisation des variables:
			_flags = ObjectFlags.None;
			__CtorFormula = string.Empty;
			_masterObjects = new SpObject[0];
			_allMasterObjects = new SpObject[0];
			_ownedObjects = new SpObject[0];
			_owner = null;
			_masterAlteredCounter = 0;
			__Color = _defColor;
			__LabelBrush = new SolidBrush(this.Color);
			_labelOrigin = new Coord3D(true);
			_labelOriginParam = 0;
			Hidden = false; ShowName = true;
			LabelFont = _defFont;
			// Initialisation du nom:
			__Name = null;
			Name = name;
			// Inscription aux événements pour la recréation du LabelBrush:
			this.ColorChanged += new EventHandler(SpObject_ColorChanged);
			this.SelectionChanged += new EventHandler(SpObject_SelectionChanged);
			// Inscription à l'événement de changement de nom d'un objet:
			SpObject.NameChanged += new NameChangedEventHandler(SpObject_NameChanged);
		}
		
		/// <summary>
		/// Destructeur. Envoie un message disant que l'objet a été supprimé.
		/// </summary>
		~SpObject()
		{
			DeletedObjectsInfos(String.Format("{0} \"really\" deleted at {1}.", this.ToString(false),
				DateTime.Now.ToLongTimeString()));
			//System.Windows.Forms.MessageBox.Show(this.ToString(false) + " \"really\" deleted.");
		}

		/// <summary>
		/// Tente de changer la valeur de la propriété IsUndefined. Si value vaut true, alors la propriété est mise à true, et l'événement IsUndefinedChanged est déclenché pour que les esclaves se mettent à true aussi. Si value vaut false, alors le propriété est mise à false à la condition que tous les maîtres directs soient définis. Si c'est le cas, l'événement est déclenché pour que les esclaves tentent eux aussi de se mettre à false. Retourne la valeur de IsUndefined au bout de l'opération. L'événement n'est déclenché que si la valeur a réellement changée. N'affiche aucun message.
		/// </summary>
		internal bool SetIsUndefined(bool value)
		{
			// Si l'objet doit être marqué comme défini, on teste les maîtres. Si l'un est indéfini, on prend
			// se marque comme indéfini. Sinon, on peut passer en mode défini. Déclenche l'événement dans tous
			// cas.
			bool aMasterUndefined = false; bool oldValue = IsUndefined;
			if (!value)
			{
				foreach (SpObject o in _masterObjects) { if (o.IsUndefined) { aMasterUndefined = true; break; } }
				if (!aMasterUndefined) { // Si pas de maître indéfini, c'est bon:
					if (oldValue) { _flags = _flags ^ ObjectFlags.Undefined; }
					if (IsUndefined != oldValue && IsUndefinedChanged != null) { IsUndefinedChanged(this, new IsUndefinedChangedEventArgs(false)); } }
			}
			// Si l'objet doit être marqué comme Undefined, ou si on vient de trouver un maître indéfini,
			// déclenche l'événement pour les esclaves:
			if (value || aMasterUndefined) {
				if (!oldValue) { _flags = _flags ^ ObjectFlags.Undefined; }
				if (IsUndefined != oldValue && IsUndefinedChanged != null) { IsUndefinedChanged(this, new IsUndefinedChangedEventArgs(true)); }
				// Et met les owneds systèmes (mais pas virtuels comme indéfinis):
				foreach (SpObject o in _ownedObjects) { if (o.IsSystem) { o.SetIsUndefined(true); } }
				}
			// Retourne la valeur finale de IsUndefined:
			return IsUndefined;
		}

		/// <summary>
		/// Comme EndAlterProcess. Pour ajouter des objets dans owned mais pas dans masters, il faut les mettre dans le tableau, en dernières positions, séparés des masters par un null. Par exemple new SpObject[]{mast1,mast2,null,owned1,owend2,...}. Puis la surcharge EndAlterProcess(masters, owned) est appelée.
		/// </summary>
		protected void EndAlterProcess(params SpObject[] objs)
		{
			SpObject[] masters, owned; My.ArrayFunctions.SplitTwoArrays(objs, out masters, out owned);
			EndAlterProcess(masters, owned);
		}
		
		/// <summary>
		/// Comme EndAlterProcess, mais offre la possibilité de mélanger en paramètres des SpObject uniques et des tableaux de SpObject. Tous les objets passés doivent être de type SpObject. L'élément null permet de séparer les Masters des Owneds.
		/// </summary>
		protected void EndAlterProcess(params object[] objs)
			{ EndAlterProcess(My.ArrayFunctions.UnrollArray<SpObject>(objs)); }
		
		/// <summary>
		/// Comme EndAlterProcess.
		/// </summary>
		protected void EndAlterProcess()
			{ EndAlterProcess(new SpObject[0], new SpObject[0]); }		

		/// <summary>
		/// Ajoute les objets passés aux MasterObjects et aux OwnedObjects, tente de définir IsUndifined à false, puis, si réussit, appelle CalculateNumericData, puis OnBigMasterAltered et enfin indique que l'objet a été construit (si c'est le premier appel) et appelle ObjectChanged (événement static pour l'extérieur). Tous les objets virtuels des MasterObjets sont ajoutés aux OwnedObjects. Pour ajouter des objets dans owned mais pas dans masters, voir surcharge.
		/// </summary>
		private void EndAlterProcess(SpObject[] mastersToAdd, SpObject[] ownedsToAdd)
		{
			// Met le flag Altering:
			if (_flags == (_flags|ObjectFlags.Created) && _flags != (_flags|ObjectFlags.Altering))
				{ _flags = _flags ^ ObjectFlags.Altering; }
			// Si réf circulaire, on met à Undefined, sinon on tente de mettre IsUndefined à false,
			// et si ça marche (ie. si tous les maîtres sont définis), on lance un calcul:
			if (!ChangeMastersAndOwneds(mastersToAdd, ownedsToAdd)) { SendCalculationResult(true, null); }
			else if (!SetIsUndefined(false)) {
				// On ne recalcule pas les objets systèmes lors de leur création, puisqu'ils le sont de toute façon plus tard.
				// De même on ne recalcule pas si 
				if (IsSystem && _flags != (_flags|ObjectFlags.Created)) { SendInfos(String.Format("{0} created.", ToString())); }
				else if (_flags == (_flags|ObjectFlags.DontRecalc)) { /* rien */; }
				else { CalculateNumericData(); CalculateLabelOrigin(); } }
			else { SendCalculationResult(true, "A master is undefined."); }
			// Dans tous les cas, on lance un OnBigMasterAltered():
			OnBigMasterAltered();
			// Met à jour les flags:
			_flags = _flags | ObjectFlags.Created;
			if (_flags == (_flags|ObjectFlags.Altering)) { _flags = _flags ^ ObjectFlags.Altering; }
			// Appelle ObjectChanged:
			if (ObjectChanged != null) { ObjectChanged(this, new ObjectChangedEventArgs(this)); }
		}
		
		/// <summary>
		/// Recalcule les données numériques de l'objet. Si l'objet est indéfini, tente de le recalculer à la condition que tous ses maîtres soient définis.
		/// </summary>
		public void Recalculate(bool callBigMasterAltered)
		{
			if (!SetIsUndefined(false))
				{ CalculateNumericData(); CalculateLabelOrigin(); if (callBigMasterAltered) { OnBigMasterAltered(); } }
			else
				{ if (_showCalcRes) { SendError(String.Format("{0} is undefined, enable to recalculate.", ToString(true))); } }
		}
		
		/// <summary>
		/// Reconstruit la __CtorFormula pour changer le nom d'un objet.
		/// </summary>
		private void RebuildObjectFormula(string oldName, string newName)
		{
			if (!String.IsNullOrEmpty(__CtorFormula))
				{ ChangeNameInFormula(ref __CtorFormula, oldName, newName); }
		}
		
		/// <summary>
		/// Affiche un message indiquant que l'objet a été créé, modifié ou recalculé (le choix est fait automatiquement). Tente de changer la propriété IsUndefined. Le message est affiché quoiqu'il arrive, mais il est affiché en tant qu'erreur.
		/// </summary>
		protected void SendCalculationResult(bool isUndefined, string message)
		{
			// Message si pas null ou vide:
			if (!String.IsNullOrEmpty(message)) {
				SendError(String.Format("{0}: {1}", ToString(false), message)); }
			// Tente de mettre IsUndefined à isUndefined, et si toujours à true, affiche message:
			if (SetIsUndefined(isUndefined)) {
				if (_flags != (_flags | ObjectFlags.Created)) {
					SendInfos(String.Format("{0} created but undefined.", ToString(true))); }
				else {
					SendError(String.Format("{0} is undefined. Enable to {1}.", ToString(true),
						(_flags==(_flags|ObjectFlags.Altering) ? "alter" : "recalculate"))); } }
			// Si n'est pas Undefined, autre message:
			else if (_showCalcRes) {
				SendInfos(String.Format("{0} {1}.", ToString(), (_flags==(_flags|ObjectFlags.Created) ?
					(_flags==(_flags|ObjectFlags.Altering) ? "altered" : "recalculated") : "created"))); }
		}

		/// <summary>
		/// Affiche un message comme quoi la création, modification ou le calcul s'est bien passé.
		/// </summary>
		protected void SendCalculationResult()
			{ SendCalculationResult(false, null); }

		/// <summary>
		/// Retourne le nom de l'objet.
		/// </summary>
		public override string ToString()
			{ return MakeToString(""); }
	
		/// <summary>
		/// Retourne une version simplifier du ToString() normal défini par chaque objet. addIsUndefined indique s'il faut inclure dans la chaîne que l'objet est indéfini, si c'est le cas.
		/// </summary>
		protected string ToString(bool addIsUndefined)
		{
			StringBuilder sb = new StringBuilder();
			if (IsUndefined && addIsUndefined) { sb.Append("(Undefined) "); }
			sb.AppendFormat("{0} {1}", this.TypeDescription, this.Name);
			sb.AppendFormat(" (Masters: {0})", (this.MasterObjects.Length == 0 ? "None" : this.GetMasterObjectsList()));
			if (IsVirtual || IsSystem) { sb.AppendFormat(" (Owner: {0})", (_owner==null ? "null" : _owner.Name)); }
			return sb.ToString();
		}
	
		/// <summary>
		/// Retourne une chaîne telle que "ObjectType Name text (Masters: ...)". text doit être une chaîne de formattage pour String.Format, et args les arguments correspondant. Si un objet SpObject est passé, alors c'est la propriété Name qui est utilisé. Sinon, c'est la méthode ToString() qui est utilisé sur l'objet.
		/// </summary>
		public virtual string MakeToString(string text, params object[] args)
		{
			StringBuilder sb = new StringBuilder();
			if (IsUndefined) { sb.Append("(Undefined) "); }
			sb.AppendFormat("{0} {1} {2}", TypeDescription, Name, FormatText(text, args));
			sb.AppendFormat(" (Masters: {0})", (MasterObjects.Length == 0 ? "None" : GetMasterObjectsList()));
			if (IsVirtual || IsSystem) { sb.AppendFormat(" (Owner: {0})", (_owner==null ? "null" : _owner.Name)); }
			return sb.ToString();
		}
		
		/// <summary>
		/// Retourne les paramètres utilisés pour la construction de l'objet. Si addName vaut true, le nom est ajouté au début (comme pour le constructeur), sinon, le nom est absent (comme pout la méthode Alter).
		/// </summary>
		public string GetCtorString(bool addName)
		{
			object[] arr = GetCtorObjects();
			if (addName) { arr = new object[]{__Name}.Concat(arr).ToArray(); }
			Func<object,string> convertToStr = delegate(object o)
				{
					if (o is SpObject) { return ((SpObject)o).CtorName; }
					else if (o is DoubleF) { return ((DoubleF)o).GetStrValue("R20"); }
					else if (o is WeightedPoint)
						{ return String.Format("{0}:{1}", ((WeightedPoint)o).Point.CtorName,
						((WeightedPoint)o).Weight.GetStrValue("R20")); }
					return o.ToString();
				};
			return My.FieldsParser.EscapeArray<object>(arr, ",", convertToStr, true);
		}

		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public virtual string GetInfos(params object[] lines)
		{
			string allMasters = My.ArrayFunctions.Join(Array.ConvertAll<SpObject,string>(_allMasterObjects,
				delegate(SpObject o) { return o.Name; }).ToArray(), ",");
			if (_allMasterObjects.Length == 0) { allMasters = "None"; }
			int c = 0;
			string ownedObjs = My.ArrayFunctions.Join(Array.ConvertAll<SpObject,string>(GetAllOwneds(this),
				delegate(SpObject o) { return String.Format("   ({0}) {1}", c++, o.ToString()); }).ToArray(), "\n");
			if (_ownedObjects.Length == 0) { ownedObjs = "   None"; }
			return String.Format("{0}\nCtor: {1}\nAll masters: {2}\nOwned objects:\n{3}{4}", ToString(), GetCtorString(true),
				allMasters, ownedObjs, (lines.Length==0 ? "" : "\n" + My.ArrayFunctions.Join(My.ArrayFunctions.UnrollArray<string>(lines), "\n")));
		}
				
		/// <summary>
		/// Déclenche les événements RequestDrawingCalc, ObjectAltered et RequestDrawing. Cette méthode ne doit être appelé que lorsqu'un objet est modifié de l'extérieur (et non par un autre maître), et demande que tous ces esclaves soient modifiés. Cette méthode ne doit pas être appelé par les esclaves quand ils sont modifiés par des maîtres (l'appel se fait directement dans le gestionnaire de l'événement ObjectAltered).
		/// </summary>
		protected void OnBigMasterAltered()
		{
			// RequestDrawingCalc appelé dans tous les cas. ObjectAltered appelé dans tous les cas. RequestDrawing appelé
			// seulement si l'objet est Excplicit, et si l'objet a déjà été créé.
			if (RequestDrawingCalc != null) { RequestDrawingCalc(this, new RequestDrawingCalcEventArgs(this)); }
			if (PreObjectAltered != null) { PreObjectAltered(this, new ObjectAlteredEventArgs(this)); }
			if (ObjectAltered != null) { ObjectAltered(this, new ObjectAlteredEventArgs(this)); }
			if (_flags == (_flags|ObjectFlags.Explicit) && _flags == (_flags|ObjectFlags.Created) && RequestDrawing != null)
				{ RequestDrawing(this, new EventArgs()); }
		}

		/// <summary>
		/// Déclenche l'événement RequestDrawing qui demande à un DrawingArea de redessiner. Il ne faut pas l'utiliser lorsqu'une propriété change parce qu'un maître a changé, ou si on déclenche ensuite BigMasterAltered. Il ne faut l'utiliser que lorsqu'une propriété d'affichage change, hors de tout autre déclenchement d'autres événements.
		/// </summary>
		protected void OnRequestDrawing()
			{ if (RequestDrawing != null) { RequestDrawing(this, new EventArgs()); } }

		/// <summary>
		/// Déclenche l'événement RequestDrawingCalc qui demande à un DrawingArea de recalculer un objet. Il ne faut pas l'utiliser lorsqu'une propriété change parce qu'un maître a changé, ou si on déclenche ensuite BigMasterAltered. Il ne faut l'utiliser que lorsqu'une propriété d'affichage change, hors de tout autre déclenchement d'autres événements.
		/// </summary>
		protected void OnRequestDrawingCalc(SpObject obj)
			{ if (RequestDrawingCalc != null) { RequestDrawingCalc(this, new RequestDrawingCalcEventArgs(obj)); } }

		/// <summary>
		/// Envoie un message d'information.
		/// </summary>
		protected void SendInfos(string msg)
			{ GeoMsgSender.SendInfos(this, msg); }

		/// <summary>
		/// Envoie un message d'erreur.
		/// </summary>
		protected void SendError(string msg)
			{ GeoMsgSender.SendError(this, msg); }

		/// <summary>
		/// Gestionnaire d'événement. Reconstruit le Brush pour l'étiquette lorsque la sélection change.
		/// </summary>
		private void SpObject_SelectionChanged(object sender, EventArgs e)
		{
			__LabelBrush = new SolidBrush(Selected ? SpObject._selColor : this.Color);
		}

		/// <summary>
		/// Gestionnaire d'événement. Reconstruit le Brush pour l'étiquette lorsque la couleur change.
		/// </summary>
		void SpObject_ColorChanged(object sender, EventArgs e)
		{
			__LabelBrush = new SolidBrush(Selected ? SpObject._selColor : this.Color);
		}
		
		/// <summary>
		/// Gestionnaire d'événement. Lance un RebuildObjectFormula et un RebuildFormulas lorsque le nom a changé.
		/// </summary>
		private void SpObject_NameChanged(object sender, SpObject.NameChangedEventArgs e)
		{
			RebuildObjectFormula(e.OldName, e.NewName);
			RebuildFormulas(e.OldName, e.NewName);
		}
		
		// ---------------------------------------------------------------------------
		// MASTER AND OWNED OBJECTS:
		
		/// <summary>
		/// Désinscrit les masters actuels, détruit les owneds actuels, puis ajoute les nouveaux masters et les nouveaux owneds. Retourne true si tous c'est bien passé, ie. s'il n'y a pas eu de référence circulaire lors de l'ajout des maîtres, false sinon. Si mastersToAdd et ownedsToAdd sont nuls ou vides, cette procédure désinscrit les masters et détruit les owneds. Les masters virtuels (mais pas systèmes) sont ajoutés aux owneds, en plus des owneds de ownedsToAdd. Définit la propriété owner pour les owneds, à condition qu'elle soit nulle (ce qui veut dire qu'un owned ne peut changer d'owner). Il ne faut pas définir d'objets explicites dans les owneds, sinon il y a une StackOverflowException (et aucun contrôle n'est effectué ici).
		/// </summary>
		private bool ChangeMastersAndOwneds(SpObject[] mastersToAdd, SpObject[] ownedsToAdd)
		{
		
			// Si null:
			if (mastersToAdd == null) { mastersToAdd = new SpObject[0]; }
			if (ownedsToAdd == null) { ownedsToAdd = new SpObject[0]; }
		
			// Enlève les masters, ie. les désinscrit des événements et supprime le tableau:
			int lenMaster = _masterObjects.Length;
			for (int i=0; i<lenMaster; i++) {
				_masterObjects[i].PreObjectAltered -= MasterObject_PreObjectAltered;
				_masterObjects[i].ObjectAltered -= MasterObject_ObjectAltered;
				_masterObjects[i].ObjectDeleted -= MasterObject_ObjectDeleted;
				_masterObjects[i].IsUndefinedChanged -= MasterObject_IsUndefinedChanged;
				_masterObjects[i].MastersListChanged -= MasterObject_MastersListChanged;
				_masterObjects[i] = null; }
			_masterObjects = null;
			
			// Forme un nouveau tableau contenant les owned, concaténant les ownedsToAdd et les masters
			// virtuels (mais pas systèmes), à la condition que l'objet this n'est pas système, car les objets
			// virtuels ne peuvent pas être owned d'un objet système: c'est l'owner explicite du système qui doit
			// être l'owner de l'ensemble des objets virtuels. Par contre, un virtuel peut être un owned d'un
			// virtuel:
			SpObject[] newOwned = ownedsToAdd;
			if (!IsSystem) { newOwned = newOwned.Concat(
				mastersToAdd.Where(delegate(SpObject o) { return (o.IsVirtual); })).ToArray(); }
			
			// Définit l'owner (cet objet this) des owned (la première fois, ie. si Owner est null):
			foreach (SpObject o in newOwned) { if (o.Owner == null) { o.Owner = this; } }
			
			// Détruit les owned, sauf s'ils sont à réinscrire (ie. s'ils sont dans newOwned):
			int lenOwned = _ownedObjects.Length;
			for (int i=0; i<lenOwned; i++) {
				if (!newOwned.Contains(_ownedObjects[i])) { _ownedObjects[i].Deleted = true; }
				_ownedObjects[i] = null; }
			_ownedObjects = null;
			
			// Inscrit les nouveaux owned dans le tableau:
			_ownedObjects = newOwned;
			
			// Inscrit les nouveaux maîtres, ie. inscrit aux événements:
			mastersToAdd = mastersToAdd.Distinct().ToArray();
			_hasCircularRef = false; lenMaster = mastersToAdd.Length;
			for (int i=0; i<lenMaster; i++) {
				// Détecte une référence circulaire, et s'il y en a une, saute et continue pour les autres masters:
				if (DetectCircularReference(mastersToAdd[i], true)) { _hasCircularRef = true; mastersToAdd[i] = null; continue; }
				mastersToAdd[i].PreObjectAltered += new ObjectAlteredEventHandler(MasterObject_PreObjectAltered);
				mastersToAdd[i].ObjectAltered += new ObjectAlteredEventHandler(MasterObject_ObjectAltered);
				mastersToAdd[i].ObjectDeleted += new EventHandler(MasterObject_ObjectDeleted);
				mastersToAdd[i].IsUndefinedChanged += new IsUndefinedChangedEventHandler(MasterObject_IsUndefinedChanged);
				mastersToAdd[i].MastersListChanged += new EventHandler(MasterObject_MastersListChanged); }
			_masterObjects = mastersToAdd.Where(delegate(SpObject o) { return o != null; }).Distinct().ToArray();
			
			// Obtient tous les maîtres des maîtres, etc., et déclenche l'event de changement de la liste des objets maîtres:
			_allMasterObjects = SpObject.GetAllMasters(this);
			if (MastersListChanged != null) { MastersListChanged(this, new EventArgs()); }
			
			// Retour:
			return !_hasCircularRef;
			
		}
		
		/// <summary>
		/// objs doit avoir un nombre d'éléments pair. Vérifie que pour l'objet sur lequel porte la méthode (qui, en toute logique, doit être un objet système), chaque premier éléments des paires d'éléments de objs est différent du second. Si c'est le cas, l'objet est reconstruit via sa méthode Alter, avec les nouveaux éléments qui remplacent les anciens. Retourne true si l'objet, après ce changement, est défini, ou false s'il ne l'est pas. Si recalculate est true, alors l'objet est recalculé et un OnBigMasterAltered est lancé (seulement si l'objet a été reconstruit, ie. s'il y a eu des changements). Sinon, l'objet n'est pas recalculé, et l'événement n'est pas lancé (il faut alors le faire manuellement par la suite).
		/// </summary>
		internal bool RebuildObject(bool recalculate, params object[] objs)
		{
			// Récupère les objets de constructions, et compare que les objets des paramètres sont différents
			// de ceux déjà présents. Si tout est pareil, sort:
			object[] ctorObjs = this.GetCtorObjects();
			int l = Math.Min(ctorObjs.Length, objs.Length); bool changed = false;
			for (int i=0; i<l; i++) { if (objs[i] != null && !ctorObjs[i].Equals(objs[i])) { ctorObjs[i] = objs[i]; changed = true; } }
			if (!changed) { return !IsUndefined; }
			// Cherche la méthode Alter corresondante, et l'invoque:
			MethodInfo meth = this.GetType().GetMethod("Alter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
				Type.GetTypeArray(ctorObjs), null);
			if (meth == null) {
				SendError(String.Format("Enable to alter system object {0} because Alter method was not found.", Name));
				return false; }
			// Si on ne doit pas recalculer, on met le flag:
			if (!recalculate) { _flags = (_flags ^ ObjectFlags.DontRecalc); }
			meth.Invoke(this, ctorObjs);
			if (!recalculate) { _flags = (_flags ^ ObjectFlags.DontRecalc); }
			return !IsUndefined;
		}
		
		/// <summary>
		/// Examine la liste de tous les maîtres, et les maîtres des maîtres, etc. de obj, afin de trouver trace de l'objet courant (this). S'il est référencé dans les maîtres de obj, retourne true, car il y a une référence circulaire (ce qui provoque fatalement un StackOverflow).
		/// </summary>
		public bool DetectCircularReference(SpObject obj, bool showError)
		{
			SpObject[] masters = obj.AllMastersObjects; bool error = false;
			if (this.Equals(obj)) { error = true; }
			else { foreach (SpObject o in masters) { if (this.Equals(o)) { error = true; break; } } }
			if (error && showError)
				{ SendError(String.Format("Enable to define {0} as master of {1}, because there is a circular reference.",
					obj.Name, this.Name)); }
			return error;
		}
		
		/// <summary>
		/// Extrait l'objet en lui donnant un nouveau nom, puis en le marquant comme extrait et en déclenchant l'événement ObjectExtracted. Retourne true si réussit (seul les objets systèmes peuvent être extraits, et une seule fois).
		/// </summary>
		public bool Extract(string newName)
		{
			// Si pas système, erreur et sort:
			if (!IsSystem) { SendError(String.Format("{0} is not a system object. Enable to extract.", Name)); return false; }
			// Si déjà extrait, erreur et sort:
			if (IsExtracted) { SendError(String.Format("{0} is already extracted. Enable to extract.", Name)); return false; }
			// Change le nom et le marquage:
			Name = GetValidName(newName, this);
			_flags = _flags ^ ObjectFlags.Extracted;
			// Message:
			SendInfos(String.Format("{0} extracted.", ToString()));
			// Lance les événements:
			if (ObjectExtracted != null) { ObjectExtracted(this, new ObjectExtractedEventArgs(this)); }
			return true;
		}
		
		/// <summary>
		/// Retourne la liste des noms des objets maîtres, séparés par des virgules.
		/// </summary>
		protected string GetMasterObjectsList()
			{ return My.ArrayFunctions.Join(_masterObjects, delegate(SpObject o) { return o.Name; }, ","); }
		
		/// <summary>
		/// Met à jour la liste des maîtres des maîtres, etc., puis lance relance l'événement pour que les esclaves se mettent à jour eux aussi.
		/// </summary>
		private void MasterObject_MastersListChanged(object sender, EventArgs e)
		{
			_allMasterObjects = SpObject.GetAllMasters(this);
			if (MastersListChanged != null) { MastersListChanged(this, new EventArgs()); }
		}
		
		/// <summary>
		/// Remise à zéro du compteur _masterAlteredCounter, et redéclenchement de l'événement pour les esclaves.
		/// </summary>
		private void MasterObject_PreObjectAltered(object sender, ObjectAlteredEventArgs e)
		{
			// Compte le nombre de maîtres qui ne sont pas esclaves (directement ou pas, du BigMaster):
			_masterAlteredCounter = 0; SpObject big = e.BigMasterObj;
			foreach (SpObject o in _masterObjects) { if (!o._allMasterObjects.Contains(big)) { _masterAlteredCounter++; } }
			if (PreObjectAltered != null) { PreObjectAltered(this, new ObjectAlteredEventArgs(e.BigMasterObj)); }
		}
		
		/// <summary>
		/// Quand un objet maître a changé et s'il n'apparaît plus tard dans le reste de la liste des objets maîtres, on recalcule ses propres données et on déclenche l'événement RequestDrawingCalc. Appelle de plus OnObjectAltered pour déclencher l'événement pour les esclaves. Si l'objet à modifier est Undefined, on tente de mettre à IsUndefined à false (ça marche si les maîtres sont définis), et si échec, on ne fait rien.
		/// </summary>
		private void MasterObject_ObjectAltered(object sender, ObjectAlteredEventArgs e)
		{
			// Si le master est l'owner ont sort (puisqu'il a été recalculé, normalement, dans le CalculateNumericData du 
			// Master/Owner, et cet appel n'est qu'une conséquence d'un objet système défini avec un this:
			if (IsSystem && sender == _owner) { return; }
			// Si l'objet est Undifined, tente de mettre IsUndefined à false (si les parents sont définis), et
			// et si échec, affiche un message d'erreur, et déclenche l'événement ObjectAltered pour que
			// les esclaves affiches à leur tour des messages d'erreur, puis sort:
			if (IsUndefined && SetIsUndefined(false)) {
				if (_showCalcRes) { SendError(String.Format("{0} is undefined. Enable to recalculate.", ToString(true))); }
				if (ObjectAltered != null) { ObjectAltered(this, new ObjectAlteredEventArgs(e.BigMasterObj)); }
				return; }
			// Le compteur _masterAlteredCounter s'incrémente chaque fois que cette procédure est appelée.
			// Lorsqu'il arrive au bout, cad quand le compteur arrive au nombre de maître, se recalcule:
			_masterAlteredCounter++;
			if (_masterAlteredCounter < _masterObjects.Length) { return; }
			// Recalcule, puis déclenche les événements RequestDrawingCalc pour le calcul des données 2D (à l'extérieur) et
			// ObjectAltered pour que les esclaves se modifient à leur tour:
			this.CalculateNumericData();
			if (!IsUndefined) { CalculateLabelOrigin(); }
			// Si le résultat du calcul est IsUndefined, pas la peine de calculer l'affichage 2D:
			if (!IsUndefined && RequestDrawingCalc != null)
				{ RequestDrawingCalc(this, new RequestDrawingCalcEventArgs(this)); }
			// Mais on envoie quand même un événement aux esclaves (qui ont été mis entre temps à Undefined), pour
			// qu'ils affichent un message d'erreur:
			if (ObjectAltered != null) { ObjectAltered(this, new ObjectAlteredEventArgs(e.BigMasterObj)); }
		}

		/// <summary>
		/// Quand un maître change sa valeur IsUndefined, exécute SetIsUndefined.
		/// </summary>
		private void MasterObject_IsUndefinedChanged(object sender, IsUndefinedChangedEventArgs e)
			{ SetIsUndefined(e.Value); }
		
		/// <summary>
		/// Quand un objet maître est marqué comme à supprimer, on se marque soi-même comme à supprimer, sauf si l'objet this est un système, et que le sender est un virtuel, car un objet système peut avoir pour maître un virtuel, mais qu'il ne doit pas être supprimer pour autant (sinon, si on définit par exemple une droite avec des poins virtuels, le vecteur directeur, qui est un système qui a alors pour maître un virtuel, serait supprimer dès que la droite serait redéfinie).
		/// </summary>
		private void MasterObject_ObjectDeleted(object sender, EventArgs e)
		{
			if (IsSystem && ((SpObject)sender).IsVirtual) { return; }
			this.Deleted = true;
		}

	
		// ---------------------------------------------------------------------------
		// METHODES ET PROPRIETES ABSTRAITES, ET METHODES ET PROPRIETES A OVERRIDER:

		/// <summary>
		/// Méthode à overrider. Reconstruit les formules pour changer le nom d'un objet. Appelle ChangeNameInFormula puis reconstruit éventuellement les délégués.
		/// </summary>
		public virtual void RebuildFormulas(string oldName, string newName)
			{ ; }
		
		/// <summary>
		/// Méthode à overrider. Calcule les coordonnées dans l'espace du point d'origine du label. Si cette méthode n'est pas overrider, la propriété LabelOrigin est une structure vide.
		/// </summary>
		protected virtual void CalculateLabelOrigin()
			{ ; }

		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public abstract string TypeDescription { get; }
				
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected abstract void CalculateNumericData();
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public abstract object[] GetCtorObjects();
		
	}


	// ===========================================================================
	
	
	/// <summary>
	/// Classe de base pour les objets de l'espace qui sont dessinés avec un Pen.
	/// </summary>
	public abstract class SpPenObject : SpObject, IPenObject
	{
		
		// Variables:
		protected Pen _pen;
		private float _width;
		private DashStyle _dashStyle;
	
		/// <summary>
		/// Epaisseur du trait pour le Pen.
		/// </summary>
		public float PenWidth {
			get { return _width; }
			set { _width = value; _pen.Width = value; } }
		
		/// <summary>
		/// Style du trait pour le Pen.
		/// </summary>
		public DashStyle DashStyle {
			get { return _dashStyle; }
			set { _dashStyle = value; _pen.DashStyle = value; } }
			
		/// <summary>
		/// Retourne le pen à utiliser pour le dessin. Lecture seule.
		/// </summary>
		public Pen Pen { get { return _pen; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPenObject(string name) : base(name)
		{
			_width = 1; _dashStyle = DashStyle.Solid;
			_pen = new Pen(_defColor, _width);
			this.ColorChanged += delegate { _pen.Color = (this.Selected ? _selColor : Color); };
			this.SelectionChanged += delegate { _pen.Color = (this.Selected ? _selColor : Color); };
		}
		
	}


	// ===========================================================================
	
	
	/// <summary>
	/// Classe de base pour les objets de l'espace qui sont dessinés avec un SolidBrush.
	/// </summary>
	public abstract class SpBrushObject : SpObject
	{
	
		protected Brush _brush;

		/// <summary>
		/// Retourne le Brush à utiliser pour le dessin. Lecture seule.
		/// </summary>
		public Brush Brush { get { return _brush; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpBrushObject(string name) : base(name)
		{
			_brush = new SolidBrush(_defColor);
			this.ColorChanged += delegate { _brush = new SolidBrush(this.Selected ? _selColor : Color); };
			this.SelectionChanged += delegate { _brush = new SolidBrush(this.Selected ? _selColor : Color); };
		}
		
	}


	// ===========================================================================
	
	
	/// <summary>
	/// Classe de base pour les objets de l'espace qui sont dessinés avec un Pen, avec possibilité d'avoir un fond dessiné avec un Brush.
	/// </summary>
	public abstract class SpPenBrushObject : SpObject, IPenObject, IBrushObject
	{

		// Variables:
		protected Brush _brush;
		private BrushStyle _brushStyle;
		private HatchStyle _hatchStyle;
		private Color _hatchColor;
		private Color _backColor;
		protected Pen _pen;
		private float _width;
		private DashStyle _dashStyle;
	
		/// <summary>
		/// Epaisseur du trait pour le Pen.
		/// </summary>
		public float PenWidth {
			get { return _width; }
			set { _width = value; _pen.Width = value; } }
		
		/// <summary>
		/// Style du trait pour le Pen.
		/// </summary>
		public DashStyle DashStyle {
			get { return _dashStyle; }
			set { _dashStyle = value; _pen.DashStyle = value; } }
			
		/// <summary>
		/// Retourne le pen à utiliser pour le dessin. Lecture seule.
		/// </summary>
		public Pen Pen { get { return _pen; } }
		
		/// <summary>
		/// Type de Brush.
		/// </summary>
		public BrushStyle BrushStyle {
			get { return _brushStyle; }
			set { _brushStyle = value;  this.CreateBrush(); } }
		
		/// <summary>
		/// Type de hachures.
		/// </summary>
		public HatchStyle HatchStyle {
			get { return _hatchStyle; }
			set { _hatchStyle = value;  this.CreateBrush(); } }
		
		/// <summary>
		/// Couleur du Brush.
		/// </summary>
		public virtual Color BackColor {
			get { return _backColor; }
			set { _backColor = value; this.CreateBrush(); } }			
		
		/// <summary>
		/// Couleur de fond pour les hachures.
		/// </summary>
		public Color HatchColor {
			get { return _hatchColor; }
			set { _hatchColor = value; this.CreateBrush(); } }
			
		/// <summary>
		/// Retourne le Brush à utiliser pour le dessin. Lecture seule.
		/// </summary>
		public Brush Brush { get { return _brush; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpPenBrushObject(string name) : base(name)
		{
			// Pen:
			_width = 1; _dashStyle = DashStyle.Solid;
			_pen = new Pen(_defColor, 1F);
			this.ColorChanged += delegate { _pen.Color = (this.Selected ? _selColor : Color); };
			this.SelectionChanged += delegate { _pen.Color = (this.Selected ? _selColor : Color); };
			// Bruch:
			_brushStyle = BrushStyle.Solid; _hatchStyle = HatchStyle.Horizontal; _backColor = _defBackColor;
			this.CreateBrush();
			this.ColorChanged += delegate { this.CreateBrush(); };
			this.SelectionChanged += delegate { this.CreateBrush(); };
		}
		
		/// <summary>
		/// Créer un nouveau brush.
		/// </summary>
		private void CreateBrush()
		{
			if (_brushStyle == BrushStyle.Solid) { _brush = new SolidBrush(this.Selected ? _selColor : _backColor); }
			else { _brush = new HatchBrush(_hatchStyle,
								(this.Selected ? SpObject._selColor : _backColor), _hatchColor); }
		}
		
	}


	// ===========================================================================
	
	
	/// <summary>
	/// Classe de base pour les objets de l'espace qui sont dessinés avec un Brush avec la possibilité d'avoir un contour dessiné avec un Pen.
	/// </summary>
	public abstract class SpBrushPenObject : SpObject, IPenObject, IBrushObject
	{
	
		// Variables:
		protected Brush _brush;
		private BrushStyle _brushStyle;
		private HatchStyle _hatchStyle;
		private Color _hatchColor;
		private Color _edgeColor;
		protected Pen _pen;
		private float _width;
		private DashStyle _dashStyle;
	
		/// <summary>
		/// Epaisseur du trait pour le Pen.
		/// </summary>
		public float PenWidth {
			get { return _width; }
			set { _width = value; _pen.Width = value; } }
		
		/// <summary>
		/// Style du trait pour le Pen.
		/// </summary>
		public DashStyle DashStyle {
			get { return _dashStyle; }
			set { _dashStyle = value; _pen.DashStyle = value; } }
			
		/// <summary>
		/// Couleur du Pen.
		/// </summary>
		public Color EdgeColor {
			get { return _edgeColor; }
			set { _edgeColor = value; _pen.Color = _edgeColor; } }			
		
		/// <summary>
		/// Retourne le pen à utiliser pour le dessin. Lecture seule.
		/// </summary>
		public Pen Pen { get { return _pen; } }
		
		/// <summary>
		/// Type de Brush.
		/// </summary>
		public BrushStyle BrushStyle {
			get { return _brushStyle; }
			set { _brushStyle = value;  this.CreateBrush(); } }
		
		/// <summary>
		/// Type de hachures.
		/// </summary>
		public HatchStyle HatchStyle {
			get { return _hatchStyle; }
			set { _hatchStyle = value;  this.CreateBrush(); } }
		
		/// <summary>
		/// Couleur de fond pour les hachures.
		/// </summary>
		public Color HatchColor {
			get { return _hatchColor; }
			set { _hatchColor = value; this.CreateBrush(); } }
			
		/// <summary>
		/// Retourne le Brush à utiliser pour le dessin. Lecture seule.
		/// </summary>
		public Brush Brush { get { return _brush; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpBrushPenObject(string name) : base(name)
		{
			// Pen:
			_width = 1; _dashStyle = DashStyle.Solid; _edgeColor = _defEdgeColor;
			_pen = new Pen(_edgeColor, _width);
			this.ColorChanged += delegate { _pen.Color = (this.Selected ? _selColor : _edgeColor); };
			this.SelectionChanged += delegate { _pen.Color = (this.Selected ? _selColor : _edgeColor); };
			// Bruch:
			_brushStyle = BrushStyle.Solid; _hatchStyle = HatchStyle.Horizontal; _hatchColor = Color.White;
			this.ColorChanged += delegate { this.CreateBrush(); };
			this.SelectionChanged += delegate { this.CreateBrush(); };
			Color = _defFillColor;
		}
		
		/// <summary>
		/// Créer un nouveau brush.
		/// </summary>
		private void CreateBrush()
		{
			if (_brushStyle == BrushStyle.Solid) { _brush = new SolidBrush(this.Selected ? _selColor : Color); }
			else { _brush = new HatchBrush(_hatchStyle,
								(this.Selected ? _selColor : Color), _hatchColor); }
		}
		
	}


	#endregion OBJECTS DE BASE: SPOBJECT, SPPENOBJECT ET SPBRUSHOBJECT



	// ---------------------------------------------------------------------------
	// INTERFACES
	// ---------------------------------------------------------------------------




	#region INTERFACES


	public interface IPenObject
	{
		float PenWidth { get; set; }
		DashStyle DashStyle { get; set; }
	}
	
	public interface IBrushObject
	{
		BrushStyle BrushStyle { get; set; }
		HatchStyle HatchStyle { get; set; }
		Color HatchColor { get; set; }
	}
	
	public interface ITransformedObject
	{
		SpTransformationObject[] Transformations { get; }
		SpObject BaseObject { get; }
		string GetInfos(params object[] lines);
	}
	

	#endregion INTERFACES
	
	
}
