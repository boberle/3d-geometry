using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;



namespace My
{






	namespace XML
	{





		// ===========================================================================
		// CLASS XMLSIMPLETREE
		// ===========================================================================




		/// <summary>
		/// Cette classe définit et lit un arbre simple, i.e. sous la forme clé/valeur, en XML. Cet arbre peut être lu et enregistré dans un fichier, ou seulement laissé en mémoire. (Se base sur l'objet XmlDocument de VS.)
		/// L'arbre se construit en sections et sous-sections, et chaque paire clé/valeur est contenu dans une balise dont le nom est indiqué par la propriété ElementTagName ("element" par défaut). Le nom de la clé est passé en attribut de balise ("key"), et la valeur représente le contenu dans la balise.
		/// </summary>
		public class XMLSimpleTree
		{





			// ---------------------------------------------------------------------------
			// DECLARATIONS
			// ---------------------------------------------------------------------------




			#region DECLARATIONS





			// Champs:

			// Objet principal:
			private XmlDocument _xmlDoc = new XmlDocument();

			// Nom du fichier associé éventuellement à l'objet:
			private string _fileName = null;

			// Nom des balises représentant les paires clé/valeur dans le document XML (initialisé par défaut par le constructeur).
			private string _elementTagName = null;




			#endregion DECLARATIONS




			// ---------------------------------------------------------------------------
			// CONSTRUCTEURS ET DESTRUCTEURS
			// ---------------------------------------------------------------------------




			#region CONSTRUCTEURS




			/// <summary>
			/// Constructeur. Initialise _elementTagName à "element" par défaut.
			/// </summary>
			public XMLSimpleTree()
			{

				// Valeurs par défaut:
				_elementTagName = "element";

			}





			/// <summary>
			/// Constructeur. Initialise _elementTagName à "element" par défaut. L'argument permet de charger directement un fichier xml en mémoire, et de noter le nom du fichier dans la propriété FileName.
			/// </summary>
			/// <param name="fileName">Fichier de base.</param>
			public XMLSimpleTree(string fileName) : this()
			{

				// Lit le fichier et définit la propriété:
				if (fileName != null) { FileName = fileName; ReadFile(); }

			}




			#endregion CONSTRUCTEURS





			// ---------------------------------------------------------------------------
			// PROPRIETES
			// ---------------------------------------------------------------------------





			#region PROPRIETES





			/// <summary>
			/// Retourne ou définit le nom du fichier éventuellement associé à l'arbre. Lors de la définition, le fichier indiqué n'est pas lu ! Il faut donc lancé la procédure de lecture séparément.
			/// </summary>
			public string FileName
			{
				get { return _fileName; }
				set { _fileName = value; }
			}




			// ---------------------------------------------------------------------------





			/// <summary>
			/// Retourne ou définit quelle est le nom de la balise utilisée pour les paires clé/valeur ("element" par défaut).
			/// </summary>
			public string ElementTagName
			{
				get { return _elementTagName; }
				set { _elementTagName = value; }
			}




			#endregion PROPRIETES





			// ---------------------------------------------------------------------------
			// INDEXEURS
			// ---------------------------------------------------------------------------



			#region INDEXEURS




			/// <summary>
			/// Retourne ou définit la valeur d'une clé, en spécifiant le chemin d'accès de la clé (les sections et sous-sections), et la clé.
			/// </summary>
			/// <param name="path">Chemin d'accès de la clé. Il est possible de l'indiqué soit par un tableau de string[] (alors l'élément de le plus élevé dans la hiérarchie doit avoir l'index le plus bas, à savoir 0), soit par une chaîne avec pour séparateur "/". Le premier terme doit être la racine du document xml (la première balise). Pas de distinction de casse.</param>
			/// <param name="key">Nom de la clé.</param>
			/// <returns>Retourne la valeur de la clé.</returns>
			public string this[string[] path, string key]
			{

				get
				{

					// Sort si key est null:
					if (key == null) { return null; }

					// Demande le noeud de la section, et sort si null:
					XmlNode sectionNode = ReturnSectionNode(path);
					if (sectionNode == null) { return null; }

					// Pour tous les enfants du noeud, si un "element" avec les bons attributs existe, retourne la valeur:
					foreach (XmlNode i in sectionNode.ChildNodes)
					{
						if ((i.Name.ToLower() == _elementTagName.ToLower()) && (NodeHasAttribute(i, "key") == true))
						{
							if (i.Attributes["key"].Value.ToLower() == key.ToLower()) { return i.InnerText; }
						}
					}

					// Retourne null par défaut:
					return null;

				}





				set
				{

					// Sort si key est null, ou si path est null:
					if ((key == null) || (path == null)) { return; }

					// Demande le noeud de la section, et sort si null:
					XmlNode sectionNode = ReturnSectionNode(path);
					if (sectionNode == null) { return; }

					// Pour tous les enfants du noeud, si un "element" avec les bons attributs existe, change la valeur et sort:
					foreach (XmlNode i in sectionNode.ChildNodes)
					{
						if ((i.Name.ToLower() == _elementTagName.ToLower()) && (NodeHasAttribute(i, "key") == true))
						{
							if (i.Attributes["key"].Value.ToLower() == key.ToLower())
							{
								// Si la valeur est null, supprime l'élément:
								if (value == null) { sectionNode.RemoveChild(i); }
								else { i.InnerXml = value; }
								return;
							}
						}
					}

					// Si pas trouvé, crée le noeud:
					if (value != null)
					{
						XmlNode insertedNode = _xmlDoc.CreateElement(_elementTagName);
						insertedNode.InnerXml = value;
						insertedNode.Attributes.Append(_xmlDoc.CreateAttribute("key"));
						insertedNode.Attributes[0].InnerText = key;
						sectionNode.AppendChild(insertedNode);
					}

				}

			}







			/// <summary>
			/// Voir surcharge.
			/// </summary>
			public string this[string path, string key]
			{

				get
				{
					// Retourne le document si path est null, appelle la surcharge correspondante, sinon le converti en tableau:
					return this[(path == null ? null : path.Split('/')), key];
				}

				set
				{
					// Retourne le document si path est null, appelle la surcharge correspondante, sinon le converti en tableau:
					this[(path == null ? null : path.Split('/')), key] = value;
				}

			}





			#endregion INDEXEURS





			// ---------------------------------------------------------------------------
			// METHODES PUBLIQUES
			// ---------------------------------------------------------------------------




			#region METHODES PUBLIQUES





			/// <summary>
			/// Charge en mémoire le document indiqué dans la propriété FileName dans le cas de la surcharge sans argument. Sinon, utilise le fichier en argument. Cependant, la propriété FileName n'est pas mise à jour, ce qui permet de charger un autre fichier, ou bien de charger un fichier sans renseigner la propriété FileName.
			/// </summary>
			/// <returns>True/False. Si FileName est vide, retourne False.</returns>
			public bool ReadFile()
			{

				// Appelle ReadFile avec un argument:
				return ReadFile(_fileName);

			}








			public bool ReadFile(string fileName)
			{

				// Charge le fichier XML:
				try
				{
					_xmlDoc.Load(fileName);

					return true;
				}
				catch (Exception exc)
				{
					My.ErrorHandler.ShowError(exc);
					return false;
				}

			}







			// ---------------------------------------------------------------------------







			/// <summary>
			/// Indique si la clé existe.
			/// </summary>
			/// <param name="path">Chemin d'accès de la clé. Il est possible de l'indiqué soit par un tableau de string[] (alors l'élément de le plus élevé dans la hiérarchie doit avoir l'index le plus bas, à savoir 0), soit par une chaîne avec pour séparateur "/". Le premier terme doit être la racine du document xml (la première balise). Pas de distinction de casse.</param>
			/// <param name="key">Nom de la clé.</param>
			/// <returns>T/F.</returns>
			public bool KeyExists(string[] path, string key)
			{

				// Cherche simplement la valeur, et détermine si c'est null ou non:
				return (this[path, key] == null ? false : true);

			}






			/// <summary>
			/// Voir surcharge.
			/// </summary>
			public bool KeyExists(string path, string key)
			{

				// Appelle la surchage après conversion de path en tableau:
				return KeyExists(((path == null) ? null : path.Split('/')), key);

			}








			// ---------------------------------------------------------------------------






			/// <summary>
			/// Indique si la section existe.
			/// </summary>
			/// <param name="path">Chemin d'accès de la section. Il est possible de l'indiquer soit par un tableau de string[] (alors l'élément de le plus élevé dans la hiérarchie doit avoir l'index le plus bas, à savoir 0), soit par une chaîne avec pour séparateur "/". Le premier terme doit être la racine du document xml (la première balise). Pas de distinction de casse.</param>
			/// <returns>T/F.</returns>
			public bool SectionExists(string[] path)
			{

				// Cherche la section, vérifie qu'elle ne s'appelle pas _elementTagName, et détermine si c'est null ou non:
				XmlNode tempNode = ReturnSectionNode(path);

				if (tempNode == null)
				{
					return false;
				}

				else
				{
					return tempNode.Name.ToLower() != _elementTagName.ToLower() ? true : false;
				}

			}






			/// <summary>
			/// Voir surcharge.
			/// </summary>
			public bool SectionExists(string path)
			{

				// Appelle la surchage après conversion de path en tableau:
				return SectionExists((path == null) ? null : path.Split('/'));

			}








			// ---------------------------------------------------------------------------








			/// <summary>
			/// Liste toutes les sous-sections d'une section (à l'exclusion des éléments).
			/// </summary>
			/// <param name="path">Chemin d'accès de la section. Il est possible de l'indiquer soit par un tableau de string[] (alors l'élément de le plus élevé dans la hiérarchie doit avoir l'index le plus bas, à savoir 0), soit par une chaîne avec pour séparateur "/". Le premier terme doit être la racine du document xml (la première balise). Pas de distinction de casse.</param>
			/// <returns>Retourne un List(string) (qui peut être vide), ou null l'argument n'est pas une section mais un élément (ou si la section n'existe pas).</returns>
			public List<string> SectionsList(string[] path)
			{

				// Cherche la section, vérifie qu'elle ne s'appelle pas _elementTagName, et enregistre les enfants dans une liste:
				XmlNode tempNode = ReturnSectionNode(path);
				if ((tempNode == null) || (tempNode.ChildNodes.Count == 0)) { return null; }

				List<string> list = new List<string>();
				foreach (XmlNode i in tempNode.ChildNodes)
				{
					if (i.Name.ToLower() != _elementTagName.ToLower()) { list.Add(i.Name); }
				}
				return list;

			}




			/// <summary>
			/// Voir surcharge.
			/// </summary>
			public List<string> SectionsList(string path)
			{

				// Appelle la surchage après conversion de path en tableau:
				return SectionsList((path == null) ? null : path.Split('/'));

			}









			// ---------------------------------------------------------------------------







			/// <summary>
			/// Liste tous les éléments d'une section. Attention, ne liste pas les sous-sections, mais seulement les éléments, i.e. les paires clé/valeur.
			/// </summary>
			/// <param name="path">Chemin d'accès de la section. Il est possible de l'indiquer soit par un tableau de string[] (alors l'élément de le plus élevé dans la hiérarchie doit avoir l'index le plus bas, à savoir 0), soit par une chaîne avec pour séparateur "/". Le premier terme doit être la racine du document xml (la première balise). Pas de distinction de casse.</param>
			/// <returns>Retourne un Dictionary(string, string) (qui peut être vide), ou null l'argument n'est pas une section mais un élément (ou si la section n'existe pas).</returns>
			public Dictionary<string, string> ElementsList(string[] path)
			{

				// Cherche la section, et sort si null:
				XmlNode tempNode = ReturnSectionNode(path);
				if ((tempNode == null) || (tempNode.ChildNodes.Count == 0)) { return null; }

				Dictionary<string, string> list = new Dictionary<string, string>();
				foreach (XmlNode i in tempNode.ChildNodes)
				{
					if ((i.Name.ToLower() == _elementTagName.ToLower()) && (NodeHasAttribute(i, "key") == true))
					{
						try { list.Add(i.Attributes["key"].Value, i.InnerXml); }
						catch (ArgumentException) { ;}
					}
				}
				return list;

			}





			/// <summary>
			/// Voir surcharge.
			/// </summary>
			public Dictionary<string, string> ElementsList(string path)
			{

				// Appelle la surchage après conversion de path en tableau:
				return ElementsList((path == null) ? null : path.Split('/'));

			}







			// ---------------------------------------------------------------------------







			/// <summary>
			/// Trie l'ensemble de l'arbre par ordre alphabétique ascendant.
			/// </summary>
			public void SortTree()
			{

				// Initie le trie...
				if (_xmlDoc.ChildNodes.Count > 1) { SortTree(_xmlDoc.ChildNodes[1]); }

			}







			// ---------------------------------------------------------------------------







			/// <summary>
			/// Enregistre dans fichier indiqué dans la propriété FileName le document XML en mémmoire dans le cas de la surcharge sans argument, ou dans le fichier spécifié dans l'argument fileName. Cependant, la propriété FileName n'est pas mise à jour, ce qui permet de charger un autre fichier, ou bien de charger un fichier sans renseigner la propriété FileName. Pour utiliser cette propriété, appelez cette méthode sans argument.
			/// </summary>
			/// <returns>True/False. Si FileName est vide, retourne False.</returns>
			public bool Save()
			{

				// Appelle ReadFile avec un argument:
				return Save(_fileName);

			}








			public bool Save(string fileName)
			{

				// Enregistre le fichier XML:
				try
				{
					_xmlDoc.Save(fileName);
					return true;
				}
				catch (Exception exc)
				{
					My.ErrorHandler.ShowError(exc);
					return false;
				}

			}





			// ---------------------------------------------------------------------------






			/// <summary>
			/// Charge en mémoire une chaîne passée en argument contenant des données XML.
			/// </summary>
			/// <param name="xml">Données XML.</param>
			/// <returns>True/False.</returns>
			public bool ReadXMLString(string xml)
			{

				// Charge la chaîne XML:
				try
				{
					_xmlDoc.LoadXml(xml);
					return true;
				}
				catch (Exception exc)
				{
					My.ErrorHandler.ShowError(exc);
					return false;
				}

			}









			// ---------------------------------------------------------------------------






			/// <summary>
			/// Ajoute une section.
			/// </summary>
			/// <param name="path">Chemin d'accès de la section (n'incluant pas la nouvelle section à créer). Il est possible de l'indiquer soit par un tableau de string[] (alors l'élément de le plus élevé dans la hiérarchie doit avoir l'index le plus bas, à savoir 0), soit par une chaîne avec pour séparateur "/". Le premier terme doit être la racine du document xml (la première balise). Pas de distinction de casse.</param>
			/// <param name="newSection">Nom de la nouvelle section.</param>
			/// <param name="allowDoubloon">Si true, alors permet d'ajoute une nouvelle section portant le nom d'une section déjà existante pour le même chemin d'accès. Très déconseillé, car le comportement des autres méthodes n'est plus contrôlable !</param>
			/// <returns>True si réussi.</returns>
			public bool AddSection(string[] path, string newSection, bool allowDoubloon)
			{

				// Si les doublons ne sont pas autorisés, vérifie que la section n'existe pas déjà:
				if ((!allowDoubloon) && (ReturnSectionNode(path) != null)) { return false; }

				// Obtient la section, et si null sort:
				XmlNode parentSection;
				if ((parentSection = ReturnSectionNode(path)) == null) { return false; }

				// Ajoute le nouveau noeud:
				try
				{
					XmlElement newNode = _xmlDoc.CreateElement(newSection);
					parentSection.AppendChild(newNode);
					return true;
				}
				catch (Exception exc)
				{
					My.ErrorHandler.ShowError(exc);
					return false;
				}

			}






			// Surcharges (appellent la précédente):


			/// <summary>
			/// Voir surcharge.
			/// </summary>
			public bool AddSection(string path, string newSection, bool allowDoubloon)
			{

				// Appelle la surchage après conversion de path en tableau:
				return AddSection(((path == null) ? null : path.Split('/')), newSection, allowDoubloon);

			}



			/// <summary>
			/// Voir surcharge.
			/// </summary>
			public bool AddSection(string path, string newSection)
			{

				// Argument allowDoubloon par défaut:
				return AddSection(((path == null) ? null : path.Split('/')), newSection, false);

			}



			/// <summary>
			/// Voir surcharge.
			/// </summary>
			public bool AddSection(string[] path, string newSection)
			{

				// Argument allowDoubloon par défaut:
				return AddSection(path, newSection, false);

			}






			// ---------------------------------------------------------------------------






			/// <summary>
			/// Supprime une section. Il n'est pas possible de supprimer le document...
			/// </summary>
			/// <param name="path">Chemin d'accès de la section. Il est possible de l'indiquer soit par un tableau de string[] (alors l'élément de le plus élevé dans la hiérarchie doit avoir l'index le plus bas, à savoir 0), soit par une chaîne avec pour séparateur "/". Le premier terme doit être la racine du document xml (la première balise). Pas de distinction de casse.</param>
			/// <returns>True si réussi, False sinon (e.g. si la section n'existait pas).</returns>
			public bool RemoveSection(string[] path)
			{

				// Sort si path est null:
				if (path == null) { return false; }

				// Obtient la section, et si null sort:
				XmlNode section;
				if ((section = ReturnSectionNode(path)) == null) { return false; }

				// Supprime la section:
				try
				{
					section.ParentNode.RemoveChild(section);
					return true;
				}
				catch (Exception exc)
				{
					My.ErrorHandler.ShowError(exc);
					return false;
				}

			}



			/// <summary>
			/// Voir surcharge.
			/// </summary>
			public bool RemoveSection(string path)
			{

				// Appelle la surchage après conversion de path en tableau:
				return RemoveSection(((path == null) ? null : path.Split('/')));

			}





			#endregion METHODES PUBLIQUES





			// ---------------------------------------------------------------------------
			// METHODES PRIVEES
			// ---------------------------------------------------------------------------




			#region METHODES PRIVEES





			/// <summary>
			/// Retourne un XmlNode représentant une section.
			/// </summary>
			/// <param name="path">Chemin d'accès de la section. Il est possible de l'indiquer soit par un tableau de string[] (alors l'élément de le plus élevé dans la hiérarchie doit avoir l'index le plus bas, à savoir 0), soit par une chaîne avec pour séparateur "/". Le premier terme doit être la racine du document xml (la première balise). Pas de distinction de casse.</param>
			/// <returns>XmlNode, ou null si la seciton n'existe pas. Retourne le document (XmlDocument) si le path est null.</returns>
			private XmlNode ReturnSectionNode(string[] path)
			{

				// Retourne le document si path est null:
				if (path == null) { return _xmlDoc; }

				// Noeud temporaire:
				XmlNode tempNode = _xmlDoc;

				// Pour chaque élément du tableau path:
				for (byte i = 0; i < path.Length; i++)
				{

					// Parcours tous les noeuds, depuis la racine du document:
					for (short j = 0; j < tempNode.ChildNodes.Count; j++)
					{
						// Si le noeud correspond à l'élément final de path, retourne le noeud:
						if ((tempNode.ChildNodes[j].Name.ToLower() == path[i].ToLower()) && (i == path.Length - 1))
						{
							return tempNode.ChildNodes[j];
						}
						// Si le noeud correspont à l'élément en cours de path, passe à l'élément suivant de path:
						else if (tempNode.ChildNodes[j].Name.ToLower() == path[i].ToLower())
						{
							tempNode = tempNode.ChildNodes[j];
							break;
						}
						//Si le noeud ne correspond à l'élément courant de path, et s'il n'y a plus de noeuds dans la collection, retourne null;
						else if ((tempNode.ChildNodes[j].Name.ToLower() != path[i].ToLower()) && (j == tempNode.ChildNodes.Count))
						{
							return null;
						}
					}

				}

				// Dans tous les cas, retourne null par défaut:
				return null;

			}




			/// <summary>
			/// Voir surcharge.
			/// </summary>
			private XmlNode ReturnSectionNode(string path)
			{

				// Retourne le document si path est null:
				if (path == null) { return _xmlDoc; }

				// Converti path en tableau, et appel la surcharge correspondante:
				return ReturnSectionNode(path.Split('/'));

			}






			// ---------------------------------------------------------------------------







			/// <summary>
			/// Retourne true si le noeud passé en argument à l'attribut spécifié par l'argument attr.
			/// </summary>
			/// <param name="node">XmlNode à tester.</param>
			/// <param name="attr">Nom de l'attribut à tester.</param>
			/// <returns>T/F.</returns>
			private bool NodeHasAttribute(XmlNode node, string attr)
			{

				// Parcourt tous les attributs, et renvoie true si on trouve le bon:
				if ((node == null) || (node.Attributes.Count == 0)) { return false; }
				foreach (XmlAttribute i in node.Attributes) { if (i.Name.ToLower() == attr.ToLower()) return true; }
				return false;

			}







			// ---------------------------------------------------------------------------





			/// <summary>
			/// Méthode privée pour le tri qui se rappelle récursivement pour tous les enfants et sous-enfants d'un noeud.
			/// </summary>
			/// <param name="node">XmlNode</param>
			private void SortTree(XmlNode node)
			{

				// Se rappelle récursivement, pour tous les noeuds... en triant au passage !
				if (node == null) { return; }
				SortChildrenOfNode(node);
				foreach (XmlNode i in node.ChildNodes) { SortTree(i); }

			}




			/// <summary>
			/// Méthode privée de tri, qui tri tous les enfants (mais pas les sous-enfants) d'un noeud donné.
			/// </summary>
			/// <param name="node">XmlNode</param>
			private void SortChildrenOfNode(XmlNode node)
			{

				// Sort si node est null:
				if (node == null) { return; }

				// Le mieux est d'utiliser deux SortedList<>, une pour les sections, l'autre pour les paires de clés.
				// Dans ces listes, seront copiés (clonés) les éléments, de façon alphabétique.
				SortedList<string, XmlNode> sections = new SortedList<string, XmlNode>();
				SortedList<string, XmlNode> elements = new SortedList<string, XmlNode>();
				// Et une troisième liste contiendra les noeuds originaux, à supprimer.
				List<XmlNode> oldNodes = new List<XmlNode>();

				// Parcourt les enfants de node en répartissant les clones, et en notant les éléments originaux...
				if (node.ChildNodes.Count > 0)
				{
					foreach (XmlNode i in node.ChildNodes)
					{
						if ((i.Name.ToLower() == _elementTagName.ToLower()) && (NodeHasAttribute(i, "key") == true))
						{
							elements.Add(i.Attributes["key"].Value, i.CloneNode(true));
						}
						else
						{ sections.Add(i.Name, i.CloneNode(true)); }
						oldNodes.Add(i);
					}
				}

				// Ajoute les noeuds des listes, puis supprimes les noeuds originaux.
				foreach (XmlNode i in sections.Values) { node.AppendChild(i); }
				foreach (XmlNode i in elements.Values) { node.AppendChild(i); }
				foreach (XmlNode i in oldNodes) { node.RemoveChild(i); }

			}






			#endregion METHODES PRIVEES





			// END CLASS XMLSIMPLE TREE

		}









		// END SPACENAME MYXML

	}






	// END SPACENAME MY

}

