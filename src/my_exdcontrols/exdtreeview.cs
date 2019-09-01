using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using My.ExdControls;


namespace My
{









	// ===========================================================================
	// CLASS ExdTreeNode
	// ===========================================================================



	/// <summary>
	/// Classe de noeud pour les arbres ExdTreeView et dérivés. Voir les informations des ces ExdTreeView.
	/// </summary>
	[Serializable()]
	public class ExdTreeNode : TreeNode, ICloneable, ISerializable
	{



		// ---------------------------------------------------------------------------
		// SOUS-CLASSE
		// ---------------------------------------------------------------------------




		#region SOUS-CLASSE



		/// <summary>
		/// Classe qui spécifie les types de noeuds, en liaison avec les ExdTreeNode et un ExdTreeView.
		/// </summary>
		[Serializable()]
		public class TreeNodeType : ICloneable
		{
			public int Id { get; set; }
			public bool IsFolder { get; set; }
			public string ImageKey { get; set; }
			public ContextMenu ContextMenu { get; set; }
			public string Description { get; set; }
			
			public TreeNodeType() {Id=0;}
			
			public object Clone()
			{
				TreeNodeType copy = new TreeNodeType();
				copy.Id = this.Id;
				copy.IsFolder = this.IsFolder;
				copy.ImageKey = this.ImageKey;
				copy.ContextMenu = null;
				copy.Description = this.Description;
				return copy;
			}
			
			public static void CopyTreeNodeTypeArray(ExdTreeView source, ExdTreeView dest)
			{
				dest.NodeTypesList = new My.ExdTreeNode.TreeNodeType[source.NodeTypesList.Length];
				for (int i=0; i<source.NodeTypesList.Length; i++)
				{
					dest.NodeTypesList[i] = (My.ExdTreeNode.TreeNodeType)source.NodeTypesList[i].Clone();
					dest.NodeTypesList[i].ContextMenu = null;
					if (dest.NodeTypesList[i].IsFolder) { dest.NodeTypesList[i].ContextMenu = dest.DefaultFolderContextMenu; }
					else { dest.NodeTypesList[i].ContextMenu = dest.DefaultDocContextMenu; }
				}
			}

			
		}




		#endregion SOUS-CLASSE








		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES




		public int NodeType { get; set; }
		public int Id { get; set; }
		public int IdDocument { get; set; }
		public bool FolderHasChildren { get; set; }

		
		#endregion PROPRIETES






		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS




		public ExdTreeNode() { this.NodeType = -1; this.Id = -1; this.IdDocument = -1; }
		public ExdTreeNode(string text) : this() { this.Text = text; }
		public ExdTreeNode(string text, int id, int nodeType) : this(text) { this.Id = id; this.NodeType = nodeType; }
		public ExdTreeNode(string text, int id, int nodeType, int indexInBranch, bool hasChildren) : this(text, id, nodeType)
			{ this.FolderHasChildren = hasChildren; }




		#endregion CONSTRUCTEURS
	







		// ---------------------------------------------------------------------------
		// METHODES POUR LA SERIALISATION
		// ---------------------------------------------------------------------------




		#region METHODES POUR LA SERIALISATION



		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.Serialize(info, context);
			if (info == null) throw new System.ArgumentNullException("info");
			info.AddValue("NodeType", this.NodeType);
			info.AddValue("IdDocument", this.IdDocument);
			info.AddValue("Id", this.Id);
			info.AddValue("FolderHasChildren", this.FolderHasChildren);
		}





		// ---------------------------------------------------------------------------
	
        
        
        
        
        
        
        
		// Constructeur pour la sérialisation (TreeNode héritant de ISerializable);
		protected ExdTreeNode(SerializationInfo info, StreamingContext context) : base(info, context)
		{	
			//base.Deserialize(info, context);
			this.Id = (int)info.GetValue("Id", typeof(int));
			this.IdDocument = (int)info.GetValue("IdDocument", typeof(int));
			this.NodeType = (int)info.GetValue("NodeType", typeof(int));
			this.FolderHasChildren = (bool)info.GetValue("FolderHasChildren", typeof(bool));
		}
        



		#endregion METHODES POUR LA SERIALISATION









		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------




		#region METHODES


        
        


		/// <summary>
		/// Fournit un clone de l'objet. Copie profonde.
		/// </summary>
		public override object Clone()
		{
			/*
			ExdTreeNode copy = (ExdTreeNode)base.Clone();
			copy.NodeType = this.NodeType;
			copy.Id = this.Id;
			copy.IdDocument = this.IdDocument;
			copy.FolderHasChildren = this.FolderHasChildren;
			return copy;
			*/
			
			return My.ClassManager.Clone(this);
			
		}




		// ---------------------------------------------------------------------------
	




		public void SetNodeType(int nodeType, TreeView treeView)
		{
			this.NodeType = nodeType;
			if (treeView is ExdTreeView) { ((ExdTreeView)treeView).SetImageKey(this); }
		}
		
		
		
		
		

		#endregion METHODES
	

		

	}











	// ===========================================================================
	// ===========================================================================
	// ===========================================================================


















	// ===========================================================================
	// CLASS ExdTreeView
	// ===========================================================================




	
	/// <summary>
	/// Fournit un TreeView largement étendu, avec de nombreuses fonctionnalités, comme l'affichage différente de dossiers et de documents, la gestion de différents types de dossiers et de documents, le drag and drop, etc. Voir la documentation de Doc CSharp.
	/// </summary>
	public class ExdTreeView : TreeView
	{











		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// ASPECTS GENERAUX
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+




		#region ASPECTS GENERAUX







		// ---------------------------------------------------------------------------
		// SOUS-CLASSES
		// ---------------------------------------------------------------------------




		#region SOUS-CLASSES






		public class DocumentSelectedEventArgs : EventArgs
		{
			public int Id { get; set; }
			public TreeNode Node { get; set; }
			public DocumentSelectedEventArgs() { ; }
			public DocumentSelectedEventArgs(int id, TreeNode node) { this.Id = id; this.Node = node; }
		}





		public class NodeSelectedEventArgs : EventArgs
		{
			public TreeNode OldNode { get; set; }
			public TreeNode NewNode { get; set; }
			public NodeSelectedEventArgs() { ; }
			public NodeSelectedEventArgs(TreeNode oldNode, TreeNode newNode) { OldNode = oldNode; NewNode = newNode; }
		}





		#endregion SOUS-CLASSES











		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS


		// Champs privés:
		
		protected TreeNode _savedSelectedNode;
		
		
		// Evénements:

		public delegate void DocumentSelectedEventHandler(object sender, DocumentSelectedEventArgs e);
		public event DocumentSelectedEventHandler DocumentSelected;

		public delegate void NodeSelectedEventHandler(object sender, NodeSelectedEventArgs e);
		public event NodeSelectedEventHandler NodeSelected;




		#endregion DECLARATIONS
	








		// ---------------------------------------------------------------------------
		// PROPRIETES GENERALES
		// ---------------------------------------------------------------------------




		#region PROPRIETES GENERALES




		public ExdTreeNode.TreeNodeType[] NodeTypesList { get; set; }

		public ContextMenu DefaultFolderContextMenu { get; set; }
		public ContextMenu DefaultDocContextMenu { get; set; }
		public ContextMenu DefaultTreeContextMenu { get; set; }





		#endregion PROPRIETES GENERALES











		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS
		
		
		
		
		
		public ExdTreeView()
		{
		
		
			// DEFINITION DU MENU CONTEXTUEL PAR DEFAUT:


			// Pour l'arbre:
			DefaultTreeContextMenu = new ContextMenu();
			DefaultTreeContextMenu.MenuItems.Add(
						MyResources.ExdTreeView_menu_NewRootFolder,
						new EventHandler(DefaultTreeContextMenu_NewRootFolder_Click)).Name = "NewRootFolder";
			this.ContextMenu = this.DefaultTreeContextMenu;
			
			// Pour les dossiers:
			DefaultFolderContextMenu = new ContextMenu();
			DefaultFolderContextMenu.MenuItems.Add(
						MyResources.ExdTreeView_menu_NewSiblingFolder,
						new EventHandler(DefaultFolderContextMenu_NewFolder_Click)).Name = "NewSiblingFolder";
			DefaultFolderContextMenu.MenuItems.Add(
						MyResources.ExdTreeView_menu_NewSubfolder, 
						new EventHandler(DefaultFolderContextMenu_NewFolder_Click)).Name = "NewSubfolder";
			DefaultFolderContextMenu.MenuItems.Add(
						MyResources.ExdTreeView_menu_DeleteFolder,
						new EventHandler(DefaultFolderContextMenu_DeleteFolder_Click)).Name = "DeleteFolder";
			DefaultFolderContextMenu.MenuItems.Add(
						MyResources.ExdTreeView_menu_RenameFolder,
						new EventHandler(DefaultFolderContextMenu_RenameFolder_Click)).Name = "RenameFolder";
			// Retour au menu contextuel par défaut pour l'arbre:
			DefaultFolderContextMenu.Collapse += delegate { this.ContextMenu = this.DefaultTreeContextMenu; };


			// Pour les documents:
			DefaultDocContextMenu = new ContextMenu();
			DefaultDocContextMenu.MenuItems.Add(
						MyResources.ExdTreeView_menu_DeleteDocument,
						new EventHandler(DefaultDocContextMenu_DeleteDoc_Click)).Name = "DeleteDoc";
			// Retour au menu contextuel par défaut pour l'arbre:
			DefaultDocContextMenu.Collapse += delegate { this.ContextMenu = this.DefaultTreeContextMenu; };
			




			// INSCRIPTIONS AUX EVENEMENTS:
			
			// Sélection d'un item ou d'un document:
			this.NodeMouseClick += new TreeNodeMouseClickEventHandler(ExdTreeView_NodeMouseClick);
			this.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(ExdTreeView_NodeMouseDoubleClick);
			
			// Souris pour l'affichage d'un menu contextuel selon le type de noeud:
			this.NodeMouseClick += new TreeNodeMouseClickEventHandler(ExdTreeView_NodeMouseClickForContextMenu);
			
			// Evénements clavier:
			this.KeyDown += new KeyEventHandler(ExdTreeView_KeyDown);
			this.KeyUp += new KeyEventHandler(ExdTreeView_KeyUp);
			
			// Evénements pour le drag and drop (seulement si XP et +):
			if (OSFeature.Feature.IsPresent(OSFeature.Themes))
			{
				this.AllowDrop = true;
				this.ItemDrag += new ItemDragEventHandler(ExdTreeView_ItemDrag);
				this.DragEnter += new DragEventHandler(ExdTreeView_DragEnter);
				this.DragOver += new DragEventHandler(ExdTreeView_DragOver);
				this.DragLeave += new EventHandler(ExdTreeView_DragLeave);
				this.DragDrop += new DragEventHandler(ExdTreeView_DragDrop);
				this.QueryContinueDrag += new QueryContinueDragEventHandler(ExdTreeView_QueryContinueDrag);
			}
			
			// Evenements pour tenir à jour la liste des noeuds cochés:
			this.AfterCheck += new TreeViewEventHandler(ExdTreeView_AfterCheck_ForCkeckBoxes);





			// PROPRIETES PAR DEFAUT:
			
			// Comportement:
			DragDropAllowedEffects = DragDropEffects.Move | DragDropEffects.Copy;
			NeutralizeMultiplyKey = true;
			SystemKeyNodeForAddChildrenWhenExpand = "system_AddChildrenWhenExpand";
			AskBeforeDelete = true;
			DoNotAskBeforeDeleteWhenShiftKey = false;

			// Apparence:
			this.ShowRootLines = true;
			this.ShowLines = false;
			this.HideSelection = false;


			
		
		}



		



		#endregion CONSTRUCTEURS












		// ---------------------------------------------------------------------------
		// METHODES ACTION ON ALL CHILDREN
		// ---------------------------------------------------------------------------




		#region METHODES ACTION ON ALL CHILDREN






		/// <summary>
		/// Permet d'effectuer un action (par délégué passé en argument) à exécuter sur chaque noeud enfant et petits-enfants de l'arbre ou d'un noeud parent spécifié.
		/// </summary>
		public void ActionOnAllChildren(TreeNode treeNode, Action<TreeNode> action, bool grandChildren, bool actionAfterRecursivity)
		{
			
			// Pour tous les noeuds enfants du noeud donné...
			TreeNode childNode;

			// Si actionAfterRecursivity=true, alors va en sens inverse:
			int start = ((actionAfterRecursivity) ? (treeNode.Nodes.Count-1) : (0));
			int step = ((actionAfterRecursivity) ? (-1) : (1));
			for (int i = start; i < treeNode.Nodes.Count && i >= 0; i += step)
			{
			
				// Sélection du noeud:
				childNode = treeNode.Nodes[i];
				
				// Si doit traiter l'action avant la récursivité:
				if (!actionAfterRecursivity) { action(childNode); }
				
				// S'il y a des enfants et si on doit traiter les petits enfants: s'appelle récursivement:
				if ((grandChildren) && (childNode.Nodes.Count > 0))
				{
					ActionOnAllChildren(childNode, action, grandChildren, actionAfterRecursivity);
				}
				
				// Si doit traiter l'action après la récursivité:
				if (actionAfterRecursivity) { action(childNode); }
			}

		}





		public void ActionOnAllChildren(TreeNode treeNode, Action<TreeNode> action)
		{
			ActionOnAllChildren(treeNode, action, true, false);
		}





		public void ActionOnAllChildren(Action<TreeNode> action, bool grandChildren, bool actionAfterRecursivity)
		{
		
			// Si c'est tout l'arbre qui est passé, parcourt les noeuds de l'arbre, et appelle la fonction pour chacun d'eux:
			//for (int i=this.Nodes.Count - 1; i>=0; i--)
			foreach (TreeNode i in this.Nodes)
			{
				// Appel la procédure qui traite et les enfants ET le noeud parent (celui qu'on passe en 1er arg):
				ActionOnAllChildren(i, action, grandChildren, actionAfterRecursivity, true);
			}
			
		}





		public void ActionOnAllChildren(Action<TreeNode> action)
		{
			ActionOnAllChildren(action, true, false);
		}





		public void ActionOnAllChildren(Action<TreeNode> action, bool grandChildren)
		{
			ActionOnAllChildren(action, grandChildren, false);
		}





		public void ActionOnAllChildren(TreeNode treeNode, Action<TreeNode> action, bool grandChildren, bool actionAfterRecursivity, bool includeParent)
		{
		
			// Traite aussi le noeud parent, ie. celui qui est passé en premier argument:

			// Si doit traiter noeud parent, et si avant la récursivité:
			if ((!actionAfterRecursivity) && (includeParent)) { action(treeNode); }
			
			// Traite les enfants du noeud:
			if (grandChildren) { ActionOnAllChildren(treeNode, action, grandChildren, actionAfterRecursivity); }
			
			// Si doit traiter noeud parent, et si après la récursivité:
			if ((actionAfterRecursivity) && (includeParent)) { action(treeNode); }
		
		}





		public void ActionOnAllChildren(TreeNode treeNode, Action<TreeNode> action, bool includeParent)
		{
			ActionOnAllChildren(treeNode, action, true, false, includeParent);
		}





		public bool ActionOnAllChildren(TreeNode treeNode, Func<TreeNode, bool> action, bool grandChildren, bool actionAfterRecursivity)
		{

			// Pour tous les noeuds enfants du noeud donné...
			TreeNode childNode;

			// Si actionAfterRecursivity=true, alors va en sens inverse:
			int start = ((actionAfterRecursivity) ? (treeNode.Nodes.Count - 1) : (0));
			int step = ((actionAfterRecursivity) ? (-1) : (1));
			for (int i = start; i < treeNode.Nodes.Count && i >= 0; i += step)
			{

				// Sélection du noeud:
				childNode = treeNode.Nodes[i];

				// Si doit traiter l'action avant la récursivité:
				if (!actionAfterRecursivity) { if (action(childNode) == false) { return false; } }

				// S'il y a des enfants et si on doit traiter les petits enfants: s'appelle récursivement:
				if ((grandChildren) && (childNode.Nodes.Count > 0))
				{
					if (ActionOnAllChildren(childNode, action, grandChildren, actionAfterRecursivity) == false) { return false; }
				}

				// Si doit traiter l'action après la récursivité:
				if (actionAfterRecursivity) { if (action(childNode) == false) { return false; } }
			}
			
			return true;

		}





		public bool ActionOnAllChildren(TreeNode treeNode, Func<TreeNode, bool> action)
		{
			return ActionOnAllChildren(treeNode, action, true, false);
		}





		public bool ActionOnAllChildren(Func<TreeNode, bool> action, bool grandChildren, bool actionAfterRecursivity)
		{

			// Si c'est tout l'arbre qui est passé, parcourt les noeuds de l'arbre, et appelle la fonction pour chacun d'eux:
			//for (int i=this.Nodes.Count - 1; i>=0; i--)
			foreach (TreeNode i in this.Nodes)
			{
				// Appel la procédure qui traite et les enfants ET le noeud parent (celui qu'on passe en 1er arg):
				if (ActionOnAllChildren(i, action, grandChildren, actionAfterRecursivity, true) == false) { return false; }
			}
			return true;

		}





		public bool ActionOnAllChildren(Func<TreeNode, bool> action)
		{
			return ActionOnAllChildren(action, true, false);
		}





		public bool ActionOnAllChildren(Func<TreeNode, bool> action, bool grandChildren)
		{
			return ActionOnAllChildren(action, grandChildren, false);
		}





		public bool ActionOnAllChildren(TreeNode treeNode, Func<TreeNode, bool> action, bool grandChildren, bool actionAfterRecursivity, bool includeParent)
		{

			// Traite aussi le noeud parent, ie. celui qui est passé en premier argument:

			// Si doit traiter noeud parent, et si avant la récursivité:
			if ((!actionAfterRecursivity) && (includeParent)) { if (action(treeNode) == false) { return false; } }

			// Traite les enfants du noeud:
			if (grandChildren)
				{ if (ActionOnAllChildren(treeNode, action, grandChildren, actionAfterRecursivity) == false) { return false; } }

			// Si doit traiter noeud parent, et si après la récursivité:
			if ((actionAfterRecursivity) && (includeParent)) { if (action(treeNode) == false) { return false; } }
			
			return true;

		}





		public bool ActionOnAllChildren(TreeNode treeNode, Func<TreeNode, bool> action, bool includeParent)
		{
			return ActionOnAllChildren(treeNode, action, true, false, includeParent);
		}





		#endregion METHODES ACTION ON ALL CHILDREN
	










		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES







		/// <summary>
		/// Supprime toutes les mises en forme de police et de couleur, mis par exemple à l'occasion de ShowNode.
		/// </summary>
		public void RemoveNodesFontStyles()
		{
			ActionOnAllChildren(
				delegate(TreeNode node)
					{
						node.NodeFont = new Font(this.Font, FontStyle.Regular);
						node.ForeColor = this.ForeColor;
						node.Text = node.Text;
					}
			);
		}






		// ---------------------------------------------------------------------------
	
		
		
		
		
		
		
		
		
		

		/// <summary>
		/// Met en gras et en couleur tous les noeuds affichés dans l'arbre et correspondant à l'idDoc passé en argument. Si parent est null, l'arbre est pris comme parent.
		/// </summary>
		public void NodesFontStyle(int idDoc, TreeNode parent, Color color)
		{
		
			Action<TreeNode> selectNodes =
						delegate(TreeNode node)
						{
							if ((node is ExdTreeNode) && (((ExdTreeNode)node).IdDocument == idDoc))
							{
								node.NodeFont = new Font(this.Font, FontStyle.Bold);
								node.ForeColor = color;
								node.Text = node.Text;
							}
						};
			if (parent == null) { this.ActionOnAllChildren(selectNodes); }
			else { this.ActionOnAllChildren(parent, selectNodes); }

		}





		public void NodesFontStyle(int idDoc, TreeNode parent)
		{
			this.NodesFontStyle(idDoc, parent, this.ForeColor);
		}





		public void NodesFontStyle(int idDoc)
		{
			this.NodesFontStyle(idDoc, null, this.ForeColor);
		}





		public void NodesFontStyle(int idDoc, Color color)
		{
			this.NodesFontStyle(idDoc, null, color);
		}





		// ---------------------------------------------------------------------------
		
		
		
		
		
		
		
		
		public ExdTreeNode ShowNode(int[] path, int id)
		{
			return this.ShowNode(((IEnumerable<int>)path.Concat(new int[] { id })).ToArray<int>());
		}




		/// <summary>
		/// Affiche le noeud dont le chemin d'accès est passé en argument, ce qui veut dire que les noeuds dossiers sont successivement déplié jusqu'à atteindre le noeud qui correspond au chemin d'accès. Cette méthode est compatible avec AddChildrenWhenExpand.
		/// </summary>
		public ExdTreeNode ShowNode(int[] path)
		{
		
			// Sort si rien:
			if ((path == null) || (path.Length == 0)) { return null; }
			
			// Variables:
			TreeNode tmpNode = null; TreeNode tmp = null;
			
			// Parcours tout le tableau, jusqu'au bon noeud; null si erreur:
			for (int i = 0; i < path.Length - 1; i++)
			{
				// Si 1er élément du tableau et si le noeud existe, le prend comme tmpNode:
				if ((i == 0) && ((tmp = this.GetNode(this, path[i])) != null)) { tmpNode = tmp; }
				// Même chose, mais dans un noeud qui n'est pas à la racine:
				else if ((tmp = this.GetNode(tmpNode, path[i])) != null) { tmpNode = tmp; }
				// Sinon, retourne null:
				else { return null; }
				// Développe le noeud (si pas déjà développé), en déclenchant un événement:
				if ((this.AddChildrenWhenExpand) && (tmpNode.IsExpanded == false))
				{
					TreeViewCancelEventArgs eventArgs = new TreeViewCancelEventArgs(tmpNode, false, TreeViewAction.Expand);
					this.ExdTreeView_BeforeExpandAndCollapseForAddChildrenWhenExpand(this, eventArgs);
					// Si annulation, sort:
					if (eventArgs.Cancel == true) { return null; }
					tmpNode.Expand();
				}
			}
			
			// On retourne le noeud, en cherchant soit dans TV, soit le noeud tmpNode:
			if (tmpNode == null) { tmp = this.GetNode(this, path[path.Length - 1]); }
			else { tmp = this.GetNode(tmpNode, path[path.Length - 1]); }
			return ((tmp == null) ? (null) : (ExdTreeNode)tmp);
			
		}





		public TreeNode ShowNode(string[] path, string key)
		{
			return this.ShowNode(((IEnumerable<string>)path.Concat(new string[] { key })).ToArray<string>());
		}





		public TreeNode ShowNode(string[] path)
		{

			// Sort si rien:
			if ((path == null) || (path.Length == 0)) { return null; }

			// Variables:
			TreeNode tmpNode = null;

			// Parcours tout le tableau, jusqu'au bon noeud; null si erreur:
			for (int i = 0; i < path.Length - 1; i++)
			{
				// Si 1er élément du tableau et si le noeud existe, le prend comme tmpNode:
				if ((i == 0) && (this.Nodes.ContainsKey(path[i]))) { tmpNode = this.Nodes[path[i]]; }
				// Même chose, mais dans un noeud qui n'est pas à la racine:
				else if ((tmpNode != null) && (tmpNode.Nodes.ContainsKey(path[i]))) { tmpNode = tmpNode.Nodes[path[i]]; }
				// Sinon, retourne null:
				else { return null; }
				// Développe le noeud, en déclenchant un événement:
				if (this.AddChildrenWhenExpand)
				{
					TreeViewCancelEventArgs eventArgs = new TreeViewCancelEventArgs(tmpNode, false, TreeViewAction.Expand);
					this.ExdTreeView_BeforeExpandAndCollapseForAddChildrenWhenExpand(this, eventArgs);
					// Si annulation, sort:
					if (eventArgs.Cancel == true) { return null; }
				}
				tmpNode.Expand();
			}

			// On retourne le noeud, en cherchant soit dans TV, soit le noeud tmpNode:
			if (tmpNode == null) { if (this.Nodes.ContainsKey(path[path.Length - 1])) { return this.Nodes[path[path.Length - 1]]; } }
			else { if (tmpNode.Nodes.ContainsKey(path[path.Length - 1])) { return tmpNode.Nodes[path[path.Length - 1]]; } }
			return null;

		}





		public TreeNode ShowNode(string path, string key)
		{
			return this.ShowNode(((path == null) ? null : path.Split('/')), key);
		}





		public TreeNode ShowNode(string path)
		{
			return this.ShowNode((path == null) ? null : path.Split('/'));
		}







	



		#endregion METHODES PUBLIQUES










		// ---------------------------------------------------------------------------
		// GESTIONNAIRES D'EVENEMENTS
		// ---------------------------------------------------------------------------




		#region GESTIONNAIRES D'EVENEMENTS






		/// <summary>
		/// Si l'utilisateur a double-cliqué sur un document, déclenche l'événement DocumentSelected.
		/// </summary>
		protected virtual void ExdTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				// Sélectionne le noeud, puis regarde si c'est un document, auquel cas déclenche l'événement DocumentSelected:
				if (this.SelectedNode != e.Node) { this.SelectedNode = e.Node; }
				bool isFolder;
				if ((GetIsFolder(e.Node, out isFolder)) && (isFolder == false)) { OnDocumentSelected(); }
			}

		}






		// ---------------------------------------------------------------------------
	





		/// <summary>
		/// Vérifie si le noeud sélectionné correspond à celui gardé en mémoire (champ _savedSelectedNode), et si non déclenche l'événement NodeSelected et modifie le champ.
		/// </summary>
		protected void ExdTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (this.SelectedNode != e.Node) { this.SelectedNode = e.Node; }
			if (_savedSelectedNode != this.SelectedNode) { OnNodeSelected(); _savedSelectedNode = this.SelectedNode; }
		}






		#endregion GESTIONNAIRES D'EVENEMENTS












		// ---------------------------------------------------------------------------
		// DECLENCHEMENTS D'EVENEMENTS
		// ---------------------------------------------------------------------------




		#region DECLENCHEMENTS D'EVENEMENTS



		/// <summary>
		/// Lance l'événement DocumentSelected, seulement si SelectedNode n'est pas nul.
		/// </summary>
		protected virtual void OnDocumentSelected()
		{
			if ((DocumentSelected != null) && (this.SelectedNode != null))
			{
				DocumentSelectedEventArgs args = new DocumentSelectedEventArgs();
				args.Id = ((ExdTreeNode)this.SelectedNode).IdDocument;
				args.Node = this.SelectedNode;
				DocumentSelected(this, args);
			}
		}






		// ---------------------------------------------------------------------------
	




		/// <summary>
		/// Déclenche l'événement NodeSelected, en se basant sur _savedSelectedNode et this.SelectedNode.
		/// </summary>
		protected void OnNodeSelected()
		{
			if (NodeSelected != null)
				{ this.NodeSelected(this, new NodeSelectedEventArgs(_savedSelectedNode, this.SelectedNode)); }
		}





		#endregion DECLENCHEMENTS D'EVENEMENTS
	












		#endregion ASPECTS GENERAUX




















		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// METHODES D'OPERATIONS SUR LES NOEUDS (AJOUT, SELECTION, RETOUR, ETC.)
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+







		#region METHODES D'OPERATIONS SUR LES NOEUDS (AJOUT, SELECTION, RETOUR, ETC.)








		/// <summary>
		/// Retourne tous les parents d'un noeud, à l'exclusion du noeud spécifié. Retourne null si le noeud est à la racine de l'arbre.
		/// </summary>
		public TreeNode[] ParentsOfNode(TreeNode node)
		{
			if (node.Level == 0) { return null; }
			TreeNode[] result = new TreeNode[node.Level];
			for (int i = node.Level - 1; i >= 0; i--)
			{
				if (i == node.Level - 1) { result[i] = node.Parent; }
				else { result[i] = result[i + 1].Parent; }
			}
			return result;
		}







		// ---------------------------------------------------------------------------
	






		/// <summary>
		/// Retourne un tableau de noeuds qui correspondent à l'id (NodeTreeType.Id) passé. Retourne un tableau vide si rien n'est trouvé.
		/// </summary>
		public ExdTreeNode[] GetNodesById(int id)
		{
			
			// Parcours tous les noeuds, et remplit un tableau de 10 éléments, qui grandit de 10 éléments au besoin.
			ExdTreeNode[] arr = new ExdTreeNode[10];
			int j = 0;
			// Pour tous les noeuds de l'arbre:
			this.ActionOnAllChildren(
				delegate (TreeNode node)
				{
					// Si c'est un ExdTreeNode...
					if (node is ExdTreeNode)
					{
						// Si c'est le bon id...
						if (((ExdTreeNode)node).Id == id)
						{
							// S'il n'y a plus assez de place dans le tableau, agrandit de 10:
							if (j >= arr.Length) { Array.Resize(ref arr, arr.Length + 10); }
							// Ajoute au tableau:
							arr[j++] = (ExdTreeNode)node;
						}
					}
				}
			);
			// Retourne null si aucun élément trouvé.
			if ((j == 0) && (arr[0] == null)) { return new ExdTreeNode[0]; }
			// Retourne le tableau après l'avoir retaillé:
			Array.Resize(ref arr, j);
			return arr;

		}








		// ---------------------------------------------------------------------------
	









		/// <summary>
		/// Retourne un tableau de noeuds qui correspondent à l'idDoc (NodeTreeType.IdDocument) passé. Retourne un tableau vide si rien n'est trouvé.
		/// </summary>
		public ExdTreeNode[] GetNodesByIdDoc(int idDoc)
		{
			
			// Parcours tous les noeuds, et remplit un tableau de 10 éléments, qui grandit de 10 éléments au besoin.
			ExdTreeNode[] arr = new ExdTreeNode[10];
			int j = 0;
			// Pour tous les noeuds de l'arbre:
			this.ActionOnAllChildren(
				delegate (TreeNode node)
				{
					// Si c'est un ExdTreeNode...
					if (node is ExdTreeNode)
					{
						// Si c'est le bon id...
						if (((ExdTreeNode)node).IdDocument == idDoc)
						{
							// S'il n'y a plus assez de place dans le tableau, agrandit de 10:
							if (j >= arr.Length) { Array.Resize(ref arr, arr.Length + 10); }
							// Ajoute au tableau:
							arr[j++] = (ExdTreeNode)node;
						}
					}
				}
			);
			// Retourne null si aucun élément trouvé.
			if ((j == 0) && (arr[0] == null)) { return new ExdTreeNode[0]; }
			// Retourne le tableau après l'avoir retaillé:
			Array.Resize(ref arr, j);
			return arr;

		}






		// ---------------------------------------------------------------------------
	







		/// <summary>
		/// Retourne un tableau d'id correspondant aux id des parents du noeud passé. Retourne null si l'un des noeuds parents n'est pas un ExdTreeNode, ou si checkForNonValidId=true et l'un des id est inférieur à 0, ou si le noeud est un noeud racine.
		/// </summary>
		/// <param name="node">Noeud dont il faut chercher les parents (ce noeud n'est pas inclu dans le tableau).</param>
		/// <param name="checkForNonValidId">Vérifie que l'id est supérieur à 0 (par défaut, les ExdTreeNode ont des id inférieurs à 0), sinon retourne null.</param>
		public int[] ParentsIdOfNode(ExdTreeNode node, bool checkForNonValidId)
		{
		
			// Récupère la liste des noeuds parents, et cherchent leur id.
			
			if (node.Level == 0) { return null; }
			TreeNode[] parents = this.ParentsOfNode(node);
			int[] result = new int[parents.Length];
			for (int i=0; i<parents.Length; i++)
			{
				if (!(parents[i] is ExdTreeNode)) { return null; }
				result[i] = ((ExdTreeNode)parents[i]).Id;
				if ((checkForNonValidId) && (((ExdTreeNode)parents[i]).Id < 0)) { return null; }
			}
			return result;
		
		}










		// ---------------------------------------------------------------------------
	












		/// <summary>
		/// Retourne le noeud correspondant au path. Retourne null si pas trouvé.
		/// </summary>
		/// <param name="path">Indique le chemin d'accès des noeuds, i.e. l'ensemble des parents pour accéder au noeud que l'on veut affiché. Si path est de type int[], les Id des noeuds sont utilisés ; si path est de type string ou string[], ce sont les Key qui sont utilisé. L'élément du tableau qui a l'index 0 représente le noeud racine de départ. Si path est de type string, le séparateur doit être "/".</param>
		public TreeNode GetNode(string[] path)
		{

			// Retourne null si path == null:
			if ((path == null) || (path.Length == 0)) { return null; }

			// Noeud temporaire:
			TreeNode tmpNode = null;

			// Boucle sur le tableau, et descend dans la hiérarchie. En cas d'erreur, retourne null:
			for (int i = 0; i < path.Length - 1; i++)
			{

				// Si 1er élément du tableau et si le noeud existe, le prend comme tmpNode:
				if ((i == 0) && (this.Nodes.ContainsKey(path[i]))) { tmpNode = this.Nodes[path[i]]; }
				// Même chose, mais dans un noeud qui n'est pas à la racine:
				else if ((tmpNode != null) && (tmpNode.Nodes.ContainsKey(path[i]))) { tmpNode = tmpNode.Nodes[path[i]]; }
				// Sinon, retourne null:
				else { return null; }
			}
			
			// Il ne reste plus qu'à renvoyé le noeud, s'il existe:
			if (tmpNode == null) { if (this.Nodes.ContainsKey(path[path.Length - 1])) { return this.Nodes[path[path.Length - 1]]; } }
			else { if (tmpNode.Nodes.ContainsKey(path[path.Length - 1])) { return tmpNode.Nodes[path[path.Length - 1]]; } }
			return null;
		}





		public TreeNode GetNode(string[] path, string key)
		{
			if ((path == null) || (path.Length == 0))
			{
				if (this.Nodes.ContainsKey(key)) { return this.Nodes[key]; }
				else { return null; }
			}
			return GetNode(((IEnumerable<string>)path.Concat(new string[] { key })).ToArray<string>());
		}





		public TreeNode GetNode(string path, string key)
		{
			return GetNode(((path == null) ? null : path.Split('/')), key); ;
		}





		public TreeNode GetNode(string path)
		{
			return GetNode(((path == null) ? null : path.Split('/'))); ;
		}





		public ExdTreeNode GetNode(int[] path, int id)
		{
			if ((path == null) || (path.Length == 0)) { return GetNode(this, id); }
			return GetNode(((IEnumerable<int>)path.Concat(new int[] { id })).ToArray<int>());
		}





		public ExdTreeNode GetNode(int[] path)
		{

			// Retourne null si path == null:
			if ((path == null) || (path.Length == 0)) { return null; }

			// Noeud temporaire:
			TreeNode tmpNode = null; TreeNode tmp = null;

			// Boucle sur le tableau, et descend dans la hiérarchie. En cas d'erreur, retourne null:
			for (int i = 0; i < path.Length - 1; i++)
			{
				// Si 1er élément du tableau et si le noeud existe, le prend comme tmpNode:
				if ((i == 0) && ((tmp = this.GetNode(this, path[i])) != null)) { tmpNode = tmp; }
				// Même chose, mais dans un noeud qui n'est pas à la racine:
				else if ((tmp = this.GetNode(tmpNode, path[i])) != null) { tmpNode = tmp; }
				// Sinon, retourne null:
				else { return null; }
			}

			// Il ne reste plus qu'à renvoyer le noeud, s'il existe (et en cherchant soit dans le TV, soit dans un noeud):
			if (tmpNode == null) { tmp = this.GetNode(this, path[path.Length - 1]); }
			else { tmp = this.GetNode(tmpNode, path[path.Length - 1]); }
			return ((tmp == null) ? (null) : (ExdTreeNode)tmp);

		}





		public ExdTreeNode GetNode(TreeNode parent, int id)
		{
			// Parcourt les noeuds, jusqu'à trouver le bon:
			if (parent == null) { return null; }
			foreach (TreeNode i in parent.Nodes) { if ((i is ExdTreeNode) && (((ExdTreeNode)i).Id == id)) { return (ExdTreeNode)i; } }
			return null;
		}





		public ExdTreeNode GetNode(TreeView parent, int id)
		{
			// Parcourt les noeuds, jusqu'à trouver le bon:
			if (parent == null) { parent = this; }
			foreach (TreeNode i in parent.Nodes) { if ((i is ExdTreeNode) && (((ExdTreeNode)i).Id == id)) { return (ExdTreeNode)i; } }
			return null;
		}







		// ---------------------------------------------------------------------------
	







		/// <summary>
		/// Retourne le noeud ayant l'IdDocument spécifié, en cherchant dans les enfants (mais pas dans les peitts-enfants) du noeud parent passé en argument. Retourne null si pas trouvé.
		/// </summary>
		public ExdTreeNode GetNodeByIdDoc(TreeNode parent, int idDoc)
		{
			// Parcourt les noeuds, jusqu'à trouver le bon:
			if (parent == null) { return null; }
			foreach (TreeNode i in parent.Nodes)
			{
				bool isFolder;
				if ((i is ExdTreeNode) && (GetIsFolder(i, out isFolder)) && (isFolder == false) && (((ExdTreeNode)i).IdDocument == idDoc))
				{ return (ExdTreeNode)i; }
			}
			return null;
		}




		public ExdTreeNode GetNodeByIdDoc(TreeView parent, int idDoc)
		{
			// Parcourt les noeuds, jusqu'à trouver le bon:
			if (parent == null) { parent = this; }
			foreach (TreeNode i in parent.Nodes)
			{
				bool isFolder;
				if ((i is ExdTreeNode) && (GetIsFolder(i, out isFolder)) && (isFolder == false) && (((ExdTreeNode)i).IdDocument == idDoc))
				{ return (ExdTreeNode)i; }
			}
			return null;
		}









		// ---------------------------------------------------------------------------
	







		/// <summary>
		/// Vérifie si le noeud spécifié se trouve parmi les enfants de du noeud parent (ou parmi les noeuds racines de l'arbre), selon l'Id du noeud ou selon le "key" du noeud. Dans tous les cas, si parent est null, prend l'arbre (this) comme parent.
		/// </summary>
		public bool NodeExists(TreeNode parent, int id)
		{
			// Regarde si on a bien quelque chose quand on demande le noeud...
			if (parent == null) { return (this.GetNode(this, id) != null); }
			return (this.GetNode(parent, id) != null);
		}





		public bool NodeExists(TreeView parent, int id)
		{
			// Regarde si on a bien quelque chose quand on demande le noeud...
			if (parent == null) { parent = this; }
			return (this.GetNode(parent, id) != null);
		}





		public bool NodeExists(TreeNode parent, string key)
		{
			if (parent == null) { return NodeExists(this, key); }
			return parent.Nodes.ContainsKey(key);
		}





		public bool NodeExists(TreeView parent, string key)
		{
			if (parent == null) { parent = this; }
			return parent.Nodes.ContainsKey(key);
		}








		// ---------------------------------------------------------------------------
	






		/// <summary>
		/// Vérifie si le noeud spécifié se trouve parmi les enfants de du noeud parent (ou parmi les noeuds racines de l'arbre), selon l'IdDocument du noeud. Si parent est null, prend l'arbre (this) comme parent.
		/// </summary>
		public bool NodeExistsByIdDoc(TreeNode parent, int idDoc)
		{
			// Regarde si on a bien quelque chose quand on demande le noeud...
			if (parent == null) { return (this.GetNodeByIdDoc(this, idDoc) != null); }
			return (this.GetNodeByIdDoc(parent, idDoc) != null);
		}









		// ---------------------------------------------------------------------------
	






		/// <summary>
		/// Ajoute un noeud au parent spécifié, ou à l'emplacement (path) spécifié. Si le noeud existe déjà (selon la surcharge, si un noeud de même Id, ou de même "key" existe déjà), n'insère rien et retourne null. Si le noeud a été inséré, retourne le noeud en question. Insère toujours un ExdTreeNode si le path est de forme int[] ou quand un ExdTreeNode est passé dans l'argument parent, mais insère un TreeNode pour les surcharges dont le path est en string ou en string[]. Lorsqu'un ExdTreeNode est insérer, l'image du noeud est définie automatiquement, et si la propriété HasChildren vaut true, un noeud "factice" si la propriété de l'arbre AddChildrenWhenExpand vaut true. Bref, le noeud est automatiquement mis en forme en fonction du type et de ses propriétés.
		/// </summary>
		public ExdTreeNode AddTreeNode(ExdTreeNode parent, ExdTreeNode node)
		{
			return this.AddTreeNode(parent, node, -1);
		}





		public ExdTreeNode AddTreeNode(ExdTreeNode parent, ExdTreeNode node, int index)
		{

			// Insère à la racine si parent est null, ou retourne null si existe déjà:
			if (parent == null)
			{
				if (this.NodeExists(this, node.Id)) { return null; }
				if (index < 0) { this.Nodes.Add(node); }
				else { this.Nodes.Insert(index, node); }
			}

			// Sinon insère dans le noeud parent:
			else
			{
				if (this.NodeExists(parent, node.Id)) { return null; }
				if (index < 0) { parent.Nodes.Add(node); }
				else { parent.Nodes.Insert(index, node); }
			}
			
			// Définit l'image du noeud:
			this.SetImageKey(node);
			
			// Définit si le noeud a des enfants, et si oui insère un noeud factice:
			if ((node.FolderHasChildren) && this.AddChildrenWhenExpand) { this.SimulateChildren(node); }
			
			return node;

		}





		public ExdTreeNode AddTreeNode(ExdTreeNode parent, string text, int id, int nodeType, int index)
		{

			// Créer le noeud:
			ExdTreeNode node = new ExdTreeNode();
			node.Text = text;
			node.Id = id;
			node.NodeType = nodeType;
						
			// Insère le noeud:
			return this.AddTreeNode(parent, node, index);

		}





		public ExdTreeNode AddTreeNode(int[] path, ExdTreeNode node)
		{
			// Vérifie que le noeud du path existe:
			TreeNode parent;
			if ((parent = this.GetNode(path)) == null) { return null; }
			return this.AddTreeNode(parent as ExdTreeNode, node);
		}





		public TreeNode AddTreeNode(string[] path, string key, TreeNode node)
		{

			// Si path est null, insère à la racine, ou retourne null si existe déjà:
			if ((path == null) || (path.Length == 0))
			{
				if (this.Nodes.ContainsKey(key)) { return null; }
				this.Nodes.Add(node);
				return node;
			}
			
			// Vérifie que le noeud du path existe:
			TreeNode parent;
			if ((parent = this.GetNode(path)) == null) { return null; }

			// Si le noeud parent existe, insère l'enfant ou retourne null si existe déjà:
			if (parent.Nodes.ContainsKey(key)) { return null; }
			parent.Nodes.Add(node);
			return node;

		}





		public TreeNode AddTreeNode(string[] path, int index, string key, string text)
		{
		
			// Si path est null, insère à la racine, ou retourne null si existe déjà:
			if ((path == null) || (path.Length == 0))
			{
				if (this.Nodes.ContainsKey(key)) { return null; }
				return this.Nodes.Insert(index, key, text);
			}

			// Vérifie que le noeud du path existe:
			TreeNode parent;
			if ((parent = this.GetNode(path)) == null) { return null; }

			// Si le noeud parent existe, insère l'enfant ou retourne null si existe déjà:
			if (parent.Nodes.ContainsKey(key)) { return null; }
			return parent.Nodes.Insert(index, key, text);
			
		}





		public TreeNode AddTreeNode(string path, int index, string key, string text)
		{
			return AddTreeNode(((path == null) ? null : path.Split('/')), index, key, text);
		}





		public TreeNode AddTreeNode(string path, string key, TreeNode node)
		{
			return AddTreeNode(((path == null) ? null : path.Split('/')), key, node);
		}








		// ---------------------------------------------------------------------------
	




		
		
		





		/// <summary>
		/// Retourne un tableau contenant les enfants (et sous-enfants éventuellement) du noeud. Le noeud passé en argument n'est pas inclus dans le tableau de retour. grandChildren indique s'il faut inclure les petits-enfants (true par défaut). Retourne null si node est null, ou si le noeud ne contient aucun enfants.
		/// </summary>
		public TreeNode[] GetChildrenOf(TreeNode node, bool grandChildren)
		{
		
			if ((node == null) || (node.GetNodeCount(grandChildren) == 0)) { return null; }
			TreeNode[] arr = new TreeNode[node.GetNodeCount(grandChildren)];
			int i = 0;
			this.ActionOnAllChildren(node, delegate(TreeNode treeNode) { arr[i++] = treeNode; });
			return arr;
		
		}





		public TreeNode[] GetChildrenOf(TreeNode node)
		{
			return this.GetChildrenOf(node, true);
		}




		#endregion METHODES D'OPERATIONS SUR LES NOEUDS (AJOUT, SELECTION, RETOUR, ETC.)




















		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// ADDCHILDRENWHENEXPAND
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+




		#region ADDCHILDRENWHENEXPAND







		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS
		
		
		
		
		// Champs privés:
		
		private bool _addChildrenWhenExpand;
		
		
		// Evénements:
		
		/// <summary>
		/// Evénement quand un noeud est un déplié et que la propriété AddChildrenWhenExpand vaut true. Autrement dit, cet événement attent qu'on insère les noeuds dont le parent correspond au noeud passé par l'argument d'événement.
		/// </summary>
		public event TreeViewCancelEventHandler ExpandForAddingChildren;
		



		#endregion DECLARATIONS
	











		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES





		/// <summary>
		/// Obtient ou définit si les noeuds enfants doivent être ajoutés au moment où le noeud est développé. En attedant, les enfants des noeuds repliés sont simulés par la présence d'un noeud "factice", supprimé lorsque le noeud est déplié, et destiné à faire apparaître un "+" à gauche du noeud, offrant ainsi la possibilité à l'utilisateur de déplier le noeud. Si cette propriété vaut true, l'événement ExpandForAddingChildren est déclenché quand un noeud est déplié et qu'il faut ajouter les noeuds enfants de ce noeud déplié.
		/// </summary>
		public bool AddChildrenWhenExpand
		{
			get { return _addChildrenWhenExpand; }
			set
			{
				if (value)
				{
					this.BeforeExpand -= new TreeViewCancelEventHandler(ExdTreeView_BeforeExpandAndCollapseForAddChildrenWhenExpand);
					this.BeforeExpand += new TreeViewCancelEventHandler(ExdTreeView_BeforeExpandAndCollapseForAddChildrenWhenExpand);
					this.BeforeCollapse -= new TreeViewCancelEventHandler(ExdTreeView_BeforeExpandAndCollapseForAddChildrenWhenExpand);
					this.BeforeCollapse += new TreeViewCancelEventHandler(ExdTreeView_BeforeExpandAndCollapseForAddChildrenWhenExpand);
					_addChildrenWhenExpand = value;
				}
				else
				{
					this.BeforeExpand -= new TreeViewCancelEventHandler(ExdTreeView_BeforeExpandAndCollapseForAddChildrenWhenExpand);
					this.BeforeCollapse -= new TreeViewCancelEventHandler(ExdTreeView_BeforeExpandAndCollapseForAddChildrenWhenExpand);
					_addChildrenWhenExpand = value;
				}
			}
		}


		
		
		
		
		
		
		/// <summary>
		/// Obtient ou définit si les noeuds enfants sont supprimés lorsque le noeud est replié. Utilisé seulement seulement si AddChildrenWhenExpand==true. Quand un noeud est replié et qu'il contenait des enfants, ceux-ci sont supprimé (avec tous les petits-enfants) et un noeud "factice" est placé dans le noeud afin de simuler les enfants disparus. Ceci permet des mises à jour de l'arbre plus rapide, par exemple lorsqu'il s'agit de mettre en forme certains noeuds.
		/// </summary>
		public bool RemoveChildrenWhenCollapse { get; set; }
		
		
		
		
		
		/// <summary>
		/// Le noeud factice inséré pour simuler les enfants est un TreeNode simple (ce qui permet de l'éliminer lors d'actions effectuées sur les ExdTreeNode par un simple contrôle (if (node is ExdTreeNode)...). Ce noeud factice a une "key" prédéfinie, qu'il est possible de personnaliser ici, bien que ce ne soit généralemnet pas utile.
		/// </summary>
		public string SystemKeyNodeForAddChildrenWhenExpand { get; set; }
		



		#endregion PROPRIETES











		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------
		
		
		
		
		#region METHODES
	
	
	
	
	
		/// <summary>
		/// Insère un noeud factice en tant qu'enfant du noeud passé en argument. Ce noeud est un TreeNode simple, qui a pour "key" la valeur de SystemKeyForAddChildrenWhenExpand.
		/// </summary>
		protected void SimulateChildren(TreeNode treeNode)
		{
			treeNode.Nodes.Add(this.SystemKeyNodeForAddChildrenWhenExpand, string.Empty);
		}


	
	
	
	
		
		
		#endregion METHODES
	






		// ---------------------------------------------------------------------------
		// GESTIONNAIRES D'EVENEMENTS
		// ---------------------------------------------------------------------------




		#region GESTIONNAIRES D'EVENEMENTS





		/// <summary>
		/// Gestionnaire d'événément quand un noeud est replié ou déplié et que AddChildrenWhenExpand est actif (vaut true). Quand le noeud est replié et que RemoveChildrenWhenCollapse vaut true, cette méthode supprime les noeuds enfants et ajoute un noeud factice pour les simuler. Quand le noeud est déplié, si un noeud factice est trouvé, cette méthode déclenche l'événement ExpandForAddingChildren après avoir supprimé le noeud factice. Le gestionnaire de cet événement ExpandForAddingChildren est censé insérer les noeuds demandé dans le noeud parent passé en argument. Si l'argument d'événement Cancel vaut true, alors l'opération est annulée : Il est donc possible que le gestion d'événement annuler l'opération en cas d'erreur.
		/// </summary>
		private void ExdTreeView_BeforeExpandAndCollapseForAddChildrenWhenExpand(object sender, TreeViewCancelEventArgs e)
		{
		
			// Si le noeud est replié, et va être déplié, supprime le noeud système, s'il existe, puis déclenche un événement:
			if (e.Node.IsExpanded == false)
			{
				if (e.Node.Nodes.ContainsKey(SystemKeyNodeForAddChildrenWhenExpand))
				{
					e.Node.Nodes.RemoveByKey(SystemKeyNodeForAddChildrenWhenExpand);
					TreeViewCancelEventArgs eventArgs = new TreeViewCancelEventArgs(e.Node, false, TreeViewAction.Expand);
					if (ExpandForAddingChildren != null) { ExpandForAddingChildren(this, eventArgs); }
					// Annulation possible:
					e.Cancel = eventArgs.Cancel;
				}
			}
			
			// Sinon, et si on doit supprimer les enfants quand le noeud se replie, on supprime lesdits enfants et on rajoute un noeud système:
			else if (RemoveChildrenWhenCollapse)
			{
				ActionOnAllChildren(e.Node, delegate(TreeNode node) { node.Remove(); }, false, true);
				e.Node.Nodes.Add(SystemKeyNodeForAddChildrenWhenExpand, string.Empty);
			}
			
		}








		#endregion GESTIONNAIRES D'EVENEMENTS
	












		#endregion ADDCHILDRENWHENEXPAND















		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// CHECKBOXES
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+




		#region CHECKBOXES
		
		
		
		
		private bool _singleCheck;
		private bool _coherentCheck;
		private bool _allowCheckOnlyByProgram;
		
		



		/// <summary>
		/// Obtient ou définit si plusieurs éléments peuvent être cochés. Si true alors CoherentCheck = false.
		/// </summary>
		public bool SingleCheck
		{
			get { return _singleCheck; }
			set { _singleCheck = value; if (value) _coherentCheck = false; }
		}




		/// <summary>
		/// Obtient ou définit si les coches se font de façon cohérente les noeuds: Si un noeud est coché, alors tous ces enfants sont cochés, et vice-versa (si tous les noeuds d'un parent sont cochés, alors ce parent est coché). Ne fonctionne pas si le drag and drop est actif, ou si AddChildrenWhenExpand est actif. Si true alors SingleCheck = false.
		/// </summary>
		public bool CoherentCheck
		{
			get { return _coherentCheck; }
			set { _coherentCheck = value; if (value) _singleCheck = false; }
		}







		/// <summary>
		/// Obtient ou définit si les noeuds ne peuvent être cochés que par le programme, par par l'utilisateur.
		/// </summary>
		public bool AllowCheckOnlyByProgram
		{
			get { return _allowCheckOnlyByProgram; }
			set
			{
				if (value)
				{
					this.BeforeCheck -= new TreeViewCancelEventHandler(ExdTreeView_BeforeCheck_ForAllowCheckOnlyByProgram);
					this.BeforeCheck += new TreeViewCancelEventHandler(ExdTreeView_BeforeCheck_ForAllowCheckOnlyByProgram);
					_allowCheckOnlyByProgram = value;
				}
				else
				{
					this.BeforeCheck -= new TreeViewCancelEventHandler(ExdTreeView_BeforeCheck_ForAllowCheckOnlyByProgram);
					_allowCheckOnlyByProgram = value;
				}
			}
		}







		// ---------------------------------------------------------------------------
	




		/// <summary>
		/// Obtient la liste des noeuds cochés (que les checkboxes soient affichées ou non). Retourne un tableau vide si rien n'est coché.
		/// </summary>
		public TreeNode[] CheckedNodes()
		{
		
			// Parcours tous les noeuds, et remplit un tableau de 10 éléments, qui grandit de 10 éléments au besoin.
			TreeNode[] arr = new TreeNode[10];
			int j = 0;
			// Pour tous les noeuds de l'arbre:
			this.ActionOnAllChildren(
				delegate(TreeNode node)
				{
					// Si coché:
					if (node.Checked)
					{
						// S'il n'y a plus assez de place dans le tableau, agrandit de 10:
						if (j >= arr.Length) { Array.Resize(ref arr, arr.Length + 10); }
						// Ajoute au tableau:
						arr[j++] = node;
					}
				}
			);
			// Retourne null si aucun élément trouvé.
			if ((j == 0) && (arr[0] == null)) { return new TreeNode[0]; }
			// Elimine toutes les cellules en trop (celle qui n'ont pas été remplies et qui sont donc à null), et retourne le résultat:
			Array.Resize(ref arr, j);
			return arr;

		}








		// ---------------------------------------------------------------------------
	






		/// <summary>
		/// Exécute une action sur tous les noeuds cochés.
		/// </summary>
		public void ActionOnCheckedNodes(Action<TreeNode> action)
		{

			// Pour tous les noeuds cochés...
			TreeNode[] arr = CheckedNodes();
			foreach (TreeNode i in arr) { action(i); }

		}









		// ---------------------------------------------------------------------------
	






		/// <summary>
		/// Gestionnaire d'événement pour SingleCheck et CoherentCheck.
		/// </summary>
		protected virtual void ExdTreeView_AfterCheck_ForCkeckBoxes(object sender, TreeViewEventArgs e)
		{
		
			// SINGLE CHECK:
			
			// Parcourt tous les noeuds cochés, efface la coche, et coche l'item courant. Désactive l'événement pour éviter un appel récursif:
			if (SingleCheck)
			{
				this.AfterCheck -= ExdTreeView_AfterCheck_ForCkeckBoxes;
					// Autre option: //if (e.Action == TreeViewAction.Unknown) return;
				bool isChecked = e.Node.Checked;
				TreeNode[] nodes = CheckedNodes();
				foreach (TreeNode i in nodes) { i.Checked = false; }
				e.Node.Checked = isChecked;
				this.AfterCheck += new TreeViewEventHandler(ExdTreeView_AfterCheck_ForCkeckBoxes);
			}
			
			
			
			// COHERENT CHECK:
			
			// Parcourt tous les noeuds enfants, coche, puis s'occupe des parents. Désactive l'événement pour éviter un appel récursif:
			if (CoherentCheck)
			{
				this.SuspendLayout();
				this.AfterCheck -= ExdTreeView_AfterCheck_ForCkeckBoxes;
				this.ActionOnAllChildren(e.Node, delegate(TreeNode node) { node.Checked = e.Node.Checked; }	);
				
				TreeNode[] parents = ParentsOfNode(e.Node);
				if (parents != null)
				{
					Action<TreeNode> deleg;
					for (int i=parents.Count()-1; i>=0; i--)
					{
						parents[i].Checked = true;
						deleg = delegate(TreeNode node) { if (!node.Checked) { node.Parent.Checked = false; } };
						ActionOnAllChildren(parents[i], deleg, false, false);
					}
				}
				this.AfterCheck += new TreeViewEventHandler(ExdTreeView_AfterCheck_ForCkeckBoxes);
				this.ResumeLayout(false);
			}
			
		}
		
		
		
		
		
		/// <summary>
		/// Gestionnaire d'événément pour CheckOnlyByProgram.
		/// </summary>
		protected virtual void ExdTreeView_BeforeCheck_ForAllowCheckOnlyByProgram(object sender, TreeViewCancelEventArgs e)
		{
			if (e.Action != TreeViewAction.Unknown) { e.Cancel = true; }
		}

		
		
		



		#endregion CHECKBOXES








		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// METHODES POUR LA GESTION DES TYPES DE NOEUDS
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+




		#region METHODES POUR LA GESTION DES TYPES DE NOEUDS






		/// <summary>
		/// Retourne le type du document, soit directement (ou une valeur par défaut si le type n'existe pas), soit indirectement en paramètre de sortie, la fonction retournant alors true si le type existe ou false s'il n'existe pas.
		/// </summary>
		/// <param name="nodeTypeId"></param>
		/// <returns></returns>
		public ExdTreeNode.TreeNodeType GetNodeType(int nodeTypeId)
		{

			ExdTreeNode.TreeNodeType nodeType = NodeTypesList.LastOrDefault<ExdTreeNode.TreeNodeType>(i => i.Id == nodeTypeId);
			return nodeType;
		
		}





		public bool GetNodeType(int nodeTypeId, out ExdTreeNode.TreeNodeType result)
		{

			result = this.GetNodeType(nodeTypeId);
			if (result == null) { return false; }
			return true;
		
		}






		// ---------------------------------------------------------------------------
	




		/// <summary>
		/// Retourne true si la fonction a réussit, c'est-à-dire si l'argument TreeNode n'est pas null, s'il est de type ExdTreeNode et pas seulement de type TreeNode, s'il a un type valide, etc., et retourne false dans le cas contraire. Si true, alors le paramètre de sortie result peut-être examiné.
		/// </summary>
		public bool GetIsFolder(TreeNode treeNode, out bool result)
		{
			
			// Valide le treeNode, sort si besion, puis lit le champ:
			result = false;
			ExdTreeNode.TreeNodeType type;
			if (!ValidateTreeNode(treeNode, out type)) { return false; }
			result = type.IsFolder;
			return true;
		}



		// ---------------------------------------------------------------------------




		/// <summary>
		/// Retourne true si la fonction a réussit, c'est-à-dire si l'argument TreeNode n'est pas null, s'il est de type ExdTreeNode et pas seulement de type TreeNode, s'il a un type valide, etc., et retourne false dans le cas contraire. Si true, alors le paramètre de sortie result peut-être examiné.
		/// </summary>
		public bool GetImageKey(TreeNode treeNode, out string result)
		{
			// Valide le treeNode, sort si besion, puis lit le champ:
			result = null;
			ExdTreeNode.TreeNodeType type;
			if (!ValidateTreeNode(treeNode, out type)) { return false; }
			result = type.ImageKey;
			return true;
		}


		// ---------------------------------------------------------------------------




		/// <summary>
		/// Retourne true si la fonction a réussit, c'est-à-dire si l'argument TreeNode n'est pas null, s'il est de type ExdTreeNode et pas seulement de type TreeNode, s'il a un type valide, etc., et retourne false dans le cas contraire. Si true, alors le paramètre de sortie result peut-être examiné.
		/// </summary>
		public bool GetContextMenu(TreeNode treeNode, out ContextMenu result)
		{
			// Valide le treeNode, sort si besion, puis lit le champ:
			result = null;
			ExdTreeNode.TreeNodeType type;
			if (!ValidateTreeNode(treeNode, out type)) { return false; }
			result = type.ContextMenu;
			return true;
		}


		// ---------------------------------------------------------------------------





		/// <summary>
		/// Retourne true si la fonction a réussit, c'est-à-dire si l'argument TreeNode n'est pas null, s'il est de type ExdTreeNode et pas seulement de type TreeNode, s'il a un type valide, etc., et retourne false dans le cas contraire. Si true, alors le paramètre de sortie result peut-être examiné.
		/// </summary>
		public bool GetDescription(TreeNode treeNode, out string result)
		{
			// Valide le treeNode, sort si besion, puis lit le champ:
			result = null;
			ExdTreeNode.TreeNodeType type;
			if (!ValidateTreeNode(treeNode, out type)) { return false; }
			result = type.Description;
			return true;
		}



		// ---------------------------------------------------------------------------





		/// <summary>
		/// Retourne true si si l'argument TreeNode n'est pas null, s'il est de type ExdTreeNode et pas seulement de type TreeNode, s'il a un type valide, etc., et retourne false dans le cas contraire.
		/// </summary>
		private bool ValidateTreeNode(TreeNode treeNode, out ExdTreeNode.TreeNodeType nodeType)
		{

			// Sort si pas un ExdTreeNode, si un type n'est pas spécifié, si la liste des types est null, ou si pas le type spécifiée:
			nodeType = null;
			if (!(treeNode is ExdTreeNode)) { return false; }
			if (((ExdTreeNode)treeNode).NodeType < 0) { return false; }
			if ((NodeTypesList == null) || (NodeTypesList.Length == 0)) { return false; }
			nodeType = this.GetNodeType(((ExdTreeNode)treeNode).NodeType);
			return ((nodeType == null) ? false : true);

		}






		// ---------------------------------------------------------------------------
	
	

		/// <summary>
		/// Cherche l'ImageKey associé au type, et la place dans le treeNode afin de lui faire affiché l'image associé au type. Retourne true si réussi, false sinon (pas un ExdTreeNode, type non valide, etc.).
		/// </summary>
		public bool SetImageKey(TreeNode treeNode)
		{
			// Cherche l'image, et si échec, retourne false, sinon ajoute l'image au noeud:
			string imgKey;
			if (this.GetImageKey(treeNode, out imgKey) == false) { return false; }
			treeNode.ImageKey = imgKey;
			treeNode.SelectedImageKey = imgKey;
			treeNode.StateImageKey = imgKey;
			return true;
		}
		
		
		
		
		
		
		#endregion METHODES POUR LA GESTION DES TYPES DE NOEUDS
	

















	





		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// GESTION DU CLAVIER
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+




		#region GESTION DU CLAVIER






		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------
		
		
		
		
		#region DECLARATIONS
		
		
		
		
		private bool _neutralizeMultiplyKey;
		
		
		
		
		#endregion DECLARATIONS
	








		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES




		/// <summary>
		/// Obtient ou définit si la touche * est neutralisée (touche qui permet de déplier l'ensemble d'un arbre). Il en va de même pour la touche + quand le noeud est déjà déplié. La neutralisation de la touche * permet d'éviter que tout l'arbre ne soit déplié automatiquement, ce qui aurait des conséquences désastreuse si l'arbre est lié à une source de données contenant un grand nombre d'éléments.
		/// </summary>
		public bool NeutralizeMultiplyKey
		{
			get { return _neutralizeMultiplyKey; }
			set
			{
				if (value)
				{
					this.KeyDown -= new KeyEventHandler(ExdTreeView_KeyDownForNeutralizeMultiplyKey);
					this.KeyDown += new KeyEventHandler(ExdTreeView_KeyDownForNeutralizeMultiplyKey);
					_neutralizeMultiplyKey = value;
				}
				else
				{
					this.KeyDown -= new KeyEventHandler(ExdTreeView_KeyDownForNeutralizeMultiplyKey);
					_neutralizeMultiplyKey = value;
				}
			}
		}
		



		#endregion PROPRIETES






		// ---------------------------------------------------------------------------
		// EVENEMENTS
		// ---------------------------------------------------------------------------




		#region EVENEMENTS





		/// <summary>
		/// Gestionnaire d'événement pour NeutralizeMultiplyKey.
		/// </summary>
		protected virtual void ExdTreeView_KeyDownForNeutralizeMultiplyKey(object sender, KeyEventArgs e)
		{
			// Neutralise *
			if (e.KeyCode == Keys.Multiply) { e.Handled = true; }
			// Neutralise + quand le noeud est déjà déplié:
			if ((e.KeyCode == Keys.Add) && (this.SelectedNode.IsExpanded)) { e.Handled = true; }
		}






		// ---------------------------------------------------------------------------
	




		/// <summary>
		/// Gestion du clavier: Touche DEL (appelle des procédures par défaut des menus contextuels), événement DocumentSelected (si touche Enter).
		/// </summary>
		protected virtual void ExdTreeView_KeyDown(object sender, KeyEventArgs e)
		{
		
			// Détermine si dossier:
			bool isFolder; bool testOK;
			testOK = GetIsFolder(this.SelectedNode, out isFolder);


			// TOUCHE DEL
			if (e.KeyCode == Keys.Delete)
			{
				// Si le noeud est un dossier, appelle procédure correspondante pour le menu contextuel par défaut:
				if ((testOK) && (isFolder))
					{ DefaultFolderContextMenu_DeleteFolder_Click(this, new EventArgs()); }
				// Si document, appelle la procédure correspondante pour le menu contextuel par défaut:
				else
					{ DefaultDocContextMenu_DeleteDoc_Click(this, new EventArgs()); }
			}
			
			
			// TOUCHE ENTER
			else if (e.KeyCode == Keys.Enter)
			{
				// Si fichier, lance l'événement OnDocumentSelected:
				if ((testOK) && (isFolder == false)) { this.OnDocumentSelected(); }
			}

		}







		// ---------------------------------------------------------------------------
		
		
		
		
		/// <summary>
		/// Gestion du clavier : Evénement NodeSelected (si le noeud sélectionné a changé).
		/// </summary>
		protected void ExdTreeView_KeyUp(object sender, KeyEventArgs e)
		{
		
			// N'importe quel touche: Vérifie si le noeud sélectionné correspond à celui gardé en mémoire (champ _savedSelectedNode), et si non déclenche l'événement NodeSelected et modifie le champ.
			if (_savedSelectedNode != this.SelectedNode)
			{
				OnNodeSelected();
				_savedSelectedNode = this.SelectedNode;
			}

		}




		#endregion EVENEMENTS





		#endregion GESTION GESTION DU CLAVIER















		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// GESTION DES MENUS CONTEXTUELS
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+




		#region GESTION DES MENUS CONTEXTUELS





		// ---------------------------------------------------------------------------
		// DECLARATIONS DE VARIABLES ET D'EVENEMENTS POUR LE MENU CONTEXTUEL
		// ---------------------------------------------------------------------------




		#region DECLARATIONS DE VARIABLES ET D'EVENEMENTS POUR LE MENU CONTEXTUEL



		// Délégués:

		public delegate void DeleteNodeEventHandler(object sender, DeleteNodeEventArgs e);
		public delegate void NodeCancelEventHandler(object sender, NodeCancelEventArgs e);



		// Evénements:

		public event DeleteNodeEventHandler DeleteNodeByDefaultMenu;
		public event NodeCancelEventHandler NewFolderByDefaultMenu;
		public event NodeCancelEventHandler RenameFolderByDefaultMenu;
		public event NodeCancelEventHandler NewRootFolderByDefaultMenu;



		#endregion DECLARATIONS DE VARIABLES ET D'EVENEMENTS POUR LE MENU CONTEXTUEL





		// ---------------------------------------------------------------------------
		// PROPRIETES POUR LES MENUS CONTEXTUELS
		// ---------------------------------------------------------------------------




		#region PROPRIETES POUR LES MENUS CONTEXTUELS





		/// <summary>
		/// Obtient ou définit s'il faut demander à l'utilisateur avant de supprimer un noeud.
		/// </summary>
		public bool AskBeforeDelete { get; set; }
		
		
		
		
		
		
		
		/// <summary>
		/// Obtient ou définit s'il ne faut pas demander à l'utilisateur (bien que la propriété AskBeforeDelete soit active) quand il appuie sur la touche MAJ pour supprimer un noeud.
		/// </summary>
		public bool DoNotAskBeforeDeleteWhenShiftKey { get; set; }







		#endregion PROPRIETES POUR LES MENUS CONTEXTUELS
	






		// ---------------------------------------------------------------------------
		// SOUS-CLASSES POUR LES MENUS CONTEXTUELS
		// ---------------------------------------------------------------------------




		#region SOUS-CLASSES POUR LES MENUS CONTEXTUELS





		/// <summary>
		/// Classe d'arguments d'événement offrant en propriété un TreeNode et une possibilité d'annulation.
		/// </summary>
		public class NodeCancelEventArgs : EventArgs
		{
			public TreeNode Node { get; set; }
			public bool Cancel { get; set; }
			public NodeCancelEventArgs(TreeNode node, bool cancel) { Node = node; Cancel = cancel; }
		}





		// ---------------------------------------------------------------------------
	




		/// <summary>
		/// Classe d'arguments d'événement offrant en propriété un TreeNode, une possibilité d'annulation et la touche utilisée (s'il y a) pour la suppression, ce qui permet notamment de vérifier si la touche Shift a été utilisé en plus de la touche Del, ce qui peut déterminer la façon dont le noeud est supprimé (affichage d'un message ou non, etc.). Cancel vaut false par défaut.
		/// </summary>
		public class DeleteNodeEventArgs : EventArgs
		{
			public TreeNode Node { get; set; }
			public bool Cancel { get; set; }
			public Keys Keys { get; set; }
			public DeleteNodeEventArgs(TreeNode node, Keys keys) { Node = node; Keys = keys; Cancel = false; }
		}






		#endregion SOUS-CLASSES POUR LES MENUS CONTEXTUELS
	




		// ---------------------------------------------------------------------------
		// GESTIONNAIRES D'EVENEMENTS DE MENU CONTEXTUEL
		// ---------------------------------------------------------------------------





		#region GESTIONNAIRES D'EVENEMENTS DE MENU CONTEXTUEL










		// -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  - 
		// SOURIS: AFFICHAGE DU MENU CONTEXTUEL
		// -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  - 





		#region SOURIS: AFFICHAGE DU MENU CONTEXTUEL





		/// <summary>
		/// Affiche le menu contextuel correspondant au type du noeud si clic sur bouton de droit.
		/// </summary>
		private void ExdTreeView_NodeMouseClickForContextMenu(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (this.SelectedNode != e.Node) { this.SelectedNode = e.Node; }
				ContextMenu contextMenu;
				if ((GetContextMenu(e.Node, out contextMenu)) && (contextMenu != null))
					{ this.ContextMenu = contextMenu; } // ou: contextMenu.Show(this, e.Location);
			}
		}



		#endregion SOURIS: AFFICHAGE DU MENU CONTEXTUEL








		// -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  - 
		// MENU CONTEXTUEL PAR DEFAUT POUR LES DOSSIERS
		// -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  - 




		#region MENU CONTEXTUEL PAR DEFAUT POUR LES DOSSIERS




		/// <summary>
		/// Supprime le dossier, en demandant si nécessaire à l'utilisateur, puis en déclenchant l'événement DeleteNodeByDefaultMenu. Annule l'opération si l'argument d'annulation de l'événement vaut, au retour, true.
		/// </summary>
		private void DefaultFolderContextMenu_DeleteFolder_Click(object sender, EventArgs e)
		{

			// S'il faut demander à l'utilisateur:
			if ((AskBeforeDelete) && !(DoNotAskBeforeDeleteWhenShiftKey && Control.ModifierKeys == Keys.Shift)) {
				if (DialogBoxes.ShowDialogQuestion(MyResources.ExdTreeView_dialog_DeleteFolder)
					== DialogBoxClickResult.No) { return; } }

			// Déclence un événement:
			DeleteNodeEventArgs eventArgs = new DeleteNodeEventArgs(this.SelectedNode, Control.ModifierKeys);
			if (DeleteNodeByDefaultMenu != null) { DeleteNodeByDefaultMenu(this, eventArgs); }

			// Si pas d'annulation, supprime et met à jour les IndexInBranch des frères:
			if (!eventArgs.Cancel) { this.SelectedNode.Remove(); }

		}








		// ---------------------------------------------------------------------------




		/// <summary>
		/// Affiche une boîte de dialogue demandant à l'utilisateur un nom (document et dossier) et un type (dossier seulement). La fonction retourne true si l'utilisateur a validé, ou false s'il a annulé. Le nom et le type apparaissent dans name et type. Si folder vaut true, demande le nom et le type (seulement le nom sinon). Retourne le nom, et le type, nom et type qu'il est possible de préciser par défaut. Retourne true si l'utilisateur a choisi quelque chose (un type et nom non vide), ou false s'il a annulé ou si le nom était vide.
		/// </summary>
		protected virtual bool DialogBoxForNameAndType(bool folder, out string name, out int type, string defaultName, int defaultType)
		{
			// Prépare un dialogue:
			name = String.Empty; type = -1;
			MyFormMultilines dialog = new MyFormMultilines();
			dialog.SetDialogIcon(DialogBoxIcon.Question);
			dialog.SetDialogMessage(MyResources.ExdTreeView_dialog_NewFolder_Message);
			TextBox txtName = new TextBox();
			txtName.Text = defaultName;
			dialog.AddLine(MyResources.ExdTreeView_dialog_NewFolder_TextField, txtName);
			ComboBox cboType = new ComboBox();
			cboType.DropDownStyle = ComboBoxStyle.DropDownList;
			// Remplit la liste, si dossier:
			if ((this.NodeTypesList != null) && (folder))
			{
				int j = -1;
				foreach (ExdTreeNode.TreeNodeType i in this.NodeTypesList) {
					if (i.IsFolder) { cboType.Items.Add(i.Description); j++; }
					if (i.Id == defaultType) { defaultType = j; } }
				dialog.AddLine(MyResources.ExdTreeView_dialog_NewFolder_TypeField, cboType);
				cboType.SelectedIndex = defaultType;
			}
			// Affiche la boîte:
			txtName.Select();
			if (dialog.ShowDialog() == DialogBoxClickResult.OK) {
				name = txtName.Text;
				int j = 0;
				foreach (ExdTreeNode.TreeNodeType i in this.NodeTypesList) {
					if (!i.IsFolder) { continue; }
					if (j == cboType.SelectedIndex) { type = i.Id; break; } j++; }
				return true; }
			else { return false; }
		}




		// ---------------------------------------------------------------------------







		/// <summary>
		/// Ajoute un dossier, en demandant nom et type à l'utilisateur, puis en déclenchant l'événement NewFolderByDefaultMenu. Annule l'opération si l'argument d'annulation de l'événement vaut, au retour, true.
		/// </summary>
		private void DefaultFolderContextMenu_NewFolder_Click(object sender, EventArgs e)
		{

			// Variables:
			bool targetNodeWasExpanded = false; string newName; int type;
		
			// Affiche une boîte de dialogue:
			if (DialogBoxForNameAndType(true, out newName, out type, string.Empty, 0))
			{

				// Choisit s'il faut inclure un menu de même niveau ou de niveau inférieur...
				ExdTreeNode newNode = new ExdTreeNode();
				newNode.Text = newName;
				if (this.NodeTypesList != null) { newNode.NodeType = type; }
				if (((MenuItem)sender).Name == "NewSubfolder")
				{
					// Déplie le noeud, pour compatibilité avec AddChildrenWhenExpand:
					targetNodeWasExpanded = this.SelectedNode.IsExpanded;
					if (!targetNodeWasExpanded) { this.SelectedNode.Expand(); }
					newNode = (ExdTreeNode)this.SelectedNode.Nodes[this.SelectedNode.Nodes.Add(newNode)];
				}
				else
				{
					if (this.SelectedNode.Level == 0) { newNode = (ExdTreeNode)this.Nodes[this.Nodes.Add(newNode)]; }
					else { newNode = (ExdTreeNode)this.SelectedNode.Parent.Nodes[this.SelectedNode.Parent.Nodes.Add(newNode)]; }
				}

				// Déclence un événement:
				NodeCancelEventArgs eventArgs = new NodeCancelEventArgs(newNode, false);
				if (NewFolderByDefaultMenu != null) { NewFolderByDefaultMenu(this, eventArgs); }

				// Si annulation, supprime le nouveau noeud:
				if (eventArgs.Cancel)
				{
					this.Nodes.Remove(newNode);
					if ((((MenuItem)sender).Name == "NewSubfolder") && (!targetNodeWasExpanded)) { this.SelectedNode.Collapse(); }
				}
				
				// Applique les caractéristiques du type au noeud:
				newNode.SetNodeType(newNode.NodeType, this);

				// Sélectionne le noeud:
				this.SelectedNode = newNode;
				
			}
			
		}





		// ---------------------------------------------------------------------------






		/// <summary>
		/// Renomme un dossier et/ou change son type, en demandant nom et type à l'utilisateur, puis en déclenchant l'événement RenameFolderByDefaultMenu. Annule l'opération si l'argument d'annulation de l'événement vaut, au retour, true.
		/// </summary>
		private void DefaultFolderContextMenu_RenameFolder_Click(object sender, EventArgs e)
		{

			// Renomme un nouveau noeud:
			
			string newName; string oldName; int type;

			if (DialogBoxForNameAndType(true, out newName, out type, this.SelectedNode.Text, ((ExdTreeNode)this.SelectedNode).NodeType))
			{

				// Remplace simplement le nom:
				oldName = this.SelectedNode.Text;
				this.SelectedNode.Text = newName;
				int oldType = 0;
				if (this.SelectedNode is ExdTreeNode)
				{
					oldType = ((ExdTreeNode)this.SelectedNode).NodeType;
					((ExdTreeNode)this.SelectedNode).NodeType = type;
				}

				// Déclence un événement:
				NodeCancelEventArgs eventArgs = new NodeCancelEventArgs(this.SelectedNode, false);
				if (RenameFolderByDefaultMenu != null) { RenameFolderByDefaultMenu(this, eventArgs); }

				// Si annulation, remet l'ancien nom:
				if (eventArgs.Cancel)
				{
					this.SelectedNode.Text = oldName;
					if (this.SelectedNode is ExdTreeNode) { ((ExdTreeNode)this.SelectedNode).NodeType = oldType; }
				}
				// Sinon, modifie l'image du type:
				else { this.SetImageKey(this.SelectedNode); }
				
			}
			
		}





		#endregion MENU CONTEXTUEL PAR DEFAUT POUR LES DOSSIERS







		// -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  - 
		// MENU CONTEXTUEL PAR DEFAUT POUR LES DOCUMENTS
		// -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  - 




		#region MENU CONTEXTUEL PAR DEFAUT POUR LES DOCUMENTS





		/// <summary>
		/// Supprime le document, en demandant si nécessaire à l'utilisateur, puis en déclenchant l'événement DeleteNodeByDefaultMenu. Annule l'opération si l'argument d'annulation de l'événement vaut, au retour, true.
		/// </summary>
		private void DefaultDocContextMenu_DeleteDoc_Click(object sender, EventArgs e)
		{
		
			// S'il faut demander à l'utilisateur:
			if ((AskBeforeDelete) && !(DoNotAskBeforeDeleteWhenShiftKey && Control.ModifierKeys == Keys.Shift)) {
				if (DialogBoxes.ShowDialogQuestion(MyResources.ExdTreeView_dialog_DeleteDocument)
					== DialogBoxClickResult.No) { return; } }

			// Déclence un événement:
			DeleteNodeEventArgs eventArgs = new DeleteNodeEventArgs(this.SelectedNode, Control.ModifierKeys);
			if (DeleteNodeByDefaultMenu != null) { DeleteNodeByDefaultMenu(this, eventArgs); }

			// Si pas d'annulation, supprime et met à jour les IndexInBranch des frères:
			if (!eventArgs.Cancel) { this.SelectedNode.Remove(); }

		}






		#endregion MENU CONTEXTUEL PAR DEFAUT POUR LES DOCUMENTS








		// -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  - 
		// MENU CONTEXTUEL PAR DEFAUT POUR  L'ARBRE
		// -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  - 




		#region MENU CONTEXTUEL PAR DEFAUT POUR  L'ARBRE




		/// <summary>
		/// Ajoute un dossier racine, en demandant nom et type à l'utilisateur, puis en déclenchant l'événement NewRootFolderByDefaultMenu. Annule l'opération si l'argument d'annulation de l'événement vaut, au retour, true.
		/// </summary>
		private void DefaultTreeContextMenu_NewRootFolder_Click(object sender, EventArgs e)
		{

			// Variables:
			string newName; int type;
		
			// Affiche une boîte de dialogue:
			if (DialogBoxForNameAndType(true, out newName, out type, string.Empty, 0))
			{

				// Choisit s'il faut inclure un menu de même niveau ou de niveau inférieur...
				ExdTreeNode newNode = new ExdTreeNode();
				newNode.Text = newName;
				if (this.NodeTypesList != null) { newNode.NodeType = type; }
				newNode = (ExdTreeNode)this.Nodes[this.Nodes.Add(newNode)];

				// Déclence un événement:
				NodeCancelEventArgs eventArgs = new NodeCancelEventArgs(newNode, false);
				if (NewRootFolderByDefaultMenu != null) { NewRootFolderByDefaultMenu(this, eventArgs); }

				// Si annulation, supprime le nouveau noeud:
				if (eventArgs.Cancel)
				{
					this.Nodes.Remove(newNode);
				}
				
				// Applique les caractéristiques du type au noeud:
				newNode.SetNodeType(newNode.NodeType, this);

				// Sélectionne le noeud:
				this.SelectedNode = newNode;
				
			}
			
		}






		#endregion MENU CONTEXTUEL PAR DEFAUT POUR L'ARBRE
	




		#endregion GESTIONNAIRES D'EVENEMENTS DE MENU CONTEXTUEL
	




		#endregion GESTION DES MENUS CONTEXTUELS
	


















		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// GESTION DU DRAG AND DROP
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+




		#region GESTION DU DRAG AND DROP





		// ---------------------------------------------------------------------------
		// DECLARATIONS DE VARIABLES ET D'EVENEMENTS POUR LE DRAG AND DROP
		// ---------------------------------------------------------------------------




		#region DECLARATIONS DE VARIABLES ET D'EVENEMENTS POUR LE DRAG AND DROP





		// Champs privés:

		private TreeNode _dropTargetNode = null;
		private int _dropInsertWhere = 0;




		// Délégués:
		
		public delegate void DropNodeEventHandler(object sender, DropNodeEventArgs e);
		
		
		
		
		// Evénements:
		
		public event DropNodeEventHandler MoveOrCopyNodeByDrop;





		#endregion DECLARATIONS DE VARIABLES ET D'EVENEMENTS POUR LE DRAG AND DROP








		// ---------------------------------------------------------------------------
		// SOUS-CLASSES POUR LE DRAG AND DROP
		// ---------------------------------------------------------------------------




		#region SOUS-CLASSES POUR LE DRAG AND DROP





		public class DropNodeEventArgs : EventArgs
		{

			public TreeNode SourceNode { get; set; }
			public TreeNode NewNode { get; set; }
			public bool IsCopy { get; set; }
			public bool Cancel { get; set; }

			public DropNodeEventArgs(TreeNode sourceNode, TreeNode newNode, bool isCopy)
			{
				this.SourceNode = sourceNode;
				this.NewNode = newNode;
				this.IsCopy = isCopy;
				this.Cancel = false;
			}

		}




		#endregion SOUS-CLASSES POUR LE DRAG AND DROP
	






		// ---------------------------------------------------------------------------
		// PROPRIETES DU DRAG AND DROP
		// ---------------------------------------------------------------------------




		#region PROPRIETES DU DRAG AND DROP




		public DragDropEffects DragDropAllowedEffects { get; set; }

		
		
		
		
		#endregion PROPRIETES DU DRAG AND DROP
		







		// ---------------------------------------------------------------------------
		// GESTIONS DES EVENEMENTS DU DRAG AND DROP
		// ---------------------------------------------------------------------------




		#region GESTIONS DES EVENEMENTS DU DRAG AND DROP











		/// <summary>
		/// A la fin du drop, remet l'affichage normal (supprime les marques, etc.).
		/// </summary>
		private void ExdTreeView_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			if ((e.Action == DragAction.Cancel) || (e.Action == DragAction.Drop))
			{
				this.Refresh();
				_dropTargetNode.BackColor = Color.Empty;
			}
			if (e.Action == DragAction.Cancel)
			{
				_dropTargetNode = null;
				_dropInsertWhere = 0;
				this.SelectedNode.ForeColor = _dropSourceNodeColor;
				_dropSourceNodeColor = Color.Empty;
			}
		}








		// ---------------------------------------------------------------------------



		protected Color _dropSourceNodeColor;




		/// <summary>
		/// Lorsqu'un item est déplacé, initie le d&d. Autorise le déplacement et/ou la copie en fontion des propriétés.
		/// </summary>
		private void ExdTreeView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			this.SelectedNode = (TreeNode)e.Item;
			this.DoDragDrop(e.Item, DragDropAllowedEffects);
		}








		// ---------------------------------------------------------------------------








		/// <summary>
		/// Autorisation des actions quant on entre (mais on y est déjà) dans le TV.
		/// </summary>
		private void ExdTreeView_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(TreeNode)) || e.Data.GetDataPresent(typeof(ExdTreeNode)))
				{ e.Effect = e.AllowedEffect; }
			else
				{ e.Effect = DragDropEffects.None; }
		}








		// ---------------------------------------------------------------------------








		/// <summary>
		/// Dessine une ligne entre deux nodes, comme "marque d'insertion". Gère aussi les interdictions : Il est interdit de copier un document dans un même dossier, de déplacer ou copier un dossier vers ses enfants ou petits-enfants ou dans lui-même. Pour les documents, on peut insérer avant ou après, alors que pour les dossiers, on peut insérer avant, après ou dedans.
		/// </summary>
		private void ExdTreeView_DragOver(object sender, DragEventArgs e)
		{
		
			// Sort si n'est pas un treenode:
			if ((e.Data.GetDataPresent(typeof(TreeNode)) == false) && (e.Data.GetDataPresent(typeof(ExdTreeNode)) == false))
				{ e.Effect = DragDropEffects.None; return; }

			// Change l'état du curseur:
			switch (e.KeyState)
			{
				case 1: e.Effect = DragDropEffects.Move; break;
				case 1 + 8: e.Effect = DragDropEffects.Copy; break; //Ctrl
				default: e.Effect = DragDropEffects.None; break;
			}


			// Suspend graphique:
			this.SuspendLayout();
			
			// Trouve le point survolé par la souris et l'item correspondant, avec son rectangle de position:
			Point ptTargetNode = this.PointToClient(new Point(e.X, e.Y));
			//int hoverItemIndex = this.GetNodeAt(ptTargetItem.X, ptTargetItem.Y).Index;
			TreeNode hoverNode = this.GetNodeAt(ptTargetNode.X, ptTargetNode.Y);
			// Sort si null (si l'utilisateur est en-dehors de l'arbre):
			if (hoverNode == null) { this.Refresh(); return; }
			// Définit le rectangle de l'arbre:
			Rectangle rectHoverNode = hoverNode.Bounds;


			// Défilement automatique...
			if (hoverNode.PrevVisibleNode != null) { hoverNode.PrevVisibleNode.EnsureVisible(); }
			if (hoverNode.NextVisibleNode != null) { hoverNode.NextVisibleNode.EnsureVisible(); ; }


			// Définit insertWhere sur -1 (premier quart), 0 (milieu), 1 (dernier quart):
			int insertWhere; bool isFolder;
			// Si l'insertion dans un objet est autorisé, il y a trois états:
			if ((GetIsFolder(hoverNode, out isFolder)) && (isFolder))
			//if ((hoverNode is ExdTreeNode) && (((ExdTreeNode)hoverNode).DragDropLikeFolder))
			{
				if (ptTargetNode.Y < (rectHoverNode.Top + (rectHoverNode.Height / 4))) { insertWhere = -1; }
				else if (ptTargetNode.Y < (rectHoverNode.Top + (3 * (rectHoverNode.Height / 4)))) { insertWhere = 0; }
				else { insertWhere = 1; }
			}
			// Sinon seulement 2:
			else
			{
				if (ptTargetNode.Y < (rectHoverNode.Top + (rectHoverNode.Height / 2))) { insertWhere = -1; }
				else { insertWhere = 1; }
			}


			// Vérifie que le noeud survolé n'est pas un enfant du noeud d'origine:
			TreeNode sourceNode = (TreeNode)e.Data.GetData(typeof(ExdTreeNode));
			

			// Remonte toute la hiérarchie pour trouver si hoverNode n'a pas un parent égal à sourceNode:
			TreeNode tmpParent = hoverNode;
			if ((GetIsFolder(sourceNode, out isFolder)) && (isFolder))
			{
				for (int i=hoverNode.Level; i>=0; i--)
				{
					// Sort si on arrive au niveau du noeud d'origine:
					if (i < sourceNode.Level) { break; }
					tmpParent = tmpParent.Parent;
					if (tmpParent == sourceNode) { e.Effect = DragDropEffects.None; break; }
				}
			}

			// Si le noeud source est un document, on ne peut pas le copier dans le même dossier, MAIS on peut le copier DANS un dossier frère...:
			if ((sourceNode.Parent == hoverNode.Parent) && (GetIsFolder(sourceNode, out isFolder)) && (isFolder == false) && (e.Effect == DragDropEffects.Copy))
			{
				if (insertWhere != 0) { e.Effect = DragDropEffects.None; }
			}


			// Vérifie que le noeud de destination n'est pas celui d'origine:
			if ((insertWhere == 0) && (hoverNode == sourceNode) && (GetIsFolder(hoverNode, out isFolder)) && (isFolder))
				{ e.Effect = DragDropEffects.None; }


			// Au début, définit _dropTargetNode qui n'a pas été initiaisé:
			bool startingDrop = false;
			if (_dropTargetNode == null) { _dropTargetNode = hoverNode; _dropInsertWhere = insertWhere; startingDrop = true; }

			// Bizarrement, la couleur du noeud disparaît... On l'enregistre donc, et elle sera remise dans DragDrop et QueryContinueDrag:
			if (_dropSourceNodeColor == Color.Empty) { _dropSourceNodeColor = sourceNode.ForeColor; }
			// Même chose pour le noeud survolé, mais la couleur sera remise cette fois en fin de procédure:
			Color hoverSavedColor = hoverNode.ForeColor;

			// Si l'item survolé ne l'a pas déjà été, ou si c'est la première fois:
			if ((_dropTargetNode != hoverNode) || (_dropInsertWhere != insertWhere) || startingDrop)
			{
				// Efface les effets graphiques:
				if (sourceNode != _dropTargetNode) { _dropTargetNode.BackColor = Color.Empty; }
				this.Refresh(); // Doit se trouver après les précédents !!!

				// Si dans 1er ou denrier quart, on dessine un trait avant ou après...
				if (insertWhere != 0)
				{
					// Défini un nouveau graphique.
					Graphics g = this.CreateGraphics();
					// Détermine le point où il faut tracer la ligne (marque d'insertion):
					Point pt = new Point(rectHoverNode.X, rectHoverNode.Y);
					if (insertWhere == 1) { pt.Y += rectHoverNode.Height; }
					// Longueur de la marque d'insertion:
					int insertMarkWidth = rectHoverNode.Width;
					// Défintion du pinceau:
					Pen p = new Pen(MySettings.TreeSelectionColor, 3);
					p.StartCap = p.EndCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
					// Dessine la ligne:
					g.DrawLine(p, pt.X, pt.Y, pt.X + insertMarkWidth, pt.Y);
				}
				// Sinon, on change la couleur du TreeNode:
				else
				{
					hoverNode.BackColor = MySettings.TreeSelectionColor;
				}


				// Enregistre l'index qu'aura l'élément inséré si l'utilisateur lâche maintenant la souris:
				_dropTargetNode = hoverNode;
				_dropInsertWhere = insertWhere;
			}
			
			// Lance les op. graphiques:
			this.ResumeLayout(false);

			// Rétablit la couleur:
			_dropTargetNode.ForeColor = hoverSavedColor;

		}








		// ---------------------------------------------------------------------------








		/// <summary>
		/// Quand la souris quitte la liste, élimine la ligne ("marque d'insertion").
		/// </summary>
		private void ExdTreeView_DragLeave(object sender, EventArgs e)
		{
			this.Refresh();
		}








		// ---------------------------------------------------------------------------
	







		/// <summary>
		/// Quand un noeud est déposé, il est déplacé ou copié à l'endroit de la marque d'insertion : avant ou après un document, avant, après ou dans un dossier. Si le dossier dans lequel un document est déplacé ou copié contient déjà un document avec le même IdDoc, un message est affiché et l'opération est annulée (sauf s'il s'agit d'un déplacement dans le même dossier). Après la création du nouveau noeud, et aussi avant la suppression de l'ancien noeud dans le cas d'un déplacement, ce qui permet de gérer l'ancien et le nouveau noeud), un événement MoveOrCopyNodeByDrop est déclenché, et le nouveau noeud est supprimé pour rétablir l'état originel de l'arbre si l'argument d'annulation de l'événement vaut, au retour, true.
		/// </summary>
		protected virtual void ExdTreeView_DragDrop(object sender, DragEventArgs e)
		{

			// Efface l'affichage:
			// Fait par QueryContinueDrag...


			// Variables:
			ExdTreeNode insertedNode; int index; bool targetNodeWasExpanded = false;

			// Récupère le noeud source.
			ExdTreeNode sourceNode = (ExdTreeNode)e.Data.GetData(typeof(ExdTreeNode));
			sourceNode.ForeColor = _dropSourceNodeColor;
			_dropSourceNodeColor = Color.Empty;

			// Suspend graphique:
			this.SuspendLayout();

			// S'il ne faut pas le mettre "dans" un dossier, mais simplement le déplacer:
			if (_dropInsertWhere != 0)
			{
				// Récupère l'index, et l'augmente s'il faut le mettre après:
				index = _dropTargetNode.Index;
				if (_dropInsertWhere == 1) { index++; }

				// Vérifie que le noeud avec son idDoc n'existe pas déjà dans le dossier, sauf s'il s'agit d'un déplacement dans le même dossier:
				if (sourceNode.Parent != _dropTargetNode.Parent)
				{
					if (NodeExistsByIdDoc(((_dropTargetNode.Level==0) ? null : _dropTargetNode.Parent), sourceNode.IdDocument))
					{
						DialogBoxes.ShowDialogMessage(MyResources.ExdTreeView_dialog_DocumentAlreadyInFolder,
							DialogBoxIcon.Exclamation);
						// Remise à zéro de _dropIndexTarget
						_dropTargetNode = null;
						_dropInsertWhere = 0;
						return;
					}
				}

				// Si le noeud de destination a un niveau 0, fait appel directement à l'arbre:
				if (_dropTargetNode.Level == 0)
				{
					this.Nodes.Insert(index, (ExdTreeNode)sourceNode.Clone());
					insertedNode = (ExdTreeNode)this.Nodes[index];
				}

				// Sinon fait appelle au parent pour permettre l'insertion à l'endroit voulu:
				else
				{
					_dropTargetNode.Parent.Nodes.Insert(index, (ExdTreeNode)sourceNode.Clone());
					insertedNode = (ExdTreeNode)_dropTargetNode.Parent.Nodes[index];
				}
			}
			
			// S'il faut l'insérer "dans" un dossier:
			else
			{
			
				// Déplie le noeud, pour compatibilité avec AddChildrenWhenExpand:
				targetNodeWasExpanded = _dropTargetNode.IsExpanded;
				if (!targetNodeWasExpanded) { _dropTargetNode.Expand(); }
				
				// Vérifie que le noeud avec son idDoc n'existe pas déjà dans le dossier:
				if (NodeExistsByIdDoc(_dropTargetNode, sourceNode.IdDocument))
				{
					DialogBoxes.ShowDialogMessage(MyResources.ExdTreeView_dialog_DocumentAlreadyInFolder,
						DialogBoxIcon.Exclamation);
					if (targetNodeWasExpanded == false) { _dropTargetNode.Collapse(); }
					// Remise à zéro de _dropIndexTarget
					_dropTargetNode = null;
					_dropInsertWhere = 0;
					return;
				}
				
				// Prend le noeud de destination directement comme parent, plutôt que de faire appel au parent de ce noeud de destination:
				index = _dropTargetNode.Nodes.Add((ExdTreeNode)sourceNode.Clone());
				insertedNode = (ExdTreeNode)_dropTargetNode.Nodes[index];
			}


			// Déclenche un événement, et regarde si la propriété Cancel est true, auquel cas supprime le noeud cloné et sort:
			DropNodeEventArgs eventArgs = new DropNodeEventArgs(sourceNode, insertedNode, ((e.KeyState == 8) ? (true) : (false)));
			if (MoveOrCopyNodeByDrop != null) { MoveOrCopyNodeByDrop(this, eventArgs); }
			if (eventArgs.Cancel)
			{
				insertedNode.Remove();
				if ((_dropInsertWhere ==0) && (targetNodeWasExpanded == false)) { _dropTargetNode.Collapse(); }
			}

			// Sinon...
			else
			{
			
				// Met à jour le texte (car si le texte est en gras, il a un problème d'affichage).
				insertedNode.Text = insertedNode.Text;
				insertedNode.ForeColor = sourceNode.ForeColor;
				
				// Si ce n'est pas une copie, supprime le noeud d'origine, et sélectionne le nouveau, et met à jour les IndexInBranch des frères:
				if (e.KeyState != 8)
				{
					sourceNode.Remove();
				}
				this.SelectedNode = insertedNode;
				if (SingleCheck) { insertedNode.Checked = insertedNode.Checked; } // Au cas où un seul noeud doit être sélectionné (à cause du clonage).

			}
			

			// Relance graphique:
			this.ResumeLayout(false);

			// Remise à zéro de _dropIndexTarget
			_dropTargetNode = null;
			_dropInsertWhere = 0;


		}



		#endregion GESTIONS DES EVENEMENTS DU DRAG AND DROP





		#endregion GESTION DU DRAG AND DROP






		// END CLASS ExdTreeView
	
	}
	
	
	
	
}
