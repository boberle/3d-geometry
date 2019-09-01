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
		private My.Command[] InitCommands_OtherCmds()
		{
		
			My.Command[] cmds = new My.Command[100];
			int c = 0;

			// MOVING MODE:
			// ------------
			
			cmds[c++] = new My.Command("Moving");
			cmds[c-1].Aliases = new string[]{"Mov"};
			cmds[c-1].Categories = new string[]{"Moving modes"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					SaveProperties();
					_actualMovingMode = MainForm.MovingMode.Coordinates;
					_console.WriteLine("Moving mode... Use direction keys for moving, PgUp and PgDown for rotate, Ctrl for little movements, Maj for changing Euler angles.");
				};

			cmds[c++] = new My.Command("MovingScale");
			cmds[c-1].Aliases = new string[]{"MovScale"};
			cmds[c-1].Categories = new string[]{"Moving modes"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					SaveProperties();
					_actualMovingMode = MainForm.MovingMode.Scale;
					_console.WriteLine("Moving mode... Use direction keys to change scale, Ctrl for little movements.");
				};

			cmds[c++] = new My.Command("MovingClipRect");
			cmds[c-1].Aliases = new string[]{"MovClip"};
			cmds[c-1].Categories = new string[]{"Moving modes"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					SaveProperties();
					_actualMovingMode = MainForm.MovingMode.ClipRect;
					_console.WriteLine("Moving mode... Use direction keys to move clip, Ctrl for little movements, Maj for right and bottom edges.");
				};

			cmds[c++] = new My.Command("MovingPoint", typeof(My.SpPointObject));
			cmds[c-1].Aliases = new string[]{"MovPt"};
			cmds[c-1].Categories = new string[]{"Moving modes"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					My.SpPointObject spt = (My.SpPointObject)args[0];
					if (!(spt is My.SpPoint || spt is My.SpPointOnLine || spt is My.SpPointOnPlane || spt is My.SpPointOnSphere
						|| spt is My.SpPointOnCircle || spt is My.SpPointOnFunction1OnPlane || spt is My.SpPointOnFunction2))
						{ e.Answer = "No moving mode for this object"; return; }
					if (spt.IsExtracted) { e.Answer = "No moving mod for extracted object."; return; }
					_movingObj = spt;
					SaveProperties();
					_actualMovingMode = MainForm.MovingMode.Point;
					_console.WriteLine("Moving mode... Use direction keys to move X and Y, Maj for Z, Ctrl for little movements.");
				};

			cmds[c++] = new My.Command("MovingLabel", typeof(My.SpObject));
			cmds[c-1].Aliases = new string[]{"MovLbl"};
			cmds[c-1].Categories = new string[]{"Moving modes"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					SaveProperties();
					_movingObj= (My.SpObject)args[0];
					_actualMovingMode = MainForm.MovingMode.Label;
					_console.WriteLine("Moving mode... Use direction keys to change text coordinates, Ctrl for little movements.");
				};
			
			cmds[c++] = new My.Command("MovingLabelParam", typeof(My.SpObject));
			cmds[c-1].Aliases = new string[]{"MovLblP"};
			cmds[c-1].Categories = new string[]{"Moving modes"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					SaveProperties();
					_movingObj= (My.SpObject)args[0];
					_actualMovingMode = MainForm.MovingMode.LabelParam;
					_console.WriteLine("Moving mode... Use direction keys to change text coordinates, Ctrl for little movements.");
				};
			
			cmds[c++] = new My.Command("MovingCursor", typeof(My.SpCursor));
			cmds[c-1].Aliases = new string[]{"MovCur"};
			cmds[c-1].Categories = new string[]{"Moving modes"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					SaveProperties();
					_movingObj= (My.SpCursor)args[0];
					_actualMovingMode = MainForm.MovingMode.Cursor;
					_console.WriteLine("Moving mode... Use direction keys to change cursor value, Ctrl for little movements.");
				};
			

			// INFORMATIONS ET PROPRIETES GENERALES DES OBJETS:
			// ------------------------------------------------

			cmds[c++] = new My.Command("RecalculateObj", typeof(bool), typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"Recalc"};
			cmds[c-1].Categories = new string[]{"Object informations"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					My.SpObject[] arr = (My.SpObject[])args[1];
					foreach (My.SpObject o in arr) { o.Recalculate((bool)args[0]); }
				};
			
			cmds[c++] = new My.Command("ShowInfos", typeof(My.SpObject[]));
			cmds[c-1].Aliases = new string[]{"SI"};
			cmds[c-1].Categories = new string[]{"Object informations"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					My.SpObject[] arr = (My.SpObject[])args[0]; StringBuilder sb = new StringBuilder();
					int l = arr.Length;
					for (int i=0; i<l; i++) {
						if (arr.Length > 1) { sb.AppendFormat("------- {0} --------\n", arr[i].Name); }
						sb.Append(arr[i].GetInfos() + (i==l-1 ? "" : "\n")); }
					e.Answer = sb.ToString();
				};

			cmds[c++] = new My.Command("ShowAllObjects");
			cmds[c-1].Aliases = new string[]{"all"};
			cmds[c-1].Categories = new string[]{"Object informations"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_dialogAllObjects.ShowDialog();
				};

			cmds[c++] = new My.Command("ShowFunctionArray", typeof(My.SpFunction1OnPlane), typeof(decimal), typeof(decimal), typeof(decimal));
			cmds[c-1].Aliases = new string[]{"ShFuncArr"};
			cmds[c-1].Categories = new string[]{"Object informations"};
			cmds[c-1].SyntaxDescription = new string[]{"min","max","resolution"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					e.Answer = My.ArrayFunctions.Join(((My.SpFunction1OnPlane)args[0]).GetArrayValues(
							(decimal)args[1], (decimal)args[2], (decimal)args[3]), "\n");
				};

			cmds[c++] = new My.Command("ShowFunctionArray", typeof(My.SpFunction2), typeof(decimal), typeof(decimal), typeof(decimal),
				typeof(decimal), typeof(decimal), typeof(decimal));
			cmds[c-1].Aliases = new string[]{"ShFuncArr"};
			cmds[c-1].Categories = new string[]{"Object informations"};
			cmds[c-1].SyntaxDescription = new string[]{"minX","maxX","resolutionX","minY","maxY","resolutionY"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					e.Answer = My.ArrayFunctions.Join(((My.SpFunction2)args[0]).GetArrayValues(
							(decimal)args[1], (decimal)args[2], (decimal)args[3], (decimal)args[4], (decimal)args[5], (decimal)args[6]), "\n");
				};


			// FICHIERS:
			// ---------
			
			cmds[c++] = new My.Command("CopyDrawing");
			cmds[c-1].Aliases = new string[]{"Copy"};
			cmds[c-1].Categories = new string[]{"Files"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					ExportDrawing(null);
				};
				
			cmds[c++] = new My.Command("ExportToPng", typeof(string));
			cmds[c-1].Aliases = new string[]{"Export"};
			cmds[c-1].Categories = new string[]{"Files"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					ExportDrawing((string)args[0]);
				};
				
			cmds[c++] = new My.Command("Message", typeof(string));
			cmds[c-1].Aliases = new string[]{"Msg"};
			cmds[c-1].Categories = new string[]{"Files"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_message = (string)args[0];
				};
				
			cmds[c++] = new My.Command("Message");
			cmds[c-1].Aliases = new string[]{"Msg"};
			cmds[c-1].Categories = new string[]{"Files"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					if (String.IsNullOrEmpty(_message)) { e.Answer = "No message."; }
					else { _console.Write(false, "Message " + My.FieldsParser.EscapeField(_message, ",")); }
				};
				
			cmds[c++] = new My.Command("SaveAs", typeof(string));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Files"};
			cmds[c-1].SyntaxDescription = new string[]{"filename"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					string filename;
					if ((filename = SaveConstruction((string)args[0], true)) != null)
					{
						_lastUsedFileName = filename;
						TitleForm = My.App.Title + " - " + _lastUsedFileName;
					}
				};
				
			cmds[c++] = new My.Command("Load", typeof(string));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Files"};
			cmds[c-1].SyntaxDescription = new string[]{"filename"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					string filename;
					if ((filename = LoadConstruction((string)args[0])) != null)
					{
						_lastUsedFileName = filename;
						TitleForm = My.App.Title + " - " + _lastUsedFileName;
					}
				};
				
			cmds[c++] = new My.Command("Save");
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Files"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					if (String.IsNullOrEmpty(_lastUsedFileName)) { _console.WriteLine("Use SaveAs command.", true); }
					else { SaveConstruction(_lastUsedFileName, false); }
				};
			
				
			// FORMULA:
			// --------

			cmds[c++] = new My.Command("Calc", typeof(string));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Formula"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					try {
						Func<object> method = (Func<object>)My.Formula.CreateFormulaMethod((string)args[0], null, typeof(Func<object>),
							typeof(MainForm), typeof(object), My.FormulaWorkingType.Double, null, null);
						e.Answer = method().ToString(); }
					catch (Exception exc) {
						My.ErrorHandler.ShowError(exc);
						e.Answer = "Error when making formula or when calculating."; e.AnswerIsError = true;}
					// Nettoie les objets éventuellement créés:
					My.GeoMethodsForFormulas.DeleteVirtualObjectsInBuffer();
				};
			
			cmds[c++] = new My.Command("?FormulaFunctions", typeof(string));
			cmds[c-1].Aliases = new string[]{"?f"};
			cmds[c-1].Categories = new string[]{"Formula"};
			cmds[c-1].AllowEmptyString = true;
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					e.Answer = My.ArrayFunctions.Join(My.Formula.SearchMethod((string)args[0]), "\n");
				};

			cmds[c++] = new My.Command("?FormulaFunctions", typeof(bool), typeof(string));
			cmds[c-1].Aliases = new string[]{"?f"};
			cmds[c-1].Categories = new string[]{"Formula"};
			cmds[c-1].SyntaxDescription = new string[]{"excludeOverloads","text"};
			cmds[c-1].AllowEmptyString = true;
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					e.Answer = My.ArrayFunctions.Join(My.Formula.SearchMethod((string)args[1], false, (bool)args[0], false), "\n");
				};

			cmds[c++] = new My.Command("MakeFormulaMan", typeof(int), typeof(string));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"Formula"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					if (My.Formula.MakeMan((int)args[0], (string)args[1]))
						{ _console.WriteLine(String.Format("Data saved in {0}.", (string)args[1])); }
					else
						{_console. WriteLine(String.Format("Error when saving in {0}.", (string)args[1])); }
				};

			// SYSTEME:
			// --------

			cmds[c++] = new My.Command("GC");
			cmds[c-1].Aliases = new string[0];
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Batch;
					GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
					GC.WaitForPendingFinalizers();
					System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Interactive;
					string[] array = My.SpObject.DeletedObjectsInfos();
					e.Answer = My.ArrayFunctions.Join(array, "\n");
					e.Answer += String.Format("{0}Memory usage: {1} bytes.", (String.IsNullOrEmpty(e.Answer) ? "" : "\n"),
						GC.GetTotalMemory(true).ToString("N0"));
				};

			cmds[c++] = new My.Command("SaveInHistory");
			cmds[c-1].Aliases = new string[]{"SaveHist"};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					SaveConstruction(null, true);
				};

			cmds[c++] = new My.Command("Cancel");
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					LoadConstruction(true);
				};

			cmds[c++] = new My.Command("Restore");
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					LoadConstruction(false);
				};

			cmds[c++] = new My.Command("ChangeIncrements");
			cmds[c-1].Aliases = new string[]{"chIncr"};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					Type[] types = new Type[]{typeof(int), typeof(int), typeof(double), typeof(double), typeof(int),
						typeof(int), typeof(int), typeof(int), typeof(double), typeof(double), typeof(int), typeof(int), typeof(double),
						typeof(double), typeof(double), typeof(double)};
					args = _console.Request("New increments", _movingIncr.GetConstruct(), types);
					_movingIncr = new MainForm.Increments((int)args[0], (int)args[1], (double)args[2], (double)args[3],
					(int)args[4], (int)args[5], (int)args[6], (int)args[7], (double)args[8], (double)args[9], (int)args[10],
					(int)args[11], (double)args[12], (double)args[13], (double)args[14], (double)args[15]);
				};
			
			cmds[c++] = new My.Command("ChangeIncrements", typeof(int), typeof(int), typeof(double), typeof(double), typeof(int),
						typeof(int), typeof(int), typeof(int), typeof(double), typeof(double), typeof(int), typeof(int), typeof(double),
						typeof(double), typeof(double), typeof(double));
			cmds[c-1].Aliases = new string[]{"chIncr"};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].SyntaxDescription = new string[]{"SystemCoordsBig","SystemCoordsLittle","ScaleBig","ScaleLittle","LabelBig",
				"LabelLittle","LblParamBig","LblParamLittle","ClipRectBig","ClipRectLittle","PointBig","PointLittle","AngleBig","AngleLittle"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_movingIncr = new MainForm.Increments((int)args[0], (int)args[1], (double)args[2], (double)args[3],
					(int)args[4], (int)args[5], (int)args[6], (int)args[7], (double)args[8], (double)args[9], (int)args[10],
					(int)args[11], (double)args[12], (double)args[13], (double)args[14], (double)args[15]);
				};
			
			cmds[c++] = new My.Command("New");
			cmds[c-1].Aliases = new string[0];
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					if (_console.Request<My.ConsoleYesNo>("Erase all {yes,no}") == My.ConsoleYesNo.Yes)
						{ StartNewConstruction(false); }
				};
			
			cmds[c++] = new My.Command("Exit");
			cmds[c-1].Aliases = new string[]{"Quit"};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					this.Close();
				};
				
			cmds[c++] = new My.Command("DisplayTypes", typeof(int));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					Type[] types = Assembly.GetAssembly(typeof(My.SpObject)).GetTypes();
					string[] res = My.ArrayFunctions.ShowJaggedArray(My.ArrayFunctions.MakeTypesList(ref types, typeof(My.SpObject)), (int)args[0]);
					e.Answer = My.ArrayFunctions.Join(res, "\n");
				};

			cmds[c++] = new My.Command("ChangeDecimalPlaces", typeof(byte));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					My.SpObject.DecimalPlaces = (byte)args[0];
				};

			cmds[c++] = new My.Command("ShowInfos", typeof(bool));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					My.GeoMsgSender.ShowInfos = (bool)args[0];
				};
				
			cmds[c++] = new My.Command("ShowDrawingMessages", typeof(bool));
			cmds[c-1].Aliases = new string[]{"ShDrawMsg"};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.ShowDrawingMessages = (bool)args[0];
				};
			
			cmds[c++] = new My.Command("SuspendCalculation", typeof(bool));
			cmds[c-1].Aliases = new string[]{};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_area.SuspendCalculation = (bool)args[0];
				};

			cmds[c++] = new My.Command("ShowCalculationResult", typeof(bool));
			cmds[c-1].Aliases = new string[]{"ShCalcRes"};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					My.SpObject.ShowCalculationResult = (bool)args[0];
				};

			cmds[c++] = new My.Command("EnlargeText");
			cmds[c-1].Aliases = new string[]{"Big"};
			cmds[c-1].Categories = new string[]{"System"};
			cmds[c-1].ExeDelegate = delegate(object[] args, My.ExecutionEventArgs e)
				{
					_split.Panel1Collapsed = !_split.Panel1Collapsed;
					_console.ScrollToCaret();
				};

			Array.Resize(ref cmds, c);
			return cmds;

		}


		// ---------------------------------------------------------------------------
	
		private static float _savedResolution;
		private static float _savedScale;
		
		/// <summary>
		/// Lance un dessin vers un fichier (si filename n'est pas null ou vide) ou vers le presse papiers.
		/// </summary>
		private void ExportDrawing(string filename)
		{
			My.ConsoleYesNoCancel ans; float res = 0, scale = 0;
			do
			{
				if (_savedResolution == 0) { _savedResolution = 300; }
				if (_savedScale == 0) { _savedScale = 1; }
				res = _console.Request<float>("Resolution", _savedResolution);
				scale = _console.Request<float>("Scale", _savedScale);
				_savedResolution = res; _savedScale = scale;
				float xCM = ( ( ((int)(_area.Scale * scale * (res / 96))) * _area.ClipRect.Width) / res ) * 2.54F;
				float yCM = ( ( ((int)(_area.Scale * scale * (res / 96))) * _area.ClipRect.Height) / res ) * 2.54F;
				string msg = String.Format("Image dimensions (in cm): {0:#.00} x {1:#.00}. Save {{y,n,c}}", xCM, yCM);
				ans = _console.Request<My.ConsoleYesNoCancel>(msg, My.ConsoleYesNoCancel.Yes);
			} while (ans == My.ConsoleYesNoCancel.No);
			if (ans == My.ConsoleYesNoCancel.Yes)
			{
				_console.WriteLine("Please wait..."); Application.DoEvents();
				_area.Draw(filename, res, scale, System.Drawing.Imaging.ImageFormat.Png);
			}
		}



	}
	
	
	
}
