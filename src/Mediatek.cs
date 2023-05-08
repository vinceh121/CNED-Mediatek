using System;
using System.Threading.Tasks;
using Gtk;
using MySqlConnector;

using Mediatek.Controllers;
using Mediatek.Dialogs;

namespace Mediatek
{
	/// <summary>
	/// GLib signals:
	/// <list type="bullet">
	/// <item>
	/// <term>quit</term>
	/// <description>Exits the app</description>
	/// </item>
	/// <item>
	/// <term>about</term>
	/// <description>Shows the À propos dialog</description>
	/// </item>
	/// <item>
	/// <term>loginDialog</term>
	/// <description>Shows the MySql login dialog</description>
	/// </item>
	/// <item>
	/// <term>staffCreate</term>
	/// <description>Shows the Create new staff dialog</description>
	/// </item>
	/// </list>
	/// </summary>
	public class Mediatek
	{
		public event EventHandler LoggedIn;
		private Application _app;
		private MainWindow _win;
		private MySqlConnection _dbConn;

		// controllers
		private StaffController _staffController;
		private LeaveController _leaveController;
		private ManagerController _managerController;
		private ReasonController _reasonController;
		private ServiceController _serviceController;

		[STAThread]
		public static void Main(string[] args)
		{
			GLib.ExceptionManager.UnhandledException += HandleUncaughtException;

			Mediatek prog = new Mediatek();
			prog.Start();
		}

		public Mediatek()
		{
			Application.Init();

			this._app = new Application("me.vinceh121.mediatek", GLib.ApplicationFlags.None);
			_app.Register(GLib.Cancellable.Current);

			this._win = new MainWindow(this);
			_app.AddWindow(_win);

			this.CreateActions();

			using Builder bldMenu = new Builder("Menu.glade");
			_app.Menubar = new GLib.MenuModel(bldMenu.GetRawOwnedObject("menubar"));
		}

		public void Start()
		{
			this._win.ShowAll();
			Application.Run();
		}

		private void CreateActions()
		{
			GLib.SimpleAction quitAction = new GLib.SimpleAction("quit", null);
			quitAction.Activated += QuitActivated;
			this._app.SetAccelsForAction("app.quit", new string[] { "<Ctrl>Q" });
			_app.AddAction(quitAction);

			GLib.SimpleAction aboutAction = new GLib.SimpleAction("about", null);
			aboutAction.Activated += AboutActivated;
			_app.AddAction(aboutAction);

			GLib.SimpleAction loginAction = new GLib.SimpleAction("loginDialog", null);
			loginAction.Activated += LoginDialogActivated;
			this._app.SetAccelsForAction("app.loginDialog", new string[] { "<Ctrl>O" });
			_app.AddAction(loginAction);

			// Staff actions //

			GLib.SimpleAction actionAddStaff = new GLib.SimpleAction("staffCreate", null);
			actionAddStaff.Activated += StaffCreateActivated;
			actionAddStaff.Enabled = false;
			this._app.SetAccelsForAction("app.staffCreate", new string[] { "<Ctrl>N" });
			_app.AddAction(actionAddStaff);
		}

		/// <summary>
		/// Asynchronously performs a login operation
		/// </summary>
		/// <param name="host">MySql host and port</param>
		/// <param name="username">MySql username</param>
		/// <param name="password">MySql password</param>
		/// <param name="database">MySql database</param>
		/// <param name="sslMode">Connection SSL policy</param>
		/// <param name="managerUsername">Manager username</param>
		/// <param name="managerPassword">Manager password</param>
		public async Task Login(string host, string username, string password, string database, MySqlSslMode sslMode,
			string managerUsername, string managerPassword)
		{
			if (this._dbConn != null)
			{
				await this._dbConn.DisposeAsync();
				this._dbConn = null;
			}

			var connString = new MySqlConnectionStringBuilder()
			{
				Server = host,
				UserID = username,
				Password = password,
				Database = database,
				SslMode = sslMode
			};

			this._dbConn = new MySqlConnection(connString.ToString());
			await this._dbConn.OpenAsync();

			this._managerController = new ManagerController(this);

			bool loginCheck = await this._managerController.VerifyAuth(managerUsername, managerPassword);
			if (!loginCheck)
			{
				throw new UnauthorizedAccessException("Mot-de-passe responsable ou login de responsable invalide");
			}

			// create controllers only once we loggedin
			this._staffController = new StaffController(this);
			this._leaveController = new LeaveController(this);
			this._reasonController = new ReasonController(this);
			this._serviceController = new ServiceController(this);

			// trigger LoggedIn event
			this.LoggedIn?.Invoke(this, null);

			((GLib.SimpleAction)this._app.LookupAction("loginDialog")).Enabled = false;

			// list of app. actions to be enabled only once we are loggedin
			// those actions need to be disabled by default
			string[] appActions = new string[] { "staffCreate" };
			foreach (string act in appActions)
			{
				((GLib.SimpleAction)this._app.LookupAction(act)).Enabled = true;
			}

			// ditto but for win. actions
			string[] winActions = new string[] { "staffDelete", "staffEdit",
				"leaveCreate", "leaveEdit", "leaveDelete",
				"refresh" };
			foreach (string act in winActions)
			{
				((GLib.SimpleAction)this._win.LookupAction(act)).Enabled = true;
			}
		}

		private void StaffCreateActivated(object sender, EventArgs e)
		{
			CreateStaffDialog dialog = new CreateStaffDialog(this);
			dialog.TransientFor = _win;
			dialog.ShowAll();
		}

		private void LoginDialogActivated(object sender, EventArgs e)
		{
			LoginDialog diag = new LoginDialog(this);
			diag.TransientFor = _win;
			diag.ShowAll();
		}

		private void QuitActivated(object sender, EventArgs e)
		{
			Application.Quit();
		}

		private void AboutActivated(object sender, EventArgs e)
		{
			Mediatek.ShowAbout(this._win);
		}

		public Application GetApplication()
		{
			return this._app;
		}

		public MainWindow GetMainWindow()
		{
			return this._win;
		}

		public MySqlConnection GetConnection()
		{
			return this._dbConn;
		}

		public StaffController GetStaffController()
		{
			return this._staffController;
		}

		public LeaveController GetLeaveController()
		{
			return this._leaveController;
		}

		public ReasonController GetReasonController()
		{
			return this._reasonController;
		}

		public ServiceController GetServiceController()
		{
			return this._serviceController;
		}

		public ManagerController GetManagerController()
		{
			return this._managerController;
		}

		public static void ShowAbout(Window transientFor)
		{
			AboutDialog dialog = new AboutDialog
			{
				TransientFor = transientFor,
				ProgramName = "MediaTek application responsable",
				Version = "0.0.0",
				Comments = "Un exercice d'application métier",
				LogoIconName = "system-run-symbolic",
				License = "MediaTek, un exercice d'application métier\n"
					+ "Copyright (C) 2023 Vincent Hyvert\n"
					+ "\n"
					+ "This program is free software: you can redistribute it and/or modify\n"
					+ "it under the terms of the GNU Affero General Public License as\n"
					+ "published by the Free Software Foundation, either version 3 of the\n"
					+ "License, or (at your option) any later version.\n"
					+ "\n"
					+ "This program is distributed in the hope that it will be useful,\n"
					+ "but WITHOUT ANY WARRANTY; without even the implied warranty of\n"
					+ "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\n"
					+ "GNU Affero General Public License for more details.\n"
					+ "\n"
					+ "You should have received a copy of the GNU Affero General Public License\n"
					+ "along with this program.  If not, see <https://www.gnu.org/licenses/>.",
				Website = "https://github.com/vinceh121/CNED-Mediatek",
				WebsiteLabel = "Repository"
			};
			dialog.Run();
			dialog.Hide();
		}

		private static void HandleUncaughtException(GLib.UnhandledExceptionArgs args)
		{
			Console.WriteLine(args.ExceptionObject);
			MessageDialog diag = new MessageDialog(null, 0,
				MessageType.Error, ButtonsType.Ok, false,
				"Erreur inattendue : {0}", new object[] { args.ExceptionObject });
			diag.Run();
			diag.Destroy();
		}
	}
}
