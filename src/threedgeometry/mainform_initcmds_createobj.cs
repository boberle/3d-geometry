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
	



		/// <summary>
		/// Retourne un tableau de commandes à introduire dans la console.
		/// </summary>
		private  My.Command[] InitCommands_CreateAndAlterObj()
		{
		
			// Inscrit les méthodes de création et d modification des objets de l'espace:
			
			My.Command[] cmds = new My.Command[300]; int c = 0;
			
			My.SpObjectCtorInfosCollection coll = My.SpObjectCtorInfosCollection.GetInstance();
			string cmdPrefixe, cmdPref; Type[] strParam;
			string[] strParamName = new string[]{"objName"};
			foreach (My.SpObjectCtorInfos ci in coll)
			{
				// Continue si l'objet n'est pas un objet que l'utilisateur peut créer:
				if (ci.IsAbstract || ci.IsBaseObject) { continue; }
				// Détermine le nom de la commande suivant que c'est un constructeur ou une méthode Alter:
				cmdPrefixe = (ci.IsCtor ? "Create" : "Alter");
				cmdPref = (ci.IsCtor ? "Cr" : "Alt");
				// Ajoute avant un paramètre string si c'est une méthode Alter:
				if (ci.IsCtor) { strParam = new Type[0]; }
				else { strParam = new Type[]{typeof(string)}; }
				// Ajoute les paramètres:
				cmds[c++] = new My.Command(cmdPrefixe+ci.Name, strParam.Concat(ci.ParameterTypes).ToArray());
				// Aliases:
				if (ci.ShortName != ci.OverloadName) {
					cmds[c-1].Aliases = new string[]{cmdPref+ci.ShortName,cmdPref+ci.OverloadName}; }
				else {
					cmds[c-1].Aliases = new string[]{cmdPref+ci.ShortName}; }
				// Syntaxe description:
				cmds[c-1].SyntaxDescription = (ci.IsCtor ? ci.ParameterNames : strParamName.Concat(ci.ParameterNames).ToArray());
				// Catégories:
				cmds[c-1].Categories = new string[]{(ci.IsCtor ? "Create objects" : "Alter objects"),coll.GetGroupOf(ci.Type)};
				// Enregistre le SpObjCtorInfos dans le tag:
				cmds[c-1].Tag = ci;
				// Ajoute enfin le délégué d'exécution:
				cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
					{ CreateOrAlterObject(args, (My.SpObjectCtorInfos)e.Command.Tag); };
			}
			
			// Ajoute les autres commandes:
			
			cmds[c++] = new My.Command("Alter", typeof(My.SpObject));
			cmds[c-1].Aliases = new string[]{"alt"};
			cmds[c-1].Categories = new string[]{"Alter objects"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					// Si l'objet est Extracted, on ne le modifie pas:
					if (((My.SpObject)args[0]).IsExtracted) { _console.WriteLine("You can't alter an extracted object.", true); return; }
					// La commande Alter affiche simplement la commande correspondante et les paramètres de construction.
					// L'user peut ensuite modifier les choses à sa guise.
					_console.Write(false, String.Format("Alter{0} {1}", My.SpObjectCtorInfosCollection.GetInstance().
						GetNameOf(((My.SpObject)args[0]).GetType()), ((My.SpObject)args[0]).GetCtorString(true)));
				};
			
			cmds[c++] = new My.Command("CreateSegmentsFromPolygon", typeof(My.SpPolygon), typeof(string));
			cmds[c-1].Aliases = new string[]{"crPolySegs"};
			cmds[c-1].Categories = new string[]{"Create objects","From other objects"};
			cmds[c-1].SyntaxDescription = new string[]{"poly","segsName"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.CreateSegmentsFromPolygon((My.SpPolygon)args[0], (string)args[1]);
				};
			
			cmds[c++] = new My.Command("CreateSegmentsFromSolid", typeof(My.SpSolid), typeof(string));
			cmds[c-1].Aliases = new string[]{"crSolidSegs"};
			cmds[c-1].Categories = new string[]{"Create objects","From other objects"};
			cmds[c-1].SyntaxDescription = new string[]{"solid","segsName"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.CreateSegmentsFromSolid((My.SpSolid)args[0], (string)args[1]);
				};
			
			cmds[c++] = new My.Command("CreatePolygonsFromSolid", typeof(My.SpSolid), typeof(string));
			cmds[c-1].Aliases = new string[]{"crSolidPolys"};
			cmds[c-1].Categories = new string[]{"Create objects","From other objects"};
			cmds[c-1].SyntaxDescription = new string[]{"solid","polysName"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.CreatePolygonsFromSolid((My.SpSolid)args[0], (string)args[1]);
				};
			
			cmds[c++] = new My.Command("CopyPoint", typeof(string), typeof(My.SpPoint));
			cmds[c-1].Aliases = new string[]{"copyPt"};
			cmds[c-1].Categories = new string[]{"Create objects","From other objects"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					My.SpPoint spt = (My.SpPoint)args[1];
					_area.SpaceObjects.Add(new My.SpPoint((string)args[0], new DoubleF(spt.X), new DoubleF(spt.Y), new DoubleF(spt.Z)));
				};
			
			cmds[c++] = new My.Command("CreateSystemPoints", typeof(bool));
			cmds[c-1].Aliases = new string[]{"crSysPts"};
			cmds[c-1].Categories = new string[]{"Create objects","From other objects"};
			cmds[c-1].Help = "Créer les points X,Y,Z et O si le paramètre est true.";
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.CreateSystemPoints((bool)args[0]);
				};
			
			cmds[c++] = new My.Command("DeleteSelected");
			cmds[c-1].Aliases = new string[]{"DelSel"};
			cmds[c-1].Categories = new string[]{"Manage objects"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					SaveConstruction(null, true);
					_area.SpaceObjects.DeleteSelected();
				};

			cmds[c++] = new My.Command("Delete", typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"Del"};
			cmds[c-1].Categories = new string[]{"Manage objects"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					SaveConstruction(null, true);
					_area.SpaceObjects.DeleteObjects((My.SpObject[])args[0]);
				};

			cmds[c++] = new My.Command("Replace", typeof(My.SpObject), typeof(My.SpObject));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Manage objects"};
			cmds[c-1].SyntaxDescription = new string[]{"oldObject","newObject"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					SaveConstruction(null, true);
					_area.SpaceObjects.ReplaceObject((My.SpObject)args[0], (My.SpObject)args[1]);
				};
			
			cmds[c++] = new My.Command("Extract", typeof(My.SpObject), typeof(string));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Manage objects"};
			cmds[c-1].SyntaxDescription = new string[]{"objectToExtract","newName"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					((My.SpObject)args[0]).Extract((string)args[1]);
				};			
			
			cmds[c++] = new My.Command("Extract", typeof(My.SpObject), typeof(string), typeof(string));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Manage objects"};
			cmds[c-1].SyntaxDescription = new string[]{"owner","nameOrProp","newName"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					if (((string)args[1]).StartsWith("%")) {
						try { My.SpObject obj = My.GeoMethodsForFormulas.Functions.GetOwnedObj((My.SpObject)args[0], (string)args[1]);
							obj.Extract((string)args[2]); }
						catch (Exception exc) { My.ErrorHandler.ShowError(exc); return; } }
					else {
						try { My.SpObject obj = My.GeoMethodsForFormulas.Functions.GetPropObj((My.SpObject)args[0], (string)args[1]);
							obj.Extract((string)args[2]); }
						catch (Exception exc) { My.ErrorHandler.ShowError(exc); return; } }
				};			
			
			cmds[c++] = new My.Command("ExtractFromParallelepiped", typeof(My.SpParallelepiped), typeof(string[]));
			cmds[c-1].Aliases = new string[]{"Extract"};
			cmds[c-1].Categories = new string[]{"Manage objects"};
			cmds[c-1].SyntaxDescription = new string[]{"objectToExtract","newName"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ExtractFromParallelepiped((My.SpParallelepiped)args[0], (string[])args[1]);
				};			
			
			cmds[c++] = new My.Command("ExtractFromTetrahedron", typeof(My.SpRegularTetrahedron), typeof(string[]));
			cmds[c-1].Aliases = new string[]{"Extract"};
			cmds[c-1].Categories = new string[]{"Manage objects"};
			cmds[c-1].SyntaxDescription = new string[]{"objectToExtract","newName"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ExtractFromTetrahedron((My.SpRegularTetrahedron)args[0], (string[])args[1]);
				};		
			
			cmds[c++] = new My.Command("ExtractFromPolygon", typeof(My.SpPolygon), typeof(string[]));
			cmds[c-1].Aliases = new string[]{"Extract"};
			cmds[c-1].Categories = new string[]{"Manage objects"};
			cmds[c-1].SyntaxDescription = new string[]{"objectToExtract","newNames"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ExtractFromPolygon((My.SpPolygon)args[0], (string[])args[1]);
				};		
			
			Array.Resize(ref cmds, c);
			return cmds;
		
		}
	
	
	
	
	}
	
	
	
}
