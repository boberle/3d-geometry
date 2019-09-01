using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.IO;

namespace ThreeDGeometry
{






	public partial class MainForm : My.MyForm
	{






		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS


		/// <summary>
		/// Type d'objet à modifier dans MovingMode.
		/// </summary>
		private enum MovingMode
		{
			None,
			Coordinates,
			Scale,
			Label,
			LabelParam,
			ClipRect,
			Point,
			Cursor
		}
		
		/// <summary>
		/// Liste des incréments grands et petits pour MovingMode.
		/// </summary>
		private struct Increments
		{
			public double PointBigMov, PointLittleMov;
			public int ClipRectBigMov,  ClipRectLittleMov;
			public int LabelBigMov, LabelLittleMov;
			public double LblParamBigMov, LblParamLittleMov;
			public int ScaleBigMov, ScaleLittleMov;
			public int CoordSystemBigMov, CoordSystemLittleMov;
			public double CoordSystemAngleBigMov, CoordSystemAngleLittleMov;
			public double AngleBigMov, AngleLittleMov;
			public Increments(int sysBig, int sysLit, double sysAngBig, double sysAngLit, int scaleBig,
				int scaleLit, int labelBig, int labelLit, double lblParamBig, double lblParamLit, int clipBig,
				int clipLit, double pointBig, double pointLit, double angleBig, double angleLit) : this()
				{ CoordSystemBigMov = sysBig; CoordSystemLittleMov = sysLit; CoordSystemAngleBigMov = sysAngBig;
					CoordSystemAngleLittleMov = sysAngLit; ScaleBigMov = scaleBig; ScaleLittleMov = scaleLit; LabelBigMov = labelBig;
					LabelLittleMov = labelLit; LblParamBigMov = lblParamBig; LblParamLittleMov = lblParamLit;
					ClipRectBigMov = clipBig; ClipRectLittleMov = clipLit; PointBigMov = pointBig;
					PointLittleMov = pointLit; AngleBigMov = angleBig; AngleLittleMov = angleLit; }
			public string GetConstruct()
				{ return String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
					CoordSystemBigMov, CoordSystemLittleMov, CoordSystemAngleBigMov, CoordSystemAngleLittleMov,
					ScaleBigMov, ScaleLittleMov, LabelBigMov, LabelLittleMov, LblParamBigMov, LblParamLittleMov,
					ClipRectBigMov, ClipRectLittleMov, PointBigMov, PointLittleMov, AngleBigMov, AngleLittleMov); }
		}
		
		/// <summary>
		/// Structure pour la sauvegarde des propriétés lors d'un moving mode.
		/// </summary>
		private struct MovingModeSaving
		{
			public bool AutoDraw, Quality, ShowInfos, ShowErros, ShowDrawingMsg, ShowCalcRes;
		}


		// ---------------------------------------------------------------------------
	
		
		// Contrôles:
		private My.Console _console;
		private My.DrawingArea _area;
		private Panel _drawingPanel;
		private SplitContainer _split;
		// Sauvegarde de propriété pour moving mode:
		private MovingModeSaving _savedProps;
		// Moving mode:
		private string _movingMessage;
		private MovingMode _actualMovingMode;
		private My.SpObject _movingObj;
		private Increments _movingIncr;
		// Mode restore:
		private bool _restoring;
		// Historique:
		private My.History<string[]> _cancelHistory;
		// Autre:
		private string _lastUsedFileName;
		private string _message;
		private bool _loadingWriteCmds;
		// Dialogues:
		private My.DialogBoxSelectColor _dialogCol;
		private My.ExdListView _listSelectSpObject;
		private My.MyFormMessage _dialogSelectSpObject;
		private My.DialogBoxAllObjects _dialogAllObjects;
		private My.DialogBoxFormulaFunctionsTree _dlgFormulaFuncs;




		#endregion DECLARATIONS






		// ---------------------------------------------------------------------------
		// CONSTRUCTEURS
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEURS




		/// <summary>
		/// Constructeur.
		/// </summary>
		public MainForm() : base(false)
		{
		
			// Création des contrôles pour les InitCommands:
			
			// Créer un ExdListWiew pour sélection:
			_listSelectSpObject = new My.ExdListView();
			_listSelectSpObject.AllowDelete = false;
			_listSelectSpObject.AllowColumnReorder = false;
			_listSelectSpObject.AllowSortByColumnClick = true;
			_listSelectSpObject.MultiSelect = false;
			_listSelectSpObject.Font = My.DrawingArea.DefaultListFont;
			_listSelectSpObject.Columns.Add("Name");
			_listSelectSpObject.Columns.Add("Description");
			_listSelectSpObject.Columns.Add("Type");
			// Créer le dialogue pour sélection:
			_dialogSelectSpObject = new My.MyFormMessage();
			_dialogSelectSpObject.Width = (int)(Screen.PrimaryScreen.WorkingArea.Width / 1.5);
			_dialogSelectSpObject.Height = (int)(Screen.PrimaryScreen.WorkingArea.Height / 1.5);
			_dialogSelectSpObject.AddButtonsCollection(My.DialogBoxButtons.OKCancel, 1, true);
			_dialogSelectSpObject.SetDialogMessage("Select object(s):");
			_dialogSelectSpObject.SetDialogIcon(My.DialogBoxIcon.Search);
			_dialogSelectSpObject.SetControl(_listSelectSpObject);
			_listSelectSpObject.DoubleClick += delegate { _dialogSelectSpObject.ClickResult = My.DialogBoxClickResult.OK;
			_dialogSelectSpObject.Hide(); };
			
			// Créer le dialogue pour l'affichage de tous les objets:
			_dialogAllObjects = new My.DialogBoxAllObjects();
			
			// Créer la boîte de dialogue des couleurs:
			_dialogCol = new My.DialogBoxSelectColor(); 			
		
			// Initialisation des variables:
			_cancelHistory = new My.History<string[]>(20);
			_restoring = false;
			_loadingWriteCmds = ThreeDGeometry.MySettings.WriteCommandsWhenLoading;

			// Evénement pour l'affichage des messages:
			My.GeoMsgSender.Error += delegate(object sender, My.InfosEventArgs e) { _console.WriteLine(e.Action, true); };
			My.GeoMsgSender.Infos += delegate(object sender, My.InfosEventArgs e) { _console.WriteLine(e.Action, false); };

			// Le form:
			_enableUserClosing = true;
			WindowState = FormWindowState.Maximized;
			KeyPreview = true;

			// Contrôles:
			
			// Splitter:
			_split = new SplitContainer();
			_split.Dock = DockStyle.Fill;
			_split.Orientation = Orientation.Horizontal;
			this.Load += delegate { 
				_split.SplitterDistance = (int)(_tlpBase.Height * MySettings.DrawingAreaPercentHeight); };
			// Console:
			_console = new My.Console(this);
			_console.Dock = DockStyle.Fill;
			_console.AnalyseParameter =
				(My.Console.AnalyseParameterDelegate)AnalyseParameter;
			_console.AnalyseArrayParameter =
				(My.Console.AnalyseArrayParameterDelegate)AnalyseArrayParameter;
			_console.OverloadTested += delegate(object sender, My.OverloadTestedEventArgs e)
				{ CleanOverloadTest(e.Result, e.Command); };
			_console.AutoCompleteInfos = delegate(string text)
				{ return My.ArrayFunctions.Join(My.Formula.SearchMethod(text, true, false, true), "\n"); };
			My.GeoMsgSender.ErrorPrefix = _console.PromptErrorAnswer;
			// Panel
			_drawingPanel = new Panel();
			_drawingPanel.Dock = DockStyle.Fill;
			_drawingPanel.BorderStyle = BorderStyle.Fixed3D;
			// Ajout des contrôles:
			_split.Panel1.Controls.Add(_drawingPanel);
			_split.Panel2.Controls.Add(_console);
			_tlpBase.Controls.Add(_split);
			
			// Définit un DrawingArea pour le panel:
			_area = new My.DrawingArea(_drawingPanel);
			
			// Evénements:
			this.KeyDown += new KeyEventHandler(MainForm_KeyDown);
			this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
			this.FormClosed += new FormClosedEventHandler(MainForm_FormClosed);
			
			// Par défaut:
			_actualMovingMode = MovingMode.None;
			_movingObj = null;
			_movingMessage = String.Empty;
			_console.Select();
			
			// Incréments:
			_movingIncr = new Increments(10, 1, 10, 1, 8, 1, 5, 1, 1.0, 0.1, 10, 1, 1, 0.1, 10, 1);
			
			// Initialise les commandes pour la console:
			My.Command[] cmds = new My.Command[0];
			cmds = cmds.Concat(InitCommands_Drawing()).ToArray();
			cmds = cmds.Concat(InitCommands_CreateAndAlterObj()).ToArray();
			cmds = cmds.Concat(InitCommands_ObjProperties()).ToArray();
			cmds = cmds.Concat(InitCommands_OtherCmds()).ToArray();
			_console.Commands = cmds;
			
			// Ajoute pour les paramètres de la console les fonctions de formules:
			_console.ParametersAutoComplete = Array.ConvertAll(My.Formula.GeneralAndDefMethods, delegate(MethodInfo mi) { return mi.Name; })
				.Distinct().ToArray();
			
			// Créer un dialogue pour l'affichage des fonctions sous forme d'arbre:
			_dlgFormulaFuncs = new My.DialogBoxFormulaFunctionsTree();
			_dlgFormulaFuncs.ListFont = My.DrawingArea.DefaultListFont;
			
			// Lance une nouvelle figure:
			this.Load += delegate { StartNewConstruction(true); };
			
		}



		#endregion CONSTRUCTEURS






		// ---------------------------------------------------------------------------
		// METHODES
		// ---------------------------------------------------------------------------




		#region METHODES


		/// <summary>
		/// Démarre une nouvelle construction. Supprime tous les objets, et soit charge le fichier en ligne de commande (si firsTime), soit charge le fichier par défaut enregistré dans les settings (s'il y a), ou bien remet simplement les proprietés par défaut (si firstTime est faux, puisque la première fois, c'est le constructeur de DrawingArea qui l'a fait).
		/// </summary>
		private void StartNewConstruction(bool firstTime)
		{
		
			// Efface tout:
			if (!firstTime && _area.SpaceObjects.Count > 0) { _area.SpaceObjects.DeleteAll(); }
			_lastUsedFileName = null; _message = string.Empty; this.Text = My.App.Title;
			
			// Cherche un fichier en ligne de commande et le fichier par défaut:
						// Détermine le nom du fichier chargé (s'il y a):
			string defaultFile = ThreeDGeometry.MySettings.DefaultFile, commandFile = null;
			if (firstTime && Environment.GetCommandLineArgs().Length > 1 && File.Exists(Environment.GetCommandLineArgs()[1]))
				{ commandFile = Environment.GetCommandLineArgs()[1]; }
			
			// Si fichier de commande:
			if (!String.IsNullOrEmpty(commandFile))
				{ _console.ExecuteCommand(String.Format("Load \"{0}\"", commandFile), true); }
			// Si fichier par défaut:
			else if (!String.IsNullOrEmpty(defaultFile)) {
				LoadConstruction(defaultFile); }
			// Sinon, on le fait manuellement:
			else {
				if (!firstTime) { _area.SetDefaultProperties(); }
				_area.CenterOrigin(); } // Centre l'origine, ce qui relance un Draw.
			
		}


		// ---------------------------------------------------------------------------
	
	
		private bool MovingCoordSystem(Keys modifiers, Keys key, bool big)
		{
			bool upd = false;
			// Si Maj enfoncé, change les angles d'Euler et la rotation (PgUp et PgDown):
			if (modifiers == (modifiers | Keys.Shift))
			{
				double mov = My.MathsGeo.DegToRad(big ? _movingIncr.CoordSystemAngleBigMov : _movingIncr.CoordSystemAngleLittleMov);
				if (key == Keys.Up) { _area.Theta -= mov; upd = true; }
				else if (key == Keys.Down) { _area.Theta += mov; upd = true; }
				else if (key == Keys.Right) { _area.Phi += mov; upd = true; }
				else if (key == Keys.Left) { _area.Phi -= mov; upd = true; }
				else if (key == Keys.PageUp) { _area.Rotation += mov; upd = true; }
				else if (key == Keys.PageDown) { _area.Rotation -= mov; upd = true; }
			}
			// Sinon, déplace le repère:
			else
			{
				int mov = (big ? _movingIncr.CoordSystemBigMov : _movingIncr.CoordSystemLittleMov);
				if (key == Keys.Up) { _area.TranslateOrigin(0, mov); upd = true; }
				else if (key == Keys.Down) { _area.TranslateOrigin(0, mov * -1); upd = true; }
				else if (key == Keys.Right) { _area.TranslateOrigin(mov, 0); upd = true; }
				else if (key == Keys.Left) { _area.TranslateOrigin(mov * -1, 0); upd = true; }
			}
			// Affiche les informations:
			if (upd) { _movingMessage = String.Format("Phi = {0}; Theta = {1}; Rotation = {2}; OriginOnWindow({3},{4})",
					_area.Phi, _area.Theta, _area.Rotation, _area.OriginOnWindow.X, _area.OriginOnWindow.Y); }
			return upd;
		}


		// ---------------------------------------------------------------------------
	

		private bool MovingScale(Keys key, bool big)
		{
			bool upd = false;
			int mov = (big ? _movingIncr.ScaleBigMov : _movingIncr.ScaleLittleMov);
			if (key == Keys.Up) { _area.Zoom += mov; upd = true; }
			else if (key == Keys.Down) { _area.Zoom -= mov; upd = true; }
			else if (key == Keys.Right) { _area.Zoom += mov; upd = true; }
			else if (key == Keys.Left) { _area.Zoom -= mov; upd = true; }
			// Affiche les informations:
			if (upd) { _movingMessage = String.Format("Zoom = {0}", _area.Zoom); }
			return upd;
		}


		// ---------------------------------------------------------------------------
	

		private bool MovingLabel(Keys key, bool big)
		{
			bool upd = false;
			int mov = (big ? _movingIncr.LabelBigMov : _movingIncr.LabelLittleMov);
			if (key == Keys.Up) {
				_movingObj.LabelCoordsOnWin = new Point(_movingObj.LabelCoordsOnWin.X, _movingObj.LabelCoordsOnWin.Y + mov); upd = true; }
			else if (key == Keys.Down) {
				_movingObj.LabelCoordsOnWin = new Point(_movingObj.LabelCoordsOnWin.X, _movingObj.LabelCoordsOnWin.Y - mov); upd = true; }
			else if (key == Keys.Right) {
				_movingObj.LabelCoordsOnWin = new Point(_movingObj.LabelCoordsOnWin.X + mov, _movingObj.LabelCoordsOnWin.Y); upd = true; }
			else if (key == Keys.Left) {
				_movingObj.LabelCoordsOnWin = new Point(_movingObj.LabelCoordsOnWin.X - mov, _movingObj.LabelCoordsOnWin.Y); upd = true; }
			return upd;
		}
		
		
		// ---------------------------------------------------------------------------
	

		private bool MovingLabelParam(Keys key, bool big)
		{
			bool upd = false;
			double mov = (big ? _movingIncr.LblParamBigMov : _movingIncr.LblParamLittleMov);
			if (key == Keys.Up) { _movingObj.LabelOriginParam += mov; upd = true; }
			else if (key == Keys.Down) { _movingObj.LabelOriginParam -= mov; upd = true; }
			else if (key == Keys.Right) { _movingObj.LabelOriginParam += mov; upd = true; }
			else if (key == Keys.Left) { _movingObj.LabelOriginParam -= mov; upd = true; }
			if (upd) {
				_movingObj.Recalculate(false);
				_area.CalculateObjectDrawingData(_movingObj);
				_movingMessage = My.SpObject.FormatText("Label origin parameter: {0}", _movingObj.LabelOriginParam); }
			return upd;
		}


		// ---------------------------------------------------------------------------
	

		private bool MovingCursor(Keys key, bool big)
		{
			bool upd = false;
			double mov = (big ? _movingIncr.PointBigMov : _movingIncr.PointLittleMov);
			My.SpCursor cur = (My.SpCursor)_movingObj;
			if (key == Keys.Up) { cur.AlterValue((double)(decimal)(cur.Value + mov)); upd = true; }
			else if (key == Keys.Down) { cur.AlterValue((double)(decimal)(cur.Value - mov)); upd = true; }
			else if (key == Keys.Right) { cur.AlterValue((double)(decimal)(cur.Value + mov)); upd = true; }
			else if (key == Keys.Left) { cur.AlterValue((double)(decimal)(cur.Value - mov)); upd = true; }
			if (upd) {
				cur.Recalculate(true);
				_area.CalculateObjectDrawingData(_movingObj);
				_movingMessage = My.SpObject.FormatText("Cursor value: {0}", cur.Value); }
			return upd;
		}


		// ---------------------------------------------------------------------------
	

		private bool MovingClipRect(Keys modifiers, Keys key, bool big)
		{
			bool upd = false;
			int mov = (big ? _movingIncr.ClipRectBigMov : _movingIncr.ClipRectLittleMov);
			int x = _area.ClipRect.X;
			int y = _area.ClipRect.Y;
			int width = _area.ClipRect.Width;
			int height = _area.ClipRect.Height;
			// Si Maj enfoncé, change les côtés droit et bas:
			if (modifiers == (modifiers | Keys.Shift))
			{
				if (key == Keys.Up && height-mov>5) { height -= mov; upd = true; }
				else if (key == Keys.Down) { height += mov; upd = true; }
				else if (key == Keys.Right) { width += mov; upd = true; }
				else if (key == Keys.Left && width-mov>5) { width -= mov; upd = true; }
			}
			// Sinon, déplace le rectangle:
			else
			{
				if (key == Keys.Up) { y -= mov; upd = true; }
				else if (key == Keys.Down) { y += mov; upd = true; }
				else if (key == Keys.Right) { x += mov; upd = true; }
				else if (key == Keys.Left) { x -= mov; upd = true; }
			}
			// Redéfinit le rectangle:
			_area.ClipRect = new Rectangle(x, y, width, height);
			// Affiche les informations:
			if (upd)
				{ _movingMessage = String.Format("X = {0}; Y = {1}; Width = {2}; Height = {3})", x, y, width, height); }
			return upd;
		}


		// ---------------------------------------------------------------------------
	

		private bool MovingPoint(Keys modifiers, Keys key, bool big)
		{
			bool upd = false;
			double mov = (big ? _movingIncr.PointBigMov : _movingIncr.PointLittleMov);
			// Si le point est un point polaire, change les angles:
			/*if (_movingObj is My.SpPointPolar3)
			{
				My.SpPointPolar3 pt = (My.SpPointPolar3)_movingObj;
				mov = My.MathsGeo.DegToRad(big ? MovingIncr.AngleBigMov : MovingIncr.AngleLittleMov);
				if (modifiers == (modifiers | Keys.Shift))
				{
					if (key == Keys.Up) { pt.AlterZ((double)(decimal)(pt.Z + mov)); upd = true; }
					else if (key == Keys.Down) { pt.AlterZ((double)(decimal)(pt.Z - mov)); upd = true; }
					else if (key == Keys.Right) { pt.AlterZ((double)(decimal)(pt.Z + mov)); upd = true; }
					else if (key == Keys.Left) { pt.AlterZ((double)(decimal)(pt.Z - mov)); upd = true; }
				}
				// Sinon, change x et y:
				else
				{
					if (key == Keys.Up) { pt.AlterY((double)(decimal)(pt.Y + mov)); upd = true; }
					else if (key == Keys.Down) { pt.AlterY((double)(decimal)(pt.Y - mov)); upd = true; }
					else if (key == Keys.Right) { pt.AlterX((double)(decimal)(pt.X + mov)); upd = true; }
					else if (key == Keys.Left) { pt.AlterX((double)(decimal)(pt.X - mov)); upd = true; }
				}
				if (upd) {
					_movingMessage = My.SpObject.FormatText("X = {0}; Y = {1}; Z = {2}; ",
						pt.X, pt.Y, pt.Z); }
			}*/
			// Si le point est un PointOnSphere, change les angles:
			if (_movingObj is My.SpPointOnSphere)
			{
				My.SpPointOnSphere pt = (My.SpPointOnSphere)_movingObj;
				mov = My.MathsGeo.DegToRad(big ? _movingIncr.AngleBigMov : _movingIncr.AngleLittleMov);
				if (key == Keys.Up) { pt.AlterAngles(pt.Lambda, (double)(decimal)(pt.Phi + mov * Math.Sign(Math.Cos(pt.Lambda)))); upd = true; }
				else if (key == Keys.Down) { pt.AlterAngles(pt.Lambda, (double)(decimal)(pt.Phi - mov * Math.Sign(Math.Cos(pt.Lambda)))); upd = true; }
				else if (key == Keys.Right) { pt.AlterAngles((double)(decimal)(pt.Lambda + mov), pt.Phi); upd = true; }
				else if (key == Keys.Left) { pt.AlterAngles((double)(decimal)(pt.Lambda - mov), pt.Phi); upd = true; }
				if (upd) {
					pt.Recalculate(true);
					_movingMessage = My.SpObject.FormatText("X = {0}; Y = {1}; Z = {2}; Lambda = {3}; Phi = {4}",
						pt.X, pt.Y, pt.Z, pt.Lambda, pt.Phi); }
			}
			// Si le point est un PointOnCircle, change l'angle:
			else if (_movingObj is My.SpPointOnCircle)
			{
				My.SpPointOnCircle pt = (My.SpPointOnCircle)_movingObj;
				mov = My.MathsGeo.DegToRad(big ? _movingIncr.AngleBigMov : _movingIncr.AngleLittleMov);
				if (key == Keys.Up) { pt.AlterApha((double)(decimal)(pt.Alpha + mov)); upd = true; }
				else if (key == Keys.Down) { pt.AlterApha((double)(decimal)(pt.Alpha - mov)); upd = true; }
				else if (key == Keys.Right) { pt.AlterApha((double)(decimal)(pt.Alpha + mov)); upd = true; }
				else if (key == Keys.Left) { pt.AlterApha((double)(decimal)(pt.Alpha - mov)); upd = true; }
				if (upd) {
					pt.Recalculate(true);
					_movingMessage = My.SpObject.FormatText("X = {0}; Y = {1}; Z = {2}; Theta = {3}",
						pt.X, pt.Y, pt.Z, pt.Alpha); }
			}
			// Si le point est un PointOfLine, change le paramètre t:
			else if (_movingObj is My.SpPointOnLine)
			{
				My.SpPointOnLine pt = (My.SpPointOnLine)_movingObj;
				if (key == Keys.Up) { pt.AlterTParam((double)(decimal)(pt.TParam + mov)); upd = true; }
				else if (key == Keys.Down) { pt.AlterTParam((double)(decimal)(pt.TParam - mov)); upd = true; }
				else if (key == Keys.Right) { pt.AlterTParam((double)(decimal)(pt.TParam + mov)); upd = true; }
				else if (key == Keys.Left) { pt.AlterTParam((double)(decimal)(pt.TParam - mov)); upd = true; }
				if (upd) {
					pt.Recalculate(true);
					_movingMessage = My.SpObject.FormatText("X = {0}; Y = {1}; Z = {2}; t = {3}", pt.X, pt.Y, pt.Z, pt.TParam); }
			}
			// Si le point est un point du plan, change les coordonnées 2D:
			else if (_movingObj is My.SpPointOnPlane)
			{
				My.SpPointOnPlane pt = (My.SpPointOnPlane)_movingObj;
				if (key == Keys.Up) { pt.AlterYOnPlane((double)(decimal)(pt.YOnPlane + mov)); upd = true; }
				else if (key == Keys.Down) { pt.AlterYOnPlane((double)(decimal)(pt.YOnPlane - mov)); upd = true; }
				else if (key == Keys.Right) { pt.AlterXOnPlane((double)(decimal)(pt.XOnPlane + mov)); upd = true; }
				else if (key == Keys.Left) { pt.AlterXOnPlane((double)(decimal)(pt.XOnPlane - mov)); upd = true; }
				if (upd) {
					pt.Recalculate(true);
					_movingMessage = My.SpObject.FormatText("X = {0}; Y = {1}; Z = {2}; X' = {3}; Y' = {4}", pt.X, pt.Y, pt.Z, pt.XOnPlane, pt.YOnPlane); }
			}
			// Si le point est un point sur une fonction du plan, change l'abscisse:
			else if (_movingObj is My.SpPointOnFunction1OnPlane)
			{
				My.SpPointOnFunction1OnPlane pt = (My.SpPointOnFunction1OnPlane)_movingObj;
				if (key == Keys.Right) { pt.AlterXOnPlane((double)(decimal)(pt.XOnPlane + mov)); upd = true; }
				else if (key == Keys.Left) { pt.AlterXOnPlane((double)(decimal)(pt.XOnPlane - mov)); upd = true; }
				if (upd) {
					pt.Recalculate(true);
					_movingMessage = My.SpObject.FormatText("X = {0}; Y = {1}; Z = {2}; X' = {3}; Y' = {4}", pt.X, pt.Y, pt.Z, pt.XOnPlane, pt.YOnPlane); }
			}
			// Si le point est un point sur une fonction de l'espace, change l'abscisse et l'ordonnée:
			else if (_movingObj is My.SpPointOnFunction2)
			{
				My.SpPointOnFunction2 pt = (My.SpPointOnFunction2)_movingObj;
				if (key == Keys.Up) { pt.AlterY((double)(decimal)(pt.Y + mov)); upd = true; }
				else if (key == Keys.Down) { pt.AlterY((double)(decimal)(pt.Y - mov)); upd = true; }
				else if (key == Keys.Right) { pt.AlterX((double)(decimal)(pt.X + mov)); upd = true; }
				else if (key == Keys.Left) { pt.AlterX((double)(decimal)(pt.X - mov)); upd = true; }
				if (upd) {
					pt.Recalculate(true);
					_movingMessage = My.SpObject.FormatText("X = {0}; Y = {1}; Z = {2}", pt.X, pt.Y, pt.Z); }
			}
			// Sinon, change les coordonnées:
			else if (_movingObj is My.SpPoint)
			{
				My.SpPoint pt = (My.SpPoint)_movingObj;
				// Si Maj enfoncé, change z:
				if (modifiers == (modifiers | Keys.Shift))
				{
					if (key == Keys.Up) { pt.AlterZ((double)(decimal)(pt.Z + mov)); upd = true; }
					else if (key == Keys.Down) { pt.AlterZ((double)(decimal)(pt.Z - mov)); upd = true; }
					else if (key == Keys.Right) { pt.AlterZ((double)(decimal)(pt.Z + mov)); upd = true; }
					else if (key == Keys.Left) { pt.AlterZ((double)(decimal)(pt.Z - mov)); upd = true; }
				}
				// Sinon, change x et y:
				else
				{
					if (key == Keys.Up) { pt.AlterY((double)(decimal)(pt.Y + mov)); upd = true; }
					else if (key == Keys.Down) { pt.AlterY((double)(decimal)(pt.Y - mov)); upd = true; }
					else if (key == Keys.Right) { pt.AlterX((double)(decimal)(pt.X + mov)); upd = true; }
					else if (key == Keys.Left) { pt.AlterX((double)(decimal)(pt.X - mov)); upd = true; }
				}
				if (upd) {
					pt.Recalculate(true);
					_movingMessage = My.SpObject.FormatText("X = {0}; Y = {1}; Z = {2}", pt.X, pt.Y, pt.Z); }
			}
			else
			{
				upd = true;
				_movingMessage = "No moving mode for this object.";
			}
			return upd;
		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Transforme le DrawingArea et sa collection sous forme de commandes, et enregistre le tout dans un fichier filename ou dans l'historique si filename est null. Retourne le nom du fichier utilisé (éventuellement modifié par l'ajout de l'extension), ou null si erreur. flag peut avoir deux significations : (1) Si filename n'est pas null ou vide, il indique s'il faut (true) ou non (false) demander avant d'écraser un fichier. (2) Si filename est null ou vide (on enregistre alors dans l'historique), il indique s'il faut replacer (true) ou non (false) la position de l'historique à la fin (voir la méthode AddLine de l'historique).
		/// </summary>
		private string SaveConstruction(string filename, bool flag)
		{
		
			// Sort si restoring mode:
			if (_restoring) { return null; }
		
			string[] lines = new string[25]; int c = 0;
			bool hist = String.IsNullOrEmpty(filename);
			
			// Rajoute l'extension:
			if (!hist && !filename.EndsWith(".sgeo", StringComparison.CurrentCultureIgnoreCase)) { filename += ".sgeo"; }
			
			// Demande s'il faut écraser le fichier:
			if (!hist && flag && File.Exists(filename) && _console.Request<My.ConsoleYesNo>("File exists. Replace {yes,no}") == My.ConsoleYesNo.No)
				{ _console.WriteLine("Operation cancelled."); return null; }
			
			// Informations et désactivation de l'autodraw:
			lines[c++] = String.Format("# File automatically generated - {0} - 3DGeometry: {1} - Geometry.dll: {2}",
				DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"),
				My.App.GetEntryAssemblyVersion(), _area.GetType().Assembly.GetName().Version.ToString());
			lines[c++] = String.Empty;
			lines[c++] = String.Format("# Turn off calcul, drawing and messages.");
			lines[c++] = String.Format("ShowInfos {0}", false);
			lines[c++] = String.Format("SuspendCalculation {0}", true);
			lines[c++] = String.Format("AutoDraw {0}", false);
			lines[c++] = String.Format("ShowCalculationResult {0}", false);
			lines[c++] = String.Format("ShowDrawingMessages {0}", false);
			lines[c++] = String.Empty;
			lines[c++] = String.Format("# Delete previous objets.");
			lines[c++] = String.Format("SelectAll");
			lines[c++] = String.Format("DeleteSelected");
			lines[c++] = String.Format("GC");
			lines[c++] = String.Empty;
			lines[c++] = String.Format("# Moving and system properties.");
			lines[c++] = String.Format("ChangeIncrements {0}", _movingIncr.GetConstruct());
			lines[c++] = String.Format("ChangeDecimalPlaces {0}", My.SpObject.DecimalPlaces);
			lines[c++] = String.Empty;
			lines[c++] = String.Format("# Message.");
			lines[c++] = String.Format("Message {0}", My.FieldsParser.EscapeField(_message, ","));
			lines[c++] = String.Empty;
			
			// Collection d'objet (récupère l'ORDRE DE CONSTRUCTION):
			lines[c++] = String.Format("# Objects Collection: {0} object(s).", _area.SpaceObjects.Count);
			lines[c++] = String.Empty;
			My.SpObject[] objectsList = _area.SpaceObjects.ConstructList;
			My.SpObjectCtorInfosCollection ctorInfosColl = My.SpObjectCtorInfosCollection.GetInstance();
			foreach (My.SpObject o in objectsList)
			{
				if (c + 20 >= lines.Length) { Array.Resize(ref lines, c + 100); }
				// Création ou extraction:
				if (o.IsExtracted) {
					lines[c++] = String.Format("Extract {0},{1},{2}", o.Owner.Name, o.SystemName, o.Name); }
				else {
					lines[c++] = String.Format("Create{0} {1}", ctorInfosColl.GetNameOf(o.GetType()), o.GetCtorString(true)); }
				// Propriétés générales:
				string name = o.Name;
				lines[c++] = String.Format("ChangeLabelCoords {0},{1},{2}", name, o.LabelCoordsOnWin.X, o.LabelCoordsOnWin.Y);
				lines[c++] = String.Format("ChangeLabelParam {0},{1}", name, o.LabelOriginParam);
				lines[c++] = String.Format("ChangeColor {0},{1}", My.ColorFunctions.GetColorDescription(o.Color, ":"), name);
				lines[c++] = String.Format("ShowName {0},{1}", o.ShowName, name);
				lines[c++] = String.Format("Hide {0},{1}", o.Hidden, name);
				lines[c++] = String.Format("ChangeLabelFont {0},{1}", My.GeneralParser.GetFontDescription(o.LabelFont, ":"), name);
				// Interface pour les pen et brush:
				if (o is My.IPenObject) {
					lines[c++] = String.Format("ChangeWidth {0},{1}", ((My.IPenObject)o).PenWidth, name);
					lines[c++] = String.Format("ChangeDashStyle {0},{1}", ((My.IPenObject)o).DashStyle, name); }
				else if (o is My.IBrushObject) {
					lines[c++] = String.Format("ChangeBrushStyle {0},{1}", ((My.IBrushObject)o).BrushStyle, name);
					lines[c++] = String.Format("ChangeHatchStyle {0},{1}", ((My.IBrushObject)o).HatchStyle, name);
					lines[c++] = String.Format("ChangeHatchColor {0},{1}", My.ColorFunctions.GetColorDescription(((My.IBrushObject)o).HatchColor, ":"), name); }
				// Changement des back et edge colors:
				if (o is My.SpPenBrushObject) {
					lines[c++] = String.Format("ChangeBackColor {0},{1}", My.ColorFunctions.GetColorDescription(((My.SpPenBrushObject)o).BackColor, ":"), name); }
				else if (o is My.SpBrushPenObject) {
					lines[c++] = String.Format("ChangeEdgeColor {0},{1}", My.ColorFunctions.GetColorDescription(((My.SpBrushPenObject)o).EdgeColor, ":"), name); }
				/*else if (o is My.SpBrushObject) {
					// rien à faire } */
				// Propriétés de certains objets:
				if (o is My.SpPointObject) {
					lines[c++] = String.Format("ChangePointShape {0},{1}", ((My.SpPointObject)o).PointShape, name); }
				if (o is My.SpSphere) {
					My.ChBmpColorValues val = ((My.SpSphere)o).BmpValues;
					lines[c++] = String.Format("ChangeBmpSphere {0},{1},{2},{3},{4},{5},{6}", o.Name, val.ConvertToGray, val.Light, val.Alpha, val.Red, val.Green, val.Blue);
					lines[c++] = String.Format("UseBmpSphere {0},{1}", o.Name, ((My.SpSphere)o).UseBmp); }
				lines[c++] = String.Empty;
			}
			objectsList = null;
			
			// Propriétés d'affichage:
			if (c + 50 >= lines.Length) { Array.Resize(ref lines, c + 100); }
			lines[c++] = String.Format("# Change display order.");
			lines[c++] = String.Format("DisplayOrder {0}", My.ArrayFunctions.Join(_area.SpaceObjects.GetDisplayOrderNames(), ","));
			lines[c++] = String.Empty;
			lines[c++] = String.Format("# Drawing properties.");
			lines[c++] = String.Empty;
			lines[c++] = String.Format("ShowCoordSystem {0}", _area.ShowCoordinateSystem);
			lines[c++] = String.Format("DrawHighQuality {0}", _area.DrawHighQuality);
			lines[c++] = String.Format("ShowClipRect {0}", _area.ShowClipRect);
			lines[c++] = String.Format("ShowAxes {0}", _area.ShowAxes);
			lines[c++] = String.Format("ShowGraduations {0}", _area.ShowGraduations);
			lines[c++] = String.Format("ShowXYGrid {0}", _area.ShowXYGrid);
			lines[c++] = String.Format("ShowXZGrid {0}", _area.ShowXZGrid);
			lines[c++] = String.Format("ShowYZGrid {0}", _area.ShowYZGrid);
			lines[c++] = String.Format("AxisWidth {0}", _area.AxisWidth);
			lines[c++] = String.Format("GridWidth {0}", _area.GridWidth);
			lines[c++] = String.Format("CoordinateSystemWidth {0}", _area.CoordinateSystemWidth);
			lines[c++] = String.Format("XAxisColor {0}", My.ColorFunctions.GetColorDescription(_area.XAxisColor, ":"));
			lines[c++] = String.Format("YAxisColor {0}", My.ColorFunctions.GetColorDescription(_area.YAxisColor, ":"));
			lines[c++] = String.Format("ZAxisColor {0}", My.ColorFunctions.GetColorDescription(_area.ZAxisColor, ":"));
			lines[c++] = String.Format("GraduationsFont {0}", My.GeneralParser.GetFontDescription(_area.GraduationsFont, ":"));
			lines[c++] = String.Format("OriginOnWindow {0},{1}", _area.OriginOnWindow.X, _area.OriginOnWindow.Y);
			lines[c++] = String.Format("Phi {0}", _area.Phi);
			lines[c++] = String.Format("Theta {0}", _area.Theta);
			lines[c++] = String.Format("Rotation {0}", _area.Rotation);
			lines[c++] = String.Format("XNorm {0}", _area.XNorm);
			lines[c++] = String.Format("YNorm {0}", _area.YNorm);
			lines[c++] = String.Format("ZNorm {0}", _area.ZNorm);
			lines[c++] = String.Format("Scale {0}", _area.Scale);
			lines[c++] = String.Format("DraftScale {0}", _area.DraftScale);
			lines[c++] = String.Format("Zoom {0}", _area.Zoom);
			lines[c++] = String.Format("ChangeClipRect {0},{1},{2},{3}", _area.ClipRect.X, _area.ClipRect.Y,
				_area.ClipRect.Width, _area.ClipRect.Height); // Doit se trouver après zoom
			lines[c++] = String.Format("ShowCalculationResult {0}", My.SpObject.ShowCalculationResult);
			lines[c++] = String.Format("ShowDrawingMessages {0}", _area.ShowDrawingMessages);
			lines[c++] = String.Format("ShowInfos {0}", My.GeoMsgSender.ShowInfos);
			lines[c++] = String.Format("SuspendCalculation {0}", _area.SuspendCalculation);
			lines[c++] = String.Format("AutoDraw {0}", _area.AutoDraw);
			lines[c++] = String.Format("Draw");
			Array.Resize(ref lines, c);
			
			// Enregistre:
			if (hist) {
				_cancelHistory.AddLine(lines, flag); }
			else {
				string errMsg = String.Format("Error when saving \"{0}\".", filename);
				if (!My.FilesAndStreams.WriteAllLines(filename, lines)) { _console.WriteLine(errMsg, true); return null; }
				_console.WriteLine(String.Format("Constuction saved in \"{0}\"", filename)); }
			return filename;
		
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Exécute les commandes du fichier dans la console. Retourne le nom du fichier utilisé (éventuellement modifié par l'ajout de l'extension), ou null si erreur.
		/// </summary>
		private string LoadConstruction(string filename)
		{
			if (!filename.EndsWith(".sgeo", StringComparison.CurrentCultureIgnoreCase)) { filename += ".sgeo"; }
			string[] lines;
			if ((lines = My.FilesAndStreams.ReadAllLines(filename)) == null)
				{ _console.WriteLine(String.Format("Error when reading \"{0}\".", filename)); return null; }
			_restoring = true;
			_message = String.Empty;
			foreach (string s in lines)
				{ if (!String.IsNullOrEmpty(s) && !s.StartsWith("#")) { _console.ExecuteCommand(s, _loadingWriteCmds); } }
			_console.WriteLine(String.Format("File \"{0}\" loaded. Use \"Cls\" to clear console.", filename));
			_restoring = false;
			return filename;
		}
	
		/// <summary>
		/// Exécute les commandes de l'historique. Si back est vrai, recule dans l'historique, sinon avance.
		/// </summary>
		private void LoadConstruction(bool back)
		{
			if (_cancelHistory.Position >= _cancelHistory.Length) { SaveConstruction(null, false); }
			_restoring = true;
			string[] lines = (back ? _cancelHistory.Back() : _cancelHistory.Forward());
			if (lines == null) { _console.WriteLine("No more step in history."); _restoring = false; return; }
			foreach (string s in lines)
				{ if (!String.IsNullOrEmpty(s) && !s.StartsWith("#")) { _console.ExecuteCommand(s, _loadingWriteCmds); } }
			_restoring = false;
			_console.WriteLine(String.Format("Step {0} on {1} loaded from history.", _cancelHistory.Position + 1, _cancelHistory.Length));
		}



		#endregion METHODES






		// ---------------------------------------------------------------------------
		// EVENEMENTS
		// ---------------------------------------------------------------------------




		#region EVENEMENTS

	
		/// <summary>
		/// Sauve les propriétés de _area avant Moving.
		/// </summary>
		private void SaveProperties()
		{
			_savedProps.AutoDraw = _area.AutoDraw;
			_savedProps.Quality = _area.DrawHighQuality;
			_savedProps.ShowInfos = My.GeoMsgSender.ShowInfos;
			_savedProps.ShowErros = My.GeoMsgSender.ShowErrors;
			_savedProps.ShowCalcRes = My.SpObject.ShowCalculationResult;
			_savedProps.ShowDrawingMsg = _area.ShowDrawingMessages;
		}


		/// <summary>
		/// Gère Moving : désactive la gestion des touches dans le form, et donc dans la console, et gère le déplacement d'objet avec les flèches de direction. Annule avec Echap.
		/// </summary>
		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
		
			// Si Shift+F1, affiche la boîte de dialogue de sélection de fonctions pour formule:
			if (e.KeyCode == Keys.F1 && e.Modifiers == Keys.Shift) {
				if (_dlgFormulaFuncs.ShowDialog() == My.DialogBoxClickResult.OK)
					{ _console.Write(false, _dlgFormulaFuncs.SelectedFunction); }
				return; }
		
			// Si F2, affiche la boîte de dialogue de sélection d'objet:
			if (e.KeyCode == Keys.F2 && e.Modifiers != Keys.Shift) {
				Array objs;
				if (!ShowMultiSelectDialog(typeof(My.SpObject), false, out objs)) { return; }
				_console.Write(false, My.ArrayFunctions.Join((My.SpObject[])objs, delegate(My.SpObject o) { return o.Name; }, ","));
				return; }
			
			// Si Shift+F2, affiche le texte en grand ou en petit:
			if (e.KeyCode == Keys.F2 && e.Modifiers == Keys.Shift) {
				_split.Panel1Collapsed = !_split.Panel1Collapsed;
				_console.ScrollToCaret(); }
			
			// Si F3, affiche AllObjects:
			if (e.KeyCode == Keys.F3) { _dialogAllObjects.ShowDialog(); return; }
			
		
			// Sort si pas MovingMode:
			if (_actualMovingMode == MovingMode.None) { return; }
			
			// Annule si Echap:
			if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
			{
				// Remet les propriétés:
				_actualMovingMode = MovingMode.None;
				_console.WriteLineTempStop();
				My.GeoMsgSender.ShowInfos = _savedProps.ShowInfos;
				My.GeoMsgSender.ShowErrors = _savedProps.ShowErros;
				_area.DrawHighQuality = _savedProps.Quality;
				_area.AutoDraw = _savedProps.AutoDraw;
				My.SpObject.ShowCalculationResult = _savedProps.ShowCalcRes;
				_area.ShowDrawingMessages = _savedProps.ShowDrawingMsg;
				_area.Draw();
				// Affiche les objets indéfinis:
				string[] undefined = _area.SpaceObjects.GetUndefinedObjectNames();
				_console.WriteLine("Undefined objects: " + (undefined.Length==0 ? "None" :
					My.ArrayFunctions.Join(undefined, ",")));
				return;
			}
			
			// Interrompt l'affichage des informations, et AutoDraw:
			My.GeoMsgSender.ShowInfos = false;
			My.GeoMsgSender.ShowErrors = false;
			_area.AutoDraw = false;
			_area.DrawHighQuality = false;
			My.SpObject.ShowCalculationResult = false;
			_area.ShowDrawingMessages = false;
			
			// Si Control enfoncé, petits déplacements, sinon grands:
			bool big = !(e.Modifiers == (e.Modifiers | Keys.Control));
			
			// Variable pour le rafraîchissement de l'affichage:
			bool upd = false;
			
			// Choisit la procédure en fonction de ce qu'il faut déplacer:
			switch (_actualMovingMode)
			{
				case MovingMode.Coordinates: upd = MovingCoordSystem(e.Modifiers, e.KeyCode, big); break;
				case MovingMode.Scale: upd = MovingScale(e.KeyCode, big); break;
				case MovingMode.Label: upd = MovingLabel(e.KeyCode, big); break;
				case MovingMode.LabelParam: upd = MovingLabelParam(e.KeyCode, big); break;
				case MovingMode.ClipRect: upd = MovingClipRect(e.Modifiers, e.KeyCode, big); break;
				case MovingMode.Point: upd = MovingPoint(e.Modifiers, e.KeyCode, big); break;
				case MovingMode.Cursor: upd = MovingCursor(e.KeyCode, big); break;
			}
			
			// Redessine en mauvaise qualité:
			if (upd) { _area.Draw();}
			
			// Affiche les informations:
			if (upd && !String.IsNullOrEmpty(_movingMessage)) { _console.WriteLineTemp(_movingMessage); }
			
			// Supprime la gestion sous-jacente des touches:
			e.Handled = true;
			e.SuppressKeyPress = true;
			
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Demande confirmation avant fermeture, et l'empêche si MovingMode.
		/// </summary>
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_actualMovingMode != MovingMode.None) { e.Cancel = true; return; }
			_console.Select();
			if (_console.Request<My.ConsoleYesNo>("Exit {yes,no}") == My.ConsoleYesNo.No) { e.Cancel = true; }
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Enregistre les paramètres.
		/// </summary>
		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			//MySettings.DrawingAreaPercentHeight = (float)_split.SplitterDistance / (float)this.tlpBase.Height;
		}


		#endregion EVENEMENTS
	
	
	
	
	}



}
