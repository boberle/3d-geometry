using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace My
{



		// ---------------------------------------------------------------------------
		// CLASSES SUPPLEMENTAIRES
		// ---------------------------------------------------------------------------




		#region CLASSES SUPPLEMENTAIRES



		/// <summary>
		/// Type de commande.
		/// </summary>
		public enum CommandType
		{
			FixedNbParams,
			VarNbParams,
			FixedAndVarNbParams,
			Choice
		}
		
		/// <summary>
		/// Type de réponse pour la console.
		/// </summary>
		public enum ConsoleYesNo { Yes,No }
		
		/// <summary>
		/// Type de réponse pour la console.
		/// </summary>
		public enum ConsoleYesNoCancel { Yes,No,Cancel }

		/// <summary>
		/// Délégué pour les commandes.
		/// </summary>
		public delegate void CmdDelegate(object[] parameters, ExecutionEventArgs e);
		

		// ---------------------------------------------------------------------------

	
		/// <summary>
		/// Classe qui définit chaque commande. Name est le nom de la commende (juste des lettres et des chiffres). Overload détermine un nom de surchage qui est retourné par la classe d'événement ExecutionEventArgs, mais ce n'est là que pour la lisibilité du code, car cette propriété n'est pas utilisée par la console. Help fournit l'aide lorsque l'utilisateur se trompe de syntaxe ou demande de l'aide. Aliases est un tableau qui contient des alias du nom de la commande : l'utilisateur peut entrer le nom Name, ou bien l'un des alias - le résultat sera le même. Pour SyntaxDescription, voir le commantaire de la propriété. AllowEmptyString indique si l'utilisateur peut entrer un texte vide (false par défaut), la chaîne retourné sera alors String.Empty.
		///	Si choices est null, alors la commande regarde ParamsTypes et demande à l'utilisateur les types demandés, qui peuvent être des tableaux, des tableaux déchiquetés et même déchiquetés sur autant de niveaux que l'on veut. Si choices n'est pas nul, alors ParamsTypes peut être null ou vide et la réponse sera l'un des choix définis dans choices, dans le type du choix. Tous les éléments de Choices doivent avoir le même type. (La casse est respectée pour Choices).
		/// Il peut y avoir des surcharges, c'est-à-dire des commandes de même noms mais avec des paramètres différents. Mais il faut faire attention à l'ordre dans lequel on enregistrement les commandes dans le tableau des commandes passées à la console, car celle-ci tente de convertir dans les types demandés, et si elle y réussit, elle ne cherche pas d'autes surcharges. Par exemple, si l'on définit deux commandes de même nom, l'une avec un paramètre String et l'autre avec un paramètre Int32, il faut mettre en premier celle avec le paramètre Int32. En effet, si on mettait d'abord celle avec un paramètre String, la deuxième ne serait jamais appelée car un nombre est parfaitement convertile en String, et donc la commande avec le paramètre String serait systématique appelée, que l'utilisateur entre un nombre ou un texte quelqueconque.
		/// Si ExeDelegate est défini, alors l'appelle de la fonction exécute ce délégué. S'il est null l'événement Execution est lancé. Dans tous les cas, on obtient un objet ExecutionEventArgs.
		/// </summary>
		public class Command
		{
		
			/// <summary>
			/// Nom de la commande.
			/// </summary>
			public string Name { get; set; }
			
			/// <summary>
			/// Nom optionnel de la surcharge.
			/// </summary>
			public string Overload { get; set; }
			
			/// <summary>
			/// Type des paramètres que doit recevoir la commande, et donc que doit entrer l'utilisateur.
			/// </summary>
			public Type[] ParamsTypes { get; set; }
			
			/// <summary>
			/// Délégué à exécuter lorsque la commande est lancée. Si cette propriété n'est pas null, alors l'événement Execution n'est pas lancée, mais cette méthode est exécutée.
			/// </summary>
			public CmdDelegate ExeDelegate { get; set; }
			
			/// <summary>
			/// Tableau d'alias du nom de la commande. L'utilisateur peut taper le nom de la commande, ou bien l'un des alias.
			/// </summary>
			public string[] Aliases { get; set; }
			
			/// <summary>
			/// Liste de choix de type VarParamType pour une commande Choice. Doit être null sinon.
			/// </summary>
			public object[] Choices { get; set; }
			
			/// <summary>
			/// Obtient ou définit si l'utilisateur peut entrer une chaîne vide. La chaîne retournée sera alors String.Empty. False par défaut.
			/// </summary>
			public bool AllowEmptyString { get; set; }
			
			/// <summary>
			/// Obtient ou définit si l'utilisateur peut entrer un tableau vide. Le tableau retourné sera alors un tableau d'élément 0. False par défaut.
			/// </summary>
			public bool AllowEmptyArray { get; set; }
			
			/// <summary>
			/// Donne des noms aux paramètres, pour l'affichage de la syntax. Le dernier élément doit être le nom des paramètres de VarParamType. Si le tableau est vide, seuls les types sont affichés dans la syntaxe. Le nombre d'éléments du tableau n'est pas obligé de correspondre aux nombres d'éléments dans les autres tableaux (bien qu'en toute logique, cela devrait). N'est pas utilisé dans le cas de Choice.
			/// </summary>
			public string[] SyntaxDescription { get; set; }
			
			/// <summary>
			/// Aide sur la commande, affichée en cas d'erreur de syntaxe ou si l'utilisateur tape le nom de la commande suivi d'un point d'interrogation.
			/// </summary>
			public string Help { get; set; }
			
			/// <summary>
			/// Catégories en forme de d'arborescence (chaque élément du tableau étant un niveau) pour la formation des manuels (MakeMan).
			/// </summary>
			public string[] Categories { get; set; }
			
			/// <summary>
			/// Conteneur divers.
			/// </summary>
			public object Tag { get; set; }
			
			/// <summary>
			/// Constructeur d'une commande sans paramètres.
			/// </summary>
			public Command(string name)
			{
				Name = name; Choices = null; ParamsTypes = new Type[0]; Aliases = new string[0];
				SyntaxDescription = new string[0]; AllowEmptyString = false; AllowEmptyArray = false;
				Categories = new string[0];
			}
			
			/// <summary>
			/// Constructeur d'une commande simple avec types de paramètres.
			/// </summary>
			public Command(string name, params Type[] paramsTypes) : this(name)
			{
				ParamsTypes = paramsTypes;
			}
			
			/// <summary>
			/// Constructeur 
			/// </summary>
			public Command(string name, bool allowEmptyArr, params Type[] paramsTypes) : this(name, paramsTypes)
			{
				AllowEmptyArray = allowEmptyArr;
			}
			
			/// <summary>
			/// Constructeur avec définition de la surcharge.
			/// </summary>
			public Command(string name, string overload, params Type[] paramsTypes) : this(name, paramsTypes)
			{
				Overload = overload;
			}
			
			/// <summary>
			/// Constructeur avec définition de la surcharge indiquant si les tableaux peuvent être vides.
			/// </summary>
			public Command(string name, bool allowEmptyArr, string overload, params Type[] paramsTypes) : this(name, overload, paramsTypes)
			{
				AllowEmptyArray = allowEmptyArr;
			}
			
			/// <summary>
			/// Constructeur pour Choices.
			/// </summary>
			public Command(string name, params object[] choices) : this(name)
			{
				Choices = choices;
			}
			
			/// <summary>
			/// Constructeur pour Choices.
			/// </summary>
			public Command(string name, string overload, params object[] choices) : this(name, choices)
			{
				Overload = overload;
			}
			
			/// <summary>
			/// Retourne le nom de la commande.
			/// </summary>
			public override string ToString() { return this.Name; }
			
		}


		// ===========================================================================
	
		
		/// <summary>
		/// Classe d'arguments d'événements.
		/// </summary>
		public class ExecutionEventArgs : EventArgs
		{
		
			/// <summary>
			/// Nom de la commande.
			/// </summary>
			public string Name { get; set; }
			
			/// <summary>
			/// Objet de la commande.
			/// </summary>
			public Command Command { get; set; }
			
			/// <summary>
			/// Contient l'ensemble des paramètres définis par la commande. Les paramètres sont du type demander, il n'y a plus qu'à aller chercher dans le tableau avec un simple cast.
			/// </summary>
			public object[] Parameters { get; set; }
			
			/// <summary>
			/// Au retour de l'événement, si cette propriété est remplit, alors le texte est affiché dans la console.
			/// </summary>
			public string Answer { get; set; }
			
			/// <summary>
			/// Le texte Answer est affiché au retour de l'événement en tant qu'erreur.
			/// </summary>
			public bool AnswerIsError { get; set; }
			
			/// <summary>
			/// Constructeur.
			/// </summary>
			public ExecutionEventArgs(Command cmd, string name, object[] parameters)
			{
				Command = cmd; Name = name; Parameters = parameters;
			}
			
		}
		
		
		/// <summary>
		/// Classe de paramètres pour l'événement OverloadTested.
		/// </summary>
		public class OverloadTestedEventArgs : EventArgs
		{
			public bool Result { get; private set; }
			public Command Command { get; private set; }
			public OverloadTestedEventArgs(bool res, Command cmd)
				{ Result = res; Command = cmd; }
		}



		#endregion CLASSES SUPPLEMENTAIRES





	/// <summary>
	/// Fournit un TextBox qui agit comme une console. Il faut remplir la propriété Commands pour passer des commandes (voir le commentaire de la classe Command). Pour les types reconnus, voir la notice, sous Word.
	/// </summary>
	public class Console : TextBox
	{




		// ---------------------------------------------------------------------------
		// SOUS-CLASSES
		// ---------------------------------------------------------------------------




		#region SOUS-CLASSES
		
		
		
		/// <summary>
		/// Dialogue pour afficher des commandes en saisie semi-automatiquement.
		/// </summary>
		private class DialogBoxAutoComplete : MyFormMessage
		{
		
			private ComboBox _cbo;
			private TextBox _txt;
			private string[] _items;
			private GetAutoCompleteInfosDelegate _getHelp;
			private bool _isParams;
			
			/// <summary>
			/// Délégué qui retourne les informations à afficher sur une commande ou un paramètre.
			/// </summary>
			public delegate string GetAutoCompleteInfosDelegate(string text);
			
			/// <summary>
			/// Obtient ou définit le texte actuellement taper par l'utilisateur.
			/// </summary>
			public string CurrentText {
				get { return _cbo.Text; }
				set
				{
					_cbo.Select();
					for (int i=0; i<_cbo.Items.Count; i++) {
						if (((string)_cbo.Items[i]).StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
							{ _cbo.SelectedIndex = i; break; } }
					_cbo.Text = value;
				}
			}
			
			/// <summary>
			/// Liste des items de la liste qui l'utilisateur peut choisir.
			/// </summary>
			public string[] Items {
				get { return _items; }
				set { _items = value; _cbo.Items.Clear();
					foreach (string s in _items) { _cbo.Items.Add(s); } } }
			
			/// <summary>
			/// Délégué qui retourne les informations à afficher pour une commande ou un paramètre.
			/// </summary>
			public GetAutoCompleteInfosDelegate GetAutoCompleteInfos {
				get { return _getHelp; }
				set { _getHelp = value; } }
			
			/// <summary>
			/// Obtient ou définit si on cherche une commande ou un paramètre.
			/// </summary>
			public bool IsParams {
				get { return _isParams; }
				set { _isParams = value; } }
			
			/// <summary>
			/// Constructeur.
			/// </summary>
			public DialogBoxAutoComplete()
			{
				// Initialisation des variables:
				_items = new string[0];
				// Initialisation de la liste:
				_cbo = new ComboBox();
				_cbo.DropDownStyle = ComboBoxStyle.Simple;
				_cbo.Dock = DockStyle.Fill;
				//_cbo.AutoCompleteMode = AutoCompleteMode.Append;
				//_cbo.AutoCompleteSource = AutoCompleteSource.ListItems;
				_cbo.SelectedIndexChanged += new EventHandler(_cbo_SelectedIndexChanged);
				// Initialisation du TextBox:
				_txt = new TextBox();
				_txt.Dock = DockStyle.Fill;
				_txt.Multiline = true;
				_txt.ScrollBars = ScrollBars.Vertical;
				_txt.ReadOnly = true;
				_cbo.Font = _txt.Font = My.ExdControls.MySettings.ConsoleListFont;
				// Initialisation du splitter:
				SplitContainer split = new SplitContainer();
				split.Dock = DockStyle.Fill;
				this.Load += delegate { split.SplitterDistance = (int)(_tlpBody.Width * 0.5); };
				split.Panel1.Controls.Add(_cbo);
				split.Panel2.Controls.Add(_txt);
				// Initialisation du form:
				SetDialogIcon(DialogBoxIcon.Search);
				SetDialogMessage("Enter or select a text:");
				AddButtonsCollection(DialogBoxButtons.OKCancel, 1, true);
				SetControl(split);
				Activated += delegate { if (Visible) { _cbo.Select(); _cbo.SelectionStart = _cbo.Text.Length; } };
				KeyPreview = true;
				Width = (int)(Screen.PrimaryScreen.WorkingArea.Width / 1.5);
				Height = (int)(Screen.PrimaryScreen.WorkingArea.Height / 1.5);
				KeyDown += new KeyEventHandler(DialogBoxAutoComplete_KeyDown);
			}

			/// <summary>
			/// Quand Enter ou Escape, masque le form (ne fonctionne pas avec Accept et CancelButton quand le ComboBox est actif.
			/// </summary>
			private void DialogBoxAutoComplete_KeyDown(object sender, KeyEventArgs e)
			{
				if (e.KeyCode == Keys.Enter) { _clickResult = DialogBoxClickResult.OK; Hide(); }
				else if (e.KeyCode == Keys.Escape) { _clickResult = DialogBoxClickResult.Cancel; Hide(); }
			}

			/// <summary>
			/// Affiche les informations sur le texte actuellement sélectionné.
			/// </summary>
			private void _cbo_SelectedIndexChanged(object sender, EventArgs e)
			{
				string infos = _getHelp(_cbo.Text);
				if (infos == null) { infos = String.Empty; }
				_txt.Text = infos.Replace("\t", "    ").Replace("\r\n", "\n").Replace("\n", "\r\n");
			}

		}
		



		#endregion SOUS-CLASSES
	




		// ---------------------------------------------------------------------------
		// DECLARATIONS
		// ---------------------------------------------------------------------------




		#region DECLARATIONS


		
		// Historique de toutes les commandes, et des commandes valides:
		protected My.History<string> _allHistory;
		protected My.History<string> _validHistory;
		protected My.History<string> _requestHistory;
		protected My.History<string> _errorHistory;
		// Position dans le TextBox du début de la commande en cours, et de la ligne en cours:
		protected int _cmdStartPos;
		protected int _lineStartPos;
		// Propriété:
		private bool __ReportErrorInConsole;
		// RequestMode:
		protected bool _requestMode;
		private bool _rebuildRequestQuestion;
		// PressAnyKey mode:
		protected bool _pressAnyKeyMode;
		// Mode WriteTemp:
		protected bool _writeTempMode;
		// Tableaux des commandes internes et externes:
		protected Command[] _intCmds;
		protected Command[] _extCmds;
		protected Command[] _allCmds;
		// Tableau d'aide pour les recherche de commande:
		protected string[] _cmdsSearchText;
		protected string[][] _cmdsSearchNames;
		protected string[] _allNames;
		// Valeur pour les MouseMove et MouseClick:
		private int _selectionLen;
		// Dialogues:
		DialogBoxAutoComplete _dlgAutoComplete;
		private string[] _paramsAutoComplete;
		private string[] _defaultAutoComplete;
		private My.DialogBoxConsoleCommandsTree	_dialogTreeCmds;


		/// <summary>
		/// Délégué d'événement.
		/// </summary>
		public delegate void ExecutionEventHandler(object sender, ExecutionEventArgs e);
		
		/// <summary>
		/// Evénement qui se déclenche lorsqu'une commande est lancée par l'utilisateur, sauf si la propriété ExeDelegate est définie pour la commande lancée.
		/// </summary>
		public event ExecutionEventHandler Execution;


		/// <summary>
		/// Délégué d'événement.
		/// </summary>
		public delegate void OverloadTestedEventHandler(object sender, OverloadTestedEventArgs e);

		/// <summary>
		/// Evénement qui se déclenche lorsqu'une surchage d'une commande a été examiné. result indique si la commande sera finalement exécutée (cad si les paramètres correspondent), ou non. Ce délgué peut être utilisé pour faire du ménage, supprimer les ressources utilisées pour le test de la surcharge (notamment si celle-ci ne sera pas exécutée).
		/// </summary>
		public event OverloadTestedEventHandler OverloadTested;
		
		/// <summary>
		/// Evénement qui se déclenche lorsque l'utilisateur entre une commande non reconnue.
		/// </summary>
		public event EventHandler UnknownCommand;



		#endregion DECLARATIONS






		// ---------------------------------------------------------------------------
		// PROPRIETES
		// ---------------------------------------------------------------------------




		#region PROPRIETES




		/// <summary>
		/// Prompteur.
		/// </summary>
		public string Prompt { get; set; }


		/// <summary>
		/// Prompteur pour la réponse (WriteLine).
		/// </summary>
		public string PromptAnswer { get; set; }


		/// <summary>
		/// Prompteur pour la réponse (WriteLine) en cas d'erreur.
		/// </summary>
		public string PromptErrorAnswer { get; set; }


		/// <summary>
		/// Séparateur de paramètres. Virgule par défaut.
		/// </summary>
		public string Separator { get; set; }


		/// <summary>
		/// Si true, les erreurs de My.ErrorHandler sont reportées dans la console, et ne s'affiche pas dans une boîte de dialogue. Une description courte est donnée, et la commande ShowError permet alors de donner le détail de la dernière erreur.
		/// </summary>
		public bool ReportErrorInConsole
		{
			get { return __ReportErrorInConsole; }
			set
			{
				__ReportErrorInConsole = value;
				My.ErrorHandler.ShowErrorMsg = !value;
				if (value) { My.ErrorHandler.ErrorOccurred += new ErrorHandler.ErrorOccurredEventHandler(ErrorHandler_ErrorOccurred); }
				else { My.ErrorHandler.ErrorOccurred -= ErrorHandler_ErrorOccurred; }
			}
		}


		/// <summary>
		/// Liste des commandes acceptés.
		/// </summary>
		public Command[] Commands
		{
			get { return _extCmds; }
			set
			{
				// Affecte la valeur et aggrège les tableaux de commandes internes et externes:
				_extCmds = value;
				_allCmds = _extCmds.Concat(_intCmds).ToArray();
				// Prépare le tableau d'aide pour les recherches de commandes:
				int l = _allCmds.Length;
				_cmdsSearchText = new string[l];
				_cmdsSearchNames = new string[l][];
				for (int i=0; i<l; i++)
				{
					_cmdsSearchText[i] = String.Format("{0}{1}{2}", _allCmds[i].Name,
						(_allCmds[i].Aliases.Length>0 ? String.Format(" {{{0}}}", My.ArrayFunctions.Join(_allCmds[i].Aliases, ",")) : ""),
						(!String.IsNullOrEmpty(_allCmds[i].Help) ? ": " + _allCmds[i].Help : ""));
					_cmdsSearchNames[i] = new string[]{_allCmds[i].Name}.Concat(_allCmds[i].Aliases).ToArray();
				}
				Array.Sort(_cmdsSearchText, _cmdsSearchNames);
				// Créer le tableau de tous les noms de commandes et d'alias:
				_allNames = ArrayFunctions.UnrollArray<string>(_cmdsSearchNames).Distinct().ToArray();
				Array.Sort(_allNames);
			}
		}
		
		
		/// <summary>
		/// Obtient les commandes internes.
		/// </summary>
		public Command[] InternalCommands {
			get { return _intCmds; } }
		
		
		/// <summary>
		/// Obtient ou définit les paramètres possibles à aficher dans la boîte de saisie semi-automatique.
		/// </summary>
		public string[] ParametersAutoComplete {
			get { return _paramsAutoComplete; }
			set {
				_paramsAutoComplete = value.
					Concat(_defaultAutoComplete).ToArray();
				Array.Sort(_paramsAutoComplete); } }
		
		
		/// <summary>
		/// Délégué pour l'analyse à distance des paramètres.
		/// </summary>
		public delegate bool AnalyseParameterDelegate(string cmdName, string param, Type type, out object result);
		
		
		/// <summary>
		/// Délégué appelé avant que la console n'analyse elle-même les paramètres. Cela est utile si on veut reconnaître des types non reconnus par défaut par la console, ou bien pour interpréter autrement les types. Si true est retourné, alors le paramètre est enregistré. Sinon, c'est l'analyse par défaut de la console qui prend le relais, en tentant d'analyser le paramètre.
		/// </summary>
		public AnalyseParameterDelegate AnalyseParameter { get; set; }


		/// <summary>
		/// Délégué pour l'analyse à distance des paramètres de type tableau.
		/// </summary>
		public delegate bool AnalyseArrayParameterDelegate(string cmdName, string param, Type type, bool allowEmpty, out Array result);
		
		
		/// <summary>
		/// Délégué appelé avant que la console n'analyse elle-même les paramètres de type tableau. Ce délégué est appelé pour chaque tableau, et pour chaque sous-tableau (tableau déchiqueté). Si la valeur de retour est false (ou si ce délégué n'est pas défini), alors le délégué AnalyseParameter est appelé pour chaque élément de tableau.
		/// </summary>
		public AnalyseArrayParameterDelegate AnalyseArrayParameter { get; set; }


		/// <summary>
		/// Voir AutoCompleteInfos.
		/// </summary>
		public delegate string AutoCompleteInfosDelegate(string parameterName);
		
		
		/// <summary>
		/// Délégué appelé quand le dialogue de saisie semi-automatique demande des informations sur un paramètre pour afficher les informations dans la zone de texte.
		/// </summary>
		public AutoCompleteInfosDelegate AutoCompleteInfos { get; set; }


		#endregion PROPRIETES






		// ---------------------------------------------------------------------------
		// CONSTRUCTEUR
		// ---------------------------------------------------------------------------




		#region CONSTRUCTEUR



		/// <summary>
		/// Constructeur.
		/// </summary>
		public Console(string welcomeMsg, Form form)
		{
		
			// Initialisation des commandes internes:
			this.InitIntCommands();
			
			// Initialise les variables:
			_rebuildRequestQuestion = false;
			_requestMode = false;
			_writeTempMode = false;
			_pressAnyKeyMode = false;
			_allHistory = new History<string>(75);
			_allHistory.AddMode = History<string>.AddModeType.AddAtEnd;
			_validHistory = new History<string>(75);
			_requestHistory = new History<string>(0);
			_requestHistory.AddMode = History<string>.AddModeType.AddAtEnd;
			_errorHistory = new History<string>(10);
			this.Commands = new Command[0];
			_defaultAutoComplete = _paramsAutoComplete = new string[]
				{"??openfile","??savefile","??font","??color","??dir"};
			
			// Dialogue de saisie semi-automatique:
			_dlgAutoComplete = new DialogBoxAutoComplete();
			_dlgAutoComplete.GetAutoCompleteInfos = GetAutoCompleteInfos;
			
			// Initialisation du TextBox:
			this.Multiline = true;
			this.AcceptsTab = true;
			this.ScrollBars = ScrollBars.Vertical;
			this.Font = My.ExdControls.MySettings.ConsoleDefaultFont;
			this.ForeColor = My.ExdControls.MySettings.ConsoleDefaultColor;
			this.BackColor = My.ExdControls.MySettings.ConsoleDefaultBackColor;
			this.MaxLength = 0; // Limite le texte à 2MM de caractères et quelques (sinon, limité par défaut à 32000 et quelques)
			
			// Evénement:
			this.KeyDown += new KeyEventHandler(Console_KeyDown);
			this.MouseClick += new MouseEventHandler(Console_MouseClick);
			this.MouseMove += new MouseEventHandler(Console_MouseMove);
			this.KeyPress += new KeyPressEventHandler(Console_KeyPress);
			form.FormClosing += new FormClosingEventHandler(Form_FormClosing);
			
			// Par défaut:
			this.ReportErrorInConsole = true;
			this.Prompt = "$> ";
			this.PromptErrorAnswer = "ERROR: ";
			this.PromptAnswer = String.Empty;
			this.Separator = ",";
			this.ClearConsole();
			
			// Affiche un msg de bienvenue:
			if (!String.IsNullOrEmpty(welcomeMsg)) { WriteLine(welcomeMsg); }
			else { WriteLine(String.Format("Welcome on {0} - {1}!\n", App.Title, App.GetEntryAssemblyVersion())); }
			
		}

		/// <summary>
		/// Constructeur.
		/// </summary>
		public Console(Form form) : this(null, form)
		 { ; }

		#endregion CONSTRUCTEUR






		// ---------------------------------------------------------------------------
		// GESTION DES DEPLACEMENTS, SELECTION ET ECRITURE DANS LE TEXTBOX
		// ---------------------------------------------------------------------------




		#region GESTION DES DEPLACEMENTS, SELECTION ET ECRITURE DANS LE TEXTBOX



		/// <summary>
		/// Supprime l'insertion de la touche Entrée, quoiqu'il arrive. En effet, malgré l'activation de SuppressKeyPress dans KeyDown, il arrive que la touche Entrée passe quand même et arrive dans le contrôle.
		/// </summary>
		private void Console_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13) { e.Handled = true; }
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Gestion des touches et des raccourcis clavier. Ne permet pas de modifier le texte précédent la commande en cours.
		/// </summary>
		private void Console_KeyDown(object sender, KeyEventArgs e)
		{
		
			// Si PressAnyKeyMode, on indique qu'une touche a été enfoncée, et annule le traitement des autres:
			if (_pressAnyKeyMode) { e.Handled =  e.SuppressKeyPress = true; _pressAnyKeyMode = false; return; }

			// Si mode WriteTemp, supprime toutes les gestions des touches et sort:
			if (_writeTempMode) { e.Handled =  e.SuppressKeyPress = true; return; }
		
			// Sort si seule une touche de contrôle a été enfoncée, ou si ReadOnly est actif, ou si Alt+F4 (Menu = Alt):
			if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu
				|| (e.Alt && e.KeyCode == Keys.F4) || (this.ReadOnly && e.KeyCode != Keys.F5)) { return; }
		
			// Par défaut, supprime la gestion de l'événement. Handled permet ainsi d'éviter un déplacement dans le contrôle,
			// mais ne supprime pas KeyPress. Or celui-ci ne se déclenche que si une touche de caractère est activé, donc on
			// peut, en usant de Handled, laissé passer les caractères sur la dernière ligne, mais supprimer les déplacements
			// dans le TextBox.
			e.Handled = true;
			
			// Si F1, affiche la liste des commandes:
			if (e.KeyCode == Keys.F1 && e.Modifiers != Keys.Shift) {
				if (_dialogTreeCmds == null) {
					_dialogTreeCmds = new My.DialogBoxConsoleCommandsTree(this);
					_dialogTreeCmds.ListFont = My.ExdControls.MySettings.ConsoleListFont; }
				if (_dialogTreeCmds.ShowDialog() == My.DialogBoxClickResult.OK)
					{ Write(false, _dialogTreeCmds.SelectedCommand); }
				return; }
			// Si Ctrl+Enter, affiche le dialogue de saisie semi-automatique:
			else if (e.KeyCode == Keys.Enter && e.Shift) {
				ShowAutoCompleteDialog(); }
			// Si F10, efface la commande en cours:
			else if (e.KeyCode == Keys.F10) {
				this.ResetCmd(); this.GoToEnd(); }
			// Si F4, affiche et exécute dernière commande, si elle existe:
			else if (e.KeyCode == Keys.F4 && !_requestMode) {
				this.ResetCmd();
				this.ExecuteCommand(_allHistory.Back(), true); }
			// Si F5, entre ou sort dans le mode ReadOnly:
			else if (e.KeyCode == Keys.F5) {
				this.ReadOnly = !this.ReadOnly;
				if (!this.ReadOnly) { this.GoToEnd(); this.ScrollToCaret(); } }
			// Si Ctrl+Enter, écrit un caractère de saut de ligne:
			else if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.Control) {
				Write(false, "\r\n"); e.SuppressKeyPress = true; }
			// Si Enter, exécute la commande courante, ou la réponse à la request, et supprime la touche:
			else if (e.KeyCode == Keys.Enter) {
				e.SuppressKeyPress = true;
				if (_requestMode) { _requestMode = false; }
				else { this.ExecuteCommand(this.GetCommandLine()); } }
			// Si Back, n'efface le caractère que si on n'est pas au début de la ligne:
			else if (e.KeyCode == Keys.Back) {
				e.SuppressKeyPress = (this.SelectionStart <= _cmdStartPos && this.SelectionLength == 0); }
			// Même principe pour Suppr:
			else if (e.KeyCode == Keys.Delete) {
				e.SuppressKeyPress = (this.SelectionStart < _cmdStartPos && this.SelectionLength == 0); }
			// Si Up ou Down, monte ou descend dans l'historique:
			else if (e.KeyCode == Keys.Up)
			{
				if (_requestMode) {
					string s = _requestHistory.Back();
					if (s != null) { this.ResetCmd(); this.Write(false, s); } }
				else {
					string s = _allHistory.Back();
					if (s != null) { this.ResetCmd(); this.Write(false, s); } }
			}
			else if (e.KeyCode == Keys.Down)
			{
				if (_requestMode) {
					string s = _requestHistory.Forward();
					if (s != null) { this.ResetCmd(); this.Write(false, s); } }
				else {
					string s = _allHistory.Forward();
					if (s != null) { this.ResetCmd(); this.Write(false, s); } }
			}
			// Si Left ou Right, avance ou recule dans la limite de la commande:
			else if (e.KeyCode == Keys.Left) {
				e.Handled = (this.SelectionStart <= _cmdStartPos); }
			else if (e.KeyCode == Keys.Right) {
				e.Handled = false; }
			// Si Home ou End, avance ou recule dans la limite de la commande:
			else if (e.KeyCode == Keys.Home) {
				this.GoToStartCommand(); }
			else if (e.KeyCode == Keys.End) {
				this.GoToEnd(); }
			// Si F6 ou F7, affiche des guillemets ouvrants ou fermants, simples ou doubles:
			else if (e.KeyCode == Keys.F6) {
				this.SelectedText = (e.Modifiers==Keys.Shift ? "‘" : "“"); }
			else if (e.KeyCode == Keys.F7) {
				this.SelectedText = (e.Modifiers==Keys.Shift ? "’" : "”"); }
		
		}


		// ---------------------------------------------------------------------------

	
		/// <summary>
		/// Si l'user est en train de sélectionner un texte, veille à ce qu'il ne le fasse pas au-delà de la commande courante, si ReadOnly n'est pas actif.
		/// </summary>
		private void Console_MouseMove(object sender, MouseEventArgs e)
		{
			if (!this.ReadOnly && this.SelectionStart < _cmdStartPos) { this.SelectionStart = _cmdStartPos;
				this.SelectionLength = _selectionLen; }
			else { _selectionLen = this.SelectionLength; }
		}


		// ---------------------------------------------------------------------------

	
		/// <summary>
		/// Annule l'événement du click si le mode ReadOnly n'est pas actif, et si le curseur n'est pas dans la commande courante.
		/// </summary>
		private void Console_MouseClick(object sender, MouseEventArgs e)
		{
			_selectionLen = 0;
			if (this.SelectionStart < _cmdStartPos && !this.ReadOnly) { this.GoToEnd(); }
		}


		// ---------------------------------------------------------------------------

	
		/// <summary>
		/// Supprime le texte de la commande en cours.
		/// </summary>
		private void ResetCmd()
		{
			if (_cmdStartPos >= this.Text.Length) { return; }
			this.Text = this.Text.Remove(_cmdStartPos);
			this.GoToEnd();
			_cmdStartPos = this.Text.Length;
		}

		/// <summary>
		/// Supprime le texte de la ligne en cours (et pas seulement de la commande en cours).
		/// </summary>
		private void ResetLine()
		{
			if (_lineStartPos >= this.Text.Length) { return; }
			this.Text = this.Text.Remove(_lineStartPos);
			this.GoToEnd();
			_cmdStartPos = _lineStartPos = this.Text.Length;
		}

		/// <summary>
		/// Obtient le texte de la ligne de commande.
		/// </summary>
		private string GetCommandLine()
		{
			return this.Text.Substring(_cmdStartPos);
		}

		/// <summary>
		/// Déplace le curseur au début de la commande courante:
		/// </summary>
		private void GoToStartCommand()
		{
			this.SelectionStart = _cmdStartPos;
			this.SelectionLength = 0;
		}

		/// <summary>
		/// Déplace le curseur à la fin du texte.
		/// </summary>
		private void GoToEnd()
		{
			this.SelectionStart = this.Text.Length;
			this.SelectionLength = 0;
			this.ScrollToCaret();
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Efface la console, mais pas l'historique.
		/// </summary>
		protected void ClearConsole()
		{
			this.Text = this.Prompt;
			_lineStartPos = 0;
			_cmdStartPos = this.Text.Length;
			this.GoToEnd();
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Ecrit un message sur la ligne courante. Si isError est vrai, alors écrit d'abord le prompt d'erreur. Si lockText est vrai, alors le texte est bloqué, c'est-à-dire que l'user ne peut pas revenir dessus pour l'effacer. De plus, si lockText est vrai, le texte commence au début de la ligne courante, après l'avoir effacée (ce qui se résume généralement à effacé le prompt), alors que si lockText est faux, le texte est écrit à la suite de la ligne courante.
		/// </summary>
		protected void Write(bool lockText, string msg, bool isError)
		{
			msg = (msg==null ? String.Empty : msg).Replace("\r\n", "\n").Replace("\n", "\r\n");
			if (lockText) { this.ResetLine(); }
			this.AppendText((isError ? this.PromptErrorAnswer : this.PromptAnswer) + msg);
			if (lockText) { _cmdStartPos = this.Text.Length; }
			this.GoToEnd();
		}
		
		/// <summary>
		/// Ecrit un message sur la ligne courante (après l'avoir effacé), bloqué.
		/// </summary>
		public void Write(string msg, bool isError)
			{ this.Write(true, msg, isError); }

		/// <summary>
		/// Ecrit un message sur la ligne courante (après l'avoir effacé), bloqué et sans erreur.
		/// </summary>
		public void Write(string msg)
			{ this.Write(true, msg, false); }

		/// <summary>
		/// Ecrit un message sur la ligne courante (après l'avoir effacé), sans erreur.
		/// </summary>
		public void Write(bool lockText, string msg)
			{ this.Write(lockText, msg, false); }


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Ecrit msg sur une nouvelle ligne. Si isError est true, affiche au début de la ligne le prompter d'erreur.
		/// </summary>
		public void WriteLine(string msg, bool isError)
		{
			if (this.GetCommandLine() != String.Empty) { this.WriteEndLine(); }
			this.Write(true, (msg==null ? String.Empty : msg), isError);
			this.WriteEndLine();
		}

		/// <summary>
		/// Ecrit msg en insérant à la fin un saut de ligne, sans erreur.
		/// </summary>
		public void WriteLine(string msg)
		 { this.WriteLine(msg, false); }
		
		/// <summary>
		/// Ecrit une ligne vide.
		/// </summary>
		public void WriteLine()
			{ this.WriteLine(null); }


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Termine la ligne courante, si besoin est, et passe à la suivante, en affichant le prompt. Ne fait rien si on est déjà au début d'une nouvelle ligne.
		/// </summary>
		public void WriteEndLine()
		{
			if (String.IsNullOrEmpty(Text)) { return; }
			if (Text.Length >= Prompt.Length && Text.Substring(Text.Length - Prompt.Length) == Prompt) { return; }
			this.GoToEnd();
			this.AppendText("\r\n" + this.Prompt);
			this.GoToEnd();
			_cmdStartPos = this.Text.Length;
			_lineStartPos = _cmdStartPos - this.Prompt.Length;
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Ecrit un message sur une nouvelle ligne qui est destiné ea être effacée, et remplacée par un autre par un futur appel de WriteLineTemp. Pendant ce temps, l'utilisateur ne peut pas modifier le texte, ni même se déplacer dans le contrôle. Pour terminer l'opération et revenir au mode normal, il faut appeler WriteLineTempStop.
		/// </summary>
		public void WriteLineTemp(string msg)
		{
			this.ResetLine();
			_writeTempMode = true;
			this.Write(false, msg, false);
		}
		
		/// <summary>
		/// Termine le mode WriteLineTemp et repasse en mode normal.
		/// </summary>
		public void WriteLineTempStop()
		{
			_writeTempMode = false;
			this.ResetLine();
			this.Write(this.Prompt);
		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Attend que l'utilisateur appuie sur une touche. On peut mettre un message personnalisé.
		/// </summary>
		public void PressAnyKeyMode(string message)
		{
			this.WriteEndLine();
			this.Write(true, message);
			_pressAnyKeyMode = true;
			// Sleep permet de ne pas faire tourner le processeur à 100% (un arg de 0 le fait tourner à plein régime,
			// une valeur supérieur à 50 ralentit la mise à jour du TextBox):
			while (_pressAnyKeyMode) { Thread.Sleep(1); Application.DoEvents(); }
			WriteEndLine();
		}
		
		/// <summary>
		/// Attend que l'utilisateur appuie sur une touche. Message par défaut.
		/// </summary>
		public void PressAnyKeyMode()
			{ PressAnyKeyMode("Press any key..."); }
	
	
		
		#endregion GESTION DES DEPLACEMENTS, SELECTION ET ECRITURE DANS LE TEXTBOX






		// ---------------------------------------------------------------------------
		// METHODES D'ANALYSE DE LA LIGNE DE COMMANDES ET D'EXCUTION
		// ---------------------------------------------------------------------------




		#region METHODES D'ANALYSE DE LA LIGNE DE COMMANDES ET D'EXCUTION




		/// <summary>
		/// Exécute la commande passée, en l'inscrivant, ou non, dans la console.
		/// </summary>
		public void ExecuteCommand(string cmdLine, bool writeInConsole)
		{
			cmdLine = cmdLine.Trim();
			if (String.IsNullOrEmpty(cmdLine)) { return; }
			if (writeInConsole) {
				if (this.GetCommandLine() != String.Empty) { this.WriteEndLine(); }
				this.Write(false, cmdLine); }
			this.ExecuteCommand(cmdLine);
		}


		/// <summary>
		/// Exécute la commande demandée. En fait, vérifie si les arguments sont bons grâce à la propriété Commands, sinon ne fait rien. Déclenche l'événement Execution, en retournant le nom de la commande et le tableau d'argument dans dans les arguments d'événements.
		/// </summary>
		protected void ExecuteCommand(string cmdLine)
		{
		
			// Sort si pas de cmd, et enregistre dans l'historique:
			cmdLine = cmdLine.Replace("\t", "").Replace("\r\n", "").Trim();
			if (String.IsNullOrEmpty(cmdLine)) { return; }
			_allHistory.AddLine(cmdLine);
						
			// Récupère le nom de la commande:
			string cmdName = String.Empty, paramsStr = String.Empty;
			string[] split = cmdLine.Split(new string[]{" "}, 2, StringSplitOptions.RemoveEmptyEntries);
			if (split.Length >= 1) { cmdName = split[0]; }
			if (split.Length >=2) { paramsStr = split[1].Trim(); }
			
			// Découpe les paramètres, et si erreur, remplace par ?
			string[] splitParams = My.FieldsParser.ParseText(paramsStr, Separator, true);
			if (splitParams == null) { paramsStr = "?"; }
			
			// Cherche la bonne surcharge de la commande dans le tableau...
			StringBuilder syntax = new StringBuilder(); object[] parameters;
			Command[] cmds = _allCmds.Where(delegate(Command cmd) { return (cmd.Name.Equals(cmdName, StringComparison.InvariantCultureIgnoreCase)
				|| cmd.Aliases.Contains(cmdName, My.ArrayFunctions.StringIgnoreCaseComparer)); }).ToArray();
			foreach (Command cmd in cmds)
			{
				// Si paramsStr == "?", affiche l'aide pour cette surcharge:
				if (paramsStr.Equals("?"))
				{
					WriteLine(GetSyntax(cmd));
				}
				// Sinon, regarde si les arguments correspondent:
				else
				{
					// Si ça marche, appelle délégué de fin de surcharge, exécute la commande et sort:
					if (this.AnalyseParamsString(cmd.ParamsTypes, splitParams, cmd.Choices, cmd.Name, cmd.AllowEmptyArray, cmd.AllowEmptyString, out parameters))
					{
						if (OverloadTested != null) { OverloadTested(this, new OverloadTestedEventArgs(true, cmd)); }
						_validHistory.AddLine(cmdLine); // Retient la commande valide passée
						WriteEndLine();
						OnExecution(cmd, parameters);
						return;
					}
					// Sinon, appelle délégué de fin de surcharge, retient syntaxe pour affichage et continue pour
					// une autre surcharge:
					else
					{
						if (OverloadTested != null) { OverloadTested(this, new OverloadTestedEventArgs(false, cmd)); }
						syntax.Append((syntax.Length == 0 ? "   " : "\n   ") + GetSyntax(cmd));
					}
				}
			}
			
			// Si on est là, c'est soit que la commande n'a pas été trouvée:
			if (cmds.Length == 0) {
				this.WriteLine("Unknown command.", true);
				if (UnknownCommand != null) { UnknownCommand(this, new EventArgs()); }
				return; }
			// soit que les paramètres ne correspondaient pas:
			else if (syntax.Length != 0) {
				this.WriteLine("Not recognized syntax. "
				+ "Use one of the following syntax:\n" + syntax, true); }
			
		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Analyse les arguments splitParams. Retourne true si les arguments correspondent à ce qui est demandé, false dans le cas contraire.
		/// </summary>
		protected bool AnalyseParamsString(Type[] paramsTypes, string[] splitParams, object[] choices, string cmdName, bool allowEmptyArr, bool allowEmptyStr, out object[] parameters)
		{
		
			// Si choice, on reconstruit paramsType, car il peut être vide ou null:
			if (choices != null) { paramsTypes = new Type[]{choices[0].GetType()}; }
		
			// Variables:
			int lenTypes = paramsTypes.Length, lenSplit = splitParams.Length; parameters = new object[lenTypes];
			
			// Si pas de paramètres, on sort de suite:
			if (lenTypes == 0 && lenSplit > 0) { return false; }
			else if (lenTypes == 0 && lenSplit == 0) { return true; }
			
			// Si choix, un seul paramètre:
			if (choices != null && splitParams.Length != 1)
				{ return false; }
			// Si pas choix, et si le dernier type n'est pas un tableau, le nb de paramètres doit correspondre:
			else if (choices == null && !paramsTypes[lenTypes-1].IsArray)
				{ if (lenSplit != lenTypes) { return false; } }
			// Si pas choix, et si le dernier type est un tableau, le nb de paramètres doit être inférieur d'un
			// (si allowEmptyArr est vrai) ou doit correspondre (si faux):
			else if (choices == null && paramsTypes[lenTypes-1].IsArray)
				{ if ((lenSplit < lenTypes - 1 && allowEmptyArr) || (lenSplit < lenTypes && !allowEmptyArr)) { return false; } }
			
			// Tente de convertir chaque argument dans le type demandé:
			for (int i=0; i<lenTypes; i++)
			{
				// Si le type est un tableau, s'appelle récursivement:
				if (paramsTypes[i].IsArray)
				{
					// Si un délgué d'analyse est défini, l'appelle. S'il réussit, on enregistre la valeur et on continue pour les paramètres suivants.
					Array objFromDelegate;
					if (AnalyseArrayParameter != null && AnalyseArrayParameter(cmdName, splitParams[i], paramsTypes[i], allowEmptyArr,
								out objFromDelegate))
					{
						parameters[i] = objFromDelegate;
						continue;
					}
					// Sinon, on analyse chaque élément du tableau:
					string[] subSplit; Type elementType = My.ArrayFunctions.GetElementType(paramsTypes[i]);
					// Si dernier type et si plus d'un paramètre, passe les paramètres restants.
					// S'il n'y en a pas, c'est que allowEmptyArr est vrai (sinon, on serait déjà sorti précédemment) et crée un tableau vide.
					if (i == lenTypes-1 && lenSplit < lenTypes)
					{
						parameters[i] = Array.CreateInstance(elementType, 0);
					}
					else
					{
						if (i == lenTypes-1 && lenSplit > lenTypes) {
							subSplit = new string[lenSplit-i];
							Array.Copy(splitParams, i, subSplit, 0, lenSplit - i); }
						else {
							if ((subSplit = My.FieldsParser.ParseText(splitParams[i], Separator, true)) == null) { return false; } }
						// Construit un tableau avec les types:
						Type[] subTypes = new Type[subSplit.Length];
						for (int j=0; j<subSplit.Length; j++) { subTypes[j] = elementType; }
						object[] temp;
						if (!AnalyseParamsString(subTypes, subSplit, null, cmdName, allowEmptyArr, allowEmptyStr, out temp)) { return false; }
						// Si tableau vide, et qu'on ne doit pas, sort:
						if (temp.Length == 0 && !allowEmptyArr) { return false; }
						// Convertit le tableau object[] pour qu'il coorponde à un tableau du type demandé:
						parameters[i] = Array.CreateInstance(elementType, temp.Length);
						for (int j=0; j<temp.Length; j++) { ((Array)parameters[i]).SetValue(temp[j], j); }
					}
				}
				// Si pas de tableau, analyse le paramètre:
				else
				{
					if (!AnalyseOneParam(splitParams[i], paramsTypes[i], cmdName, allowEmptyStr, out parameters[i])) { return false; }
				}
			}
			
			// Si la commande est de type Choice, il faut encore vérifier que la valeur est bien un choix possible:
			if (choices != null && !choices.Contains(parameters[0])) {return false; }
			
			// Valeur de retour:
			return true;

		}


		// ---------------------------------------------------------------------------
		
		
		/// <summary>
		/// Analyse un paramètre. De façon plus général, tente de convertir param en un objet de type type, retourné en paramter. Retourne true si la conversion a eu lieu, false dans le cas contraire. cmdName est le nom de la commande, utile seulement pour l'exécution de AnalyseParameter (le délégué), à l'extérieur.
		/// </summary>
		public bool AnalyseOneParam(string param, Type type, string cmdName, bool allowEmptyStr, out object parameter)
		{
			
			// Variables:
			parameter = null; object objFromDelegate; MethodInfo methInfo;
			// Vérifie s'il faut afficher une boîte de dialogue:
			param = ShowSelectDialogs(param);
			// Délégué d'analyse:
			if (AnalyseParameter != null && AnalyseParameter(cmdName, param, type, out objFromDelegate))
			{
				parameter = objFromDelegate;
			}
			// Sort si vide, sauf si chaîne:
			else if (String.IsNullOrEmpty(param) && type != typeof(String)) { return false; }
			// String : Copie simplement, si pas null ou Empty:
			else if (type == typeof(String))
			{
				if (String.IsNullOrEmpty(param) && !allowEmptyStr) { return false; }
				else if (String.IsNullOrEmpty(param)) { parameter = String.Empty; }
				else { parameter = param; }
			}
			// Boolean : Tente une conversion (true, 1, yes, y, etc.):
			else if (type == typeof(Boolean))
			{
				string s = param.ToLower();
				if (s.Equals("true") || s.Equals("1") || s.Equals("y") || s.Equals("yes")) { parameter = true; }
				else if (s.Equals("false") || s.Equals("0") || s.Equals("n") || s.Equals("no")) { parameter = false; }
				else { return false; }
			}
			// Si couleur, cherche à parser le texte:
			else if (type == typeof(Color))
			{
				Color test;
				if (!My.GeneralParser.ColorParser(param, ":", out test)) { return false; }
				parameter = test;
			}
			// Si le type est Enum, on utilise la méthode, on parcourt les valeurs,
			// jusqu'à trouver un élément d'énumération qui commence par les lettres indiquées:
			else if (type.IsEnum)
			{
				Array arr = Enum.GetValues(type); bool found = false;
				foreach (object o in arr)
				{
					if (Enum.GetName(type, o).StartsWith(param, StringComparison.CurrentCultureIgnoreCase))
						{ parameter = o; found = true; break; }
				}
				if (!found) { return false; }
			}
			// Si le type a une fonction Parse, on l'utilise:
			else if ((methInfo = type.GetMethod("Parse", new Type[]{typeof(String)})) != null)
			{
				try { parameter = methInfo.Invoke(null, new object[]{param}); }
				catch (TargetInvocationException exc) { My.ErrorHandler.ShowError(exc.InnerException); return false; }
				catch (Exception exc) { My.ErrorHandler.ShowError(exc); return false; }
			}
			// Font : Valide si le nom de la police est correct. La taille est 10 par défaut:
			else if (type == typeof(Font))
			{
				Font test;
				if (!My.GeneralParser.FontParser(param, ":", 10, out test)) { return false; }
				parameter = test;
			}					
			// Sinon, il le type demandé n'existe pas:
			else
			{
				return false;
			}
			
			// Retour:
			return true;
			
		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Retourne la syntaxe de la commande, avec une description des énumérations et des choix, ainsi que l'aide. forMan indique un formattage pour un manuel.
		/// </summary>
		public string GetSyntax(Command cmd, bool forMan)
		{

			// Titre:
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("• {0}{1}{2}: ", (forMan ? "" : "SYNTAX for "), cmd.Name,
				(cmd.Aliases.Length>0 ? String.Format(" {{{0}}}", My.ArrayFunctions.Join(cmd.Aliases, ",")) : ""));

			// Si commande de choix, énumère les possibilités et sort:
			if (cmd.Choices != null)
				{ sb.AppendFormat("arg0({0}){{{1}}}", My.ArrayFunctions.GetElementType(cmd.Choices.GetType()).Name,
					My.ArrayFunctions.Join(cmd.Choices, ",")); return sb.ToString(); }

			// Sinon, inscrit les paramètres:
			if (cmd.ParamsTypes.Length == 0) { sb.Append(forMan ? "(No param)" : "(No parameter needed)"); }
			int l = cmd.ParamsTypes.Length;
			for (int i=0; i<l; i++)
			{
				sb.Append(i<cmd.SyntaxDescription.Length ? cmd.SyntaxDescription[i] : "arg" + i.ToString());
				sb.AppendFormat("({0})", cmd.ParamsTypes[i].Name);
				if (cmd.ParamsTypes[i].IsArray)
					{ sb.AppendFormat("[{0}]", (cmd.AllowEmptyArray ? "Empty" : "Not Empty")); }
				sb.Append(i<l-1 ? ", " : "");
			}

			// Note les valeurs des énumérations et polices:
			if (!forMan)
			{
				Type[] types = cmd.ParamsTypes.Distinct().ToArray();
				l = types.Length;
				for (int i=0; i<l; i++) { if (types[i].IsArray) { types[i] = My.ArrayFunctions.GetElementType(types[i]); } }
				string[] enums = new string[0]; int c = 0;
				foreach (Type t in types)
				{
					if (t.IsEnum) {
						Array.Resize(ref enums, c + 1);
						enums[c++] = String.Format("- VALUES for {0}: {{{1}}}", t.Name, My.ArrayFunctions.Join(Enum.GetNames(t), ",")); }
					else if (t == typeof(Font)) {
						Array.Resize(ref enums, c + 1);
						enums[c++] = String.Format("- VALUES for {0}: {{{1}}}", t.Name, My.ArrayFunctions.Join(My.Functions.GetFontNames(), ",")); }
				}
				foreach (string s in enums) { sb.Append("\n" + s); }
			}
			
			// Rajoute l'aide, ou formattage pour le manuel:
			if (!String.IsNullOrEmpty(cmd.Help) && !forMan) { sb.Append("\nHelp: " + cmd.Help); }
			if (forMan) { sb.Replace(": ", "\n\t").Replace(", ", ",\n\t"); }
			return sb.ToString();
			
		}

		/// <summary>
		/// Retourne la syntaxe de la commande, avec une description des énumérations et des choix, ainsi que l'aide.
		/// </summary>
		public string GetSyntax(Command cmd)
			{ return GetSyntax(cmd, false); }


		// ---------------------------------------------------------------------------
	


		/// <summary>
		/// Pose une question et retourne les valeurs obtenues. def est le texte par défaut.
		/// </summary>
		public object[] Request(string question, string def, object[] choices, bool allowEmptyArr, bool allowEmptyStr, params Type[] paramsTypes)
		{
			// Construit la commande (pour GetSyntax, au besoin):
			Command cmd = null; object[] parameters;
			Func<Command> getCmd = delegate()
				{
					if (cmd != null) { return cmd; }
					if (choices == null) { return (cmd = new Command("RequestMode", paramsTypes)); }
					else { return cmd = new Command("RequestMode", choices); }
				};
			// Nouvelle ligne:
			this.WriteEndLine();
			// Si on ne met pas un DoEvents ici, le DoEvents de la boucle d'attente affiche le caractère Enter (\r)
			// provoquer par la validation de la commande, même s'il a été supprimé par SupressKeyPress dans KeyDown.
			// Un autre moyen aurait été de bloquer les Enter en RequestMode dans un gestionnaire KeyPress, mais 
			// c'est plus court avec ce DoEvents ici:
			//Application.DoEvents(); // Finalement, je suis passé par KeyPress...
			_requestHistory.AddLine(def);
			// Boucle jusqu'à ce qu'il y ait une réponse correcte:
			while (true)
			{
				// Ecrit la question, et la réponse par défaut:
				this.Write(true, question + "? ");
				if (!String.IsNullOrEmpty(def)) { this.Write(false, _requestHistory.Back()); }
				// Met à jour la variable isQuestion, et attend qu'elle passe à nouveau à false:
				_requestMode = true;
				// Sleep permet de ne pas faire tourner le processeur à 100% (un arg de 0 le fait tourner à plein régime,
				// une valeur supérieur à 50 ralentit la mise à jour du TextBox):
				while (_requestMode && !_rebuildRequestQuestion) { Thread.Sleep(1); Application.DoEvents(); }
				_rebuildRequestQuestion = false;
				// Obtient la ligne et ajoute une nouvelle ligne dans l'historique:
				string paramStr = this.GetCommandLine();
				_requestHistory.AddLine(paramStr);
				// Récupère les réponses, et les analyses:
				string[] splitParams = My.FieldsParser.ParseText(paramStr, Separator, true);
				if (splitParams == null
					|| !AnalyseParamsString(paramsTypes, splitParams, choices, "RequestMode", allowEmptyArr, allowEmptyStr, out parameters))
						{ this.WriteLine(GetSyntax(getCmd()), true); }
				else { this.WriteEndLine(); break; }
			}
			_requestHistory.Clear();
			return parameters;
		}
	
		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public object[] Request(string question, string def, params Type[] paramsTypes)
			{ return Request(question, def, null, false, false, paramsTypes); }

		/// <summary>
		/// Pose une question et retourne une seule réponse de type T (l'user doit fournir un seul paramètre de type T).
		/// </summary>
		public T Request<T>(string question, T def)
			{ return (T)Request(question, def.ToString(), null, false, false, typeof(T))[0]; }
		
		/// <summary>
		/// Pose une question et retourne une seule réponse de type T (l'user doit fournir un seul paramètre de type T).
		/// </summary>
		public T Request<T>(string question)
			{ return (T)Request(question, "", null, false, false, typeof(T))[0]; }
		
		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public T Request<T>(string question, string def, params T[] choices)
			{ return (T)Request(question, def, Array.ConvertAll<T,object>(choices,
				delegate(T o) { return (object)o; }).ToArray(), false, false, null)[0]; }

		/// <summary>
		/// Voir surcharge.
		/// </summary>
		public T Request<T>(string question, T def, params T[] choices)
			{ return (T)Request(question, def.ToString(), Array.ConvertAll<T,object>(choices,
				delegate(T o) { return (object)o; }).ToArray(), false, false)[0]; }



		#endregion METHODES D'ANALYSE DE LA LIGNE DE COMMANDES ET D'EXCUTION





		// ---------------------------------------------------------------------------
		// AUTRES METHODES
		// ---------------------------------------------------------------------------




		#region AUTRES METHODES


		
		/// <summary>
		/// Retourne la syntaxe d'une commande pour le dialogue de saisie semi-automatique.
		/// </summary>
		private string GetAutoCompleteInfos(string text)
		{
			string result = "";
			// Si paramètres, appelle le délégué:
			if (_dlgAutoComplete.IsParams && AutoCompleteInfos != null)
				{ return AutoCompleteInfos(text); }
			// Si commande, on la cherche et retourne le résultat:
			else if (!_dlgAutoComplete.IsParams)
			{
				foreach (Command cmd in _allCmds)
				{
					if (cmd.Name.Equals(text, StringComparison.CurrentCultureIgnoreCase)
						|| cmd.Aliases.Contains(text, My.ArrayFunctions.StringIgnoreCaseComparer))
						{ result += GetSyntax(cmd, true).Replace("\n", "\r\n") + "\r\n"; }
				}
			}
			return result;
		}
		
		/// <summary>
		/// Affiche le dialogue de saisie semi-automatique.
		/// </summary>
		private void ShowAutoCompleteDialog()
		{
			// Si avant espace, c'est qu'on est dans une commande, et on affiche en conséquence:
			int curPos = SelectionStart;
			string line = GetCommandLine().Substring(0, curPos - _cmdStartPos);
			if (line.IndexOf(" ") < 0)
			{
				_dlgAutoComplete.Items = _allNames;
				_dlgAutoComplete.CurrentText = line;
				_dlgAutoComplete.IsParams = false;
				if (_dlgAutoComplete.ShowDialog() == DialogBoxClickResult.OK) {
					SelectionStart = _cmdStartPos;
					int indexOfSpace = Text.IndexOf(" ",_cmdStartPos);
					SelectionLength = (indexOfSpace<0 ? Text.Length : indexOfSpace) - _cmdStartPos;
					SelectedText = _dlgAutoComplete.CurrentText; }
			}
			// Sinon, c'est qu'on est dans un paramètre:
			else
			{
				// Cherche le texte en remontant jusqu'au premier caractère qui n'est pas une virgule,
				// un guillemet ou un espace, exception faite des ?:
				int index = 0;
				for (int i=line.Length-1; i>=0; i--) {
					if (line.Substring(i, 1) == "?") { continue; }
					if (!Char.IsLetter(line, i) || line.Substring(i, 1) == " ") { index = i+1; break; } }
				// Affiche le dialogue:
				_dlgAutoComplete.Items = _paramsAutoComplete;
				_dlgAutoComplete.CurrentText = line.Substring(index, line.Length - index);
				_dlgAutoComplete.IsParams = true;
				if (_dlgAutoComplete.ShowDialog() == DialogBoxClickResult.OK) {
					SelectionStart = _cmdStartPos + index;
					SelectionLength = curPos - SelectionStart;
					SelectedText = _dlgAutoComplete.CurrentText; }
			}
		}


		// ---------------------------------------------------------------------------
	
		
		/// <summary>
		/// Si param est du type ??qqch, affiche la boîte de dialogue correspondante (eg. ouverture d'un fichier). Retourne la réponse, ou param sinon.
		/// </summary>
		protected string ShowSelectDialogs(string param)
		{
			string temp;
			switch (param)
			{
				case "??openfile":
					if ((temp = My.FilesAndStreams.MyOpenFileDialog()) == null) { return param; } return temp;
				case "??savefile":
					if ((temp = My.FilesAndStreams.MySaveFileDialog()) == null) { return param; } return temp;
				case "??dir":
					if ((temp = My.FilesAndStreams.MyFolderDialog()) == null) { return param; } return temp;
				case "??font":
					FontDialog ftDialog = new FontDialog();
					ftDialog.FontMustExist = true;
					ftDialog.Font = this.Font;
					if (ftDialog.ShowDialog() == DialogResult.Cancel) { return param; }
					return My.GeneralParser.GetFontDescription(ftDialog.Font, ":"); 
				case "??color":
					ColorDialog colDialog = new ColorDialog();
					colDialog.Color = this.ForeColor;
					colDialog.FullOpen = true;
					if (colDialog.ShowDialog() == DialogResult.Cancel) { return param; }
					return ColorFunctions.GetColorDescription(colDialog.Color, ":");
				default:
					return param;
			}
		}



		#endregion AUTRES METHODES
	





		// ---------------------------------------------------------------------------
		// EVENEMENTS
		// ---------------------------------------------------------------------------
		
		
		
		
		#region EVENEMENTS
		
		
		/// <summary>
		/// Déclenche l'événement Execution, et affiche éventuellement la valeur de retour dans la console. Si la commande a un délégué à exécuter, exécute ce délégué sans déclencher l'événement.
		/// </summary>
		protected void OnExecution(Command cmd, object[] parameters)
		{
			ExecutionEventArgs eventArgs = new ExecutionEventArgs(cmd, cmd.Name, parameters);
			if (cmd.ExeDelegate != null) { cmd.ExeDelegate(parameters, eventArgs); }
			else if (Execution != null) { Execution(this, eventArgs); }
			else { return; }
			if (!String.IsNullOrEmpty(eventArgs.Answer)) { this.WriteLine(eventArgs.Answer, eventArgs.AnswerIsError); }
		}


		// ---------------------------------------------------------------------------
	

		/// <summary>
		/// Bloque la fermeture de la fenêtre en mode Request, sinon l'application peut se fermer mais le programme éternellement tourner.
		/// </summary>
		private void Form_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!e.Cancel && _requestMode) {
				WriteLine("You can't stop when request mode.");
				e.Cancel = _rebuildRequestQuestion = true; }
			else if (!e.Cancel) { _pressAnyKeyMode = false; }
		}


		// ---------------------------------------------------------------------------
		

		/// <summary>
		/// Affiche le court message d'erreur, et enregistre le détail dans l'historique d'erreur.
		/// </summary>
		private void ErrorHandler_ErrorOccurred(object sender, ErrorHandler.ErrorOccurredEventArgs e)
		{
			this.WriteLine(e.ShortMessage, true);
			_errorHistory.AddLine(e.Message);
		}
		
		
		
		#endregion EVENEMENTS




	

		// ---------------------------------------------------------------------------
		// COMMANDES INTERNES
		// ---------------------------------------------------------------------------




		#region COMMANDES INTERNES



		/// <summary>
		/// Initialise les commandes internes, et définit les délégués d'événements pour la gestion des commandes.
		/// </summary>
		protected void InitIntCommands()
		{
		
			_intCmds = new Command[15]; int c = 0;
			
			_intCmds[c++] = new Command("LoadBatch", typeof(string));
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Load a text file and execute all commands.";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					string[] lines = My.FilesAndStreams.ReadAllLines((string)args[0]);
					if (lines != null) {
						for (int i=0; i<lines.Length; i++) {
							if (!String.IsNullOrEmpty(lines[i])) { this.ExecuteCommand(lines[i], true); } }
						this.WriteLine(String.Format("Commands executed from {0}.", (string)args[0])); }
					else { this.WriteLine(String.Format("Error when loading commands from {0}", (string)args[0]), true); }
				};

			_intCmds[c++] = new Command("SaveBatch", typeof(string));
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Save all history command in the specified file, unless LoadBatch and SaveBatch commands.";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					Regex reg = new Regex("^SaveBatch|^LoadBatch", RegexOptions.IgnoreCase);
					if (_validHistory.SaveInFile((string)args[0], reg))
						{ this.WriteLine(String.Format("History saved in {0}.", (string)args[0])); }
					else { this.WriteLine(String.Format("Error when saving history in {0}", (string)args[0]), true); }
				};

			_intCmds[c++] = new Command("?");
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Show all commands.";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					// Parcours le tableau des commandes, et les affiches toutes, avec alias et nom:
					foreach(string s in _cmdsSearchText) { WriteLine(s); }
				};

			_intCmds[c++] = new Command("?", typeof(string[]));
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Search a command with the specified text.";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					int l = ((string[])args[0]).Length;
					string[] text = new string[l];
					for (int i=0; i<l; i++) { text[i] = ((string[])args[0])[i].ToLower(); }
					// Parcours le tableau de commande, cherche dans les noms et les alias, puis affiche les commandes trouvées:
					l = _cmdsSearchNames.Length;
					bool found;
					for (int i=0; i<l; i++)
					{
						foreach (string s in _cmdsSearchNames[i])
						{
							found = true;
							foreach (string t in text) { found = found && s.ToLower().Contains(t); }
							if (found) { WriteLine(_cmdsSearchText[i]); break; }
						}
					}
				};

			_intCmds[c++] = new Command("GetCD");
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Get current directory.";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					this.WriteLine("Current directory: " + Environment.CurrentDirectory);
				};

			_intCmds[c++] = new Command("SetCD", typeof(string));
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Set current directory.";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					if (System.IO.Directory.Exists((string)args[0])) { My.App.SetCurrentDirectory((string)args[0]); }
					this.WriteLine("Current directory: " + Environment.CurrentDirectory);
				};

			_intCmds[c++] = new Command("Cls");
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Clear console, but not history.";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					this.ClearConsole();
				};

			_intCmds[c++] = new Command("ShowPreviousError");
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Show details of previous error.";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					string err = _errorHistory.Back();
					if (err == null) { this.WriteLine("No error to show."); }
					else { this.WriteLine(err); }
				};

			_intCmds[c++] = new Command("Ask", typeof(string));
			_intCmds[c-1].SyntaxDescription = My.ClassManager.GetParameterNames("My.Console,AskParamByParam", 0);
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Ask one per one parameters for command.";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					AskParamByParam((string)args[0]);
				};

			_intCmds[c++] = new Command("MakeMan", typeof(int), typeof(string), typeof(bool));
			_intCmds[c-1].SyntaxDescription = My.ClassManager.GetParameterNames("My.Console,MakeMan", 0);
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Save man in file. What: neg for internal, 0 for all, pos for external commands.";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					MakeMan((int)args[0], (string)args[1], (bool)args[2], null);
				};

			_intCmds[c++] = new Command("MakeMan", typeof(int), typeof(string), typeof(bool), typeof(string));
			_intCmds[c-1].SyntaxDescription = My.ClassManager.GetParameterNames("My.Console,MakeMan", 0);
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].Help = "Save man in file. Last param is RegEx for excluding cmds, e.g.: \"^(Start).+\".";
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					MakeMan((int)args[0], (string)args[1], (bool)args[2], (string)args[3]);
				};

			_intCmds[c++] = new Command("Pause");
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					PressAnyKeyMode();
				};

			_intCmds[c++] = new Command("ChangeConsoleFont", typeof(Font));
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					Font f = (Font)args[0];
					this.Font = f;
					My.ExdControls.MySettings.ConsoleDefaultFont = f;
				};

			_intCmds[c++] = new Command("ChangeConsoleColor", typeof(Color));
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					Color color = (Color)args[0];
					this.ForeColor = color;
					My.ExdControls.MySettings.ConsoleDefaultColor = color;
				};

			_intCmds[c++] = new Command("ChangeConsoleBackColor", typeof(Color));
			_intCmds[c-1].Categories = new string[]{"Internal commands"};
			_intCmds[c-1].ExeDelegate = delegate(object[] args, ExecutionEventArgs e)
				{
					Color color = (Color)args[0];
					this.BackColor = color;
					My.ExdControls.MySettings.ConsoleDefaultBackColor = color;
				};

		}


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Enregistre toutes les commandes au format manuel dans le fichier spécifié. C'est un tableau de TreeNode sérialisé, ce qui permet de disposer les commandes en arborescence selon la propriété Cotegories. what indique quels commandes il faut inclure : négatif pour les commandes internes, 0 pour toutes les commandes, positif pour les commandes externes. excldePattern est une expression régulière pour exclure certaines commandes.
		/// </summary>
		protected void MakeMan(int what, string filename, bool showInConsole, string excludePattern)
		{
			// Forme le Regex:
			Regex reg = null;
			try { if (!String.IsNullOrEmpty(excludePattern)) { reg = new Regex(excludePattern); } }
			catch (Exception exc) { My.ErrorHandler.ShowError(exc); return; }
			// Détermine les commandes à inclure:
			Command[] cmds;
			if (what < 0) { cmds = _intCmds; }
			else if (what == 0) { cmds = _allCmds; }
			else { cmds = _extCmds; }
			// Enregistre les éléments sous forme d'arbre, dans un TreeNodeCollection:
			TreeView tree = new TreeView(); TreeNode node;
			foreach (Command cmd in cmds)
			{
				if (reg != null && reg.IsMatch(cmd.Name)) { continue; }
				node = new TreeNode(cmd.Name);
				string tag = String.Empty;
				foreach (Command c in cmds) {
					if (c.Name == cmd.Name && ArrayFunctions.ArrayEquals(c.Categories, cmd.Categories))
						{ tag += GetSyntax(c, true) + "\n"; } }
				if (tag.Length > 1) { tag = tag.Substring(0, tag.Length - 1); }
				node.Tag = tag;
				ControlsFunctions.SetNodeInTreeView(cmd.Categories, node, tree, true);
			}
			// Sérialise l'arbre dans un fichier:
			tree.Sort();
			TreeNode[] nodes = new TreeNode[tree.Nodes.Count];
			for (int i=0; i<nodes.Length; i++) { nodes[i] = tree.Nodes[i]; }
			// Affiche dans la console:
			if (showInConsole) {
				Action<TreeNode> show =
					delegate(TreeNode n)
					{ WriteLine(String.Format("----- {0} -----\n{1}", n.Text, (n.Tag is string ? (string)n.Tag : ""))); };
				ControlsFunctions.ActionOnAllNodes(tree, show); }
			// Enregistre dans filename:
			if (My.FilesAndStreams.SerializeInFile(filename, nodes))
				{ WriteLine(String.Format("Data saved in {0}.", filename)); }
			else
				{ WriteLine(String.Format("Error when saving in {0}.", filename)); }
		}	


		// ---------------------------------------------------------------------------
	
	
		/// <summary>
		/// Demande les paramètres un à un à l'utilisateur pour une commande donnée (l'user doit d'abord choisir la surcharge). La méthode n'analyse rien, n'accepte qu'un texte par paramètre, puis écrit la ligne de commande à exécuter (si l'utilisateur veut toujours l'exécuter). Ce n'est donc qu'une aide à la saisie des paramètres, et non une analyse de quoi que ce soit. Pour annuler, taper $.
		/// </summary>
		protected void AskParamByParam(string cmdName)
		{
		
			// Parcours le tableau de commande, cherche dans les noms et les alias, puis affiche les commandes trouvées:
			Command[] cmds = new Command[5]; int c = 0;
			foreach (Command cmd in _allCmds)
			{
				if (cmd.Name.Equals(cmdName, StringComparison.CurrentCultureIgnoreCase)
					|| cmd.Aliases.Contains(cmdName, My.ArrayFunctions.StringIgnoreCaseComparer))
				{
					if (c >= cmds.Length) { Array.Resize(ref cmds, c + 5); }
					cmds[c] = cmd;
					WriteLine(c.ToString() + ": " + GetSyntax(cmd));
					c++;
				}
			}
			Array.Resize(ref cmds, c);
			if (c == 0) { WriteLine("Unkonw command.", true); return; }
			
			// Récupère la commande choisi par l'utilisateur et sort si la commande est un choix:
			Command command;
			if (c == 1) { command = cmds[0]; }
			else {
				int[] choices = new int[cmds.Length];
				for (int i=0; i<cmds.Length; i++) { choices[i] = i; }
				command = cmds[Request<int>("Wich overload", 0, choices)]; }
			if (command.Choices != null) { WriteLine("Mode not available for this command."); return; }
			
			// Affiche les paramètres l'un après l'autre, en récupérant chaque fois un string:
			int l = command.ParamsTypes.Length; string[] parameters = new string[l]; string msg;
			for (int i=0; i<l; i++)
			{
				// Message:
				msg = String.Format("{0}({1})", (command.SyntaxDescription.Length > i ?
					command.SyntaxDescription[i] : "arg"+i.ToString()), command.ParamsTypes[i].Name);
				// Si tableau, demande tous les index:
				if (command.ParamsTypes[i].IsArray) {
					WriteLine(msg); string arr = String.Empty, tmp = String.Empty; int indexCounter = 0;
					while (tmp != "$") { arr += tmp + Separator; tmp = Request<string>(String.Format("[{0}]", indexCounter++)); }
					parameters[i] = arr.Trim(','); }
				else { parameters[i] = Request<string>(msg); }
				if (parameters[i] == "$") { return; }
				// Si tableau et si pas dernier, on échappe:
				if (command.ParamsTypes[i].IsArray && i < l-1)
					{ parameters[i] = My.FieldsParser.EscapeField(parameters[i], Separator); }
			}
			
			// Ecrit les paramètres sur la ligne de commande:
			WriteEndLine();
			Write(false, String.Format("{0} {1}", command.Name,
				My.ArrayFunctions.Join(parameters, Separator).Trim(',')));
			
		}
		
		

		#endregion COMMANDES INTERNES
	


	}
	
	
	
}
