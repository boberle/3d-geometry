using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace ThreeDGeometry
{



	/// <summary>
	/// Fournit des méthodes pour l'initialisation des commandes de la console.
	/// </summary>
	public partial class MainForm
	{




		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------




		#region METHODES




		/// <summary>
		/// Creér ou modifier un objet en invoquant le constructeur ou la méthode Alter du SpObjCtorInfos passé en argument.
		/// </summary>
		private void CreateOrAlterObject(object[] parameters, My.SpObjectCtorInfos ci)
		{
			// Bloque les messages de GeoMsgSender:
			My.GeoMsgSender.LockEvents();
			// Invoke la méthode:
			if (ci.IsCtor) {
				My.SpObject obj = (My.SpObject)((ConstructorInfo)ci.Method).Invoke(parameters);
				_area.SpaceObjects.Add(obj); }
			else {
				My.SpObject obj;
				if ((obj = _area.SpaceObjects[(string)parameters[0]]) == null) { return; }
				// Si l'objet est Extracted, on ne le modifie pas:
				if (obj.IsExtracted) { _console.WriteLine("You can't alter an extracted object.", true); return; }
				parameters = parameters.Skip(1).ToArray();
				// Il faut ABSOLUMENT vérifier que le type de l'objet et la méthode corresponde, car
				// il peut y avoir confusion avec les surcharges d'Alter menant à des objets différents:
				if (obj.GetType() != ci.Method.DeclaringType) {
					_console.WriteLine(String.Format("The type for the \"Alter\" method is {0}, but the type of the object " +
					"you selected is {1}. You probably try to change the type of the object, but you can't!",
					ci.Type.Name, obj.GetType().Name), true); return; }
				ci.Method.Invoke(obj, parameters); }
			// Débloque les messages:
			string msgs = My.GeoMsgSender.Reset(true);
			if (!String.IsNullOrEmpty(msgs)) { _console.WriteLine(msgs); }
		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Analyse les paramètres reçu de la console.
		/// </summary>
		public bool AnalyseParameter(string cmdName, string param, Type type, out object result)
		{
		
			// Sortie par défaut:
			result = null;
			
			// Si param est ??, on affiche la boîte de dialogue:
			if (param == "??") { return ShowSelectDialog(type, out result); }
			
			// Si WeightedPoint (les objets éventuellement créés sont nettoyés dans CleanOverloadTest):
			if (type == typeof(My.WeightedPoint))
			{
				string[] splitArr = param.Split(new string[]{":"}, StringSplitOptions.RemoveEmptyEntries);
				if (splitArr.Length != 2) { return false; }
				object temp;
				if (!_console.AnalyseOneParam(splitArr[1], typeof(DoubleF), "none", false, out temp)) { return false; }
				DoubleF w = (DoubleF)temp;
				if (!_console.AnalyseOneParam(splitArr[0], typeof(My.SpPointObject), "none", false, out temp)) { return false; }
				My.SpPointObject spt = (My.SpPointObject)temp;
				result = new My.WeightedPoint(spt, w);
				return true;
			}
			
			// Si SpObject:
			if (type.IsSubclassOf(typeof(My.SpObject)) || type == typeof(My.SpObject))
			{
				My.SpObject o;
				// Regarde si le nom est valide (s'il ne contient que des caractères valides pour les noms:
				Regex reg = My.SpObject.ValidNameRegex;
				MatchCollection mc = reg.Matches(param);
				StringBuilder sb = new StringBuilder(); foreach (Match m in mc) { sb.Append(m.Value); }
				bool isValidName = (param == sb.ToString());
				// Si c'est bon pour un nom valide:
				if (isValidName) {
					if ((o = _area.SpaceObjects[param, type]) != null) { result = o; return true; } }
				// Sinon, tente une formule:
				else {
					bool creation; int createOrGet = 0;
					if (cmdName == "ShowInfos" || cmdName == "Extract") { createOrGet = -1; }
					if (My.GeoMethodsForFormulas.GetObjectFromFormula(param, type, createOrGet, out o, out creation)) { result = o; return true; }
					else { result = null; return false; } }
			}
			
			// Retour si rien de tout cela:
			return false;

		}

		/// <summary>
		/// Analyse les paramètres de type tableau reçu de la console.
		/// </summary>
		public bool AnalyseArrayParameter(string cmdName, string param, Type type, bool allowEmpty, out Array result)
		{
			if (param == "??") { return ShowMultiSelectDialog(type, allowEmpty, out result); }
			result = null; return false;
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Remplit la liste de sélection des objets. type doit être SpObject ou un héritier.
		/// </summary>
		private void FillSelectObjectDialog(Type type)
		{
			_listSelectSpObject.Items.Clear(); ListViewItem item;
			foreach (My.SpObject o in _area.SpaceObjects)
			{
				if (o.GetType() == type || o.GetType().IsSubclassOf(type))
				{
					item = new ListViewItem(o.Name);
					item.SubItems.Add(o.ToString());
					item.SubItems.Add(o.TypeDescription);
					item.ForeColor = o.Color;
					_listSelectSpObject.Items.Add(item);
				}
			}
			_listSelectSpObject.AutoResizeColumns(400);
		}
	
	
		/// <summary>
		/// Affiche une boîte de dialogue de sélection, en fonction du type, pour les paramètres en ??. Retourne true si l'utilisateur a choisi quelque chose, false sinon.
		/// </summary>
		private bool ShowSelectDialog(Type type, out object result)
		{
		
			// Retour par défaut:
			result = null;
			
			// Si KnowColor:
			if (type == typeof(Color))
			{
				if (_dialogCol.ShowDialog() == My.DialogBoxClickResult.OK) { result = _dialogCol.SelectedColor; return true; }
				else { return false; }
			}
			
			// Si SpaceObject:
			else if (type == typeof(My.SpObject) || type.IsSubclassOf(typeof(My.SpObject)))
			{
				// Affiche la boîte de dialogue:
				FillSelectObjectDialog(type);
				_listSelectSpObject.MultiSelect = false;
				_listSelectSpObject.Select();
				if (_dialogSelectSpObject.ShowDialog() == My.DialogBoxClickResult.OK && _listSelectSpObject.SelectedItems.Count > 0)
					{ result = _area.SpaceObjects[_listSelectSpObject.SelectedItems[0].Text]; return true; }
				else { return false; }
			}
				
			// Si rien de tout cela, retourne false:
			return false;

		}


		/// <summary>
		/// Affiche une boîte de dialogue de sélection, en fonction du type, pour les paramètres en ??. Retourne true si l'utilisateur a choisi quelque chose, false sinon. Cette fonction permet à l'utilisateur de choisir plusieurs élements, et donc de retourner un tableau.
		/// </summary>
		private bool ShowMultiSelectDialog(Type type, bool allowEmpty, out Array result)
		{
		
			// Retour par défaut:
			result = null;
			Type elType = My.ArrayFunctions.GetElementType(type);
			
			// Si SpObject:
			if (elType == typeof(My.SpObject) || elType.IsSubclassOf(typeof(My.SpObject)))
			{
				// Affiche la boîte de dialogue:
				FillSelectObjectDialog(elType);
				_listSelectSpObject.MultiSelect = true;
				_listSelectSpObject.Select();
				if (_dialogSelectSpObject.ShowDialog() == My.DialogBoxClickResult.OK)
				{
					// Sort si pas assez d'éléments:
					if (_listSelectSpObject.SelectedItems.Count == 0 && !allowEmpty) { return false; }
					int l = _listSelectSpObject.SelectedItems.Count;
					result = Array.CreateInstance(elType, l);
					for (int i=0; i<l; i++)
						{ result.SetValue(_area.SpaceObjects[_listSelectSpObject.SelectedItems[i].Text], i); }
					return true;
				}
				else { return false; }
			}
				
			// Si rien de tout cela, retourne false:
			return false;

		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Nettoie les objets virtuels qui ont pu être créer lors de l'analyse de la surcharge.
		/// </summary>
		private void CleanOverloadTest(bool result, My.Command cmd)
		{
			if (result) { My.GeoMethodsForFormulas.ResetVirtualObjectsBuffer(); }
			else { My.GeoMethodsForFormulas.DeleteVirtualObjectsInBuffer(); }
		}



		#endregion METHODES
	


	}
	
	
	
}
