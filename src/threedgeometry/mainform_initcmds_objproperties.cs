using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Drawing.Drawing2D;

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
		private My.Command[] InitCommands_ObjProperties()
		{
		
			My.Command[] cmds = new My.Command[100];
			int c = 0;

			// SELECTION:
			// ----------
			
			cmds[c++] = new My.Command("SelectAll");
			cmds[c-1].Aliases = new string[]{"SelAll"};
			cmds[c-1].Categories = new string[]{"Selection"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.SelectAll();
				};
			
			cmds[c++] = new My.Command("DeselectAll");
			cmds[c-1].Aliases = new string[]{"DesAll"};
			cmds[c-1].Categories = new string[]{"Selection"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.DeselectAll();
				};
			
			cmds[c++] = new My.Command("Select", typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"Sel"};
			cmds[c-1].Categories = new string[]{"Selection"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.SelectObjects((My.SpObject[])args[0]);
				};
			
			cmds[c++] = new My.Command("AppendToSelection", typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"App"};
			cmds[c-1].Categories = new string[]{"Selection"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.AppendToSelection((My.SpObject[])args[0]);
				};
			
			cmds[c++] = new My.Command("Deselect", typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"Des"};
			cmds[c-1].Categories = new string[]{"Selection"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.RemoveFromSelection((My.SpObject[])args[0]);
				};
			
			cmds[c++] = new My.Command("SelectedList");
			cmds[c-1].Aliases = new string[]{"SelList"};
			cmds[c-1].Categories = new string[]{"Selection"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					e.Answer = "Selected: " + My.ArrayFunctions.Join(_area.SpaceObjects.GetSelectedNames(), ",");
				};

			// MODIFICATION DES PROPRIETES DES OBJETS:
			// ---------------------------------------
			
			cmds[c++] = new My.Command("Rename", typeof(My.SpObject), typeof(string));
			cmds[c-1].Aliases = new string[]{"Ren"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.RenameObject((My.SpObject)args[0], (string)args[1]);
				};
				
			cmds[c++] = new My.Command("ChangeColor", true, typeof(Color), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"Col"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeColor((Color)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ShowName", true, typeof(bool), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"ShName"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ShowNames((bool)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("Hide", true, typeof(bool), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.Hide((bool)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ChangeLabelFont", true, typeof(Font), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"LblFont"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeLabelFont((Font)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ChangeWidth", true, typeof(float), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"Widh"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeWidth((float)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ChangeDashStyle", true, typeof(DashStyle), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"DStyle"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeDashStyle((DashStyle)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ChangeBrushStyle", true, typeof(My.BrushStyle), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"BStyle"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeBrushStyle((My.BrushStyle)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ChangeHatchStyle", true, typeof(HatchStyle), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"HStyle"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeHatchStyle((HatchStyle)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ChangeHatchColor", true, typeof(Color), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"HCol"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeHatchColor((Color)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ChangeBackColor", true, typeof(Color), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"BCol"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeBackColor((Color)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ChangeEdgeColor", true, typeof(Color), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"ECol"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeEdgeColor((Color)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ChangePointShape", true, typeof(My.PointShape), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"PtShape"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangePointShape((My.PointShape)args[0], (My.SpObject[])args[1]);
				};

			cmds[c++] = new My.Command("ChangeLabelCoords", typeof(My.SpObject), typeof(int), typeof(int));
			cmds[c-1].Aliases = new string[]{"LblCoords"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					((My.SpObject)args[0]).LabelCoordsOnWin = new Point((int)args[1], (int)args[2]);
				};
		
			cmds[c++] = new My.Command("ChangeLabelParam", typeof(My.SpObject), typeof(double));
			cmds[c-1].Aliases = new string[]{"LblP"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					((My.SpObject)args[0]).LabelOriginParam = (double)args[1];
				};
		
			cmds[c++] = new My.Command("ChangeBmpSphere", typeof(My.SpSphere));
			cmds[c-1].Aliases = new string[]{"ChBmpSph"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					((My.SpSphere)args[0]).ChangeBmpColors();
				};
		
			cmds[c++] = new My.Command("ChangeBmpSphere", typeof(My.SpSphere), typeof(bool), typeof(decimal), typeof(byte), typeof(decimal),
				typeof(decimal), typeof(decimal));
			cmds[c-1].Aliases = new string[]{"ChBmpSph"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].SyntaxDescription = new string[]{"name","convertToGray","light","alpha","red","green","blue"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					((My.SpSphere)args[0]).ChangeBmpColors(new My.ChBmpColorValues((bool)args[1], (decimal)args[2],
						(byte)args[3], (decimal)args[4], (decimal)args[5], (decimal)args[6]));
				};
		
			cmds[c++] = new My.Command("UseBmpSphere", typeof(My.SpSphere), typeof(bool));
			cmds[c-1].Aliases = new string[]{"UseBmpSph"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					((My.SpSphere)args[0]).UseBmp = (bool)args[1];
				};

			cmds[c++] = new My.Command("ShowUndefinedObjects");
			cmds[c-1].Aliases = new string[]{"ShUndef"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					string[] arr = _area.SpaceObjects.GetUndefinedObjectNames();
					if (arr.Length == 0) { e.Answer = "No undefined object."; }
					else { e.Answer = "Undefined object(s): " + My.ArrayFunctions.Join(arr, ","); }
				};
				
			cmds[c++] = new My.Command("ChangeProperties");
			cmds[c-1].Aliases = new string[]{"Props"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeProperties(false);
				};
			
			cmds[c++] = new My.Command("ChangeProperties", typeof(bool));
			cmds[c-1].Aliases = new string[]{"Props"};
			cmds[c-1].Categories = new string[]{"Objects properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeProperties((bool)args[0]);
				};
			
			// BASE PROPERTIES:
			// ----------------
			
			cmds[c++] = new My.Command("XNorm", typeof(float));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Base properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.XNorm = (float)args[0];
				};

			cmds[c++] = new My.Command("YNorm", typeof(float));//ok
			cmds[c-1].Aliases = new string[]{};	
			cmds[c-1].Categories = new string[]{"Base properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.YNorm = (float)args[0];
				};

			cmds[c++] = new My.Command("ZNorm", typeof(float));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Base properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ZNorm = (float)args[0];
				};

			cmds[c++] = new My.Command("ChangeClipRect", typeof(int), typeof(int), typeof(int), typeof(int));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Base properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ClipRect = new Rectangle((int)args[0], (int)args[1], (int)args[2], (int)args[3]);
				};
				
			Array.Resize(ref cmds, c);
			return cmds;
		
		}
	
	
	
	
	}
	
	
	
}
