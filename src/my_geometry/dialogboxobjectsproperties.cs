using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;

namespace My
{



	/// <summary>
	/// Affiche un dialogue qui permet de modifier les propriétés de tous les objets de la collection.
	/// </summary>
	internal class DialogBoxObjectsProperties : MyFormMessage
	{

		// ---------------------------------------------------------------------------
		// DECLARATIONS
		
		private ExdListView _list;
		private SpObjectsCollection _coll;
		private Button _cmdChange, _cmdOK, _cmdCancel;
		private ComboBox _lstProp;
		private string[] _propList;
		// Contrôle pour les modifications des propriétés:
		private CheckBox _chkTF;
		private DialogBoxSelectColor _selColor;
		private FontDialog _selFont;

		

		// ---------------------------------------------------------------------------
		// CONSTRUCTEUR
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public DialogBoxObjectsProperties()
		{
		
			// Initialisation des variables:
			_coll = SpObjectsCollection.GetInstance();
			
			// Initialisation des contrôles de propriétés:
			_chkTF = new CheckBox();
			_chkTF.Text = "Check for True, uncheck for False";
			_selColor = new DialogBoxSelectColor();
			_selFont = new FontDialog();
		
			// Initialisation des boutons:
			_cmdOK = new Button();
			_cmdOK.Text = "OK";
			_cmdOK.Tag = DialogBoxTagButton.Accept;
			_cmdOK.Click += new EventHandler(_cmdOK_Click);
			_cmdCancel = new Button();
			_cmdCancel.Text = "Cancel";
			_cmdCancel.Tag = DialogBoxTagButton.Cancel;
			_cmdCancel.Click += delegate { this.Hide(); };
			_cmdChange = new Button();
			_cmdChange.Text = "Change";
			_cmdChange.Size = MyForm.ButtonDefaultSize;
			_cmdChange.Click += new EventHandler(_cmdChange_Click);
			
			// Initialisation de la liste des propriétés:
			_propList = new string[]{"Hidden","ShowName","Color","BackColor","EdgeColor","LabelFont","PenWidth"};
			_lstProp = new ComboBox();
			_lstProp.DropDownStyle = ComboBoxStyle.DropDownList;
			_lstProp.Dock = DockStyle.Fill;
			foreach (string s in _propList) { _lstProp.Items.Add(s); }
			_lstProp.SelectedIndex = 0;
		
			// Initialisation du ListView:
			_list = new ExdListView();
			_list.AllowDelete = false;
			_list.AllowSortByColumnClick = true;
			_list.AllowColumnReorder = false;
			_list.MultiSelect = true;
			_list.HideSelection = false;
			_list.Dock = DockStyle.Fill;
			_list.Font = My.Geometry.MySettings.DefaultListFont;
			_list.Columns.Add("Name");
			_list.Columns.Add("Description");
			_list.Columns.Add("Type");
			foreach (string s in _propList) { _list.Columns.Add(s); }
			
			// Initialisation du TLP:
			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.Dock = DockStyle.Fill;
			tlp.RowCount = 2;
			tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
			tlp.ColumnCount = 2;
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tlp.SetColumnSpan(_list, 2);
			tlp.Controls.Add(_list, 0, 0);
			tlp.Controls.Add(_lstProp, 0, 1);
			tlp.Controls.Add(_cmdChange, 1, 1);

			// Initialisation du form:
			SubtitleBox = "Objects properties";
			SetDialogIcon(DialogBoxIcon.Search);
			SetDialogMessage("Edit objects properties:");
			AddButtonsCollection(new ButtonsCollection(1, _cmdCancel, _cmdOK), true);
			SetControl(tlp);
			WindowState = FormWindowState.Maximized;
			
		}


		// ---------------------------------------------------------------------------
		// METHODES:


		/// <summary>
		/// Affiche le dialogue en remplissant la liste.
		/// </summary>
		public void ShowDialog(bool onlySelected)
		{
			
			// Remplit la liste:
			_list.Items.Clear(); ListViewItem item; ListViewItem.ListViewSubItem sub; object pVal;
			int pLen = _propList.Length, c; object[] pVals; PropertyInfo pi;
			foreach (My.SpObject o in _coll)
			{
				if (onlySelected && !o.Selected) { continue; }
				item = new ListViewItem(o.Name);
				item.SubItems.Add(o.ToString());
				item.SubItems.Add(o.TypeDescription);
				c = 0; pVals = new object[pLen];
				foreach (string s in _propList)
				{
					sub = new ListViewItem.ListViewSubItem();
					sub.Name = s;
					pi = o.GetType().GetProperty(s);
					if (pi == null)
					{
						sub.Text = ""; pVals[c++] = null;
					}
					else
					{
						pVal = pi.GetValue(o, null);
						if (pVal is Color) { sub.Text = ColorFunctions.GetColorDescription((Color)pVal, ":"); }
						else if (pVal is Font) { sub.Text = GeneralParser.GetFontDescription((Font)pVal, ":"); }
						else { sub.Text = pVal.ToString(); }
						pVals[c++] = pVal;
					}
					item.SubItems.Add(sub);
				}
				item.Tag = pVals;
				item.ForeColor = o.Color;
				_list.Items.Add(item);
			}
			_list.AutoResizeColumns(300);
			
			// Affiche le dialogue:
			base.ShowDialog();
			
		}
	
	
		/// <summary>
		/// Modifie la propriété demandée.
		/// </summary>
		private void _cmdChange_Click(object sender, EventArgs e)
		{
			object pVal = null; string pStr = "ERROR", pName = _lstProp.SelectedItem.ToString(), question = pName + "?"; int tmpInt;
			switch (pName)
			{
				case "Hidden":
					if (DialogBoxes.ShowDialogInputCtrl(question, _chkTF) == DialogBoxClickResult.OK)
						{ pVal = _chkTF.Checked; pStr = pVal.ToString(); }
					break;
				case "ShowName":
					if (DialogBoxes.ShowDialogInputCtrl(question, _chkTF) == DialogBoxClickResult.OK)
						{ pVal = _chkTF.Checked; pStr = pVal.ToString(); }
					break;
				case "BackColor":
				case "EdgeColor":
				case "Color":
					if (_selColor.ShowDialog() == DialogBoxClickResult.OK)
						{ pVal = _selColor.SelectedColor; pStr = ColorFunctions.GetColorDescription((Color)pVal, ":"); }
					break;
				case "LabelFont":
					if (_selFont.ShowDialog() == DialogResult.OK)
						{ pVal = _selFont.Font; pStr = GeneralParser.GetFontDescription((Font)pVal, ":"); }
					break;
				case "PenWidth":
					if (DialogBoxes.ShowDialogInput(question) == DialogBoxClickResult.OK && Int32.TryParse(DialogBoxes.InputText, out tmpInt))
						{ pVal = tmpInt; pStr = tmpInt.ToString(); }
					break;
			}
			// Sort si pVal est null:
			if (pVal == null) { return; }
			// Pour chaque objet sélectionné:
			int index = Array.IndexOf(_propList, pName);
			foreach (ListViewItem item in _list.SelectedItems)
			{
				if (((object[])item.Tag)[index] != null) { ((object[])item.Tag)[index] = pVal; item.SubItems[pName].Text = pStr; }
				if (pName == "Color") { item.ForeColor = (Color)pVal; }
			}
		}


		/// <summary>
		/// Enregistre les modifications pour chaque objet et masque la fenêtre.
		/// </summary>
		private void _cmdOK_Click(object sender, EventArgs e)
		{
			// Pour tous les objets de la collection...
			SpObject o; object[] pVals; int c, alterCounter = 0; PropertyInfo pi; bool hasChanged;
			foreach (ListViewItem item in _list.Items)
			{
				o = _coll[item.Text];
				pVals = (object[])item.Tag; c = 0; hasChanged = false;
				// Pour toutes les propriété non nulles:
				foreach (string s in _propList)
				{
					// Obtient la propriété et ne change que si elle a changé:
					pi = o.GetType().GetProperty(s);
					if (pi != null && pi.GetValue(o, null) != pVals[c]) {
						pi.SetValue(o, pVals[c], null);
						hasChanged = true; }
					// Compteur de propriétés:
					c++;
				}
				// Compteur d'objet réellement modifiés:
				if (hasChanged) { alterCounter++; }
			}
			// Envoie message et masque:
			GeoMsgSender.SendInfos(this, String.Format("{0} objects modified.", alterCounter));
			this.Hide();
		}	

	}
	
	
	
}
