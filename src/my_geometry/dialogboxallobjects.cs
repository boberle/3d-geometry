using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace My
{


	/// <summary>
	/// Affiche un dialogue avec les commandes organisées en arbre, à partir de la ressource cmdstree.txt, et affiche éventuellement la commande dans la console.
	/// </summary>
	public class DialogBoxAllObjects : My.MyFormMessage
	{
	
	
		// ---------------------------------------------------------------------------
		// DECLARATIONS

		private TreeView _treeByMasters, _treeByOwners;
		private ExdListView _list;
		private TextBox _txt;
		private Button _cmdClose, _cmdByMaster, _cmdByOwners, _cmdList;
		private SpObjectsCollection _coll;
		private SpObject[] _allObjects;
		private int _hDist, _vDist;
		private SplitContainer _split;


		// ---------------------------------------------------------------------------
		// CONSTRUCTEUR


		/// <summary>
		/// Constructeur.
		/// </summary>
		public DialogBoxAllObjects()
		{
		
			// Initialisation des variables:
			_coll = SpObjectsCollection.GetInstance();
			
			// Initialisation des arbres et listes:
			_treeByMasters = new TreeView();
			_treeByMasters.Dock = DockStyle.Fill;
			_treeByMasters.HideSelection = false;
			_treeByMasters.Font = My.Geometry.MySettings.DefaultListFont;
			_treeByMasters.AfterSelect += new TreeViewEventHandler(_tree_AfterSelect);
			_treeByOwners = new TreeView();
			_treeByOwners.Dock = DockStyle.Fill;
			_treeByOwners.HideSelection = false;
			_treeByOwners.Font = My.Geometry.MySettings.DefaultListFont;
			_treeByOwners.AfterSelect += new TreeViewEventHandler(_tree_AfterSelect);
			_list = new ExdListView();
			_list.Dock = DockStyle.Fill;
			_list.SelectedIndexChanged += new EventHandler(_list_SelectedIndexChanged);
			_list.AllowDelete = false;
			_list.AllowColumnReorder = false;
			_list.AllowSortByColumnClick = true;
			_list.MultiSelect = false;
			_list.HideSelection = false;
			_list.Font = My.Geometry.MySettings.DefaultListFont;
			_list.Columns.Add("Name");
			_list.Columns.Add("Description");
			_list.Columns.Add("Type");
			_list.Columns.Add("Class");
			_list.Columns.Add("System type");
			_list.Columns.Add("Extracted");
			_list.Columns.Add("Masters");
			_list.Columns.Add("Owneds");
			_list.Columns.Add("Owner");

			// Initilisation du TextBox:
			_txt = new TextBox();
			_txt.Dock = DockStyle.Fill;
			_txt.Multiline = true;
			_txt.ReadOnly = true;
			_txt.ScrollBars = ScrollBars.Vertical;
			_txt.Font = My.Geometry.MySettings.DefaultListFont;
			
			// Initialisation du splitter:
			_split = new SplitContainer();
			_split.Dock = DockStyle.Fill;
			_split.Orientation = Orientation.Vertical;
			this.Load += delegate {
				_vDist = (int)(_tlpBody.Width * 0.5);
				_hDist = (int)(_tlpBody.Height * 0.5);
				_split.SplitterDistance = _vDist; };
			_split.Panel1.Controls.Add(_treeByMasters);
			_split.Panel2.Controls.Add(_txt);
			
			// Initialisation des boutons:
			_cmdClose = new Button();
			_cmdClose.Text = "Close";
			_cmdClose.Click += delegate { _allObjects = null; this.Hide(); };
			_cmdClose.Tag = My.DialogBoxTagButton.AcceptCancel;
			_cmdByMaster = new Button();
			_cmdByMaster.Text = "By masters";
			_cmdByMaster.Click += delegate { ChangeDisplayedControl(_treeByMasters); };
			_cmdByOwners = new Button();
			_cmdByOwners.Text = "By owners";
			_cmdByOwners.Click += delegate { ChangeDisplayedControl(_treeByOwners); };
			_cmdList = new Button();
			_cmdList.Text = "List";
			_cmdList.Click += delegate { ChangeDisplayedControl(_list); };
			
			// Initialisation du form:
			SubtitleBox = "All objects";
			_enableUserClosing = true;
			SetDialogIcon(My.DialogBoxIcon.Search);
			SetDialogMessage("All objects (explicit, system and virtual) are shown here:");
			AddButtonsCollection(new My.ButtonsCollection(1, _cmdClose, _cmdByMaster, _cmdByOwners, _cmdList), true);
			SetControl(_split);
			Width = (int)(Screen.PrimaryScreen.WorkingArea.Width / 1.5);
			Height = (int)(Screen.PrimaryScreen.WorkingArea.Height / 1.3);
			_treeByMasters.Select();
		
		}


		// ---------------------------------------------------------------------------
		// METHODES
		
		
		/// <summary>
		/// Change le contrôle affiché sur le Splitter.
		/// </summary>
		private void ChangeDisplayedControl(Control ctrl)
		{
			// Sauve et remet la bonne distance en fonction de l'orientation:
			if (_split.Orientation == Orientation.Horizontal) { _hDist = _split.SplitterDistance; }
			else { _vDist = _split.SplitterDistance; }
			// Enlève et rajoute le contrôle:
			_split.Panel1.Controls.RemoveAt(0);
			_split.Panel1.Controls.Add(ctrl);
			// Modifie l'orientation:
			if (ctrl is ExdListView) { _split.Orientation = Orientation.Horizontal; _split.SplitterDistance = _hDist; }
			else { _split.Orientation = Orientation.Vertical; _split.SplitterDistance = _vDist; }
			ctrl.Select();
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Affiche les infos sur l'objet sélectionné.
		/// </summary>
		private void _tree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			string name = (string)e.Node.Tag;
			foreach (SpObject o in _allObjects) { if (o.Name == name) { FillTextBox(o, (Control)sender); return; } }
		}

		/// <summary>
		/// Affiche les infos sur l'objet sélectionné.
		/// </summary>
		private void _list_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_list.SelectedItems.Count == 0) { return; }
			string name = _list.SelectedItems[0].Text;
			foreach (SpObject o in _allObjects) { if (o.Name == name) { FillTextBox(o, (Control)sender); return; } }
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Affiche les infos de l'objet passé en argument dans le TextBox.
		/// </summary>
		private void FillTextBox(SpObject obj, Control sender)
		{

			// Met à jour le TextBox:
			_txt.Clear();
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Infos for {0} ({1}) - {2}\n\n", obj.Name,
				(obj.IsVirtual ? "Virtual" : (obj.IsExtracted ? "System, Extracted" : (obj.IsSystem ? "System" : "Explicit"))),
				obj.GetType().Name);
			sb.Append(obj.GetInfos());
			_txt.Text = sb.ToString().Replace("\n", "\r\n");

			// Sélectionne dans tous les contrôles:
			_treeByMasters.AfterSelect -= _tree_AfterSelect;
			_treeByOwners.AfterSelect -= _tree_AfterSelect;
			_list.SelectedIndexChanged -= _list_SelectedIndexChanged;
			if (sender != _treeByMasters) { SelectNodes(_treeByMasters, obj.Name); }
			if (sender != _treeByOwners) { SelectNodes(_treeByOwners, obj.Name); }
			if (sender != _list) {
				foreach (ListViewItem item in _list.Items) { if (item.Text == obj.Name) { item.Selected = true; item.EnsureVisible(); } } }
			_treeByMasters.AfterSelect += _tree_AfterSelect;
			_treeByOwners.AfterSelect += _tree_AfterSelect;
			_list.SelectedIndexChanged += _list_SelectedIndexChanged;
			
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Sélectionne dans l'arbre passé en argument le noeud qui correspond à l'objet indiqué.
		/// </summary>
		private void SelectNodes(TreeView tree, string objName)
		{
			foreach (TreeNode n in tree.Nodes) {
				if ((string)n.Tag == objName) { n.TreeView.SelectedNode = n; n.EnsureVisible(); }
				else { SelectNodes(n, ref objName); } }
		}
		
		/// <summary>
		/// Procédure récursive pour la surcharge de SelectNodes.
		/// </summary>
		private void SelectNodes(TreeNode node, ref string objName)
		{
			foreach (TreeNode n in node.Nodes) {
				if ((string)n.Tag == objName) { n.TreeView.SelectedNode = n; n.EnsureVisible(); }
				else { SelectNodes(n, ref objName); } }
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Construit le tableau de tous les objest _allObjects.
		/// </summary>
		private void SetAllObjectsArray()
		{
			// Obtient le tableau de tous les objets (un objet est soit explicit, soit owner d'un autre):
			int l = _coll.Count;
			_allObjects = new SpObject[l];
			for (int i=0; i<l; i++) { _allObjects[i] = _coll[i]; }
			foreach (SpObject o in _coll) {
				if (o.OwnedObjects.Length > 0) { _allObjects = _allObjects.Concat(o.OwnedObjects).ToArray(); } }
			_allObjects = _allObjects.Distinct().ToArray();
			SetDialogMessage(String.Format("All {0} objects (explicit, system and virtual) are shown here:", _allObjects.Length));
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Vide puis remplit l'arbre avec tous les objets, en les faisant dépendre de leur maîtres.
		/// </summary>
		private void FillTreeByMasters()
		{
			_treeByMasters.Nodes.Clear();
			// Inscrit les objets indépendants, sans maître, et appelle la procédure récursive:
			TreeNode slaveNode;
			foreach (SpObject o in _allObjects)
			{
				if (o.MasterObjects.Length == 0)
				{
					slaveNode = _treeByMasters.Nodes.Add(String.Format("{0} ({1})", o.Name, o.TypeDescription));
					slaveNode.Tag = o.Name;
					slaveNode.ForeColor = o.Color;
					FillTreeByMasters(o, slaveNode);
				}
			}
			_treeByMasters.Sort();
		}
		
		/// <summary>
		/// Procédure récursive pour le remplissage de l'arbre selon dépendance aux maîtres.
		/// </summary>
		private void FillTreeByMasters(SpObject master, TreeNode masterNode)
		{
			// Cherche tous les objets qui ont le bon master, puis s'appelle pour récursivité:
			TreeNode slaveNode;
			foreach (SpObject o in _allObjects)
			{
				if (o.MasterObjects.Contains(master))
				{
					slaveNode = masterNode.Nodes.Add(String.Format("{0} ({1})", o.Name, o.TypeDescription));
					slaveNode.Tag = o.Name;
					slaveNode.ForeColor = o.Color;
					FillTreeByMasters(o, slaveNode);
				}
			}
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Vide puis remplit l'arbre avec tous les objets, en les faisant dépendre de leur owner.
		/// </summary>
		private void FillTreeByOwners()
		{
			_treeByOwners.Nodes.Clear();
			// Inscrit les objets qui ne sont pas dépendants d'un owner, donc tous les objets non systèmes et non virtuels:
			TreeNode ownedNode;
			foreach (SpObject o in _allObjects)
			{
				if (!o.IsSystem && !o.IsVirtual)
				{
					ownedNode = _treeByOwners.Nodes.Add(String.Format("{0} ({1})", o.Name, o.TypeDescription));
					ownedNode.Tag = o.Name;
					ownedNode.ForeColor = o.Color;
					FillTreeByOwners(o, ownedNode);
				}
			}
			_treeByOwners.Sort();
		}
		
		/// <summary>
		/// Procédure récursive pour le remplissage de l'arbre selon dépendance aux owners.
		/// </summary>
		private void FillTreeByOwners(SpObject owner, TreeNode ownerNode)
		{
			// Cherche tous les objets qui ont le bon master, puis s'appelle pour récursivité:
			TreeNode ownedNode;
			foreach (SpObject o in _allObjects)
			{
				if (owner.OwnedObjects.Contains(o))
				{
					ownedNode = ownerNode.Nodes.Add(String.Format("{0} ({1})", o.Name, o.TypeDescription));
					ownedNode.Tag = o.Name;
					ownedNode.ForeColor = o.Color;
					FillTreeByOwners(o, ownedNode);
				}
			}
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Remplit la liste.
		/// </summary>
		private void FillList()
		{
			_list.Items.Clear(); ListViewItem item;
			foreach (My.SpObject o in _allObjects)
			{
				item = new ListViewItem(o.Name);
				item.SubItems.Add(o.ToString());
				item.SubItems.Add(o.TypeDescription);
				item.SubItems.Add(o.GetType().Name);
				item.SubItems.Add((o.IsVirtual ? "Virtual" : (o.IsSystem ? "System" : "Explicit")));
				item.SubItems.Add(o.IsExtracted.ToString());
				item.SubItems.Add(ArrayFunctions.Join(o.MasterObjects, delegate(SpObject obj) { return obj.Name; }, ","));
				item.SubItems.Add(ArrayFunctions.Join(o.OwnedObjects, delegate(SpObject obj) { return obj.Name; }, ","));
				item.SubItems.Add(o.Owner == null ? "" : o.Owner.Name);
				item.ForeColor = o.Color;
				_list.Items.Add(item);
			}
			_list.AutoResizeColumns(300);
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Affiche la fenêtre en modal en remplissant les listes et les arbres.
		/// </summary>
		public new void ShowDialog()
			{ ShowDialog(null); }

		/// <summary>
		/// Affiche la fenêtre en modal en remplissant les listes et les arbres.
		/// </summary>
		public new void ShowDialog(IWin32Window owner)
		{
			SetAllObjectsArray();
			FillTreeByMasters();
			FillTreeByOwners();
			FillList();
			if (owner == null) { base.ShowDialog(); }
			else { base.ShowDialog(owner); }
		}
		
	
	}
	
	
	
	
	
}
