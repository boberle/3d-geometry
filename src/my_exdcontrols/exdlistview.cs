using System;
using System.Drawing;
using System.Windows.Forms;
using My.ExdControls;


namespace My
{









	// ===========================================================================
	// CLASS ExdListView
	// ===========================================================================





	

	/// <summary>
	/// Par rapport à la classe de base, offre la possibilité d'un tri sur colonne (ascendant et descendant), d'un affichage des lignes avec une couleur différente pour les lignes paires/impaires, et du drag and drop qui permet à l'utilisateur de déplacer des éléments. Lorsque la propriété MultiSelect est active, le dAd permet de déplacer plusieurs items d'un coup.
	/// </summary>
	public class ExdListView : ListView
	{






		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES




		/// <summary>
		/// Si cette prop est définie ou a une valeur différente de la prop BackColor du LV, alors les lignes paires sont affichés avec une BackColor différente des lignes impaires.
		/// </summary>
		public Color ListingStyleColor { get; set; }
		
		
		
		
		
		/// <summary>
		/// Permet la suppression d'un ou plusieurs items par l'appui sur la touche Suppr. Voir le gestionnaire d'événement ExdListView_KeyDown. True par défaut.
		/// </summary>
		public bool AllowDelete { get; set; }

		




		#endregion PROPRIETES
	
	
	
	
	
	




		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS
		
		
		

		public ExdListView()
		{

			// INSCRIPTIONS AUX EVENEMENTS
			
			// Pour le drag and dropt (si XP ou +):
			if (OSFeature.Feature.IsPresent(OSFeature.Themes))
			{
				this.AllowDrop = true;
				this.ItemDrag += new ItemDragEventHandler(ExdListView_ItemDrag);
				this.DragEnter += new DragEventHandler(ExdListView_DragEnter);
				this.DragOver += new DragEventHandler(ExdListView_DragOver);
				this.DragLeave += new EventHandler(ExdListView_DragLeave);
				this.DragDrop += new DragEventHandler(ExdListView_DragDrop);
			}

			// Pour les événements clavier:
			this.KeyDown += new KeyEventHandler(ExdListView_KeyDown);
			


			
			// PROPRIETES PAR DEFAUT:
			
			// Apparence:
			this.View = View.Details;
			this.FullRowSelect = true;
			this.HideSelection = false;
			this.ListingStyleColor = this.BackColor;

			// Comportement:
			this.Sorting = SortOrder.None;
			this.AllowColumnReorder = true;
			this.AllowSortByColumnClick = true;
			this.AllowDelete = true;
			
			// Pour le drag and drop:
			DragDropAllowedEffects = DragDropEffects.Move | DragDropEffects.Copy;
			

		}



		#endregion CONSTRUCTEURS









		// ---------------------------------------------------------------------------
		// METHODES PUBLIQUES
		// ---------------------------------------------------------------------------




		#region METHODES PUBLIQUES





		/// <summary>
		/// Apparence sous forme de feuille de listing, avec une ligne sur deux qui est dans une autre couleur. Cette procédure est toujours appelé lors des insertions, tri, etc. La couleur est déterminé par ListingStyleColor. Si cette couleur est la même que celle de la couleur d'arrière plan de la liste, cette procédure n'est pas exécuter. Elle s'exécute automatiquement après un tri de l'utilisateur par clic sur colonne, mais pas après des ajouts d'items.
		/// </summary>
		public void ListingStyle()
		{
			this.SuspendLayout();
			if (this.BackColor.ToArgb() != ListingStyleColor.ToArgb())
			{
				int j = 0;
				for (int i = 0; i < this.Items.Count; i++)
				{
					this.Items[0].UseItemStyleForSubItems = true;
					this.Items[i].BackColor = Color.Empty;
					if (j++ % 2 == 0) { this.Items[i].BackColor = ListingStyleColor; }
				}
			}
			this.ResumeLayout(false);
		}







		// ---------------------------------------------------------------------------




		/// <summary>
		/// Définit la largeur de la colonne spécifié par la clé, l'index ou la colonne en pourcentage de la largeur de la liste.
		/// </summary>
		public void SetColumnPercentWidth(string key, int width)
		{
			if (this.Columns.ContainsKey(key)) { this.Columns[key].Width = (int)Math.Floor((float)((float)width * (float)this.ClientSize.Width / 100F)); }
		}



		public void SetColumnPercentWidth(int index, int width)
		{
			if (this.Columns.Count > index) { this.Columns[index].Width = (int)Math.Floor((float)((float)width * (float)this.Size.Width / 100F)); }
		}



		public void SetColumnPercentWidth(ColumnHeader column, int width)
		{
			if (this.Columns.Contains(column)) { column.Width = (int)Math.Floor((float)((float)width / (float)this.ClientSize.Width * 100F)); }
		}







		// ---------------------------------------------------------------------------
	





		/// <summary>
		/// Ajoute une colonne en spécifiant une largeur en pourcentage par rapport à la largeur de la liste.
		/// </summary>
		public void AddColumnWithPercentWidth(string key, string text, int width)
		{
			SetColumnPercentWidth(this.Columns.Add(key, text), width);
		}


		/// <summary>
		/// Ajoute une colonne en spécifiant une largeur en pourcentage par rapport à la largeur de la liste.
		/// </summary>
		public void AddColumnWithPercentWidth(string text, int width)
		{
			SetColumnPercentWidth(this.Columns.Add(text) , width);
		}


		/// <summary>
		/// Ajoute une colonne en spécifiant une largeur en pourcentage par rapport à la largeur de la liste.
		/// </summary>
		public void AddColumnWithPercentWidth(ColumnHeader column, int width)
		{
			SetColumnPercentWidth(this.Columns.Add(column), width);
		}








		// ---------------------------------------------------------------------------
	




		/// <summary>
		/// Applique l'alignement spécifier pour toutes les colonnes.
		/// </summary>
		public void SetTextAlignForAllColumns(HorizontalAlignment align)
		{
			foreach (ColumnHeader i in this.Columns) { i.TextAlign = HorizontalAlignment.Center; }
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Redimensionne les colonnes en fonction du texte des en-têtes ou des items, selon ce qui est le plus grand.
		/// </summary>
		public void AutoResizeColumns()
		{
			int width;
			foreach (ColumnHeader col in this.Columns)
			{
				col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
				width = col.Width;
				col.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
				if (width > col.Width) { col.Width = width; }
			}
		}
		
		/// <summary>
		/// Comme AutoResizeColumns, mais en spécifiant une largeur maximale pixels.
		/// </summary>
		public void AutoResizeColumns(int max)
		{
			AutoResizeColumns();
			foreach (ColumnHeader col in this.Columns) { if (col.Width > max) { col.Width = max; } }
		}
	

		#endregion METHODES PUBLIQUES
	




















		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// GESTION DU TRI SUR COLONNE
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+




		#region GESTION DU TRI SUR COLONNE




		// ---------------------------------------------------------------------------
		// PROPRIETES POUR LE TRI SUR COLONNE
		// ---------------------------------------------------------------------------




		#region PROPRIETES POUR LE TRI SUR COLONNE



		/// <summary>
		/// Autorise le trie par colonne quand l'utilisateur clique sur l'en-tête d'une colonne.
		/// </summary>
		public bool AllowSortByColumnClick
		{
			set
			{
				if (value) {
					this.ColumnClick -= ExdListView_ColumnClick;
					this.ColumnClick += new ColumnClickEventHandler(ExdListView_ColumnClick); }
				else { this.ColumnClick -= ExdListView_ColumnClick; }
			}
		}




		#endregion PROPRIETES POUR LE TRI SUR COLONNE
	







		// ---------------------------------------------------------------------------
		// SOUS-CLASSE DE COMPARAISON
		// ---------------------------------------------------------------------------




		#region SOUS-CLASSE DE COMPARAISON




		/// <summary>
		/// Classe pour la comparaison manuel des ListViewItem, pas forcément sur la première colonne. Pour s'en servir, il faut mettre dans le gestionnaire d'événement ColumnClick les instructions:
		/// listView1.SuspendLayout();
		/// listView1.Sorting = SortOrder.None;
		/// listView1.ListViewItemSorter = (System.Collections.IComparer)(new ListViewItemComparer(e.Column));
		/// listView1.ResumeLayout(false);
		/// </summary>
		private class ListViewItemComparer : System.Collections.IComparer
		{

			// Colonne sur laquelle triée
			private int _col;

			// Variables statiques pour retenir dans quel sens il faut trier
			private static int _prevCol;
			private static SortOrder _sortOrder;

			// Constructeur statique. Initialisation des variables statiques.
			static ListViewItemComparer()
			{
				_sortOrder = SortOrder.Descending;
			}

			// Constructeur d'instance. Variables par défaut.
			public ListViewItemComparer()
			{
				_sortOrder = (_sortOrder == SortOrder.Ascending) ? (SortOrder.Descending) : (SortOrder.Ascending);
			}

			// Constructeur d'instance. Indique sur quelle colonne trier.
			public ListViewItemComparer(int column)
			{

				// Si nouvelle colonne, alors le tri est toujours ascendant.
				if (_prevCol != column)
				{
					_sortOrder = SortOrder.Ascending;
				}
				// Sinon, on récupère la valeur du static _sortOrder, et on l'inverse.
				else
				{
					if (_sortOrder == SortOrder.Ascending) { _sortOrder = SortOrder.Descending; }
					else { _sortOrder = SortOrder.Ascending; }
				}
				// Enregistrement des résultats:
				_col = column;
				_prevCol = column;
			}


			// Compare en fonction de _col, et du sens du tri.
			public int Compare(object x, object y)
			{
				int result;
				result = (String.Compare(((ListViewItem)x).SubItems[_col].Text, ((ListViewItem)y).SubItems[_col].Text));
				result *= ((_sortOrder == SortOrder.Ascending) ? (1) : (-1));
				return result;
			}

		}


		#endregion SOUS-CLASSE DE COMPARAISON








		// ---------------------------------------------------------------------------
		// GESTION EVENEMENT DE CLIC SUR COLONNE
		// ---------------------------------------------------------------------------




		#region GESTION EVENEMENT DE CLIC SUR COLONNE




		private void ExdListView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			this.SuspendLayout();
			this.Sorting = SortOrder.None;
			this.ListViewItemSorter = (System.Collections.IComparer)(new ListViewItemComparer(e.Column));
			ListingStyle();
			this.ListViewItemSorter = null;
			this.ResumeLayout(false);
		}





		#endregion GESTION EVENEMENT DE CLIC SUR COLONNE





		#endregion GESTION DU TRI SUR COLONNE








		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// GESTION DU CLAVIER
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+



		#region GESTION DU CLAVIER




		/// <summary>
		/// Touche Suppr: Supprime les items sélectionnés. Demande confirmation, sauf si touche MAJ enfoncée. Ne fonctionne que si AllowDelete est activé.
		/// </summary>
		protected virtual void ExdListView_KeyDown(object sender, KeyEventArgs e)
		{

			// Demande à l'utilisateur s'il faut supprimer, sauf si touche MAJ:
			string dlgMsg =  String.Format(MyResources.ExdListView_dialog_DeleteItem, this.SelectedItems.Count.ToString());			
			if ((this.AllowDelete) &&
					(((e.KeyCode == Keys.Delete) && (e.Shift))
					|| ((e.KeyCode == Keys.Delete) && DialogBoxes.ShowDialogQuestion(dlgMsg) == DialogBoxClickResult.Yes)))
			{
				// Pour tous les items sélectionnés, supprime...
				foreach (ListViewItem i in this.SelectedItems) { this.Items.Remove(i); }
				ListingStyle();
			}

		}





		#endregion GESTION DU CLAVIER




















		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		// GESTION DU DRAG AND DROP
		// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+




		#region GESTION DU DRAG AND DROP




		// ---------------------------------------------------------------------------
		// DECLARATIONS DE VARIABLES ET D'EVENEMENTS POUR LE DRAG AND DROP
		// ---------------------------------------------------------------------------




		#region DECLARATIONS DE VARIABLES ET D'EVENEMENTS POUR LE DRAG AND DROP




		private int _dropIndexTarget = -1;

		
		
		
		#endregion DECLARATIONS DE VARIABLES ET D'EVENEMENTS POUR LE DRAG AND DROP





		// ---------------------------------------------------------------------------
		// PROPRIETES DU DRAG AND DROP
		// ---------------------------------------------------------------------------




		#region PROPRIETES DU DRAG AND DROP




		/// <summary>
		/// Définit les opérations autorisées sur les items lors du d&d. En fait, seules deux sont prises en charge : le déplacement et la copie.
		/// </summary>
		public DragDropEffects DragDropAllowedEffects { get; set; }

	
	
		
		
		#endregion PROPRIETES DU DRAG AND DROP








		// ---------------------------------------------------------------------------
		// GESTIONS DES EVENEMENTS DU DRAG AND DROP
		// ---------------------------------------------------------------------------




		#region GESTIONS DES EVENEMENTS DU DRAG AND DROP






		/// <summary>
		/// Lorsqu'un item est déplacé, initie le d&d. Autorise le déplacement et/ou la copie en fontion des propriétés.
		/// </summary>
		protected virtual void ExdListView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			this.DoDragDrop(e.Item, DragDropAllowedEffects);
		}






		// ---------------------------------------------------------------------------





		/// <summary>
		/// Gestionnaire d'événement : Autorisation des actions.
		/// </summary>
		protected virtual void ExdListView_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = e.AllowedEffect;
		}






		// ---------------------------------------------------------------------------






		/// <summary>
		/// Gestionnaire d'événement : Dessine une ligne entre deux items, comme "marque d'insertion". Gère aussi les DragDropEffects.
		/// </summary>
		protected virtual void ExdListView_DragOver(object sender, DragEventArgs e)
		{

			// Change l'état du curseur:
			switch (e.KeyState)
			{
				case 1: e.Effect = DragDropEffects.Move; break;
				case 1 + 8: e.Effect = DragDropEffects.Copy; break; //Ctrl
				default: e.Effect = DragDropEffects.None; break;
			}

			// Trouve le point survolé par la souris et l'item correspondant, avec son rectangle de position:
			Point ptTargetItem = this.PointToClient(new Point(e.X, e.Y));
			ListViewItem item = this.GetItemAt(ptTargetItem.X, ptTargetItem.Y);
			int hoverItemIndex = (item == null ? this.Items.Count-1 : item.Index);
			Rectangle rectHoverItem = this.GetItemRect(hoverItemIndex);

			// Défilement automatique...
			if (hoverItemIndex > 0) { this.Items[hoverItemIndex - 1].EnsureVisible(); }
			if (hoverItemIndex + 1 < this.Items.Count) { this.Items[hoverItemIndex + 1].EnsureVisible(); }

			// Si la souris est dans la 2e moitié de l'item, sélectionne l'index suivant (même s'il n'existe pas: ce sera à cet index que sera inséré le (ou les) nouvel item:
			if (ptTargetItem.Y > (rectHoverItem.Top + rectHoverItem.Height / 2)) { hoverItemIndex++; }

			// Si l'item survolé ne l'a pas déjà été, ou si c'est la première fois:
			if ((hoverItemIndex != _dropIndexTarget) || (_dropIndexTarget == -1))
			{
				// Efface et suspend graphique:
				this.SuspendLayout();
				this.Refresh();
				// Défini un nouveau graphique.
				Graphics g = this.CreateGraphics();
				// Détermine le point où il faut tracer la ligne (marque d'insertion):
				Point pt;
				if (hoverItemIndex == this.Items.Count)
				{
					//Si c'est à la fin de la liste, alors on sélectionne le dernier item et on rajoute sa hauteur:
					pt = this.Items[hoverItemIndex - 1].Position;
					pt.Y += this.Items[hoverItemIndex - 1].Bounds.Height;
				}
				else
				{
					pt = this.Items[hoverItemIndex].Position;
				}
				// Longueur de la marque d'insertion:
				int insertMarkWidth = this.ClientSize.Width;
				// Défintion du pinceau:
				Pen p = new Pen(Color.Black, 3); p.StartCap = p.EndCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
				// Dessine la ligne:
				g.DrawLine(p, pt.X, pt.Y, pt.X + insertMarkWidth, pt.Y);
				// Enregistre l'index qu'aura l'élément inséré si l'utilisateur lâche maintenant la souris:
				_dropIndexTarget = hoverItemIndex;
				// Lance les op. graphiques:
				this.ResumeLayout(false);
			}

		}






		// ---------------------------------------------------------------------------






		/// <summary>
		/// Gestionnaire d'événement : Quand la souris quitte la liste, élimine la ligne ("marque d'insertion").
		/// </summary>
		protected virtual void ExdListView_DragLeave(object sender, EventArgs e)
		{
			this.Refresh();
		}






		// ---------------------------------------------------------------------------
	






		/// <summary>
		/// Gestionnaire d'événement : Quand un item est posé, tous les items sélectionnés sont déplacées à l'endroit de la "marque d'insertion".
		/// </summary>
		protected virtual void ExdListView_DragDrop(object sender, DragEventArgs e)
		{

			// Efface l'affichage:
			this.Refresh();

			this.SuspendLayout();

			// Pour tous les items sélectionnés...
			foreach (ListViewItem selectedItem in this.SelectedItems)
			{
				// Clone les items à l'endroit de la "marque d'insertion", c'est-à-dire à l'index contenu dans _dropIndexTarget:
				ListViewItem insertedItem = this.Items.Insert(_dropIndexTarget, (ListViewItem)selectedItem.Clone());
				insertedItem.Selected = true;
				// Supprime l'élément d'origine, sauf si la touche ctrl est enfoncée (auquel cas l'item est désélectionné):
				if (e.KeyState != 8) { this.Items.Remove(selectedItem); }
				else { selectedItem.Selected = false; }
				// Mettre à jour _dropIndexTarget pour l'insertion suivante s'il y a d'autres items sélectionnés:
				_dropIndexTarget = insertedItem.Index + 1;
			}

			this.ResumeLayout(false);

			// Remise à zéro de _dropIndexTarget
			_dropIndexTarget = -1;
			
			// Apparence listing:
			ListingStyle();


		}

		
		
		
		
		
		#endregion GESTIONS DES EVENEMENTS DU DRAG AND DROP
		
		
		
		
		
		
		#endregion GESTION DU DRAG AND DROP
		
		
		
		
		
		
	// END CLASS ExdListView


	}
	
	
	
	
	
}