using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace My
{


	/// <summary>
	/// L'utilisation de la propriété DataSource sur les ComboBox souffre d'un problème : les propriétés SelectedValue/Item, etc. ne se mettent à jour, et donc ne sont utilisables, que lorsque le contrôle est affiché. Impossible de s'en servir, donc, si le contrôle n'est pas affiché, ou avant qu'il ne le soit. L'ExdComboBox permet de remédier à ce problème, car il affiche de simple string, et gère en interne DataSource sans référer à la propriété du ComboBox.
	/// </summary>
	public class ExdComboBox : ComboBox
	{





		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS


		private object _dataSource;
		
		
		

		#endregion DECLARATIONS






		// ---------------------------------------------------------------------------
		// PROPRIETES DE SELECTION
		// ---------------------------------------------------------------------------




		#region PROPRIETES DE SELECTION




		/// <summary>
		/// Obtient ou définit un objet de type IList. Lors de la définition, le premier élément est automatiquement sélectionné.
		/// </summary>
		public new object DataSource
		{
		
			get
			{
				Check();
				// Retourne l'objet source:
				return _dataSource;
			}
			
			set
			{
				Check(true);
				// Déclenche événement:
				base.OnDataSourceChanged(new EventArgs());
				// Vérifie que c'est un IList:
				if (!(value is IList)) { throw new ArgumentException("The DataSource must be IList"); }
				// Enregistre la source:
				_dataSource = (IList)value;
				// Efface la liste, puis la remplit avec des strings (DisplayMember):
				this.Items.Clear(); bool first = true;
				foreach (object i in (IList)_dataSource)
				{
					this.Items.Add(i.GetType().GetProperty(this.DisplayMember).GetValue(i, null).ToString());
					if (first) { this.SelectedIndex = 0; first = false; }
				}
			}
			
		}






		// ---------------------------------------------------------------------------
	
		
		
		
		
		/// <summary>
		/// Obtient l'objet actuellement sélectionné et affiché dans l'ExdComboBox. Si l'objet n'existe pas, la propriété reste inchangé.
		/// </summary>
		public new object SelectedItem
		{
		
			get
			{
				Check();
				// Sort si pas d'index:
				if (this.SelectedIndex == -1) { return null; }
				// Retourne l'objet sélectionné:
				return ((IList)_dataSource)[this.SelectedIndex];
			}
			
			set
			{
				Check();
				// Pour tous les objets de la liste source...
				foreach (object i in (IList)_dataSource)
				{
					// Trouve la Value de la variable d'itération:
					object iObject = i.GetType()
									.GetProperty(this.ValueMember)
									.GetValue(i, null);
					// Trouve la Value de la variable passée dans la propriété:
					object valueObject = value.GetType()
									.GetProperty(this.ValueMember)
									.GetValue(value, null);
					// Compare si ça correspond:
					if (iObject.Equals(valueObject)) { this.SelectedIndex = ((IList)_dataSource).IndexOf(i); break; }
						// Autre méthode, mais ne fonctionne que si les 'références' sont les mêmes:
						//if (i.Equals(value)) { this.SelectedIndex = ((IList)_dataSource).IndexOf(i); }
				}
			}
			
		}






		// ---------------------------------------------------------------------------






		
		/// <summary>
		/// Obtient ou définit le texte actuellement affiché dans l'ExdComboBox, et qui correspond au DisplayMember. S'il s'agit d'une définition, la propriété DisplayMember de chaque objet de la liste DataSource est évaluée, et la propriété SelectedText est modifiée si un DisplayeMember semblable au texte passé à la propriété est trouvée.
		/// </summary>
		public new string SelectedText
		{
		
			get
			{
				Check();
				// Sort si pas d'index:
				if (this.SelectedIndex == -1) { return null; }
				// Retourne le texte correspondant au DisplayMember de l'objet de la liste qui correspond au SelectedIndex du combo:
				return ((IList)_dataSource)[this.SelectedIndex]
									.GetType()
									.GetProperty(this.DisplayMember)
									.GetValue(((IList)_dataSource)[this.SelectedIndex], null).ToString();
			}
			
			set
			{
				Check();
				// Pour tous les objets de la liste:
				foreach (object i in (IList)_dataSource)
				{
					// Regarde si le texte correspond au DisplayMember de la variable d'itération:
					string iText = i.GetType().GetProperty(this.DisplayMember).GetValue(i, null).ToString();
					if (iText.Equals(value)) { this.SelectedIndex = ((IList)_dataSource).IndexOf(i); break; }
				}
			}
			
		}







		// ---------------------------------------------------------------------------







		
		/// <summary>
		/// Obtient ou définit le ValueMember de l'objet actuellement affiché dans le Combo. S'il s'agit d'une définition, la propriété ValueMember de chaque objet de la liste DataSource est évaluée, et la propriété SelectedValue est modifiée si un ValueMember semblable à l'argument passé à la propriété est trouvée.
		/// </summary>
		public new object SelectedValue
		{
		
			get
			{
				Check();
				// Sort si pas d'index:
				if (this.SelectedIndex == -1) { return null; }
				// Retourne le ValueMember de l'objet de la liste qui correspond au SelectedIndex du combo
				return ((IList)_dataSource)[this.SelectedIndex]
									.GetType()
									.GetProperty(this.ValueMember)
									.GetValue(((IList)_dataSource)[this.SelectedIndex], null);
			}
			
			set
			{
				Check();
				// Pour chaque objet de la liste...
				foreach (object i in (IList)_dataSource)
				{
					// Si le ValueMember de la variable d'itération correspond à la valeur passée à la propriété:
					object iObject = i.GetType().GetProperty(this.ValueMember).GetValue(i, null);
					if (iObject.Equals(value)) { this.SelectedIndex = ((IList)_dataSource).IndexOf(i); break; }
				}
			}
			
		}





		#endregion PROPRIETES DE SELECTION










		// ---------------------------------------------------------------------------
		// CONTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONTRUCTEURS




		/// <summary>
		/// Constructeurs.
		/// </summary>
		public ExdComboBox()
		{
			// Doit être DropDownList:
			this.DropDownStyle = ComboBoxStyle.DropDownList;
			Check(true);
		}






		#endregion CONTRUCTEURS
	












		// ---------------------------------------------------------------------------
		// METHODES DE VERIFICATION
		// ---------------------------------------------------------------------------




		#region METHODES DE VERIFICATION
		
		
		protected virtual void Check()
		{
			Check(false);
		}
		
		
		/// <summary>
		/// Vérifie que les propriétés de l'ExdComboBox sont définies de façon à ce que le ComboBox de base puisse être utilisée en tant qu'ExdComboBox. Sinon, une exception est levée.
		/// </summary>
		/// <param name="callFromConstructor"></param>
		protected virtual void Check(bool callFromConstructor)
		{
		
			// Lève exception si n'est pas dropdownlist:
			if (this.DropDownStyle != ComboBoxStyle.DropDownList) { throw new Exception("This ExdComboBox must be a DropDownList!"); }
			
			// Sort si appel du constructeur:
			if (callFromConstructor) { return; }
			
			// Autres vérifications:
			if (_dataSource == null) { throw new ArgumentNullException("DataSource not defined!"); }
			if (this.Items.Count != ((IList)_dataSource).Count) { throw new Exception("The nomber of items is different of the number of elements in the DataSource!"); }

		}
		



		#endregion METHODES DE VERIFICATION
	
	
		
		
		
		
		
		
	
	}
	
	
}
