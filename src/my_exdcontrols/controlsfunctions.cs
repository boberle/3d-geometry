using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace My
{


	/// <summary>
	/// Fournit des méthodes pour la gestion des objets.
	/// </summary>
	public static class ControlsFunctions
	{
	
	
		/// <summary>
		/// Recherche dans l'arbre un noeud qui correspond au texte spécifié. Si ask est vrai, demande à l'utilisateur un nouveau texte. Commence la recherche à partir du noeud actuellement sélectionné (ou du début si rien n'est sélectionné). Sélectionne le noeud trouvé. Retourne le texte cherché, entré par l'utilisateur au besoin.
		/// </summary>
		public static string SearchInTreeView(string search, bool ask, TreeView tree)
		{
			// Demande à l'user:
			if (String.IsNullOrEmpty(search) || ask)
			{
				if (My.DialogBoxes.ShowDialogInput("Part of text to search:", search) == My.DialogBoxClickResult.Cancel
					|| String.IsNullOrEmpty(My.DialogBoxes.InputText)) { return null; }
				search = My.DialogBoxes.InputText;
			}
			// Commence la recherche à partir de l'item sélectionné, ou du premier si pas d'item sélectionné:
			bool canSearch = (tree.SelectedNode == null);
			TreeNode firstFound = null;
			foreach (TreeNode n in tree.Nodes)
			{
				// Si le texte correspond:
				if (n.Text.ToLower().Contains(search.ToLower())) {
					// Si on peut chercher et trouver, on sélectionne et on sort:
					if (canSearch) { tree.SelectedNode = n; n.EnsureVisible(); return search; }
					// Sinon, si _firstFound vaut null, on l'enregistre, au cas où on ne trouve rien d'autre par la suite:
					else if (firstFound == null) { firstFound = n; } }
				// On regarde si on peut commencer à chercher et trouver:
				canSearch = canSearch || n.IsSelected;
				// On applique la récursivité:
				if (SearchInTreeView(search, ref firstFound, ref canSearch, tree, n)) { return search; }	
			}
			// Si on n'est pas encore sorti, et qu'on a trouvé un _firstFound, on le sélectionne:
			if (firstFound != null) { tree.SelectedNode = firstFound; firstFound.EnsureVisible(); }
			return search;
		}
		
		/// <summary>
		/// Procédure récursive pour la surcharge Search. Examine les noeuds enfant de parentNode, et les petits enfants, etc., et retourne true si l'un des (petits-)enfants a été trouvé et sélectionné.
		/// </summary>
		private static bool SearchInTreeView(string search, ref TreeNode firstFound, ref bool canSearch, TreeView tree, TreeNode parentNode)
		{
			foreach (TreeNode n in parentNode.Nodes)
			{
				if (n.Text.ToLower().Contains(search.ToLower())) {
					if (canSearch) { tree.SelectedNode = n; n.EnsureVisible(); return true; }
					else if (firstFound == null) { firstFound = n; } }
				canSearch = canSearch || n.IsSelected;
				if (SearchInTreeView(search, ref firstFound, ref canSearch, tree, n)) { return true; }	
			}
			return false;
		}	
		
		
		/// <summary>
		/// Insère un TreeNode dans le TreeView passé. path désigne le chemin d'accès, chaque étape étant inscrite dans le key des différents nodes. Si les nodes étapes n'existe pas, ils sont créés. excludeDoubloon indique s'il peu y avoir deux nodes du même texte dans l'arbre au même endroit (ie. enfants du même parent). Si non, le node passé remplace l'ancien.
		/// </summary>
		public static void SetNodeInTreeView(string[] path, TreeNode node, TreeView tree, bool excludeDoubloon)
		{
			TreeNodeCollection nodes = tree.Nodes;
			foreach (string s in path)
			{
				if (nodes.ContainsKey(s)) { nodes = nodes[s].Nodes; }
				else { nodes = nodes.Add(s, s).Nodes; }
			}
			if (excludeDoubloon) { foreach (TreeNode n in nodes) { if (n.Text == node.Text) { n.Remove(); } } }
			nodes.Add(node);
		
		
		
			/*TreeNode lastNode = null;
			if (path.Length == 0) { tree.Nodes.Add(node); return; }
			foreach (string s in path)
			{
				if (lastNode == null && tree.Nodes.ContainsKey(s)) { lastNode = tree.Nodes[s]; }
				else if (lastNode == null) { lastNode = tree.Nodes.Add(s, s); }
				else if (lastNode.Nodes.ContainsKey(s)) { lastNode = lastNode.Nodes[s]; }
				else { lastNode = lastNode.Nodes.Add(s, s); }
			}
			if (excludeDoubloon) { foreach (TreeNode n in lastNode.Nodes) { if (n.Text == node.Text) { n.Remove(); } } }
			lastNode.Nodes.Add(node);*/
		}
		
		
		public static void ActionOnAllNodes(TreeView tree, Action<TreeNode> action)
			{ foreach (TreeNode n in tree.Nodes) { action(n); ActionOnAllNodes(n, action); } }
		
		private static void ActionOnAllNodes(TreeNode node, Action<TreeNode> action)
			{ foreach (TreeNode n in node.Nodes) { action(n); ActionOnAllNodes(n, action); } }

	
	}
	
	
}
