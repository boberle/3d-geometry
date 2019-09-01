using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace My
{



	// ---------------------------------------------------------------------------
	// ENUMERATIONS:
	
	
	/// <summary>
	/// Type de vue.
	/// </summary>
	public enum ViewType
	{
		Top,
		Bottom,
		Front,
		Back,
		Left,
		Right,
		Def
	}
	
	
	/// <summary>
	/// Enumération des trois axes de l'espace.
	/// </summary>
	public enum Axis
	{
		XAxis, YAxis, ZAxis
	}
	
	
	/// <summary>
	/// Type d'exportation du dessin.
	/// </summary>
	public enum DrawingType
	{
		Screen, File, Clipboard
	}

		
	// ---------------------------------------------------------------------------
	// EVENEMENTS:
		
		
	/// <summary>
	/// Classe de paramètres d'événement pour l'affichage des informations et des erreurs. Action fournit la description de l'information et IsError indique s'il s'agit d'une erreur (toujours false si Infos est déclenché, toujours true si Error est déclenché).
	/// </summary>
	public class InfosEventArgs : EventArgs
	{
		public string Action { get; set; }
		public bool IsError { get; set; }
		public InfosEventArgs(string action, bool isError) { Action = action; IsError = isError; }
	}

	/// <summary>
	/// Classe de paramètres d'événement pour l'événement RequestDrawingCalc. ObjectChanged fournit l'objet qui a été modifié.
	/// </summary>
	internal class RequestDrawingCalcEventArgs : EventArgs
	{
		public SpObject ObjectChanged { get; set; }
		public RequestDrawingCalcEventArgs(SpObject objChanged)
			{ ObjectChanged = objChanged; }
	}
	
	/// <summary>
	/// Délégué d'événement. Cet événement est destiné à l'extérieur: il indique que les données de l'objet ont été changé, et demande ainsi au DrawingArea de recalculé les données 2D pour l'affichage.
	/// </summary>
	internal delegate void RequestDrawingCalcEventHandler(object sender, RequestDrawingCalcEventArgs e);


	// ---------------------------------------------------------------------------
	// EXCEPTIONS:


	/// <summary>
	/// Classe d'exception.
	/// </summary>
	public class SpObjectNotFoundException : Exception
	{
		public SpObjectNotFoundException(string name) : base(String.Format("Object {0} doesn't exists or is not correct.", name))
			{ ; }
	}
	
	
	// ---------------------------------------------------------------------------
	// STRUCTURES:


	/// <summary>
	/// Point pondéré.
	/// </summary>
	public struct WeightedPoint
	{
		public DoubleF Weight;
		public SpPointObject Point;
		public WeightedPoint(SpPointObject spt, DoubleF weight) : this()
			{ Point = spt; Weight = weight; }
		public override string ToString()
			{ return String.Format("({0},{1})", Point.Name, Weight); }
	}

	// ---------------------------------------------------------------------------
	
	public struct LineValues<T>
	{
		public T x_0;
		public T y_0;
		public T z_0;
		public T α;
		public T β;
		public T γ;
		public LineValues(T x_0, T y_0, T z_0, T α, T β, T γ)
			{ this.x_0 = x_0; this.y_0 = y_0; this.z_0 = z_0;
			this.α = α; this.β = β; this.γ = γ; }
	}

	// ---------------------------------------------------------------------------
	
	/// <summary>
	/// Information sur l'exportation du dessin. Le constructeur sans argument contient par défaut les données pour un affichage à l'écran.
	/// </summary>
	public struct DrawingInfos
	{
		public DrawingType Type;
		public string Filename;
		public float Resolution;
		public float Scale;
		public ImageFormat Format;
		/// <summary>
		/// Exportation vers un fichier au format png.
		/// </summary>
		public DrawingInfos(string filename, float res, float scale) : this()
			{ Type = DrawingType.File; Filename = filename; Resolution = res; Scale = scale; Format = ImageFormat.Png; }
		/// <summary>
		/// Exportation vers le presse papier.
		/// </summary>
		public DrawingInfos(float res, float scale) : this()
			{ Type = DrawingType.Clipboard; Resolution = res; Scale = scale; Format = ImageFormat.Png; }
	}
	
	
	// ---------------------------------------------------------------------------
	// ATTRIBUTS:

	
	/// <summary>
	/// Indique si une fonction pour les formules créer un objet.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class CreateObjectFormulaFunctionAttribute : Attribute
	{
		public CreateObjectFormulaFunctionAttribute() {}
	}

	/// <summary>
	/// Indique si une fonction pour les formules obtient un objet.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class GetObjectFormulaFunctionAttribute : Attribute
	{
		public GetObjectFormulaFunctionAttribute() {}
	}



}
