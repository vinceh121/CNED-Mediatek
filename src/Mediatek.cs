using System;
using System.Threading.Tasks;
using Gtk;
using MySqlConnector;

namespace project
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
	/// </list>
	/// </summary>
	public class Mediatek
	{
		public event EventHandler LoggedIn;
		private Application _app;
		private MainWindow _win;
		private MySqlConnection _dbConn;

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

			var menu = CreateMenu();
			_app.Menubar = menu;
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
			_app.AddAction(quitAction);

			GLib.SimpleAction aboutAction = new GLib.SimpleAction("about", null);
			aboutAction.Activated += AboutActivated;
			_app.AddAction(aboutAction);

			GLib.SimpleAction loginAction = new GLib.SimpleAction("loginDialog", null);
			loginAction.Activated += LoginDialogActivated;
			_app.AddAction(loginAction);
		}

		private GLib.Menu CreateMenu()
		{
			GLib.Menu menu = new GLib.Menu();

			GLib.Menu menuFile = new GLib.Menu();
			menuFile.AppendItem(new GLib.MenuItem("_Connexion", "app.loginDialog"));
			menuFile.AppendItem(new GLib.MenuItem("_Quitter", "app.quit"));
			menu.AppendSubmenu("_Fichier", menuFile);

			GLib.Menu menuHelp = new GLib.Menu();
			menuHelp.AppendItem(new GLib.MenuItem("_À propos", "app.about"));
			menu.AppendSubmenu("_Aide", menuHelp);

			return menu;
		}

		/// <summary>
		/// Asynchronously performs a login operation
		/// </summary>
		/// <param name="host">MySql host and port</param>
		/// <param name="username">MySql username</param>
		/// <param name="password">MySql password</param>
		/// <param name="database">MySql database</param>
		public async Task Login(string host, string username, string password, string database)
		{
			var connString = new MySqlConnectionStringBuilder()
			{
				Server = host,
				UserID = username,
				Password = password,
				Database = database
			};

			this._dbConn = new MySqlConnection(connString.ToString());
			await this._dbConn.OpenAsync();
			this.LoggedIn?.Invoke(this, null);
			((GLib.SimpleAction)this._app.LookupAction("loginDialog")).Enabled = false;
		}

		private void LoginDialogActivated(object sender, EventArgs e)
		{
			LoginDialog diag = new LoginDialog(this);
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

		public MySqlConnection GetConnection()
		{
			return this._dbConn;
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
