using System;
using System.Threading.Tasks;
using Gtk;
using MySqlConnector;

namespace project
{
	public class Mediatek
	{
		private Application app;
		private MainWindow win;
		private MySqlConnection DbConn;

		[STAThread]
		public static void Main(string[] args)
		{
			Mediatek prog = new Mediatek();
			prog.Start();
		}

		public Mediatek()
		{
			Application.Init();

			this.app = new Application("me.vinceh121.mediatek", GLib.ApplicationFlags.None);
			app.Register(GLib.Cancellable.Current);

			this.win = new MainWindow();
			app.AddWindow(win);

			this.CreateActions();

			var menu = CreateMenu();
			app.Menubar = menu;
		}

		public void Start()
		{
			this.win.ShowAll();
			Application.Run();
		}

		private void CreateActions()
		{
			GLib.SimpleAction quitAction = new GLib.SimpleAction("quit", null);
			quitAction.Activated += QuitActivated;
			app.AddAction(quitAction);

			GLib.SimpleAction aboutAction = new GLib.SimpleAction("about", null);
			aboutAction.Activated += AboutActivated;
			app.AddAction(aboutAction);

			GLib.SimpleAction loginAction = new GLib.SimpleAction("loginDialog", null);
			loginAction.Activated += LoginDialogActivated;
			app.AddAction(loginAction);
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

		public async Task Login(string host, string username, string password, string database)
		{
			var connString = new MySqlConnectionStringBuilder()
			{
				Server = host,
				UserID = username,
				Password = password,
				Database = database
			};

			this.DbConn = new MySqlConnection(connString.ToString());
			await this.DbConn.OpenAsync();
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
			Mediatek.ShowAbout(this.win);
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
	}
}
