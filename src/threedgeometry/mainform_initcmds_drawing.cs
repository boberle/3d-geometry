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
		private My.Command[] InitCommands_Drawing()
		{
		
			My.Command[] cmds = new My.Command[100];
			int c = 0;

			// PROPRIETES D'AFFICHAGE DU DRAWING AREA:
			// ---------------------------------------

			cmds[c++] = new My.Command("Phi", typeof(double));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.Phi = (double)args[0];
				};

			cmds[c++] = new My.Command("Theta", typeof(double));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.Theta = (double)args[0];
				};

			cmds[c++] = new My.Command("Rotation", typeof(double));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.Rotation = (double)args[0];
				};

			cmds[c++] = new My.Command("Scale", typeof(float));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.Scale = (float)args[0];
				};

			cmds[c++] = new My.Command("DraftScale", typeof(float));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.DraftScale = (float)args[0];
				};

			cmds[c++] = new My.Command("Zoom", typeof(int));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.Zoom = (int)args[0];
				};
			
			cmds[c++] = new My.Command("ShowCoordSystem", typeof(bool));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ShowCoordinateSystem = (bool)args[0];
				};

			cmds[c++] = new My.Command("ShowClipRect", typeof(bool));//ok
			cmds[c-1].Aliases = new string[]{"ShClip"};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ShowClipRect = (bool)args[0];
				};

			cmds[c++] = new My.Command("ShowAxes", typeof(int));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ShowAxes = (int)args[0];
				};

			cmds[c++] = new My.Command("ShowXYGrid", typeof(bool));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ShowXYGrid = (bool)args[0];
				};

			cmds[c++] = new My.Command("ShowXZGrid", typeof(bool));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ShowXZGrid = (bool)args[0];
				};

			cmds[c++] = new My.Command("ShowYZGrid", typeof(bool));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ShowYZGrid = (bool)args[0];
				};

			cmds[c++] = new My.Command("ShowGraduations", typeof(bool));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ShowGraduations = (bool)args[0];
				};

			cmds[c++] = new My.Command("AxisWidth", typeof(float));//ok
			cmds[c-1].Aliases = new string[]{"AWidth"};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.AxisWidth = (float)args[0];
				};

			cmds[c++] = new My.Command("GridWidth", typeof(float));//ok
			cmds[c-1].Aliases = new string[]{"GWidth"};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.GridWidth = (float)args[0];
				};

			cmds[c++] = new My.Command("CoordinateSystemWidth", typeof(float));//ok
			cmds[c-1].Aliases = new string[]{"CSWidth"};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.CoordinateSystemWidth = (float)args[0];
				};

			cmds[c++] = new My.Command("XAxisColor", typeof(Color));//ok
			cmds[c-1].Aliases = new string[]{"XColor"};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.XAxisColor = (Color)args[0];
				};

			cmds[c++] = new My.Command("YAxisColor", typeof(Color));//ok
			cmds[c-1].Aliases = new string[]{"YColor"};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.YAxisColor = (Color)args[0];
				};

			cmds[c++] = new My.Command("ZAxisColor", typeof(Color));//ok
			cmds[c-1].Aliases = new string[]{"ZColor"};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ZAxisColor = (Color)args[0];
				};

			cmds[c++] = new My.Command("GraduationsFont", typeof(Font));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing looking"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.GraduationsFont = (Font)args[0];
				};
			
			// MODIFICATION DE LA VUE DU FORM:
			// -------------------------------
			
			cmds[c++] = new My.Command("OriginOnWindow", typeof(int), typeof(int));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"View properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.OriginOnWindow = new Point((int)args[0], (int)args[1]);
				};

			cmds[c++] = new My.Command("TranslateOrigin", typeof(int), typeof(int));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"View properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.TranslateOrigin((int)args[0], (int)args[1]);
				};

			cmds[c++] = new My.Command("CenterOrigin");
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"View properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.CenterOrigin();
				};

			cmds[c++] = new My.Command("ChangeView", typeof(My.SpPlaneObject));
			cmds[c-1].Aliases = new string[]{"View"};
			cmds[c-1].Categories = new string[]{"View properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ChangeView((My.SpPlaneObject)args[0]);
				};

			cmds[c++] = new My.Command("ChangeView", typeof(My.ViewType));
			cmds[c-1].Aliases = new string[]{"View"};
			cmds[c-1].Categories = new string[]{"View properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ChangeView((My.ViewType)args[0]);
				};

			cmds[c++] = new My.Command("ChangeView", typeof(My.SpPointObject), typeof(My.SpPointObject), typeof(My.SpPointObject));
			cmds[c-1].Aliases = new string[]{"View"};
			cmds[c-1].Categories = new string[]{"View properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ChangeView((My.SpPointObject)args[0], (My.SpPointObject)args[1], (My.SpPointObject)args[2]);
				};

			// PROPRIETES ET METHODES DE DESSINS
			// ---------------------------------
			
			cmds[c++] = new My.Command("DrawHighQuality", typeof(bool));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.DrawHighQuality = (bool)args[0];
				};

			cmds[c++] = new My.Command("AutoDraw", typeof(bool));//ok
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.AutoDraw = (bool)args[0];
				};

			cmds[c++] = new My.Command("Draw");
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.Draw();
				};

			cmds[c++] = new My.Command("DisplayOrder");
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeDisplayOrder();
				};

			cmds[c++] = new My.Command("DisplayOrder", typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SpaceObjects.ChangeDisplayOrder((My.SpObject[])args[0]);
				};

			cmds[c++] = new My.Command("RecalculateAll");
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Drawing properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.RecalculateAll();
				};

			cmds[c++] = new My.Command("ShowInfos");
			cmds[c-1].Aliases = new string[]{"SI"};
			cmds[c-1].Categories = new string[]{"Drawing properties"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					e.Answer = _area.GetInfos();
				};

			Array.Resize(ref cmds, c);
			return cmds;
		
		}
	
	
	
	
	}
	
	
	
}
