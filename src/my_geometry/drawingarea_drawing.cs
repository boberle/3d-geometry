using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Drawing.Imaging;

namespace My
{




	public partial class DrawingArea
	{






		// ---------------------------------------------------------------------------
		// DRAWING
		// ---------------------------------------------------------------------------




		#region DRAWING




		/// <summary>
		/// Enregistre l'image définie par le ClipRect dans un fichier, ou dans le presse papier si filename est vide ou null.
		/// </summary>
		public void Draw(string filename, float res, float scale, ImageFormat format)
		{
		
			// Il faut, pour pouvoir n'enregistrer que la découpe de ClipRect, mettre celui dans le coin supérieur gauche du form
			// et décaler le tout avec. On modifie alors OrigineOnWindow:
			float gScale = (int)(__Scale * scale * (res / 96));
			Point gOrigin = new Point((int)(__ClipRect.X * -1 * gScale), (int)(__ClipRect.Y * -1 * gScale));
		
			// Définit un graphics, et le règle:
			Bitmap bmp = new Bitmap((int)(__ClipRect.Width * gScale), (int)(__ClipRect.Height * gScale), PixelFormat.Format32bppArgb);
			Graphics g = Graphics.FromImage(bmp);
			g.Clip = new Region(new Rectangle(0, 0, bmp.Width, bmp.Height));
			g.PageUnit = GraphicsUnit.Pixel;
			// Dessine en haute qualité:
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			// Translate l'origine du repère du form, puis scale et couleur de fond:
			g.TranslateTransform(gOrigin.X, gOrigin.Y);
			g.ScaleTransform(gScale, gScale);
			g.Clear(Color.Empty);
			
			// Lance le dessin:
			GraphicsPath path = new GraphicsPath();
			bool savedShowClip = __ShowClipRect;
			__ShowClipRect = false;
			this.Draw(ref g, ref path);
			__ShowClipRect = savedShowClip;
			
			// Change la résolution et enregistre dans le fichier ou le presse papier:
			bmp.SetResolution(res, res);
			try
			{
				if (String.IsNullOrEmpty(filename)) {
					// Pour conserver le format et la transparence, on enregistre dans un fichier temporaire, et on place
					// le fichier dans le presse papier:
					string tmpFile = Path.Combine(Path.GetTempPath(), "GeometryImg.png.tmp");
					bmp.Save(tmpFile, format);
					Clipboard.SetData(DataFormats.FileDrop,  new string[]{tmpFile});
					SendInfos("Temporary filename save to clipboard."); }
				else {
					bmp.Save(filename, format);
					SendInfos("Image saved in " + filename); }
			}
			catch (Exception exc) { My.ErrorHandler.ShowError(exc); }
			
			// Supprime le grahics:
			path.Dispose(); g.Dispose();
		
		}
		
		
		/// <summary>
		/// Dessine la construction sur le form.
		/// </summary>
		public void Draw()
		{

			// Si la taille a changé, reforme le graphics et les bmp:
			if (_resizeGraphics)
			{
				// Dispose les Graphics:
				if (_bmpHighGraph != null) { _bmpHighGraph.Dispose(); }
				if (_bmpDraftGraph != null) { _bmpDraftGraph.Dispose(); }
				if (_screenGraph != null) { _screenGraph.Dispose(); }
				// Plus tard, on parcours les octets pour convertir les 32 bits en 24 bits. Or la longueur d'une ligne de numérisation (Stride)
				// est une valeur arrondi à 4 octets de la largeur de l'image fois le nombre d'octet pour un pixel. Mais comme on parcours le
				// tableau de façon séquentielle, si Stride ne tombe pas juste (Stride != width*nb_octets), alors on a un décalage, et
				// l'image est délirante. Résultat : il faut que la largeur (pas la peine de faire la hauteur) de l'image soit des multiples de 4:
				_graphWidth = (int)Math.Ceiling(_graphWidth / 4.0) * 4;
				_drawingRect = new Rectangle(0, 0, _graphWidth, _graphHeight);
				// On peut donc calculer la longueur des tableaux (Stride * Height, sachant que désormais Stride = Width * nb_octets):
				_rgbValues32 = new byte[_graphWidth * 4 * _graphHeight];
				_rgbValues24 = new byte[_graphWidth * 3 * _graphHeight];
				// Nouvelles images bmp:
				_bmp32 = new Bitmap(_graphWidth, _graphHeight, PixelFormat.Format32bppArgb);
				_bmp24 = new Bitmap(_graphWidth, _graphHeight, PixelFormat.Format24bppRgb);
				// Nouveaux graphiques form:
				_screenGraph = _controlToDraw.CreateGraphics();
				_screenGraph.Clip = new Region(_drawingRect);
				// Nouveaux graphiques bonne qualité:
				_bmpHighGraph = Graphics.FromImage(_bmp32);
				_bmpHighGraph.Clip = new Region(_drawingRect);
				_bmpHighGraph.PageUnit = GraphicsUnit.Pixel;
				_bmpHighGraph.SmoothingMode = SmoothingMode.HighQuality;
				_bmpHighGraph.PixelOffsetMode = PixelOffsetMode.HighQuality;
				_bmpHighGraph.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				_bmpHighGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
				// Nouveaux graphiques mauvaises qualité:
				_bmpDraftGraph = Graphics.FromImage(_bmp32);
				_bmpDraftGraph.Clip = new Region(_drawingRect);
				_bmpDraftGraph.PageUnit = GraphicsUnit.Pixel;
				_bmpDraftGraph.SmoothingMode = SmoothingMode.HighSpeed;
				_bmpDraftGraph.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				_bmpDraftGraph.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
				_bmpDraftGraph.InterpolationMode = InterpolationMode.Low;
				// Fin de la variable:
				_resizeGraphics = false;
			}
			
			// Mise à zéro des Graphics:
			Graphics g = (__DrawHighQuality ? _bmpHighGraph : _bmpDraftGraph);
			g.ResetTransform(); _screenGraph.ResetTransform();
			g.Clear(Color.White);
			// Translate l'origine:
			g.TranslateTransform(_OriginOnWin.X * (__DrawHighQuality ? 1 : __DraftScale),
				_OriginOnWin.Y * (__DrawHighQuality ? 1 : __DraftScale));
			// Mise à l'échelle pour le brouillon ou l'affichage normal:
			g.ScaleTransform(__Scale, __Scale);
			if (!__DrawHighQuality && __DraftScale != 1) { g.ScaleTransform(__DraftScale, __DraftScale); }
			
			// Lance le dessin:
			if (_gPath == null) { _gPath = new GraphicsPath(); }
			_gPath.Reset();
			this.Draw(ref g, ref _gPath);
			
			// L'affichage des données RVB est beaucoup plus rapide que celles des données RVB, surtout sous Windows XP.
			// On a dessiné sur un bmp 32, donc on le convertit en 24 bits, puis on l'affiche directement, ce qui
			// est vraiment beaucoup plus rapide, surtout sous Win XP:
			_bmp32Data = _bmp32.LockBits(_drawingRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			_bmp24Data = _bmp24.LockBits(_drawingRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
			// Il y a 4 octets pour les 32 bits, 3 pour les 24 bits, donc on copie les trois premiers octets du bmp 32:
      System.Runtime.InteropServices.Marshal.Copy(_bmp32Data.Scan0, _rgbValues32, 0, _rgbValues32.Length);
      int max = _graphWidth * _graphHeight;
      for (int i=0; i<max; i++) {
				_rgbValues24[i*3] = _rgbValues32[i*4];
				_rgbValues24[i*3+1] = _rgbValues32[i*4+1];
				_rgbValues24[i*3+2] = _rgbValues32[i*4+2]; }
      System.Runtime.InteropServices.Marshal.Copy(_rgbValues24, 0, _bmp24Data.Scan0, _rgbValues24.Length);
			_bmp32.UnlockBits(_bmp32Data);
			_bmp24.UnlockBits(_bmp24Data);
			
			// Enfin, on affiche sur le form, en remettant à l'échelle si brouillon:
			if (!__DrawHighQuality && __DraftScale != 1) { _screenGraph.ScaleTransform(1 / __DraftScale, 1 / __DraftScale); }
			_screenGraph.DrawImageUnscaled(_bmp24, 0, 0);
			
			// Message:
			if (__ShowDrawingMessages) { SendInfos("        Drawn."); }
			
		}
		
		
		/// <summary>
		/// Dessine sur le graphique, puis le retourne. Retourne null en cas d'erreur.
		/// </summary>
		protected void Draw(ref Graphics g, ref GraphicsPath path)
		{

			// Si SuspendCalculation est true, on relance un calcul complet avant de dessiner:
			if (__SuspendCalculation) { RecalculateAll(); }		
			// Dessine le rectangle de découpage:
			if (this.ShowClipRect)
			{
				g.DrawRectangle(new Pen(Color.Black, 1), __ClipRect);
			}
			// Dessine le système de coordonnées
			if (__ShowCoordinateSystem)
			{
				g.DrawLine(_XCoordSystemPen, 0F, 0F, _XPointOnWin.X, _XPointOnWin.Y);
				g.DrawLine(_YCoordSystemPen, 0F, 0F, _YPointOnWin.X, _YPointOnWin.Y);
				g.DrawLine(_ZCoordSystemPen, 0F, 0F, _ZPointOnWin.X, _ZPointOnWin.Y);
			}
			// Dessine les axes et les grilles:
			this.DrawAxes(ref g);
			this.DrawGrids(ref g, ref path);
			
			// Dessine les objets dans l'ordre demandé:
			foreach (SpObject o in _spObjects)
			{
			
				// Passe au suivant si l'objet est masqué, ou si l'objet n'est pas défini:
				if (o.Hidden || o.IsUndefined) { continue; }
				
				// Si c'est un point, un milieu, etc.
				if (o is SpPointObject)
				{
					SpPointObject pt = o as SpPointObject;
					if (pt.PointShape == PointShape.Round) { g.FillEllipse(pt.Brush, pt.PtOnWin.X - 2, pt.PtOnWin.Y - 2, 4, 4); }
					if (pt.PointShape == PointShape.Square) { g.FillRectangle(pt.Brush, pt.PtOnWin.X - 2, pt.PtOnWin.Y - 2, 4, 4); }
				}
				// Si c'est une droite, un segment, une demi-droite:
				if (o is SpLineObject)
				{
					SpLineObject line = o as SpLineObject;
					g.DrawLine(line.Pen, line.PtOnWin1, line.PtOnWin2);
				}
				// Si c'est un polygone:
				if (o is SpPolygon)
				{
					SpPolygon polygon = o as SpPolygon;
					if (polygon.Color.A != 0) { g.FillPolygon(polygon.Brush, polygon.PointsOnWin); }
					if (polygon.EdgeColor.A != 0) { g.DrawPolygon(polygon.Pen, polygon.PointsOnWin); }
				}
				// Si c'est un angle:
				if (o is SpAngle)
				{
					SpAngle angle = (SpAngle)o;
					if (angle.PointsOnWin.Length < 2) { continue; }
					if (Maths.Approx(angle.RadValue, Math.PI / 2)  || (angle.RadValue < 0 && Maths.Approx(angle.RadValue, -Math.PI / 2)))
						{ g.DrawLines(angle.Pen, angle.PointsOnWin); }
					else { g.DrawCurve(angle.Pen, angle.PointsOnWin); }
				}
				// Si c'est une sphère:
				if (o is SpSphere)
				{
					SpSphere sphere = o as SpSphere;
					if (sphere.UseBmp)
						{ g.DrawImage(sphere.BmpOnWin, sphere.RectOnWin); }
					else
						{ g.FillEllipse(sphere.Brush, sphere.RectOnWin); }
					if (sphere.EdgeColor.A != 0)
						{ g.DrawEllipse(sphere.Pen, sphere.RectOnWin); }
				}
				// Si c'est un cercle:
				if (o is SpCircle)
				{
					SpCircle circle = o as SpCircle;
					// Si c'est une intersection entre un plan et une sphère, que le rayon est nul et que le centre n'est pas
					// Extracted, on dessine le centre:
					if (circle is SpPlaneSphereIntersection && Maths.Approx(circle.Radius, 0) && !circle.Center.IsExtracted)
						{ g.FillEllipse(circle.Center.Brush, circle.Center.PtOnWin.X - 2, circle.Center.PtOnWin.Y - 2, 4, 4); }
					if (circle.PointsOnWin.Length < 2) { continue; }
					g.DrawCurve(circle.Pen, circle.PointsOnWin);
					// Si remplit et si complet, on trace simplement un FillCurve:
					if (circle.BackColor.A != 0 && circle.IsComplete) { g.FillClosedCurve(circle.Brush, circle.PointsOnWin); }
					// Sinon, un suite de polygones:
					else if (circle.BackColor.A != 0) {
						foreach (PointF[] ptfs in circle.PolygonsOnWin) { g.FillPolygon(circle.Brush, ptfs); } }
				}
				// Si c'est un plan:
				if (o is SpPlaneObject)
				{
					SpPlaneObject plane = o as SpPlaneObject;
					g.DrawLine(plane.Pen, plane.XVector.Point1.PtOnWin, plane.XVector.Point2.PtOnWin);
					g.DrawLine(plane.Pen, plane.YVector.Point1.PtOnWin, plane.YVector.Point2.PtOnWin);
				}
				// Si c'est une function:
				if (o is SpFunctionObject)
				{
					SpFunctionObject func = o as SpFunctionObject;
					path = new GraphicsPath();
					// Si SpFunction2, s'il y a un BackColor et pas d'erreur, on affiche les polygones:
					if (func is SpFunction2 && func.BackColor.A != 0 && !((SpFunction2)func).ErrorOccurred) {
						//foreach (PointF[] ptsf in func.PolygonsOnWin) { if (ptsf.Length > 1) { path.StartFigure(); path.AddPolygon(ptsf); path.CloseFigure(); } }
						//g.FillPath(new SolidBrush(Color.FromArgb(150, Color.Red)), path);
						//path.Reset(); } // Ne fonctionne pas (remplit les polygones de façon aberrante).
						foreach (PointF[] ptsf in func.PolygonsOnWin)
							{ if (ptsf.Length > 1) { g.FillPolygon(func.Brush, ptsf); } } }
						// Pour toutes les fonctions, on affiche au besoin les lignes:
					if (func.Color.A != 0) {
						foreach (PointF[] ptsf in func.LinesOnWin)
							{ if (ptsf.Length > 1) { path.StartFigure(); path.AddCurve(ptsf, func.TensionOnWin); } }
						g.DrawPath(func.Pen, path);
						path.Reset(); }
					// Pour toutes les fonctions, on affiche au besoin les points. Si SpFunction2, on ne le fait que s'il y eu erreur
					// (sinon, les polygones ont déjà été affichés):
					if (func.BackColor.A != 0 && (!(func is SpFunction2) || (func is SpFunction2 && ((SpFunction2)func).ErrorOccurred))) {
						foreach (PointF ptf in func.PointsOnWin)
							{ path.AddEllipse(ptf.X, ptf.Y, 1F, 1F); }
						path.FillMode = FillMode.Winding;
						g.FillPath(func.Brush, path);
						path.Reset(); }
				}
				// Si c'est un solide:
				if (o is SpSolid)
				{
					SpSolid solid = o as SpSolid;
					// Le path ne fonctionne pas si on veut remplir plusieurs polygones...
					if (solid.Color.A != 0) { foreach (PointF[] ptfs in solid.FacesOnWin) { g.FillPolygon(solid.Brush, ptfs); } }
					//if (solid.EdgeColor.A != 0) { foreach (PointF[] ptfs in solid.FacesOnWin) { g.DrawPolygon(solid.Pen, ptfs); } }
					if (solid.EdgeColor.A != 0) {
						path = new GraphicsPath();
						foreach (PointF[] ptfs in solid.FacesOnWin) { path.AddPolygon(ptfs); }
						g.DrawPath(solid.Pen, path);
						path.Reset(); }
				}
				// Si c'est un cone:
				if (o is SpCone)
				{
					SpCone cone = o as SpCone;
					// Le path ne fonctionne pas si on veut remplir plusieurs polygones...
					if (cone.Color.A != 0) { foreach (PointF[] ptfs in cone.FacesOnWin) { g.FillPolygon(cone.Brush, ptfs); } }
					//if (solid.EdgeColor.A != 0) { foreach (PointF[] ptfs in solid.FacesOnWin) { g.DrawPolygon(solid.Pen, ptfs); } }
					if (cone.EdgeColor.A != 0) {
						path = new GraphicsPath();
						foreach (PointF[] ptfs in cone.FacesOnWin) { path.AddPolygon(ptfs); }
						g.DrawPath(cone.Pen, path);
						path.Reset(); }
				}

			}
			
			// Dessine les étiquettes de nom:
			float leftPos, topPos, subTopPos; // Variable de position (left, top et top pour le text en indice)
			Font normalFont, subFont; // Police pour le texte normal et pour le texte en indice)			
			string[] split, underscoreArr = new string[]{"_"}; string text = string.Empty; int l; // Variable utilisées pour le texte en indice
			
			foreach (SpObject o in _spObjects)
			{
			
				// Passe au suivant si l'étiquette ne doit pas être affichée, ou si l'objet n'est pas défini:
				if (o.Hidden || o.IsUndefined || !o.ShowName || (o is SpText && o.Hidden)) { continue; }
				
				// Définit le texte pour les objets:
				if (o is SpText && !o.Hidden) { text = ((SpText)o).Text; }
				else { text = o.Name; }

				// Si pas d'underscore, affiche directement le texte, sinon le découpe, et affiche une partie sur deux
				// en indice:
				if (text.IndexOf("_") == -1)
					{ g.DrawString(text, o.LabelFont, o.LabelBrush, o.LabelOriginOnWin.X + o.LabelCoordsOnWin.X, o.LabelOriginOnWin.Y - o.LabelCoordsOnWin.Y); }
				else
				{
					leftPos = o.LabelOriginOnWin.X + o.LabelCoordsOnWin.X;
					topPos = o.LabelOriginOnWin.Y - o.LabelCoordsOnWin.Y;
					subTopPos = topPos + g.MeasureString(o.Name, o.LabelFont).Height / 2F;
					normalFont = o.LabelFont;
					subFont = new Font(normalFont.FontFamily, normalFont.Size / 2);
					split = text.Split(underscoreArr, StringSplitOptions.RemoveEmptyEntries);
					l = split.Length;
					for (int i=0; i<l; i++)
					{
						if ((double)i % 2.0 == 0) { // Texte normal
							g.DrawString(split[i], normalFont, o.LabelBrush, leftPos, topPos);
							leftPos += g.MeasureString(split[i], normalFont).Width - 2F; }
						else { // Texte en indice
							g.DrawString(split[i], subFont, o.LabelBrush, leftPos, subTopPos);
							leftPos += g.MeasureString(split[i], subFont, 1000).Width - 2F; }
					}
				}
				
			}

		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Affiche les axes jusqu'à une abscisse, sur chaque axe, de MaxAxes.
		/// </summary>
		protected void DrawAxes(ref Graphics g)
		{
		
			// Sort s'il ne faut pas afficher les axes:
			if (__MaxAxes == 0) { return; }
			
			int min = __MaxAxes * -1; int max = __MaxAxes; PointF pt;
			
			// Axe X:
			pt = this.CalculatePointDrawingCoords(min, 0, 0);
			for (int i=min; i<=max; i++)
			{
				g.DrawLine(_XAxisPen, pt, pt = this.CalculatePointDrawingCoords(i, 0, 0));
				if (this.ShowGraduations) { g.DrawString(i.ToString(), __GraduationsFont, _XGraduationsBrush, pt); }
			}

			// Axe Y:
			pt = this.CalculatePointDrawingCoords(0, min, 0);
			for (int i=min; i<=max; i++)
			{
				g.DrawLine(_YAxisPen, pt, pt = this.CalculatePointDrawingCoords(0, i, 0));
				if (this.ShowGraduations) { g.DrawString(i.ToString(), __GraduationsFont, _YGraduationsBrush, pt); }
			}

			// Axe Z:
			pt = this.CalculatePointDrawingCoords(0, 0, min);
			for (int i=min; i<=max; i++)
			{
				g.DrawLine(_ZAxisPen, pt, pt = this.CalculatePointDrawingCoords(0, 0, i));
				if (this.ShowGraduations) { g.DrawString(i.ToString(), __GraduationsFont, _ZGraduationsBrush, pt); }
			}
			
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Dessine les grilles sur les différents plans en fonction de ce qui est demandé, jusqu'à une abscisse, sur chaque axe, coorespond à MaxAxes.
		/// </summary>
		protected void DrawGrids(ref Graphics g, ref GraphicsPath path)
		{
		
			// Sort s'il ne faut pas afficher les axes:
			if (__MaxAxes == 0) { return; }
			
			int min = __MaxAxes * -1; int max = __MaxAxes;
			
			// Plan X-Y:
			if (__ShowXYGrid)
			{
				for (int i=min; i<=max; i++)
					{ path.AddLine(this.CalculatePointDrawingCoords(min, i, 0), this.CalculatePointDrawingCoords(max, i, 0));
					path.CloseFigure(); }
				g.DrawPath(_XGridPen, path); path.Reset();
				for (int i=min; i<=max; i++)
					{ path.AddLine(this.CalculatePointDrawingCoords(i, min, 0), this.CalculatePointDrawingCoords(i, max, 0));
					path.CloseFigure(); }
				g.DrawPath(_YGridPen, path); path.Reset();
			}

			// Plan X-Z
			if (__ShowXZGrid)
			{
				for (int i=min; i<=max; i++)
					{ path.AddLine(this.CalculatePointDrawingCoords(min, 0, i), this.CalculatePointDrawingCoords(max, 0, i));
					path.CloseFigure(); }
				g.DrawPath(_XGridPen, path); path.Reset();
				for (int i=min; i<=max; i++)
					{ path.AddLine(this.CalculatePointDrawingCoords(i, 0, min), this.CalculatePointDrawingCoords(i, 0, max));
					path.CloseFigure(); }
				g.DrawPath(_ZGridPen, path); path.Reset();
			}

			// Plan Y-Z
			if (__ShowYZGrid)
			{
				for (int i=min; i<=max; i++)
					{ path.AddLine(this.CalculatePointDrawingCoords(0, min, i), this.CalculatePointDrawingCoords(0, max, i));
					path.CloseFigure(); }
				g.DrawPath(_YGridPen, path); path.Reset();
				for (int i=min; i<=max; i++)
					{ path.AddLine(this.CalculatePointDrawingCoords(0, i, min), this.CalculatePointDrawingCoords(0, i, max));
					path.CloseFigure(); }
				g.DrawPath(_ZGridPen, path); path.Reset();
			}
			
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Ne redessine le graphique que si AutoDraw == true et si l'assembly passé en argument n'est pas cet assembly-ci (ce qui évique les appels récurrents).
		/// </summary>
		protected void DrawAuto(Assembly assembly)
		{
			if ((__AutoDraw) && (!_assemblyName.Equals(assembly.FullName))) { this.Draw(); }
		}




		#endregion DRAWING







		// ---------------------------------------------------------------------------
		// CALCULATION
		// ---------------------------------------------------------------------------




		#region CALCULATION



		/// <summary>
		/// Calcule l'affichage du repère dans le form, en 2D
		/// </summary>
		protected void CalculateCoordSystemDrawingData()
		{
		
			// Sort s'il ne faut pas calculer:
			if (__SuspendCalculation) { return; }
		
			// Si le repère d'origine n'existe pas encore, le place au milieu du form:
			if (_OriginOnWin.IsEmpty) { _OriginOnWin = new Point(_graphWidth /2, _graphHeight / 2); }
			
			// Calcul les valeurs min et max de x et y dans le repère du form:
			_XWin_min = _OriginOnWin.X * -1;
			_YWin_min = _OriginOnWin.Y * -1;
			_XWin_max = _graphWidth - _OriginOnWin.X;
			_YWin_max = _graphHeight - _OriginOnWin.Y;
			
			// Calcule les coordonnées 2D des vecteurs du repère:
			// Dans le form, l'axe des "y" va vers le bas... Or il est plus courant qu'il aille vers le haut...
			// Donc on convertit en multipliant par -1:
			int invert = -1;
					_XPointOnWin = new Point(); _YPointOnWin = new Point(); _ZPointOnWin = new Point();
			_XPointOnWin.X = (int)(_OXYZ.OPoint.X + __Zoom * __XNorm * Math.Sin(__Phi));
			_XPointOnWin.Y = (int)(_OXYZ.OPoint.Y - __Zoom * __XNorm * Math.Cos(__Phi) * Math.Sin(__Theta)) * invert;
			_YPointOnWin.X = (int)(_OXYZ.OPoint.X + __Zoom * __YNorm * Math.Cos(__Phi));
			_YPointOnWin.Y = (int)(_OXYZ.OPoint.Y + __Zoom * __YNorm * Math.Sin(__Phi) * Math.Sin(__Theta)) * invert;
			_ZPointOnWin.X = (int)(_OXYZ.OPoint.X);
			_ZPointOnWin.Y = (int)(_OXYZ.OPoint.Y + __Zoom * __ZNorm * Math.Cos(__Theta)) * invert;
			
			// Rotation, en utilisant les coordonnées polaires dans le plan du form:
			if (__Rotation != 0)
			{
				double polarR, polarθ; Coord2D coords;
				polarθ = My.MathsGeo.ToPolar((double)_XPointOnWin.X, (double)_XPointOnWin.Y, out polarR) + __Rotation;
				coords = My.MathsGeo.FromPolar(polarR, polarθ);
				_XPointOnWin.X = (int)coords.X; _XPointOnWin.Y = (int)coords.Y;
							polarθ = My.MathsGeo.ToPolar((double)_YPointOnWin.X, (double)_YPointOnWin.Y, out polarR) + __Rotation;
							coords = My.MathsGeo.FromPolar(polarR, polarθ);
							_YPointOnWin.X = (int)coords.X; _YPointOnWin.Y = (int)coords.Y;
				polarθ = My.MathsGeo.ToPolar((double)_ZPointOnWin.X, (double)_ZPointOnWin.Y, out polarR) + __Rotation;
				coords = My.MathsGeo.FromPolar(polarR, polarθ);
				_ZPointOnWin.X = (int)coords.X; _ZPointOnWin.Y = (int)coords.Y;
			}

			// Calcule le nouveau centre du form:
			if (__Scale != 0)
			{
				_XCenterForm = (int)(((_graphWidth / 2) - _OriginOnWin.X) / __Scale);
				_YCenterForm = (int)(((_graphHeight / 2) - _OriginOnWin.Y) / __Scale);
			}
			
			if (__ShowDrawingMessages) { SendInfos("        2D Coordinate system calculated."); }
		
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Calcule les coordonnées d'un point de l'espace dans le repère du form, en additionnant les vecteurs du repère de l'espace reporté sur le plan du form.
		/// </summary>
		protected PointF CalculatePointDrawingCoords(float x, float y, float z)
		{
			// Calcule les coordonnées 2D en additionnant les vecteurs:
			return new PointF(x * _XPointOnWin.X + y * _YPointOnWin.X + z * _ZPointOnWin.X,
				x * _XPointOnWin.Y + y * _YPointOnWin.Y + z * _ZPointOnWin.Y);
		}

		/// <summary>
		/// Calcule les coordonnées d'un point de l'espace dans le repère du form, en additionnant les vecteurs du repère de l'espace reporté sur le plan du form.
		/// </summary>
		protected PointF CalculatePointDrawingCoords(SpPointObject spt)
		{
			return this.CalculatePointDrawingCoords((float)spt.X, (float)spt.Y, (float)spt.Z);
		}

		/// <summary>
		/// Calcule les coordonnées d'un point de l'espace dans le repère du form, en additionnant les vecteurs du repère de l'espace reporté sur le plan du form.
		/// </summary>
		protected PointF CalculatePointDrawingCoords(Coord3D coords)
		{
			return this.CalculatePointDrawingCoords((float)coords.X, (float)coords.Y, (float)coords.Z);
		}

		// ---------------------------------------------------------------------------


		/// <summary>
		/// Calcule les coordonnées et valeurs numériques nécessaires à l'affichage 2D dans le repère du form, pour un objet particulier. updMode définit ce qui doit être recalculer. Si Calc3D, alors on considère que l'orientation du repère de l'espace à changer, et donc tout doit être recalculer. Si Calc2D, alors on considère qu'il y a simplement eu une translation de l'origine du repère du form, et donc seuls quelques éléments sont recalculer, comme les "extrémités" des droites (cad les points extrême à la limite de la zone d'affichage du form).
		/// </summary>
		protected void CalculateObjectDrawingData(SpObject o, CalcUpdMode updMode, bool showInfos)
		{
			this.CalculateObjectDrawingData(o, updMode, showInfos, o.Name);
		}
		
		/// <summary>
		/// Voir surcharge. fullName désigne le nom complet à placer avant le nom de l'objet, si celui-ci est un objet temporaire $.
		/// </summary>
		private void CalculateObjectDrawingData(SpObject o, CalcUpdMode updMode, bool showInfos, string fullName)
		{

			// Sort s'il ne faut pas calculer, ou si l'objet n'est pas défini:
			if (__SuspendCalculation || o.IsUndefined) { return; }
			
			// Calcul les objets OwnedObjects de l'objet:
			foreach (SpObject tempObj in o.OwnedObjects)
				{ this.CalculateObjectDrawingData(tempObj, updMode, showInfos, String.Format("{0}.{1}", fullName, tempObj.Name)); }

			// ---------- POINT D'ORIGINE DU LABEL:
			if (!o.LabelOrigin.Empty && o.ShowName && !(o is SpPointObject))
			{
				o.LabelOriginOnWin = CalculatePointDrawingCoords(o.LabelOrigin);
			}

			// ---------- POINT:
			if (o is SpPointObject && updMode == CalcUpdMode.Calc3D)
			{
				// Calcule les coordonnées 2D en additionnant les vecteurs. Et point d'origine du label:
				SpPointObject pt = (SpPointObject)o;
				pt.PtOnWin = pt.LabelOriginOnWin = this.CalculatePointDrawingCoords(pt);
			}
			
			// ---------- LINE or RAY:
			if (o is SpLine && !(o is SpSegment))
			{
				SpLine line =(SpLine)o;
				float x1, x2, y1, y2, ptOnWin1X, ptOnWin1Y, ptOnWin2X, ptOnWin2Y;
				// Ne fait rien si les points sont au même endroit:
				if (line.Point1.PtOnWin.Equals(line.Point2.PtOnWin)) { return; }
				// Récupère les coordonnées 2D dans le plan du form des points de référence:
				ptOnWin1X = line.Point1.PtOnWin.X;
				ptOnWin1Y = line.Point1.PtOnWin.Y;
				ptOnWin2X = line.Point2.PtOnWin.X;
				ptOnWin2Y = line.Point2.PtOnWin.Y;
				// Si les points ont même abscisse, calcule les points extrêmes en fonction de _Y_min et max:
				if (ptOnWin1X == ptOnWin2X)
				{
					x1 = x2 = ptOnWin1X;
					y1 = _YWin_min;
					y2= _YWin_max;
				}
				// Sinon, utilise _X_min et max pour limiter la droite, en calculant son équation:
				else
				{
					x1 = _XWin_min;
					y1 = ptOnWin1Y * (_XWin_min - ptOnWin2X) / (ptOnWin1X - ptOnWin2X) + ptOnWin2Y * (_XWin_min - ptOnWin1X) / (ptOnWin2X - ptOnWin1X);
					x2 = _XWin_max;
					y2 = ptOnWin1Y * (_XWin_max - ptOnWin2X) / (ptOnWin1X - ptOnWin2X) + ptOnWin2Y * (_XWin_max - ptOnWin1X) / (ptOnWin2X - ptOnWin1X);
				}
				// Enregistre les points:
				if (o is SpRay)
				{
					if (ptOnWin1X == ptOnWin2X)
					{
						if (ptOnWin1Y > ptOnWin2Y)
							{ line.PtOnWin1 = new PointF(x1, y1); line.PtOnWin2 = new PointF(ptOnWin1X, ptOnWin1Y); }
						else 
							{ line.PtOnWin1 = new PointF(x2, y2); line.PtOnWin2 = new PointF(ptOnWin1X, ptOnWin1Y); }
					}
					else
					{
						if (ptOnWin1X > ptOnWin2X)
							{ line.PtOnWin1 = new PointF(x1, y1); line.PtOnWin2 = new PointF(ptOnWin1X, ptOnWin1Y); }
						else 
							{ line.PtOnWin1 = new PointF(x2, y2); line.PtOnWin2 = new PointF(ptOnWin1X, ptOnWin1Y); }
					}
				}
				else
				{
					line.PtOnWin1 = new PointF(x1, y1);
					line.PtOnWin2 = new PointF(x2, y2);
				}
			}
			
			// ---------- SEGMENT or VECTOR:
			if ((o is SpSegment || o is SpVectorObject) && updMode == CalcUpdMode.Calc3D)
			{
				SpLineObject line = (SpLineObject)o;
				line.PtOnWin1 = line.Point1.PtOnWin;
				line.PtOnWin2 = line.Point2.PtOnWin;
			}
			
			// ---------- TEXT:
			if (o is SpText && updMode == CalcUpdMode.Calc3D)
			{
				SpText text = o as SpText;
				text.LabelOriginOnWin = new PointF((text.Absolute ? _XCenterForm : 0), (text.Absolute ? _YCenterForm : 0));
			}

			// ---------- POLYGON:
			if (o is SpPolygon && updMode == CalcUpdMode.Calc3D)
			{
				SpPolygon polygon = (SpPolygon)o;
				int l = polygon.Vertices.Length;
				polygon.PointsOnWin = new PointF[l];
				for (int i=0; i<l; i++)
					{ polygon.PointsOnWin[i] = polygon.Vertices[i].PtOnWin; }
			}
			
			// ---------- ANGLE :
			if (o is SpAngle && updMode == CalcUpdMode.Calc3D)
			{
				SpAngle angle = (SpAngle)o;
				int l = angle.PointsOnWinCoords.Length;
				angle.PointsOnWin = new PointF[l];
				for (int i=0; i<l; i++)
					{ angle.PointsOnWin[i] = CalculatePointDrawingCoords(angle.PointsOnWinCoords[i]); }
			}

			// ---------- SPHERE:
			if (o is SpSphere && updMode == CalcUpdMode.Calc3D)
			{
				SpSphere sphere = (SpSphere)o;
				// Calcul le rayon sur le repère du form:
				float radius = __Zoom * (float)sphere.Radius;
				sphere.RectOnWin = new RectangleF(sphere.Center.PtOnWin.X - radius,
					sphere.Center.PtOnWin.Y - radius, radius * 2F, radius * 2F);
			}
			
			// ---------- CERCLE:
			if (o is SpCircle && updMode == CalcUpdMode.Calc3D)
			{
				SpCircle circle = (SpCircle)o;
				int l = circle.PointsOnWinCoords.Length;
				PointF[] ptfs = new PointF[l];
				for (int i=0; i<l; i++)
					{ ptfs[i] = CalculatePointDrawingCoords(circle.PointsOnWinCoords[i]); }
				circle.PointsOnWin = ptfs;
				// Si on doit le remplir, on le fait à l'aide de polygones, sauf s'il est complet (auquel cas on trace un CloseCurve):
				if (circle.BackColor.A != 0 && !circle.IsComplete) {
					PointF center = CalculatePointDrawingCoords(circle.Center);
					l = circle.PointsOnWin.Length - 1;
					PointF[][] ptffs = new PointF[l][];
					for (int i=0; i<l; i++) { ptffs[i] = new PointF[]{center, ptfs[i], ptfs[i+1]}; }
					circle.PolygonsOnWin = ptffs;
				}
			}
			
			// ---------- FONCTION:
			if (o is SpFunctionObject && updMode == CalcUpdMode.Calc3D)
			{
				SpFunctionObject func = (SpFunctionObject)o;
				Coord3D[] arr = func.PointsOnWinCoords;
				int l = arr.Length;
				PointF[] pts = new PointF[l];
				for (int i=0; i<l; i++) { pts[i] = CalculatePointDrawingCoords(arr[i]); }
				func.PointsOnWin = pts;
				CalculateFunctionDrawingData(func);
			}
			
			// ---------- SOLIDE:
			if (o is SpSolid && updMode == CalcUpdMode.Calc3D)
			{
				SpSolid solid = (SpSolid)o;
				int lenFaces = solid.Faces.Length, l;
				solid.FacesOnWin = new PointF[lenFaces][];
				for (int i=0; i<lenFaces; i++) {
					l = solid.Faces[i].Length;
					solid.FacesOnWin[i] = new PointF[l];
					for (int j=0; j<l; j++) { solid.FacesOnWin[i][j] = solid.Faces[i][j].PtOnWin; } }
			}
			
			// ---------- CONE:
			if (o is SpCone && updMode == CalcUpdMode.Calc3D)
			{
				SpCone cone = (SpCone)o;
				PointF vertex = cone.Vertex.PtOnWin;
				PointF[] circlePtfs = cone.BaseCircle.PointsOnWin;
				int l = circlePtfs.Length;
				PointF[][] polys = new PointF[l-1 + (cone.BaseCircle.IsComplete ? 1 : 0)][];
				for (int i=0; i<l-1; i++) { polys[i] = new PointF[]{vertex, circlePtfs[i], circlePtfs[i+1]}; }
				if (cone.BaseCircle.IsComplete) { polys[l-1] = new PointF[]{vertex, circlePtfs[l-1], circlePtfs[0]}; }
				cone.FacesOnWin = polys;
			}
			
			// Affiche message:
			if (showInfos && __ShowDrawingMessages) { SendInfos(String.Format("        New calculation executed for {0} (type {1}).", fullName, updMode.ToString())); }
		
		}
		
		/// <summary>
		/// Calcul les données numériques 2D d'un objet, en affichant par défaut les informations.
		/// </summary>
		protected void CalculateObjectDrawingData(SpObject o, CalcUpdMode updMode)
			{ CalculateObjectDrawingData(o, updMode, true); }
		
		/// <summary>
		/// Procédure pour le public. Calcule les données numériques 2D d'un objet, en affichant par défaut les informations.
		/// </summary>
		public void CalculateObjectDrawingData(SpObject o)
			{ this.CalculateObjectDrawingData(o, CalcUpdMode.Calc3D, true); }
		
		/// <summary>
		/// Exécute CalculateObjectDrawingData pour tous les objets. Utilise l'ordre de construction (et non d'affichage), afin de pouvoir réutiliser les calculs déjà exécutés pour les sous objets.
		/// </summary>
		protected void CalculateAllObjectsDrawingData(CalcUpdMode updMode)
		{
			if (__SuspendCalculation) { return; }
			SpObject[] constructOrder = _spObjects.ConstructList;
			foreach (SpObject o in constructOrder) { this.CalculateObjectDrawingData(o, updMode, false); }
			if (__ShowDrawingMessages) { SendInfos(String.Format("        New calculation executed for all objets (type {0}).", updMode.ToString())); }
		}


		// ---------------------------------------------------------------------------


		/// <summary>
		/// Gestion des points, des lignes et des polygones pour les fonctions.
		/// </summary>
		protected void CalculateFunctionDrawingData(SpFunctionObject func)
		{
			// Procédures particulières pour fonctions particulières:
			if (func is SpFunction1OnPlane) { CalculateFunctionDrawingData(func as SpFunction1OnPlane); }
			else if (func is SpFunction2) { CalculateFunctionDrawingData(func as SpFunction2); }
			// S'il faut afficher les lignes, on copie simplement le tableau de points:
			else if (func.Color.A != 0) { func.LinesOnWin = new PointF[1][]; func.LinesOnWin[0] = func.PointsOnWin; }
		}


		/// <summary>
		/// Gestion des points et des lignes pour les fonctions 1 sur plan.
		/// </summary>
		protected void CalculateFunctionDrawingData(SpFunction1OnPlane func)
		{
		
			// Sort s'il n'y a pas de points:
			if (func.PointsOnWin.Length == 0) { return; }
			
			// Le tableau PointsOnWinCoords peut contenir des structures Empty... mais les structures PointF, elles sont considérées
			// comme Empty si, et seulement si, les deux coordonnées sont nulles, ce qui ne veut rien dire (puisqu'un point (0,0) peut
			// très bien exister...
			
			// Variables:
			PointF[] arr = func.PointsOnWin; Coord3D[] coords = func.PointsOnWinCoords;
			
			// S'il faut afficher les points:
			if (func.BackColor.A != 0)
			{
				// S'il n'y a pas de structures Empty, on n'a rien à faire. Sinon, on élimine les PointF correspondant à un
				// Coord3D Empty:
				if (func.ErrorOccurred) { func.PointsOnWin = arr.Where(delegate(PointF pt, int i) { return !coords[i].Empty; }).ToArray(); }
			}
			
			// S'il faut afficher les lignes (relier les points):
			if (func.Color.A != 0)
			{
				// S'il n'y a pas de structures Empty, on copie simplement le tableau:
				if (!func.ErrorOccurred)
				{
					func.LinesOnWin = new PointF[1][];
					func.LinesOnWin[0] = func.PointsOnWin;
				}
				else
				{
					// S'il y a eu des erreurs au milieu de la fonction, c'est qu'il faut découper le tableau pour avoir plusieurs
					// tracé (sinon, par exemple, avec 1/x, +∞ rejoint -∞...):
					int l = arr.Length;
					int c = 0, d = 0;
					PointF[][] lines = new PointF[1][];
					lines[0] = new PointF[100];
					for (int i=0; i<l; i++)
					{
						if (coords[i].Empty && i == l-1) { break; } // Elimine l'erreur à la fin, s'il y en a une
						if (coords[i].Empty) {
							Array.Resize(ref lines[c], d);
							Array.Resize(ref lines, ++c + 1); d = 0;
							lines[c] = new PointF[100]; }
						else {
							if (d >= lines[c].Length) { Array.Resize(ref lines[c], d + 100); }
							lines[c][d++] = arr[i]; }
					}
					Array.Resize(ref lines[c], d);
					func.LinesOnWin = lines;
				}
			}

		}
		
		/// <summary>
		/// Gestion des points, lignes et polygones pour les fonctions 2 de l'espace.
		/// </summary>
		protected void CalculateFunctionDrawingData(SpFunction2 func)
		{
		
			// Sort s'il n'y a pas de points, ou s'il ne faut pas dessiner les lignes:
			if (func.PointsOnWin.Length == 0) {
				func.LinesOnWin = new PointF[0][];
				func.PolygonsOnWin = new PointF[0][];
				return; }
			// S'il y a eu des erreurs, on ne dessine que les points et les ligns brutes (puisqu'on ne peut pas calculer les lignes ou les
			// polygones):
			if (func.ErrorOccurred) {
				func.LinesOnWin = new PointF[1][];
				func.LinesOnWin[0] = func.PointsOnWin;
				func.PolygonsOnWin = new PointF[0][];
				return; }
			
			// Variables:
			int xlines = func.XLines, ylines = func.YLines, c = 0;
			PointF[] arr = func.PointsOnWin;
			
			// Note: le tableau arr contient les points calculés de la façon suivante:
			// 0 3 6 9     avec en ligne les X et en colonnes les Y.
			// 1 4 7 10
			// 2 5 8 11
			
			// S'il faut tracer le quadrillage, on établit les points pour les lignes en x et en y:
			if (func.Color.A != 0)
			{
				PointF[][] lines = new PointF[xlines+ylines][]; c = 0;
				// Copie les lignes Y:
				for (int i=0; i<xlines; i++)
				{
					lines[c] = new PointF[ylines];
					Array.Copy(arr, i*ylines, lines[c], 0, ylines);
					c++;
				}
				// Copie les lignes X:
				for (int j=0; j<ylines; j++)
				{
					lines[c] = new PointF[xlines];
					for (int i=0; i<xlines; i++) { lines[c][i] = arr[i*ylines+j]; }
					c++;
				}
				// Enregistre:
				func.LinesOnWin = lines;
			}
			
			// S'il faut afficher les polygones:
			if (func.BackColor.A != 0)
			{
				// Répartit les résultats précédents pour faire des quadrilatères...
				PointF[][] polys = new PointF[(xlines-1)*(ylines-1)][]; c = 0;
				for (int i=0; i<xlines-1; i++)
				{
					for (int j=0; j<ylines-1; j++)
					{
						polys[c] = new PointF[4];
						polys[c][0] = arr[i*ylines+j];
						polys[c][1] = arr[(i+1)*ylines+j];
						polys[c][2] = arr[(i+1)*ylines+j+1];;
						polys[c][3] = arr[i*ylines+j+1];;
						c++;
					}
				}
				// Enregistre le résultat:
				func.PolygonsOnWin = polys;
			}

		}


		#endregion CALCULATION





	}
	
	
	
	
}