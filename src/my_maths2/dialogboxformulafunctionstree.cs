using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace My
{


	/// <summary>
	/// Affiche un dialogue avec les fonctions pour formules, internes et externes, organisées en arbre, à partir de l'attribut FormulaFunctionsCategoriesAttribute. Les fonctions avec l'attributs ExcludeFromManAttribute ne sont pas incluses.
	/// </summary>
	public class DialogBoxFormulaFunctionsTree : MyFormMessage
	{
	
	
		// ---------------------------------------------------------------------------
		// DECLARATIONS

		private TreeView _tree;
		private TextBox _txt;
		private Button _cmdOK, _cmdCancel;
		private string _search, _selectedFunction;
		
		
		// ---------------------------------------------------------------------------
		// PROPRIETES
		
		
		/// <summary>
		/// Obtient ou définit la police des arbres et du texte.
		/// </summary>
		public Font ListFont {
			get { return _tree.Font; }
			set { _tree.Font = _txt.Font = value; } }
		
		/// <summary>
		/// Obtient le nom de la fonction sélectionnée par l'utilisateur, s'il y en a un.
		/// </summary>
		public string SelectedFunction {
			get { return _selectedFunction; } }
		

		// ---------------------------------------------------------------------------
		// CONSTRUCTEUR
		
		


		/// <summary>
		/// Constructeur.
		/// </summary>
		public DialogBoxFormulaFunctionsTree()
		{
		
			// Initialisation des variables:
			_selectedFunction = String.Empty;
			
			// Initialisation de l'arbre:
			_tree = new TreeView();
			_tree.Dock = DockStyle.Fill;
			_tree.HideSelection = false;
			_tree.AfterSelect += new TreeViewEventHandler(_tree_AfterSelect);

			// Obtient les méthodes et enlève les surcharges à exclure:
			MethodInfo[] methods = Formula.GeneralAndDefMethods;
			methods = methods.Where(delegate(MethodInfo mi)
				{ return mi.GetCustomAttributes(typeof(ExcludeFromManAttribute), true).Length == 0; }).ToArray();
			// Obtient les méthodes et les catégories pour toutes les méthodes:
			string[] defIntPath = new string[]{"Internal maths functions","General"};
			string[] defExtPath = new string[]{"Other functions"};
			object[] customAttr; TreeNode node;
			string[][] cat = Array.ConvertAll(methods,
				delegate(MethodInfo mi)
				{
					customAttr = mi.GetCustomAttributes(typeof(FormulaFunctionCategoriesAttribute), true);
					if (customAttr.Length == 0 && mi.DeclaringType == typeof(Math)) { return defIntPath; }
					else if (customAttr.Length == 0) { return defExtPath; }
					else { return ((FormulaFunctionCategoriesAttribute)customAttr[0]).Path; }
				});
			// Insère les fonctions:
			int l = methods.Length;
			for (int i=0; i<l; i++)
			{
				// Sinon, insère:
				node = new TreeNode(methods[i].Name);
				// Cherche les synstaxes:
				string tag = String.Empty;
				for (int j=0; j<l; j++) {
					if (methods[i].Name == methods[j].Name && ArrayFunctions.ArrayEquals(cat[i], cat[j]))
						{ tag += ClassManager.GetMethodSyntax(methods[j], true) + "\n"; } }
				if (tag.Length > 1) { tag = tag.Substring(0, tag.Length - 1); }
				node.Tag = tag.Replace("\n", "\r\n").Replace("\t", "    ");
				ControlsFunctions.SetNodeInTreeView(cat[i], node, _tree, true);
			}

			// Tri l'arbre:
			_tree.Sort();

			// Initilisation du TextBox:
			_txt = new TextBox();
			_txt.Dock = DockStyle.Fill;
			_txt.Multiline = true;
			_txt.ReadOnly = true;
			_txt.ScrollBars = ScrollBars.Vertical;
			
			// Initialisation du TLP:
			SplitContainer split = new SplitContainer();
			split.Dock = DockStyle.Fill;
			split.Orientation = Orientation.Horizontal;
			split.SplitterDistance = (int)(_tlpBody.Height * 0.8);
			split.Panel1.Controls.Add(_tree);
			split.Panel2.Controls.Add(_txt);
			
			// Initialisation des boutons:
			_cmdOK = new Button();
			_cmdOK.Text = "OK";
			_cmdOK.Click += new EventHandler(_cmdOK_Click);
			_cmdOK.Tag = My.DialogBoxTagButton.Accept;
			_cmdCancel = new Button();
			_cmdCancel.Text = "Cancel";
			_cmdCancel.Click += delegate { _clickResult = My.DialogBoxClickResult.Cancel; this.Hide(); };
			_cmdCancel.Tag = My.DialogBoxTagButton.Cancel;
			
			// Initialisation du form:
			SubtitleBox = "Functions for formula";
			SetDialogIcon(My.DialogBoxIcon.Search);
			SetDialogMessage("Choose a function (Shift+F3 to search, F3 to find next):");
			AddButtonsCollection(new My.ButtonsCollection(1, _cmdCancel, _cmdOK), true);
			SetControl(split);
			Width = Screen.PrimaryScreen.WorkingArea.Width / 2;
			Height = (int)(Screen.PrimaryScreen.WorkingArea.Height / 1.5);
			KeyPreview = true;
			KeyDown += new KeyEventHandler(DialogBoxCommandsTree_KeyDown);
			_tree.Select();
		
		}


		// ---------------------------------------------------------------------------
		// METHODES

		
		/// <summary>
		/// Remplit la propriété SelectedFunction et sort;
		/// </summary>
		private void _cmdOK_Click(object sender, EventArgs e)
		{
			TreeNode node = _tree.SelectedNode;
			if (node != null && node.Tag is string) { _selectedFunction = node.Text; }
			_clickResult = My.DialogBoxClickResult.OK;
			this.Hide();
		}

		/// <summary>
		/// Affiche les syntaxes des fonctions dans le TextBox.
		/// </summary>
		private void _tree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			_txt.Clear();
			if (e.Node.Tag is string) { _txt.Text = (string)e.Node.Tag; }
		}
		
		/// <summary>
		/// Si l'utilisateu a appuyer sur F3 ou Shift+F3, lance une recherche.
		/// </summary>
		private void DialogBoxCommandsTree_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F3) { _search = My.ControlsFunctions.SearchInTreeView(_search, (e.Modifiers == Keys.Shift), _tree); }
		}

	}
	
	
	
	
	
}
