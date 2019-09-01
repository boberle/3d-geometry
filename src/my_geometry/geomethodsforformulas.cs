using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace My
{






	// ===========================================================================
	



	/// <summary>
	/// Fournit une collection contenant des informations sur les constructeurs et les méthodes Alter des SpObjects, et créer un assemblage dynamique contenant les fonctions à utiliser dans les formules pour obtenir ou créer des objets virtuels.
	/// </summary>
	public static class GeoMethodsForFormulas
	{


	
		private static MethodInfo[] _methods;
		private static string[] _methodsNames;
		private static Assembly _dynAssembly;
		private static SpObjectsCollection _coll;
		private static My.Buffer<SpObject> _virtObjsBuf;
		private readonly static string _dynClassName;
		private readonly static string _dynClassFullname;
		private readonly static string _dynClassNamespace;
		private readonly static string _SetVirtObjBufferMethodFullname;


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Obtient les méthodes de cette classe, et les méthodes créés dynamiquement.
		/// </summary>
		public static MethodInfo[] MethodsForFormulas { get { return _methods; } }
		
		/// <summary>
		/// Obtient les nom des méthodes de cette classe, et des méthodes créées dynamiquement.
		/// </summary>
		public static string[] MethodsForFormulasNames{ get { return _methodsNames; } }


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Constructeur. Création des méthodes qui sont compilés dans un nouvel assemblage.
		/// </summary>
		static GeoMethodsForFormulas()
		{
		
			// Initialisation des variables:
			_coll = SpObjectsCollection.GetInstance();
			_virtObjsBuf = new Buffer<SpObject>(5);
			_dynClassName = "DynamicMethodsForFormulas";
			_dynClassNamespace = "My";
			_dynClassFullname = String.Format("{0}.{1}", _dynClassNamespace, _dynClassName);
			_SetVirtObjBufferMethodFullname = "GeoMethodsForFormulas.SetVirtualObjectsBuffer";
			
			// Méthodes définies non dynamiquement:
			_methods = typeof(GeoMethodsForFormulas.Functions).GetMethods(BindingFlags.Static | BindingFlags.Public);
			_methods = _methods.Where(delegate(MethodInfo mi) { return (!mi.IsSpecialName); }).ToArray();
			_methodsNames = Array.ConvertAll<MethodInfo,string>(_methods, delegate(MethodInfo m) { return m.Name; });

			// Si l'assemblage existe déjà, et que le numéro de version correspond, on le charge sans le recompiler:
			string savedCD = Environment.CurrentDirectory; Environment.CurrentDirectory = My.App.ExePath;
			string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			string savedVersion = My.Geometry.MySettings.DynamicAssemblyGeometryVersion;
			string path = My.Geometry.MySettings.DynamicAssemblyPath;
			if (App.DebugMode) { path = ""; }
			if (path != "" && savedVersion == currentVersion)
			{
				_dynAssembly = Assembly.LoadFile(path);
				GeoMsgSender.SendInfos(typeof(GeoMethodsForFormulas), String.Format("\nAssembly {0} loaded with success.\n", path));
			}
			// Sinon, on le recompile:
			else
			{
				if ((_dynAssembly = CompileAssembly(out path)) == null) { return; }
				GeoMsgSender.SendInfos(typeof(GeoMethodsForFormulas), String.Format("\nAssembly {0} compiled with success.\n", path));
				// Sauve le nom et le numéro de version (si pas mode debug):
				if (App.DebugMode) { path = ""; } 
				My.Geometry.MySettings.DynamicAssemblyPath = Path.Combine(My.App.ExePath, path);
				My.Geometry.MySettings.DynamicAssemblyGeometryVersion = currentVersion;
			}
			Environment.CurrentDirectory = savedCD;
		
			// Ajoute les méthodes de l'assemblage dynamique, en supprimant les méthodes get et set des prop (SpecialName):
			_methods = _methods.Concat(_dynAssembly.GetType(_dynClassFullname).GetMethods(BindingFlags.Static | BindingFlags.Public)).ToArray();
			/*_methods = _methods.Where(delegate(MethodInfo mi) { return (!mi.IsSpecialName); }).ToArray();*/ // Il n'y a plus de propriété
			_methodsNames = Array.ConvertAll<MethodInfo,string>(_methods, delegate(MethodInfo m) { return m.Name; });
		
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Génère un assemblage dynamique pour la création automatique des methodes de création et d'acquisition d'objet dans les formules. Retourne null en cas d'erreur.
		/// </summary>
		private static Assembly CompileAssembly(out string filename)
		{
			
			// Propriétés du code généré:
			CompilerParameters parameters = new CompilerParameters(new string[]{"System.dll","My.Geometry.dll","My.Maths2.dll"},
				_dynClassFullname + ".dll");
			parameters.IncludeDebugInformation = false;
			parameters.GenerateExecutable = false; // Bibliothèque de classe
			parameters.GenerateInMemory = false;
			parameters.TreatWarningsAsErrors = true;
			parameters.WarningLevel = 4;
			//parameters.CompilerOptions = "/optimize"; // Déjà inclus par défaut
			// Création et paramétrage d'une nouvelle unité de code:
			CodeCompileUnit codeUnit = new CodeCompileUnit();
			codeUnit.Namespaces.Add(new CodeNamespace(_dynClassNamespace));
			codeUnit.Namespaces[0].Imports.Add(new CodeNamespaceImport("System"));
			CodeTypeDeclaration dynClass = new CodeTypeDeclaration(_dynClassName);
			dynClass.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			codeUnit.Namespaces[0].Types.Add(dynClass);
			
			// Attribut:
			CodeAttributeDeclaration excludeFromManAtt = new CodeAttributeDeclaration(
				new CodeTypeReference(typeof(ExcludeFromManAttribute))); // N'inclut pas dans le man les surcharges.
			CodeAttributeDeclaration createAtt = new CodeAttributeDeclaration(
				new CodeTypeReference(typeof(CreateObjectFormulaFunctionAttribute)));
			CodeAttributeDeclaration getAtt = new CodeAttributeDeclaration(
				new CodeTypeReference(typeof(GetObjectFormulaFunctionAttribute)));
			
			// Méthode anonyme pour les attributs de catégories (array est le tableau des catégories, séparées par des virgules):
			Func<string,CodeAttributeDeclaration> getCatAttr =
				delegate(string array)
				{
					// Créer l'instruction pour le tableau:
					string[] split = array.Split(','); int l = split.Length;
					for (int i=0; i<l; i++) { split[i] = String.Format("\"{0}\"", split[i]); }
					string instr = String.Format("new string[]{{{0}}}", ArrayFunctions.Join(split, ","));
					return new CodeAttributeDeclaration(new CodeTypeReference(typeof(FormulaFunctionCategoriesAttribute)),
						new CodeAttributeArgument(new CodeSnippetExpression(instr)));
				};
			
			// Pour tous les SpObjects, ajoute une (ou deux, ou trois) fonction par constructeur:
			CodeMemberMethod method; StringBuilder methCode; int len; bool dblfFound, spobjFound;
			SpObjectCtorInfosCollection coll = SpObjectCtorInfosCollection.GetInstance();
			foreach (SpObjectCtorInfos o in coll)
			{
				if (!o.IsCtor || o.IsAbstract || o.IsBaseObject) { continue; }
				len = o.ParameterTypes.Length;
				// Méthode de création avec les paramètres d'origine:
				method = new CodeMemberMethod();
				method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
				method.CustomAttributes.Add(createAtt); 
				method.CustomAttributes.Add(getCatAttr(String.Format("Create objects,{0}", o.Group)));
				method.Name = "Cr" + o.ShortName;
				method.ReturnType = new CodeTypeReference(o.Type);
				methCode = new StringBuilder(String.Format("object[] args = new object[{0}];", len));
				dblfFound = false; spobjFound = false;
				for (int i=0; i<len; i++) {
					if (i == 0) { methCode.AppendFormat("args[{0}] = \"$\";", i); continue; }
					method.Parameters.Add(new CodeParameterDeclarationExpression(o.ParameterTypes[i], o.ParameterNames[i]));
					methCode.AppendFormat("args[{0}] = {1};", i, o.ParameterNames[i]);
					dblfFound = dblfFound || (o.ParameterTypes[i] == typeof(DoubleF));
					spobjFound = spobjFound || (o.ParameterTypes[i] == typeof(SpObject) || o.ParameterTypes[i].IsSubclassOf(typeof(SpObject))); }
				methCode.AppendFormat("SpObject o = (SpObject)typeof({0}).GetConstructor(Type.GetTypeArray(args)).Invoke(args);", o.Type.Name);
				methCode.AppendFormat("{0}(o);", _SetVirtObjBufferMethodFullname);
				methCode.AppendFormat("return ({0})o;", o.Type.Name);
				method.Statements.Add(new CodeSnippetExpression(methCode.ToString()));
					dynClass.Members.Add(method);
				// Méthode de création en remplaçant les DoubleF par des double:
				if (dblfFound)
				{
					method = new CodeMemberMethod();
					method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
					method.CustomAttributes.Add(createAtt);
					method.CustomAttributes.Add(excludeFromManAtt); // Exclut du manuel
					method.CustomAttributes.Add(getCatAttr(String.Format("Create objects,{0}", o.Group)));
					method.Name = "Cr" + o.ShortName;
					method.ReturnType = new CodeTypeReference(o.Type);
					methCode = new StringBuilder(String.Format("object[] args = new object[{0}];", len));
					dblfFound = false;
					for (int i=0; i<len; i++) {
					if (i == 0) { methCode.AppendFormat("args[{0}] = \"$\";", i); continue; }
						if (o.ParameterTypes[i] == typeof(DoubleF)) {
							method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(double), o.ParameterNames[i]));
							methCode.AppendFormat("args[{0}] = new DoubleF({1});", i, o.ParameterNames[i]); }
						else {
							method.Parameters.Add(new CodeParameterDeclarationExpression(o.ParameterTypes[i], o.ParameterNames[i]));
							methCode.AppendFormat("args[{0}] = {1};", i, o.ParameterNames[i]); } }
				methCode.AppendFormat("SpObject o = (SpObject)typeof({0}).GetConstructor(Type.GetTypeArray(args)).Invoke(args);", o.Type.Name);
				methCode.AppendFormat("{0}(o);", _SetVirtObjBufferMethodFullname);
				methCode.AppendFormat("return ({0})o;", o.Type.Name);
					method.Statements.Add(new CodeSnippetExpression(methCode.ToString()));
					dynClass.Members.Add(method);
				}
				// Méthode de création en remplaçant les DoubleF par des double, et les SpObjects pour des String:
				if (spobjFound)
				{
					method = new CodeMemberMethod();
					method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
					method.CustomAttributes.Add(createAtt);
					method.CustomAttributes.Add(excludeFromManAtt); // Exclut du manuel
					method.CustomAttributes.Add(getCatAttr(String.Format("Create objects,{0}", o.Group)));
					method.Name = "Cr" + o.ShortName;
					method.ReturnType = new CodeTypeReference(o.Type);
					methCode = new StringBuilder(String.Format("object[] args = new object[{0}];", len));
					dblfFound = false;
					for (int i=0; i<len; i++) {
					if (i == 0) { methCode.AppendFormat("args[{0}] = \"$\";", i); continue; }
						if (o.ParameterTypes[i] == typeof(DoubleF)) {
							method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(double), o.ParameterNames[i]));
							methCode.AppendFormat("args[{0}] = new DoubleF({1});", i, o.ParameterNames[i]); }
						else if (o.ParameterTypes[i] == typeof(SpObject) || o.ParameterTypes[i].IsSubclassOf(typeof(SpObject))) {
							method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), o.ParameterNames[i]));
							methCode.AppendFormat("args[{0}] = ({1})_coll[{2},typeof({1}),false];", i, o.ParameterTypes[i].Name,
								o.ParameterNames[i]); }
						else {
							method.Parameters.Add(new CodeParameterDeclarationExpression(o.ParameterTypes[i], o.ParameterNames[i]));
							methCode.AppendFormat("args[{0}] = {1};", i, o.ParameterNames[i]); } }
				methCode.AppendFormat("SpObject o = (SpObject)typeof({0}).GetConstructor(Type.GetTypeArray(args)).Invoke(args);", o.Type.Name);
				methCode.AppendFormat("{0}(o);", _SetVirtObjBufferMethodFullname);
				methCode.AppendFormat("return ({0})o;", o.Type.Name);
					method.Statements.Add(new CodeSnippetExpression(methCode.ToString()));
					dynClass.Members.Add(method);
				}
			}
			
			// Pour tous les objets, ajoute une méthode Get à partir du nom:
			CodeTypeReference getAttType = new CodeTypeReference(typeof(GetObjectFormulaFunctionAttribute));
			foreach (Type type in coll.SpObjectTypes)
			{
				method = new CodeMemberMethod();
				method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
				method.CustomAttributes.Add(getAtt);
				method.CustomAttributes.Add(getCatAttr(String.Format("Get objects,{0}", coll.GetGroupOf(type))));
				method.Name = "Get" + coll.GetShortNameOf(type);
				method.ReturnType = new CodeTypeReference(type);
				method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "name"));
				methCode = new StringBuilder(String.Format("return ({0})_coll[name,typeof({0}),false];", type.Name));
				method.Statements.Add(new CodeSnippetExpression(methCode.ToString()));
				dynClass.Members.Add(method);
			}
			
			// Insère des commandes pour obtenir les propriétés de chaque objets:
			// Pour tous les objets, ajoute une méthode Get à partir du nom:
			PropertyInfo[] pis; Type[] allowedTypes = new Type[]{typeof(string),typeof(double),typeof(DoubleF),typeof(SpObject),
				typeof(Coord2D),typeof(Coord3D)};
			foreach (Type type in coll.SpObjectTypes)
			{
				pis = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
				foreach (PropertyInfo pi in pis)
				{
					// Si c'est une propriété d'une des trois classes abstraites de base, on saute:
					if (pi.DeclaringType.IsAbstract) { continue; }
					// Avec la ligne suivante, on peut limiter le nombre de fonctions en inscrivant que les
					// propriété pour le type déclarant, et non pour tous les types héritiers, ce qui est assez
					// inutile, puisqu'on peut toujours obtenir le type parent avec les méthodes GetXXX:
					if (pi.DeclaringType != type) { continue; }
					// On élimine les propriétés qui ne retourne pas du texte, un SpObject, un Double, un DoubleF ou un Coord2/3D:
					if (!allowedTypes.Contains(pi.PropertyType) && !pi.PropertyType.IsSubclassOf(typeof(SpObject))) { continue; }
					// Saute si la propriété est TypeDescription:
					if (pi.Name == "TypeDescription") { continue; }
					// Obtention une propriété avec un objet comme argument:
					method = new CodeMemberMethod();
					method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
					method.CustomAttributes.Add(getCatAttr(String.Format("Get properties,{0}", coll.GetGroupOf(type))));
					if (pi.PropertyType == typeof(SpObject) || pi.PropertyType.IsSubclassOf(typeof(SpObject)))
						{ method.CustomAttributes.Add(getAtt); }
					method.Name = coll.GetShortNameOf(type) + "_" + pi.Name;
					method.ReturnType = new CodeTypeReference(pi.PropertyType);
					method.Parameters.Add(new CodeParameterDeclarationExpression(type, "obj"));
					methCode = new StringBuilder(String.Format("return obj.{0};", pi.Name));
					method.Statements.Add(new CodeSnippetExpression(methCode.ToString()));
					dynClass.Members.Add(method);
						// Obtention une propriété avec un texte comme argument:
						method = new CodeMemberMethod();
						method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
						if (pi.PropertyType == typeof(SpObject) || pi.PropertyType.IsSubclassOf(typeof(SpObject)))
							{ method.CustomAttributes.Add(getAtt);   }
						method.CustomAttributes.Add(excludeFromManAtt); // Exclut du manuel
						method.CustomAttributes.Add(getCatAttr(String.Format("Get properties,{0}", coll.GetGroupOf(type))));
						method.Name = coll.GetShortNameOf(type) + "_" + pi.Name;
						method.ReturnType = new CodeTypeReference(pi.PropertyType);
						method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "name"));
						methCode = new StringBuilder(String.Format("return (({0})_coll[name,typeof({0}),false]).{1};", type.Name, pi.Name));
						method.Statements.Add(new CodeSnippetExpression(methCode.ToString()));
						dynClass.Members.Add(method);
				}
			}

			// Ajoute un champ et un constructeur pour la collection des objets de l'espace (pour les méthodes Get):
			CodeMemberField field = new CodeMemberField(typeof(SpObjectsCollection), "_coll");
			field.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			field.InitExpression = null;
			dynClass.Members.Add(field);
			/*CodeMemberProperty property = new CodeMemberProperty();
			property.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			property.Name = _dynClassCollProperty;
			property.Type = new CodeTypeReference(typeof(SpObjectsCollection));
			property.SetStatements.Add(new CodeSnippetExpression("_coll = value;"));
			property.GetStatements.Add(new CodeSnippetExpression("return _coll;"));
			dynClass.Members.Add(property);*/
			CodeTypeConstructor ctor = new CodeTypeConstructor();
			ctor.Statements.Add(new CodeSnippetExpression("_coll = SpObjectsCollection.GetInstance();"));
			dynClass.Members.Add(ctor);
			
			// Vérifie et compile l'assemblage:
			CodeGenerator.ValidateIdentifiers(codeUnit);
			CompilerResults  compiledResult = CodeDomProvider.CreateProvider("CSharp")
				.CompileAssemblyFromDom(parameters, new CodeCompileUnit[]{codeUnit});
			// Affiche les messages et/ou erreurs:
			if (compiledResult.Output.Count > 0 && compiledResult.Output.Count < 8) {
				GeoMsgSender.SendInfos(typeof(GeoMethodsForFormulas),
				 String.Format("\nCompilation of {0}:\n{1}", _dynClassFullname,
				My.ArrayFunctions.Join<string>(compiledResult.Output.OfType<string>().ToArray(), "\n"))); }
			else if (compiledResult.Output.Count > 0 || compiledResult.Errors.Count > 0) {
				string msg = String.Format("\nCompilation of {0}:\n", _dynClassFullname);
					msg += String.Format("Messages:\n{0}\nErrors:\n{1}",
					My.ArrayFunctions.Join<string>(compiledResult.Output.OfType<string>().ToArray(), "\n"),
					My.ArrayFunctions.Join<string>(compiledResult.Errors.OfType<string>().ToArray(), "\n"));
					msg += "Some methods for formulas are not available.";
					My.ErrorHandler.ShowError(new Exception(msg));
					if (App.DebugMode) { System.Windows.Forms.MessageBox.Show(msg); }
					_methods = new MethodInfo[0]; filename = ""; return null; }
			// Récupère l'assemblage et créer les délégués pour chaque méthodes:
			filename = compiledResult.PathToAssembly;
			return compiledResult.CompiledAssembly;

		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne les objets virtuels créer par les méthodes pour les formules, entre deux appels de cette fonction.
		/// </summary>
		public static SpObject[] GetVirtualObjectsBuffer()
			{ return _virtObjsBuf.Reset(); }
		
		/// <summary>
		/// Remet à zéro le buffer des objets virtuels créés.
		/// </summary>
		public static SpObject[] ResetVirtualObjectsBuffer()
			{ return _virtObjsBuf.Reset(); }
		
		/// <summary>
		/// Inscrit dans le buffer l'objet passé en argument. Cette fonction est réservée et ne doit pas être appelée.
		/// </summary>
		public static void SetVirtualObjectsBuffer(SpObject obj)
			{ _virtObjsBuf.SetValue(obj); }
	
		/// <summary>
		/// Supprime tous les objets dans le buffer et remet à zéro. (On peut appeler cette méthode, par exemple, lorsque des objets virtuels ont été créés dans une seule formule, mais que l'objet final n'a finalement pas été créé.)
		/// </summary>
		public static void DeleteVirtualObjectsInBuffer()
		{
			SpObject[] objs = _virtObjsBuf.Reset(); int len = objs.Length;
			for (int i=0; i<len; i++) { objs[i].Deleted = true; objs[i] = null; }
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Créer ou obtient un objet à partir d'une formule. Retourne true si la formule est valide et que l'objet a été créer, false sinon. Le paramètre de sortie creation indique si obj est un nouvel objet (créer par la formule) ou un objet déjà existant (obtenu par la formule). Les objets créés par une formule sont des objets virtuels: Leur nom est donc $[n]. objType indique le type d'objet désiré. La formule n'est valide que si elle retourne un objet du type objType, ou un sous-type de objType. La première fonction de la formule doit être une fonction marquée par un attribut [GetObjectFormulaFunction()] ou [CreateObjectFormulaFunction()]. Si createOrGet est négatif, la formule n'est acceptée que s'il y a obtention (et non création) d'un objet. S'il est positif, on attent une création, et s'il est 0, les deux sont acceptés.
		/// </summary>
		public static bool GetObjectFromFormula(string formula, Type objType, int createOrGet, out SpObject obj, out bool creation)
		{
			try
			{
				// Récupère le nom de la première fonction, la cherche, puis détermine si l'attribut est create ou get:
				int indexOf; MethodInfo meth = null; creation = false; obj = null;
				if ((indexOf = formula.IndexOf("(")) < 1) 
					{ GeoMsgSender.SendError(typeof(GeoMethodsForFormulas), String.Format("{0} can't create an object.", formula)); return false; }
				else if (indexOf > 0)
				{
					string func = formula.Substring(0, indexOf);
					int index = Array.IndexOf(MethodsForFormulasNames, func);
					if (index < 0) { GeoMsgSender.SendError(typeof(GeoMethodsForFormulas), String.Format("{0} is not a valid fucntion.", func)); return false; }
					meth = MethodsForFormulas[index];
					if (meth.GetCustomAttributes(typeof(CreateObjectFormulaFunctionAttribute), true).Length > 0) { creation = true; }
					else if (meth.GetCustomAttributes(typeof(GetObjectFormulaFunctionAttribute), true).Length > 0) { creation = false; }
					else { GeoMsgSender.SendError(typeof(GeoMethodsForFormulas),
						String.Format("{0} is not marked as a create ou get function.", func)); return false; }
					if ((creation && createOrGet < 0) || (!creation && createOrGet > 0)) {
						GeoMsgSender.SendError(typeof(GeoMethodsForFormulas),
						String.Format("{0} is not a {1} function.", func, (creation ? "get" : "create"))); return false; }
				}
				// Tente de créer la fonction (la vérification que le type d'objet créé ou obtenu correspond à objType se fait directement
				// dans la formule, en passan objType comme type de retour, puisque si ce n'est pas le bon type, l'analyse de la formule
				// lève une exception):
				Func<My.SpObject> deleg = (Func<My.SpObject>)My.Formula.CreateFormulaMethod(formula, null,typeof(Func<My.SpObject>),
					typeof(My.SpObject), objType, My.FormulaWorkingType.Double, null, null);
				obj = deleg();
				// Ajoute la formule si création:
				if (creation) { obj.CtorFormula = formula; }
				// Nettoie l'historique avant de sortir:
				ResetVirtualObjectsBuffer(); return true;
			}
			catch (Exception exc)
				{ My.ErrorHandler.ShowError(exc); obj = null; creation = false; DeleteVirtualObjectsInBuffer(); return false; }
		}


		// ===========================================================================
		
		
		/// <summary>
		/// Fournit les méthodes pour les formules:
		/// </summary>
		public static class Functions
		{
		
			[FormulaFunctionCategories("Get properties","Space points")]
			public static double X(SpPointObject obj)
				{ return obj.X; }
		
			[FormulaFunctionCategories("Get properties","Space points")]
			public static double Y(SpPointObject obj)
				{ return obj.Y; }
		
			[FormulaFunctionCategories("Get properties","Space points")]
			public static double Z(SpPointObject obj)
				{ return obj.Z; }
		
			[FormulaFunctionCategories("Get properties","Vectors")]
			public static double X(SpVectorObject obj)
				{ return obj.X; }
		
			[FormulaFunctionCategories("Get properties","Vectors")]
			public static double Y(SpVectorObject obj)
				{ return obj.Y; }
		
			[FormulaFunctionCategories("Get properties","Vectors")]
			public static double Z(SpVectorObject obj)
				{ return obj.Z; }
		
			[FormulaFunctionCategories("Get properties","Plane objects")]
			public static double Radius(SpCircle obj)
				{ return obj.Radius; }
		
			[FormulaFunctionCategories("Get properties","Space objects")]
			public static double Radius(SpSphere obj)
				{ return obj.Radius; }
			
			[FormulaFunctionCategories("Get properties","Vectors")]
			public static double Norm(SpVectorObject obj)
				{ return obj.Norm; }
			
			[FormulaFunctionCategories("Get properties","Lines")]
			public static double Length(SpSegment obj)
				{ return obj.Length; }

			[FormulaFunctionCategories("Get properties","Others")]
			public static double Cur(SpCursor obj)
				{ return obj.Value; }


			// ---------------------------------------------------------------------------
	
		
			[FormulaFunctionCategories("Get properties")]
			public static double X(string name)
			{
				SpObject o = _coll.GetObject(name, false);
				if (o is SpPointObject) { return ((SpPointObject)o).X; }
				if (o is SpVectorObject) { return ((SpVectorObject)o).X; }
				throw new SpObjectNotFoundException(name);
			}
			
			[FormulaFunctionCategories("Get properties")]
			public static double Y(string name)
			{
				SpObject o = _coll.GetObject(name, false);
				if (o is SpPointObject) { return ((SpPointObject)o).Y; }
				if (o is SpVectorObject) { return ((SpVectorObject)o).Y; }
				throw new SpObjectNotFoundException(name);
			}
			
			[FormulaFunctionCategories("Get properties")]
			public static double Z(string name)
			{
				SpObject o = _coll.GetObject(name, false);
				if (o is SpPointObject) { return ((SpPointObject)o).Z; }
				if (o is SpVectorObject) { return ((SpVectorObject)o).Z; }
				throw new SpObjectNotFoundException(name);
			}
			
			[FormulaFunctionCategories("Get properties")]
			public static double Radius(string name)
			{
				SpObject o = _coll.GetObject(name, false);
				if (o is SpCircle) { return ((SpCircle)o).Radius; }
				if (o is SpSphere) { return ((SpSphere)o).Radius; }
				throw new SpObjectNotFoundException(name);
			}

			[FormulaFunctionCategories("Get properties","Lines")]
			public static double Length(string name)
				{ return ((SpSegment)_coll[name, true]).Length; }

			[FormulaFunctionCategories("Get properties","Vectors")]
			public static double Norm(string name)
				{ return ((SpVectorObject)_coll[name, true]).Norm; }
			
			[FormulaFunctionCategories("Get properties","Others")]
			public static double Cur(string name)
				{ return ((SpCursor)_coll[name, true]).Value; }

			// ---------------------------------------------------------------------------
	

			[GetObjectFormulaFunction()]
			[FormulaFunctionCategories("Get properties","Owned objects")]
			public static SpObject GetOwnedObj(string ownerName, int ownedIndex)
			{
				SpObject parent = _coll[ownerName, true]; SpObject[] arr = SpObject.GetAllOwneds(parent);
				if (ownedIndex < 0 || ownedIndex >= arr.Length)
					{ throw new SpObjectNotFoundException(ownedIndex.ToString()); }
				return arr[ownedIndex];
			}
			
			[GetObjectFormulaFunction()]
			[FormulaFunctionCategories("Get properties","Owned objects")]
			public static SpObject GetOwnedObj(string ownerName, string ownedName)
			{
				SpObject parent = _coll[ownerName, true];
				foreach (SpObject o in parent.OwnedObjects) { if (o.SystemName == ownedName) { return o; } }
				throw new SpObjectNotFoundException(ownedName);
			}
			
			[GetObjectFormulaFunction()]
			[FormulaFunctionCategories("Get properties","Owned objects")]
			public static SpObject GetOwnedObj(SpObject owner, string ownedName)
			{
				foreach (SpObject o in owner.OwnedObjects) { if (o.SystemName == ownedName) { return o; } }
				throw new SpObjectNotFoundException(ownedName);
			}
			
			[GetObjectFormulaFunction()]
			[FormulaFunctionCategories("Get properties")]
			public static SpObject GetPropObj(SpObject obj, string propName)
			{
				PropertyInfo pi = obj.GetType().GetProperty(propName);
				if (pi == null) { throw new SpObjectNotFoundException(propName); }
				object temp = pi.GetValue(obj, null);
				if (temp as SpObject == null) { throw new SpObjectNotFoundException(propName); }
				return (SpObject)temp;
			}
			

		}
	
	}
	
	
	
	
}
