using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace My
{


	/// <summary>
	/// Fournit des informations sur les constructeurs et méthodes Alter des SpObjects.
	/// </summary>
	public struct SpObjectCtorInfos
	{
	
		/// <summary>
		/// Type du SpObject.
		/// </summary>
		public Type Type { get; private set; }
		
		/// <summary>
		/// Nom complet et unique.
		/// </summary>
		public string Name { get; private set; }
		
		/// <summary>
		/// Nom abrégé et unique.
		/// </summary>
		public string ShortName { get; private set; }
		
		/// <summary>
		/// Nom pour surcharge, qui n'est pas unique.
		/// </summary>
		public string OverloadName { get; private set; }
		
		/// <summary>
		/// Obtient si le type est un type de base, que l'utilisateur n'est pas censé créé lui-même.
		/// </summary>
		public bool IsBaseObject { get; private set; }
		
		/// <summary>
		/// Obtient si le type est un type abstrait.
		/// </summary>
		public bool IsAbstract { get; private set; }
		
		/// <summary>
		/// Obtient le groupe de l'objet.
		/// </summary>
		public string Group { get; private set; }
		
		/// <summary>
		/// Method Alter de l'objet ou constructeur.
		/// </summary>
		public MethodBase Method { get; private set; }
		
		/// <summary>
		/// Types des paramètres de la méthode ou du constructeur.
		/// </summary>
		public Type[] ParameterTypes { get; private set; }
		
		/// <summary>
		/// Obtient les noms des paramètres.
		/// </summary>
		public string[] ParameterNames { get; private set; }
		
		/// <summary>
		/// Obtient si la méthode de Method est une méthode Alter ou un constructeur.
		/// </summary>
		public bool IsCtor { get; private set; }
		
		/// <summary>
		/// Constructeur. Les propriétés ParameterTypes, etc. se remplissent automatiquement à partir de method.
		/// </summary>
		public SpObjectCtorInfos(Type type, string name, string shortName, string overName, bool isBaseObj, string group, MethodBase method) : this()
		{
			Type = type; Name = name; ShortName = shortName; OverloadName = overName; Group = group;
			IsBaseObject = isBaseObj; Method = method; IsCtor = method.IsConstructor; IsAbstract = type.IsAbstract;
			ParameterTypes = Array.ConvertAll<ParameterInfo,Type>(method.GetParameters(), delegate(ParameterInfo pi) { return pi.ParameterType; });
			ParameterNames = Array.ConvertAll<ParameterInfo,string>(method.GetParameters(), delegate(ParameterInfo pi) { return pi.Name; });
		}

		/// <summary>
		/// Affiche la description de l'objet.
		/// </summary>
		public override string ToString()
			{ return Name; }
	
	}



	// ===========================================================================
	
	
	
	/// <summary>
	/// Fournit une collection de SpObjectCtorInfos.
	/// </summary>
	public class SpObjectCtorInfosCollection : IEnumerator
	{
	
		private static SpObjectCtorInfosCollection _instance;
		private SpObjectCtorInfos[] _ctorInfos;
		private Dictionary<Type,string> _spObjTypeNames;
		private int _enumeratorIndex;
		private Type[] _spObjTypes;
		
		
		/// <summary>
		/// Obtient les types SpObject pour lesquels il existe des informations dans la collection.
		/// </summary>
		public Type[] SpObjectTypes { get { return _spObjTypes; } }
		
		
		/// <summary>
		/// Constructeur. Initialise la liste de tous les types de SpObjects, et de leurs constructeurs et méthodes Alter.
		/// </summary>
		private SpObjectCtorInfosCollection()
		{
		
			// Création d'un dictionnaire pour les noms:
			_spObjTypeNames = new Dictionary<Type,string>();
			// Objet de base:
			_spObjTypeNames.Add(typeof(SpObject), "Object,Obj,Obj,1,Object");
			//names.Add(typeof(SpBrushObject), "BrushObject,BrushObject,BrushObject,1");
			//names.Add(typeof(SpPenObject), "PenObject,PenObject,BrushObject,1");
			// Point de l'espace:
			_spObjTypeNames.Add(typeof(SpPointObject), "PointObject,PtObj,Pt,1,Space points");
			_spObjTypeNames.Add(typeof(SpPoint), "Point,Pt,Pt,0,Space points");
			_spObjTypeNames.Add(typeof(SpPointPolar), "PointPolar,PolPt,Pt,0,Space points");
			_spObjTypeNames.Add(typeof(SpPointPolar3), "PointPolar3UsingCoords,Pol3Pt,Pt,0,Space points");
			_spObjTypeNames.Add(typeof(SpPointPolar3UsingCoords), "PointPolar3UsingCoords,Pol3PtUsCoords,Pt,0,Space points");
			_spObjTypeNames.Add(typeof(SpMidpoint), "Midpoint,Midpt,Pt,0,Space points");
			_spObjTypeNames.Add(typeof(SpImagePoint), "ImagePoint,ImgPt,Pt,0,Space points");
			_spObjTypeNames.Add(typeof(SpBarycenter), "Barycenter,Bar,Pt,0,Space points");
			_spObjTypeNames.Add(typeof(SpPointOnLine), "PointOnLine,LinePt,Pt,0,Space points");
			_spObjTypeNames.Add(typeof(SpPointOnSphere), "PointOnSphere,SphPt,Pt,0,Space points");
			_spObjTypeNames.Add(typeof(SpOrthoProjPointOnLine), "OrthoProjPointOnLine,LineOrthoProjPt,Pt,0,Space points");
			_spObjTypeNames.Add(typeof(SpLineSphereIntersection), "LineSphereIntersection,LineSphInter,Inter,0,Space points");
			_spObjTypeNames.Add(typeof(SpLinesIntersection), "LinesIntersection,LinesInter,Inter,0,Space points");
			_spObjTypeNames.Add(typeof(SpPointOnFunction2), "PointOnFunction2,Func2Pt,Pt,0,Space points");
			// Objets du plan:
			_spObjTypeNames.Add(typeof(SpPlaneObject), "PlaneObject,PlaneObj,Plane,1,Plane objects");
			_spObjTypeNames.Add(typeof(SpPlane), "Plane,Plane,Plane,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpPlaneUsingPoints), "PlaneUsingPoints,PlaneUsPts,Plane,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpParallelPlane), "ParallelPlane,ParPlane,Plane,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpOrthonormalPlane), "OrthonormalPlane,OrthoPlane,Plane,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpOrthogonalPlaneToVector), "OrthogonalPlaneToVector,VecOrthoPlane,Plane,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpOrthogonalPlaneToLine), "OrthogonalPlaneToLine,LineOrthoPlane,Plane,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpOrthogonalPlaneToLineUsingPoints), "OrthogonalPlaneToLineUsingPoints,LineOrthoPlaneUsPts,Plane,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpOrthogonalPlaneToPlane), "OrthogonalPlaneToPlane,OrthogonalPlane,Plane,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpCircle), "Circle,Circle,Circle,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpCircleUsingPoint), "CircleUsingPoint,CircleUsPt,Circle,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpEllipse), "Ellipse,Ellipse,Circle,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpPlaneSphereIntersection), "PlaneSphereIntersection,PlaneSphInter,Inter,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpPolygon), "Polygon,Poly,Poly,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpPolygonOnPlane), "PolygonOnPlane,PlanePoly,Poly,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpRectangleOnPlane), "Rectangle,Rect,Poly,0,Plane objects");
			_spObjTypeNames.Add(typeof(SpRegularPolygonOnPlane), "RegularPolygonOnPlane,RegPoly,Poly,0,Plane objects");
			// Angles
			_spObjTypeNames.Add(typeof(SpAngle), "Angle,Angle,Angle,0,Angles");
			_spObjTypeNames.Add(typeof(SpFixedAngle), "FixedAngle,FixAngle,Angle,0,Angles");
			_spObjTypeNames.Add(typeof(SpAngleOnPlane), "AngleOnPlane,PlaneAngle,Angle,0,Angles");
			_spObjTypeNames.Add(typeof(SpFixedAngleOnPlane), "FixedAngleOnPlane,PlaneFixAngle,Angle,0,Angles");
			// Fonctions:
			_spObjTypeNames.Add(typeof(SpFunctionObject), "FunctionObject,FuncObj,Func,1,Functions");
			_spObjTypeNames.Add(typeof(SpFunction1OnPlane), "Function1OnPlane,PlaneFunc1,Func,0,Functions");
			_spObjTypeNames.Add(typeof(SpFunction2), "Function2,Func2,Func,0,Functions");
			_spObjTypeNames.Add(typeof(SpFunction2OnPlane), "Function2OnPlane,PlaneFunc2,Func,0,Functions");
			_spObjTypeNames.Add(typeof(SpFunction3), "Function3,Func3,Func,0,Functions");
			// Autres:
			_spObjTypeNames.Add(typeof(SpText), "Text,Text,Text,0,Others");
			_spObjTypeNames.Add(typeof(SpCursor), "Cursor,Cur,Cur,0,Others");
			// Droites, etc.:
			_spObjTypeNames.Add(typeof(SpLineObject), "LineObject,LineObj,Line,0,Lines");
			_spObjTypeNames.Add(typeof(SpLine), "Line,Line,Line,0,Lines");
			_spObjTypeNames.Add(typeof(SpLineUsingVector), "LineUsingVector,LineUsVec,Line,0,Lines");
			_spObjTypeNames.Add(typeof(SpParallelLine), "ParallelLine,ParLine,ParLine,0,Lines");
			_spObjTypeNames.Add(typeof(SpPerpendicularLine), "PerpendicularLine,PerLine,PerLine,0,Lines");
			_spObjTypeNames.Add(typeof(SpPerpendicularLineToPlane), "PerpendicularLineToPlane,PlanePerLine,PerLine,0,Lines");
			_spObjTypeNames.Add(typeof(SpSegment), "Segment,Seg,Seg,0,Lines");
			_spObjTypeNames.Add(typeof(SpRay), "Ray,Ray,Ray,0,Lines");
			_spObjTypeNames.Add(typeof(SpPlanesIntersection), "PlanesIntersection,PlanesInter,Inter,0,Lines");
			_spObjTypeNames.Add(typeof(SpPlanePolygonIntersection), "PlanePolygonIntersection,PlanePolyInter,Inter,0,Lines");
			_spObjTypeNames.Add(typeof(SpAngleBissector), "AngleBissector,Biss,Line,0,Lines");
			_spObjTypeNames.Add(typeof(SpCircleTangent), "CircleTangent,CircleTan,Line,0,Lines");
			_spObjTypeNames.Add(typeof(SpFunction1OnPlaneTangent), "Function1OnPlanTangent,Func1Tan,Line,0,Lines");
			// Vecteurs:
			_spObjTypeNames.Add(typeof(SpVectorObject), "VectorObject,VecObj,Vec,1,Vectors");
			_spObjTypeNames.Add(typeof(SpVectorUsingCoords), "VectorUsingCoords,VectUsCoords,Vec,0,Vectors");
			_spObjTypeNames.Add(typeof(SpVectorUsingPoints), "VectorUsingPoints,VecUsPts,Vec,0,Vectors");
			_spObjTypeNames.Add(typeof(SpVectorUsingPointsAndOrigin), "SpVectorUsingPointsAndOrigin,VecUsPtsAndOrig,Vec,0,Vectors");
			_spObjTypeNames.Add(typeof(SpVectorUsingPointsAndCoeff), "VectorUsingPointsAndCoeff,VecUsPtsCoeff,Vec,0,Vectors");
			_spObjTypeNames.Add(typeof(SpVectorUsingMultiply), "VectorUsingMultiply,VecUsMul,Vec,0,Vectors");
			_spObjTypeNames.Add(typeof(SpVectorUsingSum), "VectorUsingSum,VecUsSum,Vec,0,Vectors");
			_spObjTypeNames.Add(typeof(SpOrthonormalVector), "OrthonormalVector,OrthoVec,Vec,0,Vectors");
			_spObjTypeNames.Add(typeof(SpNormalVectorToLine), "NormalVectorToLine,LineNormalVec,Vec,0,Vectors");
			_spObjTypeNames.Add(typeof(SpNormalVectorToLineStartingAtLine), "NormalVectorToLineStartingAtLine,LineNormalVecAtLine,Vec,0,Vectors");
			_spObjTypeNames.Add(typeof(SpNormalVectorToPlane), "NormalVectorToPlane,PlaneNormalVec,Vec,0,Vectors");
			_spObjTypeNames.Add(typeof(SpNormalVectorToPlaneStartingAtPlane), "NormalVectorToPlaneStartingAtPlane,PlaneNormalVecAtPlane,Vec,0,Vectors");
			// Objets de l'espace:
			_spObjTypeNames.Add(typeof(SpSphere), "Sphere,Sph,Sph,0,Space objects");
			_spObjTypeNames.Add(typeof(SpSphereUsingPoint), "SphereUsingPoint,SphUsPt,Sph,0,Space objects");
			_spObjTypeNames.Add(typeof(SpSolid), "Solid,Solid,Solid,0,Space objects");
			_spObjTypeNames.Add(typeof(SpSolidWithCenter), "SolidWithCenter,CenteredSolid,Solid,0,Space objects");
			_spObjTypeNames.Add(typeof(SpParallelepiped), "Parallelepiped,Cube,Solid,0,Space objects");
			_spObjTypeNames.Add(typeof(SpCone), "Cone,Cone,Solid,0,Space objects");
			_spObjTypeNames.Add(typeof(SpRegularTetrahedron), "RegularTetrahedron,Tetra,Solid,0,Space objects");
			_spObjTypeNames.Add(typeof(SpRegularTetrahedronOnPlane), "RegularTetrahedronOnPlane,PlaneTetra,Solid,0,Space objects");
			_spObjTypeNames.Add(typeof(SpCubeOnPlane), "CubeOnPlane,PlaneCube,Solid,0,Space objects");
			// Points du plan:
			_spObjTypeNames.Add(typeof(SpPointOnPlaneObject), "PointOnPlaneObject,PlanePtObj,Pt,1,Plane points");
			_spObjTypeNames.Add(typeof(SpPointOnPlaneFromSpace), "PointOnPlaneFromSpace,SpPlanePt,Pt,0,Plane points");
			_spObjTypeNames.Add(typeof(SpPointOnPlane), "PointOnPlane,PlanePt,Pt,0,Plane points");
			_spObjTypeNames.Add(typeof(SpPointOnPlanePolar), "PointOnPlanePolar,PlanePolPt,Pt,0,Plane points");
			_spObjTypeNames.Add(typeof(SpPointOnFunction1OnPlane), "PointOnFunction1OnPlane,Func1Pt,Pt,0,Plane points");
			_spObjTypeNames.Add(typeof(SpPointOnCircle), "PointOnCircle,CirclePt,Pt,0,Plane points");
			_spObjTypeNames.Add(typeof(SpLineCircleIntersection), "LineCircleIntersection,LineCircleInter,Inter,0,Plane points");
			_spObjTypeNames.Add(typeof(SpPlaneLineIntersection), "PlaneLineIntersection,PlaneLineInter,Inter,0,Plane points");
			_spObjTypeNames.Add(typeof(SpOrthoProjPointOnPlane), "OrthoProjPointOnPlane,PlaneOrthoProjPt,PlanePt,0,Plane points");
			// Transformations:
			_spObjTypeNames.Add(typeof(SpTransformationObject), "TransformationObject,Transf,Transf,1,Transformations");
			_spObjTypeNames.Add(typeof(SpTranslation), "Translation,Translation,Transf,0,Transformations");
			_spObjTypeNames.Add(typeof(SpAxialRotation), "AxialRotation,AxialRot,Transf,0,Transformations");
			_spObjTypeNames.Add(typeof(SpRotationOnPlane), "RotationOnPlane,PlaneRot,Transf,0,Transformations");
			_spObjTypeNames.Add(typeof(SpRotation), "Rotation,Rot,Transf,0,Transformations");
			_spObjTypeNames.Add(typeof(SpRotationOfSolid), "RotationOfSolid,SolidRot,Transf,0,Transformations");
			_spObjTypeNames.Add(typeof(SpHomothety), "Homothety,Homoth,Transf,0,Transformations");
			_spObjTypeNames.Add(typeof(SpAxialSymmetry), "AxialSymmetry,AxialSym,Transf,0,Transformations");
			// Objets transformés:
			_spObjTypeNames.Add(typeof(SpTrPoint), "TrPoint,TrPt,TrObj,0,Transformed objects");
			_spObjTypeNames.Add(typeof(SpTrPointOnPlane), "TrPointOnPlane,TrPlanePt,TrObj,0,Transformed objects");
			_spObjTypeNames.Add(typeof(SpTrSolid), "TrSolid,TrSolid,TrObj,0,Transformed objects");
			
			// Transfert dans le tableau des objets:
			_spObjTypes = _spObjTypeNames.Keys.ToArray();
			// Pour tous les types, ajoute les constructeurs et méthodes Alter dans le tableau:
			MethodBase[] mbs; string[] split, separator = new string[]{","};
			_ctorInfos = new SpObjectCtorInfos[200]; int c = 0;
			foreach (Type t in _spObjTypes)
			{
				mbs = ((MethodBase[])t.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
						.Concat((MethodBase[])t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
						.Where(delegate(MethodBase mi) { return (mi.Name == "Alter" && mi.DeclaringType == t); }).ToArray()).ToArray();
				split = _spObjTypeNames[t].Split(separator, StringSplitOptions.None);
				foreach (MethodBase mb in mbs)
					{ _ctorInfos[c++] = new SpObjectCtorInfos(t, split[0], split[1], split[2], (split[3]=="1"), split[4], mb); }
			}
			
			// Retaille le tableau:
			Array.Resize(ref _ctorInfos, c);
			
		}
		
		/// <summary>
		/// Retourne l'instance unique.
		/// </summary>
		public static SpObjectCtorInfosCollection GetInstance()
		{
			if (_instance == null) { _instance = new SpObjectCtorInfosCollection(); }
			return _instance;
		}
		
		/// <summary>
		/// Retourne le nom du type d'objet passé en argument.
		/// </summary>
		public string GetNameOf(Type type)
			{ return _spObjTypeNames[type].Split(new string[]{","}, StringSplitOptions.None)[0]; }
		
		/// <summary>
		/// Retourne le nom du type d'objet passé en argument.
		/// </summary>
		public string GetShortNameOf(Type type)
			{ return _spObjTypeNames[type].Split(new string[]{","}, StringSplitOptions.None)[1]; }
		
		/// <summary>
		/// Retourne le nom du type d'objet passé en argument.
		/// </summary>
		public string GetOverloadNameOf(Type type)
			{ return _spObjTypeNames[type].Split(new string[]{","}, StringSplitOptions.None)[2]; }
		
		/// <summary>
		/// Retourne le goupe de l'objet.
		/// </summary>
		public string GetGroupOf(Type type)
			{ return _spObjTypeNames[type].Split(new string[]{","}, StringSplitOptions.None)[4]; }
		
		// MEDHODES D'ENUMERATION:
		
		public IEnumerator GetEnumerator() { _enumeratorIndex = -1; return this; }
		public bool MoveNext() { return ++_enumeratorIndex < _ctorInfos.Length; }
		public void Reset() { _enumeratorIndex = -1; }
		public object Current { get { return _ctorInfos[_enumeratorIndex]; } }
	
	}



}
