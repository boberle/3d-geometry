using System;
using System.Drawing;
using System.Windows.Forms;


namespace My
{



	/// <summary>
	/// Classe d'argments d'événement pour ExternalDataDragged.
	/// </summary>
	public class ExternalDataDraggedEventArgs : EventArgs
	{
		public DragDropEffects Effect { get; set; }
		public ExternalDataDraggedEventArgs(DragDropEffects effect)
			{ Effect = effect; }
	}
	
	
	/// <summary>
	/// Classe d'argments d'événement pour ExternalDataDragged.
	/// </summary>
	public class ExternalDataDroppedEventArgs : EventArgs
	{
		public DragEventArgs Args { get; set; }
		public int TargetIndex { get; set; }
		public ExternalDataDroppedEventArgs(DragEventArgs args, int targetIndex)
			{ Args = args; TargetIndex = targetIndex; }
	}

	/// <summary>
	/// Délégué d'événement.
	/// </summary>
	public delegate void ExternalDataDroppedEventHandler(object sender, ExternalDataDroppedEventArgs e);
	
	/// <summary>
	/// Délégué d'événement.
	/// </summary>
	public delegate void ExternalDataDraggedEventHandler(object sender, DragEventArgs e);


	// ===========================================================================
	

	/// <summary>
	/// Fournit un ListView gérant le drag and drop des items, ainsi que des éléments de l'extérieur avec les événements ExternalDataDragged et ExternalDataDropped. La liste doit être utilisée en Small ou LargeIcon.
	/// </summary>
	public class DragAndDropListView : ListView
	{
	
		// ---------------------------------------------------------------------------
		// SOUS-CLASSES:

		/// <summary>
		/// Classe de tri par index des items.
		/// </summary>
		private class ListViewIndexComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y) { return ((ListViewItem)x).Index - ((ListViewItem)y).Index; }
		}
		
		
		// ---------------------------------------------------------------------------
		// EVENEMENTS:
		
		/// <summary>
		/// Cet événement se déclenche lorsqu'un élément de l'extérieur (pas un item) est déplacé sur le contrôle. Il est alors possible d'accepter ou non le drag and drop. S'il est accepté, il faut le gérer lors de l'événement ExternalDataDrapped. Si l'événement n'est pas géré, le drag and drop est annulé.
		/// </summary>
		public event ExternalDataDraggedEventHandler ExternalDataDragged;

		/// <summary>
		/// Cet événement se déclenche lorsqu'un élément de l'extérieur (pas un item) est déposé sur le contrôle. Il faut alors gérer le gestionnaire d'événement l'introduction des données externes. TargetIndex indique la position où il faut insérer les nouveaux items.
		/// </summary>
		public event ExternalDataDroppedEventHandler ExternalDataDropped;


		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS:
	
		/// <summary>
		/// Constructeur.
		/// </summary>
		public DragAndDropListView()
		{
		
			// Propriétés indispensables:
			AutoArrange = true;
			Sorting = SortOrder.None;
			InsertionMark.Color = SystemColors.Highlight;
			// Trie selon les index (avec AutoArrange à true):
			ListViewItemSorter = new ListViewIndexComparer();
			
			// Autres propriétés:
			HideSelection = false;
			
			// Ne fonctionne que si Win XP ou plus:
			if (OSFeature.Feature.IsPresent(OSFeature.Themes))
			{
				AllowDrop = true;
				ItemDrag += new ItemDragEventHandler(DragAndDropListView_ItemDrag);
				DragEnter += new DragEventHandler(DragAndDropListView_DragEnter);
				DragOver += new DragEventHandler(DragAndDropListView_DragOver);
				DragLeave += new EventHandler(DragAndDropListView_DragLeave);
				DragDrop += new DragEventHandler(DragAndDropListView_DragDrop);
			}
			
		}

		// ---------------------------------------------------------------------------
		// METHODES DU DRAG AND DROP:

		/// <summary>
		/// Déplace l'item à la position de la marque d'insertion, à la fin du drag and drop. Si ce n'est pas un ListViewItem, où s'il n'appartient pas à ce ListView, déclenche l'événement ExternalDataDropped.
		/// </summary>
		protected void DragAndDropListView_DragDrop(object sender, DragEventArgs e)
		{
			// Obtient l'index de la marque d'insertion:
			int targetIndex = InsertionMark.Index;
			// Si la marque d'insertion est à droite de l'item dont on a l'index, on augmente l'index d'un:
			if (InsertionMark.AppearsAfterItem) { targetIndex++; }
			// Si l'objet déplacé n'est pas un item de ce ListView, appelle l'événement puis sort directement:
			ListViewItem item = null;
			if (!e.Data.GetDataPresent(typeof(ListViewItem)) || (item = e.Data.GetData(typeof(ListViewItem)) as ListViewItem) == null)
				{ if (ExternalDataDropped != null) { ExternalDataDropped(this, new ExternalDataDroppedEventArgs(e, targetIndex)); } return; }
			// Sort si l'index cible est -1 (car alors l'item est sur lui-même):
			if (targetIndex == -1) { return; }
			// Insère des copie des items sélectionnés, dans l'ordre, à la position spécifié. Les items doivent
			// être insérés avant la suppression des items originaux, pour préservés les valeurs des index:
			int l = SelectedItems.Count;
			ListViewItem[] newItems = new ListViewItem[l];
			for (int i=0; i<l; i++) { newItems[i] = Items.Insert(targetIndex++, (ListViewItem)SelectedItems[i].Clone()); }
			// Supprime les items originaux:
			while (SelectedItems.Count > 0) { Items.Remove(SelectedItems[0]); }
			// Sélectionne les nouveaux items:
			foreach (ListViewItem i in newItems) { i.Selected = true; }
			FocusedItem = newItems[l-1];
			EnsureVisible(newItems[l-1].Index);
		}


		/// <summary>
		/// Enlève la marque d'insertion quand la souris quitte le contrôle.
		/// </summary>
		protected void DragAndDropListView_DragLeave(object sender, EventArgs e)
		{
			InsertionMark.Index = -1;
		}


		/// <summary>
		/// Déplace la marque d'insertion lorsqu'un item est déplacé.
		/// </summary>
		protected void DragAndDropListView_DragOver(object sender, DragEventArgs e)
		{
			// Obtient les coordonnées client à partir du pointeur de la souris:
      Point targetPt = PointToClient(new Point(e.X, e.Y));
			// Obtient l'objet le plus proche de du pointeur de la souris:
			int targetIndex = InsertionMark.NearestIndex(targetPt);
			// Si le pointeur n'est pas sur l'item déplacé...
			if (targetIndex > -1)
			{
				// Détermine si le pointeur est à droite ou à gauche du milieu de l'item le plus proche du pointeur
				// et détermine la propriété InsertionMark.AppearsAfterItem:
				Rectangle itemRect = GetItemRect(targetIndex);
				InsertionMark.AppearsAfterItem = (targetPt.X > itemRect.Left + (itemRect.Width / 2));
			}
			// Définit la position de la marque d'insertion. Si le pointeur est sur l'item déplacé,
			// targetItem vaut -1, et la marque disparaît:
			InsertionMark.Index = targetIndex;
		}


		/// <summary>
		/// Définit l'effet du drag and drop: autorisé seulement si c'est un item de ce ListView. Si ce n'est pas le cas, appelle ExternalDataDragged.
		/// </summary>
		protected void DragAndDropListView_DragEnter(object sender, DragEventArgs e)
		{
			ListViewItem item = null;
			if (e.Data.GetDataPresent(typeof(ListViewItem)) && (item = e.Data.GetData(typeof(ListViewItem)) as ListViewItem) != null )
			{
				e.Effect = e.AllowedEffect;
			}
			else if (ExternalDataDragged != null)
			{
				ExternalDataDraggedEventArgs args = new ExternalDataDraggedEventArgs(e.Effect);
				ExternalDataDragged(this, e);
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}


		/// <summary>
		/// Démarre un drag and drop quand un item est déplacé.
		/// </summary>
		protected void DragAndDropListView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			DoDragDrop(e.Item, DragDropEffects.Move);
		}


	}



}
