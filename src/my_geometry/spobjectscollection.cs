using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Linq;

namespace My
{



	/// <summary>
	/// Fournit une collection d'objets de l'espace, et les méthodes appropriées pour la gérée. Il ne peut y avoir qu'une seule instance.
	/// </summary>
	[Serializable()]
	public partial class SpObjectsCollection : IEnumerator
	{






		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS
		
		
		// Une seule instance:
		private static SpObjectsCollection _instance;
		// Tableaux des objets:
		private SpObject[] _allObjects;
		private SpObject[] _constructOrder;
		// Index pour l'énumérateur:
		private int _enumeratorIndex;
		// Contrôles:
		private My.ExdListView _listDisplayOrder;
		private My.MyFormMessage _dialogDisplayOrder;
		private DialogBoxObjectsProperties _dialogProps;
		// Nom de cet assemblage:
		private string _assemblyName;
		// Nombre d'objets sélectionnés:
		private int _selectedLength;
		// Tableau de tous les noms de la collection, le tableau est trié et sert à la recherche rapide d'objets:
		private string[] _objSortedNames;
		private int[] _objSortedIndexes;


		/// <summary>
		/// Se déclenche lorsqu'une propriété ou un élément a été modifiée de telle sorte que l'aspect du dessin peut changer. Autrement dit, demande un nouveau dessin du graphique sans besoin de recalcul des coordonnées 2D (celles ayant été normalement calculées avec les événements ObjectChanged pour chaque objet.
		/// </summary>
		public event EventHandler RequestDrawing;
		
		/// <summary>
		/// Délégué d'événement.
		/// </summary>
		internal delegate void RequestDrawingCalcEventHandler(object sender, RequestDrawingCalcEventArgs e);

		/// <summary>
		/// Se déclenche lorsqu'une propriété ou un élément a été modifiée de telle sorte que le calcul des coordonnées 2D sur le plan du form peut changer. Autrement dit, demande un nouveau calcul des coordonnées 2D dans le repère du form, mais pas un nouveau dessin (qui es demandé par DisplayChanged).
		/// </summary>
		internal event RequestDrawingCalcEventHandler RequestDrawingCalc;



		#endregion DECLARATIONS










		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES
		
		
		
		/// <summary>
		/// Obtient le nombre d'éléments dans la collection.
		/// </summary>
		public int Count { get { return _allObjects.Length; } }


		/// <summary>
		/// C'est le tableau à utiliser pour construire la figure. En effet, si l'utilisateur mélange l'ordre des objets (changement d'ordre d'affichage, ou remplacement d'objets, eg.), il est possible, si l'on suit l'ordre "normal" (celui de l'énumation), d'avoir un objet qui arrive avant que ces maîtres ne soient définis. Ca ne marche donc pas pour recréer la figure géométrique. Le tableau d'ordre de construction s'assure que les objets n'apparaîssent pas avant leur maîtres, mais bien après.
		/// </summary>
		public SpObject[] ConstructList { get { return _constructOrder; } }



		#endregion PROPRIETES









		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS
		
		
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		private SpObjectsCollection()
		{
			// Initialisation:
			_allObjects = new SpObject[0];
			_constructOrder = new SpObject[0];
			_objSortedIndexes = new int[0];
			_objSortedNames = new string[0];
			_constructOrder = new SpObject[0];
			_assemblyName = Assembly.GetExecutingAssembly().FullName;
			_enumeratorIndex = -1;
			// Note: Ne pas mettre _dialogProps ici, car son constructeur à besoin de SpObjectsCollection... et on tourne en rond.
			// S'inscrit aux événements statiques et aux délégués de SpObject:
			SpObject.GetFreeName = GetFreeName;
			SpObject.NameChanged += new SpObject.NameChangedEventHandler(SpObject_NameChanged);
			SpObject.ObjectExtracted += new SpObject.ObjectExtractedEventHandler(SpObject_ObjectExtracted);
			SpObject.ObjectChanged += new SpObject.ObjectChangedEventHandler(SpObject_ObjectChanged);
		}

		/// <summary>
		/// Retourne l'instance unique.
		/// </summary>
		public static SpObjectsCollection GetInstance()
		{
			if (_instance == null) { _instance = new SpObjectsCollection(); }
			return _instance;
		}


		#endregion CONSTRUCTEURS







		// ---------------------------------------------------------------------------
		// INDEXEURS
		// ---------------------------------------------------------------------------




		#region INDEXEURS




		/// <summary>
		/// Retourne un objet à partir du nom, ou null si n'existe pas. Les erreurs sont affichées.
		/// </summary>
		public SpObject this[string name]
		{
			get { return this.GetObject(name, true); }
		}

		/// <summary>
		/// Retourne un objet à partir du nom. Si throwExc vaut true, lève une exception si l'objet n'est pas trouvé. Sinon, affiche un message d'erreur.
		/// </summary>
		public SpObject this[string name, bool throwExc]
		{
			get { return this.GetObject(name, !throwExc, throwExc); }
		}

		/// <summary>
		/// Retourne un objet à partir du nom et du type, ou null si n'existe pas ou n'est pas du bon type. Les erreurs sont affichées.
		/// </summary>
		public SpObject this[string name, Type objType]
		{
			get { return this.GetObject(name, objType, true); }
		}

		/// <summary>
		/// Retourne un objet à partir du nom et du type, ou null si n'existe pas ou n'est pas du bon type. Les erreurs ne sont pas affiché, mais une exception est levée si l'objet n'est pas trouvé.
		/// </summary>
		public SpObject this[string name, Type objType, bool exactMatch]
		{
			get { return this.GetObject(name, objType, exactMatch, false, true); }
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne un objet en fonction de son index.
		/// </summary>
		public SpObject this[int index]
		{
			get
			{
				if (index < 0 || index > _allObjects.Length) { throw new IndexOutOfRangeException(); }
				return _allObjects[index];
			}
		}
	

		#endregion INDEXEURS
	







		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES DE GESTION DE LA COLLECTION
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES DE GESTION DE LA COLLECTION
		
		

		/// <summary>
		/// Obtient le numéro de l'index de l'objet name, ou -1 si n'existe pas.
		/// </summary>
		protected int GetIndex(string name, bool showError)
		{
			int l = _allObjects.Length;
			for (int i=0; i<l; i++) { if (_allObjects[i].Name.Equals(name)) { return i; } }
			if (showError) { SendError(String.Format("{0} doesn't exists.", name)); }
			return -1;
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Obtient un nom libre pour un objet, i.e. name si libre, ou name_1/2/3/... si name n'est pas libre.
		/// </summary>
		protected string GetFreeName(string name)
		{
			int l = name.Length, c = -1;
			for (int i=l-1; i>=0; i--) { if (!"0123456789".Contains(name.Substring(i, 1))) { c = i; break; } }
			string test = name;
			while (this.GetIndex(test, false) != -1)
			{
				if (c+1 == test.Length) { test += "_1"; c++; }
				else { test = test.Substring(0, c+1) + (Int32.Parse(test.Substring(c+1))+1).ToString(); }
			}
			if (!test.Equals(name)) { SendError(String.Format("{0} already exists. {0} renamed to {1}.", name, test)); }
			return test;
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne un objet correspondant au nom demandé, ou null si l'objet n'existe pas. Ne lève pas d'exception.
		/// </summary>
		public SpObject GetObject(string name, bool showError)
		{
			return GetObject(name, showError, false);
		}
		
		/// <summary>
		/// Retourne un objet correspondant au nom demandé, ou affiche (ou non) et/ou lève une exception (ou non) si l'objet n'existe pas.
		/// </summary>
		public SpObject GetObject(string name, bool showError, bool throwExc)
		{
			int index;
			if ((index = Array.BinarySearch(_objSortedNames, name)) >= 0) { return _allObjects[_objSortedIndexes[index]]; }
			if (showError) { SendError(String.Format("Object {0} doesn't exists.", name)); }
			if (throwExc) { throw new SpObjectNotFoundException(name); }
			return null;
		}
		
		/// <summary>
		/// Retourne un objet correspondant non seulement au nom demandé, mais aussi au type demandé, ou null si l'objet n'existe pas ou s'il n'est pas du bon type. exactMatch indique si le type doit exactement correspondre, ou si l'objet name peut-être d'un sous-type de objType (par exemple, un MidPoint est un objet de sous-type de Point). Si throwExc est vrai, une SpObjectNotFoundException est levée si l'objet n'est pas trouvé.
		/// </summary>
		public SpObject GetObject(string name, Type objType, bool exactMatch, bool showError, bool throwExc)
		{
			int index; Type type;
			if ((index = Array.BinarySearch(_objSortedNames, name)) >= 0) {
				type = _allObjects[_objSortedIndexes[index]].GetType();
				if (type == objType || (!exactMatch && type.IsSubclassOf(objType))) { return _allObjects[_objSortedIndexes[index]]; } }
			if (showError) { SendError(String.Format("{0} doesn't exists or is not correct.", name)); }
			if (throwExc) { throw new SpObjectNotFoundException(name); }
			return null;
		}

		/// <summary>
		/// Voir surcharge. exactMatch et throwExc valent ici false.
		/// </summary>
		public SpObject GetObject(string name, Type objType, bool showError)
			{ return this.GetObject(name, objType, false, showError); }		

		/// <summary>
		/// Voir surcharge. throwExc vaut ici false.
		/// </summary>
		public SpObject GetObject(string name, Type objType, bool exactMatch, bool showError)
			{ return GetObject(name, objType, exactMatch, showError, false); }
			
		/// <summary>
		/// Retourne un tableau d'objet correspondant aux noms passés en argument. Si un nom ne correspond à rien, les autres objets sont quand même retourné, ce qui signifie que les tailles du tableau d'entrée et du tableau de sortie ne sont pas forcément les mêmes.
		/// </summary>
		public SpObject[] GetObjets(string[] names, bool showErrors)
		{
			SpObject[] result = new SpObject[names.Length]; SpObject o; int c = 0;
			foreach (string s in names)
				{ if ((o = this.GetObject(s, showErrors)) == null) { continue; } else { result[c++] = o; } }
			Array.Resize(ref result, c);
			return result;
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Affiche une boîte de dialogue contenant une ExdListView dont on peut ordonner les items, puis reforme le tableau _displayOrder.
		/// </summary>
		public void ChangeDisplayOrder()
		{
			// Créer un ExdListWiew:
			if (_listDisplayOrder == null)
			{
				_listDisplayOrder = new ExdListView();
				_listDisplayOrder.AllowDelete = false;
				_listDisplayOrder.AllowColumnReorder = false;
				_listDisplayOrder.AllowSortByColumnClick = true;
				_listDisplayOrder.Columns.Add("Name");
				_listDisplayOrder.Columns.Add("Description");
				_listDisplayOrder.Columns.Add("Order");
				_listDisplayOrder.Font = DrawingArea.DefaultListFont;
			}
			// Créer la boîte de dialogue:
			if (_dialogDisplayOrder == null)
			{
				_dialogDisplayOrder = new MyFormMessage();
				_dialogDisplayOrder.Width = Screen.PrimaryScreen.WorkingArea.Width / 2;
				_dialogDisplayOrder.Height = Screen.PrimaryScreen.WorkingArea.Height / 2;
				_dialogDisplayOrder.SetDialogMessage("Order the objects:");
				_dialogDisplayOrder.SetDialogIcon(DialogBoxIcon.Search);
				_dialogDisplayOrder.SetControl(_listDisplayOrder);
				_dialogDisplayOrder.AddButtonsCollection(DialogBoxButtons.OKCancel, 1, true);
			}
			// Remplit la liste:
			ListViewItem item; int l = _allObjects.Length;
			_listDisplayOrder.Items.Clear();
			for (int i=0; i<l; i++)
			{
				item = new ListViewItem(new string[]{_allObjects[i].Name,_allObjects[i].ToString(),i.ToString()});
				item.Tag = i;
				item.ForeColor =_allObjects[i].Color;
				_listDisplayOrder.Items.Add(item);
			}
			_listDisplayOrder.AutoResizeColumns();
			// Affiche la boîte:
			if (_dialogDisplayOrder.ShowDialog() == DialogBoxClickResult.OK)
			{
				// Tri en fonction des réponses:
				SpObject[] newObjects = new SpObject[l];
				for (int i=0; i<l; i++) {
					newObjects[i] = _allObjects[(int)_listDisplayOrder.Items[i].Tag];
					_objSortedNames[i] = newObjects[i].Name;
					_objSortedIndexes[i] = i; }
				_allObjects = newObjects;
				newObjects = null;
				Array.Sort(_objSortedNames, _objSortedIndexes);
				this.OnRequestDrawing(Assembly.GetCallingAssembly());
				// Refait le tableau de l'ordre de construction:
				MakeConstructList();
				SendInfos(String.Format("Reorder done. {0} objects in collection.", _allObjects.Length));
			}
		}

		/// <summary>
		/// Change l'ordre d'affichage dans l'ordre d'apparition des objets dans le tableau. Tous les objets de la collection, et seulement ceux de la collection doivent être dans le tableau.
		/// </summary>
		public void ChangeDisplayOrder(params SpObject[] objs)
		{
			objs = objs.Distinct().ToArray();
			if (objs.Length != _allObjects.Length)
				{ SendError("Length of parameter array mismatch length of collection. Enable to reorder."); return; }
			foreach (SpObject o in objs)
				{ if (o.IsVirtual || (o.IsSystem && !o.IsExtracted)) { SendError(String.Format("{0} is virtual or system. Enable to reorder", o.Name)); return; } }
			_allObjects = objs;
			// Refait le tableau de l'ordre de construction:
			MakeConstructList();
			// Refait le tableau de tri alphabétique des noms:
			_objSortedNames = (string[])Array.ConvertAll<SpObject,string>(_allObjects, delegate(SpObject o) { return o.Name; });
			for (int i=0; i<_allObjects.Length; i++) { _objSortedIndexes[i] = i; }
			Array.Sort(_objSortedNames, _objSortedIndexes);
			SendInfos(String.Format("Reorder done. {0} objects in collection.", _allObjects.Length));
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne un tableau contenant les noms de tous les objets, dans l'ordre d'affichage. Cette méthode est plus rapide que les appels et conversion par énumération.
		/// </summary>
		public string[] GetDisplayOrderNames()
		{
			// A partir des index et des noms par ordre alphabétique, obtient rapidement, par le tri des index
			// et non plus des noms, l'ordre voulu:
			int[] indexes = (int[])_objSortedIndexes.Clone();
			string[] names = (string[])_objSortedNames.Clone();
			Array.Sort(indexes, names);
			return names;
		}
		
	
		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Supprime les objets.
		/// </summary>
		public void DeleteObjects(params SpObject[] objs)
		{
		
			// Sort si rien:
			objs = objs.Distinct().ToArray();
			if (objs.Length == 0) { SendInfos("No object to delete."); return; }
		
			// Méthode anonyme qui détermine si l'objet doit être extrait du tableau ou non... En effet, certains
			// objects extracted ont été réintégré: ils ne doivent donc plus être dans la collection, mais ils ne sont
			// quand même pas marqué Deleted ! Il faut donc voir à la fois la propriété Deleted (pour les objets normaux)
			// et la propriété IsSystem à true et la propriété IsExtracted à false, car un objet système ne peut pas se
			// trouver dans la collection, sauf s'il est à la fois système et extracted:
			Func<SpObject,bool> mustBeRemoved =
				delegate(SpObject obj) { return (obj.Deleted || (obj.IsSystem && !obj.IsExtracted)); };

			// Marque les objets nommés comme supprimé, et compte le nombre d'objets restant:
			int oldLength = _allObjects.Length; int newLength = 0;
			foreach (SpObject o in objs) { o.Deleted = true; }
			foreach (SpObject o in _allObjects) { if (!mustBeRemoved(o)) { newLength++; } }
			
			// Créer un nouveau tableau _objNamesOrder:
			/* On ne peut pas se baser sur la propriété Deleted pour refaire le tableau des noms, car certains
			 * certains objets extracted ont été réintégré*/
			int[] newIndexes = new int[oldLength]; int c = 0;
			for (int i=0; i<oldLength; i++) { newIndexes[i] = (mustBeRemoved(_allObjects[i]) ? -1 : c++); }
			int[] newNamesOrder = new int[newLength]; string[] newNames = new string[newLength]; c = 0;
			for (int i=0; i<oldLength; i++)
			{
				if (newIndexes[_objSortedIndexes[i]] != -1) {
					newNamesOrder[c] = newIndexes[_objSortedIndexes[i]];
					newNames[c] = _allObjects[_objSortedIndexes[i]].Name;
					c++; }
			}
			_objSortedIndexes = newNamesOrder; _objSortedNames = newNames;
			
			// Supprime réellement les objets marqués:
			for (int i=0; i<oldLength; i++) {
				if (!mustBeRemoved(_allObjects[i])) { continue; }
				_allObjects[i].RequestDrawing -= SpObject_RequestDrawing;
				_allObjects[i].RequestDrawingCalc -= SpObject_RequestDrawingCalc;
				_allObjects[i] = null; }
			_allObjects = _allObjects.Where(delegate(SpObject o) { return (o != null); }).ToArray();
			
			// Recompte les objets sélectionnés:
			_selectedLength = 0;
			foreach (SpObject o in _allObjects) { if (o.Selected) { _selectedLength++; } }

			// Force un GC (mais ça ne marche pas vraiment quand c'est dans la même procédure):
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			GC.WaitForPendingFinalizers();

			// Refait le tableau de l'ordre de construction:
			MakeConstructList();
			
			// Envoie un message:
			SendInfos(String.Format("{0} object(s) marked as deleted, {1} object(s) selected", oldLength - newLength, _selectedLength));
			this.OnRequestDrawing(Assembly.GetCallingAssembly());
			
		}		
		
		/// <summary>
		/// Supprime les objets sélectionnés.
		/// </summary>
		public void DeleteSelected()
		{
			DeleteObjects(GetSelected());
			this.OnRequestDrawing(Assembly.GetCallingAssembly());
		}
		
		/// <summary>
		/// Supprime tous les objets.
		/// </summary>
		public void DeleteAll()
		{
			DeleteObjects(_allObjects);
			this.OnRequestDrawing(Assembly.GetCallingAssembly());
		}
		

		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Ajoute un objet aux deux tableaux (_allObjects et _displayOrder), et lie les événements NumericDataChanged, DisplayChanged, Infos et Error à ceux de la collection. Lance un calcul de l'objet et un dessin. Retourne l'objet ajouté. Si le nom de l'objet est déjà utilisé dans la collection, l'objet est renommé automatiquement (un nombre est ajouté à la suite du nom actuel).
		/// </summary>
		public SpObject Add(SpObject obj)
		{
			// Ajoute l'objet aux tableaux:
			int l = _allObjects.Length;
			Array.Resize(ref _allObjects, l + 1);	_allObjects[l] = obj;
			Array.Resize(ref _objSortedNames, l + 1);	_objSortedNames[l] = obj.Name;
			Array.Resize(ref _objSortedIndexes, l + 1);	_objSortedIndexes[l] = l;
			Array.Sort(_objSortedNames, _objSortedIndexes);
			// Ajoute l'élément au tableau d'ordre de construction:
			Array.Resize(ref _constructOrder, l + 1); _constructOrder[l] = obj;
			// Lie les événements de l'objet à ceux de la collection:
			obj.RequestDrawingCalc += new My.RequestDrawingCalcEventHandler(SpObject_RequestDrawingCalc);
			obj.RequestDrawing += new EventHandler(SpObject_RequestDrawing);
			// Lance un calcul de l'objet et un dessin:
			this.OnRequestDrawingCalc(obj);
			this.OnRequestDrawing(Assembly.GetCallingAssembly());
			return obj;
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Change le nom de l'objet, en vérifiant que le nom n'est pas déjà utilisé. Si c'est le cas, le nom est remplacé par un nom libre.
		/// </summary>
		public void RenameObject(SpObject obj, string newName)
			{ obj.Name = newName; }


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne les noms de l'ensemble des objets, dans l'ordre de construction.
		/// </summary>
		public string[] GetAllNames()
			{ return _objSortedNames; }
		

		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne les objets indéfinis.
		/// </summary>
		public SpObject[] GetUndefinedObjects()
			{ return _allObjects.Where(delegate(SpObject o) { return o.IsUndefined; }).ToArray(); }
		
		/// <summary>
		/// Retourne le nom des objets indéfinis.
		/// </summary>
		public string[] GetUndefinedObjectNames()
			{ return Array.ConvertAll<SpObject,string>(GetUndefinedObjects(), delegate(SpObject o) { return o.Name; }).ToArray(); }


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Tente de remplacer oldObj par newObj. Les deux objets doivent être compatibles, y compris pour les objets dont il sont des maîtres.
		/// </summary>
		public void ReplaceObject(SpObject oldObj, SpObject newObj)
		{
		
			// Sort si les objets ne sont pas explicites:
			if (oldObj.IsSystem || oldObj.IsVirtual || newObj.IsSystem || newObj.IsVirtual)
				{ SendError(String.Format("{0} and/or {1} are not explicit. Enable to replace.", oldObj.Name, newObj.Name)); return; }
			
			// Méthode anonyme: Détermine si param est d'un type correspondant à newObj:
			Func<object,bool> testTypes = delegate(object param)
				{ return (newObj.GetType() == param.GetType() || newObj.GetType().IsSubclassOf(param.GetType())); };
			
			// Cherche tous les objets qui ont oldObj dans leur constructeur, et sort si rien à remplacer,
			// ou si les types ne correspondent pas.
			int len = _allObjects.Length, c = 0; int[] indexes = new int[len]; MethodInfo[] meths = new MethodInfo[len];
			object[][] ctorsObjs = new object[len][]; MethodInfo meth; object[] ctorObjs;
			bool changed = false, errorOccurred = false;
			
			// Méthode anonyme qui examine l'objet param, et y remplace éventuellement oldObj par newObj. Modifie les valeurs de changed et errorOccurred:
			Func<object,object> testReplacement = delegate(object param)
				{
					// Sort de suite si on a déjà détecter une erreur:
					if (errorOccurred) { return param; }
					// Si WeightedPoint:
					if (param is WeightedPoint)
					{
						if (((WeightedPoint)param).Point == oldObj && newObj is SpPointObject)
							{ changed = true; return new WeightedPoint((SpPointObject)newObj, ((WeightedPoint)param).Weight); }
						else if (((WeightedPoint)param).Point == oldObj) { changed = true; errorOccurred = true; }
					}
					// Sinon:
					else if (param == oldObj) { changed = true; return newObj;  }
					return param;
				};
			
			// Parcourt tous les objets, récupère les CtorObjs et analyse chacun des CtorObjs pour y remplacer éventuellement oldObj par newObj.
			// Puis, avec ce nouveau tableau d'objet tente de récupérer la méthode Alter correspondante, et sort avec erreur si la méthode n'est pas
			// disponible à cause d'une incompatibilité de type. Mais il faut aussi vérifier si l'objet ne se trouve pas dans les AllMastersObjects,
			// car s'il est dans une formule (d'un DoubleF ou d'une formule de construction), il apparaît automatiquement dans les 
			// AllMasterObjects (mais pas forcément dans les CtorObjects). Il faut alors aussi relancer la reconstruction de l'objet, puisqu'on
			// déjà auparavant modifier la formule.
			for (int i=0; i<len; i++)
			{
				// Récupère les CtorObjs:
				ctorObjs = _allObjects[i].GetCtorObjects();
				// Remplace oldObj par newObj, puis regarde dans les AllMastersObjects et sort si pas de remplacement:
				changed = false; errorOccurred = false;
				ctorObjs = (object[])My.ArrayFunctions.RecursiveArrayBrowsing(ctorObjs, testReplacement);
				if (_allObjects[i].AllMastersObjects.Contains(oldObj)) { changed = true; }
				if (!changed) { continue; }
				// Tente de récupérer la méthode Alter correspondante, et si ça ne marche pas, erreur et sort:
				meth = _allObjects[i].GetType().GetMethod("Alter", Type.GetTypeArray(ctorObjs));
				if (meth == null || errorOccurred)
					{ SendError(String.Format("{0} ({1}) is not compatible with parameters of {2}. Enable to replace.",
						newObj.Name, newObj.TypeDescription, _allObjects[i].Name)); return; }
				// Inscrit les nouveaux éléments dans le tableau:
				ctorsObjs[c] = ctorObjs; meths[c] = meth; indexes[c] = i; c++;
			}
			if (c == 0) { SendError(String.Format("Nothing to replace.")); return; }
			Array.Resize(ref ctorsObjs, c); Array.Resize(ref meths, c); Array.Resize(ref indexes, c);

			// Change les noms dans les formules, mais cela change aussi le nom dans _objNames: on modifie donc ce tableau manuellement:
			int savedIndex = Array.IndexOf(_allObjects, oldObj);
			SpObject.ChangeName(oldObj.Name, newObj.Name);
			_objSortedNames[Array.IndexOf(_objSortedIndexes, savedIndex)] = oldObj.Name;
			Array.Sort(_objSortedNames, _objSortedIndexes);
			
			// Exécuter un Alter sur les objets ne suffit pas, car si l'objet a pour owned un objet virtuel dont la formule
			// a été changée par le remplacement d'objet, celui-ci ne sera pas recalculer. Le meilleur moyen, donc, est de reprendre
			// à chaque fois la formule, au besoin, c'est-à-dire de recréer les owned par leur formule, sans avoir besoin de 
			// chercher les owned sur plusieurs niveaux, puisque, console oblige, seuls les owned de premier niveau ont
			// une formule, et les sous-owned sont créés par la formule de l'owned de premier niveau:
			SpObject obj; bool creation;
			// Pour faire tout cela, on utilise à nouveau la méthode récursive qui agit sur chaque objet, avec un nouveau délégué
			// (testObjectForRebuildingVirt) qui appelle le délégué rebuildVirt pour l'analyse spécifique des SpObjects:
			Func<SpObject,SpObject> rebuildVirt = delegate(SpObject o)
				{
					if (!o.IsDefinedByFormula) { return o; }
					if (GeoMethodsForFormulas.GetObjectFromFormula(o.CtorFormula, typeof(SpObject), 0, out obj, out creation))
						{ return obj; }
					else
						{ SendError(String.Format("Warning! Enable to recreate a virtual object by {0}! That sounds very bad!", o.CtorFormula));
						return o; }
				};
			Func<object,object> testObjectForRebuildingVirt = delegate(object o)
				{
					// Si WeightedPoint:
					if (o is WeightedPoint)
						{ return new WeightedPoint((SpPointObject)rebuildVirt(((WeightedPoint)o).Point), ((WeightedPoint)o).Weight); }
					// Si SpObject:
					else if (o is SpObject) { return rebuildVirt((SpObject)o); }
					return o;
				};
			// Pour tous les objets à remplacer:
			for (int i=0; i<c; i++) {
				ctorsObjs[i] = (object[])My.ArrayFunctions.RecursiveArrayBrowsing(ctorsObjs[i], testObjectForRebuildingVirt);
				meths[i].Invoke(_allObjects[indexes[i]], ctorsObjs[i]); }

			// Refait le tableau de l'ordre de construction:
			MakeConstructList();
			
			// Affiche les messages:
			SendInfos(String.Format("{0} objects altered. {1} replaced by {2}.", c, oldObj.Name, newObj.Name));
			for (int i=0; i<len; i++) {
				if (_allObjects[i].HasCircularRef)
					{ SendError(String.Format("Warning! {0} has now a circular reference!", _allObjects[i].Name)); } }
			
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Reconstruit le tableau _constructOrder.
		/// </summary>
		private void MakeConstructList()
		{
			// Obtient une copie du tableau:
			int len = _allObjects.Length, c = 0, start = -1; bool ok, restart = true;
			SpObject[] allObjs = (SpObject[])_allObjects.Clone();
			_constructOrder = new SpObject[len];
			// Parcours tous les objets. Le principe est simple: Pour chaque objet, dans l'ordre d'apparition, de allObjs, on regarde
			// s'il les maîtres explicites (les virtuels et systèmes, on s'en fiche) sont déjà dans le tableau result. -- Si oui, on
			// inscrit l'objet à la suite de ceux déjà inscrit dans result, puis on on efface l'objet de allObjs (mise à null), puis
			// on redéfinit i (le compteur) à start pour examiner depuis le début les objets qui n'ont pas encore été inscrit, et
			// et peut-être avoir la chance de pouvoir les inscrire maintenant qu'un nouvel objet est apparu dans result. -- Si non,
			// on passe à la suite. Le compteur start n'est là que pour augmenter la rapidité, et éviter que i ne recommence chaque
			// fois à 0.
			// Mais il faut aussi tenir compte des Extracted. On ne les inscrit qu'une fois que leur propriétaire est dans la liste.
			for (int i=0; i<len; i++)
			{
				if (allObjs[i] == null) { if (restart) { start = i-1; } continue; }
				restart = false;
				if (allObjs[i].IsExtracted && !_constructOrder.Contains(allObjs[i].Owner)) { continue; }
				ok = true;
				foreach (SpObject o in allObjs[i].AllMastersObjects)
					{ if ((!o.IsSystem || (o.IsSystem && o.IsExtracted)) && !o.IsVirtual && !_constructOrder.Contains(o)) { ok = false; break; } }
				if (ok) { _constructOrder[c++] = allObjs[i]; allObjs[i] = null; restart = true; i = start; }
			}
			allObjs = null;
			if (c != _constructOrder.Length)
				{ throw new Exception("There is a programmation issue in SpObjectsCollection.MakeConstructList."); }
		}
		
		
		
		
	

		#endregion METHODES PUBLIQUES DE GESTION DE LA COLLECTION









		// ---------------------------------------------------------------------------
		// METHODES DE CHANGEMENT DES PROPRIETES
		// ---------------------------------------------------------------------------




		#region METHODES DE CHANGEMENT DES PROPRIETES



		/// <summary>
		/// Exécute le délégué toDo pour chaque objet. Si objs est vide, alors utilise les objets sélectionnés.
		/// </summary>
		protected void ForEachSpObject(Func<SpObject,bool> toDo, params SpObject[] objs)
		{
			SpObject[] arr = (objs.Length==0 ? GetSelected() : objs); int c = 0;
			foreach (SpObject o in arr) { if (toDo(o)) c++; }
			SendInfos(String.Format("{0} object(s) modified.", c));
			this.OnRequestDrawing(null);
		}



		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Masque ou affiche les étiquettes des objets.
		/// </summary>
		public void ShowNames(bool value, params SpObject[] objs)
			{ ForEachSpObject(delegate(SpObject o) { o.ShowName = value; return true; }, objs); }

		/// <summary>
		/// Masque ou affiche les objets.
		/// </summary>
		public void Hide(bool value, params SpObject[] objs)
			{ ForEachSpObject(delegate(SpObject o) { o.Hidden = value; return true; }, objs); }

		/// <summary>
		/// Change la couleur des objets.
		/// </summary>
		public void ChangeColor(Color color, params SpObject[] objs)
			{ ForEachSpObject(delegate(SpObject o) { o.Color = color; return true; }, objs); }
		
		/// <summary>
		/// Change la police d'étiquette des objets.
		/// </summary>
		public void ChangeLabelFont(Font value, params SpObject[] objs)
			{ ForEachSpObject(delegate(SpObject o) { o.LabelFont = value; return true; }, objs); }

		/// <summary>
		/// Change l'épaisseur du trait des objets.
		/// </summary>
		public void ChangeWidth(float value, params SpObject[] objs)
		{
			ForEachSpObject(
				delegate(SpObject o) {
					if (o is IPenObject) { ((IPenObject)o).PenWidth = value;  return true; }
					return false; }, objs);
		}

		/// <summary>
		/// Change le style du trait des objets.
		/// </summary>
		public void ChangeDashStyle(DashStyle value, params SpObject[] objs)
		{
			ForEachSpObject(
				delegate(SpObject o) {
					if (o is IPenObject) { ((IPenObject)o).DashStyle = value;  return true; }
					return false; }, objs);
		}

		/// <summary>
		/// Change le style de brosse des objets.
		/// </summary>
		public void ChangeBrushStyle(BrushStyle value, params SpObject[] objs)
		{
			ForEachSpObject(
				delegate(SpObject o) {
					if (o is IBrushObject) { ((IBrushObject)o).BrushStyle = value; return true; }
					return false; }, objs);
		}

		/// <summary>
		/// Change le style des hachures des objets.
		/// </summary>
		public void ChangeHatchStyle(HatchStyle value, params SpObject[] objs)
		{
			ForEachSpObject(
				delegate(SpObject o) {
					if (o is IBrushObject) { ((IBrushObject)o).HatchStyle = value; return true; }
					return false; }, objs);
		}

		/// <summary>
		/// Change la couleur de fond des hachures des objets.
		/// </summary>
		public void ChangeHatchColor(Color value, params SpObject[] objs)
		{
			ForEachSpObject(
				delegate(SpObject o) {
					if (o is IBrushObject) { ((IBrushObject)o).HatchColor = value; return true; }
					return false; }, objs);
		}

		/// <summary>
		/// Change la couleur de contour des objets.
		/// </summary>
		public void ChangeEdgeColor(Color value, params SpObject[] objs)
		{
			ForEachSpObject(
				delegate(SpObject o) {
					if (o is SpBrushPenObject) { ((SpBrushPenObject)o).EdgeColor = value;  return true; }
					return false; }, objs);
		}

		/// <summary>
		/// Change la couleur de fond des objets.
		/// </summary>
		public void ChangeBackColor(Color value, params SpObject[] objs)
		{
			ForEachSpObject(
				delegate(SpObject o) {
					if (o is SpPenBrushObject) { ((SpPenBrushObject)o).BackColor = value;  return true; }
					return false; }, objs);
		}

		/// <summary>
		/// Change le style de point des objets.
		/// </summary>
		public void ChangePointShape(PointShape value, params SpObject[] objs)
			{ ForEachSpObject(delegate(SpObject o) { if (o is SpPoint) { ((SpPoint)o).PointShape = value; return true; } return false; }, objs); }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Affiche un dialogue pour changer d'un coup nombre de propriétés.
		/// </summary>
		public void ChangeProperties(bool onlySelected)
		{
			if (_dialogProps == null) { _dialogProps = new DialogBoxObjectsProperties(); }
			_dialogProps.ShowDialog(onlySelected);
			OnRequestDrawing(null);
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Sélectionne les objets indiqués dans names. Si un nom ne correspond à rien, sélectionne quand même les autres.
		/// </summary>
		public void SelectObjects(params SpObject[] objs)
		{
			_selectedLength = 0; bool val;
			foreach (SpObject o in _allObjects) { val = objs.Contains(o); o.Selected = val; _selectedLength += (val?1:0); }
			SendInfos(String.Format("{0} object(s) selected: {1}", _selectedLength, My.ArrayFunctions.Join(this.GetSelectedNames(), ",")));
			OnRequestDrawing(Assembly.GetCallingAssembly());
		}


		/// <summary>
		/// Ajoute les objets indiqués à la sélection. Si un nom ne ne correspond à rien, sélectionne quand même les autres.
		/// </summary>
		public void AppendToSelection(params SpObject[] objs)
		{
			foreach (SpObject o in objs) { o.Selected = true; _selectedLength++; }
			SendInfos(String.Format("{0} object(s) selected: {1}", _selectedLength, My.ArrayFunctions.Join(this.GetSelectedNames(), ",")));
			this.OnRequestDrawing(Assembly.GetCallingAssembly());
		}
		
		
		/// <summary>
		/// Enlève les éléments indiqué de la sélection en cours.
		/// </summary>
		public void RemoveFromSelection(params SpObject[] objs)
		{
			foreach (SpObject o in objs) { o.Selected = false; _selectedLength--; }
			SendInfos(String.Format("{0} object(s) selected: {1}", _selectedLength, My.ArrayFunctions.Join(this.GetSelectedNames(), ",")));
			this.OnRequestDrawing(Assembly.GetCallingAssembly());
		}
		
		
		/// <summary>
		/// Sélectionne tous les objets.
		/// </summary>
		public void SelectAll()
		{
			foreach (SpObject o in _allObjects) { o.Selected = true; }
			_selectedLength = _allObjects.Length;
			SendInfos(String.Format("{0} (all) object(s) selected: {1}", _selectedLength, My.ArrayFunctions.Join(this.GetSelectedNames(), ",")));
			this.OnRequestDrawing(Assembly.GetCallingAssembly());
		}
		
		
		/// <summary>
		/// Déselectionne tous les objets.
		/// </summary>
		public void DeselectAll()
		{
			foreach (SpObject o in _allObjects) { o.Selected = false; }
			_selectedLength = 0;
			SendInfos("0 object selected.");
			this.OnRequestDrawing(Assembly.GetCallingAssembly());
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Retourne le nom des objets sélectionnés.
		/// </summary>
		public string[] GetSelectedNames()
		{
			return Array.ConvertAll<SpObject,string>(GetSelected(), delegate(SpObject o) { return o.Name; }).ToArray();
		}

		/// <summary>
		/// Retourne les objets sélectionnés.
		/// </summary>
		public SpObject[] GetSelected()
		{
			return _allObjects.Where(delegate(SpObject o) { return o.Selected; }).ToArray();
		}



		#endregion METHODES DE CHANGEMENT DES PROPRIETES







		
		// ---------------------------------------------------------------------------
		// CREATION D'OBJETS
		// ---------------------------------------------------------------------------




		#region CREATION D'OBJETS



		
		/// <summary>
		/// Ajoute l'ensemble des segments entourant le polygone donné dans la collection, avec des noms par défaut.
		/// </summary>
		public void CreateSegmentsFromPolygon(SpPolygon polygon, string name)
		{
			// Sort si l'objet est indéfini ou si tous les points ne sont pas explicites ou extraits:
			if (polygon.IsUndefined) { SendError("Polygone is undefined"); return; }
			foreach (SpPointObject o in polygon.Vertices) {
				if (o.IsSystem && !o.IsExtracted) { SendError("All points are not explicit or extracted."); return; } }
			int l = polygon.Vertices.Length;
			if (l < 2) { SendError("Not enough vertices."); return; }
			for (int i=0; i<l-1; i++) { Add(new SpSegment(name, polygon.Vertices[i], polygon.Vertices[i+1])); }
			Add(new SpSegment(name, polygon.Vertices[l-1], polygon.Vertices[0]));
			OnRequestDrawing(Assembly.GetCallingAssembly());
		}
		
		
		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Ajoute l'ensemble des segments entourant le solide donné dans la collection, avec des noms par défaut.
		/// </summary>
		public void CreateSegmentsFromSolid(SpSolid solid, string name)
		{
			// Sort si l'objet est indéfini ou si tous les points ne sont pas explicites ou extraits:
			if (solid.IsUndefined) { SendError("Solid is undefined"); return; }
			foreach (SpPointObject o in solid.Vertices) {
				if (o.IsSystem && !o.IsExtracted) { SendError("All points are not explicit or extracted."); return; } }
			SpSegment[] segs = new SpSegment[10]; int c = 0, l = 0; bool found;
			foreach (SpPointObject[] pts in solid.Faces)
			{
				l = pts.Length;
				if (l < 2) { continue; }
				for (int i=0; i<l-1; i++)
				{
					found = false;
					for (int j=0; j<c; j++) {
						if ((segs[j].Point1 == pts[i] || segs[j].Point2 == pts[i])
							&& (segs[j].Point1 == pts[i+1] || segs[j].Point2 == pts[i+1])) { found = true; break; } }
					if (!found) {
						if (c >= segs.Length) { Array.Resize(ref segs, c + 10); }
						segs[c] = new SpSegment(name, pts[i], pts[i+1]);
						Add(segs[c]);
						c++; }
				}
				found = false;
				for (int j=0; j<c; j++) {
					if ((segs[j].Point1 == pts[l-1] || segs[j].Point2 == pts[l-1])
						&& (segs[j].Point1 == pts[0] || segs[j].Point2 == pts[0])) { found = true; break; } }
				if (!found) {
					if (c >= segs.Length) { Array.Resize(ref segs, c + 10); }
					segs[c] = new SpSegment(name, pts[l-1], pts[0]);
					Add(segs[c]);
					c++; }
			}
			OnRequestDrawing(Assembly.GetCallingAssembly());
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Ajoute l'ensemble des polygones formant les faces d'un solide.
		/// </summary>
		public void CreatePolygonsFromSolid(SpSolid solid, string name)
		{
			// Sort si l'objet est indéfini ou si tous les points ne sont pas explicites ou extraits:
			if (solid.IsUndefined) { SendError("Solid is undefined"); return; }
			foreach (SpPointObject o in solid.Vertices) {
				if (o.IsSystem && !o.IsExtracted) { SendError("All points are not explicit or extracted."); return; } }
			foreach (SpPointObject[] pts in solid.Faces) { Add(new SpPolygon(name, pts)); }
			OnRequestDrawing(Assembly.GetCallingAssembly());
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Extrait l'ensemble des points d'un parallélipipède.
		/// </summary>
		public void ExtractFromParallelepiped(SpParallelepiped solid, string[] names)
		{
			if (names.Length != 8) { SendError("You must enter 8 names."); return; }
			for (int i=0; i<8; i++) { solid.Vertices[i].Extract(names[i]); }
			OnRequestDrawing(Assembly.GetCallingAssembly());
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Extrait l'ensemble des points d'un tétraèdre.
		/// </summary>
		public void ExtractFromTetrahedron(SpRegularTetrahedron solid, string[] names)
		{
			if (names.Length != 4) { SendError("You must enter 4 names."); return; }
			for (int i=0; i<4; i++) { solid.Vertices[i].Extract(names[i]); }
			OnRequestDrawing(Assembly.GetCallingAssembly());
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Extrait l'ensemble des points d'un polygone.
		/// </summary>
		public void ExtractFromPolygon(SpPolygon polygon, string[] names)
		{
			if (names.Length != polygon.Vertices.Length)
				{ SendError(String.Format("You must enter {0} names.", polygon.Vertices.Length)); return; }
			for (int i=0; i<polygon.Vertices.Length; i++) { polygon.Vertices[i].Extract(names[i]); }
			OnRequestDrawing(Assembly.GetCallingAssembly());
		}
			


		#endregion CREATION D'OBJETS
	




		// ---------------------------------------------------------------------------
		// METHODES PRIVEES
		// ---------------------------------------------------------------------------




		#region METHODES PRIVEES



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
		/// Déclenche l'événement DisplayChanged, si l'assembly passé en argument n'est pas l'assembly qui contient cette classe.
		/// </summary>
		protected void OnRequestDrawing(Assembly assembly)
		{
			if (assembly == null || (!_assemblyName.Equals(assembly.FullName) && RequestDrawing != null))
				{ RequestDrawing(this, new EventArgs()); }
		}
		
		/// <summary>
		/// Déclenche l'événement NumericDataChanged.
		/// </summary>
		protected void OnRequestDrawingCalc(SpObject objChanged)
		{
			if (RequestDrawingCalc != null) { RequestDrawingCalc(this, new RequestDrawingCalcEventArgs(objChanged)); }
		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Quand un objet demande un dessin, on le demande aussi...
		/// </summary>
		private void SpObject_RequestDrawing(object sender, EventArgs e)
			{ this.OnRequestDrawing(null); }

		/// <summary>
		/// Quand un objet demande un recalcul des données d'affichage, on le demande aussi...
		/// </summary>
		private void SpObject_RequestDrawingCalc(object sender, RequestDrawingCalcEventArgs e)
			{ this.OnRequestDrawingCalc(e.ObjectChanged); }



		// ---------------------------------------------------------------------------


		/// <summary>
		/// Modifie la liste des noms:
		/// </summary>
		private void SpObject_NameChanged(object sender, SpObject.NameChangedEventArgs e)
		{
			int index;
			if ((index = Array.BinarySearch(_objSortedNames, e.OldName)) < 0) { return; }
			_objSortedNames[index] = e.NewName;
			Array.Sort(_objSortedNames, _objSortedIndexes);
		}

		/// <summary>
		/// Ajoute l'objet extrait à la collection et lance un nouveau dessin.
		/// </summary>
		private void SpObject_ObjectExtracted(object sender, SpObject.ObjectExtractedEventArgs e)
		{
			Add(e.ObjectExtracted);
			OnRequestDrawing(null);
		}

		/// <summary>
		/// Relance un MakeConstructList au cas où les objets auraient de nouveaux maîtres.
		/// </summary>
		private void SpObject_ObjectChanged(object sender, SpObject.ObjectChangedEventArgs e)
		{
			MakeConstructList();
		}


		#endregion METHODES PRIVEES







		// ---------------------------------------------------------------------------
		// METHODES D'ENUMERATION
		// ---------------------------------------------------------------------------




		#region METHODES D'ENUMERATION
		
		
		
		public IEnumerator GetEnumerator() { _enumeratorIndex = -1; return this; }
		public bool MoveNext() { return ++_enumeratorIndex < _allObjects.Length; }
		public void Reset() 	{ _enumeratorIndex = -1; }
		public object Current { get { return _allObjects[_enumeratorIndex]; } }



		#endregion METHODES D'ENUMERATION
	
	
	
	
	
	}
	
	
	
}
