using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace My
{




	// ---------------------------------------------------------------------------
	// TRANSFORMED OBJECTS
	// ---------------------------------------------------------------------------




	#region TRANSFORMED OBJECTS


	/// <summary>
	/// Point transformé.
	/// </summary>
	public class SpTrPoint : SpPointObject, ITransformedObject
	{
	
		protected SpTransformationObject[] _transformations;
		protected SpPointObject _base;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Transformed point"; } }
		
		/// <summary>
		/// Obtient les transformations.
		/// </summary>
		public SpTransformationObject[] Transformations { get { return _transformations; } }
	
		/// <summary>
		/// Obtient l'objet de base.
		/// </summary>
		public SpObject BaseObject { get { return _base; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpTrPoint(string name, SpPointObject baseObj, params SpTransformationObject[] transformations) : base(name)
		{
			Alter(baseObj, transformations);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointObject baseObj, params SpTransformationObject[] transformations)
		{
			_transformations = transformations; _base = baseObj;
			EndAlterProcess(_base, _transformations);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			Coord3D[] coords = new Coord3D[]{_base.Coordinates};
			if (!SpTransformationObject.TransformPoints(_transformations, ref coords))
				{ SendCalculationResult(true, "Enable to apply a transformation"); return; }
			_coords = coords[0];
			AlterCoords(_coords);
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} img of {1} {2}", BaseToString(), _base, SpTransformationObject.GetUsingTransf(_transformations));
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_base, _transformations}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			return base.GetInfos(SpTransformationObject.GetTrGetInfos(_transformations), lines);
		}
	
	}


	// ---------------------------------------------------------------------------


	/// <summary>
	/// Point transformé.
	/// </summary>
	public class SpTrPointOnPlane : SpPointOnPlaneObject, ITransformedObject
	{
	
		protected SpTransformationObject[] _transformations;
		protected SpPointOnPlaneObject _base;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Transformed point on plane"; } }
		
		/// <summary>
		/// Obtient les transformations.
		/// </summary>
		public SpTransformationObject[] Transformations { get { return _transformations; } }
	
		/// <summary>
		/// Obtient l'objet de base.
		/// </summary>
		public SpObject BaseObject { get { return _base; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpTrPointOnPlane(string name, SpPointOnPlaneObject baseObj,
			params SpTransformationObject[] transformations) : base(name)
		{
			Alter(baseObj, transformations);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpPointOnPlaneObject baseObj, params SpTransformationObject[] transformations)
		{
			_transformations = transformations; _base = baseObj; _plane = baseObj.Plane;
			EndAlterProcess(_base, _transformations);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Récupère les objest:
			_plane = _base.Plane;
			Coord2D[] coords = new Coord2D[]{_base.CoordinatesOnPlane};
			if (!SpTransformationObject.TransformPoints(_transformations, ref coords, _plane))
				{ SendCalculationResult(true, "Enable to apply a transformation"); return; }
			AlterCoords(coords[0]);
			CalculateCoordsStruct();
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return MakeToString("{0} img of {1} {2}", BaseToString(), _base, SpTransformationObject.GetUsingTransf(_transformations));
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_base, _transformations}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			return base.GetInfos(SpTransformationObject.GetTrGetInfos(_transformations), lines);
		}
	
	}	


	// ---------------------------------------------------------------------------
	

	/// <summary>
	/// Solide transformé.
	/// </summary>
	public class SpTrSolid : SpSolid, ITransformedObject
	{
	
		protected SpTransformationObject[] _transformations;
		protected SpSolid _base;
		
		/// <summary>
		/// Obtient une chaîne décrivant le type d'objet.
		/// </summary>
		public override string TypeDescription { get { return "Transformed solid"; } }
		
		/// <summary>
		/// Obtient les transformations.
		/// </summary>
		public SpTransformationObject[] Transformations { get { return _transformations; } }
	
		/// <summary>
		/// Obtient l'objet de base.
		/// </summary>
		public SpObject BaseObject { get { return _base; } }
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public SpTrSolid(string name, SpSolid baseObj, params SpTransformationObject[] transformations) : base(name)
		{
			// Prépare le tableau, ce qui signifie qu'on ne peut changer le nombre de points par la méthode alter:
			int l =  baseObj.Vertices.Length;
			_vertices = new SpPointObject[l];
			for (int i=0; i<l; i++) { _vertices[i] = new SpPointObject("%pt" + i.ToString(), 0, 0, 0); }
			// Centre:
			_center = new SpPointObject("%center", 0, 0, 0);
			// Les faces sont mises à jours dans Alter.
			Alter(baseObj, transformations);
		}
		
		/// <summary>
		/// Reconstruit l'objet.
		/// </summary>
		public virtual void Alter(SpSolid baseObj, params SpTransformationObject[] transformations)
		{
			_transformations = transformations; _base = baseObj;
			// Undefinded si le nombre de points a changé dans le solide de base:
			if (baseObj.Vertices.Length != _vertices.Length)
				{ SendCalculationResult(true, "You can't change the number of vertices of base object."); return; }
			// Met à jour les faces:
			SpPointObject[] baseVert = baseObj.Vertices; int l = baseObj.Faces.Length, index;
			_faces = new SpPointObject[l][]; int k;
			for (int i=0; i<l; i++)
			{	
				k = baseObj.Faces[i].Length;
				_faces[i] = new SpPointObject[k];
				for (int j=0; j<k; j++)
				{
					index = Array.IndexOf(baseVert, baseObj.Faces[i][j]);
					// Sort si l'objet n'a pas été trouvé:
					if (index < 0) {
						SendCalculationResult(true, String.Format("{0} is not in vertices list.", baseObj.Faces[i][j])); return; }
					_faces[i][j] = _vertices[index];
				}
			}
			EndAlterProcess(_base, _transformations, null, _vertices);
		}
		
		/// <summary>
		/// Recalcule ses propres données numériques lorsque celles d'un objet maître ont changées.
		/// </summary>
		protected override void CalculateNumericData()
		{
			// Prépare le tableau des coordonnées en vu des transformations:
			int l = _base.Vertices.Length;
			Coord3D[] coords = new Coord3D[l];
			coords = Array.ConvertAll<SpPointObject,Coord3D>(_base.Vertices, delegate(SpPointObject o) { return o.Coordinates; });
			// Rajoute le centre:
			coords = coords.Concat(new Coord3D[]{_center.Coordinates}).ToArray();
			// Transforme puis recalcule les points:
			if (!SpTransformationObject.TransformPoints(_transformations, ref coords))
				{ SendCalculationResult(true, "Enable to apply a transformation"); return; }
			for (int i=0; i<l; i++) { _vertices[i].AlterCoords(coords[i]); _vertices[i].Recalculate(true); }
			_center.AlterCoords(coords[l]); _center.Recalculate(true);
			SendCalculationResult();
		}
		
		/// <summary>
		/// Retourne la description de l'objet.
		/// </summary>
		public override string ToString()
		{
			return this.MakeToString("{0} img of {1} {2}", BaseToString(), _base, SpTransformationObject.GetUsingTransf(_transformations));
		}
		
		/// <summary>
		/// Retourne les objets utilisés pour la constructeur de l'objet, sans le nom (ce qui correspond donc aux paramètres d'une méthode Alter).
		/// </summary>
		public override object[] GetCtorObjects()
			{ return new object[]{_base, _transformations}; }
		
		/// <summary>
		/// Retourne une description détaillée de l'objet.
		/// </summary>
		public override string GetInfos(params object[] lines)
		{
			return base.GetInfos(SpTransformationObject.GetTrGetInfos(_transformations), lines);
		}
	
	}
	
	
	
	#endregion TRANSFORMED OBJECTS
	



}
